using System;
using System.Collections.Generic;
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
    [DisallowMultipleComponent]
    public class BuyPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private BuyPanelItem _itemPrefab;
        [SerializeField] private Text _goldDisplay;
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

        private void Update()
        {
            if (_canvasGroup != null && _canvasGroup.interactable && Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackClicked();
            }
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
            _modalManager?.PushModal(Modal.ModalType.Buy);
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
                return;
            }

            if (session.ShopData.Items == null)
            {
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
                    Debug.LogWarning($"BuyPanel: Item '{entry.ItemId}' not found in database");
                    continue;
                }

                var item = Instantiate(_itemPrefab, _itemsContainer);
                item.Initialize(itemData, session.GetItemStock(entry.ItemId), session.ShopData.PriceMultiplier, _itemDatabase, OnItemBuyClicked);
                _displayedItems.Add(item);
            }
        }

        private void OnItemBuyClicked(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(_shopId) || !_shopManager.TryBuyItem(_shopId, itemId, amount, out var totalCost))
            {
                Debug.LogWarning($"BuyPanel: Failed to buy {itemId} x{amount}");
                return;
            }

            if (_playerManager != null && !_playerManager.TrySpendGold(totalCost))
            {
                Debug.LogWarning($"BuyPanel: Player doesn't have {totalCost}g");
                _shopManager.TryGetSession(_shopId, out var session);
                if (session != null)
                {
                    session.DecrementStock(itemId, -amount);
                }
                return;
            }

            if (_inventoryManager != null && !_inventoryManager.AddItem(itemId, amount))
            {
                Debug.LogWarning($"BuyPanel: Inventory full for {itemId}");
                if (_playerManager != null)
                {
                    _playerManager.AddGold(totalCost);
                }
                if (_shopManager.TryGetSession(_shopId, out var session))
                {
                    session.DecrementStock(itemId, -amount);
                }
                return;
            }

            GameEventBus.Publish(new EconomyTransactionCompletedEvent(true, "ShopBuy", itemId, amount, -totalCost, $"Comprou {itemId} x{amount} por {totalCost}g"));
            UpdateGoldDisplay();
            RefreshItemStock();
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
                _itemNameText.text = itemData.DisplayName ?? itemData.Id;
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
