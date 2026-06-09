using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player.Movement
{
    // WAVE_INTEGRATION_11: Orchestrates Dodge (double-tap directional) movement ability.
    // Dash is handled by PlayerDashController (Space + direction).
    // Block is audited below but deferred (no combat scene target in MVP FarmScene).
    //
    // Design source: docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md §14
    //   Dodge: double tap direcional, 1.2-1.8 tiles, i-frame 0.16s-0.24s
    //   Cost: 40 Stamina base
    //   Cooldown: 0.45s-0.90s base
    //   Does NOT occupy active skill slot.
    //
    // Block audit:
    //   Input: Left Shift (from COMBAT_CORE_DIRECTION.md §5)
    //   Status: BLOCK_RUNTIME_DEFERRED_WITH_REASON
    //   Reason: Block requires combat/cave target context and EnemyHealth pipeline.
    //           FarmScene MVP does not have enemies. Deferred to combat integration wave.
    [DisallowMultipleComponent]
    public sealed class PlayerMovementAbilityController : MonoBehaviour
    {
        // Design direction: 1.2-1.8 tiles
        [SerializeField] private float _dodgeDistance = 1.5f;
        // Design direction: 0.28s-0.45s
        [SerializeField] private float _dodgeDuration = 0.32f;
        // Design direction: 0.45s-0.90s
        [SerializeField] private float _dodgeCooldown = 0.6f;
        // Design direction: 40 Stamina base
        [SerializeField] private int _dodgeStaminaCost = 40;

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private StaminaManager _staminaManager;

        [SerializeField] private DirectionalDoubleTapDetector _doubleTapDetector;
        private float _lastDodgeTime = float.MinValue;
        private bool _isDodging;
        private float _previousSpeedMultiplier = 1f;

        public bool IsDodging => _isDodging;

        private void Start()
        {
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null) _collider = GetComponent<Collider2D>();
            if (_playerController == null) _playerController = GetComponent<PlayerController>();
            if (_doubleTapDetector == null) _doubleTapDetector = GetComponent<DirectionalDoubleTapDetector>();
            if (_doubleTapDetector == null) _doubleTapDetector = gameObject.AddComponent<DirectionalDoubleTapDetector>();

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && _staminaManager == null)
                _staminaManager = bootstrap.StaminaManager;
        }

        private void Update()
        {
            if (GetComponent<PlayerDodgeController>() != null)
                return;

            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
                return;

            var doubleTapDir = _doubleTapDetector != null ? _doubleTapDetector.UpdateAndCheckDoubleTap() : null;
            if (doubleTapDir.HasValue)
            {
                TryDodge(doubleTapDir.Value);
            }
        }

        private void OnDisable()
        {
            if (_isDodging && _playerController != null)
            {
                _playerController.SpeedMultiplier = _previousSpeedMultiplier;
            }

            _isDodging = false;
        }

        private void TryDodge(Vector2 direction)
        {
            if (_isDodging)
                return;

            var dashController = GetComponent<PlayerDashController>();
            if (dashController != null && dashController.IsDashing)
                return;

            if (Time.time - _lastDodgeTime < _dodgeCooldown)
            {
                float remaining = _dodgeCooldown - (Time.time - _lastDodgeTime);
                GameEventBus.Publish(new PlayerActionFeedbackEvent($"Dodge em cooldown ({remaining:F1}s)."));
                return;
            }

            if (_staminaManager != null && !_staminaManager.TrySpendStamina(_dodgeStaminaCost))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para dodge."));
                return;
            }

            _lastDodgeTime = Time.time;
            StartCoroutine(ExecuteDodge(direction));
        }

        private IEnumerator ExecuteDodge(Vector2 direction)
        {
            _isDodging = true;

            Vector2 origin = transform.position;
            Vector2 target = GridMovementDisplacementResolver.Resolve(
                origin, direction, _dodgeDistance, colliderRadius: 0.3f, movingCollider: _collider);

            if (_playerController != null)
            {
                _previousSpeedMultiplier = _playerController.SpeedMultiplier;
                _playerController.SpeedMultiplier = 0f;
            }

            float elapsed = 0f;
            while (elapsed < _dodgeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _dodgeDuration);
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

            GameEventBus.Publish(new PlayerActionFeedbackEvent("Dodge!"));
            if (_playerController != null)
            {
                _playerController.SpeedMultiplier = _previousSpeedMultiplier;
            }

            _isDodging = false;
        }
    }
}
