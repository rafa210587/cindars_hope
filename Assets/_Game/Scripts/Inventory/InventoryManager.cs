using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;
using UnityEngine;

namespace CindarsHope.Inventory
{
    [DisallowMultipleComponent]
    public class InventoryManager : MonoBehaviour
    {
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
        private ItemDatabaseSO _itemDatabase;

        public bool IsInitialized { get; private set; }
        public bool HasItemDatabase => _itemDatabase != null;
        public IReadOnlyDictionary<string, int> Items => _items;

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void Initialize(ItemDatabaseSO itemDatabase)
        {
            Initialize();

            if (itemDatabase == null)
            {
                Debug.LogWarning("InventoryManager initialized without ItemDatabaseSO. Item operations will reject unknown ids until a database is assigned.", this);
                return;
            }

            _itemDatabase = itemDatabase;
        }

        public void InitializeFromStartingItems(PlayerDataSO playerData, ItemDatabaseSO itemDatabase)
        {
            Initialize(itemDatabase);
            Clear();

            if (playerData == null)
            {
                Debug.LogWarning("InventoryManager cannot initialize starting items because PlayerDataSO is missing.", this);
                return;
            }

            if (itemDatabase == null)
            {
                Debug.LogWarning("InventoryManager cannot initialize starting items because ItemDatabaseSO is missing.", this);
                return;
            }

            if (playerData.StartingItems == null)
            {
                return;
            }

            foreach (var startingItem in playerData.StartingItems)
            {
                if (startingItem.Item == null)
                {
                    Debug.LogWarning("InventoryManager skipped a null starting item.", this);
                    continue;
                }

                if (startingItem.Amount <= 0)
                {
                    Debug.LogWarning($"InventoryManager skipped starting item '{startingItem.Item.Id}' with invalid amount {startingItem.Amount}.", this);
                    continue;
                }

                AddItem(startingItem.Item.Id, startingItem.Amount);
            }
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            Clear();
            IsInitialized = false;
        }

        public void Clear()
        {
            if (_items.Count == 0)
            {
                return;
            }

            var removedItems = new List<KeyValuePair<string, int>>(_items);
            foreach (var item in removedItems)
            {
                GameEventBus.Publish(new InventoryChangedEvent(item.Key, -item.Value, 0));
            }

            _items.Clear();
        }

        public int GetAmount(string itemId)
        {
            return string.IsNullOrWhiteSpace(itemId) || !_items.TryGetValue(itemId, out var amount) ? 0 : amount;
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            return GetAmount(itemId) >= amount;
        }

        public bool IsKnownItem(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId)
                && _itemDatabase != null
                && _itemDatabase.TryGetById(itemId, out _);
        }

        public bool TryGetItemData(string itemId, out ItemDataSO itemData)
        {
            itemData = null;
            return !string.IsNullOrWhiteSpace(itemId)
                && _itemDatabase != null
                && _itemDatabase.TryGetById(itemId, out itemData)
                && itemData != null;
        }

        public bool AddItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (amount <= 0)
            {
                return false;
            }

            if (_itemDatabase == null || !_itemDatabase.TryGetById(itemId, out var itemData))
            {
                Debug.LogWarning($"InventoryManager rejected unknown item id '{itemId}'.", this);
                return false;
            }

            var currentAmount = GetAmount(itemId);
            var maxAmount = itemData.MaxStack;
            if (currentAmount >= maxAmount)
            {
                return false;
            }

            var newAmount = Mathf.Min(currentAmount + amount, maxAmount);
            var delta = newAmount - currentAmount;
            if (delta <= 0)
            {
                return false;
            }

            _items[itemId] = newAmount;
            GameEventBus.Publish(new InventoryChangedEvent(itemId, delta, newAmount));
            return true;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            var currentAmount = GetAmount(itemId);
            if (currentAmount < amount)
            {
                return false;
            }

            var newAmount = currentAmount - amount;
            if (newAmount == 0)
            {
                _items.Remove(itemId);
            }
            else
            {
                _items[itemId] = newAmount;
            }

            GameEventBus.Publish(new InventoryChangedEvent(itemId, -amount, newAmount));
            return true;
        }
    }
}
