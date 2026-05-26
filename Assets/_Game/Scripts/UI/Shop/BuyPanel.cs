using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    [DisallowMultipleComponent]
    public class BuyPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private BuyPanelItem _itemPrefab;
        [SerializeField] private Text _goldDisplay;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Button _backButton;

        private ShopManager _shopManager;
        private PlayerManager _playerManager;
        private InventoryManager _inventoryManager;
        private ItemDatabaseSO _itemDatabase;
        private Modal.ModalManager _modalManager;
        private string _shopId;
        private List<BuyPanelItem> _displayedItems = new List<BuyPanelItem>();

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

        public void Initialize(ShopManager shopManager, PlayerManager playerManager, InventoryManager inventoryManager, ItemDatabaseSO itemDatabase, Modal.ModalManager modalManager)
        {
            _shopManager = shopManager;
            _playerManager = playerManager;
            _inventoryManager = inventoryManager;
            _itemDatabase = itemDatabase;
            _modalManager = modalManager;
            Hide();
        }

        public void Show(string shopId)
        {
            if (_shopManager == null || !_shopManager.TryGetSession(shopId, out var session))
            {
                Debug.LogWarning($"BuyPanel cannot show shop '{shopId}'");
                return;
            }

            if (_modalManager != null && !_modalManager.PushModal(Modal.ModalType.Buy))
            {
                Debug.LogWarning("BuyPanel rejected because another interactive modal is active.", this);
                return;
            }

            _shopId = shopId;
            ClearItems();
            PopulateItems(session);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            gameObject.SetActive(true);
            SetFeedback(string.Empty);
            UpdateGoldDisplay();
        }

        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
            _modalManager?.TryPopModal(Modal.ModalType.Buy, out _);
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
                SetFeedback("Sem itens disponíveis.");
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

                var item = Instantiate(_itemPrefab, _itemsContainer);
                item.gameObject.SetActive(true);
                item.Initialize(itemData, session.GetItemStock(entry.ItemId), session.ShopData.BuyPriceMultiplier, _itemDatabase, OnItemBuyClicked);
                _displayedItems.Add(item);
            }

            if (_displayedItems.Count == 0)
            {
                Debug.LogWarning($"BuyPanel: No valid items found for shop '{session.ShopData.Id}'. Check ShopDataSO.Items and ItemDatabaseSO.");
                SetFeedback("Sem itens disponíveis para compra.");
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

        private void OnBackClicked()
        {
            Hide();
            OnBackPressed?.Invoke();
        }
    }

    [DisallowMultipleComponent]
    public class BuyPanelItem : MonoBehaviour
    {
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _priceText;
        [SerializeField] private Text _stockText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private InputField _amountInput;

        private ItemDataSO _itemData;
        private int _currentStock;
        private float _priceMultiplier;
        public string ItemId => _itemData?.Id;

        public void Initialize(ItemDataSO itemData, int stock, float priceMultiplier, ItemDatabaseSO database, Action<string, int> onBuyClicked)
        {
            _itemData = itemData;
            _currentStock = stock;
            _priceMultiplier = priceMultiplier;

            if (_itemNameText != null)
            {
                var name = itemData.DisplayName ?? itemData.Id;
                _itemNameText.text = string.IsNullOrWhiteSpace(itemData.Description)
                    ? name
                    : $"{name}\n{itemData.Description}";
            }

            UpdatePriceDisplay();
            UpdateStockDisplay();

            if (_buyButton != null && onBuyClicked != null)
            {
                _buyButton.onClick.AddListener(() =>
                {
                    var amount = 1;
                    if (_amountInput != null && int.TryParse(_amountInput.text, out var parsed))
                    {
                        amount = Mathf.Max(1, parsed);
                    }

                    onBuyClicked?.Invoke(itemData.Id, amount);
                });
            }
        }

        public void SetStock(int stock)
        {
            _currentStock = stock;
            UpdateStockDisplay();
        }

        private void UpdatePriceDisplay()
        {
            if (_priceText != null && _itemData != null)
            {
                var price = Mathf.RoundToInt(_itemData.BaseValue * _priceMultiplier);
                _priceText.text = $"{price}g";
            }
        }

        private void UpdateStockDisplay()
        {
            if (_stockText != null)
            {
                _stockText.text = $"Estoque: {_currentStock}";
            }

            if (_buyButton != null)
            {
                _buyButton.interactable = _currentStock > 0;
            }
        }
    }
}
