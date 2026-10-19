using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnPack_", menuName = "CindarsHope/Enemy/Spawn Pack")]
    public class EnemySpawnPackSO : ScriptableObject, IIdentifiedData
    {
        public string PackId;
        public string DisplayName;
        public int CaveLevelMin = 1;
        public int CaveLevelMax = 10;
        public string[] BiomeTags = new string[0];
        public string[] EnvironmentTags = new string[0];
        public string[] RequiredFactionIds = new string[0];
        public EnemySpawnPackEntry[] Entries = new EnemySpawnPackEntry[0];
        public int Weight = 1;
        public EnemyRoomSizeClass MinimumRoomSize = EnemyRoomSizeClass.Small;
        public int MaxTotalEnemies = 4;
        public bool IsEnabled = true;

        string IIdentifiedData.Id => PackId;

        private void OnValidate()
        {
            CaveLevelMin = Mathf.Max(1, CaveLevelMin);
            CaveLevelMax = Mathf.Max(CaveLevelMin, CaveLevelMax);
            Weight = Mathf.Max(0, Weight);
            MaxTotalEnemies = Mathf.Max(1, MaxTotalEnemies);
        }
    }

    [System.Serializable]
    public class EnemySpawnPackEntry
    {
        public string EnemyId;
        public int MinCount = 1;
        public int MaxCount = 1;
        public int Weight = 1;
        public bool IsRequired = true;
        public string RequiresUnlockedFactionLock;

        public void Normalize()
        {
            MinCount = Mathf.Max(0, MinCount);
            MaxCount = Mathf.Max(MinCount, MaxCount);
            Weight = Mathf.Max(0, Weight);
        }
    }
}
