using CindarsHope.Combat;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnProfile_", menuName = "CindarsHope/Enemy/Spawn Profile")]
    public class EnemySpawnProfileSO : ScriptableObject, IIdentifiedData
    {
        public string SpawnProfileId;
        public string DisplayName;
        public string EnemyId;
        public int CaveLevelMin = 1;
        public int CaveLevelMax = 10;
        public string[] BiomeTags = new string[0];
        public string[] EnvironmentTags = new string[0];
        public string FactionId;
        public string FactionLockId;
        public string RequiredBossGateProgress;
        public int Weight = 1;
        public int MaxCountPerRoom = 1;
        public bool CanSpawnAsElite;
        public EnemyRoomSizeClass MinimumRoomSizeForSizeClass = EnemyRoomSizeClass.Small;
        public EnemySizeClass SizeClass = EnemySizeClass.Medium;
        public string[] AllowedRoomTags = new string[0];
        public string[] DeniedRoomTags = new string[0];
        public string[] PackIds = new string[0];
        public bool IsEnabled = true;

        string IIdentifiedData.Id => SpawnProfileId;

        private void OnValidate()
        {
            CaveLevelMin = Mathf.Max(1, CaveLevelMin);
            CaveLevelMax = Mathf.Max(CaveLevelMin, CaveLevelMax);
            Weight = Mathf.Max(0, Weight);
            MaxCountPerRoom = Mathf.Max(1, MaxCountPerRoom);

            if (string.IsNullOrWhiteSpace(SpawnProfileId) && !string.IsNullOrWhiteSpace(EnemyId))
            {
                SpawnProfileId = $"spawn_{EnemyId}";
            }
        }
    }
}
