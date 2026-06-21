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
        private readonly StringBuilder _builder = new StringBuilder(256);

        // Estado vital/ouro/prompt, alimentado por eventos (cache; sem ler manager por frame).
        private int _hp;
        private int _maxHp;
        private int _stamina;
        private int _maxStamina;
        private int _hunger;
        private int _maxHunger;
        private int _gold;
        private bool _hasPrompt;
        private string _prompt = string.Empty;

        private bool _vitalsSeeded;
        private float _pollTimer;
        private string _lastRendered = string.Empty;
        private bool _loggedReady;

        public void Build()
        {
            BuildCanvasHierarchy();
            SeedFromManagers();
            Render(force: true);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Subscribe<HungerChangedEvent>(OnHungerChanged);
            GameEventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            GameEventBus.Subscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
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
            panelRect.sizeDelta = new Vector2(760f, 64f);

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
            _text.fontSize = 22;
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
            if (_text == null)
            {
                return;
            }

            _builder.Length = 0;
            AppendClock();
            _builder.Append("    HP ").Append(_hp).Append('/').Append(_maxHp);
            _builder.Append("    Stamina ").Append(_stamina).Append('/').Append(_maxStamina);
            _builder.Append("    Fome ").Append(_hunger).Append('/').Append(_maxHunger);
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

            if (!_loggedReady)
            {
                _loggedReady = true;
                Debug.Log("[GameplayHud] HUD visivel montado (relogio/vitais/ouro).");
            }
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
