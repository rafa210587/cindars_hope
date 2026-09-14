using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Economy.Pricing;
using CindarsHope.Economy.Transactions;
using CindarsHope.Foundation.Transactions;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Economy
{
    public class PurchasedOriginTransactionTests
    {
        private sealed class InventoryPort : IPurchasedInventoryTransactionPort
        {
            public bool Capacity = true;
            public bool WriteSucceeds = true;
            public int Amount;
            public int PaidGold;
            public int Published;

            public bool CanAddItem(string itemId, int amount) => Capacity;
            public bool TryAddPurchasedItemSilently(string itemId, int amount, int totalCost)
            {
                if (!WriteSucceeds) return false;
                Amount += amount;
                PaidGold += totalCost;
                return true;
            }
            public bool TryRollbackPurchasedAdd()
            {
                Amount = 0;
                PaidGold = 0;
                return true;
            }
            public void PublishCommittedInventoryChange(string itemId, int amount) => Published++;
        }

        private sealed class WalletPort : IStagedPurchaseWalletPort
        {
            public int Balance { get; private set; } = 100;
            public bool StageSucceeds = true;
            public bool CommitSucceeds = true;
            public int Published;
            public Action OnGoldChanged;
            private int _before;

            public bool TryStageDebit(int amount)
            {
                if (!StageSucceeds) return false;
                _before = Balance;
                Balance -= amount;
                return true;
            }
            public bool TryRollbackDebit()
            {
                Balance = _before;
                return true;
            }
            public bool TryCommitDebit()
            {
                if (!CommitSucceeds) return false;
                if (Balance != _before)
                {
                    Published++;
                    OnGoldChanged?.Invoke();
                }
                return true;
            }
        }

        private sealed class StockPort : IShopPurchaseStockPort
        {
            public int Amount = 5;
            public int Requested = 3;
            public bool ReserveSucceeds = true;
            public int Published;

            public bool CanReserve() => Amount >= Requested;
            public bool TryReserve()
            {
                if (!ReserveSucceeds) return false;
                Amount -= Requested;
                return true;
            }
            public bool Rollback()
            {
                Amount += Requested;
                return true;
            }
            public void PublishCommittedChange() => Published++;
        }

        [TestCase(17, new int[] { 5, 6, 6 })]
        [TestCase(5, new int[] { 1, 2, 2 })]
        [TestCase(0, new int[] { 0, 0, 0 })]
        public void Purchase_PreservesExactPaidBasis(int totalCost, int[] expectedBases)
        {
            var inventory = new InventoryPort();
            var wallet = new WalletPort();
            var stock = new StockPort();
            var result = AtomicPurchaseTransaction.ExecutePurchased(
                inventory, wallet, "item_seed", 3, totalCost, stock);

            Assert.That(result.Success, Is.True);
            Assert.That(wallet.Balance, Is.EqualTo(100 - totalCost));
            Assert.That(inventory.Amount, Is.EqualTo(3));
            Assert.That(inventory.PaidGold, Is.EqualTo(totalCost));
            Assert.That(stock.Amount, Is.EqualTo(2));
            Assert.That(inventory.Published, Is.EqualTo(1));
            Assert.That(stock.Published, Is.EqualTo(1));
            Assert.That(ItemOriginLot.Purchased(3, totalCost)
                .SelectMany(lot => Enumerable.Repeat(lot.UnitPaidBasisGold, lot.Quantity)),
                Is.EqualTo(expectedBases));
        }

        [TestCase("capacity")]
        [TestCase("stock")]
        [TestCase("debit")]
        [TestCase("stockWrite")]
        [TestCase("inventoryWrite")]
        [TestCase("commit")]
        public void PurchaseFailure_RestoresWalletStockAndLots(string failure)
        {
            var inventory = new InventoryPort { Capacity = failure != "capacity",
                WriteSucceeds = failure != "inventoryWrite" };
            var wallet = new WalletPort { StageSucceeds = failure != "debit",
                CommitSucceeds = failure != "commit" };
            var stock = new StockPort { Amount = failure == "stock" ? 0 : 5,
                ReserveSucceeds = failure != "stockWrite" };
            var startingStock = stock.Amount;

            var result = AtomicPurchaseTransaction.ExecutePurchased(
                inventory, wallet, "item_seed", 3, 17, stock);

            Assert.That(result.Success, Is.False);
            Assert.That(wallet.Balance, Is.EqualTo(100));
            Assert.That(inventory.Amount, Is.Zero);
            Assert.That(inventory.PaidGold, Is.Zero);
            Assert.That(stock.Amount, Is.EqualTo(startingStock));
            Assert.That(wallet.Published + inventory.Published + stock.Published, Is.Zero);
        }

        [Test]
        public void GoldListener_SeesPurchasedLotsDebitAndReducedStock()
        {
            var inventory = new InventoryPort();
            var wallet = new WalletPort();
            var stock = new StockPort();
            wallet.OnGoldChanged = () =>
            {
                Assert.That(wallet.Balance, Is.EqualTo(83));
                Assert.That(inventory.Amount, Is.EqualTo(3));
                Assert.That(inventory.PaidGold, Is.EqualTo(17));
                Assert.That(stock.Amount, Is.EqualTo(2));
            };

            var result = AtomicPurchaseTransaction.ExecutePurchased(
                inventory, wallet, "item_seed", 3, 17, stock);

            Assert.That(result.Success, Is.True);
            Assert.That(wallet.Published, Is.EqualTo(1));
        }
    }

    public class PurchasedOriginEntrypointTests
    {
        private const string ItemId = "item_test_purchased_origin";
        private GameObject _host;
        private ItemDataSO _item;
        private ItemDatabaseSO _database;
        private ShopDataSO _shopData;
        private InventoryManager _inventory;
        private PlayerManager _player;
        private ShopManager _shop;
        private EconomyManager _economy;
        private SeedShopPoint _seedPoint;
        private SellPoint _sellPoint;
        private GameObject _bootstrapHost;
        private GameBootstrap _previousBootstrap;

        [SetUp]
        public void SetUp()
        {
            _item = ScriptableObject.CreateInstance<ItemDataSO>();
            _item.Id = ItemId;
            _item.BaseValue = 17;
            _item.MaxStack = 99;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registry = typeof(DataRegistrySO<ItemDataSO>);
            registry.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _item });
            registry.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);

            _host = new GameObject("PurchasedOriginEntrypointTests");
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
            _player = _host.AddComponent<PlayerManager>();
            _player.SetGold(100);
            _shop = _host.AddComponent<ShopManager>();
            _shop.Configure(_database);
            _shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            _shopData.Id = "shop_test_purchased_origin";
            _shopData.Items = new[] { new ShopItemEntry
            {
                ItemId = ItemId, BaseDailyStock = 5, IsFiniteStock = true,
                BuyPriceOverride = 17
            } };
            Assert.That(_shop.InitializeShop(_shopData), Is.True);

            _economy = _host.AddComponent<EconomyManager>();
            SetField(_economy, "_inventoryManager", _inventory);
            SetField(_economy, "_playerManager", _player);
            // EditMode AddComponent does not consistently invoke runtime OnEnable.
            // Subscribe explicitly after wiring, as the bootstrap would in PlayMode.
            InvokeLifecycle(_economy, "OnEnable");
            _seedPoint = _host.AddComponent<SeedShopPoint>();
            SetField(_seedPoint, "_inventoryManager", _inventory);
            SetField(_seedPoint, "_playerManager", _player);
            SetField(_seedPoint, "_seedId", ItemId);
            SetField(_seedPoint, "_amount", 3);
            SetField(_seedPoint, "_totalCost", 5);
            _sellPoint = _host.AddComponent<SellPoint>();
            SetField(_sellPoint, "_inventoryManager", _inventory);
            SetField(_sellPoint, "_playerManager", _player);

            // SellableItemPolicy resolves the catalog through the scene bootstrap.
            // Keep this test bootstrap inactive so it does not initialize a whole game.
            _previousBootstrap = GameBootstrap.Instance;
            _bootstrapHost = new GameObject("PurchasedOriginCatalogBootstrap");
            _bootstrapHost.SetActive(false);
            var bootstrap = _bootstrapHost.AddComponent<GameBootstrap>();
            SetField(bootstrap, "_itemDatabase", _database);
            SetBootstrap(bootstrap);
        }

        [TearDown]
        public void TearDown()
        {
            SetBootstrap(_previousBootstrap);
            if (_bootstrapHost != null) UnityEngine.Object.DestroyImmediate(_bootstrapHost);
            if (_economy != null) InvokeLifecycle(_economy, "OnDisable");
            if (_host != null) UnityEngine.Object.DestroyImmediate(_host);
            if (_shopData != null) UnityEngine.Object.DestroyImmediate(_shopData);
            if (_database != null) UnityEngine.Object.DestroyImmediate(_database);
            if (_item != null) UnityEngine.Object.DestroyImmediate(_item);
        }

        [Test]
        public void ShopManager_GoldListenerCapturesPaidItemAndReducedStock()
        {
            var goldEvents = 0;
            var observedGold = -1;
            var observedStock = -1;
            InventorySaveData observedInventory = null;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ =>
            {
                goldEvents++;
                observedGold = _player.CurrentGold;
                observedInventory = _inventory.CaptureSaveData();
                observedStock = _shop.CaptureAllShopStock()[0].Items[0].CurrentStock;
            }))
            {
                var result = _shop.TryBuyItem(_shopData.Id, ItemId, 1, _player, _inventory);
                Assert.That(result.Success, Is.True);
            }
            Assert.That(goldEvents, Is.EqualTo(1));
            Assert.That(observedGold, Is.EqualTo(83));
            AssertPurchasedSave(observedInventory, 1, 17);
            Assert.That(observedStock, Is.EqualTo(4));
        }

        [Test]
        public void EconomyManager_RequestCommitsThreeForSeventeen()
        {
            var goldEvents = 0;
            var observedGold = -1;
            InventorySaveData observedInventory = null;
            Assert.That(GameEventBus.HasSubscribers<ItemPurchaseRequestedEvent>(), Is.True);
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ =>
            {
                goldEvents++;
                observedGold = _player.CurrentGold;
                observedInventory = _inventory.CaptureSaveData();
            }))
            {
                GameEventBus.Publish(new ItemPurchaseRequestedEvent(ItemId, 3, 17, "test"));
            }
            Assert.That(goldEvents, Is.EqualTo(1));
            Assert.That(observedGold, Is.EqualTo(83));
            AssertPurchasedSave(observedInventory, 3, 17);
            AssertPurchasedSave(3, 17);
        }

        [Test]
        public void SeedShopPoint_InteractCommitsThreeForFive()
        {
            var goldEvents = 0;
            var observedGold = -1;
            InventorySaveData observedInventory = null;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ =>
            {
                goldEvents++;
                observedGold = _player.CurrentGold;
                observedInventory = _inventory.CaptureSaveData();
            }))
            {
                _seedPoint.Interact(null);
            }
            Assert.That(goldEvents, Is.EqualTo(1));
            Assert.That(observedGold, Is.EqualTo(95));
            AssertPurchasedSave(observedInventory, 3, 5);
            AssertPurchasedSave(3, 5);
        }

        [Test]
        public void FailedEntrypoints_DoNotChangeWalletStockOrLots()
        {
            var goldEvents = 0;
            var inventoryEvents = 0;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ => goldEvents++))
            using (GameEventBus.Subscribe<InventoryChangedEvent>(_ => inventoryEvents++))
            {
                var shopResult = _shop.TryBuyItem(_shopData.Id, ItemId, 6, _player, _inventory);
                Assert.That(shopResult.Success, Is.False);
                _player.SetGold(0);
                GameEventBus.Publish(new ItemPurchaseRequestedEvent(ItemId, 3, 17, "test"));
                _seedPoint.Interact(null);
                Assert.That(goldEvents, Is.EqualTo(1), "Only the test's SetGold should publish.");
                Assert.That(inventoryEvents, Is.Zero);
            }
            Assert.That(_player.CurrentGold, Is.Zero);
            Assert.That(_inventory.GetAmount(ItemId), Is.Zero);
            Assert.That(_shop.CaptureAllShopStock()[0].Items[0].CurrentStock, Is.EqualTo(5));
        }

        [Test]
        public void ShopManager_InvalidComputedPriceCannotGrantFreeItem()
        {
            _shopData.Items[0].BuyPriceOverride = int.MaxValue;
            var result = _shop.TryBuyItem(_shopData.Id, ItemId, 3, _player, _inventory);

            Assert.That(result.Success, Is.False);
            Assert.That(_player.CurrentGold, Is.EqualTo(100));
            Assert.That(_inventory.GetAmount(ItemId), Is.Zero);
            Assert.That(_shop.CaptureAllShopStock()[0].Items[0].CurrentStock, Is.EqualTo(5));
        }

        [Test]
        public void SaleBatch_CapsPurchasedAfterBonusAndLeavesInvalidInReentrantSave()
        {
            Assert.That(_inventory.TryAddPurchasedItem(ItemId, 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem(ItemId, 1).Success, Is.True);
            Assert.That(_inventory.TryAddItemWithLots(ItemId, 1,
                new[] { new ItemOriginLot(ItemOriginKind.Invalid, 1) }).Success, Is.True);
            Assert.That(_inventory.GetEligibleAmount(ItemId), Is.EqualTo(4));
            Assert.That(_inventory.TryPrepareBatchRemoval(
                new Dictionary<string, int> { [ItemId] = 4 }, out var batch), Is.True);
            Assert.That(AtomicOriginSaleTransaction.TryQuoteBatch(batch,
                new Dictionary<string, int> { [ItemId] = 20 }, 80,
                out var payout, out var limitedUnits), Is.True);
            Assert.That(payout, Is.EqualTo(34)); // earned 20 + purchased caps 4+5+5
            Assert.That(limitedUnits, Is.EqualTo(3));

            var goldEvents = 0;
            var observedGold = -1;
            InventorySaveData observedInventory = null;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ =>
            {
                goldEvents++;
                observedGold = _player.CurrentGold;
                observedInventory = _inventory.CaptureSaveData();
            }))
            {
                Assert.That(AtomicOriginSaleTransaction.TryCommitBatch(
                    _inventory, _player, batch, payout), Is.True);
            }
            Assert.That(goldEvents, Is.EqualTo(1));
            Assert.That(observedGold, Is.EqualTo(134));
            AssertOnlyInvalidRemains(observedInventory);
            Assert.That(_inventory.GetAmount(ItemId), Is.EqualTo(1));
        }

        [Test]
        public void SaleBatch_CreditOverflowLeavesInventoryAndWalletUnchanged()
        {
            Assert.That(_inventory.TryAddPurchasedItem(ItemId, 3, 17).Success, Is.True);
            Assert.That(_inventory.TryPrepareBatchRemoval(
                new Dictionary<string, int> { [ItemId] = 3 }, out var batch), Is.True);
            _player.SetGold(int.MaxValue);
            var events = 0;
            using (GameEventBus.Subscribe<InventoryChangedEvent>(_ => events++))
            {
                Assert.That(AtomicOriginSaleTransaction.TryCommitBatch(
                    _inventory, _player, batch, 14), Is.False);
            }
            Assert.That(_player.CurrentGold, Is.EqualTo(int.MaxValue));
            Assert.That(_inventory.GetAmount(ItemId), Is.EqualTo(3));
            Assert.That(events, Is.Zero);
            AssertPurchasedSave(3, 17);
        }

        [Test]
        public void SaleBatch_BonusRemainderUsesSlotLotOrderEvenWhenEarnedSelectedFirst()
        {
            Assert.That(_inventory.TryAddPurchasedItem(ItemId, 1, 5).Success, Is.True);
            Assert.That(_inventory.TryAddItem(ItemId, 1).Success, Is.True);
            Assert.That(_inventory.TryPrepareBatchRemoval(
                new Dictionary<string, int> { [ItemId] = 2 }, out var batch), Is.True);
            Assert.That(batch.Selections[0].Slices[0].Kind, Is.EqualTo(ItemOriginKind.Earned),
                "Partial-sale selection favors earned units, independent of bonus order.");

            Assert.That(AtomicOriginSaleTransaction.TryQuoteBatch(batch,
                new Dictionary<string, int> { [ItemId] = 10 }, 21,
                out var payout, out var limitedUnits), Is.True);
            Assert.That(payout, Is.EqualTo(14),
                "The +1 remainder belongs to the purchased lot at the earlier slot/lot index.");
            Assert.That(limitedUnits, Is.EqualTo(1));
        }

        [Test]
        public void ShopSell_CapsPurchasedAndPreservesEarnedPrice()
        {
            Assert.That(_shop.TryBuyItem(_shopData.Id, ItemId, 1,
                _player, _inventory).Success, Is.True);
            _shopData.SellPriceMultiplier = 2f;
            var normal = ShopManager.CalculateSellPrice(_item, _shopData);
            Assert.That(normal, Is.GreaterThan(16));
            var observedAmount = -1;
            var observedGold = -1;
            using (GameEventBus.Subscribe<InventoryChangedEvent>(evt =>
            {
                if (evt.Delta >= 0) return;
                observedAmount = _inventory.GetAmount(ItemId);
                observedGold = _player.CurrentGold;
            }))
            {
                var purchased = _shop.TrySellItem(_shopData.Id, ItemId, 1,
                    _player, _inventory);
                Assert.That(purchased.Success, Is.True);
                Assert.That(purchased.GoldDelta, Is.EqualTo(16));
            }
            Assert.That(observedAmount, Is.Zero);
            Assert.That(observedGold, Is.EqualTo(99));

            Assert.That(_inventory.TryAddItem(ItemId, 1).Success, Is.True);
            var earned = _shop.TrySellItem(_shopData.Id, ItemId, 1, _player, _inventory);
            Assert.That(earned.Success, Is.True);
            Assert.That(earned.GoldDelta, Is.EqualTo(normal));
        }

        [Test]
        public void ShopSell_PartialRequestReportsRetainedInvalidUnits()
        {
            Assert.That(_inventory.TryAddItem(ItemId, 1).Success, Is.True);
            Assert.That(_inventory.TryAddItemWithLots(ItemId, 1,
                new[] { new ItemOriginLot(ItemOriginKind.Invalid, 1) }).Success, Is.True);

            var result = _shop.TrySellItem(_shopData.Id, ItemId, 2,
                _player, _inventory);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Does.Contain("1 unidade(s) retida(s)"));
            Assert.That(_inventory.GetAmount(ItemId), Is.EqualTo(1));
            AssertOnlyInvalidRemains();
        }

        [Test]
        public void SellAll_MixedStackPaysOnlyEligibleLotsWithOneCredit()
        {
            AddMixedSaleStack();
            var goldEvents = 0;
            var observedGold = -1;
            InventorySaveData observedInventory = null;
            string saleMessage = null;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ =>
            {
                goldEvents++;
                observedGold = _player.CurrentGold;
                observedInventory = _inventory.CaptureSaveData();
            }))
            using (GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(evt =>
            {
                if (evt.TransactionType == "SellAll") saleMessage = evt.Message;
            }))
            {
                GameEventBus.Publish(new SellAllRequestedEvent("test_non_urban"));
            }
            Assert.That(goldEvents, Is.EqualTo(1));
            Assert.That(observedGold, Is.EqualTo(131));
            AssertOnlyInvalidRemains(observedInventory);
            AssertOnlyInvalidRemains();
            Assert.That(saleMessage, Does.Contain("1 unidade(s) retida(s)"));
        }

        [Test]
        public void SellPoint_MixedStackPaysOnlyEligibleLotsWithOneCredit()
        {
            AddMixedSaleStack();
            var config = Resources.Load<EconomyBalanceConfigSO>("EconomyBalanceConfig");
            var unit = ItemPriceResolver.ResolveSellingPrice(
                _item.BaseValue, SellContext.Shipping, config);
            Assert.That(unit, Is.GreaterThan(0));
            var expected = unit + Math.Min(unit, 4) + Math.Min(unit, 5) + Math.Min(unit, 5);
            var goldEvents = 0;
            var observedGold = -1;
            InventorySaveData observedInventory = null;
            string saleMessage = null;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ =>
            {
                goldEvents++;
                observedGold = _player.CurrentGold;
                observedInventory = _inventory.CaptureSaveData();
            }))
            using (GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(evt =>
            {
                if (evt.TransactionType == "SellPoint") saleMessage = evt.Message;
            }))
            {
                _sellPoint.Interact(null);
            }
            Assert.That(goldEvents, Is.EqualTo(1));
            Assert.That(observedGold, Is.EqualTo(100 + expected));
            AssertOnlyInvalidRemains(observedInventory);
            AssertOnlyInvalidRemains();
            Assert.That(saleMessage, Does.Contain("1 unidade(s) retida(s)"));
        }

        private void AddMixedSaleStack()
        {
            Assert.That(_inventory.TryAddPurchasedItem(ItemId, 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem(ItemId, 1).Success, Is.True);
            Assert.That(_inventory.TryAddItemWithLots(ItemId, 1,
                new[] { new ItemOriginLot(ItemOriginKind.Invalid, 1) }).Success, Is.True);
        }

        private void AssertOnlyInvalidRemains()
            => AssertOnlyInvalidRemains(_inventory.CaptureSaveData());

        private void AssertOnlyInvalidRemains(InventorySaveData saved)
        {
            Assert.That(saved, Is.Not.Null);
            var slots = saved.Slots
                .Where(slot => slot.ItemId == ItemId).ToArray();
            Assert.That(slots.Sum(slot => slot.Amount), Is.EqualTo(1));
            Assert.That(slots.SelectMany(slot => slot.OriginLots).Single().Kind,
                Is.EqualTo((int)ItemOriginKind.Invalid));
        }

        private void AssertPurchasedSave(int quantity, int paidGold)
            => AssertPurchasedSave(_inventory.CaptureSaveData(), quantity, paidGold);

        private void AssertPurchasedSave(InventorySaveData saved, int quantity, int paidGold)
        {
            Assert.That(saved, Is.Not.Null);
            var slots = saved.Slots.Where(slot => slot.ItemId == ItemId).ToArray();
            Assert.That(slots.Sum(slot => slot.Amount), Is.EqualTo(quantity));
            Assert.That(slots.SelectMany(slot => slot.OriginLots).Sum(lot =>
                lot.Quantity * lot.UnitPaidBasisGold), Is.EqualTo(paidGold));
            Assert.That(slots.SelectMany(slot => slot.OriginLots).All(lot =>
                lot.Kind == (int)ItemOriginKind.Purchased), Is.True);
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }

        private static void InvokeLifecycle(object target, string name)
        {
            target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(target, null);
        }

        private static void SetBootstrap(GameBootstrap bootstrap)
        {
            typeof(GameBootstrap).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, bootstrap);
        }
    }
}
