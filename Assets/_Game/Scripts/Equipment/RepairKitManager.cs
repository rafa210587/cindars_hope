using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Equipment
{
    [DisallowMultipleComponent]
    public class RepairKitManager : MonoBehaviour
    {
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private ItemDatabaseSO _itemDatabase;

        public bool TryRepairEquipmentWithKit(EquipmentSlot slot, string repairKitId)
        {
            if (_equipmentManager == null || _inventoryManager == null || _itemDatabase == null)
            {
                Debug.LogError("RepairKitManager: missing dependencies", this);
                return false;
            }

            var equippedItemId = _equipmentManager.GetEquippedItem(slot);
            if (string.IsNullOrEmpty(equippedItemId))
            {
                Debug.LogWarning($"RepairKitManager: no item equipped in {slot}", this);
                return false;
            }

            if (!_itemDatabase.TryGetById(repairKitId, out var kitData) || kitData == null || kitData.ConsumableSubtype != ConsumableSubtype.RepairKit)
            {
                Debug.LogWarning($"RepairKitManager: {repairKitId} is not a valid repair kit", this);
                return false;
            }

            if (kitData.DurabilityRestoreAmount <= 0)
            {
                Debug.LogWarning($"RepairKitManager: {repairKitId} has no durability restore amount", this);
                return false;
            }

            var durData = _equipmentManager.GetItemDurability(equippedItemId);
            if (durData == null)
            {
                Debug.LogWarning($"RepairKitManager: {equippedItemId} has no durability data", this);
                return false;
            }

            _equipmentManager.RepairItem(equippedItemId, kitData.DurabilityRestoreAmount);

            if (!_inventoryManager.RemoveItem(repairKitId, 1))
            {
                Debug.LogWarning($"RepairKitManager: could not remove repair kit from inventory", this);
                return false;
            }

            Debug.Log($"RepairKitManager: repaired {equippedItemId} in {slot} with {repairKitId} (+{kitData.DurabilityRestoreAmount} durability)", this);
            return true;
        }

        public bool CanRepairEquipment(EquipmentSlot slot, string repairKitId)
        {
            if (_equipmentManager == null || _inventoryManager == null || _itemDatabase == null)
                return false;

            var equippedItemId = _equipmentManager.GetEquippedItem(slot);
            if (string.IsNullOrEmpty(equippedItemId))
                return false;

            if (!_itemDatabase.TryGetById(repairKitId, out var kitData) || kitData == null || kitData.ConsumableSubtype != ConsumableSubtype.RepairKit)
                return false;

            if (kitData.DurabilityRestoreAmount <= 0)
                return false;

            var durData = _equipmentManager.GetItemDurability(equippedItemId);
            if (durData == null || (durData.IsBroken && durData.MaxDurability <= 0))
                return false;

            return _inventoryManager.HasItem(repairKitId);
        }
    }
}
