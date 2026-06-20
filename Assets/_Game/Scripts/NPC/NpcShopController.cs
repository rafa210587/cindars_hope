using System.Collections.Generic;
using CindarsHope.Economy;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC.Schedule;
using CindarsHope.Player;
using CindarsHope.Quests.Runtime;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Shop;
using UnityEngine;
using UiDialogueChoice = CindarsHope.UI.Dialogue.DialogueChoice;

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

        private const string ThalindraQuestId = "quest_first_supplies_for_cindar";

        private bool _isInteracting;
        private bool _isClosing;
        private bool _isReady;
        private bool _closingForQuestOffer;

        public string InteractionPrompt =>
            NpcScheduleAvailabilityGate.IsUnavailable(_npcData)
                ? NpcScheduleAvailabilityGate.UnavailablePrompt(_npcData)
                : $"Conversar com {_npcData?.DisplayName ?? "NPC"}";
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
            _closingForQuestOffer = false;
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
            // fable_11 (CA-4): if the vendor is unavailable by schedule (e.g. at home at night, or a
            // night vendor during the day), do not open the shop — show the honest reason instead.
            if (NpcScheduleAvailabilityGate.IsUnavailable(_npcData))
            {
                NpcScheduleAvailabilityGate.PublishUnavailableFeedback(_npcData);
                return;
            }

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
            if (IsThalindra())
                ShowThalindraQuestShopDialogue();
            else if (_npcData != null && _npcData.DialogueTree != null)
                ShowRootShopDialogue();
            else
                ShowShopMenuOrClose();
        }

        private bool IsThalindra()
        {
            if (_npcData == null) return false;
            if (!string.IsNullOrEmpty(_npcData.NpcId) &&
                _npcData.NpcId.Equals("npc_thalindra", System.StringComparison.OrdinalIgnoreCase))
                return true;
            return !string.IsNullOrEmpty(_npcData.DisplayName) &&
                   _npcData.DisplayName.IndexOf("Thalindra", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ShowThalindraQuestShopDialogue()
        {
            if (!_isInteracting || _isClosing || _dialogueModal == null) return;

            DetachDialogueChoiceHandler();
            _dialogueModal.OnChoiceSelected += HandleThalindraChoice;
            _dialogueModal.OnClose += HandleThalindraDialogueClosed;

            // Determine quest label based on current state
            var service = QuestRuntimeBootstrap.QuestService;
            string questLabel;
            if (service != null && service.CanTurnIn(ThalindraQuestId))
                questLabel = "Entregar suprimentos";
            else if (service == null || service.GetQuestState(ThalindraQuestId) == null)
                questLabel = "! Qual é a tarefa?";
            else
                questLabel = null; // quest already accepted and not ready to turn in

            var choices = new List<UiDialogueChoice>();
            if (questLabel != null)
                choices.Add(new UiDialogueChoice(questLabel, "quest"));
            choices.Add(new UiDialogueChoice("Comprar", "buy"));
            choices.Add(new UiDialogueChoice("Vender", "sell"));
            choices.Add(new UiDialogueChoice("Adeus", "exit"));

            _dialogueModal.ShowWithChoices("Como posso ajudar?", choices);
        }

        private void HandleThalindraChoice(UiDialogueChoice choice)
        {
            DetachDialogueChoiceHandler();
            _dialogueModal.OnClose -= HandleThalindraDialogueClosed;
            _dialogueModal.Hide();

            if (choice == null) return;

            switch (choice.ChoiceId)
            {
                case "quest":
                    var service = QuestRuntimeBootstrap.QuestService;
                    var mode = QuestGiverInteractionMode.Offer;
                    if (service != null && service.CanTurnIn(ThalindraQuestId))
                        mode = QuestGiverInteractionMode.TurnIn;
                    else if (service != null && service.GetQuestState(ThalindraQuestId) != null)
                        mode = QuestGiverInteractionMode.NoQuest;

                    _closingForQuestOffer = true;
                    CloseInteraction();
                    GameEventBus.Publish(new QuestGiverInteractedEvent(_npcData.NpcId, ThalindraQuestId, mode));
                    break;

                case "buy":
                    if (EnsureTransactionUiReady(ShopMenuOption.Buy))
                    {
                        _buyPanel.OnBackPressed -= HandlePanelBack;
                        _buyPanel.OnBackPressed += HandlePanelBack;
                        _buyPanel.Show(_shopData.Id);
                    }
                    else BeginCloseInteraction();
                    break;

                case "sell":
                    if (EnsureTransactionUiReady(ShopMenuOption.Sell))
                    {
                        _sellPanel.OnBackPressed -= HandlePanelBack;
                        _sellPanel.OnBackPressed += HandlePanelBack;
                        _sellPanel.Show(_shopData.Id);
                    }
                    else BeginCloseInteraction();
                    break;

                case "exit":
                default:
                    BeginCloseInteraction();
                    break;
            }
        }

        private void HandleThalindraDialogueClosed()
        {
            DetachDialogueChoiceHandler();
            _dialogueModal.OnClose -= HandleThalindraDialogueClosed;
            BeginCloseInteraction();
        }

        private void DetachDialogueChoiceHandler()
        {
            if (_dialogueModal == null) return;
            _dialogueModal.OnChoiceSelected -= HandleThalindraChoice;
            _dialogueModal.OnClose -= HandleThalindraDialogueClosed;
            _dialogueModal.OnChoiceSelected -= HandleRootShopChoice;
            _dialogueModal.OnChoiceSelected -= HandleTreeChoice;
            _dialogueModal.OnClose -= HandleTreeDialogueClosed;
        }

        // ─── Dialogue tree support for shop NPCs ─────────────────────────────
        // Shop NPCs now expose their authored DialogueTreeSO ("Conversar") in addition
        // to the buy/sell flow, so the expanded WAVE25 dialogue content is reachable.

        private readonly Dictionary<string, DialogueChoice> _treeChoiceMap = new Dictionary<string, DialogueChoice>();

        private void ShowRootShopDialogue()
        {
            if (!_isInteracting || _isClosing || _dialogueModal == null) return;

            DetachDialogueChoiceHandler();
            _dialogueModal.OnChoiceSelected += HandleRootShopChoice;
            _dialogueModal.OnClose += HandleTreeDialogueClosed;

            var choices = new List<UiDialogueChoice>
            {
                new UiDialogueChoice("Conversar", "talk"),
                new UiDialogueChoice("Comprar", "buy"),
                new UiDialogueChoice("Vender", "sell")
            };

            // fable_22 (CA-3): opção "Temperar" só no Brumdar e só com o gate aberto
            // (sq_brumdar_3_done + Ato 1). Fail-closed: sem resolver de gate, a opção não aparece.
            if (IsBrumdar() && CindarsHope.Economy.TemperingForgeAccess.IsGateOpen())
            {
                choices.Add(new UiDialogueChoice("Temperar", "temper"));
            }

            // fable_19 (CA-2): opção de serviço civic só nos provedores canônicos (Tovin = licença de
            // barraca; Mara = registro de fazenda). Mostra o rótulo de compra se ainda não possui, ou
            // um rótulo de já-possui (idempotente). Mesmo idioma do gate do Brumdar acima.
            if (TryGetCityServiceChoice(out var serviceChoice))
            {
                choices.Add(serviceChoice);
            }

            choices.Add(new UiDialogueChoice("Adeus", "exit"));

            _dialogueModal.ShowWithChoices("Como posso ajudar?", choices);
        }

        private bool IsBrumdar()
        {
            if (_npcData == null) return false;
            if (!string.IsNullOrEmpty(_npcData.NpcId) &&
                _npcData.NpcId.Equals("npc_brumdar", System.StringComparison.OrdinalIgnoreCase))
                return true;
            return !string.IsNullOrEmpty(_npcData.DisplayName) &&
                   _npcData.DisplayName.IndexOf("Brumdar", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ─── fable_19: opção de serviço civic (Tovin/Mara) ───────────────────────────────────────

        /// <summary>
        /// Monta a opção de diálogo de serviço civic se este NPC for um provedor canônico (Tovin =
        /// licença de barraca; Mara = registro de fazenda). Se já possui, mostra um rótulo informativo
        /// (a compra é idempotente). ChoiceId = "service".
        /// </summary>
        private bool TryGetCityServiceChoice(out UiDialogueChoice choice)
        {
            choice = null;
            var npcId = _npcData != null ? _npcData.NpcId : null;
            if (!CindarsHope.City.Services.CityServiceCatalog.IsServiceProvider(npcId))
            {
                return false;
            }

            var serviceId = CindarsHope.City.Services.CityServiceCatalog.ServiceIdFor(npcId);
            if (string.IsNullOrEmpty(serviceId))
            {
                return false;
            }

            string label = CindarsHope.City.Services.CityServiceAccess.OwnsService(serviceId)
                ? "Servico (ja contratado)"
                : CindarsHope.City.Services.CityServiceCatalog.DisplayLabelFor(serviceId);

            if (string.IsNullOrEmpty(label))
            {
                return false;
            }

            choice = new UiDialogueChoice(label, "service");
            return true;
        }

        /// <summary>
        /// Executa a compra do serviço civic deste NPC pela fachada CityServiceAccess (débito + flag,
        /// idempotente) e publica o feedback por toast.
        /// </summary>
        private void PurchaseCityServiceForThisNpc()
        {
            var npcId = _npcData != null ? _npcData.NpcId : null;
            var serviceId = CindarsHope.City.Services.CityServiceCatalog.ServiceIdFor(npcId);
            if (string.IsNullOrEmpty(serviceId))
            {
                return;
            }

            var result = CindarsHope.City.Services.CityServiceAccess.TryPurchase(serviceId);
            var message = result != null && !string.IsNullOrEmpty(result.Message)
                ? result.Message
                : "Servico indisponivel.";
            GameEventBus.Publish(new PlayerActionFeedbackEvent(message));
        }

        private void HandleRootShopChoice(UiDialogueChoice choice)
        {
            DetachDialogueChoiceHandler();

            if (choice == null)
            {
                BeginCloseInteraction();
                return;
            }

            switch (choice.ChoiceId)
            {
                case "talk":
                    var tree = _npcData != null ? _npcData.DialogueTree : null;
                    var startNode = tree != null ? tree.GetNodeById(tree.StartNodeId) : null;
                    if (startNode != null)
                    {
                        ShowTreeNode(startNode);
                    }
                    else
                    {
                        ShowRootShopDialogue();
                    }
                    break;

                case "buy":
                    _dialogueModal.Hide();
                    if (EnsureTransactionUiReady(ShopMenuOption.Buy))
                    {
                        _buyPanel.OnBackPressed -= HandlePanelBack;
                        _buyPanel.OnBackPressed += HandlePanelBack;
                        _buyPanel.Show(_shopData.Id);
                    }
                    else BeginCloseInteraction();
                    break;

                case "sell":
                    _dialogueModal.Hide();
                    if (EnsureTransactionUiReady(ShopMenuOption.Sell))
                    {
                        _sellPanel.OnBackPressed -= HandlePanelBack;
                        _sellPanel.OnBackPressed += HandlePanelBack;
                        _sellPanel.Show(_shopData.Id);
                    }
                    else BeginCloseInteraction();
                    break;

                case "temper":
                    // fable_22: abre a UI de têmpera se ligada (wiring de cena/Play Mode); senão
                    // sinaliza disponibilidade da forja por toast. Re-checa o gate por segurança.
                    _dialogueModal.Hide();
                    if (CindarsHope.Economy.TemperingForgeAccess.IsGateOpen()
                        && CindarsHope.Economy.TemperingForgeAccess.HasForgeUi)
                    {
                        CindarsHope.Economy.TemperingForgeAccess.OpenForgeUi();
                    }
                    else
                    {
                        GameEventBus.Publish(new PlayerActionFeedbackEvent(
                            "A forja de tempera do Brumdar esta disponivel."));
                    }
                    BeginCloseInteraction();
                    break;

                case "service":
                    // fable_19 (CA-2): compra do serviço civic via diálogo. Ponto único: a fachada
                    // CityServiceAccess decide débito + concessão de flag (idempotente). Feedback por
                    // toast; fecha a interação. Persistência por flag (sem nova seção de save).
                    _dialogueModal.Hide();
                    PurchaseCityServiceForThisNpc();
                    BeginCloseInteraction();
                    break;

                case "exit":
                default:
                    _dialogueModal.Hide();
                    BeginCloseInteraction();
                    break;
            }
        }

        private void ShowTreeNode(DialogueNode node)
        {
            if (node == null || _dialogueModal == null)
            {
                ShowRootShopDialogue();
                return;
            }

            string text = node.Text;
            if (node.RandomLinePool != null && node.RandomLinePool.Count > 0)
            {
                text = node.RandomLinePool[Random.Range(0, node.RandomLinePool.Count)];
            }

            DetachDialogueChoiceHandler();
            _dialogueModal.OnClose += HandleTreeDialogueClosed;

            if (node.Choices != null && node.Choices.Count > 0)
            {
                _treeChoiceMap.Clear();
                var uiChoices = new List<UiDialogueChoice>();
                for (var i = 0; i < node.Choices.Count; i++)
                {
                    var source = node.Choices[i];
                    if (source == null) continue;
                    var choiceId = $"{node.NodeId}_{i}";
                    _treeChoiceMap[choiceId] = source;
                    uiChoices.Add(new UiDialogueChoice(source.Label, choiceId));
                }

                _dialogueModal.OnChoiceSelected += HandleTreeChoice;
                _dialogueModal.ShowWithChoices(text, uiChoices);
            }
            else
            {
                _dialogueModal.Show(text);
            }
        }

        private void HandleTreeChoice(UiDialogueChoice choice)
        {
            DetachDialogueChoiceHandler();

            if (choice == null || !_treeChoiceMap.TryGetValue(choice.ChoiceId, out var npcChoice))
            {
                BeginCloseInteraction();
                return;
            }

            switch (npcChoice.ActionType)
            {
                case DialogueActionType.OpenShop:
                    _dialogueModal.Hide();
                    if (EnsureTransactionUiReady(ShopMenuOption.Buy))
                    {
                        _buyPanel.OnBackPressed -= HandlePanelBack;
                        _buyPanel.OnBackPressed += HandlePanelBack;
                        _buyPanel.Show(_shopData.Id);
                    }
                    else BeginCloseInteraction();
                    return;

                case DialogueActionType.CloseDialogue:
                    _dialogueModal.Hide();
                    BeginCloseInteraction();
                    return;
            }

            if (string.IsNullOrWhiteSpace(npcChoice.NextNodeId))
            {
                _dialogueModal.Hide();
                BeginCloseInteraction();
                return;
            }

            var tree = _npcData != null ? _npcData.DialogueTree : null;
            var nextNode = tree != null ? tree.GetNodeById(npcChoice.NextNodeId) : null;
            if (nextNode == null)
            {
                Debug.LogWarning($"{nameof(NpcShopController)} could not resolve dialogue node '{npcChoice.NextNodeId}' for '{_npcData?.NpcId}'.", this);
                _dialogueModal.Hide();
                BeginCloseInteraction();
                return;
            }

            ShowTreeNode(nextNode);
        }

        private void HandleTreeDialogueClosed()
        {
            DetachDialogueChoiceHandler();
            BeginCloseInteraction();
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
            if (IsThalindra())
                ShowThalindraQuestShopDialogue();
            else
                ShowShopMenu();
        }

        private void BeginCloseInteraction()
        {
            if (!_isInteracting || _isClosing)
            {
                return;
            }

            _isClosing = true;
            switch (_modalManager != null ? _modalManager.CurrentModal : ModalType.None)
            {
                case ModalType.Buy:
                    _buyPanel?.Hide();
                    _sellPanel?.HideVisualOnly();
                    _shopMenuModal?.HideVisualOnly();
                    break;
                case ModalType.Sell:
                    _sellPanel?.Hide();
                    _buyPanel?.HideVisualOnly();
                    _shopMenuModal?.HideVisualOnly();
                    break;
                case ModalType.ShopMenu:
                    _shopMenuModal?.Hide();
                    _buyPanel?.HideVisualOnly();
                    _sellPanel?.HideVisualOnly();
                    break;
                default:
                    _buyPanel?.HideVisualOnly();
                    _sellPanel?.HideVisualOnly();
                    _shopMenuModal?.HideVisualOnly();
                    break;
            }

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
            // Skip ClearAllModals when handing off to QuestOfferPanel — it manages its own modal state.
            if (!_closingForQuestOffer)
                _modalManager?.ClearAllModals();
            _isInteracting = false;
            _isClosing = false;
            _closingForQuestOffer = false;
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
                DetachDialogueChoiceHandler();
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
