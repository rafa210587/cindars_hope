using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace CindarsHope.Player.Movement
{
    // Input: hold Left Shift. Does not occupy active skill slots.
    [DisallowMultipleComponent]
    public sealed class PlayerBlockController : MonoBehaviour
    {
        // TODO_INTEGRATION_NOT_FINAL: final block slow/drain balance depends on combat tuning.
        [SerializeField] private float _blockSlowMultiplier = 0.5f;
        [SerializeField] private float _staminaDrainPerSecond = 10f;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private StaminaManager _staminaManager;

        private bool _isBlocking;
        private float _previousSpeedMultiplier = 1f;
        private float _staminaDrainAccumulator;

        public bool IsBlocking => _isBlocking;
        public float BlockSlowMultiplier => _blockSlowMultiplier;

        private void Start()
        {
            if (_playerController == null) _playerController = GetComponent<PlayerController>();

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
                if (_isBlocking) StopBlock();
                return;
            }

            if (UnityInput.GetKey(KeyCode.LeftShift))
            {
                if (!_isBlocking)
                {
                    TryStartBlock();
                }
                else
                {
                    DrainStamina();
                }
            }
            else if (_isBlocking)
            {
                StopBlock();
            }
        }

        private void OnDisable()
        {
            if (_isBlocking)
            {
                StopBlock();
            }
        }

        private void TryStartBlock()
        {
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(1))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para block."));
                return;
            }

            _previousSpeedMultiplier = _playerController != null ? _playerController.SpeedMultiplier : 1f;
            if (_playerController != null)
            {
                _playerController.SpeedMultiplier = Mathf.Max(0.01f, _previousSpeedMultiplier * _blockSlowMultiplier);
            }

            _isBlocking = true;
            _staminaDrainAccumulator = 0f;
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Block."));
        }

        private void DrainStamina()
        {
            if (_staminaManager == null)
            {
                // STAMINA_BLOCK_DEBT: block slow works without drain if no StaminaManager exists.
                return;
            }

            _staminaDrainAccumulator += _staminaDrainPerSecond * Time.deltaTime;
            var spend = Mathf.FloorToInt(_staminaDrainAccumulator);
            if (spend <= 0)
            {
                return;
            }

            _staminaDrainAccumulator -= spend;
            if (!_staminaManager.TrySpendStamina(spend))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para block."));
                StopBlock();
            }
        }

        private void StopBlock()
        {
            if (_playerController != null)
            {
                _playerController.SpeedMultiplier = _previousSpeedMultiplier;
            }

            _isBlocking = false;
            _staminaDrainAccumulator = 0f;
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Block released."));
        }
    }
}
