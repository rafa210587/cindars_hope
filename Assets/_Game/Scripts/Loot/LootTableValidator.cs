using System.Collections.Generic;

namespace CindarsHope.Loot
{
    // Protected item IDs that must not appear as common repeat loot
    public static class ProtectedItemIds
    {
        public static readonly HashSet<string> Ids = new HashSet<string>
        {
            "item_fruto_mana",
            "item_agua_viva",
            "item_pedra_negra_estabilizada",
            "item_blackstone_corrupted_shard"
        };

        public static bool IsProtected(string itemId) =>
            !string.IsNullOrEmpty(itemId) && Ids.Contains(itemId);
    }

    public class LootTableValidator
    {
        public static List<string> Validate(LootTableDefinition def)
        {
            var errors = new List<string>();
            if (def == null) { errors.Add("LootTableDefinition is null"); return errors; }
            if (string.IsNullOrEmpty(def.LootTableId)) errors.Add("LootTableId is required");
            if (def.SourceType == LootSourceType.Unknown) errors.Add($"{def.LootTableId}: SourceType is Unknown");

            ValidateEntries(def.LootTableId, def.GuaranteedDrops, errors, allowProtected: true);
            ValidateEntries(def.LootTableId, def.WeightedDrops, errors, allowProtected: false);
            ValidateEntries(def.LootTableId, def.RareDrops, errors, allowProtected: false);
            ValidateUniqueEntries(def.LootTableId, def.UniqueDrops, errors);
            ValidateProgressionCritical(def.LootTableId, def.RareDrops, def.PityRules, errors);
            return errors;
        }

        private static void ValidateEntries(string tableId, List<LootEntry> entries, List<string> errors, bool allowProtected)
        {
            if (entries == null) return;
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (string.IsNullOrEmpty(e.ItemId)) errors.Add($"{tableId}: entry missing ItemId");
                if (e.QuantityMin > e.QuantityMax) errors.Add($"{tableId}/{e.ItemId}: QuantityMin > QuantityMax");
                if (!allowProtected && e.Repeatable && ProtectedItemIds.IsProtected(e.ItemId))
                    errors.Add($"{tableId}/{e.ItemId}: protected item must not appear as common repeat loot");
            }
        }

        private static void ValidateUniqueEntries(string tableId, List<LootEntry> entries, List<string> errors)
        {
            if (entries == null) return;
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (!e.IsUniqueReward && !e.FirstTimeOnly)
                    errors.Add($"{tableId}/{e.ItemId}: UniqueDrops entry must have IsUniqueReward=true or FirstTimeOnly=true");
                if (e.Repeatable)
                    errors.Add($"{tableId}/{e.ItemId}: UniqueDrops entry must not be Repeatable");
            }
        }

        private static void ValidateProgressionCritical(string tableId, List<LootEntry> rareDrops, PityRules pity, List<string> errors)
        {
            if (rareDrops == null) return;
            foreach (var e in rareDrops)
            {
                if (e == null || !e.IsProgressionCritical) continue;
                bool hasSafety = pity != null && pity.Enabled &&
                    (!string.IsNullOrEmpty(pity.PityGrantItemId) ||
                     !string.IsNullOrEmpty(pity.AlternativeRecipeId) ||
                     !string.IsNullOrEmpty(pity.LateFallbackQuestId));
                if (!hasSafety)
                    errors.Add($"{tableId}/{e.ItemId}: IsProgressionCritical=true but no PityRules safety net defined");
            }
        }
    }
}
