using System;

namespace CindarsHope.Foundation
{
    public enum RecoverableStatusFamily
    {
        Other = 0,
        Poison = 1,
        Burn = 2,
        Slow = 3
    }

    /// <summary>Final seams for survival passives whose consumers live outside Skills.</summary>
    public static class PassiveSurvivalModifierProvider
    {
        public static Func<float> ManaBaseRegenBonusFractionSource;
        public static Func<float> StatusRecoveryReductionSource;
        public static Func<float> TerrainPenaltyRecoverySource;

        public static float ResolveManaBaseRegen(float baseRegen)
        {
            float safeBase = Math.Max(0f, baseRegen);
            return safeBase * Math.Max(0f, ManaBaseRegenBonusFractionSource?.Invoke() ?? 0f);
        }

        public static float ResolveStatusDuration(float durationAfterResistance,
            RecoverableStatusFamily family)
        {
            float safeDuration = Math.Max(0f, durationAfterResistance);
            if (family == RecoverableStatusFamily.Other) return safeDuration;
            float reduction = Clamp01(StatusRecoveryReductionSource?.Invoke() ?? 0f);
            return Math.Max(1f, Math.Min(30f, safeDuration * (1f - reduction)));
        }

        public static float ResolveStatusRecoveryReduction(RecoverableStatusFamily family)
        {
            if (family == RecoverableStatusFamily.Other) return 0f;
            return Clamp01(StatusRecoveryReductionSource?.Invoke() ?? 0f);
        }

        public static float ResolveTerrainFactor(float basePenaltyFactor)
        {
            float penaltyFactor = Clamp01(basePenaltyFactor);
            float recovered = Clamp01(TerrainPenaltyRecoverySource?.Invoke() ?? 0f);
            return 1f - (1f - penaltyFactor) * (1f - recovered);
        }

        public static void Reset()
        {
            ManaBaseRegenBonusFractionSource = null;
            StatusRecoveryReductionSource = null;
            TerrainPenaltyRecoverySource = null;
        }

        private static float Clamp01(float value)
            => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
