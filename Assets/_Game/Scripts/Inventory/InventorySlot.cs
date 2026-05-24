using System;

namespace CindarsHope.Inventory
{
    [Serializable]
    public sealed class InventorySlot
    {
        public int SlotIndex;
        public string ItemId = string.Empty;
        public int Amount;
        public bool IsEquipped;
        public string EquipmentBindingId = string.Empty;

        public bool IsEmpty => string.IsNullOrWhiteSpace(ItemId) || Amount <= 0;

        public void Clear()
        {
            ItemId = string.Empty;
            Amount = 0;
            IsEquipped = false;
            EquipmentBindingId = string.Empty;
        }
    }
}
