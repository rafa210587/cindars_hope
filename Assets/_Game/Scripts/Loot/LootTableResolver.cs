using System;
using System.Collections.Generic;

namespace CindarsHope.Loot
{
    public class LootRollContext
    {
        public string TableId { get; set; }
        public int Seed { get; set; }
        public HashSet<string> GrantedFirstTimeFlags { get; set; } = new HashSet<string>();
        public HashSet<string> GrantedUniqueItemIds { get; set; } = new HashSet<string>();
        public Dictionary<string, int> DailyGrantCounts { get; set; } = new Dictionary<string, int>();
        public int CurrentDay { get; set; } = 0;
    }

    public class LootResolveResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public List<string> ItemIds { get; set; } = new List<string>();
        public List<int> Quantities { get; set; } = new List<int>();
        public int GoldGrant { get; set; }
        public bool FirstTimeBonusApplied { get; set; }
        public List<string> DebugLog { get; set; } = new List<string>();

        public static LootResolveResult Fail(string reason) =>
            new LootResolveResult { Success = false, FailureReason = reason };
    }

    // Deterministic resolver using seeded random — used for cave snapshots and stable run
    public class LootTableResolver
    {
        public LootResolveResult Resolve(LootTableDefinition table, LootRollContext context)
        {
            if (table == null) return LootResolveResult.Fail("table is null");
            var errors = LootTableValidator.Validate(table);
            if (errors.Count > 0) return LootResolveResult.Fail($"validation failed: {string.Join("; ", errors)}");

            var result = new LootResolveResult { Success = true };
            var rng = new Random(context.Seed);

            // Guaranteed drops always apply
            foreach (var e in table.GuaranteedDrops)
                ApplyEntry(e, result, context, rng);

            // Weighted drops
            ResolveWeighted(table.WeightedDrops, result, context, rng);

            // Rare drops (per-entry chance)
            foreach (var e in table.RareDrops)
            {
                if ((float)rng.NextDouble() <= e.DropChance)
                    ApplyEntry(e, result, context, rng);
            }

            // Unique drops (first-time or authored)
            foreach (var e in table.UniqueDrops)
            {
                if (context.GrantedUniqueItemIds.Contains(e.ItemId)) continue;
                ApplyEntry(e, result, context, rng);
                context.GrantedUniqueItemIds.Add(e.ItemId);
            }

            // Gold
            if (!table.GoldRange.IsEmpty)
                result.GoldGrant = rng.Next(table.GoldRange.Min, table.GoldRange.Max + 1);

            // First time bonus
            if (table.FirstTimeBonus != null && !table.FirstTimeBonus.IsConsumed)
            {
                result.ItemIds.AddRange(table.FirstTimeBonus.BonusItemIds);
                result.GoldGrant += table.FirstTimeBonus.BonusGold;
                result.FirstTimeBonusApplied = true;
                table.FirstTimeBonus.IsConsumed = true;
            }

            return result;
        }

        private void ApplyEntry(LootEntry e, LootResolveResult result, LootRollContext context, Random rng)
        {
            if (string.IsNullOrEmpty(e.ItemId)) return;
            if (!string.IsNullOrEmpty(e.RequiredFlag) && !context.GrantedFirstTimeFlags.Contains(e.RequiredFlag)) return;
            if (!string.IsNullOrEmpty(e.ForbiddenFlag) && context.GrantedFirstTimeFlags.Contains(e.ForbiddenFlag)) return;

            // Daily cap check
            if (context.DailyGrantCounts.TryGetValue(e.ItemId, out int count))
            {
                // RepeatFarmRules daily cap is checked by caller/table-level; we just record
            }

            int qty = rng.Next(e.QuantityMin, e.QuantityMax + 1);
            result.ItemIds.Add(e.ItemId);
            result.Quantities.Add(qty);
        }

        private void ResolveWeighted(List<LootEntry> entries, LootResolveResult result, LootRollContext context, Random rng)
        {
            if (entries == null || entries.Count == 0) return;
            int total = 0;
            foreach (var e in entries) if (e.Weight > 0) total += e.Weight;
            if (total <= 0) return;

            int roll = rng.Next(1, total + 1);
            int cursor = 0;
            foreach (var e in entries)
            {
                if (e.Weight <= 0) continue;
                cursor += e.Weight;
                if (roll <= cursor)
                {
                    ApplyEntry(e, result, context, rng);
                    break;
                }
            }
        }
    }
}
