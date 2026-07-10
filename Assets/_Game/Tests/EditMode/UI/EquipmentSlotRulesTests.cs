using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using CindarsHope.UI;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.UI
{
    /// <summary>
    /// Testes determinísticos para EquipmentSlotRules — regra pura de resolução/compatibilidade de
    /// slot de equipamento extraída do InventoryPanelController. Sem MonoBehaviour, sem cena.
    /// </summary>
    public class EquipmentSlotRulesTests
    {
        // ---- Helpers ----

        private static ItemDataSO MakeItem(
            ItemCategory category = ItemCategory.Weapon,
            string id = "item_test",
            bool isEquippable = true,
            EquipmentSlot[] allowedSlots = null)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Category = category;
            item.Id = id;
            item.IsEquippable = isEquippable;
            item.AllowedEquipmentSlots = allowedSlots;
            return item;
        }

        // ---- ResolveEquipmentSlot ----

        [Test]
        public void ResolveEquipmentSlot_ExplicitAllowedSlots_WinsOverCategory()
        {
            // Weapon por categoria resolveria RightHand, mas AllowedEquipmentSlots explicito
            // com apenas Chest deve vencer a inferencia por categoria.
            var item = MakeItem(ItemCategory.Weapon, allowedSlots: new[] { EquipmentSlot.Chest });

            var result = EquipmentSlotRules.ResolveEquipmentSlot(item);

            Assert.AreEqual(EquipmentSlot.Chest, result);
        }

        [Test]
        public void ResolveEquipmentSlot_ExplicitAllowedSlots_PrefersLeftHandWhenPresent()
        {
            var item = MakeItem(ItemCategory.Ammo, allowedSlots: new[] { EquipmentSlot.RightHand, EquipmentSlot.LeftHand });

            var result = EquipmentSlotRules.ResolveEquipmentSlot(item);

            Assert.AreEqual(EquipmentSlot.LeftHand, result);
        }

        [Test]
        public void ResolveEquipmentSlot_Weapon_ResolvesRightHand()
        {
            var item = MakeItem(ItemCategory.Weapon);

            var result = EquipmentSlotRules.ResolveEquipmentSlot(item);

            Assert.AreEqual(EquipmentSlot.RightHand, result);
        }

        [Test]
        public void ResolveEquipmentSlot_Tool_ResolvesLeftHand()
        {
            var item = MakeItem(ItemCategory.Tool);

            var result = EquipmentSlotRules.ResolveEquipmentSlot(item);

            Assert.AreEqual(EquipmentSlot.LeftHand, result);
        }

        [Test]
        public void ResolveEquipmentSlot_Ammo_ResolvesLeftHand()
        {
            var item = MakeItem(ItemCategory.Ammo);

            var result = EquipmentSlotRules.ResolveEquipmentSlot(item);

            Assert.AreEqual(EquipmentSlot.LeftHand, result);
        }

        [Test]
        public void ResolveEquipmentSlot_IdContainsArmor_ResolvesChest()
        {
            var item = MakeItem(ItemCategory.Misc, id: "item_iron_armor");

            var result = EquipmentSlotRules.ResolveEquipmentSlot(item);

            Assert.AreEqual(EquipmentSlot.Chest, result);
        }

        [Test]
        public void ResolveEquipmentSlot_IdContainsRingOrAmuletOrAccessory_ResolvesAccessory()
        {
            Assert.AreEqual(EquipmentSlot.Accessory, EquipmentSlotRules.ResolveEquipmentSlot(MakeItem(ItemCategory.Misc, id: "item_gold_ring")));
            Assert.AreEqual(EquipmentSlot.Accessory, EquipmentSlotRules.ResolveEquipmentSlot(MakeItem(ItemCategory.Misc, id: "item_lucky_amulet")));
            Assert.AreEqual(EquipmentSlot.Accessory, EquipmentSlotRules.ResolveEquipmentSlot(MakeItem(ItemCategory.Misc, id: "item_shiny_accessory")));
        }

        [Test]
        public void ResolveEquipmentSlot_NonEquippableCategoryAndId_ResolvesNone()
        {
            var item = MakeItem(ItemCategory.Misc, id: "item_wheat_seed");

            var result = EquipmentSlotRules.ResolveEquipmentSlot(item);

            Assert.AreEqual(EquipmentSlot.None, result);
        }

        // ---- IsCompatibleWithEquipmentSlot ----

        [Test]
        public void IsCompatibleWithEquipmentSlot_NotEquippable_ReturnsFalse()
        {
            var item = MakeItem(ItemCategory.Weapon, isEquippable: false);

            var result = EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.RightHand);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsCompatibleWithEquipmentSlot_NullItem_ReturnsFalse()
        {
            Assert.IsFalse(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(null, EquipmentSlot.RightHand));
        }

        [Test]
        public void IsCompatibleWithEquipmentSlot_ExplicitAllowedSlots_WinsOverCategory()
        {
            // Weapon aceitaria RightHand pela categoria, mas AllowedEquipmentSlots explicito
            // restringe a apenas Chest — RightHand deve ser rejeitado.
            var item = MakeItem(ItemCategory.Weapon, allowedSlots: new[] { EquipmentSlot.Chest });

            Assert.IsFalse(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.RightHand));
            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.Chest));
        }

        [Test]
        public void IsCompatibleWithEquipmentSlot_Ammo_AcceptedInBothHands()
        {
            var item = MakeItem(ItemCategory.Ammo);

            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.RightHand));
            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.LeftHand));
        }

        [Test]
        public void IsCompatibleWithEquipmentSlot_Weapon_OnlyRightHand()
        {
            var item = MakeItem(ItemCategory.Weapon);

            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.RightHand));
            Assert.IsFalse(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.LeftHand));
        }

        [Test]
        public void IsCompatibleWithEquipmentSlot_Tool_OnlyLeftHand()
        {
            var item = MakeItem(ItemCategory.Tool);

            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.LeftHand));
            Assert.IsFalse(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.RightHand));
        }

        [Test]
        public void IsCompatibleWithEquipmentSlot_IdContainsArmor_CompatibleWithChest()
        {
            var item = MakeItem(ItemCategory.Misc, id: "item_iron_armor");

            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.Chest));
        }

        [Test]
        public void IsCompatibleWithEquipmentSlot_IdContainsRingOrAmuletOrAccessory_CompatibleWithAccessory()
        {
            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(MakeItem(ItemCategory.Misc, id: "item_gold_ring"), EquipmentSlot.Accessory));
            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(MakeItem(ItemCategory.Misc, id: "item_lucky_amulet"), EquipmentSlot.Accessory));
            Assert.IsTrue(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(MakeItem(ItemCategory.Misc, id: "item_shiny_accessory"), EquipmentSlot.Accessory));
        }

        [Test]
        public void IsCompatibleWithEquipmentSlot_NonEquippableCategoryAndId_ReturnsFalse()
        {
            var item = MakeItem(ItemCategory.Misc, id: "item_wheat_seed");

            Assert.IsFalse(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.Chest));
            Assert.IsFalse(EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.Accessory));
        }
    }
}
