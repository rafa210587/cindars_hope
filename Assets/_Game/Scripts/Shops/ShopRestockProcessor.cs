using System;

namespace CindarsHope.Shops
{
    public class ShopRestockProcessor
    {
        // Process restock at day start — never on menu open
        public void ProcessDayStart(ShopInventoryDefinition def, ShopStockState state, int currentDay)
        {
            if (!ShouldRestock(def.RestockPolicy, state, currentDay)) return;

            var rng = new Random(state.RotatingStockSeed + currentDay);

            foreach (var line in def.AllStockLines())
            {
                if (!state.StockLineStates.TryGetValue(line.StockLineId, out var lineState))
                {
                    lineState = new StockLineState { StockLineId = line.StockLineId, CurrentQuantity = 0 };
                    state.StockLineStates[line.StockLineId] = lineState;
                }

                // UniqueStock: never restock
                if (line.IsUniqueStock) continue;

                // LimitedStock: restock only if lifetime limit allows
                if (line.StockType == StockType.LimitedStock)
                {
                    if (line.PurchaseLimitLifetime > 0 && lineState.LifetimePurchaseCount >= line.PurchaseLimitLifetime) continue;
                }

                // Rotating stock: seed-based selection per day
                if (line.StockType == StockType.RotatingStock)
                {
                    float roll = (float)rng.NextDouble();
                    if (roll > line.RestockChance) continue;
                }

                // Reset daily counters
                lineState.DayPurchaseCount = 0;

                // Apply restock quantity
                int restockQty = line.RestockToQuantity;
                if (line.RestockVariance > 0)
                    restockQty += rng.Next(-line.RestockVariance, line.RestockVariance + 1);
                restockQty = Math.Max(line.MinQuantity, Math.Min(line.MaxQuantity, restockQty));
                lineState.CurrentQuantity = restockQty;
            }

            state.LastRestockDay = currentDay;
            state.NextRestockDay = currentDay + (def.RestockPolicy?.IntervalDays ?? 1);
        }

        private bool ShouldRestock(ShopRestockPolicyDefinition policy, ShopStockState state, int currentDay)
        {
            if (policy == null) return false;
            return policy.PolicyType switch
            {
                RestockPolicyType.Never => false,
                RestockPolicyType.DailyMorning => currentDay > state.LastRestockDay,
                RestockPolicyType.Weekly => currentDay >= state.NextRestockDay,
                _ => currentDay >= state.NextRestockDay
            };
        }
    }
}
