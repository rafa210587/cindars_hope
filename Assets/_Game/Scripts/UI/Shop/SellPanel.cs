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
    public class SellPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private SellPanelItem _itemPrefab;
        [SerializeField] private Text _goldDisplay;
        [SerializeField] private Button _backButton;

        private ShopManager _shopManager;
        private PlayerManager _playerManager;
        private InventoryManager _inventoryManager;
        private ItemDatabaseSO _itemDatabase;
        private Modal.ModalManager _modalManager;
        private string _shopId;
        private List<SellPanelItem> _displayedItems = new List<SellPanelItem>();

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
            if (_shopManager == null)
            {
                Debug.LogWarning("SellPanel: ShopManager not initialized");
                return;
            }

            _shopId = shopId;
            ClearItems();
            PopulateItems();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            gameObject.SetActive(true);
            _modalManager?.PushModal(Modal.ModalType.Sell);
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
            _modalManager?.TryPopModal(Modal.ModalType.Sell, out _);
            ClearItems();
        }

        private void PopulateItems()
        {
            if (_itemPrefab == null || _itemsContainer == null || _inventoryManager == null)
            {
                return;
            }

            var items = _inventoryManager.Items;
            foreach (var kvp in items)
            {
                var itemId = kvp.Key;
                var amount = kvp.Value;

                if (amount <= 0 || !SellableItemPolicy.IsSellable(itemId))
                {
                    continue;
                }

                if (!_itemDatabase.TryGetById(itemId, out var itemData) || itemData == null)
                {
                    continue;
                }

                var item = Instantiate(_itemPrefab, _itemsContainer);
                item.Initialize(itemData, amount, OnItemSellClicked);
                _displayedItems.Add(item);
            }
        }

        private void OnItemSellClicked(string itemId, int amount)
        {
            if (!_shopManager.TrySellItem(_shopId, itemId, amount, out var totalGold))
            {
                Debug.LogWarning($"SellPanel: Failed to sell {itemId} x{amount}");
                return;
            }

            if (_inventoryManager != null && !_inventoryManager.RemoveItem(itemId, amount))
            {
                Debug.LogWarning($"SellPanel: Failed to remove {itemId} x{amount} from inventory");
                return;
            }

            if (_playerManager != null)
            {
                _playerManager.AddGold(totalGold);
            }

            GameEventBus.Publish(new EconomyTransactionCompletedEvent(true, "ShopSell", itemId, amount, totalGold, $"Vendeu {itemId} x{amount} por {totalGold}g"));
            UpdateGoldDisplay();
            RefreshPanel();
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
    public class SellPanelItem : MonoBehaviour
    {
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _priceText;
        [SerializeField] private Text _amountText;
        [SerializeField] private Button _sellButton;
        [SerializeField] private InputField _amountInput;

        private ItemDataSO _itemData;
        private int _playerAmount;

        public void Initialize(ItemDataSO itemData, int playerAmount, Action<string, int> onSellClicked)
        {
            _itemData = itemData;
            _playerAmount = playerAmount;

            if (_itemNameText != null)
            {
                _itemNameText.text = itemData.DisplayName ?? itemData.Id;
            }

            UpdatePriceDisplay();
            UpdateAmountDisplay();

            if (_sellButton != null && onSellClicked != null)
            {
                _sellButton.onClick.AddListener(() =>
                {
                    var amount = 1;
                    if (_amountInput != null && int.TryParse(_amountInput.text, out var parsed))
                    {
                        amount = Mathf.Min(Mathf.Max(1, parsed), _playerAmount);
                    }

                    onSellClicked?.Invoke(itemData.Id, amount);
                });
            }
        }

        private void UpdatePriceDisplay()
        {
            if (_priceText != null && _itemData != null)
            {
                var sellPrice = Mathf.RoundToInt(_itemData.BaseValue * 0.6f);
                _priceText.text = $"{sellPrice}g";
            }
        }

        private void UpdateAmountDisplay()
        {
            if (_amountText != null)
            {
                _amountText.text = $"Tem: {_playerAmount}";
            }
        }
    }
}
