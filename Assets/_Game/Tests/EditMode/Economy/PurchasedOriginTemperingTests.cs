using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Economy
{
    public sealed class PurchasedOriginTemperingTests
    {
        private sealed class Wallet : IOriginTemperingWallet
        {
            public int Gold = 2000;
            public bool FailStage;
            public int CurrentGold => Gold;
            public bool TrySpendGold(int amount) { if (Gold < amount) return false; Gold -= amount; return true; }
            public bool TryStageSpend(int amount, out object token)
            {
                token = null;
                if (FailStage || Gold < amount) return false;
                token = Gold; Gold -= amount; return true;
            }
            public bool TryRollbackSpend(object token) { Gold = (int)token; return true; }
            public bool TryCommitSpend(object token) => true;
        }

        private sealed class Classifier : ITemperingItemClassifier
        {
            public bool IsWeaponInstance(string id) => true;
            public bool IsToolInstance(string id) => false;
        }


        private sealed class FailSecondTake : IOriginTemperingInventory
        {
            private readonly IOriginTemperingInventory _inner;
            private int _takes;
            public FailSecondTake(IOriginTemperingInventory inner) => _inner = inner;
            public bool HasItem(string id, int amount) => _inner.HasItem(id, amount);
            public bool RemoveItem(string id, int amount) => _inner.RemoveItem(id, amount);
            public bool AddItem(string id, int amount) => _inner.AddItem(id, amount);
            public bool TryTakeLots(string id, int amount, out InventoryLotSelection selection)
            {
                selection = null;
                return ++_takes != 2 && _inner.TryTakeLots(id, amount, out selection);
            }
            public bool TryReturnLots(InventoryLotSelection selection) => _inner.TryReturnLots(selection);
            public bool TryAddLots(string id, IReadOnlyList<ItemOriginLot> lots) => _inner.TryAddLots(id, lots);
            public void PublishCommittedChange(string id, int delta) => _inner.PublishCommittedChange(id, delta);
        }

        private GameObject _host;
        private InventoryManager _inventory;
        private ItemDatabaseSO _database;
        private ItemDataSO _fire;
        private ItemDataSO _ice;
        private ItemDataSO _void;
        private WeaponInfusionRegistry _registry;
        private Wallet _wallet;
        private TemperingService _service;

        [SetUp]
        public void SetUp()
        {
            _fire = Item("item_essence_fire");
            _ice = Item("item_essence_ice");
            _void = Item("item_essence_void");
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var type = typeof(DataRegistrySO<ItemDataSO>);
            type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _fire, _ice, _void });
            type.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-tempering-tests");
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
            _registry = new WeaponInfusionRegistry();
            _wallet = new Wallet();
            _service = new TemperingService(_registry,
                new InventoryManagerTemperingAdapter(_inventory), _wallet, new Classifier());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_database);
            Object.DestroyImmediate(_fire);
            Object.DestroyImmediate(_ice);
            Object.DestroyImmediate(_void);
        }

        [Test]
        public void ReplaceAndReload_RefundsPriorPurchasedEssenceWithExactBasis()
        {
            Assert.That(_inventory.TryAddPurchasedItem("item_essence_fire", 3, 21).Success, Is.True);
            Assert.That(_inventory.TryAddPurchasedItem("item_essence_ice", 3, 12).Success, Is.True);
            Assert.That(_service.ApplyTempering("weapon_sword#1", TemperingElement.Fire, 1, true).Success, Is.True);

            var restored = new WeaponInfusionRegistry();
            restored.RestoreFromSaveData(_registry.CaptureSaveData(), true);
            var service = new TemperingService(restored,
                new InventoryManagerTemperingAdapter(_inventory), _wallet, new Classifier());
            var result = service.ApplyTempering("weapon_sword#1", TemperingElement.Ice, 1, true);

            Assert.That(result.Success, Is.True, result.FailReason);
            Assert.That(_inventory.TryPrepareRemoval("item_essence_fire", 1, out var refund), Is.True);
            Assert.That(refund.CopyLots()[0].Kind, Is.EqualTo(ItemOriginKind.Purchased));
            Assert.That(refund.CopyLots()[0].UnitPaidBasisGold, Is.EqualTo(7));
            Assert.That(refund.CopyLots()[0].UnitResaleCapGold, Is.EqualTo(6));
            Assert.That(restored.GetConsumedEssenceLots("weapon_sword#1")[0].UnitPaidBasisGold, Is.EqualTo(4));
        }

        [Test]
        public void VoidT2_RefundExcludesSameIdCatalystAfterReload()
        {
            Assert.That(_inventory.TryAddPurchasedItem("item_essence_void", 6, 60).Success, Is.True);
            Assert.That(_inventory.TryAddPurchasedItem("item_essence_void", 1, 90).Success, Is.True);
            Assert.That(_inventory.TryAddPurchasedItem("item_essence_fire", 3, 9).Success, Is.True);
            Assert.That(_service.ApplyTempering("weapon_sword#1", TemperingElement.Void, 2, true).Success, Is.True);
            var consumed = _registry.GetConsumedEssenceLots("weapon_sword#1");
            Assert.That(consumed.Count, Is.EqualTo(1));
            Assert.That(consumed[0].Quantity, Is.EqualTo(6));
            Assert.That(consumed[0].UnitPaidBasisGold, Is.EqualTo(10));

            var restored = new WeaponInfusionRegistry();
            restored.RestoreFromSaveData(_registry.CaptureSaveData(), true);
            var result = new TemperingService(restored,
                new InventoryManagerTemperingAdapter(_inventory), _wallet, new Classifier())
                .ApplyTempering("weapon_sword#1", TemperingElement.Fire, 1, true);

            Assert.That(result.Success, Is.True, result.FailReason);
            Assert.That(result.RefundedEssences, Is.EqualTo(3));
            Assert.That(_inventory.TryPrepareRemoval("item_essence_void", 3, out var refund), Is.True);
            Assert.That(refund.CopyLots()[0].UnitPaidBasisGold, Is.EqualTo(10));
            Assert.That(_inventory.GetAmount("item_essence_void"), Is.EqualTo(3));
        }

        [Test]
        public void GoldStageFailure_LeavesPurchasedLotsAndInfusionUntouched()
        {
            Assert.That(_inventory.TryAddPurchasedItem("item_essence_fire", 3, 21).Success, Is.True);
            _wallet.FailStage = true;
            var before = _inventory.CaptureSaveData();
            var result = _service.ApplyTempering("weapon_sword#1", TemperingElement.Fire, 1, true);
            Assert.That(result.Success, Is.False);
            Assert.That(_inventory.GetAmount("item_essence_fire"), Is.EqualTo(3));
            Assert.That(_inventory.Slots[0].OriginLots[0].UnitPaidBasisGold,
                Is.EqualTo(before.Slots[0].OriginLots[0].UnitPaidBasisGold));
            Assert.That(_wallet.Gold, Is.EqualTo(2000));
            Assert.That(_registry.Count, Is.Zero);
        }

        [Test]
        public void CatalystCommitFailure_RestoresPurchasedEssenceAndGold()
        {
            Assert.That(_inventory.TryAddPurchasedItem("item_essence_fire", 6, 42).Success, Is.True);
            Assert.That(_inventory.TryAddPurchasedItem("item_essence_void", 1, 30).Success, Is.True);
            var service = new TemperingService(_registry,
                new FailSecondTake(new InventoryManagerTemperingAdapter(_inventory)),
                _wallet, new Classifier());

            var result = service.ApplyTempering("weapon_sword#1", TemperingElement.Fire, 2, true);

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailReason, Is.EqualTo("VOID_REMOVE_FAILED"));
            Assert.That(_wallet.Gold, Is.EqualTo(2000));
            Assert.That(_registry.Count, Is.Zero);
            Assert.That(_inventory.GetAmount("item_essence_fire"), Is.EqualTo(6));
            Assert.That(_inventory.GetAmount("item_essence_void"), Is.EqualTo(1));
            Assert.That(_inventory.TryPrepareRemoval("item_essence_fire", 6, out var selection), Is.True);
            Assert.That(selection.CopyLots()[0].UnitPaidBasisGold, Is.EqualTo(7));
        }

        [Test]
        public void V5AndCorruptV6Refunds_AreLegacyAndInvalidRespectively()
        {
            var old = new List<CindarsHope.Foundation.WeaponInfusionSaveData>
            { new CindarsHope.Foundation.WeaponInfusionSaveData
                { ItemInstanceId = "weapon_sword#1", InfusionElement = "fire", InfusionTier = 1 } };
            _registry.RestoreFromSaveData(old, false);
            Assert.That(_registry.GetConsumedEssenceLots("weapon_sword#1")[0].Kind,
                Is.EqualTo(ItemOriginKind.LegacyUntracked));
            _registry.RestoreFromSaveData(old, true);
            Assert.That(_registry.GetConsumedEssenceLots("weapon_sword#1")[0].Kind,
                Is.EqualTo(ItemOriginKind.Invalid));
        }

        private static ItemDataSO Item(string id)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.MaxStack = 20;
            return item;
        }
    }
}
