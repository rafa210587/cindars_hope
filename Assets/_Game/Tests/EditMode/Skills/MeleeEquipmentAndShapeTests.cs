using System.Reflection;
using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using CindarsHope.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class MeleeEquipmentAndShapeTests
    {
        private GameObject _managerObject;
        private GameObject _caster;
        private EquipmentManager _equipment;
        private ItemDatabaseSO _items;
        private WeaponDatabaseSO _weapons;
        private ItemDataSO _daggerItem;
        private ItemDataSO _bowItem;
        private WeaponDataSO _dagger;
        private WeaponDataSO _bow;

        [SetUp]
        public void SetUp()
        {
            if (EquipmentManager.Instance != null)
                Object.DestroyImmediate(EquipmentManager.Instance.gameObject);

            _dagger = Weapon("weapon_dagger_fixture", WeaponType.Dagger, WeaponWeightClass.Light, 75);
            _bow = Weapon("weapon_bow_fixture", WeaponType.Bow, WeaponWeightClass.Light, 60);
            _daggerItem = Item("item_dagger_fixture", _dagger.Id);
            _bowItem = Item("item_bow_fixture", _bow.Id);
            _items = Registry<ItemDatabaseSO, ItemDataSO>(_daggerItem, _bowItem);
            _weapons = Registry<WeaponDatabaseSO, WeaponDataSO>(_dagger, _bow);

            _managerObject = new GameObject("Equipment gate fixture");
            _equipment = _managerObject.AddComponent<EquipmentManager>();
            SetField(_equipment, "_itemDatabase", _items);
            SetField(_equipment, "_durabilityTracker", new EquipmentDurabilityTracker());
            _caster = new GameObject("Caster fixture");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_caster);
            Object.DestroyImmediate(_managerObject);
            Object.DestroyImmediate(_items);
            Object.DestroyImmediate(_weapons);
            Object.DestroyImmediate(_daggerItem);
            Object.DestroyImmediate(_bowItem);
            Object.DestroyImmediate(_dagger);
            Object.DestroyImmediate(_bow);
        }

        [Test]
        public void OffhandCut_NoLeftHand_FailsBeforeStaminaLookup()
        {
            var gate = Gate();
            var action = Action(MeleeEquipmentGate.OffhandCutActionId);
            var result = new MeleeStrikeSkillEffectExecutor("fixture", "Offhand", action, gate)
                .Execute(new SkillEffectContext { Caster = _caster, ActionData = action, Rank = 1 });

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(MeleeEquipmentGate.RequiresOffhandFailureKey));
            Object.DestroyImmediate(action);
        }

        [Test]
        public void OffhandCut_LightIntactDagger_IsReady()
        {
            EquipWithDurability(_daggerItem.Id, _dagger.DurabilityMax);

            var result = Gate().Evaluate(_caster, MeleeEquipmentGate.OffhandCutActionId);

            Assert.That(result.CanExecute, Is.True);
        }

        [Test]
        public void OffhandCut_IndividualDagger_ResolvesDefinitionAndKeepsInstanceDurability()
        {
            const string instanceId = "item_dagger_fixture#crafted-42";
            EquipWithDurability(instanceId, _dagger.DurabilityMax);

            var result = Gate().Evaluate(_caster, MeleeEquipmentGate.OffhandCutActionId);

            Assert.That(result.CanExecute, Is.True);
            Assert.That(_equipment.GetEquippedItem(EquipmentSlot.LeftHand), Is.EqualTo(instanceId));
            Assert.That(_equipment.GetItemDurability(instanceId).MaxDurability, Is.EqualTo(_dagger.DurabilityMax));
        }

        [Test]
        public void OffhandCut_BrokenDagger_IsRejected()
        {
            EquipWithDurability(_daggerItem.Id, _dagger.DurabilityMax);
            _equipment.GetItemDurability(_daggerItem.Id).CurrentDurability = 0;

            var result = Gate().Evaluate(_caster, MeleeEquipmentGate.OffhandCutActionId);

            Assert.That(result.CanExecute, Is.False);
            Assert.That(result.FailureKey, Is.EqualTo(MeleeEquipmentGate.RequiresOffhandFailureKey));
        }

        [Test]
        public void OffhandCut_LightBow_IsRejected()
        {
            EquipWithDurability(_bowItem.Id, _bow.DurabilityMax);

            var result = Gate().Evaluate(_caster, MeleeEquipmentGate.OffhandCutActionId);

            Assert.That(result.CanExecute, Is.False);
            Assert.That(result.FailureKey, Is.EqualTo(MeleeEquipmentGate.RequiresOffhandFailureKey));
        }

        [Test]
        public void OtherMeleeAction_DoesNotRequireOffhand()
        {
            Assert.That(Gate().Evaluate(_caster, "skill_melee_whirl_cut").CanExecute, Is.True);
        }

        [Test]
        public void CanonicalMeleeShapes_AreAuthoredPerContract()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                var offhand = actions.Find(x => x.SkillActionId == MeleeEquipmentGate.OffhandCutActionId);
                var whirl = actions.Find(x => x.SkillActionId == "skill_melee_whirl_cut");
                Assert.That(offhand.ResolveRank(1).Damage, Is.EqualTo(8));
                Assert.That(offhand.ResolveRank(2).Damage, Is.EqualTo(10));
                Assert.That(offhand.ResolveRank(3).Damage, Is.EqualTo(12));
                Assert.That(offhand.ArcDegrees, Is.EqualTo(140f));
                Assert.That(offhand.Range, Is.EqualTo(1.2f));
                Assert.That(whirl.MaxTargets, Is.EqualTo(6));
                Assert.That(whirl.ResolveDamageForTargetIndex(3, 0), Is.EqualTo(14));
                Assert.That(whirl.ResolveDamageForTargetIndex(3, 2), Is.EqualTo(14));
                Assert.That(whirl.ResolveDamageForTargetIndex(3, 3), Is.EqualTo(10));
                Assert.That(whirl.ResolveDamageForTargetIndex(3, 5), Is.EqualTo(10));
            }
            finally
            {
                foreach (var action in actions) Object.DestroyImmediate(action);
            }
        }

        [TestCase("item_weapon_dagger_copper")]
        [TestCase("item_weapon_dagger_steel")]
        [TestCase("item_weapon_dagger_mithril")]
        public void CanonicalDagger_IsCompatibleWithBothHandsAfterGeneration(string itemId)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>($"Assets/_Game/Data/Items/{itemId}.asset");
            Assert.That(item, Is.Not.Null);
            Assert.That(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.LeftHand), Is.True);
            Assert.That(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.RightHand), Is.True);
        }

        private MeleeEquipmentGate Gate()
            => new MeleeEquipmentGate(_equipment, new EquippedItemResolver(_items, _weapons, null));

        private void EquipWithDurability(string itemId, int durability)
        {
            _equipment.EquipItem(EquipmentSlot.LeftHand, itemId);
            Assert.That(_equipment.DurabilityTracker, Is.Not.Null);
            _equipment.DurabilityTracker.InitializeEquipment(itemId, durability);
        }

        private static SkillActionSO Action(string id)
        {
            var action = ScriptableObject.CreateInstance<SkillActionSO>();
            action.SkillActionId = id;
            action.BaseDamage = 8;
            action.StaminaCost = 10;
            action.Range = 1.2f;
            action.ArcDegrees = 140;
            return action;
        }

        private static WeaponDataSO Weapon(string id, WeaponType type, WeaponWeightClass weight, int durability)
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
            weapon.Id = id;
            weapon.Type = type;
            weapon.WeightClass = weight;
            weapon.DurabilityMax = durability;
            return weapon;
        }

        private static ItemDataSO Item(string id, string weaponId)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.Category = ItemCategory.Weapon;
            item.IsEquippable = true;
            item.WeaponId = weaponId;
            return item;
        }

        private static TRegistry Registry<TRegistry, TItem>(params TItem[] items)
            where TRegistry : DataRegistrySO<TItem>
            where TItem : ScriptableObject, IIdentifiedData
        {
            var database = ScriptableObject.CreateInstance<TRegistry>();
            var registryType = typeof(DataRegistrySO<TItem>);
            registryType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(database, items);
            registryType.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(database, null);
            return database;
        }

        private static void SetField(object target, string field, object value)
            => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
