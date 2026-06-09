using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace CindarsHope.Player.Movement
{
    // WAVE_INTEGRATION_11: Implements Dash as a forward movement ability.
    // Design source: docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md §13
    //   Input: Space + direction
    //   Distance base: 3.2-4.0 tiles
    //   Cost: 40 Stamina base
    //   Cooldown: 0.75s-1.20s base
    //   Does NOT occupy active skill slot.
    //
    // Input: Space + direction. Does not occupy active skill slots.
    [DisallowMultipleComponent]
    public sealed class PlayerDashController : MonoBehaviour
    {
        // TODO_INTEGRATION_NOT_FINAL: final movement/stamina balance depends on combat tuning.
        [SerializeField] private float _dashDistance = 3.5f;
        [SerializeField] private float _dashDuration = 0.14f;
        [SerializeField] private float _dashCooldown = 1.0f;
        [SerializeField] private int _dashStaminaCost = 40;

        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlayerMovementDisplacementResolver _displacementResolver;
        [SerializeField] private StaminaManager _staminaManager;

        private float _lastDashTime = float.MinValue;

        public bool IsDashing => _displacementResolver != null && _displacementResolver.IsDisplacing;

        private void Start()
        {
            if (_playerController == null) _playerController = GetComponent<PlayerController>();
            if (_displacementResolver == null) _displacementResolver = GetComponent<PlayerMovementDisplacementResolver>();

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && _staminaManager == null)
                _staminaManager = bootstrap.StaminaManager;
        }

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
                return;

            // Dash input: Space + direction.
            if (UnityInput.GetKeyDown(KeyCode.Space))
            {
                var moveInput = ReadDirectionalInput();
                if (moveInput.sqrMagnitude <= 0.1f && _playerController != null) moveInput = _playerController.LastFacingDirection;

                if (moveInput.sqrMagnitude > 0.1f)
                {
                    TryDash(moveInput.normalized);
                }
                else
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash bloqueado: nenhuma direcao disponivel."));
                }
            }
        }

        private void TryDash(Vector2 direction)
        {
            if (_displacementResolver == null || _displacementResolver.IsDisplacing)
                return;

            if (Time.time - _lastDashTime < _dashCooldown)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash em cooldown."));
                return;
            }

            if (_staminaManager != null && !_staminaManager.TrySpendStamina(_dashStaminaCost))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para dash."));
                return;
            }

            _lastDashTime = Time.time;
            if (!_displacementResolver.TryDisplace(direction, _dashDistance, _dashDuration, () =>
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash!"));
            }))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash bloqueado: movimento indisponivel."));
            }
        }

        private static Vector2 ReadDirectionalInput()
        {
            var x = 0f;
            var y = 0f;
            if (UnityInput.GetKey(KeyCode.A) || UnityInput.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (UnityInput.GetKey(KeyCode.D) || UnityInput.GetKey(KeyCode.RightArrow)) x += 1f;
            if (UnityInput.GetKey(KeyCode.S) || UnityInput.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (UnityInput.GetKey(KeyCode.W) || UnityInput.GetKey(KeyCode.UpArrow)) y += 1f;
            var direction = new Vector2(x, y);
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }
    }
}
