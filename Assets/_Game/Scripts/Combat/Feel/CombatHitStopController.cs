using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat.Feel
{
    /// <summary>
    /// fable_71 — short hit-stop (freeze-frame) on a heavy hit / posture break, for tactile weight.
    ///
    /// Trigger (Phase 0 audit): consumes the EXISTING <see cref="EnemyPostureBrokenEvent"/> (F02 posture
    /// break = the canonical "heavy hit" moment). No new combat event is invented.
    ///
    /// Safety (CA-1, risk mitigation): captures the original <c>Time.timeScale</c> exactly once, never
    /// stacks simultaneous hit-stops, measures the window with UNSCALED time (so the freeze itself does
    /// not pause its own timer), and ALWAYS restores — in the coroutine, in <c>OnDisable</c> and in
    /// <c>OnDestroy</c>. It refuses to start when a pause/modal already froze the clock (timescale ~0)
    /// so it can never "eat" an active pause (<see cref="CombatFeelTuning.CanStartHitStop"/>).
    ///
    /// Thin adapter: every deterministic rule lives in <see cref="CombatFeelTuning"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatHitStopController : MonoBehaviour
    {
        [Tooltip("Hit-stop window in milliseconds. Clamped to the canonical 40-60 ms range.")]
        [SerializeField] private float _hitStopMilliseconds = CombatFeelTuning.HitStopDefaultMs;

        private bool _active;
        private float _restoreTimeScale = CombatFeelTuning.NormalTimeScale;
        private Coroutine _routine;
        private bool _subscribed;

        /// <summary>True while a hit-stop window is currently freezing the game.</summary>
        public bool IsHitStopActive => _active;

        private void OnEnable()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<EnemyPostureBrokenEvent>(OnPostureBroken);
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                GameEventBus.Unsubscribe<EnemyPostureBrokenEvent>(OnPostureBroken);
                _subscribed = false;
            }

            // Guaranteed restoration: a disabled controller must never leave the game frozen.
            ForceRestore();
        }

        private void OnDestroy()
        {
            ForceRestore();
        }

        private void OnPostureBroken(EnemyPostureBrokenEvent evt)
        {
            ApplyHitStop(_hitStopMilliseconds);
        }

        /// <summary>
        /// Apply a hit-stop of <paramref name="milliseconds"/> (clamped 40-60). No-op if a hit-stop is
        /// already active (no stacking) or if a pause/modal already froze the clock.
        /// </summary>
        public void ApplyHitStop(float milliseconds)
        {
            if (!CombatFeelTuning.CanStartHitStop(Time.timeScale, _active))
            {
                return;
            }

            _restoreTimeScale = Time.timeScale;
            _active = true;
            Time.timeScale = 0f;

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }
            _routine = StartCoroutine(HitStopRoutine(CombatFeelTuning.HitStopSeconds(milliseconds)));
        }

        private IEnumerator HitStopRoutine(float seconds)
        {
            // Unscaled time: the freeze (timeScale=0) would otherwise stop its own timer forever.
            yield return new WaitForSecondsRealtime(seconds);
            RestoreFromActive();
        }

        private void RestoreFromActive()
        {
            _routine = null;
            if (!_active) return;
            Time.timeScale = _restoreTimeScale;
            _active = false;
        }

        private void ForceRestore()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
            if (_active)
            {
                Time.timeScale = _restoreTimeScale;
                _active = false;
            }
        }
    }
}
