using System;
using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC.Services;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.City
{
    public sealed class PurchasedOriginServiceTests
    {
        private GameObject _host;
        private InventoryManager _inventory;
        private ItemDatabaseSO _database;
        private ItemDataSO _scroll;
        private ItemDataSO _part;
        private ItemDataSO _filler;

        [SetUp]
        public void SetUp()
        {
            _scroll = Item("scroll", 1);
            _part = Item("monster_part", 1);
            _filler = Item("filler", 1);
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registry = typeof(DataRegistrySO<ItemDataSO>);
            registry.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _scroll, _part, _filler });
            registry.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("npc-origin-tests");
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_host);
            UnityEngine.Object.DestroyImmediate(_database);
            UnityEngine.Object.DestroyImmediate(_scroll);
            UnityEngine.Object.DestroyImmediate(_part);
            UnityEngine.Object.DestroyImmediate(_filler);
        }

        [Test]
        public void PaidBookOrder_RoundTripsPaidBasisAndDeliversOnceAfterCapacityRecovers()
        {
            var wallet = new BookContext();
            var effects = new BookEffects { BookId = "scroll" };
            var state = new NpcServicesPendingState();
            var executor = new NpcServiceExecutor(state, wallet, effects)
                { CurrentDayProvider = () => 5 };

            Assert.That(executor.TryExecute(NpcServiceCatalog.BookOrderServiceId).Success, Is.True);
            Assert.That(wallet.Gold, Is.Zero);
            var captured = state.Capture();
            Assert.That(captured.PendingBookOrders[0].PaidGold, Is.EqualTo(200));
            Assert.That(captured.PendingBookOrders[0].OriginLots[0].UnitPaidBasisGold, Is.EqualTo(200));

            var restored = new NpcServicesPendingState();
            restored.Restore(captured, 6);
            Assert.That(restored.GetDueBookOrders(7), Is.Empty);
            Assert.That(_inventory.TryAddItem("filler", InventoryManager.DefaultCapacity).Success, Is.True);
            Assert.That(TryDeliver(restored, 8), Is.False);
            Assert.That(restored.PendingBookOrderCount, Is.EqualTo(1));
            Assert.That(_inventory.RemoveItem("filler", 1), Is.True);
            Assert.That(TryDeliver(restored, 8), Is.True);
            Assert.That(TryDeliver(restored, 9), Is.False);
            Assert.That(restored.PendingBookOrderCount, Is.Zero);
            Assert.That(_inventory.GetAmount("scroll"), Is.EqualTo(1));
            Assert.That(_inventory.TryPrepareRemoval("scroll", 1, out var selection), Is.True);
            Assert.That(selection.Slices[0].Kind, Is.EqualTo(ItemOriginKind.Purchased));
            Assert.That(selection.Slices[0].UnitPaidBasisGold, Is.EqualTo(200));
            Assert.That(selection.Slices[0].UnitResaleCapGold, Is.EqualTo(199));
        }

        [Test]
        public void UnavailableBook_DoesNotChargeOrCreateOrder()
        {
            var wallet = new BookContext();
            var state = new NpcServicesPendingState();
            var executor = new NpcServiceExecutor(state, wallet,
                new BookEffects { BookId = null });
            Assert.That(executor.TryExecute(NpcServiceCatalog.BookOrderServiceId).Success, Is.False);
            Assert.That(wallet.Gold, Is.EqualTo(200));
            Assert.That(state.PendingBookOrderCount, Is.Zero);
        }

        [Test]
        public void OldOrderAndCorruptV6Order_HaveDistinctFailClosedOrigins()
        {
            var oldSave = new NpcServicesSaveData
            {
                PendingBookOrders = new List<NpcBookOrderSaveData>
                { new NpcBookOrderSaveData { DeliveredBookItemId = "scroll", DeliveryDay = 8 } }
            };
            var state = new NpcServicesPendingState();
            state.Restore(oldSave, 5);
            Assert.That(state.GetDueBookOrders(8)[0].OriginLots[0].Kind,
                Is.EqualTo((int)ItemOriginKind.LegacyUntracked));
            var migrated = state.Capture();
            state.Restore(migrated, 6);
            Assert.That(state.GetDueBookOrders(8)[0].OriginLots[0].Kind,
                Is.EqualTo((int)ItemOriginKind.LegacyUntracked));

            var corrupt = new NpcServicesSaveData
            {
                PendingBookOrders = new List<NpcBookOrderSaveData>
                { new NpcBookOrderSaveData { DeliveredBookItemId = "scroll", DeliveryDay = 8,
                    PaidGold = 200, OriginSchemaVersion = 0 } }
            };
            state.Restore(corrupt, 6);
            Assert.That(state.GetDueBookOrders(8)[0].OriginLots[0].Kind,
                Is.EqualTo((int)ItemOriginKind.Invalid));
            Assert.That(TryDeliver(state, 8), Is.True);
            Assert.That(_inventory.TryPrepareRemoval("scroll", 1, out _), Is.False);
        }

        [Test]
        public void FailedCreatureAnalysis_RestoresPurchasedPartInFullInventory()
        {
            Assert.That(_inventory.TryAddPurchasedItem("monster_part", 1, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem("filler", InventoryManager.DefaultCapacity - 1).Success,
                Is.True);
            var before = _inventory.CaptureSaveData();
            Assert.That(NpcServiceRuntime.TryRunCreatureAnalysis(_inventory, "monster_part", () => false),
                Is.False);
            Assert.That(_inventory.GetAmount("monster_part"), Is.EqualTo(1));
            Assert.That(_inventory.CaptureSaveData().Slots[0].OriginLots[0].UnitPaidBasisGold,
                Is.EqualTo(before.Slots[0].OriginLots[0].UnitPaidBasisGold));
            Assert.That(_inventory.CaptureSaveData().Slots[0].OriginLots[0].UnitResaleCapGold,
                Is.EqualTo(before.Slots[0].OriginLots[0].UnitResaleCapGold));
            Assert.That(NpcServiceRuntime.TryRunCreatureAnalysis(_inventory, "monster_part", () => true),
                Is.True);
            Assert.That(_inventory.GetAmount("monster_part"), Is.Zero);
        }

        private bool TryDeliver(NpcServicesPendingState state, int day)
        {
            var due = state.GetDueBookOrders(day);
            if (due.Count == 0) return false;
            var order = due[0];
            var port = (IOriginInventoryRuntime)_inventory;
            if (!port.TryStageOriginAddition(order.DeliveredBookItemId, null, 1,
                order.OriginLots, out var token)) return false;
            if (!state.TryCompleteBookOrder(order.OrderId))
            {
                port.TryRollbackOriginAddition(token);
                return false;
            }
            return port.TryCommitOriginAddition(token);
        }

        private static ItemDataSO Item(string id, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.MaxStack = maxStack;
            return item;
        }

        private sealed class BookContext : INpcServiceContext
        {
            public int Gold = 200;
            public bool FriendshipAtLeast(string npcId, int level) => true;
            public bool FlagIsSet(string flagId) => true;
            public int CurrentGold() => Gold;
            public bool SpendGold(int amount)
            {
                if (Gold < amount) return false;
                Gold -= amount;
                return true;
            }
            public bool HasItem(string itemId, int amount) => false;
            public bool RemoveItem(string itemId, int amount) => false;
        }

        private sealed class BookEffects : INpcServiceEffects
        {
            public string BookId;
            public bool TryCreatureAnalysis() => false;
            public bool ApplyRepairDiscount(float fraction) => false;
            public bool ApplyThermalBath() => false;
            public bool ApplyPremiumPasture(int days) => false;
            public string ResolveOrderedBookItemId() => BookId;
            public string ResolveHuntContractTarget() => null;
            public bool RegisterHuntContract(string target, int multiplier) => false;
            public bool ApplyDailyDish() => false;
            public bool TryIdentifyRelic() => false;
        }
    }
}
