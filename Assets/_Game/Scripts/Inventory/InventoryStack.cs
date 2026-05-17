using System;

namespace CindarsHope.Inventory
{
    public readonly struct InventoryStack
    {
        public string ItemId { get; }
        public int Amount { get; }

        public InventoryStack(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException("Inventory stack item id cannot be empty.", nameof(itemId));
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Inventory stack amount cannot be negative.");
            }

            ItemId = itemId;
            Amount = amount;
        }
    }
}
