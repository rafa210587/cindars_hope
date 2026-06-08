using System.Collections.Generic;

namespace CindarsHope.Shops
{
    public class StockLineState
    {
        public string StockLineId { get; set; }
        public int CurrentQuantity { get; set; }
        public bool IsPurchased { get; set; } = false;
        public int DayPurchaseCount { get; set; } = 0;
        public int WeekPurchaseCount { get; set; } = 0;
        public int LifetimePurchaseCount { get; set; } = 0;
    }

    public class ShopStockState
    {
        public string ShopId { get; set; }
        public Dictionary<string, StockLineState> StockLineStates { get; set; } = new Dictionary<string, StockLineState>();
        public HashSet<string> UniqueStockPurchasedFlags { get; set; } = new HashSet<string>();
        public Dictionary<string, int> LimitedStockCounters { get; set; } = new Dictionary<string, int>();
        public int LastRestockDay { get; set; } = -1;
        public int NextRestockDay { get; set; } = -1;
        public int RotatingStockSeed { get; set; } = 0;
        public List<string> RotatingStockSelection { get; set; } = new List<string>();

        public bool TryPurchase(string stockLineId, StockLineDefinition def, out string failReason)
        {
            failReason = null;
            if (def.IsUniqueStock && UniqueStockPurchasedFlags.Contains(stockLineId))
            {
                failReason = "UniqueStock already purchased";
                return false;
            }
            if (!StockLineStates.TryGetValue(stockLineId, out var state))
            {
                failReason = "StockLine not found in state";
                return false;
            }
            if (state.CurrentQuantity <= 0)
            {
                failReason = "Out of stock";
                return false;
            }
            if (def.PurchaseLimitPerDay > 0 && state.DayPurchaseCount >= def.PurchaseLimitPerDay)
            {
                failReason = "Daily purchase limit reached";
                return false;
            }
            if (def.PurchaseLimitLifetime > 0 && state.LifetimePurchaseCount >= def.PurchaseLimitLifetime)
            {
                failReason = "Lifetime purchase limit reached";
                return false;
            }

            state.CurrentQuantity--;
            state.DayPurchaseCount++;
            state.WeekPurchaseCount++;
            state.LifetimePurchaseCount++;
            if (def.IsUniqueStock) UniqueStockPurchasedFlags.Add(stockLineId);
            return true;
        }
    }
}
