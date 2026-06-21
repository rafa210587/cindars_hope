namespace CindarsHope.Combat.Feel
{
    /// <summary>
    /// fable_71 — pure-C# tuning + math for the combat "feel" layer (hit-stop + screen shake).
    /// No UnityEngine dependency so every deterministic rule (clamp window, restore guard, shake
    /// decay, no-stack guard) is EditMode-testable. The MonoBehaviour adapters
    /// (<see cref="CombatHitStopController"/>, <see cref="CameraShakeController"/>) only translate
    /// engine input (Time.timeScale, camera transform, the GameEventBus) into calls on this logic.
    ///
    /// All feel values live here as named constants (rule no-magic-balance-values) — never inline
    /// magic numbers in the adapters.
    /// </summary>
    public static class CombatFeelTuning
    {
        // ---- Hit-stop window (milliseconds). Decision 4.2-A: 40-60 ms freeze-frame. ----
        public const float HitStopMinMs = 40f;
        public const float HitStopMaxMs = 60f;
        public const float HitStopDefaultMs = 50f;

        /// <summary>The "normal" timescale the hit-stop restores to. Hit-stop never restores blind.</summary>
        public const float NormalTimeScale = 1f;

        /// <summary>Below this timescale a pause/modal is considered already active (do not hit-stop).</summary>
        public const float PauseTimeScaleEpsilon = 0.001f;

        // ---- Screen shake. Leve = light amplitude/duration so it reads as emphasis, not chaos. ----
        public const float ShakeDefaultAmplitude = 0.18f;
        public const float ShakeDefaultDurationSeconds = 0.22f;
        public const float ShakeMaxAmplitude = 0.6f;
        public const float ShakeMaxDurationSeconds = 0.6f;

        /// <summary>Clamp a requested hit-stop (ms) into the canonical 40-60 ms window.</summary>
        public static float ClampHitStopMs(float requestedMs)
        {
            if (requestedMs < HitStopMinMs) return HitStopMinMs;
            if (requestedMs > HitStopMaxMs) return HitStopMaxMs;
            return requestedMs;
        }

        /// <summary>Convert a clamped ms window to seconds (measured against UNSCALED time).</summary>
        public static float HitStopSeconds(float requestedMs) => ClampHitStopMs(requestedMs) / 1000f;

        /// <summary>
        /// Whether a hit-stop is allowed to start. It must NOT start when a pause/modal already froze
        /// the clock (timescale at/near 0) — otherwise it would "eat" the pause on restore.
        /// </summary>
        public static bool CanStartHitStop(float currentTimeScale, bool alreadyActive)
        {
            if (alreadyActive) return false;
            return currentTimeScale > PauseTimeScaleEpsilon;
        }

        /// <summary>Clamp shake amplitude into a safe light range.</summary>
        public static float ClampAmplitude(float amplitude)
        {
            if (amplitude < 0f) return 0f;
            if (amplitude > ShakeMaxAmplitude) return ShakeMaxAmplitude;
            return amplitude;
        }

        /// <summary>Clamp shake duration into a safe light range.</summary>
        public static float ClampDuration(float durationSeconds)
        {
            if (durationSeconds < 0f) return 0f;
            if (durationSeconds > ShakeMaxDurationSeconds) return ShakeMaxDurationSeconds;
            return durationSeconds;
        }

        /// <summary>
        /// Linear decay factor [0..1] of the shake over its lifetime: full at start, 0 at the end.
        /// Deterministic so EditMode can assert the offset shrinks to exactly 0 (return to origin).
        /// </summary>
        public static float ShakeDecay(float elapsedSeconds, float totalSeconds)
        {
            if (totalSeconds <= 0f) return 0f;
            if (elapsedSeconds <= 0f) return 1f;
            if (elapsedSeconds >= totalSeconds) return 0f;
            return 1f - (elapsedSeconds / totalSeconds);
        }

        /// <summary>Current shake magnitude at <paramref name="elapsedSeconds"/> (amplitude * decay).</summary>
        public static float ShakeMagnitude(float amplitude, float elapsedSeconds, float totalSeconds)
        {
            return ClampAmplitude(amplitude) * ShakeDecay(elapsedSeconds, totalSeconds);
        }
    }
}
