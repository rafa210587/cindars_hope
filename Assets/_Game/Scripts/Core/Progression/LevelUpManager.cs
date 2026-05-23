using UnityEngine;

namespace CindarsHope.Player.Progression
{
    public static class LevelUpManager
    {
        public const int MaxLevel = PlayerProgressionRules.MaxLevel;
        public const int StartingAttributeValue = 1;
        public const int AttributeMaxValue = 100;

        public static int GetXpRequiredForLevel(int targetLevel)
        {
            if (targetLevel < 2 || targetLevel > MaxLevel)
            {
                return 0;
            }

            return PlayerProgressionRules.CalculateXpToNextLevel(targetLevel - 1);
        }

        public static int GetAttributePointsAtLevel(int level)
        {
            return PlayerProgressionRules.CalculateTotalAttributePointsAtLevel(level);
        }

        public static int GetSkillPointsAtLevel(int level)
        {
            return PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(level);
        }

        public static void ApplyAttributePoint(PlayerProgressionSaveData progression, PlayerAttribute attribute)
        {
            if (progression == null || progression.UnspentAttributePoints <= 0)
            {
                return;
            }

            switch (attribute)
            {
                case PlayerAttribute.Strength:
                    progression.Strength = Mathf.Min(AttributeMaxValue, progression.Strength + 1);
                    break;
                case PlayerAttribute.Dexterity:
                    progression.Dexterity = Mathf.Min(AttributeMaxValue, progression.Dexterity + 1);
                    break;
                case PlayerAttribute.Intelligence:
                    progression.Intelligence = Mathf.Min(AttributeMaxValue, progression.Intelligence + 1);
                    break;
                case PlayerAttribute.Willpower:
                    progression.Willpower = Mathf.Min(AttributeMaxValue, progression.Willpower + 1);
                    break;
                case PlayerAttribute.Constitution:
                    progression.Constitution = Mathf.Min(AttributeMaxValue, progression.Constitution + 1);
                    break;
                case PlayerAttribute.Breath:
                    progression.Breath = Mathf.Min(AttributeMaxValue, progression.Breath + 1);
                    break;
            }

            progression.UnspentAttributePoints--;
        }
    }

    public enum PlayerAttribute
    {
        Strength,
        Dexterity,
        Intelligence,
        Willpower,
        Constitution,
        Breath
    }
}