using System;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>Pure rank tables and eligibility rules for Telisandra's caveborn capstone.</summary>
    public static class CavebornCapstoneResolver
    {
        public const string NodeId = "survival_capstone_caveborn";
        public const float HpThreshold = .25f;
        public const float StaminaThreshold = .15f;
        public const float DodgeDistanceBonus = .4f;
        public const float RankThreeHealthRegenMultiplier = 2f;
        private const long ThresholdScale = 100L;
        private const long HpThresholdUnits = 25L;
        private const long StaminaThresholdUnits = 15L;

        private static readonly float[] Durations = { 0f, 8f, 9f, 10f };
        private static readonly float[] CostReductions = { 0f, .30f, .38f, .45f };
        private static readonly float[] Resistances = { 0f, .20f, .25f, .30f };

        public static int ClampRank(int rank) => Math.Max(0, Math.Min(3, rank));
        public static float DurationSeconds(int rank) => Durations[ClampRank(rank)];
        public static float CostMultiplier(int rank) => 1f - CostReductions[ClampRank(rank)];
        public static float ResistanceFraction(int rank) => Resistances[ClampRank(rank)];

        public static bool ShouldActivate(int rank, string runId, bool alreadyConsumed,
            int currentHp, int maxHp, int currentStamina, int maxStamina, bool isExhausted)
        {
            if (ClampRank(rank) <= 0 || string.IsNullOrWhiteSpace(runId) || alreadyConsumed)
                return false;

            bool lowHp = maxHp > 0 &&
                (long)currentHp * ThresholdScale < (long)maxHp * HpThresholdUnits;
            bool lowStamina = maxStamina > 0 &&
                (long)currentStamina * ThresholdScale < (long)maxStamina * StaminaThresholdUnits;
            return lowHp || lowStamina || isExhausted;
        }

        public static bool IsProtectedDamage(DamageType damageType)
            => damageType == DamageType.Ice || damageType == DamageType.Fire ||
               damageType == DamageType.Toxic;

        public static bool IsProtectedStatus(CavebornStatusFamily family)
            => family == CavebornStatusFamily.Cold || family == CavebornStatusFamily.Heat ||
               family == CavebornStatusFamily.Toxic || family == CavebornStatusFamily.Fear ||
               family == CavebornStatusFamily.Confusion;

        public static int ResolveIncomingDamage(int rawDamage, DamageType damageType, int rank)
        {
            if (rawDamage <= 0 || !IsProtectedDamage(damageType)) return Math.Max(0, rawDamage);
            return Math.Max(0, (int)Math.Ceiling(rawDamage * (1f - ResistanceFraction(rank))));
        }

        public static float ResolveStatusDuration(float durationSeconds,
            CavebornStatusFamily family, int rank)
        {
            if (durationSeconds <= 0f || !IsProtectedStatus(family))
                return Math.Max(0f, durationSeconds);
            return Math.Max(0f, durationSeconds * (1f - ResistanceFraction(rank)));
        }
    }
}
