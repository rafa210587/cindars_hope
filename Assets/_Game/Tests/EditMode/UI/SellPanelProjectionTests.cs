using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.UI
{
    public sealed class SellPanelProjectionTests
    {
        [Test]
        public void ShopProjectionEqualsCommittedMixedLotPayoutAndRetainsInvalid()
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = "item_projection_wood";
            item.MaxStack = 99;
            item.BaseValue = 17;
            var database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registry = typeof(DataRegistrySO<ItemDataSO>);
            registry.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(database, new[] { item });
            registry.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(database, null);
            var shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            shopData.Id = "shop_projection";
            shopData.SellPriceMultiplier = 2f;
            shopData.Items = new[] { new ShopItemEntry { ItemId = item.Id, BaseDailyStock = 5 } };
            var host = new GameObject("sell-projection-fixture");
            var bootstrapHost = new GameObject("sell-projection-bootstrap");
            var previousBootstrap = GameBootstrap.Instance;
            try
            {
                var inventory = host.AddComponent<InventoryManager>();
                inventory.Initialize(database);
                var player = host.AddComponent<PlayerManager>();
                player.SetGold(100);
                var shop = host.AddComponent<ShopManager>();
                shop.Configure(database);
                Assert.IsTrue(shop.InitializeShop(shopData));
                bootstrapHost.SetActive(false);
                var bootstrap = bootstrapHost.AddComponent<GameBootstrap>();
                typeof(GameBootstrap).GetField("_itemDatabase",
                    BindingFlags.Instance | BindingFlags.NonPublic).SetValue(bootstrap, database);
                typeof(GameBootstrap).GetField("_instance",
                    BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, bootstrap);

                Assert.IsTrue(inventory.TryAddPurchasedItem(item.Id, 1, 5).Success);
                Assert.IsTrue(inventory.TryAddItem(item.Id, 1).Success);
                Assert.IsTrue(inventory.TryAddItemWithLots(item.Id, 1,
                    new[] { new ItemOriginLot(ItemOriginKind.Invalid, 1) }).Success);
                Assert.IsTrue(shop.TryQuoteSellItem(shopData.Id, item.Id, 3,
                    inventory, out var quote));
                Assert.AreEqual(2, quote.SellAmount);
                Assert.AreEqual(1, quote.RetainedAmount);
                Assert.AreEqual(1, quote.LimitedUnits);
                Assert.AreEqual(quote.NormalGold / 2 + 4, quote.FinalGold);

                var result = shop.TrySellItem(shopData.Id, item.Id, 3, player, inventory);
                Assert.IsTrue(result.Success, result.Message);
                Assert.AreEqual(quote.FinalGold, result.GoldDelta);
                Assert.AreEqual(1, inventory.GetAmount(item.Id));
                Assert.AreEqual(0, inventory.GetEligibleAmount(item.Id));
            }
            finally
            {
                typeof(GameBootstrap).GetField("_instance",
                    BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, previousBootstrap);
                Object.DestroyImmediate(bootstrapHost);
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(shopData);
                Object.DestroyImmediate(database);
                Object.DestroyImmediate(item);
            }
        }
    }
}
