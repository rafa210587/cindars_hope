using CindarsHope.Combat;

namespace CindarsHope.Enemy
{
    /// <summary>Normalization and precedence rules applied when EnemyBrain receives runtime config.</summary>
    public static class EnemyBrainConfigurationPolicy
    {
        public static string NormalizePackId(string packId)
        {
            return string.IsNullOrWhiteSpace(packId) ? null : packId;
        }

        public static float ResolveDecisionTick(EnemyMovementProfileSO profile, float currentTickSeconds)
        {
            return profile != null && profile.DecisionTickSeconds > 0f
                ? profile.DecisionTickSeconds
                : currentTickSeconds;
        }
    }
}
