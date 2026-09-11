using System.Collections.Generic;
using CindarsHope.Core.Data;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class ItemDropSpawner : MonoBehaviour
    {
        private const int DroppedItemSortingOrder = 10;

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
            else if (Application.isPlaying) DontDestroyOnLoad(go);
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
            if (transform.parent == null && Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public static ItemDropSpawner Instance => _instance;

        public void Initialize(InventoryManager inventoryManager, ItemDatabaseSO itemDatabase)
        {
            _inventoryManager = inventoryManager;
            _itemDatabase = itemDatabase;
        }

        public bool TryDropItem(string itemId, int amount, Vector3 dropPosition)
            => TryDropItem(itemId, string.Empty, amount, dropPosition);

        public bool TryDropItem(string itemId, string itemInstanceId, int amount, Vector3 dropPosition)
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
            if (!string.IsNullOrWhiteSpace(itemInstanceId) &&
                (itemData.MaxStack != 1 || amount != 1 || !ItemInstanceIdUtility.IsForItem(itemId, itemInstanceId)))
            {
                Debug.LogWarning($"ItemDropSpawner: Invalid instance id '{itemInstanceId}' for '{itemId}'.", this);
                return false;
            }

            var pickupGo = new GameObject($"Dropped_{itemId}");
            pickupGo.transform.position = dropPosition;

            var spriteRenderer = pickupGo.AddComponent<SpriteRenderer>();
            if (itemData.Icon != null)
            {
                spriteRenderer.sprite = itemData.Icon;
            }
            spriteRenderer.sortingLayerName = WorldSortingLayers.World;
            spriteRenderer.sortingOrder = DroppedItemSortingOrder;

            var collider = pickupGo.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;

            var rigidbody = pickupGo.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var pickup = pickupGo.AddComponent<ItemPickup>();
            pickup.Configure(_nextPickupIndex++, itemId, itemInstanceId, amount, _inventoryManager);

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

                var previousCount = _droppedPickups.Count;
                if (TryDropItem(saveData.ItemId, saveData.ItemInstanceId, saveData.Amount, saveData.Position)
                    && _droppedPickups.Count > previousCount)
                {
                    var lastPickup = _droppedPickups[_droppedPickups.Count - 1];
                    lastPickup.RestoreFromSaveData(saveData);
                    _nextPickupIndex = Mathf.Max(_nextPickupIndex, saveData.PickupIndex + 1);
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
