using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Farm;
using CindarsHope.Player.Data;
using UnityEngine;
// PR-010 keeps the legacy keyboard fallback so movement works without changing
// Packages/ProjectSettings. PlayerInputActions.inputactions is the contract for
// a future PR that installs/enables the Input System package and migrates input.
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        private const float DefaultMoveSpeed = 5f;
        private const float StepEventDistance = 1f;

        [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private CindarsHope.Core.Data.PlayerNeedsBalanceSO _playerNeedsBalance;

#if ENABLE_INPUT_SYSTEM
        private InputAction _moveAction;
#endif
        private float _distanceSinceLastStep;
        private bool _loggedMissingPlayerData;
        private bool _loggedMissingRigidbody;
        private Vector2 _lastFacingDirection = Vector2.right;

        public Vector2 MoveInput { get; private set; }
        public float SpeedMultiplier { get; set; } = 1f;
        public Vector2 LastFacingDirection => _lastFacingDirection;

        private void Awake()
        {
            AssignRigidbodyIfNeeded();

#if ENABLE_INPUT_SYSTEM
            _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
#endif
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            _moveAction?.Enable();
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            _moveAction?.Disable();
#endif
            MoveInput = Vector2.zero;
        }

        private void Reset()
        {
            AssignRigidbodyIfNeeded();
        }

        private void OnValidate()
        {
            AssignRigidbodyIfNeeded();
        }

        private void Update()
        {
            MoveInput = ReadMoveInput();
            if (MoveInput.sqrMagnitude > 1f)
            {
                MoveInput = MoveInput.normalized;
            }

            if (MoveInput.sqrMagnitude > 0.01f)
            {
                _lastFacingDirection = MoveInput;
            }
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null)
            {
                LogMissingRigidbodyOnce();
                return;
            }

            var previousPosition = _rigidbody.position;
            var speed = GetMoveSpeed();
            var movement = MoveInput * speed * Mathf.Max(0f, SpeedMultiplier) * GetHungerMoveSpeedModifier() * Time.fixedDeltaTime;
            var nextPosition = previousPosition + movement;

            _rigidbody.MovePosition(nextPosition);
            TrackStepDistance(previousPosition, nextPosition);
        }

        private Vector2 ReadMoveInput()
        {
            if (FarmPlot.IsAnyActionMenuOpen)
            {
                return Vector2.zero;
            }

            var modalManager = GameBootstrap.Instance?.ModalManager;
            if (modalManager != null && modalManager.HasActiveModal)
            {
                return Vector2.zero;
            }

#if ENABLE_INPUT_SYSTEM
            return _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
#else
            var x = 0f;
            var y = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                x -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                x += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                y -= 1f;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                y += 1f;
            }

            return new Vector2(x, y);
#endif
        }

        private float GetMoveSpeed()
        {
            if (_playerData != null)
            {
                return Mathf.Max(0.1f, _playerData.MoveSpeed);
            }

            if (!_loggedMissingPlayerData)
            {
                Debug.LogWarning($"{nameof(PlayerController)} on '{name}' has no PlayerDataSO assigned. Using fallback move speed {DefaultMoveSpeed}.");
                _loggedMissingPlayerData = true;
            }

            return DefaultMoveSpeed;
        }

        private float GetHungerMoveSpeedModifier()
        {
            var hungerManager = GameBootstrap.Instance != null ? GameBootstrap.Instance.HungerManager : null;
            if (hungerManager == null)
            {
                return 1f;
            }

            if (_playerNeedsBalance != null)
            {
                return Mathf.Max(0f, _playerNeedsBalance.GetMoveSpeedModifier(hungerManager.CurrentHunger));
            }

            return hungerManager.CurrentHunger > 0 && hungerManager.CurrentHunger < 10 ? 0.85f : 1f;
        }

        private void TrackStepDistance(Vector2 previousPosition, Vector2 nextPosition)
        {
            var delta = Vector2.Distance(previousPosition, nextPosition);
            if (delta <= 0f)
            {
                return;
            }

            _distanceSinceLastStep += delta;
            if (_distanceSinceLastStep < StepEventDistance)
            {
                return;
            }

            var publishedDistance = _distanceSinceLastStep;
            _distanceSinceLastStep = 0f;
            GameEventBus.Publish(new PlayerStepEvent(nextPosition, publishedDistance));
        }

        private void AssignRigidbodyIfNeeded()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
        }

        private void LogMissingRigidbodyOnce()
        {
            if (_loggedMissingRigidbody)
            {
                return;
            }

            Debug.LogError($"{nameof(PlayerController)} on '{name}' requires a Rigidbody2D assigned on the same GameObject.");
            _loggedMissingRigidbody = true;
        }
    }
}
