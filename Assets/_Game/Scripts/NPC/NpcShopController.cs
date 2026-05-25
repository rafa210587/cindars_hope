using CindarsHope.Economy;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Shop;
using UnityEngine;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public sealed class NpcShopController : MonoBehaviour, IInteractable
    {
        [SerializeField] private NpcDataSO _npcData;
        [SerializeField] private ShopDataSO _shopData;
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private DialogueModal _dialogueModal;
        [SerializeField] private ShopMenuModal _shopMenuModal;
        [SerializeField] private BuyPanel _buyPanel;
        [SerializeField] private SellPanel _sellPanel;
        [SerializeField] private ModalManager _modalManager;

        private bool _isInteracting;
        private bool _isClosing;

        public string InteractionPrompt => $"Conversar com {_npcData?.DisplayName ?? "NPC"}";
        public NpcDataSO NpcData => _npcData;
        public bool HasMet { get; private set; }

        private void Start()
        {
            _modalManager?.Initialize();
            _dialogueModal?.Initialize(_modalManager);
            _shopMenuModal?.Initialize(_modalManager);
            _buyPanel?.Initialize(_shopManager, _playerManager, _inventoryManager, _itemDatabase, _modalManager);
            _sellPanel?.Initialize(_shopManager, _playerManager, _inventoryManager, _itemDatabase, _modalManager);

            if (_shopData != null && _shopManager != null)
            {
                _shopManager.Configure(_itemDatabase);
                _shopManager.InitializeShop(_shopData);
            }
        }

        private void OnDisable()
        {
            DetachUiEvents();
            _isInteracting = false;
            _isClosing = false;
        }

        private void Update()
        {
            if (_isInteracting
                && !_isClosing
                && _modalManager != null
                && _modalManager.CurrentModal != ModalType.Dialogue
                && Input.GetKeyDown(KeyCode.Escape))
            {
                BeginCloseInteraction();
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return _npcData != null && !_isInteracting;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            _isInteracting = true;
            _isClosing = false;
            HasMet = true;
            GameEventBus.Publish(new NpcInteractionStartedEvent(_npcData.NpcId));
            ShowOpeningDialogue();
        }

        private void ShowOpeningDialogue()
        {
            if (_dialogueModal == null)
            {
                ShowShopMenuOrClose();
                return;
            }

            _dialogueModal.OnClose += HandleOpeningClosed;
            _dialogueModal.Show(_npcData.OpeningLine);
        }

        private void HandleOpeningClosed()
        {
            _dialogueModal.OnClose -= HandleOpeningClosed;
            ShowShopMenuOrClose();
        }

        private void ShowShopMenuOrClose()
        {
            if (_shopData == null)
            {
                BeginCloseInteraction();
                return;
            }

            ShowShopMenu();
        }

        private void ShowShopMenu()
        {
            if (!_isInteracting || _isClosing || _shopMenuModal == null)
            {
                return;
            }

            _shopMenuModal.OnOptionSelected -= HandleShopMenuOption;
            _shopMenuModal.OnOptionSelected += HandleShopMenuOption;
            if (_buyPanel != null)
            {
                _buyPanel.OnBackPressed -= HandlePanelBack;
                _buyPanel.OnBackPressed += HandlePanelBack;
            }

            if (_sellPanel != null)
            {
                _sellPanel.OnBackPressed -= HandlePanelBack;
                _sellPanel.OnBackPressed += HandlePanelBack;
            }
            _shopMenuModal.Show();
        }

        private void HandleShopMenuOption(ShopMenuOption option)
        {
            _shopMenuModal.OnOptionSelected -= HandleShopMenuOption;
            _shopMenuModal.Hide();

            switch (option)
            {
                case ShopMenuOption.Buy:
                    if (_buyPanel != null)
                    {
                        _buyPanel.Show(_shopData.Id);
                    }
                    else
                    {
                        BeginCloseInteraction();
                    }
                    break;
                case ShopMenuOption.Sell:
                    if (_sellPanel != null)
                    {
                        _sellPanel.Show(_shopData.Id);
                    }
                    else
                    {
                        BeginCloseInteraction();
                    }
                    break;
                case ShopMenuOption.Exit:
                    BeginCloseInteraction();
                    break;
            }
        }

        private void HandlePanelBack()
        {
            ShowShopMenu();
        }

        private void BeginCloseInteraction()
        {
            if (!_isInteracting || _isClosing)
            {
                return;
            }

            _isClosing = true;
            _buyPanel?.Hide();
            _sellPanel?.Hide();
            _shopMenuModal?.Hide();

            if (_dialogueModal == null || string.IsNullOrWhiteSpace(_npcData.ClosingLine))
            {
                CloseInteraction();
                return;
            }

            _dialogueModal.OnClose += HandleClosingClosed;
            _dialogueModal.Show(_npcData.ClosingLine);
        }

        private void HandleClosingClosed()
        {
            _dialogueModal.OnClose -= HandleClosingClosed;
            CloseInteraction();
        }

        private void CloseInteraction()
        {
            DetachUiEvents();
            _modalManager?.ClearAllModals();
            _isInteracting = false;
            _isClosing = false;
            GameEventBus.Publish(new NpcInteractionEndedEvent(_npcData.NpcId));
        }

        public void RestoreState(bool hasMet)
        {
            HasMet = hasMet;
        }

        private void DetachUiEvents()
        {
            if (_dialogueModal != null)
            {
                _dialogueModal.OnClose -= HandleOpeningClosed;
                _dialogueModal.OnClose -= HandleClosingClosed;
            }

            if (_shopMenuModal != null)
            {
                _shopMenuModal.OnOptionSelected -= HandleShopMenuOption;
            }

            if (_buyPanel != null)
            {
                _buyPanel.OnBackPressed -= HandlePanelBack;
            }

            if (_sellPanel != null)
            {
                _sellPanel.OnBackPressed -= HandlePanelBack;
            }
        }
    }
}
