using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [CreateAssetMenu(fileName = "EnemyFactionLock_", menuName = "CindarsHope/Enemy/Faction Lock")]
    public class EnemyFactionLockSO : ScriptableObject, IIdentifiedData
    {
        public string FactionLockId;
        public string DisplayName;
        public string RequiredBossGateId;
        public string RequiredStoryFlagId;
        public int RequiredCaveLevelMin;
        public string[] UnlocksFactionIds = new string[0];
        public string[] UnlocksPackIds = new string[0];
        public bool IsUnlockedByDefault;

        string IIdentifiedData.Id => FactionLockId;

        private void OnValidate()
        {
            RequiredCaveLevelMin = Mathf.Max(0, RequiredCaveLevelMin);
        }
    }
}
