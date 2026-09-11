using System;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>Deterministic calculations shared by survival executors and their tests.</summary>
    public static class SurvivalSkillActionRules
    {
        public const float MovingAwayDotThreshold = .5f;

        public static int ResolvePercentHeal(int maxHp, float fraction)
            => maxHp <= 0 || fraction <= 0f
                ? 0
                : Mathf.CeilToInt(maxHp * Mathf.Clamp01(fraction));

        public static bool IsMovingAway(Vector2 playerPosition, Vector2 threatPosition,
            Vector2 committedDirection)
        {
            if (committedDirection.sqrMagnitude <= .0001f)
                return false;

            Vector2 away = playerPosition - threatPosition;
            if (away.sqrMagnitude <= .0001f)
                return false;

            return Vector2.Dot(committedDirection.normalized, away.normalized)
                >= MovingAwayDotThreshold;
        }

        public static float ResolveLureDuration(EnemyDifficulty difficulty,
            bool hasEliteClassification, float normalDuration, float eliteDuration)
        {
            if (difficulty == EnemyDifficulty.Boss)
                return 0f;
            if (difficulty == EnemyDifficulty.Elite ||
                difficulty == EnemyDifficulty.MiniBoss || hasEliteClassification)
                return Mathf.Max(0f, eliteDuration);
            return Mathf.Max(0f, normalDuration);
        }

        public static bool IsInsideInclusiveRadius(Vector2 point, Vector2 center, float radius)
            => radius >= 0f && (point - center).sqrMagnitude <= radius * radius;

        public static int CompareTargets(float leftDistanceSquared, string leftId,
            float rightDistanceSquared, string rightId)
        {
            int byDistance = leftDistanceSquared.CompareTo(rightDistanceSquared);
            return byDistance != 0
                ? byDistance
                : string.Compare(leftId ?? string.Empty, rightId ?? string.Empty,
                    StringComparison.Ordinal);
        }
    }
}
