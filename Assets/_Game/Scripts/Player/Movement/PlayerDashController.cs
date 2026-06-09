using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    // NOTE: The existing PlayerAttackController.TryDodge() (Space) and
    // PlayerDodgeController (Space) also use Space. This controller adds
    // directional dash behavior; the conflict is documented as movement input debt.
    // TODO_INTEGRATION_NOT_FINAL: Unify Dash/Dodge input handling in a future movement refactor.
    // Dash/Dodge input debt: PlayerAttackController and PlayerDodgeController both claim Space key.
    [DisallowMultipleComponent]
    public sealed class PlayerDashController : MonoBehaviour
    {
        // Design direction: 3.2-4.0 tiles base
        [SerializeField] private float _dashDistance = 3.5f;
        // Design direction: 0.18s-0.30s duration
        [SerializeField] private float _dashDuration = 0.22f;
        // Design direction: 0.75s-1.20s cooldown
        [SerializeField] private float _dashCooldown = 1.0f;
        // Design direction: 40 Stamina cost
        [SerializeField] private int _dashStaminaCost = 40;

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private StaminaManager _staminaManager;

        private float _lastDashTime = float.MinValue;
        private bool _isDashing;
        private float _previousSpeedMultiplier = 1f;

        public bool IsDashing => _isDashing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBinding()
        {
            BindToPlayer();
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindToPlayer();
        }

        private static void BindToPlayer()
        {
            var playerObject = GameBootstrap.Instance?.PlayerManager != null
                ? GameBootstrap.Instance.PlayerManager.gameObject
                : null;
            if (playerObject == null)
            {
                return;
            }

            if (playerObject.GetComponent<PlayerDashController>() == null)
            {
                playerObject.AddComponent<PlayerDashController>();
            }

            if (playerObject.GetComponent<PlayerMovementAbilityController>() == null)
            {
                playerObject.AddComponent<PlayerMovementAbilityController>();
            }
        }

        private void Start()
        {
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null) _collider = GetComponent<Collider2D>();
            if (_playerController == null) _playerController = GetComponent<PlayerController>();

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && _staminaManager == null)
                _staminaManager = bootstrap.StaminaManager;
        }

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
                return;

            // Dash: Space + any directional input (W/A/S/D)
            if (UnityInput.GetKeyDown(KeyCode.Space))
            {
                var moveInput = _playerController != null ? _playerController.MoveInput : Vector2.zero;
                if (moveInput.sqrMagnitude <= 0.1f && _playerController != null)
                {
                    moveInput = _playerController.LastFacingDirection;
                }

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

        private void OnDisable()
        {
            if (_isDashing && _playerController != null)
            {
                _playerController.SpeedMultiplier = _previousSpeedMultiplier;
            }

            _isDashing = false;
        }

        private void TryDash(Vector2 direction)
        {
            if (_isDashing)
                return;

            var dodgeController = GetComponent<PlayerMovementAbilityController>();
            if (dodgeController != null && dodgeController.IsDodging)
                return;

            if (Time.time - _lastDashTime < _dashCooldown)
            {
                float remaining = _dashCooldown - (Time.time - _lastDashTime);
                GameEventBus.Publish(new PlayerActionFeedbackEvent($"Dash em cooldown ({remaining:F1}s)."));
                return;
            }

            if (_staminaManager != null && !_staminaManager.TrySpendStamina(_dashStaminaCost))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para dash."));
                return;
            }

            _lastDashTime = Time.time;
            StartCoroutine(ExecuteDash(direction));
        }

        private IEnumerator ExecuteDash(Vector2 direction)
        {
            _isDashing = true;

            Vector2 origin = transform.position;
            Vector2 target = GridMovementDisplacementResolver.Resolve(
                origin, direction, _dashDistance, colliderRadius: 0.3f, movingCollider: _collider);

            if (_playerController != null)
            {
                _previousSpeedMultiplier = _playerController.SpeedMultiplier;
                _playerController.SpeedMultiplier = 0f;
            }

            float elapsed = 0f;
            while (elapsed < _dashDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _dashDuration);
                Vector2 newPos = Vector2.Lerp(origin, target, t);

                if (_rigidbody != null)
                    _rigidbody.MovePosition(newPos);
                else
                    transform.position = newPos;

                yield return null;
            }

            if (_rigidbody != null)
                _rigidbody.MovePosition(target);
            else
                transform.position = target;

            GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash!"));
            if (_playerController != null)
            {
                _playerController.SpeedMultiplier = _previousSpeedMultiplier;
            }
            _isDashing = false;
        }
    }
}
