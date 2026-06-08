using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Shops;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class ShopInventoryStockTests
    {
        private ShopInventoryDefinition DefaultShop(string shopId = "shop_general")
        {
            return new ShopInventoryDefinition
            {
                ShopId = shopId,
                NpcOwnerId = "npc_merchant",
                RestockPolicy = new ShopRestockPolicyDefinition
                {
                    PolicyType = RestockPolicyType.DailyMorning,
                    IntervalDays = 1
                },
                BaseStockLines = new List<StockLineDefinition>
                {
                    new StockLineDefinition
                    {
                        StockLineId = "sl_carrot_seed", ItemId = "item_carrot_seed",
                        StockType = StockType.BaseStock, MinQuantity = 1, MaxQuantity = 10,
                        RestockToQuantity = 10, RestockChance = 1f
                    }
                }
            };
        }

        private ShopStockState DefaultState(string shopId = "shop_general")
        {
            return new ShopStockState
            {
                ShopId = shopId,
                StockLineStates = new Dictionary<string, StockLineState>
                {
                    ["sl_carrot_seed"] = new StockLineState { StockLineId = "sl_carrot_seed", CurrentQuantity = 5 }
                },
                LastRestockDay = 0,
                NextRestockDay = 1
            };
        }

        [Test]
        public void Purchase_DecreasesQuantity()
        {
            var shop = DefaultShop();
            var state = DefaultState();
            bool ok = state.TryPurchase("sl_carrot_seed", shop.BaseStockLines[0], out _);
            Assert.IsTrue(ok);
            Assert.AreEqual(4, state.StockLineStates["sl_carrot_seed"].CurrentQuantity);
        }

        [Test]
        public void Purchase_OutOfStock_Fails()
        {
            var shop = DefaultShop();
            var state = DefaultState();
            state.StockLineStates["sl_carrot_seed"].CurrentQuantity = 0;
            bool ok = state.TryPurchase("sl_carrot_seed", shop.BaseStockLines[0], out var reason);
            Assert.IsFalse(ok);
            Assert.AreEqual("Out of stock", reason);
        }

        [Test]
        public void UniqueStock_CannotPurchaseTwice()
        {
            var line = new StockLineDefinition
            {
                StockLineId = "sl_unique_sword", ItemId = "item_unique_sword",
                StockType = StockType.UniqueStock, IsUniqueStock = true,
                MinQuantity = 1, MaxQuantity = 1
            };
            var shop = new ShopInventoryDefinition { ShopId = "shop_smith" };
            var state = new ShopStockState { ShopId = "shop_smith" };
            state.StockLineStates["sl_unique_sword"] = new StockLineState { StockLineId = "sl_unique_sword", CurrentQuantity = 1 };

            bool first = state.TryPurchase("sl_unique_sword", line, out _);
            Assert.IsTrue(first);
            state.StockLineStates["sl_unique_sword"].CurrentQuantity = 1; // simulate restock attempt — must still be blocked by flag
            bool second = state.TryPurchase("sl_unique_sword", line, out var reason2);
            Assert.IsFalse(second);
            Assert.AreEqual("UniqueStock already purchased", reason2);
        }

        [Test]
        public void DailyLimit_Enforced()
        {
            var line = new StockLineDefinition
            {
                StockLineId = "sl_limited", ItemId = "item_potion",
                StockType = StockType.LimitedStock,
                MinQuantity = 1, MaxQuantity = 5, PurchaseLimitPerDay = 2
            };
            var state = new ShopStockState { ShopId = "shop_alc" };
            state.StockLineStates["sl_limited"] = new StockLineState { StockLineId = "sl_limited", CurrentQuantity = 5 };

            state.TryPurchase("sl_limited", line, out _);
            state.TryPurchase("sl_limited", line, out _);
            bool third = state.TryPurchase("sl_limited", line, out var reason);
            Assert.IsFalse(third);
            Assert.AreEqual("Daily purchase limit reached", reason);
        }

        [Test]
        public void Restock_DoesNotTriggerOnSameDay()
        {
            var shop = DefaultShop();
            var state = DefaultState();
            state.StockLineStates["sl_carrot_seed"].CurrentQuantity = 2;
            state.LastRestockDay = 1;
            var processor = new ShopRestockProcessor();
            processor.ProcessDayStart(shop, state, 1); // same day
            Assert.AreEqual(2, state.StockLineStates["sl_carrot_seed"].CurrentQuantity, "Must not restock same day");
        }

        [Test]
        public void Restock_TriggersOnNewDay()
        {
            var shop = DefaultShop();
            var state = DefaultState();
            state.StockLineStates["sl_carrot_seed"].CurrentQuantity = 2;
            state.LastRestockDay = 0;
            var processor = new ShopRestockProcessor();
            processor.ProcessDayStart(shop, state, 1); // next day
            Assert.AreEqual(10, state.StockLineStates["sl_carrot_seed"].CurrentQuantity, "Must restock to RestockToQuantity");
        }

        [Test]
        public void UniqueStock_NotRestocked_ByProcessor()
        {
            var shop = new ShopInventoryDefinition
            {
                ShopId = "shop_smith",
                RestockPolicy = new ShopRestockPolicyDefinition { PolicyType = RestockPolicyType.DailyMorning, IntervalDays = 1 },
                BaseStockLines = new List<StockLineDefinition>
                {
                    new StockLineDefinition
                    {
                        StockLineId = "sl_unique_blade", ItemId = "item_unique_blade",
                        StockType = StockType.UniqueStock, IsUniqueStock = true,
                        MinQuantity = 0, MaxQuantity = 1, RestockToQuantity = 1
                    }
                }
            };
            var state = new ShopStockState { ShopId = "shop_smith", LastRestockDay = 0, NextRestockDay = 1 };
            state.StockLineStates["sl_unique_blade"] = new StockLineState { StockLineId = "sl_unique_blade", CurrentQuantity = 0 };

            var processor = new ShopRestockProcessor();
            processor.ProcessDayStart(shop, state, 1);
            Assert.AreEqual(0, state.StockLineStates["sl_unique_blade"].CurrentQuantity, "UniqueStock must not be restocked");
        }

        [Test]
        public void StockLine_RequiredQuestFlag_IsUnlocked()
        {
            var line = new StockLineDefinition
            {
                StockLineId = "sl_rare", ItemId = "item_rare_seed", RequiredQuestFlag = "quest_unlock_rare_seeds"
            };
            bool unlocked = line.IsUnlocked(null, 0, "quest_unlock_rare_seeds", 0, 0, null);
            bool locked = line.IsUnlocked(null, 0, null, 0, 0, null);
            Assert.IsTrue(unlocked);
            Assert.IsFalse(locked);
        }

        [Test]
        public void ShopInventory_ForbiddenItems_ListExists()
        {
            var shop = DefaultShop();
            shop.ForbiddenItems.Add("item_fruto_mana");
            Assert.IsTrue(shop.ForbiddenItems.Contains("item_fruto_mana"));
        }
    }
}
