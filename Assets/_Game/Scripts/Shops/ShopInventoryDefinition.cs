using System.Collections.Generic;

namespace CindarsHope.Shops
{
    public class ShopRestockPolicyDefinition
    {
        public string RestockPolicyId { get; set; }
        public RestockPolicyType PolicyType { get; set; } = RestockPolicyType.DailyMorning;
        public int IntervalDays { get; set; } = 1;
        public bool RestockAtDayStart { get; set; } = true;
        public bool RestockBeforeShopOpen { get; set; } = true;
        public bool UsesPersistentRandomSeed { get; set; } = true;
    }

    public class ShopInventoryDefinition
    {
        public string ShopId { get; set; }
        public string NpcOwnerId { get; set; }
        public List<StockLineDefinition> BaseStockLines { get; set; } = new List<StockLineDefinition>();
        public List<StockLineDefinition> SeasonalStockLines { get; set; } = new List<StockLineDefinition>();
        public List<StockLineDefinition> ReputationStockLines { get; set; } = new List<StockLineDefinition>();
        public List<StockLineDefinition> QuestUnlockedStockLines { get; set; } = new List<StockLineDefinition>();
        public List<StockLineDefinition> CaveProgressStockLines { get; set; } = new List<StockLineDefinition>();
        public List<StockLineDefinition> FarmLevelStockLines { get; set; } = new List<StockLineDefinition>();
        public List<StockLineDefinition> LimitedStockLines { get; set; } = new List<StockLineDefinition>();
        public List<StockLineDefinition> RotatingStockLines { get; set; } = new List<StockLineDefinition>();
        public string PlayerSoldStockPolicyId { get; set; }
        public ShopRestockPolicyDefinition RestockPolicy { get; set; } = new ShopRestockPolicyDefinition();
        public List<string> ForbiddenItems { get; set; } = new List<string>();
        public List<string> DebugTags { get; set; } = new List<string>();

        public IEnumerable<StockLineDefinition> AllStockLines()
        {
            foreach (var l in BaseStockLines) yield return l;
            foreach (var l in SeasonalStockLines) yield return l;
            foreach (var l in ReputationStockLines) yield return l;
            foreach (var l in QuestUnlockedStockLines) yield return l;
            foreach (var l in CaveProgressStockLines) yield return l;
            foreach (var l in FarmLevelStockLines) yield return l;
            foreach (var l in LimitedStockLines) yield return l;
            foreach (var l in RotatingStockLines) yield return l;
        }
    }
}
