using System;
using System.Collections.Generic;
using CindarsHope.Core.Data;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    // arch: quebra do par mutuo NPC|UI (2026-07-15) — implementa CindarsHope.NPC.INpcSellPanel para
    // que NpcShopController/NpcShopTransactionFacade consumam via porta, sem nomear este tipo
    // concreto.
    [DisallowMultipleComponent]
    public class SellPanel : MonoBehaviour, CindarsHope.NPC.INpcSellPanel
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private SellPanelItem _itemPrefab;
        [SerializeField] private Text _goldDisplay;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Text _detailsText;
        [SerializeField] private Button _backButton;

        private ShopManager _shopManager;
        private PlayerManager _playerManager;
        private InventoryManager _inventoryManager;
        private ItemDatabaseSO _itemDatabase;
        private CindarsHope.Foundation.IModalRuntime _modalManager;
        private string _shopId;
        private readonly List<SellPanelItem> _displayedItems = new List<SellPanelItem>();
        private bool _isSelling;

        public event Action OnBackPressed;

        private void OnEnable()
        {
            GameEventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            GameEventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            GameEventBus.Subscribe<SkillTreeRespecCompletedEvent>(OnSkillsChanged);
            GameEventBus.Subscribe<SkillDerivedStatsChangedEvent>(OnSkillStatsChanged);
            GameEventBus.Subscribe<SkillNodePurchasedEvent>(OnSkillNodePurchased);
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            GameEventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            GameEventBus.Unsubscribe<SkillTreeRespecCompletedEvent>(OnSkillsChanged);
            GameEventBus.Unsubscribe<SkillDerivedStatsChangedEvent>(OnSkillStatsChanged);
            GameEventBus.Unsubscribe<SkillNodePurchasedEvent>(OnSkillNodePurchased);
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
            }

            ClearItems();
        }

        public void Initialize(ShopManager shopManager, PlayerManager playerManager, InventoryManager inventoryManager, ItemDatabaseSO itemDatabase, CindarsHope.Foundation.IModalRuntime modalManager)
        {
            _shopManager = shopManager;
            _playerManager = playerManager;
            _inventoryManager = inventoryManager;
            _itemDatabase = itemDatabase;
            _modalManager = modalManager;
            _detailsText = ShopPanelLayoutUtility.EnsureResponsiveLayout(transform as RectTransform, _itemsContainer, _detailsText);
            HideVisualOnly();
        }

        public bool IsInitializedWith(ShopManager shopManager, PlayerManager playerManager, InventoryManager inventoryManager, ItemDatabaseSO itemDatabase, CindarsHope.Foundation.IModalRuntime modalManager)
        {
            return _shopManager == shopManager
                && _playerManager == playerManager
                && _inventoryManager == inventoryManager
                && _itemDatabase == itemDatabase
                && _modalManager == modalManager;
        }

        public void Show(string shopId)
        {
            if (_shopManager == null)
            {
                Debug.LogError($"{GetDiagnosticContext()} cannot show shopId '{shopId}': field '_shopManager' is null.", this);
                return;
            }

            if (!_shopManager.TryGetSession(shopId, out _))
            {
                Debug.LogError($"{GetDiagnosticContext()} cannot show shopId '{shopId}': no initialized ShopSession exists. IsInitialized={_shopManager.IsInitialized}. {_shopManager.GetDiagnosticSummary()}", this);
                return;
            }

            if (_modalManager != null && !_modalManager.PushModal(CindarsHope.Foundation.ModalType.Sell))
            {
                Debug.LogWarning("SellPanel rejected because another interactive modal is active.", this);
                return;
            }

            _shopId = shopId;
            ClearItems();
            PopulateItems();
            SetPanelVisible(true);
            SetFeedback(string.Empty);
            if (_displayedItems.Count > 0)
            {
                _displayedItems[0].Focus();
            }
            else
            {
                SetDetails("Selecione um item para ver detalhes.");
            }

            UpdateGoldDisplay();
        }

        public void Hide()
        {
            HideVisualOnly();
            _modalManager?.TryPopIfCurrent(CindarsHope.Foundation.ModalType.Sell);
        }

        public void HideVisualOnly()
        {
            SetPanelVisible(false);
            ClearItems();
        }

        private void PopulateItems()
        {
            if (_itemPrefab == null || _itemsContainer == null || _inventoryManager == null)
            {
                Debug.LogWarning("SellPanel: ItemPrefab, ItemsContainer, or InventoryManager not assigned");
                SetFeedback("Erro ao carregar inventario.");
                return;
            }

            var items = _inventoryManager.Items;
            if (items == null || items.Count == 0)
            {
                SetFeedback("Nenhum item vendavel.");
                return;
            }

            foreach (var kvp in items)
            {
                var itemId = kvp.Key;
                var amount = kvp.Value;
                var eligible = _inventoryManager.GetEligibleAmount(itemId);
                var retained = Math.Max(0, amount - eligible);
                if (amount <= 0 || !SellableItemPolicy.IsSellable(itemId))
                {
                    continue;
                }

                if (!_itemDatabase.TryGetById(itemId, out var itemData) || itemData == null)
                {
                    Debug.LogWarning($"SellPanel: Item '{itemId}' not found in database");
                    continue;
                }

                var item = Instantiate(_itemPrefab, _itemsContainer);
                item.gameObject.SetActive(true);
                item.Initialize(itemData, eligible, retained,
                    requested => _shopManager.TryQuoteSellItem(_shopId, itemId, requested,
                        _inventoryManager, out var quote) ? quote : default,
                    OnItemSellClicked, ShowItemDetails);
                _displayedItems.Add(item);
            }

            if (_displayedItems.Count == 0)
            {
                SetFeedback("Nenhum item vendavel no inventario.");
            }
        }

        private void OnItemSellClicked(string itemId, int amount)
        {
            _isSelling = true;
            ShopTransactionResult result;
            try
            {
                result = _shopManager.TrySellItem(_shopId, itemId, amount,
                    _playerManager, _inventoryManager);
            }
            finally { _isSelling = false; }
            SetFeedback(result.Message);
            if (!result.Success)
            {
                return;
            }

            UpdateGoldDisplay();
            RefreshPanel();
        }

        private void SetFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message ?? string.Empty;
            }
        }

        private void ShowItemDetails(ItemDataSO itemData,
            ShopManager.ShopSellQuote quote, int eligibleAmount, int retainedAmount)
        {
            var value = quote.Success ? $"{quote.FinalGold}g por {quote.SellAmount}"
                : "Sem unidade vendavel";
            SetDetails($"{ItemDisplayNameFormatter.GetTooltip(itemData)}\nVenda: {value}"
                + $" | Vendavel: {eligibleAmount} | Retido: {retainedAmount}"
                + (quote.Success && quote.LimitedUnits > 0
                    ? $" | {quote.LimitedUnits} unidade(s) limitadas pela compra" : string.Empty));
        }

        private void SetDetails(string message)
        {
            var display = _detailsText != null ? _detailsText : _feedbackText;
            if (display != null)
            {
                display.text = message ?? string.Empty;
            }
        }

        private void UpdateGoldDisplay()
        {
            if (_goldDisplay != null && _playerManager != null)
            {
                _goldDisplay.text = $"Ouro: {_playerManager.CurrentGold}g";
            }
        }

        private void RefreshPanel()
        {
            ClearItems();
            PopulateItems();
            if (_displayedItems.Count > 0)
            {
                _displayedItems[0].Focus();
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (!_isSelling && isActiveAndEnabled && !string.IsNullOrWhiteSpace(_shopId))
                RefreshPanel();
        }

        private void OnGoldChanged(GoldChangedEvent evt)
        {
            if (_isSelling || !isActiveAndEnabled) return;
            UpdateGoldDisplay();
            RefreshPanel();
        }

        private void OnSkillsChanged(SkillTreeRespecCompletedEvent evt) => RefreshForPriceChange();
        private void OnSkillStatsChanged(SkillDerivedStatsChangedEvent evt) => RefreshForPriceChange();
        private void OnSkillNodePurchased(SkillNodePurchasedEvent evt) => RefreshForPriceChange();

        private void RefreshForPriceChange()
        {
            if (!_isSelling && isActiveAndEnabled && !string.IsNullOrWhiteSpace(_shopId))
                RefreshPanel();
        }

        private void ClearItems()
        {
            foreach (var item in _displayedItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            _displayedItems.Clear();
        }

        private void SetPanelVisible(bool visible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }

            gameObject.SetActive(visible);
        }

        private void OnBackClicked()
        {
            Hide();
            OnBackPressed?.Invoke();
        }

        private string GetDiagnosticContext()
        {
            return $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(SellPanel)}'";
        }
    }
}
