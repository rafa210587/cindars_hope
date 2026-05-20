using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Player.Progression
{
    public static class PlayerProgressionRules
    {
        public const int MaxLevel = 100;

        public static int CalculateXpToNextLevel(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            int levelBand = (level - 1) / 10;
            int levelMultiplier = 50 + levelBand * 10;
            return 100 + ((level - 1) * levelMultiplier);
        }

        public static int CalculateEnemyXpReward(int enemyLevel, EnemyDifficulty difficulty, int overrideValue)
        {
            if (overrideValue > 0)
            {
                return overrideValue;
            }

            int multiplier;
            switch (difficulty)
            {
                case EnemyDifficulty.VeryEasy:
                    multiplier = 5;
                    break;
                case EnemyDifficulty.Easy:
                    multiplier = 8;
                    break;
                case EnemyDifficulty.Normal:
                    multiplier = 10;
                    break;
                case EnemyDifficulty.Hard:
                    multiplier = 15;
                    break;
                case EnemyDifficulty.Elite:
                    multiplier = 25;
                    break;
                case EnemyDifficulty.MiniBoss:
                    multiplier = 50;
                    break;
                case EnemyDifficulty.Boss:
                    multiplier = 100;
                    break;
                default:
                    multiplier = 8;
                    break;
            }

            return Mathf.Max(1, enemyLevel) * multiplier;
        }
    }
}
