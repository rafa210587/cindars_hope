using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Shop;
using UnityEngine;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public class NpcShopController : MonoBehaviour
    {
        [SerializeField] private NpcDialogueDataSO _dialogueData;
        [SerializeField] private ShopDataSO _shopData;
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private DialogueModal _dialogueModal;
        [SerializeField] private ShopMenuModal _shopMenuModal;
        [SerializeField] private UI.Modal.ModalManager _modalManager;

        private bool _isInteracting = false;

        private void OnEnable()
        {
            GameEventBus.Subscribe<InteractionPromptChangedEvent>(HandleInteractionPrompt);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<InteractionPromptChangedEvent>(HandleInteractionPrompt);
        }

        private void Start()
        {
            if (_shopData != null && _shopManager != null)
            {
                _shopManager.InitializeShop(_shopData);
            }
        }

        private void Update()
        {
            if (_isInteracting && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseInteraction();
            }
        }

        public void Interact()
        {
            if (_isInteracting)
            {
                return;
            }

            _isInteracting = true;
            ShowOpeningDialogue();
        }

        private void ShowOpeningDialogue()
        {
            if (_dialogueModal == null)
            {
                HandleDialogueClosed();
                return;
            }

            var text = _dialogueData != null ? _dialogueData.OpeningLine : "Bem-vindo!";
            _dialogueModal.Show(text);
            _dialogueModal.OnClose += HandleDialogueClosed;
        }

        private void HandleDialogueClosed()
        {
            if (_dialogueModal != null)
            {
                _dialogueModal.OnClose -= HandleDialogueClosed;
            }

            if (_shopData != null)
            {
                ShowShopMenu();
            }
            else
            {
                CloseInteraction();
            }
        }

        private void ShowShopMenu()
        {
            if (_shopMenuModal == null)
            {
                CloseInteraction();
                return;
            }

            _shopMenuModal.Show();
            _shopMenuModal.OnOptionSelected += HandleShopMenuOption;
        }

        private void HandleShopMenuOption(ShopMenuOption option)
        {
            _shopMenuModal.OnOptionSelected -= HandleShopMenuOption;

            switch (option)
            {
                case ShopMenuOption.Buy:
                    Debug.Log("Buy option selected - will implement with BuyPanel");
                    break;
                case ShopMenuOption.Sell:
                    Debug.Log("Sell option selected - will implement with SellPanel");
                    break;
                case ShopMenuOption.Exit:
                    ShowClosingDialogue();
                    break;
            }
        }

        private void ShowClosingDialogue()
        {
            if (_dialogueModal == null)
            {
                CloseInteraction();
                return;
            }

            var text = _dialogueData != null ? _dialogueData.ClosingLine : "Até logo!";
            _dialogueModal.Show(text);
            _dialogueModal.OnClose += () => CloseInteraction();
        }

        private void CloseInteraction()
        {
            _isInteracting = false;
            _modalManager?.ClearAllModals();
            Debug.Log("Interaction closed");
        }

        private void HandleInteractionPrompt(InteractionPromptChangedEvent evt)
        {
            // Will be used for player input to trigger interaction
        }
    }
}
