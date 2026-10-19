using CindarsHope.Core.Data;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Farm.Data
{
    public enum GrowthPeriod
    {
        Day,
        Night,
        Both
    }

    [CreateAssetMenu(fileName = "SeedData", menuName = "CindarsHope/Data/Seed")]
    public class SeedDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public ItemDataSO SeedItem;
        public Sprite[] GrowthStageSprites;
        public ItemDataSO[] HarvestItems;
        public int[] HarvestAmounts;
        public int GrowthDays = 1;
        public bool RequiresWater = true;
        public int RegrowDays;
        public string[] SeasonTags;
        public GrowthPeriod Period = GrowthPeriod.Both;
        public int MinYield = 1;
        public int MaxYield = 1;
        public float FertilizerYieldMultiplier = 1f;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            GrowthDays = Mathf.Max(1, GrowthDays);
            RegrowDays = Mathf.Max(0, RegrowDays);
            MinYield = Mathf.Max(1, MinYield);
            MaxYield = Mathf.Max(MinYield, MaxYield);
            FertilizerYieldMultiplier = Mathf.Max(1f, FertilizerYieldMultiplier);
        }
    }
}
