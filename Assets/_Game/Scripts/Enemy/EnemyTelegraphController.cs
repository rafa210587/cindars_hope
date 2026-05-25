using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [DisallowMultipleComponent]
    public class EnemyTelegraphController : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private float _blinkTimer;
        private bool _isTelegraphing;
        private Color _blinkColor = Color.yellow;
        private float _blinkFrequency = 0.1f;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        private void Update()
        {
            if (!_isTelegraphing)
                return;

            _blinkTimer -= Time.deltaTime;
            if (_blinkTimer <= 0f)
            {
                _blinkTimer = _blinkFrequency;
                ToggleBlink();
            }
        }

        public void StartTelegraph(Color telegraphColor, float blinkFrequency = 0.1f)
        {
            if (_spriteRenderer == null)
                return;

            _isTelegraphing = true;
            _blinkColor = telegraphColor;
            _blinkFrequency = blinkFrequency;
            _blinkTimer = _blinkFrequency;
            _spriteRenderer.color = _blinkColor;
        }

        public void EndTelegraph()
        {
            _isTelegraphing = false;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
            GameEventBus.Publish(new EnemyTelegraphEndedEvent(gameObject.name));
        }

        private void ToggleBlink()
        {
            if (_spriteRenderer == null)
                return;

            _spriteRenderer.color = _spriteRenderer.color == _blinkColor ? _originalColor : _blinkColor;
        }

        public void SetOriginalColor(Color color)
        {
            _originalColor = color;
            if (!_isTelegraphing)
            {
                _spriteRenderer.color = _originalColor;
            }
        }
    }
}
