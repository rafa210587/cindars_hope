using System;
using CindarsHope.Core;
using CindarsHope.Foundation;
using UnityEngine;
using UnityEngine.UI;
// arch: quebra do par mutuo NPC|UI (2026-07-15) — ShopMenuOption movido para CindarsHope.NPC (não
// Foundation, para não crescer a fronteira ratchet-tracked); ShopMenuModal agora implementa
// CindarsHope.NPC.INpcShopMenuPresenter para que NpcShopController consuma via porta, sem nomear este
// tipo concreto. A aresta UI->NPC deste using já existia (ex.: NpcInteractionPortraitHud.cs).
using ShopMenuOption = CindarsHope.NPC.ShopMenuOption;

namespace CindarsHope.UI.Shop
{
    [DisallowMultipleComponent]
    public class ShopMenuModal : MonoBehaviour, CindarsHope.NPC.INpcShopMenuPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _exitButton;

        private IModalRuntime _modalManager;
        private ShopMenuOption _selectedOption = ShopMenuOption.Buy;
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
            if (_canvasGroup == null || !_canvasGroup.interactable)
            {
                return;
            }

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.W) || global::UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSelection(-1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.S) || global::UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSelection(1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.E) || global::UnityEngine.Input.GetKeyDown(KeyCode.Return) || global::UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                SelectOption(_selectedOption);
            }
        }

        public void Initialize(IModalRuntime modalManager)
        {
            _modalManager = modalManager;
            HideVisualOnly();
        }

        public void Show()
        {
            if (_modalManager != null && !_modalManager.PushModal(CindarsHope.Foundation.ModalType.ShopMenu))
            {
                Debug.LogWarning("ShopMenuModal rejected because another interactive modal is active.", this);
                return;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            gameObject.SetActive(true);
            _selectedOption = ShopMenuOption.Buy;
            UpdateHighlight();
        }

        public void Hide()
        {
            HideVisualOnly();
            _modalManager?.TryPopIfCurrent(CindarsHope.Foundation.ModalType.ShopMenu);
        }

        public void HideVisualOnly()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
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

        private void MoveSelection(int direction)
        {
            var options = new[] { ShopMenuOption.Buy, ShopMenuOption.Sell, ShopMenuOption.Exit };
            var currentIndex = System.Array.IndexOf(options, _selectedOption);
            currentIndex = (currentIndex + direction + options.Length) % options.Length;
            _selectedOption = options[currentIndex];
            UpdateHighlight();
        }

        private void UpdateHighlight()
        {
            SetHighlight(_buyButton, _selectedOption == ShopMenuOption.Buy);
            SetHighlight(_sellButton, _selectedOption == ShopMenuOption.Sell);
            SetHighlight(_exitButton, _selectedOption == ShopMenuOption.Exit);
        }

        private static void SetHighlight(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }

            var colors = button.colors;
            colors.normalColor = selected ? new Color(1f, 0.86f, 0.34f) : Color.white;
            button.colors = colors;
        }
    }
}
