using System;
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    [DisallowMultipleComponent]
    public sealed class SellPanelItem : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _priceText;
        [SerializeField] private Text _amountText;
        [SerializeField] private Button _sellButton;
        [SerializeField] private InputField _amountInput;

        private ItemDataSO _itemData;
        private int _playerAmount;
        private ShopManager _shopManager;
        private string _shopId;
        private Action<ItemDataSO, int, int> _onFocused;

        public void Initialize(ItemDataSO itemData, int playerAmount, ShopManager shopManager, string shopId, Action<string, int> onSellClicked, Action<ItemDataSO, int, int> onFocused)
        {
            _itemData = itemData;
            _playerAmount = playerAmount;
            _shopManager = shopManager;
            _shopId = shopId;
            _onFocused = onFocused;

            if (_itemNameText != null)
            {
                _itemNameText.text = ItemDisplayNameFormatter.GetShortName(itemData);
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

                    onSellClicked.Invoke(itemData.Id, amount);
                });
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Focus();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Focus();
        }

        public void Focus()
        {
            _onFocused?.Invoke(_itemData, CalculateUnitPrice(), _playerAmount);
        }

        private void UpdatePriceDisplay()
        {
            if (_priceText != null)
            {
                _priceText.text = $"{CalculateUnitPrice()}g";
            }
        }

        private void UpdateAmountDisplay()
        {
            if (_amountText != null)
            {
                _amountText.text = $"Tem: {_playerAmount}";
            }
        }

        private int CalculateUnitPrice()
        {
            _shopManager.TryGetSession(_shopId, out var session);
            return ShopManager.CalculateSellPrice(_itemData, session?.ShopData);
        }
    }
}
