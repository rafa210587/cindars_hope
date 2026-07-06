using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Inventory
{
    [DisallowMultipleComponent]
    public class ItemUseManager : MonoBehaviour
    {
        private static ItemUseManager _instance;

        private readonly Dictionary<string, ItemUseHandler> _handlers = new Dictionary<string, ItemUseHandler>();
        private InventoryManager _inventoryManager;

        public static ItemUseManager Install(Transform owner)
        {
            if (_instance != null)
            {
                return _instance;
            }

            var go = new GameObject("ItemUseManager");
            if (owner != null) go.transform.SetParent(owner, false);
            else DontDestroyOnLoad(go);
            _instance = go.AddComponent<ItemUseManager>();
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        public static ItemUseManager Instance => _instance;

        public void Initialize(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
        }

        public void RegisterHandler(string itemId, ItemUseHandler handler)
        {
            if (string.IsNullOrWhiteSpace(itemId) || handler == null)
            {
                Debug.LogWarning("ItemUseManager: Cannot register null handler or empty item id.", this);
                return;
            }

            _handlers[itemId] = handler;
        }

        public bool CanUseItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || !_handlers.TryGetValue(itemId, out var handler))
            {
                return false;
            }

            if (_inventoryManager == null || !_inventoryManager.TryGetItemData(itemId, out var itemData))
            {
                return false;
            }

            var isConsumable = itemData.Category == ItemCategory.Food
                || itemData.Category == ItemCategory.Consumable
                || itemData.ConsumableSubtype != ConsumableSubtype.None;

            return isConsumable && handler.CanUseItem(itemId, _inventoryManager.GetAmount(itemId));
        }

        public bool TryUseItem(string itemId, GameObject user)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                Debug.LogWarning("ItemUseManager: Invalid item id.", this);
                return false;
            }

            if (_inventoryManager == null)
            {
                Debug.LogWarning("ItemUseManager: InventoryManager not initialized.", this);
                return false;
            }

            if (!_handlers.TryGetValue(itemId, out var handler))
            {
                Debug.LogWarning($"ItemUseManager: No handler registered for item '{itemId}'.", this);
                return false;
            }

            var amount = _inventoryManager.GetAmount(itemId);
            if (amount <= 0)
            {
                Debug.LogWarning($"ItemUseManager: Item '{itemId}' not in inventory.", this);
                return false;
            }

            if (!handler.TryUseItem(itemId, 1, user))
            {
                Debug.LogWarning($"ItemUseManager: Handler failed for item '{itemId}'.", this);
                return false;
            }

            if (!_inventoryManager.RemoveItem(itemId, 1))
            {
                Debug.LogWarning($"ItemUseManager: Could not remove '{itemId}' from inventory after use.", this);
                return false;
            }

            GameEventBus.Publish(new ItemUsedEvent(itemId, 1));
            return true;
        }
    }
}
