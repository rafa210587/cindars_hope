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

        private void OnEnable()
        {
            GameEventBus.Subscribe<DamageAppliedEvent>(DisplayDamage);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DamageAppliedEvent>(DisplayDamage);
        }

        private void DisplayDamage(DamageAppliedEvent evt)
        {
            if (evt?.DamageResult == null || evt.DamageResult.FinalDamage <= 0 && !evt.DamageResult.WasImmune)
                return;

            string damageText = evt.DamageResult.WasImmune ? "Immune" : evt.DamageResult.FinalDamage.ToString();
            Color damageColor = GetDamageColor(evt.DamageResult);

            CreateFloatingNumber(damageText, Vector3.zero, damageColor);
        }

        private void CreateFloatingNumber(string text, Vector3 position, Color color)
        {
            if (_worldCanvas == null)
            {
                _worldCanvas = FindObjectOfType<Canvas>();
                if (_worldCanvas == null)
                    return;
            }

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
