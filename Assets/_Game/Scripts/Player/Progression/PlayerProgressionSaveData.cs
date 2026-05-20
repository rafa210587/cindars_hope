using System;

namespace CindarsHope.Player.Progression
{
    [Serializable]
    public class PlayerProgressionSaveData
    {
        public int Level = 1;
        public int CurrentXp;
        public int XpToNextLevel = 100;
        public int UnspentAttributePoints;
        public int UnspentSkillPoints;
        public int Strength = 1;
        public int Dexterity = 1;
        public int Intelligence = 1;
        public int Willpower = 1;
        public int Constitution = 1;
        public int Breath = 1;
    }
}
