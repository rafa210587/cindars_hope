using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Thin adapter wrapping InventoryManager for QuestService.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    /// </summary>
    public class QuestInventoryAdapter : IQuestInventoryAccess
    {
        private readonly InventoryManager _inventoryManager;

        public QuestInventoryAdapter(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
        }

        public int GetItemCount(string itemId)
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning("[QuestInventoryAdapter] InventoryManager is null.");
                return 0;
            }
            return _inventoryManager.GetAmount(itemId);
        }

        public bool TryAddItem(string itemId, int count)
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning("[QuestInventoryAdapter] InventoryManager is null.");
                return false;
            }
            return _inventoryManager.AddItem(itemId, count);
        }

        public bool TryRemoveItems(string itemId, int count)
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning("[QuestInventoryAdapter] InventoryManager is null.");
                return false;
            }
            return _inventoryManager.RemoveItem(itemId, count);
        }
    }
}
