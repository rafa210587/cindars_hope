using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Economy.Transactions;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public sealed class ShopManager : MonoBehaviour
    {
        [SerializeField] private ItemDatabaseSO _itemDatabase;

        private readonly Dictionary<string, ShopSession> _sessions = new Dictionary<string, ShopSession>();

        public bool IsInitialized { get; private set; }
        public IReadOnlyCollection<string> RegisteredShopIds => _sessions.Keys;

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
            IsInitialized = true;
        }

        public void Configure(ItemDatabaseSO itemDatabase)
        {
            if (itemDatabase != null)
            {
                _itemDatabase = itemDatabase;
            }

            Initialize();
        }

        public void Shutdown()
        {
            _sessions.Clear();
            IsInitialized = false;
        }

        public bool InitializeShop(ShopDataSO shopData, int currentDay = 1)
        {
            if (shopData == null)
            {
                Debug.LogError($"{GetDiagnosticContext()} cannot initialize shop: field 'shopData' is null.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(shopData.Id))
            {
                Debug.LogError($"{GetDiagnosticContext()} cannot initialize shop: field 'ShopDataSO.Id' is empty.", this);
                return false;
            }

            if (_sessions.ContainsKey(shopData.Id))
            {
                return true;
            }

            if (_itemDatabase == null)
            {
                Debug.LogError($"{GetDiagnosticContext()} cannot initialize shopId '{shopData.Id}': field '_itemDatabase' is null.", this);
                return false;
            }

            if (shopData.Items == null || shopData.Items.Length == 0)
            {
                Debug.LogError($"{GetDiagnosticContext()} cannot initialize shopId '{shopData.Id}': field 'ShopDataSO.Items' is empty.", this);
                return false;
            }

            foreach (var entry in shopData.Items)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    Debug.LogError($"{GetDiagnosticContext()} cannot initialize shopId '{shopData.Id}': field 'ShopDataSO.Items' contains an empty item id.", this);
                    return false;
                }

                if (!_itemDatabase.TryGetById(entry.ItemId, out var itemData) || itemData == null)
                {
                    Debug.LogError($"{GetDiagnosticContext()} cannot initialize shopId '{shopData.Id}': item '{entry.ItemId}' is absent from ItemDatabaseSO.", this);
                    return false;
                }

                if (entry.BuyPriceOverride <= 0 && itemData.BaseValue <= 0)
                {
                    Debug.LogError($"{GetDiagnosticContext()} cannot initialize shopId '{shopData.Id}': item '{entry.ItemId}' has no valid price.", this);
                    return false;
                }
            }

            Initialize();
            var session = new ShopSession(shopData, _itemDatabase);
            session.RestockAllItems(currentDay);
            _sessions[shopData.Id] = session;
            Debug.Log($"{GetDiagnosticContext()} initialized session '{shopData.Id}'. {GetDiagnosticSummary()}", this);
            return true;
        }

        public bool TryGetSession(string shopId, out ShopSession session)
        {
            if (string.IsNullOrWhiteSpace(shopId))
            {
                session = null;
                return false;
            }

            return _sessions.TryGetValue(shopId, out session);
        }

        public bool HasSession(string shopId)
        {
            return !string.IsNullOrWhiteSpace(shopId) && _sessions.ContainsKey(shopId);
        }

        public string GetDiagnosticSummary()
        {
            var sessions = _sessions.Keys.OrderBy(id => id).ToArray();
            return $"ShopManager initialized sessions: {(sessions.Length == 0 ? "<none>" : string.Join(", ", sessions))}";
        }

        public ShopTransactionResult TryBuyItem(
            string shopId,
            string itemId,
            int amount,
            PlayerManager playerManager,
            InventoryManager inventoryManager)
        {
            if (!TryGetSession(shopId, out var session))
            {
                return PublishFailure("ShopBuy", itemId, amount, $"Compra falhou: loja '{shopId}' indisponivel.");
            }

            if (playerManager == null || inventoryManager == null)
            {
                return PublishFailure("ShopBuy", itemId, amount, "Compra falhou: managers de player/inventory ausentes.");
            }

            if (amount <= 0 || !session.TryGetItemData(itemId, out var itemData, out var entry))
            {
                return PublishFailure("ShopBuy", itemId, amount, $"Compra falhou: item '{itemId}' invalido.");
            }

            if (entry.IsFiniteStock && session.GetItemStock(itemId) < amount)
            {
                return PublishFailure("ShopBuy", itemId, amount, $"Compra falhou: estoque insuficiente de '{itemId}'.");
            }

            var totalCost = CalculateBuyPrice(itemData, entry, session.ShopData, amount);
            PurchaseTransactionResult transaction = AtomicPurchaseTransaction.Execute(
                inventoryManager,
                playerManager,
                itemId,
                amount,
                totalCost);
            if (!transaction.Success)
            {
                string failure = transaction.Failure switch
                {
                    PurchaseTransactionFailure.InsufficientFunds =>
                        $"Compra falhou: ouro insuficiente para '{itemId}' x{amount}.",
                    PurchaseTransactionFailure.InventoryFull =>
                        $"Compra falhou: inventario sem espaco para '{itemId}' x{amount}.",
                    PurchaseTransactionFailure.DebitFailed =>
                        $"Compra falhou ao gastar {totalCost}g.",
                    PurchaseTransactionFailure.InventoryWriteFailed =>
                        $"Compra falhou ao adicionar '{itemId}' x{amount}; ouro reembolsado.",
                    _ => $"Compra falhou: transacao invalida para '{itemId}' x{amount}."
                };
                return PublishFailure("ShopBuy", itemId, amount, failure);
            }

            if (entry.IsFiniteStock)
            {
                session.DecrementStock(itemId, amount);
                GameEventBus.Publish(new ShopStockChangedEvent(shopId, itemId, session.GetItemStock(itemId)));
            }

            var message = $"Comprou '{itemId}' x{amount} por {totalCost}g.";
            GameEventBus.Publish(new EconomyTransactionCompletedEvent(true, "ShopBuy", itemId, amount, -totalCost, message));
            return ShopTransactionResult.Succeeded(-totalCost, message);
        }

        public ShopTransactionResult TrySellItem(
            string shopId,
            string itemId,
            int amount,
            PlayerManager playerManager,
            InventoryManager inventoryManager)
        {
            if (!TryGetSession(shopId, out var session))
            {
                return PublishFailure("ShopSell", itemId, amount, $"Venda falhou: loja '{shopId}' indisponivel.");
            }

            if (playerManager == null || inventoryManager == null || amount <= 0)
            {
                return PublishFailure("ShopSell", itemId, amount, "Venda falhou: transacao invalida.");
            }

            if (!SellableItemPolicy.IsSellable(itemId)
                || _itemDatabase == null
                || !_itemDatabase.TryGetById(itemId, out var itemData)
                || itemData == null
                || itemData.BaseValue <= 0)
            {
                return PublishFailure("ShopSell", itemId, amount, $"Venda falhou: item '{itemId}' nao vendavel.");
            }

            if (!inventoryManager.HasItem(itemId, amount))
            {
                return PublishFailure("ShopSell", itemId, amount, $"Venda falhou: quantidade insuficiente de '{itemId}'.");
            }

            var totalGold = CalculateSellPrice(itemData, session.ShopData, amount);
            if (totalGold <= 0 || !inventoryManager.RemoveItem(itemId, amount))
            {
                return PublishFailure("ShopSell", itemId, amount, $"Venda falhou ao remover '{itemId}' x{amount}.");
            }

            playerManager.AddGold(totalGold);
            var message = $"Vendeu '{itemId}' x{amount} por {totalGold}g.";
            GameEventBus.Publish(new EconomyTransactionCompletedEvent(true, "ShopSell", itemId, amount, totalGold, message));
            return ShopTransactionResult.Succeeded(totalGold, message);
        }

        public void LoadShopStock(ShopStockSaveData stockData)
        {
            if (stockData == null || string.IsNullOrWhiteSpace(stockData.ShopId))
            {
                return;
            }

            if (!TryGetSession(stockData.ShopId, out var session))
            {
                Debug.LogWarning($"{nameof(ShopManager)}: Cannot load stock for unknown shop '{stockData.ShopId}'.", this);
                return;
            }

            session.LoadStockData(stockData);
        }

        public List<ShopStockSaveData> CaptureAllShopStock()
        {
            var result = new List<ShopStockSaveData>();
            foreach (var session in _sessions.Values)
            {
                result.Add(session.CaptureSaveData());
            }

            return result;
        }

        public static int CalculateSellPrice(ItemDataSO itemData, ShopDataSO shopData, int amount = 1)
        {
            if (itemData == null || itemData.BaseValue <= 0 || amount <= 0)
            {
                return 0;
            }

            var multiplier = shopData != null ? Mathf.Max(0f, shopData.SellPriceMultiplier) : 0.6f;
            return Mathf.Max(1, Mathf.FloorToInt(itemData.BaseValue * multiplier)) * amount;
        }

        private static int CalculateBuyPrice(ItemDataSO itemData, ShopItemEntry entry, ShopDataSO shopData, int amount)
        {
            var unitPrice = entry.BuyPriceOverride > 0
                ? entry.BuyPriceOverride
                : Mathf.RoundToInt(itemData.BaseValue * Mathf.Max(0f, shopData.BuyPriceMultiplier));
            return Mathf.Max(1, unitPrice) * amount;
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            foreach (var session in _sessions.Values)
            {
                if (session.RestockAllItems(evt.DayNumber))
                {
                    GameEventBus.Publish(new ShopRestockedEvent(session.ShopData.Id));
                }
            }
        }

        private static ShopTransactionResult PublishFailure(string operation, string itemId, int amount, string message)
        {
            GameEventBus.Publish(new EconomyTransactionCompletedEvent(false, operation, itemId, amount, 0, message));
            return ShopTransactionResult.Failed(message);
        }

        private string GetDiagnosticContext()
        {
            return $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(ShopManager)}'";
        }
    }

    public sealed class ShopSession
    {
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly Dictionary<string, int> _itemStock = new Dictionary<string, int>();
        private int _lastRestockDay = -1;

        public ShopDataSO ShopData { get; }

        public ShopSession(ShopDataSO shopData, ItemDatabaseSO itemDatabase)
        {
            ShopData = shopData;
            _itemDatabase = itemDatabase;
            foreach (var entry in ShopData.Items ?? new ShopItemEntry[0])
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    _itemStock[entry.ItemId] = 0;
                }
            }
        }

        public int GetItemStock(string itemId)
        {
            return _itemStock.TryGetValue(itemId, out var stock) ? stock : 0;
        }

        public void DecrementStock(string itemId, int amount)
        {
            if (_itemStock.TryGetValue(itemId, out var stock) && amount > 0)
            {
                _itemStock[itemId] = Mathf.Max(0, stock - amount);
            }
        }

        public bool TryGetItemData(string itemId, out ItemDataSO itemData, out ShopItemEntry entry)
        {
            entry = ShopData.GetEntry(itemId);
            itemData = null;
            return entry != null
                && _itemDatabase != null
                && _itemDatabase.TryGetById(itemId, out itemData)
                && itemData != null;
        }

        public bool RestockAllItems(int currentDay)
        {
            if (!ShopData.DailyRestock || _lastRestockDay == currentDay)
            {
                return false;
            }

            _lastRestockDay = currentDay;
            foreach (var entry in ShopData.Items ?? new ShopItemEntry[0])
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    _itemStock[entry.ItemId] = Mathf.Max(0, entry.BaseDailyStock);
                }
            }

            return true;
        }

        public void LoadStockData(ShopStockSaveData data)
        {
            if (data == null)
            {
                return;
            }

            _lastRestockDay = data.LastRestockDay;
            foreach (var item in data.Items ?? new List<ShopItemStockEntry>())
            {
                var entry = ShopData.GetEntry(item.ItemId);
                if (entry == null)
                {
                    Debug.LogWarning($"Shop '{ShopData.Id}' ignored unknown saved item '{item.ItemId}'.");
                    continue;
                }

                _itemStock[item.ItemId] = Mathf.Clamp(item.CurrentStock, 0, entry.BaseDailyStock);
            }
        }

        public ShopStockSaveData CaptureSaveData()
        {
            var data = new ShopStockSaveData { ShopId = ShopData.Id, LastRestockDay = _lastRestockDay };
            foreach (var item in _itemStock)
            {
                data.Items.Add(new ShopItemStockEntry { ItemId = item.Key, CurrentStock = item.Value });
            }

            return data;
        }
    }

    public readonly struct ShopTransactionResult
    {
        public bool Success { get; }
        public int GoldDelta { get; }
        public string Message { get; }

        private ShopTransactionResult(bool success, int goldDelta, string message)
        {
            Success = success;
            GoldDelta = goldDelta;
            Message = message;
        }

        public static ShopTransactionResult Succeeded(int goldDelta, string message)
        {
            return new ShopTransactionResult(true, goldDelta, message);
        }

        public static ShopTransactionResult Failed(string message)
        {
            return new ShopTransactionResult(false, 0, message);
        }
    }
}
