using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using TMPro;

namespace CindarsHope.Combat
{
    public class FloatingDamageNumberDisplayer : MonoBehaviour
    {
        [SerializeField] private Canvas _worldCanvas;
        [SerializeField] private float _displayDuration = 1.5f;
        [SerializeField] private float _moveDistance = 2f;
        [SerializeField] private int _fontSize = 36;

        // SPEC 14A-FIX7: track current instance so callers (e.g. CaveSceneRuntimeReferenceInstaller)
        // can avoid duplicate bootstrap without using FindObjectOfType (banned at runtime).
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

            // FIX7 fallback: build a world-space Canvas as our child so floating numbers render
            // even when the instance was created at runtime without a serialized canvas.
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
            if (evt?.DamageResult == null || evt.DamageResult.FinalDamage <= 0 && !evt.DamageResult.WasImmune)
                return;

            if (_worldCanvas == null)
            {
                Debug.LogWarning("FloatingDamageNumberDisplayer: World Canvas not assigned and not found in hierarchy.", this);
                return;
            }

            string damageText = evt.DamageResult.WasImmune ? "Immune" : evt.DamageResult.FinalDamage.ToString();
            Color damageColor = GetDamageColor(evt.DamageResult);

            Vector3 displayPosition = Vector3.zero;
            if (evt.TargetPosition != Vector3.zero)
            {
                displayPosition = evt.TargetPosition;
            }

            CreateFloatingNumber(damageText, displayPosition, damageColor);
        }

        private void DisplayPlayerDamage(PlayerDamagedEvent evt)
        {
            if (evt == null || evt.DamageAmount <= 0)
                return;

            if (_worldCanvas == null)
            {
                Debug.LogWarning("FloatingDamageNumberDisplayer: World Canvas not assigned and not found in hierarchy.", this);
                return;
            }

            CreateFloatingNumber(evt.DamageAmount.ToString(), evt.WorldPosition, Color.red);
        }

        private void CreateFloatingNumber(string text, Vector3 position, Color color)
        {
            if (_worldCanvas == null)
                return;

            var go = new GameObject("FloatingDamageNumber");
            go.transform.SetParent(_worldCanvas.transform, false);
            go.transform.position = position + Vector3.up * 0.5f;

            var textComponent = go.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = _fontSize;
            textComponent.color = color;
            textComponent.alignment = TextAlignmentOptions.Center;

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 100);

            var floatingBehavior = go.AddComponent<FloatingNumberBehavior>();
            floatingBehavior.SetupAnimation(_displayDuration, _moveDistance);
        }

        private Color GetDamageColor(DamageResult result)
        {
            if (result.WasImmune)
                return Color.gray;

            return result.DamageType switch
            {
                DamageType.Physical => Color.white,
                DamageType.Fire => new Color(1f, 0.5f, 0f),
                DamageType.Ice => new Color(0.5f, 0.8f, 1f),
                DamageType.Toxic => new Color(0f, 1f, 0.5f),
                DamageType.Lightning => new Color(1f, 1f, 0f),
                DamageType.Arcane => new Color(0.8f, 0.5f, 1f),
                DamageType.True => Color.red,
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

            // Move up
            transform.position = _startPos + Vector3.up * (_moveDistance * progress);

            // Fade out
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
