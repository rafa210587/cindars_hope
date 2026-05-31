using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using TMPro;

namespace CindarsHope.Combat
{
    // SPEC 14A-FIX8: small floating damage numbers anchored above the target.
    // Configuration is tuned for a top-down 2D view; the runtime world-space Canvas is scaled
    // down so font/sizeDelta values translate to reasonable world units instead of giant text.
    public class FloatingDamageNumberDisplayer : MonoBehaviour
    {
        [SerializeField] private Canvas _worldCanvas;
        [SerializeField] private float _displayDuration = 0.8f;
        [SerializeField] private float _moveDistance = 0.4f;
        [SerializeField] private int _fontSize = 24;            // canvas-pixel size; with canvas scale 0.02 -> ~0.5 world units tall
        [SerializeField] private float _canvasWorldScale = 0.02f;
        [SerializeField] private Vector2 _rectSize = new Vector2(80f, 30f);
        [SerializeField] private float _headOffsetY = 0.6f;    // fallback offset when no collider available

        private static FloatingDamageNumberDisplayer s_currentInstance;

        public static FloatingDamageNumberDisplayer EnsureExists(Transform parent = null)
        {
            if (s_currentInstance != null) return s_currentInstance;
            var go = new GameObject("FloatingDamageNumberDisplayer_Runtime");
            if (parent != null) go.transform.SetParent(parent, false);
            return go.AddComponent<FloatingDamageNumberDisplayer>();
        }

        private void Awake()
        {
            if (s_currentInstance != null && s_currentInstance != this)
            {
                Debug.LogWarning("FloatingDamageNumberDisplayer: another instance already exists. Destroying this duplicate.", this);
                Destroy(gameObject);
                return;
            }
            s_currentInstance = this;
        }

        private void OnEnable()
        {
            if (_worldCanvas == null)
                _worldCanvas = GetComponentInParent<Canvas>();

            if (_worldCanvas == null)
            {
                var canvasGo = new GameObject("FloatingDamageCanvas_Runtime");
                canvasGo.transform.SetParent(transform, false);
                _worldCanvas = canvasGo.AddComponent<Canvas>();
                _worldCanvas.renderMode = RenderMode.WorldSpace;
                _worldCanvas.sortingOrder = 100;
                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                Debug.Log("FloatingDamageNumberDisplayer: created runtime world-space Canvas fallback.", this);
            }

            // FIX8: canvas-space units -> world units. With scale 0.02f, fontSize 24 renders at
            // ~0.48 world units tall, which reads as a small top-down popup instead of giant text.
            _worldCanvas.transform.localScale = new Vector3(_canvasWorldScale, _canvasWorldScale, _canvasWorldScale);

            GameEventBus.Subscribe<DamageAppliedEvent>(DisplayDamage);
            GameEventBus.Subscribe<PlayerDamagedEvent>(DisplayPlayerDamage);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DamageAppliedEvent>(DisplayDamage);
            GameEventBus.Unsubscribe<PlayerDamagedEvent>(DisplayPlayerDamage);
        }

        private void OnDestroy()
        {
            if (s_currentInstance == this) s_currentInstance = null;
        }

        private void DisplayDamage(DamageAppliedEvent evt)
        {
            if (evt?.DamageResult == null) return;
            if (evt.DamageResult.FinalDamage <= 0 && !evt.DamageResult.WasImmune) return;
            if (_worldCanvas == null) return;
            if (evt.TargetPosition == Vector3.zero) return; // skip nonsense origin

            string damageText = evt.DamageResult.WasImmune ? "Immune" : evt.DamageResult.FinalDamage.ToString();
            Color damageColor = GetDamageColor(evt.DamageResult);

            Vector3 head = ResolveHeadPosition(evt.TargetPosition);
            CreateFloatingNumber(damageText, head, damageColor);
        }

        private void DisplayPlayerDamage(PlayerDamagedEvent evt)
        {
            if (evt == null || evt.DamageAmount <= 0) return;
            if (_worldCanvas == null) return;
            if (evt.WorldPosition == Vector3.zero) return;

            Vector3 head = ResolveHeadPosition(evt.WorldPosition);
            CreateFloatingNumber(evt.DamageAmount.ToString(), head, Color.red);
        }

        // FIX8: anchor above the actual target via Collider2D bounds when available,
        // fallback to a fixed offset above the supplied position.
        private Vector3 ResolveHeadPosition(Vector3 targetWorldPosition)
        {
            var hit = Physics2D.OverlapPoint(targetWorldPosition);
            if (hit != null)
            {
                var b = hit.bounds;
                return new Vector3(b.center.x, b.max.y + 0.05f, targetWorldPosition.z);
            }
            return targetWorldPosition + Vector3.up * _headOffsetY;
        }

        private void CreateFloatingNumber(string text, Vector3 worldPosition, Color color)
        {
            if (_worldCanvas == null) return;

            var go = new GameObject("FloatingDamageNumber");
            go.transform.SetParent(_worldCanvas.transform, false);
            go.transform.position = worldPosition;

            var textComponent = go.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = _fontSize;
            textComponent.color = color;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.outlineWidth = 0.2f;
            textComponent.outlineColor = new Color(0f, 0f, 0f, 0.85f);

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = _rectSize;

            var floatingBehavior = go.AddComponent<FloatingNumberBehavior>();
            floatingBehavior.SetupAnimation(_displayDuration, _moveDistance);
        }

        // FIX8: distinguish physical (red) from magical/elemental (blue) damage.
        // Other types kept color-coded for clarity; "Immune" stays gray.
        private Color GetDamageColor(DamageResult result)
        {
            if (result.WasImmune) return new Color(0.6f, 0.6f, 0.6f);

            return result.DamageType switch
            {
                DamageType.Physical => new Color(1f, 0.85f, 0.85f),         // off-white red for melee
                DamageType.Fire => new Color(1f, 0.55f, 0.1f),
                DamageType.Ice => new Color(0.55f, 0.85f, 1f),
                DamageType.Toxic => new Color(0.4f, 1f, 0.4f),
                DamageType.Lightning => new Color(1f, 1f, 0.4f),
                DamageType.Arcane => new Color(0.7f, 0.55f, 1f),
                DamageType.True => new Color(1f, 0.2f, 0.2f),
                _ => Color.white
            };
        }
    }

    public class FloatingNumberBehavior : MonoBehaviour
    {
        private float _duration;
        private float _elapsedTime;
        private float _moveDistance;
        private Vector3 _startPos;

        public void SetupAnimation(float duration, float moveDistance)
        {
            _duration = duration;
            _moveDistance = moveDistance;
            _elapsedTime = 0f;
            _startPos = transform.position;
        }

        private void Update()
        {
            _elapsedTime += UnityEngine.Time.deltaTime;
            float progress = _elapsedTime / _duration;

            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = _startPos + Vector3.up * (_moveDistance * progress);

            var textComponent = GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                var color = textComponent.color;
                color.a = 1f - progress;
                textComponent.color = color;
            }
        }
    }
}
