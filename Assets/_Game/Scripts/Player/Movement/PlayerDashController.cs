using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player.Movement
{
    // Input: Space + direction. Does not occupy active skill slots.
    // Combat Core §13: Dash base 3.2–4.0 tiles, duration 0.18–0.30s, cooldown 0.75–1.20s, cost 40 stamina.
    [DisallowMultipleComponent]
    public sealed class PlayerDashController : MonoBehaviour
    {
        // BALANCE_FINAL_PENDING — top of base range per COMBAT_CORE_DIRECTION §13
        [SerializeField] private float _dashDistance = 4.0f;
        [SerializeField] private float _dashDuration = 0.22f;
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
                _staminaManager = bootstrap.StaminaManager as StaminaManager;
        }

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;
            if (!PlayerMovementActionInput.WasDashPressed()) return;

            var moveInput = PlayerMovementActionInput.GetMoveDirectionHeld();
            if (moveInput.sqrMagnitude <= 0.1f && _playerController != null)
                moveInput = _playerController.LastFacingDirection;

            if (moveInput.sqrMagnitude > 0.1f)
                TryDash(moveInput.normalized);
            else
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash bloqueado: nenhuma direcao disponivel."));
        }

        private void TryDash(Vector2 direction)
        {
            if (_displacementResolver == null || _displacementResolver.IsDisplacing) return;

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
            Debug.Log($"[PlayerDashController] Dash requested direction={direction} distance={_dashDistance} targetObject={gameObject.name}");

            if (!_displacementResolver.TryDisplace(direction, _dashDistance, _dashDuration, () =>
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash!"))))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash bloqueado: movimento indisponivel."));
            }
        }
    }
}
