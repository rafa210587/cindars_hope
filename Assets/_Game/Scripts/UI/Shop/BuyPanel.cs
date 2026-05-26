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
                Debug.LogError($"{GetDiagnosticContext()} cannot show shop '{shopId}': ShopManager was not initialized.", this);
                return;
            }

            if (!_shopManager.TryGetSession(shopId, out var session))
            {
                Debug.LogError($"{GetDiagnosticContext()} cannot show shop '{shopId}': no initialized ShopSession exists.", this);
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

        private string GetDiagnosticContext()
        {
            return $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(BuyPanel)}'";
        }
    }

}
