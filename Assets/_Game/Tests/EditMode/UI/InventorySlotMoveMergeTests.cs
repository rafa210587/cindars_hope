using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.EditMode.UI
{
    public sealed class InventorySlotMoveMergeTests
    {
        private GameObject _inventoryHost;
        private InventoryManager _inventory;
        private ItemDatabaseSO _database;
        private ItemDataSO _wood;
        private ItemDataSO _stone;
        private ItemDataSO _sword;

        [SetUp]
        public void SetUp()
        {
            _wood = CreateItem("wood", 10);
            _stone = CreateItem("stone", 10);
            _sword = CreateItem("sword", 1);
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            SetRegistryItems(_database, _wood, _stone, _sword);
            _inventoryHost = new GameObject("inventory-slot-move-merge-tests");
            _inventory = _inventoryHost.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_inventoryHost);
            UnityEngine.Object.DestroyImmediate(_database);
            UnityEngine.Object.DestroyImmediate(_wood);
            UnityEngine.Object.DestroyImmediate(_stone);
            UnityEngine.Object.DestroyImmediate(_sword);
        }

        [Test]
        public void MoveToEmptySlot_MovesWholeStackAndClearsSource()
        {
            RestoreSlots(Slot(0, "wood", 4));

            Assert.That(_inventory.TryMoveOrMergeSlot(0, 1, out var failureReason), Is.True, failureReason);
            AssertSlot(0, string.Empty, 0);
            AssertSlot(1, "wood", 4);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(4));
        }

        [Test]
        public void MergeSameItem_TransfersOnlyAvailableCapacityAndConservesTotal()
        {
            RestoreSlots(Slot(0, "wood", 6), Slot(1, "wood", 8));

            Assert.That(_inventory.TryMoveOrMergeSlot(0, 1, out var failureReason), Is.True, failureReason);
            AssertSlot(0, "wood", 4);
            AssertSlot(1, "wood", 10);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(14));
        }

        [Test]
        public void MergeFullStack_FailsWithoutMutatingEitherSlot()
        {
            RestoreSlots(Slot(0, "wood", 2), Slot(1, "wood", 10));

            Assert.That(_inventory.TryMoveOrMergeSlot(0, 1, out var failureReason), Is.False);
            Assert.That(failureReason, Is.Not.Empty);
            AssertSlot(0, "wood", 2);
            AssertSlot(1, "wood", 10);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(12));
        }

        [Test]
        public void SwapDifferentItems_SwapsBothSlotsWithoutChangingTotals()
        {
            RestoreSlots(Slot(0, "wood", 3), Slot(1, "stone", 5));

            Assert.That(_inventory.TryMoveOrMergeSlot(0, 1, out var failureReason), Is.True, failureReason);
            AssertSlot(0, "stone", 5);
            AssertSlot(1, "wood", 3);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(3));
            Assert.That(_inventory.GetAmount("stone"), Is.EqualTo(5));
        }

        [Test]
        public void MoveWithEquippedSlot_FailsWithoutMutatingSlots()
        {
            RestoreSlots(Slot(0, "wood", 3, true), Slot(1, "stone", 5));

            Assert.That(_inventory.TryMoveOrMergeSlot(0, 1, out var failureReason), Is.False);
            Assert.That(failureReason, Is.Not.Empty);
            AssertSlot(0, "wood", 3, true);
            AssertSlot(1, "stone", 5);
        }

        [Test]
        public void InvalidDestination_FailsWithoutMutatingSource()
        {
            RestoreSlots(Slot(0, "wood", 3));

            Assert.That(_inventory.TryMoveOrMergeSlot(0, InventoryManager.DefaultCapacity, out var failureReason), Is.False);
            Assert.That(failureReason, Is.Not.Empty);
            AssertSlot(0, "wood", 3);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(3));
        }

        [Test]
        public void Swap_PublishesSourceThenDestinationWithConsistentAggregate()
        {
            RestoreSlots(Slot(0, "wood", 3), Slot(1, "stone", 5));
            var events = new List<InventoryChangedEvent>();
            var observedSlots = new List<string>();
            Action<InventoryChangedEvent> onChanged = change =>
            {
                events.Add(change);
                observedSlots.Add(_inventory.Slots[0].ItemId + ":" + _inventory.GetAmount("wood"));
            };
            GameEventBus.Subscribe(onChanged);
            try
            {
                Assert.That(_inventory.TryMoveOrMergeSlot(0, 1, out _), Is.True);
                Assert.That(events.Count, Is.EqualTo(2));
                Assert.That(events[0].ItemId, Is.EqualTo("wood"));
                Assert.That(events[1].ItemId, Is.EqualTo("stone"));
                Assert.That(events[0].Delta, Is.Zero);
                Assert.That(events[1].Delta, Is.Zero);
                Assert.That(events[0].NewAmount, Is.EqualTo(3));
                Assert.That(events[1].NewAmount, Is.EqualTo(5));
                Assert.That(observedSlots, Is.EqualTo(new[] { "stone:3", "stone:3" }));
            }
            finally
            {
                GameEventBus.Unsubscribe(onChanged);
            }
        }

        [Test]
        public void Merge_PublishesOnceAndRejectedRetryPublishesNothing()
        {
            RestoreSlots(Slot(0, "wood", 6), Slot(1, "wood", 8));
            var events = new List<InventoryChangedEvent>();
            Action<InventoryChangedEvent> onChanged = events.Add;
            GameEventBus.Subscribe(onChanged);
            try
            {
                Assert.That(_inventory.TryMoveOrMergeSlot(0, 1, out _), Is.True);
                Assert.That(_inventory.TryMoveOrMergeSlot(0, 1, out _), Is.False);
                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].ItemId, Is.EqualTo("wood"));
                Assert.That(events[0].Delta, Is.Zero);
                Assert.That(events[0].NewAmount, Is.EqualTo(14));
            }
            finally
            {
                GameEventBus.Unsubscribe(onChanged);
            }
        }

        [Test]
        public void SplitOddStack_PreservesTotalAndPublishesOneRefresh()
        {
            RestoreSlots(Slot(0, "wood", 5));
            var events = new List<InventoryChangedEvent>();
            Action<InventoryChangedEvent> onChanged = events.Add;
            GameEventBus.Subscribe(onChanged);
            try
            {
                Assert.That(_inventory.SplitSlot(0), Is.True);
                AssertSlot(0, "wood", 3);
                AssertSlot(1, "wood", 2);
                Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(5));
                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].Delta, Is.Zero);
                Assert.That(events[0].NewAmount, Is.EqualTo(5));
            }
            finally
            {
                GameEventBus.Unsubscribe(onChanged);
            }
        }

        [Test]
        public void Add_FillsPartialStackBeforeEarlierEmptySlot_ThenPublishesConsistentTotal()
        {
            RestoreSlots(Slot(1, "wood", 8, true));
            var events = new List<InventoryChangedEvent>();
            var observed = new List<string>();
            Action<InventoryChangedEvent> onChanged = change =>
            {
                events.Add(change);
                observed.Add(_inventory.Slots[0].Amount + ":" + _inventory.Slots[1].Amount + ":" + _inventory.GetAmount("wood"));
            };
            GameEventBus.Subscribe(onChanged);
            try
            {
                Assert.That(_inventory.AddItem("wood", 5), Is.True);
                AssertSlot(0, "wood", 3);
                AssertSlot(1, "wood", 10, true);
                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].Delta, Is.EqualTo(5));
                Assert.That(events[0].NewAmount, Is.EqualTo(13));
                Assert.That(observed, Is.EqualTo(new[] { "3:10:13" }));
            }
            finally { GameEventBus.Unsubscribe(onChanged); }
        }

        [Test]
        public void AddItemInstance_RequiresNonStackableCanonicalUniqueIdentity()
        {
            Assert.That(_inventory.TryAddItemInstance("sword", "sword#crafted-1").Success, Is.True);
            Assert.That(_inventory.Slots[0].ItemInstanceId, Is.EqualTo("sword#crafted-1"));
            Assert.That(_inventory.Slots[0].EffectiveItemInstanceId, Is.EqualTo("sword#crafted-1"));

            Assert.That(_inventory.TryAddItemInstance("sword", "sword#crafted-1").Success, Is.False);
            Assert.That(_inventory.TryAddItemInstance("wood", "wood#crafted-1").Success, Is.False);
            Assert.That(_inventory.TryAddItemInstance("sword", "foreign#crafted-2").Success, Is.False);
            Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(1));
        }

        [Test]
        public void Identity_RoundTripsAndNormalNonStackableGetsStableIdentity()
        {
            Assert.That(_inventory.TryAddItemInstance("sword", "sword#crafted-7").Success, Is.True);
            Assert.That(_inventory.AddItem("sword", 1), Is.True);
            var saved = _inventory.CaptureSaveData();

            _inventory.RestoreFromSaveData(saved);

            Assert.That(_inventory.Slots[0].ItemInstanceId, Is.EqualTo("sword#crafted-7"));
            Assert.That(_inventory.Slots[0].EffectiveItemInstanceId, Is.EqualTo("sword#crafted-7"));
            Assert.That(_inventory.Slots[1].ItemInstanceId, Is.EqualTo("sword#inventory-1"));
            Assert.That(_inventory.Slots[1].EffectiveItemInstanceId, Is.EqualTo("sword#inventory-1"));
        }

        [Test]
        public void NormalNonStackableMultiAdd_IsAtomicUniqueAndSequenceRoundTrips()
        {
            Assert.That(_inventory.TryAddItem("sword", 2).Success, Is.True);
            Assert.That(_inventory.Slots[0].ItemInstanceId, Is.EqualTo("sword#inventory-1"));
            Assert.That(_inventory.Slots[1].ItemInstanceId, Is.EqualTo("sword#inventory-2"));

            var saved = _inventory.CaptureSaveData();
            Assert.That(saved.NextItemInstanceSequence, Is.EqualTo(3));
            _inventory.RestoreFromSaveData(saved);
            Assert.That(_inventory.AddItem("sword", 1), Is.True);
            Assert.That(_inventory.Slots[2].ItemInstanceId, Is.EqualTo("sword#inventory-3"));
        }

        [Test]
        public void RestoreLegacyNonStackableWithoutIdentity_IsDeterministicAcrossReload()
        {
            var legacy = new InventorySaveData { Capacity = InventoryManager.DefaultCapacity };
            legacy.Slots.Add(Slot(4, "sword", 1));

            _inventory.RestoreFromSaveData(legacy);
            var first = _inventory.Slots[4].ItemInstanceId;
            Assert.That(first, Is.EqualTo("sword#inventory-1"));

            _inventory.RestoreFromSaveData(legacy);
            Assert.That(_inventory.Slots[4].ItemInstanceId, Is.EqualTo(first));
        }

        [Test]
        public void RestoreAdvancesPastSavedInventoryIdentity_AndStackablesStayIdentityFree()
        {
            RestoreSlots(Slot(0, "sword", 1, instanceId: "sword#inventory-12"));
            Assert.That(_inventory.AddItem("sword", 1), Is.True);
            Assert.That(_inventory.Slots[1].ItemInstanceId, Is.EqualTo("sword#inventory-13"));

            Assert.That(_inventory.AddItem("wood", 2), Is.True);
            Assert.That(_inventory.Slots[2].ItemInstanceId, Is.Empty);
        }

        [Test]
        public void NormalNonStackableMultiAdd_FullInventoryFailsWithoutConsumingSequence()
        {
            var occupied = new InventorySlotSaveData[InventoryManager.MaxCapacity - 1];
            for (var index = 0; index < occupied.Length; index++)
                occupied[index] = Slot(index, "wood", 1);
            RestoreSlots(occupied);

            Assert.That(_inventory.TryAddItem("sword", 2).Success, Is.False);
            Assert.That(_inventory.GetAmount("sword"), Is.Zero);
            Assert.That(_inventory.TryAddItem("sword", 1).Success, Is.True);
            Assert.That(_inventory.Slots[InventoryManager.MaxCapacity - 1].ItemInstanceId,
                Is.EqualTo("sword#inventory-1"));
        }

        [Test]
        public void Restore_DuplicateExplicitIdentityKeepsOnlyFirstInstance()
        {
            LogAssert.Expect(LogType.Warning,
                "InventoryManager ignored duplicate saved item instance id 'sword#crafted-11'.");
            RestoreSlots(
                Slot(0, "sword", 1, instanceId: "sword#crafted-11"),
                Slot(1, "sword", 1, instanceId: "sword#crafted-11"));

            Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(1));
            Assert.That(_inventory.Slots[0].ItemInstanceId, Is.EqualTo("sword#crafted-11"));
            Assert.That(_inventory.Slots[1].IsEmpty, Is.True);
        }

        [Test]
        public void MoveUniqueItem_PreservesIdentityAndOrdinaryStacksRemainIdentityFree()
        {
            Assert.That(_inventory.TryAddItemInstance("sword", "sword#crafted-9").Success, Is.True);
            Assert.That(_inventory.AddItem("wood", 2), Is.True);

            Assert.That(_inventory.TryMoveOrMergeSlot(0, 3, out var reason), Is.True, reason);

            Assert.That(_inventory.Slots[0].IsEmpty, Is.True);
            Assert.That(_inventory.Slots[3].ItemInstanceId, Is.EqualTo("sword#crafted-9"));
            Assert.That(_inventory.Slots[1].ItemInstanceId, Is.Empty);
            Assert.That(_inventory.GetAllItems().Single(item => item.ItemId == "wood").ItemInstanceId, Is.Empty);
        }

        [Test]
        public void Remove_ConsumesLastStacksFirst_ThenPublishesConsistentTotal()
        {
            RestoreSlots(Slot(0, "wood", 5), Slot(2, "wood", 2, true));
            var events = new List<InventoryChangedEvent>();
            var observed = new List<string>();
            Action<InventoryChangedEvent> onChanged = change =>
            {
                events.Add(change);
                observed.Add(_inventory.Slots[0].Amount + ":" + _inventory.Slots[2].IsEmpty + ":" + _inventory.GetAmount("wood"));
            };
            GameEventBus.Subscribe(onChanged);
            try
            {
                Assert.That(_inventory.RemoveItem("wood", 3), Is.True);
                AssertSlot(0, "wood", 4);
                AssertSlot(2, string.Empty, 0);
                Assert.That(_inventory.Slots[2].EquipmentBindingId, Is.Empty);
                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].Delta, Is.EqualTo(-3));
                Assert.That(events[0].NewAmount, Is.EqualTo(4));
                Assert.That(observed, Is.EqualTo(new[] { "4:True:4" }));
            }
            finally { GameEventBus.Unsubscribe(onChanged); }
        }

        [Test]
        public void RejectedAddRemoveAndUnknownItem_DoNotMutateOrPublish()
        {
            var slots = new InventorySlotSaveData[InventoryManager.DefaultCapacity];
            for (int index = 0; index < slots.Length; index++) slots[index] = Slot(index, "wood", 10);
            RestoreSlots(slots);
            var events = new List<InventoryChangedEvent>();
            Action<InventoryChangedEvent> onChanged = events.Add;
            GameEventBus.Subscribe(onChanged);
            try
            {
                Assert.That(_inventory.AddItem("wood", 1), Is.False);
                Assert.That(_inventory.RemoveItem("wood", 401), Is.False);
                LogAssert.Expect(LogType.Warning, "InventoryManager rejected unknown item id 'missing'.");
                Assert.That(_inventory.AddItem("missing", 1), Is.False);
                Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(400));
                foreach (var slot in _inventory.Slots) Assert.That(slot.Amount, Is.EqualTo(10));
                Assert.That(events, Is.Empty);
            }
            finally { GameEventBus.Unsubscribe(onChanged); }
        }

        [Test]
        public void Remove_StaleAggregateCannotConsumePartialContentsOrPublish()
        {
            RestoreSlots(Slot(0, "wood", 5), Slot(2, "wood", 2, true));
            _inventory.Slots[0].Amount = 1;
            var events = new List<InventoryChangedEvent>();
            Action<InventoryChangedEvent> onChanged = events.Add;
            GameEventBus.Subscribe(onChanged);
            try
            {
                Assert.That(_inventory.RemoveItem("wood", 4), Is.False);
                AssertSlot(0, "wood", 1);
                AssertSlot(2, "wood", 2, true);
                Assert.That(_inventory.Slots[2].EquipmentBindingId, Is.EqualTo("equipment-slot:Hand"));
                Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(7));
                Assert.That(events, Is.Empty);
            }
            finally { GameEventBus.Unsubscribe(onChanged); }
        }

        private void RestoreSlots(params InventorySlotSaveData[] slots)
        {
            var data = new InventorySaveData { Capacity = InventoryManager.DefaultCapacity };
            data.Slots.AddRange(slots);
            _inventory.RestoreFromSaveData(data);
        }

        private void AssertSlot(int index, string itemId, int amount, bool equipped = false)
        {
            Assert.That(_inventory.TryGetSlot(index, out var slot), Is.True);
            Assert.That(slot.SlotIndex, Is.EqualTo(index));
            Assert.That(slot.ItemId, Is.EqualTo(itemId));
            Assert.That(slot.Amount, Is.EqualTo(amount));
            Assert.That(slot.IsEquipped, Is.EqualTo(equipped));
        }

        private static InventorySlotSaveData Slot(int index, string itemId, int amount, bool equipped = false,
            string instanceId = "")
        {
            return new InventorySlotSaveData
            {
                SlotIndex = index,
                ItemId = itemId,
                ItemInstanceId = instanceId,
                Amount = amount,
                IsEquipped = equipped,
                EquipmentBindingId = equipped ? "equipment-slot:Hand" : string.Empty
            };
        }

        private static ItemDataSO CreateItem(string id, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.MaxStack = maxStack;
            return item;
        }

        private static void SetRegistryItems(ItemDatabaseSO database, params ItemDataSO[] items)
        {
            var registryType = typeof(DataRegistrySO<ItemDataSO>);
            var itemsField = registryType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var rebuildIndex = registryType.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(itemsField, Is.Not.Null);
            Assert.That(rebuildIndex, Is.Not.Null);
            itemsField.SetValue(database, items);
            rebuildIndex.Invoke(database, null);
        }
    }
}
