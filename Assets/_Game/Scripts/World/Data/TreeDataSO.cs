using CindarsHope.Core.Data;
using CindarsHope.Loot;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.World.Data
{
    [CreateAssetMenu(fileName = "TreeData", menuName = "CindarsHope/Data/Tree")]
    public class TreeDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        public string WoodItemId = "item_wood";
        public int WoodAmount = 2;
        public int RequiredHits = 3;
        public int MaxHp = 3;
        public ToolType RequiredToolType = ToolType.Axe;
        public ToolTier MinimumToolTier = ToolTier.Basic;
        public int WoodPerHitMin = 1;
        public int WoodPerHitMax = 1;
        public int FinalHitMultiplier = 2;
        public int RegrowthDays = 3;
        public LootTableSO LootTable;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            WoodAmount = Mathf.Max(1, WoodAmount);
            RequiredHits = Mathf.Max(1, RequiredHits);
            MaxHp = Mathf.Max(1, MaxHp);
            WoodPerHitMin = Mathf.Max(1, WoodPerHitMin);
            WoodPerHitMax = Mathf.Max(WoodPerHitMin, WoodPerHitMax);
            FinalHitMultiplier = Mathf.Max(2, FinalHitMultiplier);
            RegrowthDays = Mathf.Max(0, RegrowthDays);

            if (string.IsNullOrWhiteSpace(Id))
            {
                Debug.LogWarning($"{nameof(TreeDataSO)} '{name}' has an empty Id.", this);
            }
        }
    }
}
