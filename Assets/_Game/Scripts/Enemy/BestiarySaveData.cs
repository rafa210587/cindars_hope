using System.Collections.Generic;

namespace CindarsHope.Enemy
{
    [System.Serializable]
    public class BestiarySaveData
    {
        public List<BestiaryEntrySaveData> Entries = new List<BestiaryEntrySaveData>();
    }

    [System.Serializable]
    public class BestiaryEntrySaveData
    {
        public string EnemyId;
        public float FirstSeen;
        public int KillCount;
        public List<string> DropsDiscovered = new List<string>();
        public List<string> WeaknessesDiscovered = new List<string>();
        public List<string> ResistancesDiscovered = new List<string>();
        public bool VulnerabilityWindowDiscovered;
        public int LastSeenCaveLevel;

        public BestiaryEntrySaveData() { }

        public BestiaryEntrySaveData(BestiaryEntry entry)
        {
            EnemyId = entry.EnemyId;
            FirstSeen = entry.FirstSeen;
            KillCount = entry.KillCount;
            DropsDiscovered = new List<string>(entry.DropsDiscovered);
            WeaknessesDiscovered = new List<string>(entry.WeaknessesDiscovered);
            ResistancesDiscovered = new List<string>(entry.ResistancesDiscovered);
            VulnerabilityWindowDiscovered = entry.VulnerabilityWindowDiscovered;
            LastSeenCaveLevel = entry.LastSeenCaveLevel;
        }

        public BestiaryEntry ToEntry()
        {
            return new BestiaryEntry
            {
                EnemyId = EnemyId,
                FirstSeen = FirstSeen,
                KillCount = KillCount,
                DropsDiscovered = new List<string>(DropsDiscovered),
                WeaknessesDiscovered = new List<string>(WeaknessesDiscovered),
                ResistancesDiscovered = new List<string>(ResistancesDiscovered),
                VulnerabilityWindowDiscovered = VulnerabilityWindowDiscovered,
                LastSeenCaveLevel = LastSeenCaveLevel
            };
        }
    }
}
