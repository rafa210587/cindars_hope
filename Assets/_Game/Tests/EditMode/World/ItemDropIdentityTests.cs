using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    public sealed class ItemDropIdentityTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var value in _created)
                if (value != null) Object.DestroyImmediate(value);
            _created.Clear();
        }

        [Test]
        public void CraftedGear_DropAndPickup_PreservesInstanceAndDurabilityMetadata()
        {
            var gear = Item("gear_drop", 1);
            var database = Database(gear);
            var inventory = Manager(database);
            var spawner = Spawner(inventory, database);
            var tracker = new EquipmentDurabilityTracker();
            tracker.InitializeEquipment("gear_drop#crafted-9", 120);
            Assert.That(inventory.TryAddItemInstance("gear_drop", "gear_drop#crafted-9").Success, Is.True);

            Assert.That(inventory.DropItem(0, Vector3.zero), Is.True);
            var pickup = LastPickup(spawner);
            Assert.That(pickup.ItemInstanceId, Is.EqualTo("gear_drop#crafted-9"));
            pickup.Interact(null);

            Assert.That(pickup.IsCollected, Is.True);
            Assert.That(inventory.TryGetSlot(0, out var restored), Is.True);
            Assert.That(restored.ItemInstanceId, Is.EqualTo("gear_drop#crafted-9"));
            Assert.That(tracker.GetDurability("gear_drop#crafted-9").MaxDurability, Is.EqualTo(120));
        }

        [Test]
        public void CommonStack_DropAndPickup_RemainsAggregatedWithoutInstanceIdentity()
        {
            var wood = Item("wood_drop", 20);
            var database = Database(wood);
            var inventory = Manager(database);
            var spawner = Spawner(inventory, database);
            inventory.AddItem("wood_drop", 5);

            Assert.That(inventory.DropItem(0, Vector3.zero), Is.True);
            var pickup = LastPickup(spawner);
            Assert.That(pickup.ItemInstanceId, Is.Empty);
            pickup.Interact(null);

            Assert.That(inventory.GetAmount("wood_drop"), Is.EqualTo(5));
            Assert.That(inventory.TryGetSlot(0, out var restored), Is.True);
            Assert.That(restored.ItemInstanceId, Is.Empty);
        }

        [Test]
        public void IndividualPickup_WhenInventoryIsFull_RemainsInWorldWithSameIdentity()
        {
            var gear = Item("gear_full", 1);
            var blocker = Item("blocker_full", 1);
            var database = Database(gear, blocker);
            var inventory = Manager(database);
            Assert.That(inventory.AddItem("blocker_full", InventoryManager.DefaultCapacity), Is.True);
            var go = new GameObject("FullInventoryPickup"); _created.Add(go);
            var pickup = go.AddComponent<ItemPickup>();
            pickup.Configure(9001, "gear_full", "gear_full#crafted-11", 1, inventory);

            pickup.Interact(null);

            Assert.That(pickup.IsCollected, Is.False);
            Assert.That(pickup.ItemInstanceId, Is.EqualTo("gear_full#crafted-11"));
            Assert.That(inventory.GetAmount("gear_full"), Is.Zero);
        }

        private InventoryManager Manager(ItemDatabaseSO database)
        {
            var go = new GameObject("DropInventoryTest"); _created.Add(go);
            var manager = go.AddComponent<InventoryManager>(); manager.Initialize(database); return manager;
        }

        private ItemDropSpawner Spawner(InventoryManager inventory, ItemDatabaseSO database)
        {
            var spawner = ItemDropSpawner.Install(null);
            _created.Add(spawner.gameObject);
            spawner.Initialize(inventory, database);
            return spawner;
        }

        private ItemDatabaseSO Database(params ItemDataSO[] items)
        {
            var database = ScriptableObject.CreateInstance<ItemDatabaseSO>(); _created.Add(database);
            foreach (var item in items) _created.Add(item);
            var type = typeof(DataRegistrySO<ItemDataSO>);
            type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(database, items);
            type.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(database, null);
            return database;
        }

        private static ItemDataSO Item(string id, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>(); item.Id = id; item.MaxStack = maxStack; return item;
        }

        private ItemPickup LastPickup(ItemDropSpawner spawner)
        {
            var field = typeof(ItemDropSpawner).GetField("_droppedPickups", BindingFlags.Instance | BindingFlags.NonPublic);
            var pickups = (List<ItemPickup>)field.GetValue(spawner);
            var pickup = pickups[pickups.Count - 1]; _created.Add(pickup.gameObject); return pickup;
        }
    }
}
