using System;
using System.Collections.Generic;

namespace CindarsHope.Inventory
{
    // arch: movido de CindarsHope.Save (SaveData.cs) para co-localizar com o owner InventoryManager,
    // quebrando o ciclo mutuo Inventory|Save (precedente: NpcManagerSaveData/NpcSaveData movidos
    // para CindarsHope.NPC). JsonUtility serializa por nome de campo, nao por namespace/type-name —
    // sem migration.
    [Serializable]
    public class InventorySaveData
    {
        public int Capacity;
        public List<InventorySlotSaveData> Slots = new List<InventorySlotSaveData>();
        public List<InventoryItemSaveData> Items = new List<InventoryItemSaveData>();
    }

    [Serializable]
    public class InventorySlotSaveData
    {
        public int SlotIndex;
        public string ItemId;
        public int Amount;
        public bool IsEquipped;
        public string EquipmentBindingId;
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public string ItemId;
        public int Amount;
    }
}
