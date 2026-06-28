using System.Text;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.HUD
{
    /// <summary>
    /// HUD de texto sempre-on, construido 100% em codigo (sem editar .unity).
    /// Fecha o gap "headless" do GameplayHudCanvasController: torna VISIVEL o relogio/dia/fase,
    /// os vitais (HP/Stamina/Fome), o ouro e o prompt de interacao.
    ///
    /// Le dados pelo idiom de bootstrap (GameBootstrap.Instance.*) — nunca FindObjectOfType de
    /// gameplay novo — e reage a eventos do GameEventBus para marcar dirty. O relogio avanca
    /// continuamente, entao um poll leve (cada PollIntervalSeconds) reconstroi a string do tempo.
    ///
    /// NAO substitui o DebugHud (IMGUI), so adiciona o HUD legivel por cima. Ancorado no TOPO-CENTRO,
    /// fora das colunas laterais do DebugHud (esquerda/direita).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayHudTextOverlay : MonoBehaviour
    {
        private const float PollIntervalSeconds = 0.25f;
        private const int MinutesPerPhase = 12 * 60; // cada fase (Dia/Noite) cobre 12h.

        private Text _text;
        private RectTransform _panelRect;
        private readonly StringBuilder _builder = new StringBuilder(256);

        // Painel auto-ajusta a largura/altura ao texto (corrige o overflow para fora da caixa).
        private const float PanelPaddingX = 56f;
        private const float PanelPaddingY = 18f;
        private const float PanelMinWidth = 420f;
        private const float PanelMaxWidth = 1860f;

        // Estado vital/ouro/prompt, alimentado por eventos (cache; sem ler manager por frame).
        private int _hp;
        private int _maxHp;
        private int _stamina;
        private int _maxStamina;
        private int _mana;
        private int _maxMana;
        private int _hunger;
        private int _maxHunger;
        private int _gold;
        private bool _hasPrompt;
        private string _prompt = string.Empty;

        private bool _vitalsSeeded;
        private float _pollTimer;
        private string _lastRendered = string.Empty;
        private bool _loggedReady;

        // ---- HUD de vitais (barras coloridas), separado do texto do topo. Esconde durante modais
        //      (dialogo/loja/inventario) e reaparece ao fechar. Layout: duas colunas centralizadas —
        //      ESQUERDA Stamina/Fome, DIREITA HP/MP, cada par empilhado.
        private CanvasGroup _barsGroup;
        private Image _hpFill, _staminaFill, _manaFill, _hungerFill;
        private Text _hpValue, _staminaValue, _manaValue, _hungerValue;

        private static readonly Color HpColor = new Color(0.85f, 0.22f, 0.22f);      // vermelho
        private static readonly Color StaminaColor = new Color(0.93f, 0.82f, 0.25f); // amarelo
        private static readonly Color ManaColor = new Color(0.30f, 0.55f, 0.95f);    // azul
        private static readonly Color HungerColor = new Color(0.95f, 0.58f, 0.20f);  // laranja
        private const float BarWidth = 320f;

        public void Build()
        {
            BuildCanvasHierarchy();
            BuildVitalsBars();
            SeedFromManagers();
            Render(force: true);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Subscribe<ManaChangedEvent>(OnManaChanged);
            GameEventBus.Subscribe<HungerChangedEvent>(OnHungerChanged);
            GameEventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            GameEventBus.Subscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Unsubscribe<ManaChangedEvent>(OnManaChanged);
            GameEventBus.Unsubscribe<HungerChangedEvent>(OnHungerChanged);
            GameEventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            GameEventBus.Unsubscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
        }

        private void Update()
        {
            // Vitais podem nao ter publicado evento ainda (HUD criado antes do GameBootstrap).
            // Faz uma tentativa preguicosa de seed ate conseguir.
            if (!_vitalsSeeded)
            {
                SeedFromManagers();
            }

            // Esconde as barras de vitais enquanto um modal (dialogo/loja/inventario) esta ativo; checagem
            // barata por frame para resposta imediata ao abrir/fechar a conversa.
            UpdateBarsVisibility();

            _pollTimer += Time.unscaledDeltaTime;
            if (_pollTimer < PollIntervalSeconds)
            {
                return;
            }

            _pollTimer = 0f;
            Render(force: false);
        }

        // ---- Construcao da hierarquia de Canvas (sem editar .unity) ----

        private void BuildCanvasHierarchy()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Acima do DebugHud (IMGUI desenha em GUI depth normal); sortingOrder alto garante topo.
            canvas.sortingOrder = 5000;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Painel escuro semi-transparente para contraste, ancorado no topo-centro.
            var panelGo = new GameObject("HudTextPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(transform, false);
            var panelRect = (RectTransform)panelGo.transform;
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -8f);
            panelRect.sizeDelta = new Vector2(760f, 72f);
            _panelRect = panelRect;

            var panelImage = panelGo.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.55f);
            panelImage.raycastTarget = false;

            // Texto legivel (claro) com sombra para contraste sobre qualquer cenario.
            var textGo = new GameObject("HudText", typeof(RectTransform), typeof(Text), typeof(Shadow));
            textGo.transform.SetParent(panelGo.transform, false);
            var textRect = (RectTransform)textGo.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 4f);
            textRect.offsetMax = new Vector2(-12f, -4f);

            _text = textGo.GetComponent<Text>();
            _text.font = ResolveBuiltinFont();
            _text.fontSize = 28;
            _text.alignment = TextAnchor.MiddleCenter;
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            _text.verticalOverflow = VerticalWrapMode.Overflow;
            _text.color = new Color(0.96f, 0.96f, 0.86f, 1f);
            _text.raycastTarget = false;
            _text.text = "[GameplayHud] aguardando dados...";

            var shadow = textGo.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private static Font ResolveBuiltinFont()
        {
            // LegacyRuntime.ttf substitui Arial.ttf nas versoes recentes da Unity; tenta ambos.
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        // ---- Leitura de dados (eventos + bootstrap) ----

        private void SeedFromManagers()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return;
            }

            var player = bootstrap.PlayerManager;
            if (player != null)
            {
                _hp = player.CurrentHP;
                _maxHp = player.MaxHP;
                _gold = player.CurrentGold;
                _vitalsSeeded = true;
            }

            var stamina = bootstrap.StaminaManager;
            if (stamina != null)
            {
                _stamina = stamina.CurrentStamina;
                _maxStamina = stamina.MaxStamina;
            }

            var mana = bootstrap.ManaManager;
            if (mana != null)
            {
                _mana = mana.CurrentMana;
                _maxMana = mana.MaxMana;
            }

            var hunger = bootstrap.HungerManager;
            if (hunger != null)
            {
                _hunger = hunger.CurrentHunger;
                _maxHunger = hunger.MaxHunger;
            }
        }

        private void OnHpChanged(HPChangedEvent evt)
        {
            _hp = evt.CurrentHP;
            _maxHp = evt.MaxHP;
            _vitalsSeeded = true;
            Render(force: true);
        }

        private void OnStaminaChanged(StaminaChangedEvent evt)
        {
            _stamina = evt.CurrentStamina;
            _maxStamina = evt.MaxStamina;
            Render(force: true);
        }

        private void OnManaChanged(ManaChangedEvent evt)
        {
            _mana = evt.CurrentMana;
            _maxMana = evt.MaxMana;
            Render(force: true);
        }

        private void OnHungerChanged(HungerChangedEvent evt)
        {
            _hunger = evt.CurrentValue;
            _maxHunger = evt.MaxValue;
            Render(force: true);
        }

        private void OnGoldChanged(GoldChangedEvent evt)
        {
            _gold = evt.NewTotal;
            Render(force: true);
        }

        private void OnInteractionPromptChanged(InteractionPromptChangedEvent evt)
        {
            _hasPrompt = evt.HasCandidate;
            _prompt = evt.Prompt ?? string.Empty;
            Render(force: true);
        }

        // ---- Render ----

        private void Render(bool force)
        {
            // HP/Stamina/MP/Fome agora vivem nas barras coloridas (sempre refletem o cache atual).
            UpdateVitalsBars();

            if (_text == null)
            {
                return;
            }

            // Texto do topo: so relogio/dia/fase, ouro e prompt de interacao (vitais sairam para as barras).
            _builder.Length = 0;
            AppendClock();
            _builder.Append("    Ouro ").Append(_gold);

            if (_hasPrompt && !string.IsNullOrWhiteSpace(_prompt))
            {
                _builder.Append("\n[E] ").Append(_prompt);
            }

            var rendered = _builder.ToString();
            if (!force && rendered == _lastRendered)
            {
                return;
            }

            _lastRendered = rendered;
            _text.text = rendered;
            ResizePanelToText();

            if (!_loggedReady)
            {
                _loggedReady = true;
                Debug.Log("[GameplayHud] HUD visivel montado (relogio/vitais/ouro).");
            }
        }

        // Hugs the dark panel to the text so nothing spills outside the box, and grows the height when the
        // interaction prompt adds a second line. preferredWidth/Height are the natural unwrapped text metrics.
        private void ResizePanelToText()
        {
            if (_panelRect == null || _text == null)
            {
                return;
            }

            var width = Mathf.Clamp(_text.preferredWidth + PanelPaddingX, PanelMinWidth, PanelMaxWidth);
            var height = _text.preferredHeight + PanelPaddingY;
            _panelRect.sizeDelta = new Vector2(width, height);
        }

        // ---- Barras de vitais (construcao + atualizacao) ----

        private void BuildVitalsBars()
        {
            var barsGo = new GameObject("VitalsBars", typeof(RectTransform), typeof(CanvasGroup));
            barsGo.transform.SetParent(transform, false);
            var barsRect = (RectTransform)barsGo.transform;
            // Faixa de largura total no RODAPE da tela; as colunas ancoram no CENTRO com um afastamento fixo
            // (ColX), entao ficam centralizadas e proximas (nao nos cantos), iguais em qualquer largura.
            barsRect.anchorMin = new Vector2(0f, 0f);
            barsRect.anchorMax = new Vector2(1f, 0f);
            barsRect.pivot = new Vector2(0.5f, 0f);
            barsRect.anchoredPosition = new Vector2(0f, 14f); // bem abaixo, ~14px do fundo
            barsRect.sizeDelta = new Vector2(0f, 90f);
            _barsGroup = barsGo.GetComponent<CanvasGroup>();
            _barsGroup.interactable = false;
            _barsGroup.blocksRaycasts = false;

            const float colX = 300f;   // afastamento de cada coluna a partir do centro (menor = mais juntas)
            const float rowGap = 46f;  // espacamento vertical entre as duas barras empilhadas

            // Coluna ESQUERDA: HP (cima) / MP (baixo).
            _hpFill = CreateVitalBar(barsRect, "HP", -colX, 0f, HpColor, out _hpValue);
            _manaFill = CreateVitalBar(barsRect, "MP", -colX, -rowGap, ManaColor, out _manaValue);
            // Coluna DIREITA: Stamina (cima) / Fome (baixo).
            _staminaFill = CreateVitalBar(barsRect, "Stamina", colX, 0f, StaminaColor, out _staminaValue);
            _hungerFill = CreateVitalBar(barsRect, "Fome", colX, -rowGap, HungerColor, out _hungerValue);
        }

        private Image CreateVitalBar(Transform parent, string label, float xOffset, float yOffset, Color color, out Text valueText)
        {
            var barGo = new GameObject(label + "Bar", typeof(RectTransform));
            barGo.transform.SetParent(parent, false);
            var barRect = (RectTransform)barGo.transform;
            // Centralizado no topo da faixa, deslocado por (xOffset, yOffset).
            barRect.anchorMin = new Vector2(0.5f, 1f);
            barRect.anchorMax = new Vector2(0.5f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.anchoredPosition = new Vector2(xOffset, yOffset);
            barRect.sizeDelta = new Vector2(BarWidth, 32f);

            // Fundo (preenche toda a barra).
            CreateStretchImage(barRect, "Bg", new Color(0f, 0f, 0f, 0.55f));

            // Preenchimento ancorado a ESQUERDA, com a largura ajustada em SetBar (largura = BarWidth * t).
            // Nao usamos Image.Type.Filled de proposito: fillAmount so funciona com um sprite atribuido, e
            // estas imagens nao tem sprite — por isso encolhemos a largura do retangulo, que sempre funciona.
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(barRect, false);
            var fillRect = (RectTransform)fillGo.transform;
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(BarWidth, 0f);
            var fill = fillGo.GetComponent<Image>();
            fill.color = color;
            fill.raycastTarget = false;

            // Rotulo (esquerda) e valor (direita) sobre a barra.
            CreateBarText(barRect, "Label", label, TextAnchor.MiddleLeft, 16);
            valueText = CreateBarText(barRect, "Value", "0/0", TextAnchor.MiddleRight, 15);
            return fill;
        }

        private static Image CreateStretchImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private Text CreateBarText(Transform parent, string name, string content, TextAnchor anchor, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10f, 0f);
            rect.offsetMax = new Vector2(-10f, 0f);

            var txt = go.GetComponent<Text>();
            txt.font = ResolveBuiltinFont();
            txt.fontSize = fontSize;
            txt.alignment = anchor;
            txt.color = new Color(0.97f, 0.97f, 0.9f, 1f);
            txt.raycastTarget = false;
            txt.text = content;

            var sh = go.GetComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.85f);
            sh.effectDistance = new Vector2(1f, -1f);
            return txt;
        }

        private void UpdateVitalsBars()
        {
            SetBar(_hpFill, _hpValue, _hp, _maxHp);
            SetBar(_staminaFill, _staminaValue, _stamina, _maxStamina);
            SetBar(_manaFill, _manaValue, _mana, _maxMana);
            SetBar(_hungerFill, _hungerValue, _hunger, _maxHunger);
        }

        private static void SetBar(Image fill, Text value, int current, int max)
        {
            if (fill == null)
            {
                return;
            }

            var t = max > 0 ? Mathf.Clamp01(current / (float)max) : 0f;
            fill.rectTransform.sizeDelta = new Vector2(BarWidth * t, 0f);
            if (value != null)
            {
                value.text = current + "/" + max;
            }
        }

        private void UpdateBarsVisibility()
        {
            if (_barsGroup == null)
            {
                return;
            }

            var modalActive = GameBootstrap.Instance != null
                && GameBootstrap.Instance.ModalManager != null
                && GameBootstrap.Instance.ModalManager.HasActiveModal;
            _barsGroup.alpha = modalActive ? 0f : 1f;
        }

        private void AppendClock()
        {
            var bootstrap = GameBootstrap.Instance;
            var gameTime = bootstrap != null ? bootstrap.GameTimeManager : null;
            var timeManager = bootstrap != null ? bootstrap.TimeManager : null;

            var day = timeManager != null ? timeManager.CurrentDay : 1;

            if (gameTime == null)
            {
                _builder.Append("Dia ").Append(day).Append(" — --:-- (relogio nao conectado)");
                return;
            }

            var isDay = gameTime.CurrentPhase == GamePhaseChangedEvent.GamePhase.Day;
            ComputeHourMinute(gameTime, isDay, out var hour, out var minute);

            _builder.Append("Dia ").Append(day).Append(" — ");
            AppendTwoDigits(hour);
            _builder.Append(':');
            AppendTwoDigits(minute);
            _builder.Append(" (").Append(isDay ? "Dia" : "Noite").Append(')');
        }

        // GameTimeManager so expoe hora inteira (CurrentHourOfDay). Derivamos os minutos do
        // progresso normalizado dentro do span de 12h da fase para um relogio HH:MM continuo.
        private static void ComputeHourMinute(GameTimeManager gameTime, bool isDay, out int hour, out int minute)
        {
            var t = Mathf.Clamp01(gameTime.CurrentPhaseNormalized);
            var phaseStartHour = isDay ? 6 : 18;
            var totalMinutes = phaseStartHour * 60 + Mathf.FloorToInt(t * MinutesPerPhase);
            totalMinutes %= 24 * 60;
            hour = totalMinutes / 60;
            minute = totalMinutes % 60;
        }

        private void AppendTwoDigits(int value)
        {
            if (value < 10)
            {
                _builder.Append('0');
            }

            _builder.Append(value);
        }
    }
}
