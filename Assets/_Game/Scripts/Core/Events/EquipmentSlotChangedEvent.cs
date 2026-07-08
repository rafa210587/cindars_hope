using CindarsHope.Foundation;

namespace CindarsHope.Core.Events
{
    public class EquipmentSlotChangedEvent
    {
        public EquipmentSlot Slot { get; }
        public string ItemInstanceId { get; }

        public EquipmentSlotChangedEvent(EquipmentSlot slot, string itemInstanceId)
        {
            Slot = slot;
            ItemInstanceId = itemInstanceId;
        }
    }

    public class DurabilityChangedEvent
    {
        public string ItemInstanceId { get; }
        public int CurrentDurability { get; }
        public int MaxDurability { get; }

        public DurabilityChangedEvent(string itemInstanceId, int current, int max)
        {
            ItemInstanceId = itemInstanceId;
            CurrentDurability = current;
            MaxDurability = max;
        }
    }

    public class ItemBrokenEvent
    {
        public string ItemInstanceId { get; }
        public EquipmentSlot Slot { get; }

        public ItemBrokenEvent(string itemInstanceId, EquipmentSlot slot)
        {
            ItemInstanceId = itemInstanceId;
            Slot = slot;
        }
    }

    public class ItemRepairedEvent
    {
        public string ItemInstanceId { get; }
        public int RestoredDurability { get; }

        public ItemRepairedEvent(string itemInstanceId, int restored)
        {
            ItemInstanceId = itemInstanceId;
            RestoredDurability = restored;
        }
    }
}
