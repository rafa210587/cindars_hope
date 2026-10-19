using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Economy.Transactions;
using CindarsHope.Economy.Pricing;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public sealed class ShopManager : MonoBehaviour, IGameBootstrapRuntimeService, IShopStockRuntime
    {
        // arch: Core|Economy (spec_arch_core_economy_cycle_reduction_v33) — self-registro estatico,
        // molde Audio/AudioManager.cs (ja usado em Core|Enemy e Core|Craft); GameBootstrap nao segura
        // mais [SerializeField] deste manager (nem cria fallback via AddComponent).
        private static ShopManager _instance;
        public static ShopManager Instance => _instance;

        [SerializeField] private ItemDatabaseSO _itemDatabase;

        private readonly Dictionary<string, ShopSession> _sessions = new Dictionary<string, ShopSession>();

        public bool IsInitialized { get; private set; }
        public string BootstrapServiceId => nameof(ShopManager);
        public IReadOnlyCollection<string> RegisteredShopIds => _sessions.Keys;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DomainManagerRegistry.Register<IShopStockRuntime>(this);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            DomainManagerRegistry.Unregister<IShopStockRuntime>(this);
        }

        public void Initialize()
        {
            IsInitialized = true;
        }

        public void InitializeFromBootstrap(GameBootstrapRuntimeContext context)
        {
            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — context.ItemDatabase agora e
            // ScriptableObject (Core parou de nomear CindarsHope.Inventory); cast local para o tipo
            // concreto, permitido pois ShopManager (modulo Economy) ja referencia CindarsHope.Inventory.
            Configure(context.ItemDatabase as ItemDatabaseSO);
        }

        public void Configure(ItemDatabaseSO itemDatabase)
        {
            if (itemDatabase != null)
            {
                _itemDatabase = itemDatabase;
            }

            Initialize();
        }

        public void ShutdownFromBootstrap()
        {
            Shutdown();
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
            if (totalCost <= 0)
            {
                return PublishFailure("ShopBuy", itemId, amount,
                    $"Compra falhou: preco invalido para '{itemId}' x{amount}.");
            }
            PurchaseTransactionResult transaction = AtomicPurchaseTransaction.ExecutePurchased(
                inventoryManager,
                playerManager,
                itemId,
                amount,
                totalCost,
                entry.IsFiniteStock ? new ShopPurchaseStockAdapter(session, shopId, itemId, amount) : null);
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
                        $"Compra falhou ao adicionar '{itemId}' x{amount}.",
                    PurchaseTransactionFailure.StockUnavailable =>
                        $"Compra falhou: estoque insuficiente de '{itemId}'.",
                    _ => $"Compra falhou: transacao invalida para '{itemId}' x{amount}."
                };
                return PublishFailure("ShopBuy", itemId, amount, failure);
            }

            var message = $"Comprou '{itemId}' x{amount} por {totalCost}g.";
            GameEventBus.Publish(new EconomyTransactionCompletedEvent(true, "ShopBuy", itemId, amount, -totalCost, message));
            return ShopTransactionResult.Succeeded(-totalCost, message);
        }

        public readonly struct ShopSellQuote
        {
            public readonly bool Success;
            public readonly int SellAmount;
            public readonly int RetainedAmount;
            public readonly int NormalGold;
            public readonly int FinalGold;
            public readonly int LimitedUnits;
            public readonly InventoryLotSelection Selection;

            public ShopSellQuote(int sellAmount, int retainedAmount, int normalGold,
                int finalGold, int limitedUnits, InventoryLotSelection selection)
            {
                Success = true;
                SellAmount = sellAmount;
                RetainedAmount = retainedAmount;
                NormalGold = normalGold;
                FinalGold = finalGold;
                LimitedUnits = limitedUnits;
                Selection = selection;
            }
        }

        /// <summary>Read-only sale projection; the commit reuses this exact selection and price.</summary>
        public bool TryQuoteSellItem(string shopId, string itemId, int amount,
            InventoryManager inventoryManager, out ShopSellQuote quote)
        {
            quote = default;
            if (!TryGetSession(shopId, out var session) || inventoryManager == null
                || amount <= 0 || !SellableItemPolicy.IsSellable(itemId)
                || _itemDatabase == null || !_itemDatabase.TryGetById(itemId, out var itemData)
                || itemData == null || itemData.BaseValue <= 0)
                return false;

            var sellAmount = System.Math.Min(amount, inventoryManager.GetEligibleAmount(itemId));
            var retainedAmount = System.Math.Max(0,
                System.Math.Min(amount, inventoryManager.GetAmount(itemId)) - sellAmount);
            if (sellAmount <= 0 || !inventoryManager.TryPrepareRemoval(itemId, sellAmount,
                    out var selection)) return false;
            var normalGold = CalculateSellPrice(itemData, session.ShopData, sellAmount);
            var unitGold = AtomicOriginSaleTransaction.SplitAggregatePrice(normalGold, sellAmount);
            var originQuote = OriginAwareSaleQuote.Calculate(selection, unitGold);
            if (normalGold <= 0 || !originQuote.Success) return false;
            quote = new ShopSellQuote(sellAmount, retainedAmount, normalGold,
                originQuote.FinalGold, originQuote.LimitedUnits, selection);
            return true;
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

            if (!TryQuoteSellItem(shopId, itemId, amount, inventoryManager, out var quote))
            {
                var retainedAmount = System.Math.Max(0,
                    System.Math.Min(amount, inventoryManager.GetAmount(itemId))
                    - inventoryManager.GetEligibleAmount(itemId));
                return PublishFailure("ShopSell", itemId, amount,
                    $"Venda falhou: quantidade ou cotacao invalida de '{itemId}'."
                    + (retainedAmount > 0 ? $" {retainedAmount} unidade(s) retida(s)." : string.Empty));
            }

            var sellAmount = quote.SellAmount;
            var retainedAmountConfirmed = quote.RetainedAmount;
            var totalGold = quote.FinalGold;
            if (!AtomicOriginSaleTransaction.TryCommitSingle(inventoryManager, playerManager,
                quote.Selection, totalGold))
            {
                return PublishFailure("ShopSell", itemId, amount,
                    $"Venda falhou ao confirmar '{itemId}' x{sellAmount}.");
            }
            var message = $"Vendeu '{itemId}' x{sellAmount} por {totalGold}g."
                + (retainedAmountConfirmed > 0 ? $" {retainedAmountConfirmed} unidade(s) retida(s)." : string.Empty);
            GameEventBus.Publish(new EconomyTransactionCompletedEvent(true, "ShopSell", itemId,
                sellAmount, totalGold, message));
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

            var profile = PricingProfile.Default();
            profile.ChannelSellMultipliers[PriceChannel.GenericShopBuyFromPlayer] =
                shopData != null ? Mathf.Max(0f, shopData.SellPriceMultiplier) : .6f;
            profile.RoundingRule = RoundingRule.Floor;
            var result = new EconomyPricingService(profile).CalculatePrice(new PriceRequest
            {
                ItemId = itemData.Id,
                Category = itemData.Category,
                BaseValue = itemData.BaseValue,
                Quantity = amount,
                Channel = PriceChannel.GenericShopBuyFromPlayer
            });
            return result.Success ? result.TotalPrice : 0;
        }

        public static int CalculateBuyPrice(ItemDataSO itemData, ShopItemEntry entry, ShopDataSO shopData, int amount = 1)
        {
            if (itemData == null || entry == null || itemData.BaseValue <= 0 || amount <= 0)
                return 0;
            var profile = PricingProfile.Default();
            bool hasOverride = entry.BuyPriceOverride > 0;
            profile.ChannelSellMultipliers[PriceChannel.ShopSellToPlayer] = hasOverride
                ? 1f
                : Mathf.Max(0f, shopData != null ? shopData.BuyPriceMultiplier : 1f);
            profile.RoundingRule = RoundingRule.Round;
            var result = new EconomyPricingService(profile).CalculatePrice(new PriceRequest
            {
                ItemId = itemData.Id,
                Category = itemData.Category,
                BaseValue = hasOverride ? entry.BuyPriceOverride : itemData.BaseValue,
                Quantity = amount,
                Channel = PriceChannel.ShopSellToPlayer
            });
            return result.Success ? result.TotalPrice : 0;
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

        private sealed class ShopPurchaseStockAdapter : IShopPurchaseStockPort
        {
            private readonly ShopSession _session;
            private readonly string _shopId;
            private readonly string _itemId;
            private readonly int _amount;
            private int _stockBefore;
            private bool _reserved;

            public ShopPurchaseStockAdapter(ShopSession session, string shopId, string itemId, int amount)
            {
                _session = session;
                _shopId = shopId;
                _itemId = itemId;
                _amount = amount;
            }

            public bool CanReserve() => _session.GetItemStock(_itemId) >= _amount;

            public bool TryReserve()
            {
                if (_reserved || !CanReserve()) return false;
                _stockBefore = _session.GetItemStock(_itemId);
                _session.DecrementStock(_itemId, _amount);
                _reserved = true;
                return true;
            }

            public bool Rollback()
            {
                if (!_reserved) return false;
                var restored = _session.TryRestoreReservedStock(_itemId,
                    _stockBefore - _amount, _stockBefore);
                if (restored) _reserved = false;
                return restored;
            }

            public void PublishCommittedChange()
            {
                GameEventBus.Publish(new ShopStockChangedEvent(
                    _shopId, _itemId, _session.GetItemStock(_itemId)));
            }
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

        internal bool TryRestoreReservedStock(string itemId, int expectedStock, int previousStock)
        {
            if (!_itemStock.TryGetValue(itemId, out var currentStock)
                || currentStock != expectedStock || previousStock < currentStock) return false;
            _itemStock[itemId] = previousStock;
            return true;
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
