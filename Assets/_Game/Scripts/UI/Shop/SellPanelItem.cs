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
        private int _eligibleAmount;
        private int _retainedAmount;
        private Func<int, ShopManager.ShopSellQuote> _quoteForAmount;
        private Action<ItemDataSO, ShopManager.ShopSellQuote, int, int> _onFocused;

        public void Initialize(ItemDataSO itemData, int eligibleAmount, int retainedAmount,
            Func<int, ShopManager.ShopSellQuote> quoteForAmount,
            Action<string, int> onSellClicked,
            Action<ItemDataSO, ShopManager.ShopSellQuote, int, int> onFocused)
        {
            _itemData = itemData;
            _eligibleAmount = eligibleAmount;
            _retainedAmount = retainedAmount;
            _quoteForAmount = quoteForAmount;
            _onFocused = onFocused;

            if (_itemNameText != null)
            {
                _itemNameText.text = ItemDisplayNameFormatter.GetShortName(itemData);
            }

            UpdatePriceDisplay();
            UpdateAmountDisplay();
            if (_amountInput != null)
                _amountInput.onValueChanged.AddListener(_ => { UpdatePriceDisplay(); Focus(); });

            if (_sellButton != null && onSellClicked != null)
            {
                _sellButton.interactable = _eligibleAmount > 0;
                _sellButton.onClick.AddListener(() =>
                {
                    if (_eligibleAmount > 0)
                        onSellClicked.Invoke(itemData.Id, SelectedAmount());
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
            _onFocused?.Invoke(_itemData, _quoteForAmount != null
                ? _quoteForAmount(SelectedAmount()) : default,
                _eligibleAmount, _retainedAmount);
        }

        private void UpdatePriceDisplay()
        {
            if (_priceText != null)
            {
                var quote = _quoteForAmount != null
                    ? _quoteForAmount(SelectedAmount()) : default;
                _priceText.text = quote.Success
                    ? $"{quote.FinalGold}g / {quote.SellAmount}"
                    : "Sem venda";
            }
        }

        private void UpdateAmountDisplay()
        {
            if (_amountText != null)
            {
                _amountText.text = $"Vendavel: {_eligibleAmount} | Retido: {_retainedAmount}";
            }
        }

        private int SelectedAmount()
        {
            if (_eligibleAmount <= 0) return 0;
            if (_amountInput != null && int.TryParse(_amountInput.text, out var parsed))
                return Mathf.Min(Mathf.Max(1, parsed), _eligibleAmount);
            return 1;
        }
    }
}
