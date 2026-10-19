using CindarsHope.Core;
using CindarsHope.Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.Enemy.Speech
{
    /// <summary>
    /// Desenha as falas de criatura como balões estilo HQ (cantos arredondados, fundo translúcido,
    /// rabicho apontando para baixo) num canvas world-space. Singleton lazy (mesmo padrão do
    /// <c>FloatingDamageNumberDisplayer</c>): assina <see cref="CreatureSpokeEvent"/> e instancia um
    /// balão que faz fade-in, segura e some, subindo de leve. Puramente cosmético.
    /// </summary>
    public sealed class CreatureSpeechBubbleDisplayer : MonoBehaviour
    {
        private const float CanvasWorldScale = 0.02f;

        // Aparência do balão (unidades do canvas; multiplicadas por CanvasWorldScale no mundo).
        private const int FontSize = 20;
        private const float MaxWidth = 170f;
        private const float MinWidth = 56f;
        private const float PadX = 22f;
        private const float PadY = 14f;
        private const float TailSize = 14f;

        // Tempo: fade-in, hold (escala com o tamanho da fala), fade-out.
        private const float FadeInSeconds = 0.14f;
        private const float FadeOutSeconds = 0.32f;
        private const float HoldBaseSeconds = 1.6f;
        private const float HoldPerCharSeconds = 0.045f;
        private const float HoldMaxSeconds = 3.6f;
        private const float RiseDistance = 0.18f;

        private static readonly Color BubbleColor = new Color(0.97f, 0.96f, 0.91f, 0.72f); // pergaminho translúcido
        private static readonly Color TextColor = new Color(0.12f, 0.10f, 0.14f, 1f);
        private static readonly Color OutlineColor = new Color(0.08f, 0.07f, 0.10f, 0.85f);

        private static CreatureSpeechBubbleDisplayer s_instance;
        private static Sprite s_bubbleSprite;

        private Canvas _worldCanvas;

        // Sprite branco 1x1 criado em runtime (NÃO usar Resources.GetBuiltinResource("UI/Skin/UISprite.psd"):
        // aquela API não acha o UISprite e falha em runtime/build). Tintado para o fundo translúcido do balão.
        private static Sprite BubbleSprite()
        {
            if (s_bubbleSprite == null)
            {
                var tex = Texture2D.whiteTexture;
                s_bubbleSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                s_bubbleSprite.name = "CreatureBubbleWhite";
            }

            return s_bubbleSprite;
        }

        public static CreatureSpeechBubbleDisplayer EnsureExists(Transform parent = null)
        {
            if (s_instance != null)
            {
                return s_instance;
            }

            var go = new GameObject("CreatureSpeechBubbleDisplayer_Runtime");
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return go.AddComponent<CreatureSpeechBubbleDisplayer>();
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
        }

        private void OnEnable()
        {
            if (_worldCanvas == null)
            {
                var canvasGo = new GameObject("CreatureSpeechCanvas_Runtime");
                canvasGo.transform.SetParent(transform, false);
                _worldCanvas = canvasGo.AddComponent<Canvas>();
                _worldCanvas.renderMode = RenderMode.WorldSpace;
                _worldCanvas.sortingOrder = 101; // acima dos números de dano (100)
                canvasGo.AddComponent<CanvasScaler>();
                _worldCanvas.transform.localScale = new Vector3(CanvasWorldScale, CanvasWorldScale, CanvasWorldScale);
            }

            GameEventBus.Subscribe<CreatureSpokeEvent>(HandleCreatureSpoke);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CreatureSpokeEvent>(HandleCreatureSpoke);
        }

        private void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }

        private void HandleCreatureSpoke(CreatureSpokeEvent evt)
        {
            if (_worldCanvas == null || string.IsNullOrEmpty(evt.Line))
            {
                return;
            }

            BuildBubble(evt.Line, evt.WorldPosition);
        }

        private void BuildBubble(string line, Vector3 worldPosition)
        {
            // Raiz com CanvasGroup (fade unificado de fundo + texto + rabicho).
            var root = new GameObject("CreatureSpeechBubble", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(_worldCanvas.transform, false);
            root.transform.position = worldPosition;
            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;

            // Mede a fala para dimensionar o balão de forma justa.
            Vector2 textSize = MeasureText(line);
            float w = Mathf.Clamp(textSize.x + PadX, MinWidth, MaxWidth);
            float h = textSize.y + PadY;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);

            // Fundo arredondado translúcido.
            var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(root.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bg.GetComponent<Image>();
            bgImage.sprite = BubbleSprite();
            bgImage.color = BubbleColor;

            // Rabicho (quadradinho rotacionado 45 graus) apontando para baixo, atrás do texto.
            var tail = new GameObject("Tail", typeof(RectTransform), typeof(Image));
            tail.transform.SetParent(root.transform, false);
            var tailRect = tail.GetComponent<RectTransform>();
            tailRect.sizeDelta = new Vector2(TailSize, TailSize);
            tailRect.anchorMin = new Vector2(0.5f, 0f);
            tailRect.anchorMax = new Vector2(0.5f, 0f);
            tailRect.pivot = new Vector2(0.5f, 0.5f);
            tailRect.anchoredPosition = new Vector2(0f, -h * 0.5f + 1f);
            tailRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var tailImage = tail.GetComponent<Image>();
            tailImage.sprite = BubbleSprite();
            tailImage.color = BubbleColor;

            // Texto da fala.
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(root.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6f, 4f);
            textRect.offsetMax = new Vector2(-6f, -4f);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = line;
            tmp.fontSize = FontSize;
            tmp.color = TextColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.12f;
            tmp.outlineColor = OutlineColor;

            float hold = Mathf.Min(HoldMaxSeconds, HoldBaseSeconds + line.Length * HoldPerCharSeconds);
            var behavior = root.AddComponent<CreatureSpeechBubbleBehavior>();
            behavior.Setup(group, FadeInSeconds, hold, FadeOutSeconds, RiseDistance);
        }

        private Vector2 MeasureText(string line)
        {
            // GetPreferredValues funciona sem um canvas montado; usamos um TMP temporário.
            var probe = new GameObject("SpeechProbe", typeof(RectTransform));
            probe.transform.SetParent(_worldCanvas.transform, false);
            var tmp = probe.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = FontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.text = line;
            Vector2 pref = tmp.GetPreferredValues(line, MaxWidth - PadX, 0f);
            Destroy(probe);
            return new Vector2(Mathf.Min(pref.x, MaxWidth - PadX), pref.y);
        }
    }

    /// <summary>Fade-in → hold → fade-out com leve subida. Destrói o balão ao final.</summary>
    public sealed class CreatureSpeechBubbleBehavior : MonoBehaviour
    {
        private CanvasGroup _group;
        private float _fadeIn;
        private float _hold;
        private float _fadeOut;
        private float _rise;
        private float _elapsed;
        private float _total;
        private Vector3 _startPos;

        public void Setup(CanvasGroup group, float fadeIn, float hold, float fadeOut, float rise)
        {
            _group = group;
            _fadeIn = Mathf.Max(0.01f, fadeIn);
            _hold = Mathf.Max(0f, hold);
            _fadeOut = Mathf.Max(0.01f, fadeOut);
            _rise = rise;
            _elapsed = 0f;
            _total = _fadeIn + _hold + _fadeOut;
            _startPos = transform.position;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _total)
            {
                Destroy(gameObject);
                return;
            }

            float alpha;
            if (_elapsed < _fadeIn)
            {
                alpha = _elapsed / _fadeIn;
            }
            else if (_elapsed < _fadeIn + _hold)
            {
                alpha = 1f;
            }
            else
            {
                alpha = 1f - (_elapsed - _fadeIn - _hold) / _fadeOut;
            }

            if (_group != null)
            {
                _group.alpha = Mathf.Clamp01(alpha);
            }

            float progress = _elapsed / _total;
            transform.position = _startPos + Vector3.up * (_rise * progress);
        }
    }
}
