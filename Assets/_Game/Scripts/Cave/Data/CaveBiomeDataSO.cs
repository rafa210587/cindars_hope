using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveBiomeData", menuName = "CindarsHope/Cave/Biome Data")]
    public sealed class CaveBiomeDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        public int MinLevel;
        public int MaxLevel;
        public string[] AllowedEnemyIds;
        public string[] AllowedResourceNodeIds;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            MinLevel = Mathf.Max(1, MinLevel);
            MaxLevel = Mathf.Max(MinLevel, MaxLevel);
        }
    }
}
