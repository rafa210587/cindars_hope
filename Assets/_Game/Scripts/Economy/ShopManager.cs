using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public sealed class ShopManager : MonoBehaviour
    {
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        private readonly Dictionary<string, ShopSession> _sessions = new Dictionary<string, ShopSession>();

        public bool IsInitialized { get; private set; }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            _sessions.Clear();
            IsInitialized = false;
        }

        public void InitializeShop(ShopDataSO shopData)
        {
            if (shopData == null || string.IsNullOrWhiteSpace(shopData.Id))
            {
                Debug.LogWarning($"{nameof(ShopManager)}: Cannot initialize shop with null data or empty ID.");
                return;
            }

            if (_sessions.ContainsKey(shopData.Id))
            {
                Debug.LogWarning($"{nameof(ShopManager)}: Shop '{shopData.Id}' already initialized.");
                return;
            }

            if (_itemDatabase == null)
            {
                Debug.LogWarning($"{nameof(ShopManager)}: ItemDatabase not assigned. Cannot initialize shops.");
                return;
            }

            var session = new ShopSession(shopData, _itemDatabase);
            session.RestockAllItems(_timeManager != null ? _timeManager.CurrentDay : 0);
            _sessions[shopData.Id] = session;
        }

        public bool TryGetSession(string shopId, out ShopSession session)
        {
            return _sessions.TryGetValue(shopId, out session);
        }

        public bool CanBuyItem(string shopId, string itemId, int amount)
        {
            if (!TryGetSession(shopId, out var session))
            {
                return false;
            }

            return session.GetItemStock(itemId) >= amount;
        }

        public bool TryBuyItem(string shopId, string itemId, int amount, out int totalCost)
        {
            totalCost = 0;

            if (!TryGetSession(shopId, out var session))
            {
                return false;
            }

            if (!session.TryGetItemData(itemId, out var itemData, out var entry))
            {
                return false;
            }

            if (session.GetItemStock(itemId) < amount)
            {
                return false;
            }

            totalCost = Mathf.RoundToInt(itemData.BaseValue * amount * session.ShopData.PriceMultiplier);
            session.DecrementStock(itemId, amount);
            return true;
        }

        public bool TrySellItem(string shopId, string itemId, int amount, out int totalGold)
        {
            totalGold = 0;

            if (!TryGetSession(shopId, out var session))
            {
                return false;
            }

            if (!session.TryGetItemData(itemId, out var itemData, out _))
            {
                return false;
            }

            totalGold = Mathf.RoundToInt(itemData.BaseValue * amount * 0.6f);
            return true;
        }

        public void LoadShopStock(ShopStockSaveData stockData)
        {
            if (stockData == null || string.IsNullOrWhiteSpace(stockData.ShopId))
            {
                return;
            }

            if (!TryGetSession(stockData.ShopId, out var session))
            {
                Debug.LogWarning($"{nameof(ShopManager)}: Cannot load stock for unknown shop '{stockData.ShopId}'.");
                return;
            }

            session.LoadStockData(stockData);
        }

        public ShopStockSaveData CaptureShopStock(string shopId)
        {
            if (!TryGetSession(shopId, out var session))
            {
                return null;
            }

            return session.CaptureSaveData();
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            if (_timeManager == null)
            {
                return;
            }

            foreach (var kvp in _sessions)
            {
                kvp.Value.RestockAllItems(_timeManager.CurrentDay);
            }
        }
    }

    public class ShopSession
    {
        public ShopDataSO ShopData { get; private set; }
        private ItemDatabaseSO _itemDatabase;
        private Dictionary<string, int> _itemStock;
        private int _lastRestockDay = -1;

        public ShopSession(ShopDataSO shopData, ItemDatabaseSO itemDatabase)
        {
            ShopData = shopData;
            _itemDatabase = itemDatabase;
            _itemStock = new Dictionary<string, int>();
            InitializeStock();
        }

        private void InitializeStock()
        {
            _itemStock.Clear();
            if (ShopData.Items == null)
            {
                return;
            }

            foreach (var entry in ShopData.Items)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    _itemStock[entry.ItemId] = 0;
                }
            }
        }

        public int GetItemStock(string itemId)
        {
            return _itemStock.ContainsKey(itemId) ? _itemStock[itemId] : 0;
        }

        public void DecrementStock(string itemId, int amount)
        {
            if (_itemStock.ContainsKey(itemId))
            {
                _itemStock[itemId] = Mathf.Max(0, _itemStock[itemId] - amount);
            }
        }

        public bool TryGetItemData(string itemId, out ItemDataSO itemData, out ShopItemEntry entry)
        {
            itemData = null;
            entry = null;

            if (string.IsNullOrWhiteSpace(itemId) || _itemDatabase == null)
            {
                return false;
            }

            entry = ShopData.GetEntry(itemId);
            if (entry == null)
            {
                return false;
            }

            if (!_itemDatabase.TryGetById(itemId, out itemData) || itemData == null)
            {
                return false;
            }

            return true;
        }

        public void RestockAllItems(int currentDay)
        {
            if (_lastRestockDay == currentDay)
            {
                return;
            }

            _lastRestockDay = currentDay;
            if (ShopData.Items == null)
            {
                return;
            }

            foreach (var entry in ShopData.Items)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    _itemStock[entry.ItemId] = entry.MaxStock;
                }
            }
        }

        public void LoadStockData(ShopStockSaveData data)
        {
            if (data == null)
            {
                return;
            }

            _lastRestockDay = data.LastRestockDay;
            _itemStock.Clear();
            InitializeStock();

            if (data.Items == null)
            {
                return;
            }

            foreach (var item in data.Items)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ItemId))
                {
                    _itemStock[item.ItemId] = item.CurrentStock;
                }
            }
        }

        public ShopStockSaveData CaptureSaveData()
        {
            var data = new ShopStockSaveData
            {
                ShopId = ShopData.Id,
                LastRestockDay = _lastRestockDay
            };

            foreach (var kvp in _itemStock)
            {
                data.Items.Add(new ShopItemStockEntry
                {
                    ItemId = kvp.Key,
                    CurrentStock = kvp.Value
                });
            }

            return data;
        }
    }
}
