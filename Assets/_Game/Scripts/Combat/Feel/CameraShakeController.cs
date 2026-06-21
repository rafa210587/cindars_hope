using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat.Feel
{
    /// <summary>
    /// fable_71 — light screen shake on a charged attack / boss moment, to emphasize danger.
    ///
    /// Triggers (Phase 0 audit): consumes EXISTING events only — <see cref="PlayerChargedAttackEvent"/>
    /// (charged release, F02) and <see cref="BossPhaseChangedEvent"/> (cave boss phase change, F05). No
    /// new combat/boss event is invented, so the boss part is NOT deferred.
    ///
    /// Camera access (CA-2, no-runtime-global-search): the main camera is a SERIALIZED reference (or
    /// injected via <see cref="SetCamera"/> at bootstrap). It is NEVER resolved with a runtime global
    /// scene search (no-runtime-global-search). If no camera is wired the controller is a silent no-op with a single clear
    /// wiring log — never a global-search fallback.
    ///
    /// Return-to-origin is deterministic: the camera local position is restored exactly to the captured
    /// origin when the shake ends (<see cref="CombatFeelTuning.ShakeDecay"/> reaches 0). Thin adapter —
    /// magnitude math lives in <see cref="CombatFeelTuning"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraShakeController : MonoBehaviour
    {
        [Tooltip("Main camera transform to shake. Serialized ref or injected at bootstrap; no global search.")]
        [SerializeField] private Transform _cameraTransform;

        [SerializeField] private float _amplitude = CombatFeelTuning.ShakeDefaultAmplitude;
        [SerializeField] private float _durationSeconds = CombatFeelTuning.ShakeDefaultDurationSeconds;

        private bool _subscribed;
        private bool _wiringWarned;
        private bool _shaking;
        private float _elapsed;
        private float _activeAmplitude;
        private float _activeDuration;
        private Vector3 _origin;

        /// <summary>True while a shake is in progress.</summary>
        public bool IsShaking => _shaking;

        /// <summary>True when a camera transform is wired (otherwise every shake is a no-op).</summary>
        public bool HasCamera => _cameraTransform != null;

        /// <summary>Bootstrap injection point (avoids any scene search).</summary>
        public void SetCamera(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
            _wiringWarned = false;
        }

        private void OnEnable()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<PlayerChargedAttackEvent>(OnChargedAttack);
            GameEventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                GameEventBus.Unsubscribe<PlayerChargedAttackEvent>(OnChargedAttack);
                GameEventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
                _subscribed = false;
            }

            // Deterministic return-to-origin on teardown: never leave the camera offset.
            RestoreOrigin();
        }

        private void OnChargedAttack(PlayerChargedAttackEvent evt) => Shake(_amplitude, _durationSeconds);

        private void OnBossPhaseChanged(BossPhaseChangedEvent evt) => Shake(_amplitude, _durationSeconds);

        /// <summary>
        /// Start a light shake. No-op (with a single wiring log) when no camera is wired. Restarting
        /// while already shaking keeps the same captured origin so it never drifts.
        /// </summary>
        public void Shake(float amplitude, float durationSeconds)
        {
            if (_cameraTransform == null)
            {
                if (!_wiringWarned)
                {
                    Debug.LogWarning(
                        $"CameraShakeController: no main camera wired on '{name}' — shake is a no-op. " +
                        "Assign the serialized camera transform or inject via SetCamera at bootstrap.",
                        this);
                    _wiringWarned = true;
                }
                return;
            }

            _activeAmplitude = CombatFeelTuning.ClampAmplitude(amplitude);
            _activeDuration = CombatFeelTuning.ClampDuration(durationSeconds);
            if (_activeAmplitude <= 0f || _activeDuration <= 0f)
            {
                return;
            }

            if (!_shaking)
            {
                _origin = _cameraTransform.localPosition;
            }
            _elapsed = 0f;
            _shaking = true;
        }

        private void LateUpdate()
        {
            if (!_shaking || _cameraTransform == null) return;

            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed >= _activeDuration)
            {
                RestoreOrigin();
                return;
            }

            float magnitude = CombatFeelTuning.ShakeMagnitude(_activeAmplitude, _elapsed, _activeDuration);
            Vector2 offset = Random.insideUnitCircle * magnitude;
            _cameraTransform.localPosition = _origin + new Vector3(offset.x, offset.y, 0f);
        }

        private void RestoreOrigin()
        {
            if (!_shaking) return;
            _shaking = false;
            _elapsed = 0f;
            if (_cameraTransform != null)
            {
                _cameraTransform.localPosition = _origin;
            }
        }
    }
}
