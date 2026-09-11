using System;

namespace CindarsHope.Inventory
{
    [Serializable]
    public sealed class InventorySlot
    {
        public int SlotIndex;
        public string ItemId = string.Empty;
        /// <summary>
        /// Stable identity for an individual non-stackable item. Empty means legacy/content-only
        /// inventory state and resolves safely to <see cref="ItemId"/>.
        /// </summary>
        public string ItemInstanceId = string.Empty;
        public int Amount;
        public bool IsEquipped;
        public string EquipmentBindingId = string.Empty;

        public bool IsEmpty => string.IsNullOrWhiteSpace(ItemId) || Amount <= 0;
        public string EffectiveItemInstanceId => string.IsNullOrWhiteSpace(ItemInstanceId)
            ? ItemId
            : ItemInstanceId;

        public void Clear()
        {
            ItemId = string.Empty;
            ItemInstanceId = string.Empty;
            Amount = 0;
            IsEquipped = false;
            EquipmentBindingId = string.Empty;
        }
    }
}
