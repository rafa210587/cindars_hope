using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Items;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class ItemDefinitionTests
    {
        private ItemDefinition SellableItem() => new ItemDefinition
        {
            ItemId = "item_carrot", DisplayName = "Cenoura",
            Category = ItemCategory.Crop, Rarity = ItemRarity.Common,
            BaseValue = 10, MaxStack = 99, QualityEnabled = true,
            Tags = ItemTag.Sellable | ItemTag.Stackable | ItemTag.CookingIngredient
        };

        private ItemDefinition QuestItem() => new ItemDefinition
        {
            ItemId = "item_ancient_key", IsQuestItem = true, BaseValue = 0,
            Category = ItemCategory.Quest, Rarity = ItemRarity.Unique
        };

        private ItemDefinition KeyItem() => new ItemDefinition
        {
            ItemId = "item_key_ring", IsKeyItem = true, BaseValue = 0, Category = ItemCategory.KeyItem
        };

        private ItemDefinition UniqueItem() => new ItemDefinition
        {
            ItemId = "item_unique_sword", IsUnique = true, BaseValue = 500,
            StackBehavior = ItemStackBehavior.InstanceOnly
        };

        [Test]
        public void SellableItem_CanSell_WhenTaggedSellable()
        {
            var item = SellableItem();
            Assert.IsTrue(item.EconomicFlags.CanSell);
        }

        [Test]
        public void QuestItem_CannotSell()
        {
            var item = QuestItem();
            Assert.IsFalse(item.EconomicFlags.CanSell);
            Assert.IsFalse(item.EconomicFlags.CanDiscard);
        }

        [Test]
        public void KeyItem_CannotSell_CannotDiscard()
        {
            var item = KeyItem();
            Assert.IsFalse(item.EconomicFlags.CanSell);
            Assert.IsFalse(item.EconomicFlags.CanDiscard);
        }

        [Test]
        public void UniqueItem_CannotSell_RequiresConfirmation()
        {
            var item = UniqueItem();
            Assert.IsFalse(item.EconomicFlags.CanSell);
            Assert.IsTrue(item.EconomicFlags.RequiresStrongDiscardConfirmation);
        }

        [Test]
        public void ItemDefinition_StackableItem_IsStackable()
        {
            var item = SellableItem();
            Assert.IsTrue(item.IsStackable);
        }

        [Test]
        public void ItemDefinition_UniqueItem_IsInstance()
        {
            var item = UniqueItem();
            Assert.IsTrue(item.IsInstance);
        }

        [Test]
        public void Validator_SellableWithBaseValue_NoErrors()
        {
            var item = SellableItem();
            var errors = ItemDefinitionValidator.Validate(item);
            Assert.AreEqual(0, errors.Count, string.Join(", ", errors));
        }

        [Test]
        public void Validator_SellableWithZeroBaseValue_HasError()
        {
            var item = SellableItem();
            item.BaseValue = 0;
            var errors = ItemDefinitionValidator.Validate(item);
            Assert.Greater(errors.Count, 0);
        }

        [Test]
        public void Quality_Distinct_From_Rarity()
        {
            // Quality = production excellence; Rarity = availability/weight
            Assert.AreNotEqual((int)ItemQuality.Q3_RaraOuPerfeita, (int)ItemRarity.Rare);
        }

        [Test]
        public void ItemTag_HasFlag_Works()
        {
            var item = SellableItem();
            Assert.IsTrue(item.HasTag(ItemTag.Sellable));
            Assert.IsTrue(item.HasTag(ItemTag.CookingIngredient));
            Assert.IsFalse(item.HasTag(ItemTag.LoreLocked));
        }
    }
}
