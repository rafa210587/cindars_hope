using System;
using System.Collections.Generic;
using CindarsHope.Combat.Telemetry;

namespace CindarsHope.Skills
{
    /// <summary>
    /// Pure math for Phase 21 observations. It cannot certify asset provenance or accept a build:
    /// only a PlayMode run backed by real build/enemy assets may supply observations for acceptance.
    /// </summary>
    public static class SkillBalanceObservationEvaluator
    {
        public const float CommonStrongControlCap = .65f;
        public const float EliteStrongControlCap = .40f;
        public const float BossStrongControlCap = .20f;
        public const float MaximumUnreviewedTtkRatio = 1.25f;

        /// <summary>
        /// True means only that the input could be summarized. The caller must separately inspect
        /// TtkEvaluation and WithinStrongControlCap, then prove real asset provenance and tradeoffs.
        /// </summary>
        public static bool TrySummarize(
            TelemetryCreatureClass targetClass,
            IReadOnlyList<float> ttkSeconds,
            float strongControlSeconds,
            float observationWindowSeconds,
            out SkillBalanceObservationSummary summary)
        {
            summary = null;
            if (ttkSeconds == null || ttkSeconds.Count == 0 ||
                !IsFinitePositive(observationWindowSeconds) ||
                !IsFiniteNonNegative(strongControlSeconds) ||
                strongControlSeconds > observationWindowSeconds)
                return false;

            var values = new float[ttkSeconds.Count];
            for (var i = 0; i < values.Length; i++)
            {
                if (!IsFinitePositive(ttkSeconds[i])) return false;
                values[i] = ttkSeconds[i];
            }

            Array.Sort(values);
            var middle = values.Length / 2;
            // Use double for the even case so two large, valid floats cannot overflow before averaging.
            var median = values.Length % 2 == 0
                ? (float)(((double)values[middle - 1] + values[middle]) / 2d)
                : values[middle];
            var ttkEvaluation = TelemetryTargetEvaluator.EvaluateTtk(targetClass, median, values.Length);
            if (ttkEvaluation == TelemetryEvaluation.NoTarget) return false;

            var controlFraction = strongControlSeconds / observationWindowSeconds;
            var hasControlCap = TryGetControlCap(targetClass, out var cap);
            summary = new SkillBalanceObservationSummary(
                values.Length, median, ttkEvaluation, controlFraction,
                hasControlCap ? (bool?)(controlFraction <= cap) : null);
            return true;
        }

        /// <summary>
        /// Reports a gap requiring a measured risk/resource tradeoff review. It never turns a gap
        /// into acceptance by assigning arbitrary weights to control, damage or resources.
        /// </summary>
        public static bool RequiresTradeoffReview(float firstBuildMedianTtk, float secondBuildMedianTtk)
        {
            if (!IsFinitePositive(firstBuildMedianTtk) || !IsFinitePositive(secondBuildMedianTtk))
                throw new ArgumentOutOfRangeException(nameof(firstBuildMedianTtk), "Both measured TTK medians must be finite and positive.");
            var faster = Math.Min(firstBuildMedianTtk, secondBuildMedianTtk);
            var slower = Math.Max(firstBuildMedianTtk, secondBuildMedianTtk);
            return (double)slower / faster > MaximumUnreviewedTtkRatio;
        }

        private static bool TryGetControlCap(TelemetryCreatureClass targetClass, out float cap)
        {
            switch (targetClass)
            {
                case TelemetryCreatureClass.Common: cap = CommonStrongControlCap; return true;
                case TelemetryCreatureClass.Elite: cap = EliteStrongControlCap; return true;
                case TelemetryCreatureClass.Boss: cap = BossStrongControlCap; return true;
                default: cap = 0f; return false;
            }
        }

        private static bool IsFinitePositive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool IsFiniteNonNegative(float value) => value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public sealed class SkillBalanceObservationSummary
    {
        public int SampleCount { get; }
        public float TtkMedianSeconds { get; }
        public TelemetryEvaluation TtkEvaluation { get; }
        public float StrongControlFraction { get; }
        /// <summary>Null when no cap is authored for this target class (Miniboss).</summary>
        public bool? WithinStrongControlCap { get; }

        public SkillBalanceObservationSummary(int sampleCount, float ttkMedianSeconds,
            TelemetryEvaluation ttkEvaluation, float strongControlFraction, bool? withinStrongControlCap)
        {
            SampleCount = sampleCount;
            TtkMedianSeconds = ttkMedianSeconds;
            TtkEvaluation = ttkEvaluation;
            StrongControlFraction = strongControlFraction;
            WithinStrongControlCap = withinStrongControlCap;
        }
    }
}
