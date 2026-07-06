using CindarsHope.Combat;

namespace CindarsHope.Enemy
{
    /// <summary>Canonical precedence rules for movement tuning used by EnemyBrain.</summary>
    public static class EnemyBrainTuningResolver
    {
        public static float DetectionRange(EnemyMovementProfileSO profile, EnemyDataSO data)
        {
            return profile?.DetectionRange ?? data?.detectionRadius ?? 10f;
        }

        public static float LeashRange(EnemyMovementProfileSO profile, EnemyDataSO data)
        {
            return profile?.LeashRange ?? DetectionRange(profile, data) * 3f;
        }

        public static float MoveSpeed(
            EnemyMovementProfileSO profile,
            EnemyDataSO data,
            float externalSpeedFactor,
            float phaseMoveSpeedMultiplier)
        {
            return (profile?.MoveSpeed ?? data?.moveSpeed ?? 2f)
                * externalSpeedFactor
                * phaseMoveSpeedMultiplier;
        }
    }
}
