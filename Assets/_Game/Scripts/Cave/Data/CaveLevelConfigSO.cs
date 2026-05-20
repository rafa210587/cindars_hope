using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveLevelConfig", menuName = "CindarsHope/Cave/Level Config")]
    public sealed class CaveLevelConfigSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public int CaveLevel = 1;
        public string BiomeId = "biome_cave_earth";
        public int MinEnemyCount = 1;
        public int MaxEnemyCount = 4;
        public int MinResourceNodeCount = 3;
        public int MaxResourceNodeCount = 8;
        public bool HasCheckpoint;
        public bool HasBoss;
        public string BossEnemyId;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            CaveLevel = Mathf.Max(1, CaveLevel);
            MinEnemyCount = Mathf.Max(0, MinEnemyCount);
            MaxEnemyCount = Mathf.Max(MinEnemyCount, MaxEnemyCount);
            MinResourceNodeCount = Mathf.Max(0, MinResourceNodeCount);
            MaxResourceNodeCount = Mathf.Max(MinResourceNodeCount, MaxResourceNodeCount);
        }
    }
}
