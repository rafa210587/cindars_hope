using System;
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    [DisallowMultipleComponent]
    public sealed class SellPanelItem : MonoBehaviour
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

        public void Initialize(ItemDataSO itemData, int playerAmount, ShopManager shopManager, string shopId, Action<string, int> onSellClicked)
        {
            _itemData = itemData;
            _playerAmount = playerAmount;
            _shopManager = shopManager;
            _shopId = shopId;

            if (_itemNameText != null)
            {
                var name = itemData.DisplayName ?? itemData.Id;
                _itemNameText.text = string.IsNullOrWhiteSpace(itemData.Description)
                    ? name
                    : $"{name}\n{itemData.Description}";
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

        private void UpdatePriceDisplay()
        {
            if (_priceText != null && _itemData != null)
            {
                _shopManager.TryGetSession(_shopId, out var session);
                var sellPrice = ShopManager.CalculateSellPrice(_itemData, session?.ShopData);
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
