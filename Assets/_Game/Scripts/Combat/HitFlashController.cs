using System.Collections;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class HitFlashController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _flashColor = Color.red;
        [SerializeField] private float _flashDuration = 0.12f;

        private Color _originalColor;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        private void OnDisable()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        public void Flash()
        {
            if (_spriteRenderer == null)
            {
                Debug.LogWarning($"HitFlashController: no SpriteRenderer found on '{name}'.", this);
                return;
            }

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashRoutine());
            Debug.Log($"HitFlashController: flash on '{name}'.");
        }

        private IEnumerator FlashRoutine()
        {
            _spriteRenderer.color = _flashColor;
            yield return new WaitForSeconds(_flashDuration);
            _spriteRenderer.color = _originalColor;
            _flashCoroutine = null;
        }
    }
}
