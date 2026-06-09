using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player.Movement
{
    // Input: double tap directional. Does not occupy active skill slots.
    [DisallowMultipleComponent]
    public sealed class PlayerDodgeController : MonoBehaviour
    {
        // TODO_INTEGRATION_NOT_FINAL: final movement/stamina balance depends on combat tuning.
        [SerializeField] private float _dodgeDistance = 1.5f;
        [SerializeField] private float _dodgeDuration = 0.10f;
        [SerializeField] private float _dodgeCooldown = 0.6f;
        [SerializeField] private int _dodgeStaminaCost = 40;

        [SerializeField] private DirectionalDoubleTapDetector _doubleTapDetector;
        [SerializeField] private PlayerMovementDisplacementResolver _displacementResolver;
        [SerializeField] private StaminaManager _staminaManager;

        private float _lastDodgeTime = float.MinValue;

        public bool IsDodging => _displacementResolver != null && _displacementResolver.IsDisplacing;

        private void Start()
        {
            if (_doubleTapDetector == null) _doubleTapDetector = GetComponent<DirectionalDoubleTapDetector>();
            if (_displacementResolver == null) _displacementResolver = GetComponent<PlayerMovementDisplacementResolver>();

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && _staminaManager == null)
            {
                _staminaManager = bootstrap.StaminaManager;
            }
        }

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
            {
                return;
            }

            var direction = _doubleTapDetector != null ? _doubleTapDetector.UpdateAndCheckDoubleTap() : null;
            if (direction.HasValue)
            {
                TryDodge(direction.Value);
            }
        }

        private void TryDodge(Vector2 direction)
        {
            if (_displacementResolver == null || _displacementResolver.IsDisplacing)
            {
                return;
            }

            if (Time.time - _lastDodgeTime < _dodgeCooldown)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dodge em cooldown."));
                return;
            }

            if (_staminaManager != null && !_staminaManager.TrySpendStamina(_dodgeStaminaCost))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para dodge."));
                return;
            }

            _lastDodgeTime = Time.time;
            if (!_displacementResolver.TryDisplace(direction, _dodgeDistance, _dodgeDuration, () =>
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dodge!"));
            }))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dodge bloqueado: movimento indisponivel."));
            }
        }
    }
}
