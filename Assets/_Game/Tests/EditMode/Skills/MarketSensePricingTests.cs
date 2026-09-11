using CindarsHope.Economy;
using CindarsHope.Economy.Pricing;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class MarketSensePricingTests
    {
        [TearDown]
        public void TearDown() => MarketSkillModifierProvider.MultiplierSource = null;

        [Test]
        public void ExclusiveVariant_AppliesOnlyToChosenDirection()
        {
            Assert.That(MarketSkillModifierProvider.Resolve(3, "buy", MarketPriceDirection.BuyFromShop),
                Is.EqualTo(.85f).Within(.0001f));
            Assert.That(MarketSkillModifierProvider.Resolve(3, "buy", MarketPriceDirection.SellToShop), Is.EqualTo(1f));
            Assert.That(MarketSkillModifierProvider.Resolve(3, "sell", MarketPriceDirection.SellToShop),
                Is.EqualTo(1.15f).Within(.0001f));
            Assert.That(MarketSkillModifierProvider.Resolve(3, "sell", MarketPriceDirection.BuyFromShop), Is.EqualTo(1f));
            Assert.That(MarketSkillModifierProvider.Resolve(99, "buy", MarketPriceDirection.BuyFromShop),
                Is.EqualTo(.85f).Within(.0001f), "benefit is capped at 15%");
        }

        [Test]
        public void SellPoint_IsExcludedFromMarketSense()
        {
            MarketSkillModifierProvider.MultiplierSource = _ => 2f;
            var result = new EconomyPricingService().CalculatePrice(new PriceRequest
            {
                ItemId = "crop",
                BaseValue = 100,
                Channel = PriceChannel.SellPoint
            });

            Assert.That(result.Success, Is.True);
            Assert.That(result.UnitPrice, Is.EqualTo(90));
        }

        [Test]
        public void CrossRespec_SaveRoundTripAndExistingShopEntry_CannotBuyLowAndSellHigh()
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            var shop = ScriptableObject.CreateInstance<ShopDataSO>();
            var entry = new ShopItemEntry
            {
                ItemId = "material",
                BuyPriceOverride = 100,
                BaseDailyStock = 1
            };

            item.Id = entry.ItemId;
            item.BaseValue = 100;
            shop.Id = "existing_shop";
            shop.BuyPriceMultiplier = 1f;
            shop.SellPriceMultiplier = .6f;
            shop.Items = new[] { entry };

            var purchasedState = BuildRankThreeState(MarketSkillModifierProvider.BuyVariantId);
            var buySave = purchasedState.ToSaveData(slot => $"slot_{slot}");
            var restoredState = new SkillTreeState();
            restoredState.LoadFromSaveData(buySave, totalAvailablePoints: 3, _ => "crafting");

            MarketSkillModifierProvider.MultiplierSource = direction =>
                MarketSkillModifierProvider.Resolve(
                    restoredState.GetRank(MarketSkillModifierProvider.NodeId),
                    restoredState.GetChosenVariant(MarketSkillModifierProvider.NodeId),
                    direction);

            try
            {
                int boughtFor = ShopManager.CalculateBuyPrice(item, entry, shop);

                restoredState.FullRespec(restoredPoints: 3);
                restoredState.Purchase(MarketSkillModifierProvider.NodeId, 1, "crafting");
                restoredState.RankUp(MarketSkillModifierProvider.NodeId, "crafting");
                restoredState.RankUp(MarketSkillModifierProvider.NodeId, "crafting");
                restoredState.SetChosenVariant(
                    MarketSkillModifierProvider.NodeId,
                    MarketSkillModifierProvider.SellVariantId);

                var sellSave = restoredState.ToSaveData(slot => $"slot_{slot}");
                var postRespecState = new SkillTreeState();
                postRespecState.LoadFromSaveData(sellSave, totalAvailablePoints: 3, _ => "crafting");
                restoredState = postRespecState;

                int soldFor = ShopManager.CalculateSellPrice(item, shop);

                Assert.That(restoredState.RespecCount, Is.EqualTo(1));
                Assert.That(restoredState.GetRank(MarketSkillModifierProvider.NodeId), Is.EqualTo(3));
                Assert.That(restoredState.GetChosenVariant(MarketSkillModifierProvider.NodeId),
                    Is.EqualTo(MarketSkillModifierProvider.SellVariantId));
                Assert.That(shop.GetEntry(item.Id), Is.SameAs(entry), "The original shop listing remains in use.");
                Assert.That(soldFor, Is.LessThan(boughtFor),
                    "Buying with the buy focus and selling after a sell-focus respec must never yield profit.");
            }
            finally
            {
                Object.DestroyImmediate(shop);
                Object.DestroyImmediate(item);
            }
        }

        private static SkillTreeState BuildRankThreeState(string variant)
        {
            var state = new SkillTreeState(3);
            state.Purchase(MarketSkillModifierProvider.NodeId, 1, "crafting");
            state.RankUp(MarketSkillModifierProvider.NodeId, "crafting");
            state.RankUp(MarketSkillModifierProvider.NodeId, "crafting");
            state.SetChosenVariant(MarketSkillModifierProvider.NodeId, variant);

            return state;
        }
    }
}
