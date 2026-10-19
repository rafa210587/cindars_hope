using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class ItemDropIdentityPlayModeTests
    {
        [UnityTest]
        public IEnumerator CraftedDropPickup_RetainsInstanceKeyAndDurabilityMax()
        {
            var gear = ScriptableObject.CreateInstance<ItemDataSO>();
            gear.Id = "gear_drop_play"; gear.MaxStack = 1;
            var database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registryType = typeof(DataRegistrySO<ItemDataSO>);
            registryType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(database, new[] { gear });
            registryType.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(database, null);
            var inventoryGo = new GameObject("DropIdentityPlayInventory");
            var inventory = inventoryGo.AddComponent<InventoryManager>();
            inventory.Initialize(database);
            var spawner = ItemDropSpawner.Install(null);
            spawner.Initialize(inventory, database);
            var tracker = new EquipmentDurabilityTracker();
            tracker.InitializeEquipment("gear_drop_play#crafted-27", 175);
            inventory.TryAddItemInstance("gear_drop_play", "gear_drop_play#crafted-27");

            Assert.That(inventory.DropItem(0, Vector3.zero), Is.True);
            var field = typeof(ItemDropSpawner).GetField("_droppedPickups", BindingFlags.Instance | BindingFlags.NonPublic);
            var pickups = (List<ItemPickup>)field.GetValue(spawner);
            var pickup = pickups[pickups.Count - 1];
            pickup.Interact(null);

            Assert.That(pickup.IsCollected, Is.True);
            Assert.That(inventory.TryGetSlot(0, out var restored), Is.True);
            Assert.That(restored.ItemInstanceId, Is.EqualTo("gear_drop_play#crafted-27"));
            Assert.That(tracker.GetDurability(restored.ItemInstanceId).MaxDurability, Is.EqualTo(175));

            Object.Destroy(pickup.gameObject);
            Object.Destroy(spawner.gameObject);
            Object.Destroy(inventoryGo);
            Object.Destroy(database);
            Object.Destroy(gear);
            yield return null;
        }
    }
}
