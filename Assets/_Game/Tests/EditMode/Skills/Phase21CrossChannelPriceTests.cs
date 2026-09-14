using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.City.Services;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Farm.Shipping;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using CindarsHope.World.Events;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    [TestFixture]
    public sealed class Phase21CrossChannelPriceTests
    {
        private const string ShopFolder = "Assets/_Game/Data/Economy";
        private const string ItemFolder = "Assets/_Game/Data/Items";

        private Func<MarketPriceDirection, float> _previousMarketSource;
        private Func<float> _previousGoldGainSource;
        private Func<string, bool> _previousServiceQuery;
        private WorldDayResolution _previousDay;

        [SetUp]
        public void SetUp()
        {
            _previousMarketSource = MarketSkillModifierProvider.MultiplierSource;
            _previousGoldGainSource = AccessoryEffectRouter.GoldGainModifierSource;
            _previousServiceQuery = CityServiceAccess.OwnsServiceQuery;
            _previousDay = WorldEventHooks.Active;
            MarketSkillModifierProvider.MultiplierSource = null;
            // A concessão de contrato é um teto alcançável para shipping; nenhum bootstrap é usado.
            CityServiceAccess.OwnsServiceQuery = _ => true;
        }

        [TearDown]
        public void TearDown()
        {
            MarketSkillModifierProvider.MultiplierSource = _previousMarketSource;
            AccessoryEffectRouter.GoldGainModifierSource = _previousGoldGainSource;
            CityServiceAccess.OwnsServiceQuery = _previousServiceQuery;
            WorldEventHooks.Active = _previousDay;
        }

        [TestCase("neutral", false, false, false)]
        [TestCase("finan", true, false, false)]
        [TestCase("amber", false, true, false)]
        [TestCase("surge", false, false, true)]
        [TestCase("combined", true, true, true)]
        [Explicit("AC05: diagnóstico de arbitragem; reexecutar após aprovação da política de preços.")]
        public void ListedSellableItems_BuyFocusRanksCannotProfitAcrossOrdinaryResaleChannels(
            string scenario, bool finan, bool amber, bool surge)
        {
            ConfigureSellAllBonuses(finan, amber, surge);
            var shops = LoadShops();
            var items = LoadItems();
            var shipping = new ShippingPriceResolver();
            var violations = new List<string>();
            int checkedQuotes = 0;
            int overrides = 0;
            int fractionalBuyQuotes = 0;

            foreach (var shop in shops)
            foreach (var entry in shop.Items ?? Array.Empty<ShopItemEntry>())
            {
                Assert.That(entry, Is.Not.Null, shop.Id + " has a null listing");
                Assert.That(items.TryGetValue(entry.ItemId, out var item), Is.True,
                    shop.Id + "/" + entry.ItemId + " is absent from ItemDataSO assets");
                if (!IsOrdinarilySellable(item)) continue;

                if (entry.BuyPriceOverride > 0) overrides++;
                int sourceValue = entry.BuyPriceOverride > 0 ? entry.BuyPriceOverride : item.BaseValue;

                foreach (int rank in new[] { 1, 2, 3 })
                foreach (int quantity in Quantities(entry))
                {
                    MarketSkillModifierProvider.MultiplierSource = direction =>
                        MarketSkillModifierProvider.Resolve(rank, MarketSkillModifierProvider.BuyVariantId, direction);
                    int cost = ShopManager.CalculateBuyPrice(item, entry, shop, quantity);
                    Assert.That(cost, Is.GreaterThan(0), shop.Id + "/" + item.Id + " buy quote");
                    float buyMultiplier = entry.BuyPriceOverride > 0 ? 1f : shop.BuyPriceMultiplier;
                    if (quantity == 1 && Math.Abs(sourceValue * buyMultiplier *
                        MarketSkillModifierProvider.Resolve(rank, MarketSkillModifierProvider.BuyVariantId,
                            MarketPriceDirection.BuyFromShop) % 1f) > 0.0001f)
                        fractionalBuyQuotes++;

                    // A revenda pode acontecer após respec. O primeiro respec é gratuito; não
                    // descontar custo hipotético do quote. ShopSell é o melhor destino de loja.
                    MarketSkillModifierProvider.MultiplierSource = direction =>
                        MarketSkillModifierProvider.Resolve(3, MarketSkillModifierProvider.SellVariantId, direction);
                    int shopSell = shops.Max(destination => ShopManager.CalculateSellPrice(item, destination, quantity));
                    int sellAll = WorldEventHooks.ApplySellGold(
                        AccessoryEffectRouter.ApplyGoldGain(item.BaseValue * quantity));
                    int sellPoint = ItemPriceResolver.ResolveSellingPrice(
                        item.BaseValue, SellContext.Shipping) * quantity;
                    int farmShipping = Mathf.RoundToInt(shipping.Resolve(new ShippingPriceInput
                    {
                        ItemId = item.Id, BaseValue = item.BaseValue, QualityTier = 0,
                        ChannelMultiplier = 0.95f
                    }) * quantity);

                    checkedQuotes++;
                    RecordProfit(violations, shop, entry, rank, quantity, scenario, cost,
                        "ShopSell", shopSell);
                    RecordProfit(violations, shop, entry, rank, quantity, scenario, cost,
                        "SellAll", sellAll);
                    RecordProfit(violations, shop, entry, rank, quantity, scenario, cost,
                        "SellPoint", sellPoint);
                    RecordProfit(violations, shop, entry, rank, quantity, scenario, cost,
                        "FarmShippingWithContract", farmShipping);
                }
            }

            Assert.That(checkedQuotes, Is.GreaterThan(0), "No sellable listing was checked.");
            Assert.That(overrides, Is.GreaterThan(0), "BuyPriceOverride entries must be present in this audit.");
            Assert.That(fractionalBuyQuotes, Is.GreaterThan(0), "Rounded purchase quotes must be present.");
            if (violations.Count > 0)
                Assert.Fail($"{scenario}: {violations.Count} profitable channel quotes among {checkedQuotes} " +
                    "item/rank/lot quotes. First cases: " + string.Join(" | ", violations.Take(20)));
        }

        [Test]
        public void GeneralStore_BreadGetsEffectiveDiscountAtEveryBuyFocusRank()
        {
            var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(
                ShopFolder + "/Shop_General_Store.asset");
            var bread = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
                ItemFolder + "/item_consumable_food_bread.asset");
            Assert.That(shop, Is.Not.Null);
            Assert.That(bread, Is.Not.Null);
            var entry = shop.GetEntry(bread.Id);
            Assert.That(entry, Is.Not.Null);

            int previousPrice = ShopManager.CalculateBuyPrice(bread, entry, shop);
            for (int rank = 1; rank <= 3; rank++)
            {
                int capturedRank = rank;
                MarketSkillModifierProvider.MultiplierSource = direction =>
                    MarketSkillModifierProvider.Resolve(capturedRank,
                        MarketSkillModifierProvider.BuyVariantId, direction);
                int currentPrice = ShopManager.CalculateBuyPrice(bread, entry, shop);
                Assert.That(currentPrice, Is.LessThan(previousPrice),
                    $"General Store bread: buy focus R{rank} must lower the actual quoted price.");
                previousPrice = currentPrice;
            }
        }

        private static void ConfigureSellAllBonuses(bool finan, bool amber, bool surge)
        {
            var finanRing = AccessoryCatalog.GetById(AccessoryCatalog.RingFinan);
            Assert.That(finanRing, Is.Not.Null);
            float goldGain = finanRing.Effects.Single(effect =>
                effect.Type == AccessoryEffectType.GoldGainPercent).Magnitude;
            AccessoryEffectRouter.GoldGainModifierSource = () => finan ? goldGain : 0f;

            var amberDefinition = WorldEventDefinitions.LunarPeaks.Single(peak =>
                peak.Effect == WorldEventDefinitions.LunarPeakEffect.AmberSellPrices);
            var surgeDefinition = WorldEventDefinitions.RandomEventPool.Single(worldEvent =>
                worldEvent.Effect == WorldEventDefinitions.WorldEventEffect.CropPriceSurge);
            WorldEventHooks.Active = new WorldDayResolution
            {
                LunarEffect = amber ? amberDefinition.Effect : WorldEventDefinitions.LunarPeakEffect.None,
                LunarMagnitude = amber ? amberDefinition.Magnitude : 0f,
                WorldEffect = surge ? surgeDefinition.Effect : WorldEventDefinitions.WorldEventEffect.None,
                WorldEventMagnitude = surge ? surgeDefinition.Magnitude : 0f
            };
        }

        private static ShopDataSO[] LoadShops()
        {
            var shops = AssetDatabase.FindAssets("t:ShopDataSO", new[] { ShopFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ShopDataSO>)
                .ToArray();
            Assert.That(shops, Is.Not.Empty);
            Assert.That(shops.All(shop => shop != null && !string.IsNullOrWhiteSpace(shop.Id)), Is.True);
            return shops;
        }

        private static Dictionary<string, ItemDataSO> LoadItems()
        {
            var items = AssetDatabase.FindAssets("t:ItemDataSO", new[] { ItemFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ItemDataSO>)
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.Id))
                .ToArray();
            Assert.That(items, Is.Not.Empty);
            return items.ToDictionary(item => item.Id, StringComparer.Ordinal);
        }

        // Espelha somente os gates observáveis de SellableItemPolicy. A política real depende de
        // GameBootstrap e, por isso, este teste certifica quotes de assets, não transações.
        private static bool IsOrdinarilySellable(ItemDataSO item) =>
            item.BaseValue > 0 &&
            item.Category != ItemCategory.Quest && item.Category != ItemCategory.KeyItem &&
            !item.Id.Contains("tool_pickaxe") && !item.Id.Contains("tool_hoe") &&
            !item.Id.Contains("tool_watering");

        private static IEnumerable<int> Quantities(ShopItemEntry entry)
        {
            yield return 1;
            yield return 2;
            int stock = entry.BaseDailyStock > 0 ? entry.BaseDailyStock : 5;
            if (stock != 1 && stock != 2) yield return stock;
        }

        private static void RecordProfit(List<string> violations, ShopDataSO shop, ShopItemEntry entry,
            int rank, int quantity, string scenario, int cost, string channel, int revenue)
        {
            if (revenue <= cost) return;
            violations.Add($"{shop.Id}/{entry.ItemId} override={entry.BuyPriceOverride} " +
                $"R{rank} x{quantity} {scenario}/{channel}: buy={cost}, sell={revenue}, gain={revenue - cost}");
        }
    }
}
