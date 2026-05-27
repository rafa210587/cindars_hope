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
    public class SellPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private SellPanelItem _itemPrefab;
        [SerializeField] private Text _goldDisplay;
        [SerializeField] private Text _feedbackText;
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

        public void Initialize(ShopManager shopManager, PlayerManager playerManager, InventoryManager inventoryManager, ItemDatabaseSO itemDatabase, Modal.ModalManager modalManager)
        {
            _shopManager = shopManager;
            _playerManager = playerManager;
            _inventoryManager = inventoryManager;
            _itemDatabase = itemDatabase;
            _modalManager = modalManager;
            Hide();
        }

        public bool IsInitializedWith(ShopManager shopManager, PlayerManager playerManager, InventoryManager inventoryManager, ItemDatabaseSO itemDatabase, Modal.ModalManager modalManager)
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
                Debug.LogError(
                    $"{GetDiagnosticContext()} cannot show shopId '{shopId}': no initialized ShopSession exists. IsInitialized={_shopManager.IsInitialized}. {_shopManager.GetDiagnosticSummary()}",
                    this);
                return;
            }

            if (_modalManager != null && !_modalManager.PushModal(Modal.ModalType.Sell))
            {
                Debug.LogWarning("SellPanel rejected because another interactive modal is active.", this);
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
            _modalManager?.TryPopModal(Modal.ModalType.Sell, out _);
            ClearItems();
        }

        private void PopulateItems()
        {
            if (_itemPrefab == null || _itemsContainer == null || _inventoryManager == null)
            {
                Debug.LogWarning("SellPanel: ItemPrefab, ItemsContainer, or InventoryManager not assigned");
                SetFeedback("Erro ao carregar inventário.");
                return;
            }

            var items = _inventoryManager.Items;
            if (items == null || items.Count == 0)
            {
                Debug.Log("SellPanel: Inventory is empty");
                SetFeedback("Nenhum item vendável.");
                return;
            }

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
                    Debug.LogWarning($"SellPanel: Item '{itemId}' not found in database");
                    continue;
                }

                var item = Instantiate(_itemPrefab, _itemsContainer);
                item.gameObject.SetActive(true);
                item.Initialize(itemData, amount, _shopManager, _shopId, OnItemSellClicked);
                _displayedItems.Add(item);
            }

            if (_displayedItems.Count == 0)
            {
                Debug.Log("SellPanel: No sellable items in inventory");
                SetFeedback("Nenhum item vendável no inventário.");
            }
        }

        private void OnItemSellClicked(string itemId, int amount)
        {
            var result = _shopManager.TrySellItem(_shopId, itemId, amount, _playerManager, _inventoryManager);
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

        private string GetDiagnosticContext()
        {
            return $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(SellPanel)}'";
        }
    }

}
