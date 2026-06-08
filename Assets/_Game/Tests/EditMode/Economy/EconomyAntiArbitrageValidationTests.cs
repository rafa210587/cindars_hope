using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Economy.Pricing;
using CindarsHope.Economy.Validation;
using CindarsHope.Loot;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class EconomyAntiArbitrageValidationTests
    {
        private EconomyPricingService _pricing;
        private EconomyBalanceValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _pricing = new EconomyPricingService(PricingProfile.Default());
            _validator = new EconomyBalanceValidator(_pricing);
        }

        [Test]
        public void BuySell_NormalItem_NoViolation()
        {
            var report = new EconomyValidationReport();
            _validator.ValidateBuySell("item_carrot", 100, report);
            Assert.AreEqual(0, report.Errors.Count, string.Join(", ", report.Errors));
        }

        [Test]
        public void BuySell_InvertedPrices_DetectsViolation()
        {
            // Force buy < sell by custom profile
            var profile = PricingProfile.Default();
            profile.ChannelSellMultipliers[PriceChannel.ShopSellToPlayer] = 0.5f;  // buy from shop = cheap
            profile.ChannelSellMultipliers[PriceChannel.GenericShopBuyFromPlayer] = 0.9f; // shop buys = high
            var svc = new EconomyPricingService(profile);
            var val = new EconomyBalanceValidator(svc);
            var report = new EconomyValidationReport();
            val.ValidateBuySell("item_carrot", 100, report, isLimitedException: false);
            Assert.Greater(report.Errors.Count, 0, "Should detect inverted buy/sell prices");
        }

        [Test]
        public void RestockPolicy_UniqueStock_WithNonUniqueRestock_Warning()
        {
            var c = new AntiArbitrageCase
            {
                ItemId = "item_unique_sword",
                StockLimit = 1,
                RestockPolicy = RestockPolicy.Daily,
                IsBoundedException = false
            };
            var report = new EconomyValidationReport();
            _validator.ValidateRestockPolicy(c, report);
            Assert.Greater(report.Warnings.Count, 0);
        }

        [Test]
        public void RestockPolicy_UniqueStock_UniquePolicy_NoWarning()
        {
            var c = new AntiArbitrageCase
            {
                ItemId = "item_unique_sword",
                StockLimit = 1,
                RestockPolicy = RestockPolicy.Unique
            };
            var report = new EconomyValidationReport();
            _validator.ValidateRestockPolicy(c, report);
            Assert.AreEqual(0, report.Warnings.Count);
        }

        [Test]
        public void Processing_ProtectedItem_NoGate_Error()
        {
            var recipe = new ProcessingRecipeCheck
            {
                RecipeId = "recipe_mana_extract",
                InputBaseValue = 100,
                OutputBaseValue = 200,
                InputIsProtected = true,
                HasExplicitAuthoring = false,
                RequiredTimeTicks = 10,
                RequiresStation = true
            };
            var report = new EconomyValidationReport();
            _validator.ValidateProcessingRecipe(recipe, report);
            Assert.Greater(report.Errors.Count, 0);
        }

        [Test]
        public void Processing_HighMultiplier_NoTime_Warning()
        {
            var recipe = new ProcessingRecipeCheck
            {
                RecipeId = "recipe_instant_profit",
                InputBaseValue = 10,
                OutputBaseValue = 50,  // x5
                RequiredTimeTicks = 0,
                RequiresStation = false,
                RequiresCapacity = false
            };
            var report = new EconomyValidationReport();
            _validator.ValidateProcessingRecipe(recipe, report);
            Assert.Greater(report.Warnings.Count, 0);
        }

        [Test]
        public void LootEntry_ProtectedItem_Repeatable_Error()
        {
            var entry = new LootEntry { ItemId = "item_fruto_mana", Repeatable = true, IsLoreReward = false };
            var report = new EconomyValidationReport();
            _validator.ValidateLootEntry(entry, "table_common", report);
            Assert.Greater(report.Errors.Count, 0);
        }

        [Test]
        public void LootEntry_NormalItem_Repeatable_NoError()
        {
            var entry = new LootEntry { ItemId = "item_bone", Repeatable = true };
            var report = new EconomyValidationReport();
            _validator.ValidateLootEntry(entry, "table_cave", report);
            Assert.AreEqual(0, report.Errors.Count);
        }

        [Test]
        public void GoldHour_Exceeds_Max_Warning()
        {
            var budget = new GoldHourBudget { Source = "cave_boss_loop", EstimatedGoldPerHour = 10000f, MaxSafeGoldPerHour = 5000f };
            var report = new EconomyValidationReport();
            _validator.ValidateGoldHour(budget, report);
            Assert.Greater(report.Warnings.Count, 0);
        }

        [Test]
        public void BossReward_FirstTimeNotMarked_Error()
        {
            var firstTime = new LootEntry { ItemId = "item_boss_trophy", FirstTimeOnly = false };
            var repeat = new LootEntry { ItemId = "item_bone", IsUniqueReward = false };
            var report = new EconomyValidationReport();
            _validator.ValidateBossReward(firstTime, repeat, "boss_swamp_queen", report);
            Assert.Greater(report.Errors.Count, 0);
        }

        [Test]
        public void Report_IsValid_WhenNoErrors()
        {
            var report = new EconomyValidationReport();
            Assert.IsTrue(report.IsValid);
        }

        [Test]
        public void AntiArbitrageCase_IsArbitrage_WhenSellGeqBuy()
        {
            var c = new AntiArbitrageCase { ItemId = "item_test", BuyPrice = 100, SellPrice = 110, IsBoundedException = false };
            Assert.IsTrue(c.IsArbitrage);
        }
    }
}
