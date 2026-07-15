using System;
using System.Collections.Generic;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    // arch: quebra do par mutuo NPC|UI (2026-07-15) — implementa CindarsHope.NPC.INpcBuyPanel para
    // que NpcShopController/NpcShopTransactionFacade consumam via porta, sem nomear este tipo
    // concreto.
    [DisallowMultipleComponent]
    public class BuyPanel : MonoBehaviour, CindarsHope.NPC.INpcBuyPanel
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private BuyPanelItem _itemPrefab;
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
        private readonly List<BuyPanelItem> _displayedItems = new List<BuyPanelItem>();

        public event Action OnBackPressed;

        private void OnEnable()
        {
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnDisable()
        {
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

            if (!_shopManager.TryGetSession(shopId, out var session))
            {
                Debug.LogError($"{GetDiagnosticContext()} cannot show shopId '{shopId}': no initialized ShopSession exists. IsInitialized={_shopManager.IsInitialized}. {_shopManager.GetDiagnosticSummary()}", this);
                return;
            }

            if (_modalManager != null && !_modalManager.PushModal(CindarsHope.Foundation.ModalType.Buy))
            {
                Debug.LogWarning("BuyPanel rejected because another interactive modal is active.", this);
                return;
            }

            _shopId = shopId;
            ClearItems();
            PopulateItems(session);
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
            _modalManager?.TryPopIfCurrent(CindarsHope.Foundation.ModalType.Buy);
        }

        public void HideVisualOnly()
        {
            SetPanelVisible(false);
            ClearItems();
        }

        private void PopulateItems(ShopSession session)
        {
            if (_itemPrefab == null || _itemsContainer == null)
            {
                Debug.LogWarning("BuyPanel: ItemPrefab or ItemsContainer not assigned");
                SetFeedback("Erro ao carregar itens.");
                return;
            }

            if (session.ShopData.Items == null || session.ShopData.Items.Length == 0)
            {
                Debug.LogWarning($"BuyPanel: Shop '{session.ShopData.Id}' has no items configured");
                SetFeedback("Sem itens disponiveis.");
                return;
            }

            foreach (var entry in session.ShopData.Items)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    continue;
                }

                if (!_itemDatabase.TryGetById(entry.ItemId, out var itemData) || itemData == null)
                {
                    Debug.LogWarning($"BuyPanel: Item '{entry.ItemId}' not found in database for shop '{session.ShopData.Id}'");
                    continue;
                }

                var unitPrice = entry.BuyPriceOverride > 0
                    ? entry.BuyPriceOverride
                    : Mathf.Max(1, Mathf.RoundToInt(itemData.BaseValue * session.ShopData.BuyPriceMultiplier));
                var item = Instantiate(_itemPrefab, _itemsContainer);
                item.gameObject.SetActive(true);
                item.Initialize(itemData, session.GetItemStock(entry.ItemId), unitPrice, OnItemBuyClicked, ShowItemDetails);
                _displayedItems.Add(item);
            }

            if (_displayedItems.Count == 0)
            {
                Debug.LogWarning($"BuyPanel: No valid items found for shop '{session.ShopData.Id}'. Check ShopDataSO.Items and ItemDatabaseSO.");
                SetFeedback("Sem itens disponiveis para compra.");
            }
        }

        private void OnItemBuyClicked(string itemId, int amount)
        {
            var result = _shopManager.TryBuyItem(_shopId, itemId, amount, _playerManager, _inventoryManager);
            SetFeedback(result.Message);
            if (!result.Success)
            {
                return;
            }

            UpdateGoldDisplay();
            RefreshItemStock();
        }

        private void SetFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message ?? string.Empty;
            }
        }

        private void ShowItemDetails(ItemDataSO itemData, int unitPrice, int stock)
        {
            SetDetails($"{ItemDisplayNameFormatter.GetTooltip(itemData)}\nCompra: {unitPrice}g | Estoque: {stock}");
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

        private void RefreshItemStock()
        {
            if (!_shopManager.TryGetSession(_shopId, out var session))
            {
                return;
            }

            foreach (var item in _displayedItems)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ItemId))
                {
                    item.SetStock(session.GetItemStock(item.ItemId));
                }
            }
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
            return $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(BuyPanel)}'";
        }
    }
}
