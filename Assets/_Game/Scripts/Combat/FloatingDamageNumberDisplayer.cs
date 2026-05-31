using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using TMPro;

namespace CindarsHope.Combat
{
    // SPEC 14A-FIX10: floating damage numbers anchored at the target via DamagePopupAnchor.
    // Physical damage = red, magical/elemental = blue, immune = gray. World-space canvas with
    // a tight scale so font/sizeDelta values translate to small, readable popups.
    public class FloatingDamageNumberDisplayer : MonoBehaviour
    {
        [SerializeField] private Canvas _worldCanvas;
        [SerializeField] private float _displayDuration = 0.8f;
        [SerializeField] private float _moveDistance = 0.35f;
        [SerializeField] private int _fontSize = 24;
        [SerializeField] private float _canvasWorldScale = 0.02f;
        [SerializeField] private Vector2 _rectSize = new Vector2(80f, 30f);
        [SerializeField] private float _headOffsetY = 0.55f;

        private static FloatingDamageNumberDisplayer s_currentInstance;

        public static FloatingDamageNumberDisplayer EnsureExists(Transform parent = null)
        {
            if (s_currentInstance != null) return s_currentInstance;
            var go = new GameObject("FloatingDamageNumberDisplayer_Runtime");
            if (parent != null) go.transform.SetParent(parent, false);
            return go.AddComponent<FloatingDamageNumberDisplayer>();
        }

        // SPEC 14A-FIX10: explicit show-at-target entry point. Callers in EnemyHealth/EnemyContactDamage
        // pass the GameObject they damaged; the displayer reads the DamagePopupAnchor (or the
        // collider/transform fallback) so the popup always lands above the actual target head.
        public static void ShowAtTarget(GameObject target, int amount, DamageType type, bool wasImmune, bool targetIsPlayer)
        {
            if (s_currentInstance == null || target == null) return;
            if (!wasImmune && amount <= 0) return;
            s_currentInstance.RenderAtTarget(target, amount, type, wasImmune, targetIsPlayer);
        }

        private void Awake()
        {
            if (s_currentInstance != null && s_currentInstance != this)
            {
                Debug.LogWarning("FloatingDamageNumberDisplayer: duplicate instance destroyed.", this);
                Destroy(gameObject);
                return;
            }
            s_currentInstance = this;
        }

        private void OnEnable()
        {
            if (_worldCanvas == null) _worldCanvas = GetComponentInParent<Canvas>();

            if (_worldCanvas == null)
            {
                var canvasGo = new GameObject("FloatingDamageCanvas_Runtime");
                canvasGo.transform.SetParent(transform, false);
                _worldCanvas = canvasGo.AddComponent<Canvas>();
                _worldCanvas.renderMode = RenderMode.WorldSpace;
                _worldCanvas.sortingOrder = 100;
                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                Debug.Log("FloatingDamageNumberDisplayer: created runtime world-space Canvas.", this);
            }

            _worldCanvas.transform.localScale = new Vector3(_canvasWorldScale, _canvasWorldScale, _canvasWorldScale);

            GameEventBus.Subscribe<DamageAppliedEvent>(DisplayDamageEvent);
            GameEventBus.Subscribe<PlayerDamagedEvent>(DisplayPlayerDamageEvent);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DamageAppliedEvent>(DisplayDamageEvent);
            GameEventBus.Unsubscribe<PlayerDamagedEvent>(DisplayPlayerDamageEvent);
        }

        private void OnDestroy()
        {
            if (s_currentInstance == this) s_currentInstance = null;
        }

        // Event-driven fallback when callers don't use ShowAtTarget directly. Uses OverlapPoint
        // at the event's TargetPosition to find a Collider2D and read its DamagePopupAnchor.
        private void DisplayDamageEvent(DamageAppliedEvent evt)
        {
            if (evt?.DamageResult == null) return;
            if (evt.DamageResult.FinalDamage <= 0 && !evt.DamageResult.WasImmune) return;
            if (evt.TargetPosition == Vector3.zero) return;

            string text = evt.DamageResult.WasImmune ? "Immune" : evt.DamageResult.FinalDamage.ToString();
            Color color = GetDamageColor(evt.DamageResult.DamageType, evt.DamageResult.WasImmune, false);

            Vector3 head = ResolveHeadPositionFromWorld(evt.TargetPosition);
            CreateFloatingNumber(text, head, color);
        }

        private void DisplayPlayerDamageEvent(PlayerDamagedEvent evt)
        {
            if (evt == null || evt.DamageAmount <= 0) return;
            if (evt.WorldPosition == Vector3.zero) return;

            Vector3 head = ResolveHeadPositionFromWorld(evt.WorldPosition);
            CreateFloatingNumber(evt.DamageAmount.ToString(), head, GetDamageColor(DamageType.Physical, false, true));
        }

        private void RenderAtTarget(GameObject target, int amount, DamageType type, bool wasImmune, bool targetIsPlayer)
        {
            Vector3 head = ResolveHeadPositionFromTarget(target);
            string text = wasImmune ? "Immune" : amount.ToString();
            Color color = GetDamageColor(type, wasImmune, targetIsPlayer);
            CreateFloatingNumber(text, head, color);
        }

        private Vector3 ResolveHeadPositionFromTarget(GameObject target)
        {
            if (target == null) return Vector3.zero;
            var anchor = target.GetComponent<DamagePopupAnchor>()
                         ?? target.GetComponentInParent<DamagePopupAnchor>()
                         ?? target.GetComponentInChildren<DamagePopupAnchor>();
            if (anchor != null) return anchor.GetPopupWorldPosition();
            var col = target.GetComponent<Collider2D>() ?? target.GetComponentInChildren<Collider2D>();
            if (col != null)
            {
                var b = col.bounds;
                return new Vector3(b.center.x, b.max.y + 0.05f, target.transform.position.z);
            }
            return target.transform.position + Vector3.up * _headOffsetY;
        }

        private Vector3 ResolveHeadPositionFromWorld(Vector3 worldPosition)
        {
            var hit = Physics2D.OverlapPoint(worldPosition);
            if (hit != null)
            {
                var anchor = hit.GetComponent<DamagePopupAnchor>() ?? hit.GetComponentInParent<DamagePopupAnchor>();
                if (anchor != null) return anchor.GetPopupWorldPosition();
                var b = hit.bounds;
                return new Vector3(b.center.x, b.max.y + 0.05f, worldPosition.z);
            }
            return worldPosition + Vector3.up * _headOffsetY;
        }

        private void CreateFloatingNumber(string text, Vector3 worldPosition, Color color)
        {
            if (_worldCanvas == null) return;
            if (worldPosition == Vector3.zero) return;

            var go = new GameObject("FloatingDamageNumber");
            go.transform.SetParent(_worldCanvas.transform, false);
            go.transform.position = worldPosition;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = _fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = new Color(0f, 0f, 0f, 0.9f);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = _rectSize;

            var behavior = go.AddComponent<FloatingNumberBehavior>();
            behavior.SetupAnimation(_displayDuration, _moveDistance);
        }

        // SPEC 14A-FIX10: Physical = vermelho saturado, magical/elemental = azul saturado.
        // Targeted at top-down 2D readability. Immune = cinza. Player damage uses Physical=red.
        private Color GetDamageColor(DamageType type, bool wasImmune, bool targetIsPlayer)
        {
            if (wasImmune) return new Color(0.55f, 0.55f, 0.55f);
            if (targetIsPlayer) return new Color(1f, 0.15f, 0.15f);

            switch (type)
            {
                case DamageType.Physical:  return new Color(1f, 0.15f, 0.15f);          // red
                case DamageType.True:      return new Color(1f, 0.4f, 0.4f);
                case DamageType.Fire:
                case DamageType.Ice:
                case DamageType.Lightning:
                case DamageType.Arcane:
                case DamageType.Toxic:     return new Color(0.35f, 0.65f, 1f);          // blue (magic/elemental)
                default:                   return new Color(1f, 0.15f, 0.15f);
            }
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
            if (progress >= 1f) { Destroy(gameObject); return; }

            transform.position = _startPos + Vector3.up * (_moveDistance * progress);

            var tmp = GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                var c = tmp.color;
                c.a = 1f - progress;
                tmp.color = c;
            }
        }
    }
}
