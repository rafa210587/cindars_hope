using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Farm.Shipping;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public sealed class PurchasedOriginShippingTests
    {
        private GameObject _host;
        private ShippingBinRuntimeService _shipping;
        private PlayerManager _player;
        private ItemDataSO _item;
        private ItemDatabaseSO _database;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("PurchasedOriginShippingTests");
            _player = _host.AddComponent<PlayerManager>();
            _shipping = _host.AddComponent<ShippingBinRuntimeService>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            if (_database != null) Object.DestroyImmediate(_database);
            if (_item != null) Object.DestroyImmediate(_item);
        }

        [Test]
        public void DepositFromInventory_TransfersExactLotsAndLeavesInvalidUnit()
        {
            _item = ScriptableObject.CreateInstance<ItemDataSO>();
            _item.Id = "item_bread";
            _item.BaseValue = 20;
            _item.MaxStack = 99;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registry = typeof(DataRegistrySO<ItemDataSO>);
            registry.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _item });
            registry.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            var inventory = _host.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            var lots = ItemOriginLot.Purchased(3, 17);
            lots.Add(new ItemOriginLot(ItemOriginKind.Invalid, 1));
            Assert.That(inventory.TryAddItemWithLots("item_bread", 4, lots).Success, Is.True);

            var result = _shipping.DepositFromInventory(inventory, "item_bread",
                inventory.GetEligibleAmount("item_bread"), 0, 20f);
            Assert.That(result.Success, Is.True);
            Assert.That(inventory.GetAmount("item_bread"), Is.EqualTo(1));
            Assert.That(inventory.GetEligibleAmount("item_bread"), Is.Zero);
            Assert.That(result.CreatedEntry.Quantity, Is.EqualTo(3));
            Assert.That(result.CreatedEntry.OriginLots[0].UnitPaidBasisGold, Is.EqualTo(5));
            Assert.That(result.CreatedEntry.OriginLots[1].UnitPaidBasisGold, Is.EqualTo(6));
            Assert.That(result.CreatedEntry.PricePreview, Is.EqualTo(14f));
        }

        [Test]
        public void PurchasedRounding_CapsEachUnitAfterShippingMultiplier()
        {
            var entries = new List<PendingShippingEntry>();
            var service = new FarmShippingService(entries, new ShippingPriceResolver(),
                new ItemSellabilityProvider());
            var purchased = ItemOriginLot.Purchased(3, 17);
            var deposit = service.Deposit("bin", "item_bread", 3, 0, 20f, 1, purchased);

            Assert.That(deposit.Success, Is.True);
            Assert.That(deposit.CreatedEntry.PricePreview, Is.EqualTo(14f));
            var batch = service.ProcessDayBatch(2);
            Assert.That(batch.TotalGoldChecked, Is.EqualTo(14));
            Assert.That(batch.ItemCount, Is.EqualTo(3));
            Assert.That(entries[0].State, Is.EqualTo(ShippingEntryState.Pending));
            Assert.That(service.TryCommitBatch(batch), Is.True);
            Assert.That(service.ProcessDayBatch(2).Entries.Count, Is.Zero);
        }

        [Test]
        public void RawAndBatchOverflow_KeepEveryEntryPending()
        {
            var entries = new List<PendingShippingEntry>();
            var service = new FarmShippingService(entries, new ShippingPriceResolver(),
                new ItemSellabilityProvider());
            Assert.That(service.Deposit("bin", "item_bread", 1, 0,
                float.MaxValue, 1).Success, Is.False);
            Assert.That(entries.Count, Is.Zero);

            Assert.That(service.Deposit("bin", "item_bread", 1, 0,
                1_200_000_000f, 1).Success, Is.True);
            Assert.That(service.Deposit("bin", "item_bread", 1, 0,
                1_200_000_000f, 1).Success, Is.True);
            var batch = service.ProcessDayBatch(2);
            Assert.That(batch.State, Is.EqualTo(ShippingBatchState.FailedPartial));
            Assert.That(batch.Entries.Count, Is.Zero);
            Assert.That(entries[0].State, Is.EqualTo(ShippingEntryState.Pending));
            Assert.That(entries[1].State, Is.EqualTo(ShippingEntryState.Pending));
        }

        [Test]
        public void AbsentPlayerAndOverflow_RetryLaterWithoutSuccessEvent()
        {
            var deposited = _shipping.Deposit("item_bread", 1, 0, 20f);
            Assert.That(deposited.Success, Is.True);
            deposited.CreatedEntry.OriginLots = ItemOriginLot.Purchased(1, 17);
            _shipping.ProcessOvernight(2, null);
            Assert.That(deposited.CreatedEntry.State, Is.EqualTo(ShippingEntryState.Pending));

            _player.SetGold(int.MaxValue - 1);
            var successEvents = 0;
            using (GameEventBus.Subscribe<ShippingBatchProcessedEvent>(_ => successEvents++))
            {
                _shipping.ProcessOvernight(2, _player);
                Assert.That(deposited.CreatedEntry.State, Is.EqualTo(ShippingEntryState.Pending));
                Assert.That(_player.CurrentGold, Is.EqualTo(int.MaxValue - 1));
                Assert.That(successEvents, Is.Zero);

                _player.SetGold(0);
                _shipping.ProcessOvernight(3, _player);
                Assert.That(_player.CurrentGold, Is.EqualTo(16));
                Assert.That(successEvents, Is.EqualTo(1));
                _shipping.ProcessOvernight(3, _player);
                Assert.That(_player.CurrentGold, Is.EqualTo(16));
                Assert.That(successEvents, Is.EqualTo(1));
            }
        }

        [Test]
        public void GoldListenerSave_SeesProcessedShippingAndCreditedWallet()
        {
            var deposited = _shipping.Deposit("item_bread", 1, 0, 20f);
            deposited.CreatedEntry.OriginLots = ItemOriginLot.Purchased(1, 17);
            var events = 0;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ =>
            {
                events++;
                var saved = _shipping.CaptureSaveData();
                Assert.That(_player.CurrentGold, Is.EqualTo(16));
                Assert.That(saved.Entries[0].State, Is.EqualTo((int)ShippingEntryState.Processed));
                Assert.That(saved.Entries[0].ProcessedPaymentId, Is.Not.Empty);
                Assert.That(saved.Entries[0].OriginLots[0].UnitPaidBasisGold, Is.EqualTo(17));
            }))
            {
                _shipping.ProcessOvernight(2, _player);
            }
            Assert.That(events, Is.EqualTo(1));
            var roundTrip = JsonUtility.FromJson<PendingShippingSaveData>(
                JsonUtility.ToJson(_shipping.CaptureSaveData()));
            _shipping.RestoreFromSaveData(roundTrip);
            _shipping.ProcessOvernight(3, _player);
            Assert.That(_player.CurrentGold, Is.EqualTo(16));
        }

        [Test]
        public void CorruptEntryIsRetained_OtherEntryStillPays()
        {
            var good = _shipping.Deposit("item_bread", 1, 0, 20f).CreatedEntry;
            good.OriginLots = ItemOriginLot.Purchased(1, 17);
            var bad = _shipping.Deposit("item_bread", 1, 0, 20f).CreatedEntry;
            var saved = _shipping.CaptureSaveData();
            saved.Entries[1].OriginLots[0].Kind = 0;
            _shipping.RestoreFromSaveData(saved);

            Assert.That(_shipping.PendingEntries[1].State, Is.EqualTo(ShippingEntryState.Invalid));
            _shipping.ProcessOvernight(2, _player);
            Assert.That(_player.CurrentGold, Is.EqualTo(16));
            Assert.That(_shipping.PendingEntries[0].State, Is.EqualTo(ShippingEntryState.Processed));
            Assert.That(_shipping.PendingEntries[1].State, Is.EqualTo(ShippingEntryState.Invalid));
            Assert.That(_shipping.PendingEntries[1].Quantity, Is.EqualTo(1));
        }

        [Test]
        public void CorruptLotInOneEntry_RetainsOnlyAffectedUnits()
        {
            var entry = _shipping.Deposit("item_bread", 3, 0, 20f).CreatedEntry;
            entry.OriginLots = ItemOriginLot.Purchased(3, 17);
            var saved = _shipping.CaptureSaveData();
            saved.Entries[0].OriginLots[1].Kind = 0;
            _shipping.RestoreFromSaveData(saved);

            Assert.That(_shipping.PendingEntries.Count, Is.EqualTo(2));
            Assert.That(_shipping.PendingEntries[0].Quantity, Is.EqualTo(1));
            Assert.That(_shipping.PendingEntries[0].OriginLots[0].UnitPaidBasisGold, Is.EqualTo(5));
            Assert.That(_shipping.PendingEntries[1].Quantity, Is.EqualTo(2));
            Assert.That(_shipping.PendingEntries[1].State, Is.EqualTo(ShippingEntryState.Invalid));
            _shipping.ProcessOvernight(2, _player);
            Assert.That(_player.CurrentGold, Is.EqualTo(4));
            Assert.That(_shipping.PendingEntries[0].State, Is.EqualTo(ShippingEntryState.Processed));
            Assert.That(_shipping.PendingEntries[1].Quantity, Is.EqualTo(2));
        }
    }
}
