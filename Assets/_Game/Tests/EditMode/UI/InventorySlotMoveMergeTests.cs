using System;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.UI
{
    public sealed class InventorySlotMoveMergeTests
    {
        private GameObject _inventoryHost;
        private InventoryManager _inventory;
        private ItemDatabaseSO _database;
        private ItemDataSO _wood;
        private ItemDataSO _stone;

        [SetUp]
        public void SetUp()
        {
            _wood = CreateItem("wood", 10);
            _stone = CreateItem("stone", 10);
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            SetRegistryItems(_database, _wood, _stone);
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

        private static InventorySlotSaveData Slot(int index, string itemId, int amount, bool equipped = false)
        {
            return new InventorySlotSaveData
            {
                SlotIndex = index,
                ItemId = itemId,
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
