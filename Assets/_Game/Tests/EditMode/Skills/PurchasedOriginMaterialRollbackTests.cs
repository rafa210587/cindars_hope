using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Crafting;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class PurchasedOriginMaterialRollbackTests
    {
        private sealed class RefusingEquipment : IFieldRepairEquipment
        {
            public IReadOnlyList<FieldRepairTarget> GetEquippedTargets()
                => new[] { new FieldRepairTarget("sword#1", 20, 100, 0, false, false) };
            public bool TryApplyExactRepair(FieldRepairTarget expected, int amount,
                int capDurability, out int restored) { restored = 0; return false; }
        }
        private sealed class RefusingWallet : IUpgradeWallet
        {
            public int CurrentGold => 10000;
            public bool TrySpendGold(int amount) => false;
        }

        private GameObject _host;
        private InventoryManager _inventory;
        private ItemDatabaseSO _database;
        private ItemDataSO _kit;
        private ItemDataSO _ore;

        [SetUp]
        public void SetUp()
        {
            _kit = Item(CraftingRepairItemIds.BasicKit);
            _ore = Item(HighTierGearCanon.MaterialBandItemId(MaterialBand.Mithril));
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var type = typeof(DataRegistrySO<ItemDataSO>);
            type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _kit, _ore });
            type.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-material-rollback-tests");
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_database);
            Object.DestroyImmediate(_kit);
            Object.DestroyImmediate(_ore);
        }

        [Test]
        public void FailedFieldRepair_RestoresExactPurchasedKit()
        {
            Assert.That(_inventory.TryAddPurchasedItem(CraftingRepairItemIds.BasicKit, 1, 17).Success, Is.True);
            var result = new CraftingRepairTransaction(new SkillItemInventoryAdapter(_inventory),
                new RefusingEquipment()).TryCommit(CraftingRepairItemIds.BasicKit, .2f, .6f);

            Assert.That(result.Failure, Is.EqualTo(FieldRepairFailure.EquipmentChanged));
            Assert.That(_inventory.TryPrepareRemoval(CraftingRepairItemIds.BasicKit, 1, out var selection), Is.True);
            Assert.That(selection.CopyLots()[0].Kind, Is.EqualTo(ItemOriginKind.Purchased));
            Assert.That(selection.CopyLots()[0].UnitPaidBasisGold, Is.EqualTo(17));
        }

        [Test]
        public void FailedGoldDebit_RestoresExactPurchasedUpgradeMaterial()
        {
            var ore = HighTierGearCanon.MaterialBandItemId(MaterialBand.Mithril);
            Assert.That(_inventory.TryAddPurchasedItem(ore, 1, 83).Success, Is.True);
            var registry = new EquipmentUpgradeRegistry();
            var service = new EquipmentUpgradeService(registry,
                new InventoryManagerUpgradeAdapter(_inventory), new RefusingWallet());

            var result = service.TryUpgrade(new UpgradeTarget("sword#1", 640, MaterialBand.Mithril),
                UpgradeFocus.Damage);

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailReason, Is.EqualTo(EquipmentUpgradeService.FailGoldSpend));
            Assert.That(registry.GetLevel("sword#1"), Is.Zero);
            Assert.That(_inventory.TryPrepareRemoval(ore, 1, out var selection), Is.True);
            Assert.That(selection.CopyLots()[0].Kind, Is.EqualTo(ItemOriginKind.Purchased));
            Assert.That(selection.CopyLots()[0].UnitPaidBasisGold, Is.EqualTo(83));
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
