using System;
using CindarsHope.Core.Data;
using CindarsHope.Inventory.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    [DisallowMultipleComponent]
    public sealed class BuyPanelItem : MonoBehaviour
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

                    onBuyClicked.Invoke(itemData.Id, amount);
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
