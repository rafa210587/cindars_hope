using NUnit.Framework;
using CindarsHope.UI.Inventory;
using CindarsHope.UI.Equipment;
using CindarsHope.UI.Tooltips;

namespace CindarsHope.Tests.EditMode.UI
{
    [TestFixture]
    public class InventoryEquipmentTooltipTests
    {
        // ---- InventoryItemViewModel ----

        [Test]
        public void InventoryItemViewModel_HasRequiredFields()
        {
            var vm = new InventoryItemViewModel
            {
                SlotId = "slot_1", ItemId = "herb_01", DisplayName = "Erva", Quantity = 5,
                Quality = "Common", Rarity = "Common", IsSellable = true, IsDroppable = true
            };
            Assert.AreEqual("herb_01", vm.ItemId);
            Assert.AreEqual(5, vm.Quantity);
            Assert.IsTrue(vm.IsSellable);
            Assert.IsTrue(vm.IsMovable);
        }


        // ---- ProtectedItemActionGuard ----

        [Test]
        public void Guard_QuestItem_CannotSellOrDrop()
        {
            var item = new InventoryItemViewModel { IsQuestItem = true, IsSellable = true, IsDroppable = true };
            var avail = ProtectedItemActionGuard.Evaluate(item);
            Assert.IsFalse(avail.CanSell);
            Assert.IsFalse(avail.CanDrop);
            Assert.AreEqual("QUEST_ITEM_PROTECTED", avail.BlockedReason);
        }

        [Test]
        public void Guard_KeyItem_CannotSellOrDrop()
        {
            var item = new InventoryItemViewModel { IsKeyItem = true, IsSellable = true, IsDroppable = true };
            var avail = ProtectedItemActionGuard.Evaluate(item);
            Assert.IsFalse(avail.CanSell);
            Assert.IsFalse(avail.CanDrop);
            Assert.AreEqual("KEY_ITEM_PROTECTED", avail.BlockedReason);
        }

        [Test]
        public void Guard_LockedItem_CannotSellOrDrop()
        {
            var item = new InventoryItemViewModel { IsLocked = true, IsSellable = true, IsDroppable = true };
            var avail = ProtectedItemActionGuard.Evaluate(item);
            Assert.IsFalse(avail.CanSell);
            Assert.IsFalse(avail.CanDrop);
            Assert.AreEqual("ITEM_LOCKED", avail.BlockedReason);
        }

        [Test]
        public void Guard_FavoriteItem_CannotSellOrDrop()
        {
            var item = new InventoryItemViewModel { IsFavorite = true, IsSellable = true, IsDroppable = true };
            var avail = ProtectedItemActionGuard.Evaluate(item);
            Assert.IsFalse(avail.CanSell);
            Assert.IsFalse(avail.CanDrop);
        }

        [Test]
        public void Guard_NormalSellableItem_CanSellAndDrop()
        {
            var item = new InventoryItemViewModel { IsSellable = true, IsDroppable = true };
            var avail = ProtectedItemActionGuard.Evaluate(item);
            Assert.IsTrue(avail.CanSell);
            Assert.IsTrue(avail.CanDrop);
        }

        [Test]
        public void Guard_UniqueItem_RequiresConfirmation()
        {
            var item = new InventoryItemViewModel { IsUnique = true, IsSellable = true, IsDroppable = true };
            var avail = ProtectedItemActionGuard.Evaluate(item);
            Assert.IsTrue(avail.RequiresConfirmation);
            Assert.IsNotEmpty(avail.ConfirmationTextKey);
        }

        [Test]
        public void Guard_NullItem_ReturnsBlockedReason()
        {
            var avail = ProtectedItemActionGuard.Evaluate(null);
            Assert.AreEqual("ITEM_NULL", avail.BlockedReason);
        }

        [Test]
        public void Guard_StackItem_CanSplit()
        {
            var item = new InventoryItemViewModel { IsStack = true, Quantity = 10 };
            var avail = ProtectedItemActionGuard.Evaluate(item);
            Assert.IsTrue(avail.CanSplit);
        }

        [Test]
        public void Guard_SingleItem_CannotSplit()
        {
            var item = new InventoryItemViewModel { IsStack = false, Quantity = 1 };
            var avail = ProtectedItemActionGuard.Evaluate(item);
            Assert.IsFalse(avail.CanSplit);
        }

        // ---- StackSplitRequest ----

        [Test]
        public void StackSplitRequest_IsValid_WhenSplitInRange()
        {
            var req = new StackSplitRequest { TotalQuantity = 10, SplitQuantity = 3 };
            Assert.IsTrue(req.IsValid);
        }

        [Test]
        public void StackSplitRequest_IsNotValid_WhenSplitEqualsTotal()
        {
            var req = new StackSplitRequest { TotalQuantity = 10, SplitQuantity = 10 };
            Assert.IsFalse(req.IsValid);
        }

        [Test]
        public void StackSplitRequest_IsNotValid_WhenSplitIsZero()
        {
            var req = new StackSplitRequest { TotalQuantity = 10, SplitQuantity = 0 };
            Assert.IsFalse(req.IsValid);
        }

        // ---- EquipmentSlotViewModel ----

        [Test]
        public void EquipmentSlotViewModel_Empty_WhenNoItemEquipped()
        {
            var slot = new EquipmentSlotViewModel { SlotType = "weapon" };
            Assert.IsTrue(slot.IsEmpty);
        }

        [Test]
        public void EquipmentSlotViewModel_DurabilityPercent_Calculated()
        {
            var slot = new EquipmentSlotViewModel { DurabilityCurrent = 50, DurabilityMax = 100 };
            Assert.AreEqual(0.5f, slot.DurabilityPercent, 0.001f);
        }

        // ---- EquipmentComparisonViewModel ----

        [Test]
        public void EquipmentComparison_PreviewOnly_Default()
        {
            var vm = new EquipmentComparisonViewModel();
            Assert.IsTrue(vm.PreviewOnly);
        }

        [Test]
        public void EquipmentComparison_HasComparison_WhenBothItems()
        {
            var vm = new EquipmentComparisonViewModel
            {
                Current = new EquipmentComparisonItem { ItemId = "sword_01", AttackPower = 10, Defense = 2 },
                Candidate = new EquipmentComparisonItem { ItemId = "sword_02", AttackPower = 15, Defense = 1 }
            };
            Assert.IsTrue(vm.HasComparison);
            Assert.AreEqual(5, vm.AttackDelta);
            Assert.AreEqual(-1, vm.DefenseDelta);
        }


        // ---- ItemTooltipViewModel ----

        [Test]
        public void ItemTooltipViewModel_SpoilerSafe_Default()
        {
            var vm = new ItemTooltipViewModel();
            Assert.IsTrue(vm.SpoilerSafe);
        }


        // ---- TooltipLayerPolicy ----

        [Test]
        public void TooltipLayerPolicy_ShopContext_ShowsContextualPrice()
        {
            Assert.IsTrue(TooltipLayerPolicy.ShowContextualPrice(TooltipContext.ShopSell));
            Assert.IsTrue(TooltipLayerPolicy.ShowContextualPrice(TooltipContext.ShopBuy));
            Assert.IsFalse(TooltipLayerPolicy.ShowContextualPrice(TooltipContext.Inventory));
        }

        [Test]
        public void TooltipLayerPolicy_EquipmentContext_ShowsEquipmentBlock()
        {
            Assert.IsTrue(TooltipLayerPolicy.ShowEquipmentBlock(TooltipContext.Equipment));
            Assert.IsFalse(TooltipLayerPolicy.ShowEquipmentBlock(TooltipContext.SkillMagic));
        }

        [Test]
        public void TooltipLayerPolicy_DebugContext_IsNotSpoilerSafe()
        {
            Assert.IsFalse(TooltipLayerPolicy.IsSpoilerSafe(TooltipContext.Debug));
            Assert.IsTrue(TooltipLayerPolicy.IsSpoilerSafe(TooltipContext.Inventory));
        }

        [Test]
        public void TooltipLayerPolicy_QuestItem_ShowsWarning_InAllContexts()
        {
            Assert.IsTrue(TooltipLayerPolicy.ShowQuestKeyWarning(TooltipContext.Inventory, true));
            Assert.IsTrue(TooltipLayerPolicy.ShowQuestKeyWarning(TooltipContext.ShopSell, true));
            Assert.IsFalse(TooltipLayerPolicy.ShowQuestKeyWarning(TooltipContext.Inventory, false));
        }
    }
}
