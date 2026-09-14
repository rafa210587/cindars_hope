using System.Reflection;
using System.Collections.Generic;
using CindarsHope.Core.Data;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    public sealed class PurchasedOriginTransportTests
    {
        private GameObject _host;
        private InventoryManager _inventory;
        private ItemDropSpawner _spawner;
        private ItemDatabaseSO _database;
        private ItemDataSO _wood;

        [SetUp]
        public void SetUp()
        {
            _wood = ScriptableObject.CreateInstance<ItemDataSO>();
            _wood.Id = "wood";
            _wood.MaxStack = 10;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registry = typeof(DataRegistrySO<ItemDataSO>);
            registry.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _wood });
            registry.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-drop-test");
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
            _spawner = ItemDropSpawner.Install(_host.transform);
            _spawner.Initialize(_inventory, _database);
        }

        [TearDown]
        public void TearDown()
        {
            if (_spawner != null) _spawner.Clear();
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_database);
            Object.DestroyImmediate(_wood);
        }

        [Test]
        public void DropSaveReloadPickup_PreservesPurchasedBasesAndEarnedUnits()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 3, 17).Success, Is.True);
            Assert.That(_inventory.TryAddItem("wood", 2).Success, Is.True);
            Assert.That(_inventory.DropItem(0, Vector3.zero), Is.True);
            Assert.That(_inventory.GetAmount("wood"), Is.Zero);

            var provider = new WorldSectionProvider(null, null);
            var saved = (WorldSaveData)provider.Capture(new GameSaveData());
            Assert.That(saved.Pickups.Count, Is.EqualTo(1));
            Assert.That(saved.Pickups[0].IsDynamic, Is.True);
            Assert.That(saved.Pickups[0].OriginSchemaVersion, Is.EqualTo(1));
            var roundTrip = JsonUtility.FromJson<WorldSaveData>(JsonUtility.ToJson(saved));
            provider.Restore(roundTrip);
            var restored = _spawner.CaptureDynamicPickupSaveData();
            Assert.That(restored.Count, Is.EqualTo(1));
            Assert.That(restored[0].RuntimeGeneratedPickupId,
                Is.EqualTo(saved.Pickups[0].RuntimeGeneratedPickupId));

            var worldPickup = _spawner.GetDynamicPickup(restored[0].PickupIndex);
            Assert.That(worldPickup, Is.Not.Null);
            worldPickup.Interact(null);
            Assert.That(worldPickup.IsCollected, Is.True);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(5));
            var paid = 0;
            var earned = 0;
            foreach (var slot in _inventory.Slots)
                foreach (var lot in slot.OriginLots)
                {
                    if (lot.Kind == ItemOriginKind.Purchased)
                        paid += lot.Quantity * lot.UnitPaidBasisGold;
                    if (lot.Kind == ItemOriginKind.Earned) earned += lot.Quantity;
                }
            Assert.That(paid, Is.EqualTo(17));
            Assert.That(earned, Is.EqualTo(2));
        }

        [Test]
        public void CorruptSavedPickup_RemainsInvalidAfterReloadAndPickup()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 1, 5).Success, Is.True);
            Assert.That(_inventory.DropItem(0, Vector3.zero), Is.True);
            var provider = new WorldSectionProvider(null, null);
            var saved = (WorldSaveData)provider.Capture(new GameSaveData());
            saved.Pickups[0].OriginLots[0].Kind = 0;
            provider.Restore(saved);
            var worldPickup = _spawner.GetDynamicPickup(saved.Pickups[0].PickupIndex);
            Assert.That(worldPickup, Is.Not.Null);
            worldPickup.Interact(null);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(1));
            Assert.That(_inventory.GetEligibleAmount("wood"), Is.Zero);
            Assert.That(_inventory.Slots[0].OriginLots[0].Kind, Is.EqualTo(ItemOriginKind.Invalid));
        }

        [Test]
        public void InventoryChangeListener_CapturesConsumedPickupBeforeSavingInventory()
        {
            Assert.That(_inventory.TryAddPurchasedItem("wood", 1, 5).Success, Is.True);
            Assert.That(_inventory.DropItem(0, Vector3.zero), Is.True);
            var provider = new WorldSectionProvider(null, null);
            var pending = (WorldSaveData)provider.Capture(new GameSaveData());
            var pickup = _spawner.GetDynamicPickup(pending.Pickups[0].PickupIndex);
            Assert.That(pickup, Is.Not.Null);
            InventorySaveData savedInventory = null;
            WorldSaveData savedWorld = null;
            using (GameEventBus.Subscribe<InventoryChangedEvent>(evt =>
            {
                if (evt.ItemId != "wood" || evt.Delta != 1) return;
                Assert.That(pickup.IsCollected, Is.True,
                    "save listener must not observe inventory grant with an available pickup");
                savedInventory = _inventory.CaptureSaveData();
                savedInventory.OriginSchemaVersion = 1;
                savedWorld = (WorldSaveData)provider.Capture(new GameSaveData());
            }))
            {
                pickup.Interact(null);
            }

            Assert.That(savedInventory, Is.Not.Null);
            Assert.That(savedWorld, Is.Not.Null);
            Assert.That(savedWorld.Pickups, Is.Empty);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(1));

            var inventoryRoundTrip = JsonUtility.FromJson<InventorySaveData>(
                JsonUtility.ToJson(savedInventory));
            var worldRoundTrip = JsonUtility.FromJson<WorldSaveData>(
                JsonUtility.ToJson(savedWorld));
            _inventory.RestoreFromSaveData(inventoryRoundTrip);
            provider.Restore(worldRoundTrip);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(1));
            Assert.That(_spawner.CaptureDynamicPickupSaveData(), Is.Empty);
            Assert.That(_inventory.Slots[0].OriginLots[0].Kind,
                Is.EqualTo(ItemOriginKind.Purchased));
        }

        [Test]
        public void ExistingScenePickupWithoutSerializedOrigin_IsEarnedOnlyBeforeRestore()
        {
            var scenePickup = _host.AddComponent<ItemPickup>();
            scenePickup.Configure(1, "wood", 1, _inventory);
            typeof(ItemPickup).GetField("_originLots", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(scenePickup, new List<ItemOriginLot>());
            Assert.That(scenePickup.CaptureSaveData().OriginLots[0].Kind,
                Is.EqualTo((int)ItemOriginKind.Earned));

            var corrupted = scenePickup.CaptureSaveData();
            corrupted.OriginSchemaVersion = 0;
            scenePickup.RestoreFromSaveData(corrupted);
            Assert.That(scenePickup.CaptureSaveData().OriginLots[0].Kind,
                Is.EqualTo((int)ItemOriginKind.Invalid));
        }

    }
}
