using CindarsHope.Core;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerDodgeController : MonoBehaviour
    {
        [SerializeField] private float _dodgeDistance = 3f;
        [SerializeField] private float _dodgeDuration = 0.3f;
        [SerializeField] private float _dodgeCooldown = 1f;
        [SerializeField] private int _dodgeStaminaCost = 20;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private PlayerController _playerController;

        private float _lastDodgeTime = float.MinValue;
        private bool _isDodging;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryDodge();
            }
        }

        private void TryDodge()
        {
            if (_isDodging)
                return;

            if (Time.time - _lastDodgeTime < _dodgeCooldown)
                return;

            if (_staminaManager != null && !_staminaManager.TrySpendStamina(_dodgeStaminaCost))
                return;

            _lastDodgeTime = Time.time;
            StartCoroutine(ExecuteDodge());
        }

        private System.Collections.IEnumerator ExecuteDodge()
        {
            _isDodging = true;
            Vector2 dodgeDirection = _playerController != null ? _playerController.LastFacingDirection : Vector2.right;
            Vector2 targetPosition = (Vector2)transform.position + dodgeDirection * _dodgeDistance;
            Vector2 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < _dodgeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _dodgeDuration;
                Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, progress);

                if (_rigidbody != null)
                {
                    _rigidbody.MovePosition(newPosition);
                }
                else
                {
                    transform.position = newPosition;
                }

                yield return null;
            }

            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(targetPosition);
            }
            else
            {
                transform.position = targetPosition;
            }

            _isDodging = false;
        }
    }
}
