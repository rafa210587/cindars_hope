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
        public bool FirstSeen;
        public int KillCount;
        public List<string> DropsDiscovered = new List<string>();
        public List<string> WeaknessesDiscovered = new List<string>();
        public List<string> ResistancesDiscovered = new List<string>();
        public bool VulnerabilityWindowDiscovered;
        public int LastSeenCaveLevel;

        public BestiaryEntrySaveData() { }

        public BestiaryEntrySaveData(BestiaryEntry entry)
        {
            EnemyId = entry?.EnemyId ?? string.Empty;
            if (entry == null)
            {
                return;
            }

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
                EnemyId = EnemyId ?? string.Empty,
                FirstSeen = FirstSeen,
                KillCount = System.Math.Max(0, KillCount),
                DropsDiscovered = DropsDiscovered != null ? new List<string>(DropsDiscovered) : new List<string>(),
                WeaknessesDiscovered = WeaknessesDiscovered != null ? new List<string>(WeaknessesDiscovered) : new List<string>(),
                ResistancesDiscovered = ResistancesDiscovered != null ? new List<string>(ResistancesDiscovered) : new List<string>(),
                VulnerabilityWindowDiscovered = VulnerabilityWindowDiscovered,
                LastSeenCaveLevel = LastSeenCaveLevel
            };
        }
    }
}
