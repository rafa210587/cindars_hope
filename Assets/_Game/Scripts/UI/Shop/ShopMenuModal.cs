using System;
using CindarsHope.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    public enum ShopMenuOption
    {
        None,
        Buy,
        Sell,
        Exit
    }

    [DisallowMultipleComponent]
    public class ShopMenuModal : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _exitButton;

        private Modal.ModalManager _modalManager;
        private ShopMenuOption _selectedOption = ShopMenuOption.None;
        public event Action<ShopMenuOption> OnOptionSelected;

        private void OnEnable()
        {
            if (_buyButton != null) _buyButton.onClick.AddListener(() => SelectOption(ShopMenuOption.Buy));
            if (_sellButton != null) _sellButton.onClick.AddListener(() => SelectOption(ShopMenuOption.Sell));
            if (_exitButton != null) _exitButton.onClick.AddListener(() => SelectOption(ShopMenuOption.Exit));
        }

        private void OnDisable()
        {
            if (_buyButton != null) _buyButton.onClick.RemoveAllListeners();
            if (_sellButton != null) _sellButton.onClick.RemoveAllListeners();
            if (_exitButton != null) _exitButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            if (_canvasGroup != null && _canvasGroup.interactable && Input.GetKeyDown(KeyCode.Escape))
            {
                SelectOption(ShopMenuOption.Exit);
            }
        }

        public void Initialize(Modal.ModalManager modalManager)
        {
            _modalManager = modalManager;
            Hide();
        }

        public void Show()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            gameObject.SetActive(true);
            _modalManager?.PushModal(Modal.ModalType.ShopMenu);
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
            _modalManager?.TryPopModal(Modal.ModalType.ShopMenu, out _);
        }

        private void SelectOption(ShopMenuOption option)
        {
            _selectedOption = option;
            OnOptionSelected?.Invoke(option);

            if (option == ShopMenuOption.Exit)
            {
                Hide();
            }
        }
    }
}
