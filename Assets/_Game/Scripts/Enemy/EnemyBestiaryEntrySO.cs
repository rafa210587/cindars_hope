using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [CreateAssetMenu(fileName = "BestiaryEntry_", menuName = "CindarsHope/Enemy/Enemy Bestiary Entry")]
    public class EnemyBestiaryEntrySO : ScriptableObject, IIdentifiedData
    {
        public string BestiaryEntryId;
        public string EnemyId;
        public string DisplayName;
        [TextArea(2, 4)] public string ShortDescription;
        [TextArea(1, 3)] public string HabitatText;
        [TextArea(1, 3)] public string BehaviorHint;
        [TextArea(1, 3)] public string VulnerabilityHintLocked;
        [TextArea(1, 3)] public string VulnerabilityHintDiscovered;
        [TextArea(1, 3)] public string KnownDropsHint;
        public string FactionText;
        public BestiaryFirstSeenUnlockMode FirstSeenUnlockMode = BestiaryFirstSeenUnlockMode.SeenOrSpawned;

        string IIdentifiedData.Id => BestiaryEntryId;
    }

    public enum BestiaryFirstSeenUnlockMode
    {
        SeenOrSpawned,
        Damaged,
        Killed
    }
}
