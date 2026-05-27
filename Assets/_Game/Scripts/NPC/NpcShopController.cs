using CindarsHope.Economy;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
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
        private bool _isReady;

        public string InteractionPrompt => $"Conversar com {_npcData?.DisplayName ?? "NPC"}";
        public NpcDataSO NpcData => _npcData;
        public bool HasMet { get; private set; }

        private void OnEnable()
        {
            TryEnsureShopInitialized("OnEnable");
        }

        private void Start()
        {
            TryEnsureShopInitialized("Start");
        }

        private bool TryEnsureShopInitialized(string reason)
        {
            _isReady = false;
            AdoptPersistentBootstrapReferences(reason);

            if (_shopData == null)
            {
                LogInitializationError(reason, "_shopData", "ShopDataSO is null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_shopData.Id))
            {
                LogInitializationError(reason, "_shopData.Id", "ShopDataSO.Id is empty.");
                return false;
            }

            if (_shopData.Items == null || _shopData.Items.Length == 0)
            {
                LogInitializationError(reason, "_shopData.Items", "ShopDataSO.Items is empty.");
                return false;
            }

            if (!ValidateReference(_npcData, nameof(_npcData), reason)
                || !ValidateReference(_shopManager, nameof(_shopManager), reason)
                || !ValidateReference(_playerManager, nameof(_playerManager), reason)
                || !ValidateReference(_inventoryManager, nameof(_inventoryManager), reason)
                || !ValidateReference(_itemDatabase, nameof(_itemDatabase), reason)
                || !ValidateReference(_modalManager, nameof(_modalManager), reason)
                || !ValidateReference(_dialogueModal, nameof(_dialogueModal), reason)
                || !ValidateReference(_shopMenuModal, nameof(_shopMenuModal), reason)
                || !ValidateReference(_buyPanel, nameof(_buyPanel), reason)
                || !ValidateReference(_sellPanel, nameof(_sellPanel), reason))
            {
                return false;
            }

            _modalManager.Initialize();
            _shopManager.Configure(_itemDatabase);
            if (!_shopManager.IsInitialized)
            {
                LogInitializationError(reason, "_shopManager", "ShopManager exists but IsInitialized is false after Configure.");
                return false;
            }

            if (!_shopManager.InitializeShop(_shopData) || !_shopManager.TryGetSession(_shopData.Id, out _))
            {
                LogInitializationError(reason, "_shopManager", $"ShopManager could not create session. {_shopManager.GetDiagnosticSummary()}");
                return false;
            }

            _dialogueModal.Initialize(_modalManager);
            _shopMenuModal.Initialize(_modalManager);
            _buyPanel.Initialize(_shopManager, _playerManager, _inventoryManager, _itemDatabase, _modalManager);
            _sellPanel.Initialize(_shopManager, _playerManager, _inventoryManager, _itemDatabase, _modalManager);

            _isReady = true;
            Debug.Log($"{GetDiagnosticContext()} shopId '{_shopData.Id}' ready via '{reason}'. {_shopManager.GetDiagnosticSummary()}", this);
            return true;
        }

        private bool ValidateReference(Object reference, string fieldName, string reason)
        {
            if (reference != null)
            {
                return true;
            }

            LogInitializationError(reason, fieldName, "required reference is null.");
            return false;
        }

        private void AdoptPersistentBootstrapReferences(string reason)
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return;
            }

            RebindIfAvailable(ref _shopManager, bootstrap.ShopManager, nameof(_shopManager), reason);
            RebindIfAvailable(ref _playerManager, bootstrap.PlayerManager, nameof(_playerManager), reason);
            RebindIfAvailable(ref _inventoryManager, bootstrap.InventoryManager, nameof(_inventoryManager), reason);
            RebindIfAvailable(ref _itemDatabase, bootstrap.ItemDatabase, nameof(_itemDatabase), reason);
            RebindIfAvailable(ref _modalManager, bootstrap.ModalManager, nameof(_modalManager), reason);
        }

        private void RebindIfAvailable<T>(ref T field, T stableReference, string fieldName, string reason) where T : Object
        {
            if (stableReference == null || field == stableReference)
            {
                return;
            }

            field = stableReference;
            Debug.Log($"{GetDiagnosticContext()} shopId '{GetShopId()}' rebound field '{fieldName}' from persistent GameBootstrap during '{reason}'.", this);
        }

        private void LogInitializationError(string reason, string fieldName, string cause)
        {
            Debug.LogError($"{GetDiagnosticContext()} shopId '{GetShopId()}' initialization failed during '{reason}': field '{fieldName}' - {cause}", this);
        }

        private string GetShopId()
        {
            return _shopData != null && !string.IsNullOrWhiteSpace(_shopData.Id) ? _shopData.Id : "<null>";
        }

        private string GetDiagnosticContext()
        {
            return $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(NpcShopController)}'";
        }

        private void OnDisable()
        {
            DetachUiEvents();
            _isInteracting = false;
            _isClosing = false;
            _isReady = false;
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
            if ((!_isReady || RequiresPersistentBootstrapRebind()) && !TryEnsureShopInitialized("CanInteract"))
            {
                return false;
            }

            return _npcData != null
                && _shopData != null
                && _shopManager != null
                && _itemDatabase != null
                && _modalManager != null
                && _buyPanel != null
                && _sellPanel != null
                && _shopMenuModal != null
                && _isReady
                && !_isInteracting;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor) || !TryEnsureShopInitialized("Interact"))
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
            if (!_isInteracting || _isClosing || _shopMenuModal == null || !TryEnsureShopInitialized("ShowShopMenu"))
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
                    if (EnsureTransactionUiReady(ShopMenuOption.Buy))
                    {
                        _buyPanel.Show(_shopData.Id);
                    }
                    else
                    {
                        BeginCloseInteraction();
                    }
                    break;
                case ShopMenuOption.Sell:
                    if (EnsureTransactionUiReady(ShopMenuOption.Sell))
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

        private bool EnsureTransactionUiReady(ShopMenuOption option)
        {
            if (_shopData == null)
            {
                LogTransactionError(option, "_shopData", "ShopDataSO is null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_shopData.Id))
            {
                LogTransactionError(option, "_shopData.Id", "ShopDataSO.Id is empty.");
                return false;
            }

            if (_shopManager == null)
            {
                LogTransactionError(option, "_shopManager", "ShopManager reference is null.");
                return false;
            }

            if (_itemDatabase == null)
            {
                LogTransactionError(option, "_itemDatabase", "ItemDatabaseSO reference is null.");
                return false;
            }

            if (!_isReady && !TryEnsureShopInitialized($"Before{option}"))
            {
                LogTransactionError(option, "_isReady", "controller is not ready after initialization attempt.");
                return false;
            }

            if (!_shopManager.IsInitialized)
            {
                LogTransactionError(option, "_shopManager.IsInitialized", "ShopManager exists but is not initialized.");
                return false;
            }

            if (!_shopManager.TryGetSession(_shopData.Id, out _)
                && (!TryEnsureShopInitialized($"MissingSessionBefore{option}") || !_shopManager.TryGetSession(_shopData.Id, out _)))
            {
                LogTransactionError(option, "_shopManager", $"ShopManager exists but has no session for this shopId. {_shopManager.GetDiagnosticSummary()}");
                return false;
            }

            var panelReady = option == ShopMenuOption.Buy
                ? _buyPanel != null && _buyPanel.IsInitializedWith(_shopManager, _playerManager, _inventoryManager, _itemDatabase, _modalManager)
                : _sellPanel != null && _sellPanel.IsInitializedWith(_shopManager, _playerManager, _inventoryManager, _itemDatabase, _modalManager);
            if (panelReady)
            {
                return true;
            }

            var fieldName = option == ShopMenuOption.Buy ? "_buyPanel" : "_sellPanel";
            LogTransactionError(option, fieldName, "panel is not initialized with this NPC shop context.");
            return false;
        }

        private bool RequiresPersistentBootstrapRebind()
        {
            var bootstrap = GameBootstrap.Instance;
            return bootstrap != null
                && ((bootstrap.ShopManager != null && _shopManager != bootstrap.ShopManager)
                    || (bootstrap.PlayerManager != null && _playerManager != bootstrap.PlayerManager)
                    || (bootstrap.InventoryManager != null && _inventoryManager != bootstrap.InventoryManager)
                    || (bootstrap.ItemDatabase != null && _itemDatabase != bootstrap.ItemDatabase)
                    || (bootstrap.ModalManager != null && _modalManager != bootstrap.ModalManager));
        }

        private void LogTransactionError(ShopMenuOption option, string fieldName, string cause)
        {
            Debug.LogError($"{GetDiagnosticContext()} shopId '{GetShopId()}' cannot open '{option}': field '{fieldName}' - {cause}", this);
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
