using UnityEngine;

namespace CindarsHope.Player.Progression
{
    public static class LevelUpManager
    {
        public const int MaxLevel = 100;
        public const int StartingAttributeValue = 1;
        public const int AttributeMaxValue = 100;
        public const int BaseXpForLevel2 = 100;

        public static int GetXpRequiredForLevel(int targetLevel)
        {
            if (targetLevel < 2 || targetLevel > MaxLevel)
                return 0;

            int levelBlock = (targetLevel - 1) / 10;
            float multiplier = 1f + (levelBlock * 0.1f);
            int xpNeeded = Mathf.RoundToInt(BaseXpForLevel2 * (targetLevel - 1) * multiplier);
            return Mathf.Max(1, xpNeeded);
        }

        public static int GetAttributePointsAtLevel(int level)
        {
            return Mathf.Max(0, level - 1);
        }

        public static int GetSkillPointsAtLevel(int level)
        {
            return (level - 1) / 3;
        }

        public static void ApplyAttributePoint(PlayerProgressionSaveData progression, PlayerAttribute attribute)
        {
            if (progression.UnspentAttributePoints <= 0)
                return;

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
