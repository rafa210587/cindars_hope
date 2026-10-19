using System;
using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.EditMode.Inventory
{
    public sealed class PurchasedOriginInventoryTests
    {
        private GameObject _host;
        private InventoryManager _inventory;
        private ItemDatabaseSO _database;
        private ItemDataSO _wood;
        private ItemDataSO _sword;

        [SetUp]
        public void SetUp()
        {
            _wood = Item("wood", 10);
            _sword = Item("sword", 1);
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registryType = typeof(DataRegistrySO<ItemDataSO>);
            registryType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _wood, _sword });
            registryType.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-inventory-tests");
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_host);
            UnityEngine.Object.DestroyImmediate(_database);
            UnityEngine.Object.DestroyImmediate(_wood);
            UnityEngine.Object.DestroyImmediate(_sword);
        }

        [Test]
        public void SplitMergeAndV6RoundTrip_ConservePaidBasisAndEarnedUnits()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            Assert.That(_inventory.SplitSlot(0), Is.True);
            Assert.That(_inventory.TryMoveOrMergeSlot(1, 0, out _), Is.True);

            var saved = _inventory.CaptureSaveData();
            Assert.That(saved.OriginSchemaVersion, Is.EqualTo(1));
            _inventory.RestoreFromSaveData(saved);

            var purchasedCount = 0;
            var paidBasis = 0;
            var earnedCount = 0;
            foreach (var slot in _inventory.Slots)
            {
                if (slot.IsEmpty) continue;
                foreach (var lot in slot.OriginLots)
                {
                    if (lot.Kind == ItemOriginKind.Purchased)
                    { purchasedCount += lot.Quantity; paidBasis += lot.Quantity * lot.UnitPaidBasisGold; }
                    if (lot.Kind == ItemOriginKind.Earned) earnedCount += lot.Quantity;
                }
            }
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(5));
            Assert.That(purchasedCount, Is.EqualTo(3));
            Assert.That(paidBasis, Is.EqualTo(17));
            Assert.That(earnedCount, Is.EqualTo(2));
        }

        [Test]
        public void QuoteSelection_SurvivesExactRollbackAndRejectsStaleCommit()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            Assert.That(_inventory.TryPrepareRemoval("wood", 3, out var selection), Is.True);
            Assert.That(selection.Slices[0].Kind, Is.EqualTo(ItemOriginKind.Earned));
            Assert.That(selection.Slices[0].Quantity, Is.EqualTo(2));
            var before = _inventory.CaptureSaveData();

            Assert.That(_inventory.TryCommitRemoval(selection, false), Is.True);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(2));
            Assert.That(_inventory.TryRestoreRemoval(selection, false), Is.True);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(5));
            Assert.That(_inventory.Slots[0].OriginLots.Count, Is.EqualTo(before.Slots[0].OriginLots.Count));
            Assert.That(_inventory.TryCommitRemoval(selection, false), Is.False);
        }

        [Test]
        public void CorruptV6Lot_BecomesInvalidWhileOtherPurchasedLotRemainsSelectable()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 3, 17).Success, Is.True);
            var saved = _inventory.CaptureSaveData();
            saved.OriginSchemaVersion = 1;
            saved.Slots[0].OriginLots[0].Kind = 0;
            _inventory.RestoreFromSaveData(saved);

            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(3));
            Assert.That(_inventory.Slots[0].OriginLots[0].Kind, Is.EqualTo(ItemOriginKind.Invalid));
            Assert.That(_inventory.GetEligibleAmount("wood"), Is.EqualTo(2));
            Assert.That(_inventory.TryPrepareRemoval("wood", 3, out _), Is.False);
        }

        [Test]
        public void CorruptPurchasedLots_KeepEarnedUnitsSellable()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            var saved = _inventory.CaptureSaveData();
            saved.OriginSchemaVersion = 1;
            saved.Slots[0].OriginLots[0].UnitPaidBasisGold = -1;
            saved.Slots[0].OriginLots[1].Kind = 99;

            _inventory.RestoreFromSaveData(saved);

            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(5));
            Assert.That(_inventory.GetEligibleAmount("wood"), Is.EqualTo(2));
            Assert.That(_inventory.TryPrepareRemoval("wood", 2, out var selection), Is.True);
            Assert.That(selection.Slices.Count, Is.EqualTo(1));
            Assert.That(selection.Slices[0].Kind, Is.EqualTo(ItemOriginKind.Earned));
            Assert.That(_inventory.TryCommitRemoval(selection, false), Is.True);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(3));
            Assert.That(_inventory.GetEligibleAmount("wood"), Is.Zero);
        }

        [Test]
        public void V6UnknownItem_RejectsRestoreBeforeClearingCurrentInventory()
        {
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            var saved = _inventory.CaptureSaveData();
            saved.OriginSchemaVersion = 1;
            saved.Slots[0].ItemId = "missing";
            Assert.That(_inventory.TryValidateRestoreFromSaveData(saved, out _), Is.False);
            LogAssert.Expect(LogType.Warning,
                "InventoryManager rejected inventory restore: Origin-enabled inventory has an unknown item or impossible slot.");
            _inventory.RestoreFromSaveData(saved);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(2));
        }

        [Test]
        public void PurchasedNonStackable_HasSeparateInstanceAndLotIdentities()
        {
            Assert.That(_inventory.TryAddPurchasedItem("sword", 2, 3).Success, Is.True);
            Assert.That(_inventory.Slots[0].ItemInstanceId,
                Is.Not.EqualTo(_inventory.Slots[1].ItemInstanceId));
            Assert.That(_inventory.Slots[0].OriginLots[0].UnitPaidBasisGold, Is.EqualTo(1));
            Assert.That(_inventory.Slots[1].OriginLots[0].UnitPaidBasisGold, Is.EqualTo(2));
        }

        [Test]
        public void PurchasedThreeForSeventeen_HasExactBasesAndResaleCaps()
        {
            var lots = ItemOriginLot.Purchased(3, 17);
            Assert.That(lots.Count, Is.EqualTo(2));
            Assert.That(lots[0].Quantity, Is.EqualTo(1));
            Assert.That(lots[0].UnitPaidBasisGold, Is.EqualTo(5));
            Assert.That(lots[0].UnitResaleCapGold, Is.EqualTo(4));
            Assert.That(lots[1].Quantity, Is.EqualTo(2));
            Assert.That(lots[1].UnitPaidBasisGold, Is.EqualTo(6));
            Assert.That(lots[1].UnitResaleCapGold, Is.EqualTo(5));
            Assert.That(ItemOriginLot.IsValid(lots, 3), Is.True);
        }

        [Test]
        public void PurchasedLotWithResaleCapAbovePaidBasis_FailsClosedPerLot()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            var saved = _inventory.CaptureSaveData();
            saved.OriginSchemaVersion = 1;
            saved.Slots[0].OriginLots[0].UnitResaleCapGold = 6; // Paid 5; max legal cap 4.

            _inventory.RestoreFromSaveData(saved);

            Assert.That(_inventory.Slots[0].OriginLots[0].Kind, Is.EqualTo(ItemOriginKind.Invalid));
            Assert.That(_inventory.GetEligibleAmount("wood"), Is.EqualTo(4));
            Assert.That(_inventory.TryPrepareRemoval("wood", 5, out _), Is.False);
        }

        [Test]
        public void PurchasedLotWithResaleCapBelowCanonical_FailsClosedPerLot()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            var saved = _inventory.CaptureSaveData();
            saved.OriginSchemaVersion = 1;
            saved.Slots[0].OriginLots[0].UnitResaleCapGold = 0; // Paid 5; canonical cap 4.

            _inventory.RestoreFromSaveData(saved);

            Assert.That(_inventory.Slots[0].OriginLots[0].Kind, Is.EqualTo(ItemOriginKind.Invalid));
            Assert.That(_inventory.GetEligibleAmount("wood"), Is.EqualTo(4));
            Assert.That(_inventory.TryPrepareRemoval("wood", 5, out _), Is.False);
        }

        [Test]
        public void SilentPurchasedAddRollback_RestoresMixedStackExactlyWithoutEvent()
        {
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            var before = _inventory.CaptureSaveData();
            var events = new List<InventoryChangedEvent>();
            Action<InventoryChangedEvent> onChanged = events.Add;
            GameEventBus.Subscribe(onChanged);
            try
            {
                var add = _inventory.TryAddPurchasedItem("wood", 3, 17, false, out var token);
                Assert.That(add.Success, Is.True);
                Assert.That(token, Is.Not.Null);
                Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(5));
                Assert.That(events, Is.Empty);
                Assert.That(_inventory.TryRollbackPurchasedAdd(token), Is.True);
                Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(2));
                Assert.That(_inventory.Slots[0].OriginLots.Count, Is.EqualTo(1));
                Assert.That(_inventory.Slots[0].OriginLots[0].Kind, Is.EqualTo(ItemOriginKind.Earned));
                Assert.That(_inventory.Slots[0].Amount, Is.EqualTo(before.Slots[0].Amount));
                Assert.That(events, Is.Empty);
                Assert.That(_inventory.TryRollbackPurchasedAdd(token), Is.False);
            }
            finally { GameEventBus.Unsubscribe(onChanged); }
        }

        [Test]
        public void SilentPurchasedNonStackableRollback_ReusesUnspentInstanceSequence()
        {
            Assert.That(_inventory.TryAddPurchasedItem("sword", 2, 3, false, out var token).Success,
                Is.True);
            Assert.That(_inventory.Slots[0].ItemInstanceId, Is.EqualTo("sword#inventory-1"));
            Assert.That(_inventory.Slots[1].ItemInstanceId, Is.EqualTo("sword#inventory-2"));
            Assert.That(_inventory.TryRollbackPurchasedAdd(token), Is.True);
            Assert.That(_inventory.GetAmount("sword"), Is.Zero);
            Assert.That(_inventory.TryAddItem("sword", 1).Success, Is.True);
            Assert.That(_inventory.Slots[0].ItemInstanceId, Is.EqualTo("sword#inventory-1"));
        }

        [Test]
        public void BatchRemoval_CommitAndRollbackRestoreMixedStackAndInstanceExactly()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            Assert.That(_inventory.TryAddPurchasedItem("sword", 1, 10).Success, Is.True);
            var mixed = _inventory.CaptureSaveData();
            mixed.OriginSchemaVersion = 1;
            mixed.Slots[0].Amount++;
            mixed.Slots[0].OriginLots.Add(new ItemOriginLot(ItemOriginKind.Invalid, 1).ToSaveData());
            _inventory.RestoreFromSaveData(mixed);
            var before = _inventory.CaptureSaveData();
            var requests = new Dictionary<string, int> { ["wood"] = 4, ["sword"] = 1 };
            Assert.That(_inventory.TryPrepareBatchRemoval(requests, out var batch), Is.True);
            Assert.That(batch.Selections.Count, Is.EqualTo(2));
            var events = new List<InventoryChangedEvent>();
            Action<InventoryChangedEvent> onChanged = events.Add;
            GameEventBus.Subscribe(onChanged);
            InventoryBatchRemovalToken token;
            try
            {
                Assert.That(_inventory.TryCommitBatchRemoval(batch, out token), Is.True);
                Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(2));
                Assert.That(_inventory.GetEligibleAmount("wood"), Is.EqualTo(1));
                Assert.That(_inventory.Slots[0].OriginLots.Exists(lot =>
                    lot.Kind == ItemOriginKind.Invalid && lot.Quantity == 1), Is.True);
                Assert.That(_inventory.GetAmount("sword"), Is.Zero);
                Assert.That(events, Is.Empty);
                Assert.That(_inventory.TryRollbackBatchRemoval(token), Is.True);
                Assert.That(events, Is.Empty);
            }
            finally { GameEventBus.Unsubscribe(onChanged); }
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(6));
            Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(1));
            var after = _inventory.CaptureSaveData();
            Assert.That(after.Slots.Count, Is.EqualTo(before.Slots.Count));
            for (var index = 0; index < before.Slots.Count; index++)
            {
                Assert.That(after.Slots[index].ItemId, Is.EqualTo(before.Slots[index].ItemId));
                Assert.That(after.Slots[index].ItemInstanceId,
                    Is.EqualTo(before.Slots[index].ItemInstanceId));
                Assert.That(after.Slots[index].Amount, Is.EqualTo(before.Slots[index].Amount));
                Assert.That(after.Slots[index].OriginLots.Count,
                    Is.EqualTo(before.Slots[index].OriginLots.Count));
                for (var lot = 0; lot < before.Slots[index].OriginLots.Count; lot++)
                {
                    Assert.That(after.Slots[index].OriginLots[lot].Kind,
                        Is.EqualTo(before.Slots[index].OriginLots[lot].Kind));
                    Assert.That(after.Slots[index].OriginLots[lot].Quantity,
                        Is.EqualTo(before.Slots[index].OriginLots[lot].Quantity));
                    Assert.That(after.Slots[index].OriginLots[lot].UnitPaidBasisGold,
                        Is.EqualTo(before.Slots[index].OriginLots[lot].UnitPaidBasisGold));
                    Assert.That(after.Slots[index].OriginLots[lot].UnitResaleCapGold,
                        Is.EqualTo(before.Slots[index].OriginLots[lot].UnitResaleCapGold));
                }
            }
            Assert.That(_inventory.TryRollbackBatchRemoval(token), Is.False);
        }

        [Test]
        public void BatchRemoval_InvalidOrStalePlanNeverPartiallyConsumes()
        {
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            Assert.That(_inventory.TryAddItem("sword", 1).Success, Is.True);
            var impossible = new Dictionary<string, int> { ["wood"] = 2, ["sword"] = 2 };
            Assert.That(_inventory.TryPrepareBatchRemoval(impossible, out _), Is.False);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(2));
            Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(1));

            var valid = new Dictionary<string, int> { ["wood"] = 2, ["sword"] = 1 };
            Assert.That(_inventory.TryPrepareBatchRemoval(valid, out var batch), Is.True);
            Assert.That(_inventory.TryAddItem("wood", 1).Success, Is.True);
            Assert.That(_inventory.TryCommitBatchRemoval(batch, out _), Is.False);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(3));
            Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(1));
        }

        private static ItemDataSO Item(string id, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.MaxStack = maxStack;
            return item;
        }
    }
}
