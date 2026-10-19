using System.Collections.Generic;

namespace CindarsHope.UI.Inventory
{
    public class InventoryItemViewModel
    {
        public string SlotId { get; set; }
        public string ItemId { get; set; }
        public string DisplayName { get; set; }
        public string IconId { get; set; }
        public int Quantity { get; set; }
        public string Quality { get; set; }
        public string Rarity { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsStack { get; set; }
        public bool IsInstance { get; set; }
        public bool IsEquippable { get; set; }
        public bool IsSellable { get; set; }
        public bool IsDroppable { get; set; }
        public bool IsMovable { get; set; } = true;
        public bool IsUsable { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsLocked { get; set; }
        public bool IsQuestItem { get; set; }
        public bool IsKeyItem { get; set; }
        public bool IsUnique { get; set; }
        public bool RequiresConfirmationForSell { get; set; }
        public bool RequiresConfirmationForDrop { get; set; }
        public int? ContextualPrice { get; set; }
        public string WarningTextKey { get; set; }
    }

    public class InventoryActionAvailability
    {
        public bool CanUse { get; set; }
        public bool CanEquip { get; set; }
        public bool CanMove { get; set; } = true;
        public bool CanSplit { get; set; }
        public bool CanDrop { get; set; }
        public bool CanSell { get; set; }
        public bool CanFavorite { get; set; } = true;
        public bool CanLock { get; set; } = true;
        public string BlockedReason { get; set; }
        public bool RequiresConfirmation { get; set; }
        public string ConfirmationTextKey { get; set; }
    }

    public class StackSplitRequest
    {
        public string SlotId { get; set; }
        public string ItemId { get; set; }
        public int TotalQuantity { get; set; }
        public int SplitQuantity { get; set; }
        public string TargetSlotId { get; set; }
        public bool IsValid => SplitQuantity > 0 && SplitQuantity < TotalQuantity;
    }

    public static class ProtectedItemActionGuard
    {
        public static InventoryActionAvailability Evaluate(InventoryItemViewModel item)
        {
            if (item == null) return new InventoryActionAvailability { BlockedReason = "ITEM_NULL" };

            var avail = new InventoryActionAvailability { CanMove = true, CanFavorite = true, CanLock = true };

            // Quest/key items: no sell/drop
            if (item.IsQuestItem || item.IsKeyItem)
            {
                avail.CanSell = false;
                avail.CanDrop = false;
                avail.BlockedReason = item.IsQuestItem ? "QUEST_ITEM_PROTECTED" : "KEY_ITEM_PROTECTED";
            }
            else if (item.IsLocked || item.IsFavorite)
            {
                avail.CanSell = false;
                avail.CanDrop = false;
                avail.BlockedReason = item.IsLocked ? "ITEM_LOCKED" : "ITEM_FAVORITED";
            }
            else
            {
                avail.CanSell = item.IsSellable;
                avail.CanDrop = item.IsDroppable;
            }

            // Unique items: require strong confirmation
            if (item.IsUnique && (avail.CanSell || avail.CanDrop))
            {
                avail.RequiresConfirmation = true;
                avail.ConfirmationTextKey = "confirm_unique_item_action";
            }

            avail.CanUse = item.IsUsable;
            avail.CanEquip = item.IsEquippable;
            avail.CanSplit = item.IsStack && item.Quantity > 1;

            return avail;
        }
    }
}
