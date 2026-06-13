using System;

namespace CindarsHope.Player.Progression
{
    [Serializable]
    public class PlayerProgressionSaveData
    {
        public int Level = 1;
        public int CurrentXp;
        public int XpToNextLevel = 100;
        // F42: fonte de verdade da progressão (nível é derivado). 0 em saves legados → migração.
        public long TotalXp;
        // F42: idempotência de XP por descoberta de nível da caverna e primeira colheita.
        public int DeepestXpAwardedCaveLevel;
        public System.Collections.Generic.List<string> FirstHarvestXpSeedIds = new System.Collections.Generic.List<string>();
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
