using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Cave.Death;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Equipment;
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
    public sealed class PurchasedOriginRecoveryTests
    {
        private GameObject _host;
        private InventoryManager _inventory;
        private PlayerManager _player;
        private FakeEquipment _equipment;
        private CorpseRecoveryManager _recovery;
        private DeathSectionProvider _provider;
        private ItemDatabaseSO _database;
        private ItemDataSO _wood;
        private ItemDataSO _sword;

        [SetUp]
        public void SetUp()
        {
            _wood = Item("wood", 10);
            _sword = Item("sword", 1);
            _sword.IsEquippable = true;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registry = typeof(DataRegistrySO<ItemDataSO>);
            registry.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _wood, _sword });
            registry.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-recovery-test");
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
            _player = _host.AddComponent<PlayerManager>();
            _equipment = new FakeEquipment();
            _recovery = new CorpseRecoveryManager(_player, _inventory, _equipment);
            _provider = new DeathSectionProvider(_recovery);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_database);
            Object.DestroyImmediate(_wood);
            Object.DestroyImmediate(_sword);
        }

        [Test]
        public void CaveCorpseSaveReload_PartialRecoveryRetainsLotsAndCreditsGoldOnce()
        {
            Assert.That(_inventory.TryAddItem("sword", InventoryManager.MaxCapacity).Success, Is.True);
            _recovery.SetActiveCorpse(CorpseWithPurchasedWood());
            var saved = (DeathSaveData)_provider.Capture(new GameSaveData());
            Assert.That(saved.ActiveCorpse.LostInventoryItems[0].OriginLots.Count, Is.EqualTo(2));
            var roundTrip = JsonUtility.FromJson<DeathSaveData>(JsonUtility.ToJson(saved));
            _recovery.SetActiveCorpse(null);
            _provider.Restore(roundTrip);

            _recovery.RecoverCorpse();
            Assert.That(_player.CurrentGold, Is.EqualTo(5));
            Assert.That(_recovery.HasActiveCorpse, Is.True);
            Assert.That(_recovery.ActiveCorpse.InventoryItems.Count, Is.EqualTo(1));
            Assert.That(_recovery.ActiveCorpse.InventoryItems[0].OriginLots[0].UnitPaidBasisGold,
                Is.EqualTo(5));
            var partial = (DeathSaveData)_provider.Capture(new GameSaveData());
            Assert.That(partial.ActiveCorpse.GoldAmount, Is.Zero);
            Assert.That(partial.ActiveCorpse.LostInventoryItems[0].Amount, Is.EqualTo(3));

            Assert.That(_inventory.RemoveItem("sword", 1), Is.True);
            _recovery.RecoverCorpse();
            Assert.That(_recovery.HasActiveCorpse, Is.False);
            Assert.That(_player.CurrentGold, Is.EqualTo(5));
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(3));
            var paid = 0;
            foreach (var slot in _inventory.Slots)
                if (slot.ItemId == "wood")
                    foreach (var lot in slot.OriginLots)
                        paid += lot.Quantity * lot.UnitPaidBasisGold;
            Assert.That(paid, Is.EqualTo(17));
        }

        [Test]
        public void RejectedEquipmentEquip_RollsBackInventoryAndKeepsCorpseEntry()
        {
            var corpse = new Corpse("corpse-equip") { Status = CorpseStatus.Active, RunId = "run", CaveLevel = 1 };
            corpse.EquipmentItems.Add(new CorpseItem("sword")
            {
                ItemInstanceId = "sword#test",
                SourceSlotType = (int)EquipmentSlot.LeftHand,
                OriginSchemaVersion = 1,
                OriginLots = new List<ItemOriginLotSaveData>
                {
                    new ItemOriginLotSaveData { Kind = (int)ItemOriginKind.Purchased,
                        Quantity = 1, UnitPaidBasisGold = 5, UnitResaleCapGold = 4 }
                }
            });
            _recovery.SetActiveCorpse(corpse);
            _equipment.Reject = true;
            _recovery.RecoverCorpse();
            Assert.That(_inventory.GetAmount("sword"), Is.Zero);
            Assert.That(_recovery.ActiveCorpse.EquipmentItems.Count, Is.EqualTo(1));

            _equipment.Reject = false;
            _recovery.RecoverCorpse();
            Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(1));
            Assert.That(_equipment.GetEquippedItem(EquipmentSlot.LeftHand), Is.EqualTo("sword#test"));
            Assert.That(_inventory.Slots[0].OriginLots[0].UnitPaidBasisGold, Is.EqualTo(5));
        }

        [Test]
        public void MissingV6CorpseOriginMarker_RestoresInvalidRatherThanEarned()
        {
            _recovery.SetActiveCorpse(CorpseWithPurchasedWood());
            var saved = (DeathSaveData)_provider.Capture(new GameSaveData());
            saved.ActiveCorpse.OriginSchemaVersion = 0;
            _provider.Restore(saved);
            _recovery.RecoverCorpse();
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(3));
            Assert.That(_inventory.GetEligibleAmount("wood"), Is.Zero);
        }

        [Test]
        public void LegacyEquippedInstanceWithoutInventorySlot_UsesCanonicalItemIdAndInvalidOrigin()
        {
            var equipmentHost = new GameObject("orphan-equipment");
            try
            {
                var equipment = equipmentHost.AddComponent<EquipmentManager>();
                equipment.EquipItem(EquipmentSlot.LeftHand, "sword#orphan");
                var resolver = new CaveDeathResolver(new CaveDeathPolicy(), null,
                    _player, _inventory, equipment, null);
                var corpse = new Corpse("orphan") { Status = CorpseStatus.Active };
                var move = typeof(CaveDeathResolver).GetMethod("MoveEquipmentToCorpse",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(move, Is.Not.Null);
                move.Invoke(resolver, new object[] { corpse, _inventory.CaptureSaveData() });
                Assert.That(corpse.EquipmentItems.Count, Is.EqualTo(1));
                Assert.That(corpse.EquipmentItems[0].ItemId, Is.EqualTo("sword"));
                Assert.That(corpse.EquipmentItems[0].OriginLots[0].Kind,
                    Is.EqualTo((int)ItemOriginKind.Invalid));
            }
            finally { Object.DestroyImmediate(equipmentHost); }
        }

        [Test]
        public void V5EquipmentWithoutValidBinding_RecoversAsInventoryWithoutAutoEquip()
        {
            var corpse = new Corpse("legacy-binding") { Status = CorpseStatus.Active };
            corpse.EquipmentItems.Add(new CorpseItem("sword")
            {
                ItemInstanceId = "sword#old-zero", SourceSlotType = (int)EquipmentSlot.LeftHand,
                SourceSlotIndex = 0, OriginSchemaVersion = 0
            });
            corpse.EquipmentItems.Add(new CorpseItem("sword")
            {
                ItemInstanceId = "sword#old-large", SourceSlotType = (int)EquipmentSlot.RightHand,
                SourceSlotIndex = 999, OriginSchemaVersion = 0
            });
            _recovery.SetActiveCorpse(corpse);
            var saved = (DeathSaveData)_provider.Capture(new GameSaveData());
            saved.ActiveCorpse.OriginSchemaVersion = 0;
            saved.ActiveCorpse.LostEquipmentItems[0].EquipmentBindingId = null;
            saved.ActiveCorpse.LostEquipmentItems[1].EquipmentBindingId = "999";

            _provider.Restore(saved);

            Assert.That(_recovery.ActiveCorpse.EquipmentItems, Is.Empty);
            Assert.That(_recovery.ActiveCorpse.InventoryItems.Count, Is.EqualTo(2));
            _recovery.RecoverCorpse();
            Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(2));
            Assert.That(_equipment.GetEquippedItem(EquipmentSlot.LeftHand), Is.Null.Or.Empty);
            Assert.That(_equipment.GetEquippedItem(EquipmentSlot.RightHand), Is.Null.Or.Empty);
            Assert.That(_inventory.GetEligibleAmount("sword"), Is.Zero);
        }

        [Test]
        public void DeferredReplacementEvent_SeesNewActiveCorpse()
        {
            _recovery.SetActiveCorpse(new Corpse("old") { Status = CorpseStatus.Active });
            var replacement = new Corpse("new") { Status = CorpseStatus.Active };
            var observed = false;
            System.Action<CorpseReplacedEvent> listener = evt =>
            {
                observed = true;
                Assert.That(_recovery.ActiveCorpse, Is.SameAs(replacement));
                Assert.That(evt.OldCorpseId, Is.EqualTo("old"));
                Assert.That(evt.NewCorpseId, Is.EqualTo("new"));
            };
            GameEventBus.Subscribe(listener);
            try
            {
                var oldId = _recovery.SetActiveCorpseDeferred(replacement);
                Assert.That(observed, Is.False);
                Assert.That(_recovery.ActiveCorpse, Is.SameAs(replacement));
                _recovery.PublishReplacement(oldId, replacement.CorpseId);
                Assert.That(observed, Is.True);
            }
            finally { GameEventBus.Unsubscribe(listener); }
        }

        [Test]
        public void RecoveredEquipmentEvent_ObservesCommittedInventoryAndBody()
        {
            var realEquipment = _host.AddComponent<EquipmentManager>();
            var recovery = new CorpseRecoveryManager(_player, _inventory, realEquipment);
            var provider = new DeathSectionProvider(recovery);
            var corpse = new Corpse("equipment-event") { Status = CorpseStatus.Active };
            corpse.EquipmentItems.Add(new CorpseItem("sword")
            {
                ItemInstanceId = "sword#committed",
                SourceSlotType = (int)EquipmentSlot.LeftHand,
                OriginSchemaVersion = 1,
                OriginLots = new List<ItemOriginLotSaveData>
                {
                    new ItemOriginLotSaveData { Kind = (int)ItemOriginKind.Purchased,
                        Quantity = 1, UnitPaidBasisGold = 8, UnitResaleCapGold = 7 }
                }
            });
            recovery.SetActiveCorpse(corpse);
            var events = 0;
            var inventoryEvents = 0;
            System.Action<InventoryChangedEvent> inventoryListener = evt =>
            {
                if (evt.ItemId != "sword" || evt.Delta != 1) return;
                inventoryEvents++;
                Assert.That(_inventory.Slots[0].IsEquipped, Is.True);
                Assert.That(realEquipment.GetEquippedItem(EquipmentSlot.LeftHand),
                    Is.EqualTo("sword#committed"));
                Assert.That(recovery.ActiveCorpse.EquipmentItems, Is.Empty);
                Assert.That(_inventory.TryPrepareRemoval("sword", 1, out _), Is.False);
            };
            System.Action<EquipmentSlotChangedEvent> listener = evt =>
            {
                if (evt.Slot != EquipmentSlot.LeftHand || evt.ItemInstanceId != "sword#committed") return;
                events++;
                Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(1));
                Assert.That(realEquipment.GetEquippedItem(EquipmentSlot.LeftHand),
                    Is.EqualTo("sword#committed"));
                Assert.That(recovery.ActiveCorpse.EquipmentItems, Is.Empty);
                var saved = (DeathSaveData)provider.Capture(new GameSaveData());
                Assert.That(saved.ActiveCorpse.LostEquipmentItems, Is.Empty);
                Assert.That(_inventory.TryPrepareRemoval("sword", 1, out _), Is.False,
                    "Equipped recovery must be excluded from sale selection before any event.");
                Assert.That(_inventory.Slots[0].IsEquipped, Is.True);
                Assert.That(_inventory.Slots[0].OriginLots[0].UnitPaidBasisGold, Is.EqualTo(8));
                // Reentrant inventory change may invalidate old tokens only after commit.
                Assert.That(_inventory.TryAddItem("wood", 1).Success, Is.True);
            };
            GameEventBus.Subscribe(inventoryListener);
            GameEventBus.Subscribe(listener);
            try
            {
                recovery.RecoverCorpse();
                Assert.That(events, Is.EqualTo(1));
                Assert.That(inventoryEvents, Is.EqualTo(1));
                Assert.That(recovery.HasActiveCorpse, Is.False);
                Assert.That(_inventory.GetAmount("sword"), Is.EqualTo(1));
                Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(1));
            }
            finally
            {
                GameEventBus.Unsubscribe(inventoryListener);
                GameEventBus.Unsubscribe(listener);
            }
        }

        private static Corpse CorpseWithPurchasedWood()
        {
            var corpse = new Corpse("corpse-wood")
                { Status = CorpseStatus.Active, RunId = "run", CaveSeed = "run", CaveLevel = 1, GoldAmount = 5 };
            corpse.InventoryItems.Add(new CorpseItem("wood", 3)
            {
                OriginSchemaVersion = 1,
                OriginLots = new List<ItemOriginLotSaveData>
                {
                    new ItemOriginLotSaveData { Kind = (int)ItemOriginKind.Purchased,
                        Quantity = 1, UnitPaidBasisGold = 5, UnitResaleCapGold = 4 },
                    new ItemOriginLotSaveData { Kind = (int)ItemOriginKind.Purchased,
                        Quantity = 2, UnitPaidBasisGold = 6, UnitResaleCapGold = 5 }
                }
            });
            return corpse;
        }

        private static ItemDataSO Item(string id, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.MaxStack = maxStack;
            return item;
        }

        private sealed class FakeEquipment : IEquipmentRuntime, IEquipmentSilentMutationRuntime
        {
            private readonly Dictionary<EquipmentSlot, string> _equipped = new();
            public bool Reject;
            public string GetEquippedItem(EquipmentSlot slot)
                => _equipped.TryGetValue(slot, out var item) ? item : null;
            public void EquipItem(EquipmentSlot slot, string itemInstanceId)
            {
                if (Reject && !string.IsNullOrEmpty(itemInstanceId)) return;
                _equipped[slot] = itemInstanceId;
            }
            public bool TryEquipSilently(EquipmentSlot slot, string itemInstanceId)
            {
                if (Reject || !string.IsNullOrEmpty(GetEquippedItem(slot))) return false;
                _equipped[slot] = itemInstanceId;
                return true;
            }
            public bool TryUnequipSilently(EquipmentSlot slot, string expectedItemInstanceId)
            {
                if (GetEquippedItem(slot) != expectedItemInstanceId) return false;
                _equipped.Remove(slot);
                return true;
            }
            public void PublishCommittedEquip(EquipmentSlot slot, string itemInstanceId)
                => GameEventBus.Publish(new EquipmentSlotChangedEvent(slot, itemInstanceId));
            public void ConfigureWeaponDurabilityResolver(System.Func<string, int?> resolver) { }
            public int? ResolveBaseDurability(string itemId) => null;
            public void InitializeCraftedItemDurability(string itemInstanceId, int maxDurability) { }
        }
    }
}
