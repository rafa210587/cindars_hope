using System.Collections.Generic;
using CindarsHope.Economy.Pricing;
using CindarsHope.Loot;
using CindarsHope.Economy;

namespace CindarsHope.Economy.Validation
{
    public class ProcessingRecipeCheck
    {
        public string RecipeId { get; set; }
        public int InputBaseValue { get; set; }
        public int OutputBaseValue { get; set; }
        public int RequiredTimeTicks { get; set; }
        public bool RequiresStation { get; set; }
        public bool RequiresCapacity { get; set; }
        public bool InputIsProtected { get; set; }
        public bool HasExplicitAuthoring { get; set; }
    }

    public class GoldHourBudget
    {
        public string Source { get; set; }
        public float EstimatedGoldPerHour { get; set; }
        public float MaxSafeGoldPerHour { get; set; } = 800f;
        public bool ExceedsMax => EstimatedGoldPerHour > MaxSafeGoldPerHour;
    }

    public class EconomyBalanceValidator
    {
        private readonly EconomyPricingService _pricing;
        private readonly EconomyBalanceConfigSO _config;

        public EconomyBalanceValidator(EconomyPricingService pricing,
                                       EconomyBalanceConfigSO config = null)
        {
            _pricing = pricing;
            _config  = config;
        }

        // 8.1 Buy/sell invariant: ShopSellToPlayer > ShopBuyFromPlayer
        public void ValidateBuySell(
            string itemId, int baseValue, EconomyValidationReport report, bool isLimitedException = false)
        {
            var buyReq = new PriceRequest { ItemId = itemId, BaseValue = baseValue, Channel = PriceChannel.ShopSellToPlayer };
            var sellReq = new PriceRequest { ItemId = itemId, BaseValue = baseValue, Channel = PriceChannel.GenericShopBuyFromPlayer };
            var buyR = _pricing.CalculatePrice(buyReq);
            var sellR = _pricing.CalculatePrice(sellReq);
            if (!buyR.Success || !sellR.Success) return;
            if (!isLimitedException && buyR.UnitPrice <= sellR.UnitPrice)
                report.Errors.Add($"BUY_SELL_INVARIANT: {itemId} ShopSellToPlayer={buyR.UnitPrice} <= ShopBuyFromPlayer={sellR.UnitPrice}");
        }

        // 8.2 Restock exploit: UniqueStock must not be restockable
        public void ValidateRestockPolicy(
            AntiArbitrageCase c, EconomyValidationReport report)
        {
            if (c.StockLimit == 1 && c.RestockPolicy != RestockPolicy.None && c.RestockPolicy != RestockPolicy.Unique)
                report.Warnings.Add($"RESTOCK_EXPLOIT: {c.ItemId} has StockLimit=1 but RestockPolicy={c.RestockPolicy} (should be Unique or None for truly unique stock)");
        }

        // 8.3 Processing: output value must require time/station
        public void ValidateProcessingRecipe(ProcessingRecipeCheck recipe, EconomyValidationReport report)
        {
            if (recipe.InputIsProtected && !recipe.HasExplicitAuthoring)
            {
                report.Errors.Add($"PROCESSING_PROTECTED: recipe {recipe.RecipeId} consumes protected item without explicit gate");
                return;
            }
            if (recipe.OutputBaseValue > recipe.InputBaseValue * 3)
            {
                if (recipe.RequiredTimeTicks <= 0 || (!recipe.RequiresStation && !recipe.RequiresCapacity))
                    report.Warnings.Add($"PROCESSING_LOOP_RISK: recipe {recipe.RecipeId} multiplies value x{recipe.OutputBaseValue / (float)recipe.InputBaseValue:F1} without time/station constraint");
            }
        }

        // 8.4 Loot protected items
        public void ValidateLootEntry(LootEntry entry, string tableId, EconomyValidationReport report)
        {
            var errors = new List<string>();
            if (ProtectedItemIds.IsProtected(entry.ItemId) && entry.Repeatable && !entry.IsLoreReward)
                report.Errors.Add($"LOOT_PROTECTED: {tableId}/{entry.ItemId} protected item as repeat loot");
            if (entry.IsProgressionCritical && entry.DropChance < 0.1f && entry.Repeatable)
                report.Warnings.Add($"LOOT_PITY_NEEDED: {tableId}/{entry.ItemId} progression-critical at {entry.DropChance:P0} — ensure pity or alternative exists");
        }

        // 8.5 Gold/hour budget — ceiling from EconomyBalanceConfigSO when available
        public void ValidateGoldHour(GoldHourBudget budget, EconomyValidationReport report)
        {
            float ceiling = _config != null ? _config.MaxSafeGoldPerHour : budget.MaxSafeGoldPerHour;
            if (budget.EstimatedGoldPerHour > ceiling)
                report.Warnings.Add($"GOLD_HOUR_BUDGET: {budget.Source} estimated={budget.EstimatedGoldPerHour:F0} exceeds max={ceiling:F0}");
        }

        // 8.6 Reward: boss first-time reward must differ from repeat
        public void ValidateBossReward(
            LootEntry firstTimeEntry, LootEntry repeatEntry, string bossId, EconomyValidationReport report)
        {
            if (firstTimeEntry == null || repeatEntry == null) return;
            if (!firstTimeEntry.FirstTimeOnly)
                report.Errors.Add($"BOSS_FIRST_TIME: {bossId} first-time entry is not marked FirstTimeOnly");
            if (repeatEntry.IsUniqueReward)
                report.Errors.Add($"BOSS_REPEAT: {bossId} repeat entry marked IsUniqueReward — should not be unique");
        }

        public EconomyValidationReport RunAll(
            List<(string id, int baseValue, bool isException)> buySellItems,
            List<AntiArbitrageCase> arbitrageCases,
            List<ProcessingRecipeCheck> recipes,
            List<(LootEntry entry, string tableId)> lootEntries,
            List<GoldHourBudget> goldBudgets)
        {
            var report = new EconomyValidationReport();

            foreach (var (id, bv, exc) in buySellItems)
            {
                ValidateBuySell(id, bv, report, exc);
                report.RulesRun++;
            }
            foreach (var c in arbitrageCases)
            {
                ValidateRestockPolicy(c, report);
                report.RulesRun++;
            }
            foreach (var r in recipes)
            {
                ValidateProcessingRecipe(r, report);
                report.RulesRun++;
            }
            foreach (var (e, t) in lootEntries)
            {
                ValidateLootEntry(e, t, report);
                report.RulesRun++;
            }
            foreach (var b in goldBudgets)
            {
                ValidateGoldHour(b, report);
                report.RulesRun++;
            }

            report.Passed = report.RulesRun - report.Errors.Count - report.Blockers.Count;
            return report;
        }
    }
}
