using System;
using CindarsHope.Inventory.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    [DisallowMultipleComponent]
    public sealed class BuyPanelItem : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _priceText;
        [SerializeField] private Text _stockText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private InputField _amountInput;

        private ItemDataSO _itemData;
        private int _currentStock;
        private int _unitPrice;
        private Action<ItemDataSO, int, int> _onFocused;

        public string ItemId => _itemData?.Id;

        public void Initialize(ItemDataSO itemData, int stock, int unitPrice, Action<string, int> onBuyClicked, Action<ItemDataSO, int, int> onFocused)
        {
            _itemData = itemData;
            _currentStock = stock;
            _unitPrice = unitPrice;
            _onFocused = onFocused;

            if (_itemNameText != null)
            {
                _itemNameText.text = ItemDisplayNameFormatter.GetShortName(itemData);
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

                    onBuyClicked.Invoke(itemData.Id, amount);
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
            _onFocused?.Invoke(_itemData, _unitPrice, _currentStock);
        }

        public void SetStock(int stock)
        {
            _currentStock = stock;
            UpdateStockDisplay();
        }

        private void UpdatePriceDisplay()
        {
            if (_priceText != null)
            {
                _priceText.text = $"{_unitPrice}g";
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
