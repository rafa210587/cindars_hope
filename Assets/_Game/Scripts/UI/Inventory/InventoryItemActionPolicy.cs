namespace CindarsHope.UI.Inventory
{
    public enum InventoryItemUseBlockReason
    {
        None,
        EmptySlot,
        UseSystemUnavailable,
        ItemCannotBeUsed
    }

    /// <summary>Defines use readiness and failure precedence for both action selection and execution.</summary>
    public static class InventoryItemActionPolicy
    {
        public static InventoryItemUseBlockReason EvaluateUse(bool hasSelection, bool hasUseSystem, bool canUseItem)
        {
            if (!hasSelection) return InventoryItemUseBlockReason.EmptySlot;
            if (!hasUseSystem) return InventoryItemUseBlockReason.UseSystemUnavailable;
            return canUseItem ? InventoryItemUseBlockReason.None : InventoryItemUseBlockReason.ItemCannotBeUsed;
        }
    }
}
