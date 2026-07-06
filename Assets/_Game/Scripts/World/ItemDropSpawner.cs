using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class ItemDropSpawner : MonoBehaviour
    {
        private static ItemDropSpawner _instance;

        private InventoryManager _inventoryManager;
        private ItemDatabaseSO _itemDatabase;
        private readonly List<ItemPickup> _droppedPickups = new List<ItemPickup>();
        private int _nextPickupIndex = 10000;

        public static ItemDropSpawner Install(Transform owner)
        {
            if (_instance != null)
            {
                return _instance;
            }

            var go = new GameObject("ItemDropSpawner");
            if (owner != null) go.transform.SetParent(owner, false);
            else DontDestroyOnLoad(go);
            _instance = go.AddComponent<ItemDropSpawner>();
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

        public static ItemDropSpawner Instance => _instance;

        public void Initialize(InventoryManager inventoryManager, ItemDatabaseSO itemDatabase)
        {
            _inventoryManager = inventoryManager;
            _itemDatabase = itemDatabase;
        }

        public bool TryDropItem(string itemId, int amount, Vector3 dropPosition)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                Debug.LogWarning("ItemDropSpawner: Invalid item id or amount.", this);
                return false;
            }

            if (_inventoryManager == null)
            {
                Debug.LogWarning("ItemDropSpawner: InventoryManager not initialized.", this);
                return false;
            }

            if (!_inventoryManager.TryGetItemData(itemId, out var itemData))
            {
                Debug.LogWarning($"ItemDropSpawner: Unknown item id '{itemId}'.", this);
                return false;
            }

            var pickupGo = new GameObject($"Dropped_{itemId}");
            pickupGo.transform.position = dropPosition;

            var spriteRenderer = pickupGo.AddComponent<SpriteRenderer>();
            if (itemData.Icon != null)
            {
                spriteRenderer.sprite = itemData.Icon;
            }

            var collider = pickupGo.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;

            var pickup = pickupGo.AddComponent<ItemPickup>();
            pickup.Configure(_nextPickupIndex++, itemId, amount, _inventoryManager);

            _droppedPickups.Add(pickup);

            Debug.Log($"ItemDropSpawner: Dropped '{itemId}' x{amount} at {dropPosition}.", this);
            return true;
        }

        public List<ItemPickupSaveData> CaptureDynamicPickupSaveData()
        {
            var saveData = new List<ItemPickupSaveData>();
            foreach (var pickup in _droppedPickups)
            {
                if (pickup != null && !pickup.IsCollected)
                {
                    saveData.Add(pickup.CaptureSaveData());
                }
            }

            return saveData;
        }

        public void RestoreDynamicPickups(List<ItemPickupSaveData> pickupsSaveData)
        {
            if (pickupsSaveData == null)
            {
                return;
            }

            foreach (var saveData in pickupsSaveData)
            {
                if (saveData == null || string.IsNullOrWhiteSpace(saveData.ItemId))
                {
                    continue;
                }

                TryDropItem(saveData.ItemId, saveData.Amount, saveData.Position);
                if (_droppedPickups.Count > 0)
                {
                    var lastPickup = _droppedPickups[_droppedPickups.Count - 1];
                    lastPickup.SetCollected(saveData.IsCollected);
                }
            }
        }

        public void Clear()
        {
            foreach (var pickup in _droppedPickups)
            {
                if (pickup != null && pickup.gameObject != null)
                {
                    Destroy(pickup.gameObject);
                }
            }

            _droppedPickups.Clear();
        }
    }
}
