using System;
using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Cave.Death;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Player.Death;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    public sealed class PurchasedOriginDeathEventOrderTests
    {
        private GameObject _host;
        private ItemDatabaseSO _database;
        private ItemDataSO _wood;
        private ItemDataSO _sword;

        [TearDown]
        public void TearDown()
        {
            if (_host != null) UnityEngine.Object.DestroyImmediate(_host);
            if (_database != null) UnityEngine.Object.DestroyImmediate(_database);
            if (_wood != null) UnityEngine.Object.DestroyImmediate(_wood);
            if (_sword != null) UnityEngine.Object.DestroyImmediate(_sword);
        }

        [Test]
        public void ResolverEvents_ObserveCompleteActiveCorpseAndDebitedOwners()
        {
            _wood = Item("wood", 10);
            _sword = Item("sword", 1);
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var type = typeof(DataRegistrySO<ItemDataSO>);
            type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _wood, _sword });
            type.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);

            _host = new GameObject("origin-death-event-order");
            var inventory = _host.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            var player = _host.AddComponent<PlayerManager>();
            var equipment = _host.AddComponent<EquipmentManager>();
            var cave = _host.AddComponent<CaveRunManager>();
            cave.RestoreRuntimeState(new CaveRuntimeState
            {
                HasActiveRun = true, CaveWorldSeed = "world", CaveRunSeed = "run",
                CaveRunId = "run-id", CurrentCaveLevel = 2, DeepestLayerReached = 2
            });
            var recovery = new CorpseRecoveryManager(player, inventory, equipment);
            var provider = new DeathSectionProvider(recovery);
            Assert.That(inventory.TryAddPurchasedItem("wood", 3, 15).Success, Is.True);
            Assert.That(inventory.TryAddItemInstanceWithLots("sword", "sword#1",
                new[] { new ItemOriginLot(ItemOriginKind.Purchased, 1, 40, 39) }).Success, Is.True);
            equipment.EquipItem(EquipmentSlot.LeftHand, "sword#1");
            player.SetGold(17);

            var goldObserved = 0;
            var equipmentObserved = 0;
            Action<GoldChangedEvent> onGold = evt =>
            {
                if (evt.Delta >= 0) return;
                AssertCommittedState();
                goldObserved++;
            };
            Action<EquipmentSlotChangedEvent> onEquipment = evt =>
            {
                if (evt.Slot != EquipmentSlot.LeftHand || evt.ItemInstanceId != null) return;
                AssertCommittedState();
                equipmentObserved++;
            };
            void AssertCommittedState()
            {
                Assert.That(player.CurrentGold, Is.Zero);
                Assert.That(inventory.GetAmount("wood"), Is.Zero);
                Assert.That(inventory.GetAmount("sword"), Is.Zero);
                Assert.That(equipment.GetEquippedItem(EquipmentSlot.LeftHand), Is.Null);
                Assert.That(recovery.HasActiveCorpse, Is.True);
                Assert.That(recovery.ActiveCorpse.GoldAmount, Is.EqualTo(17));
                Assert.That(recovery.ActiveCorpse.InventoryItems.Count, Is.EqualTo(1));
                Assert.That(recovery.ActiveCorpse.EquipmentItems.Count, Is.EqualTo(1));
                var json = JsonUtility.ToJson((DeathSaveData)provider.Capture(new GameSaveData()));
                var restored = JsonUtility.FromJson<DeathSaveData>(json);
                Assert.That(restored.ActiveCorpse.GoldAmount, Is.EqualTo(17));
                Assert.That(restored.ActiveCorpse.LostInventoryItems[0].OriginLots[0].UnitPaidBasisGold,
                    Is.EqualTo(5));
                Assert.That(restored.ActiveCorpse.LostEquipmentItems[0].OriginLots[0].UnitPaidBasisGold,
                    Is.EqualTo(40));
            }

            GameEventBus.Subscribe(onGold);
            GameEventBus.Subscribe(onEquipment);
            try
            {
                new CaveDeathResolver(new CaveDeathPolicy(), cave, player, inventory, equipment,
                    null, null, recovery).ResolveCaveDeath("CaveScene");
                Assert.That(goldObserved, Is.EqualTo(1));
                Assert.That(equipmentObserved, Is.EqualTo(1));
            }
            finally
            {
                GameEventBus.Unsubscribe(onGold);
                GameEventBus.Unsubscribe(onEquipment);
            }
        }

        [Test]
        public void StackableEquipmentBinding_MovesOnlyItsInventoryUnitsThroughSaveAndRecovery()
        {
            _wood = Item("arrow", 20);
            _wood.IsEquippable = true;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var type = typeof(DataRegistrySO<ItemDataSO>);
            type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _wood });
            type.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-death-ammo-binding");
            var inventory = _host.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            var player = _host.AddComponent<PlayerManager>();
            var equipment = _host.AddComponent<EquipmentManager>();
            var cave = _host.AddComponent<CaveRunManager>();
            cave.RestoreRuntimeState(new CaveRuntimeState
            {
                HasActiveRun = true, CaveWorldSeed = "world", CaveRunSeed = "run",
                CaveRunId = "run-id", CurrentCaveLevel = 2, DeepestLayerReached = 2
            });
            var recovery = new CorpseRecoveryManager(player, inventory, equipment);
            var provider = new DeathSectionProvider(recovery);
            Assert.That(inventory.TryAddPurchasedItem("arrow", 3, 17).Success, Is.True);
            equipment.EquipItem(EquipmentSlot.LeftHand, "arrow");
            player.SetGold(11);

            new CaveDeathResolver(new CaveDeathPolicy(), cave, player, inventory, equipment,
                null, null, recovery).ResolveCaveDeath("CaveScene");

            Assert.That(inventory.GetAmount("arrow"), Is.Zero);
            Assert.That(player.CurrentGold, Is.Zero);
            Assert.That(recovery.ActiveCorpse.InventoryItems.Count, Is.EqualTo(1));
            Assert.That(recovery.ActiveCorpse.InventoryItems[0].Amount, Is.EqualTo(3));
            Assert.That(recovery.ActiveCorpse.InventoryItems[0].SourceSlotType,
                Is.EqualTo((int)EquipmentSlot.LeftHand));
            Assert.That(recovery.ActiveCorpse.EquipmentItems, Is.Empty,
                "Bare stack equipment binding must not fabricate a fourth arrow.");

            var saved = (DeathSaveData)provider.Capture(new GameSaveData());
            Assert.That(saved.ActiveCorpse.LostInventoryItems[0].EquipmentBindingId,
                Is.EqualTo(((int)EquipmentSlot.LeftHand).ToString()));
            Assert.That(saved.ActiveCorpse.LostEquipmentItems, Is.Empty);
            recovery.SetActiveCorpse(null);
            provider.Restore(JsonUtility.FromJson<DeathSaveData>(JsonUtility.ToJson(saved)));
            Assert.That(recovery.ActiveCorpse.InventoryItems[0].SourceSlotType,
                Is.EqualTo((int)EquipmentSlot.LeftHand));

            recovery.RecoverCorpse();
            Assert.That(recovery.HasActiveCorpse, Is.False);
            Assert.That(inventory.GetAmount("arrow"), Is.EqualTo(3));
            Assert.That(player.CurrentGold, Is.EqualTo(11));
            var paidBasis = 0;
            foreach (var slot in inventory.Slots)
                if (slot.ItemId == "arrow")
                    foreach (var lot in slot.OriginLots)
                        paidBasis += lot.Quantity * lot.UnitPaidBasisGold;
            Assert.That(paidBasis, Is.EqualTo(17));
        }

        [Test]
        public void LastArrowConsumed_DeathSavePreflightAndReload_DoNotInventArrow()
        {
            _wood = Item("arrow", 20);
            _wood.IsEquippable = true;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var type = typeof(DataRegistrySO<ItemDataSO>);
            type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _wood });
            type.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-death-last-arrow");
            var inventory = _host.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            var player = _host.AddComponent<PlayerManager>();
            var equipment = _host.AddComponent<EquipmentManager>();
            var cave = _host.AddComponent<CaveRunManager>();
            cave.RestoreRuntimeState(new CaveRuntimeState
            {
                HasActiveRun = true, CaveWorldSeed = "world", CaveRunSeed = "run",
                CaveRunId = "run-id", CurrentCaveLevel = 2, DeepestLayerReached = 2
            });
            var recovery = new CorpseRecoveryManager(player, inventory, equipment);
            var provider = new DeathSectionProvider(recovery);
            Assert.That(inventory.TryAddPurchasedItem("arrow", 1, 5).Success, Is.True);
            equipment.EquipItem(EquipmentSlot.LeftHand, "arrow");
            Assert.That(inventory.RemoveItem("arrow", 1), Is.True,
                "BowArrowAttackService consumes ammo through this runtime entrypoint.");
            Assert.That(equipment.GetEquippedItem(EquipmentSlot.LeftHand), Is.Not.Null,
                "The depleted bare binding remains equipped until another ammo type is selected.");

            new CaveDeathResolver(new CaveDeathPolicy(), cave, player, inventory, equipment,
                null, null, recovery).ResolveCaveDeath("CaveScene");

            Assert.That(recovery.HasActiveCorpse, Is.True);
            Assert.That(recovery.ActiveCorpse.InventoryItems, Is.Empty);
            Assert.That(recovery.ActiveCorpse.EquipmentItems, Is.Empty);
            var saved = (DeathSaveData)provider.Capture(new GameSaveData());
            var roundTrip = JsonUtility.FromJson<DeathSaveData>(JsonUtility.ToJson(saved));
            var preflight = typeof(SaveManager).GetMethod("ValidateCorpseSlots",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(preflight, Is.Not.Null);
            Assert.That((bool)preflight.Invoke(null, new object[]
            {
                roundTrip.ActiveCorpse.LostInventoryItems, new Func<string, bool>(id => id == "arrow"),
                new HashSet<int>(), new HashSet<string>(StringComparer.Ordinal)
            }), Is.True);
            Assert.That((bool)preflight.Invoke(null, new object[]
            {
                roundTrip.ActiveCorpse.LostEquipmentItems, new Func<string, bool>(id => id == "arrow"),
                new HashSet<int>(), new HashSet<string>(StringComparer.Ordinal)
            }), Is.True);
            recovery.SetActiveCorpse(null);
            provider.Restore(roundTrip);
            Assert.That(recovery.ActiveCorpse.InventoryItems, Is.Empty);
            Assert.That(recovery.ActiveCorpse.EquipmentItems, Is.Empty);
            recovery.RecoverCorpse();
            Assert.That(inventory.GetAmount("arrow"), Is.Zero);
        }

        [Test]
        public void StarterBowBareBinding_DeathSavePreflightAndReload_ConservesPhysicalInstance()
        {
            const string bowId = "item_weapon_bow_wood";
            const string instanceId = "item_weapon_bow_wood#starter";
            _sword = Item(bowId, 1);
            _sword.IsEquippable = true;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var type = typeof(DataRegistrySO<ItemDataSO>);
            type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _sword });
            type.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-death-starter-bow");
            var inventory = _host.AddComponent<InventoryManager>();
            inventory.Initialize(_database);
            var player = _host.AddComponent<PlayerManager>();
            var equipment = _host.AddComponent<EquipmentManager>();
            var cave = _host.AddComponent<CaveRunManager>();
            cave.RestoreRuntimeState(new CaveRuntimeState
            {
                HasActiveRun = true, CaveWorldSeed = "world", CaveRunSeed = "run",
                CaveRunId = "run-id", CurrentCaveLevel = 2, DeepestLayerReached = 2
            });
            var recovery = new CorpseRecoveryManager(player, inventory, equipment);
            var provider = new DeathSectionProvider(recovery);
            Assert.That(inventory.TryAddItemInstanceWithLots(bowId, instanceId,
                new[] { new ItemOriginLot(ItemOriginKind.Purchased, 1, 40, 39) }).Success, Is.True);
            equipment.EquipItem(EquipmentSlot.RightHand, bowId);

            new CaveDeathResolver(new CaveDeathPolicy(), cave, player, inventory, equipment,
                null, null, recovery).ResolveCaveDeath("CaveScene");

            Assert.That(inventory.GetAmount(bowId), Is.Zero);
            Assert.That(recovery.ActiveCorpse.InventoryItems.Count, Is.EqualTo(1));
            Assert.That(recovery.ActiveCorpse.InventoryItems[0].ItemInstanceId, Is.EqualTo(instanceId));
            Assert.That(recovery.ActiveCorpse.EquipmentItems, Is.Empty,
                "The bare equipment binding is not an additional physical bow.");
            var saved = (DeathSaveData)provider.Capture(new GameSaveData());
            var roundTrip = JsonUtility.FromJson<DeathSaveData>(JsonUtility.ToJson(saved));
            var preflight = typeof(SaveManager).GetMethod("ValidateCorpseSlots",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(preflight, Is.Not.Null);
            var usedSlots = new HashSet<int>();
            var usedInstances = new HashSet<string>(StringComparer.Ordinal);
            var known = new Func<string, bool>(id => id == bowId);
            Assert.That((bool)preflight.Invoke(null, new object[]
                { roundTrip.ActiveCorpse.LostInventoryItems, known, usedSlots, usedInstances }), Is.True);
            Assert.That((bool)preflight.Invoke(null, new object[]
                { roundTrip.ActiveCorpse.LostEquipmentItems, known, usedSlots, usedInstances }), Is.True);
            recovery.SetActiveCorpse(null);
            provider.Restore(roundTrip);
            recovery.RecoverCorpse();
            Assert.That(inventory.GetAmount(bowId), Is.EqualTo(1));
            Assert.That(equipment.GetEquippedItem(EquipmentSlot.RightHand), Is.Null);
            foreach (var slot in inventory.Slots)
                if (slot.ItemId == bowId)
                {
                    Assert.That(slot.ItemInstanceId, Is.EqualTo(instanceId));
                    Assert.That(slot.OriginLots[0].Kind, Is.EqualTo(ItemOriginKind.Purchased));
                    Assert.That(slot.OriginLots[0].UnitPaidBasisGold, Is.EqualTo(40));
                }
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
