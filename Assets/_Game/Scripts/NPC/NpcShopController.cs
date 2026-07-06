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
        private const string DebugExprPrefix = "dbg:";

        private readonly NpcShopInteractionSession _interaction = new NpcShopInteractionSession();
        private bool _isReady;

        public string InteractionPrompt =>
            NpcScheduleAvailabilityGate.IsUnavailable(_npcData)
                ? NpcScheduleAvailabilityGate.UnavailablePrompt(_npcData)
                : $"Conversar com {_npcData?.DisplayName ?? "NPC"}";
        public NpcDataSO NpcData => _npcData;
        public bool HasMet { get; private set; }

        private void OnEnable()
        {
            NpcVisualRegistry.Register(_npcData);
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
            if (_npcData != null) NpcVisualRegistry.Unregister(_npcData.NpcId);
            DetachUiEvents();
            _interaction.Complete();
            _isReady = false;
        }

        private void Update()
        {
            if (_interaction.IsInteracting
                && !_interaction.IsClosing
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
                && !_interaction.IsInteracting;
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

            if (!_interaction.TryBegin()) return;
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
            if (!_interaction.IsInteracting || _interaction.IsClosing || _dialogueModal == null) return;

            DetachDialogueChoiceHandler();
            _dialogueModal.OnChoiceSelected += HandleThalindraChoice;
            _dialogueModal.OnClose += HandleThalindraDialogueClosed;

            var service = QuestRuntimeBootstrap.QuestService;
            var questDecision = ThalindraQuestDialoguePolicy.Resolve(
                service != null && service.GetQuestState(ThalindraQuestId) != null,
                service != null && service.CanTurnIn(ThalindraQuestId));

            var choices = ToUiChoices(NpcShopDialogueChoicePolicy.BuildThalindraChoices(
                questDecision,
                BuildNpcServiceChoices(),
                UnityEngine.Debug.isDebugBuild));
            // fable_25: Análise de Criatura da Thalindra entra no MESMO menu (sem segundo fluxo).

            _dialogueModal.ShowWithChoices("Como posso ajudar?", choices);
        }

        private void HandleThalindraChoice(UiDialogueChoice choice)
        {
            DetachDialogueChoiceHandler();
            _dialogueModal.OnClose -= HandleThalindraDialogueClosed;

            if (choice == null)
            {
                _dialogueModal.Hide();
                return;
            }

            // fable_25: serviços únicos (ChoiceId "svc:<serviceId>") — mesmo handler do Conversar raiz.
            if (choice.ChoiceId != null && choice.ChoiceId.StartsWith(NpcShopDialogueChoicePolicy.NpcServiceChoicePrefix, System.StringComparison.Ordinal))
            {
                HandleNpcServiceChoice(choice.ChoiceId.Substring(NpcShopDialogueChoicePolicy.NpcServiceChoicePrefix.Length));
                return;
            }

            if (choice.ChoiceId == NpcShopDialogueChoicePolicy.DebugExpressionChoiceId)
            {
                ShowDebugExpressionMenu();
                return;
            }

            _dialogueModal.Hide();

            switch (choice.ChoiceId)
            {
                case "quest":
                    var service = QuestRuntimeBootstrap.QuestService;
                    var questDecision = ThalindraQuestDialoguePolicy.Resolve(
                        service != null && service.GetQuestState(ThalindraQuestId) != null,
                        service != null && service.CanTurnIn(ThalindraQuestId));

                    _interaction.MarkQuestOfferHandoff();
                    CloseInteraction();
                    GameEventBus.Publish(new QuestGiverInteractedEvent(
                        _npcData.NpcId, ThalindraQuestId, questDecision.InteractionMode));
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
            _dialogueModal.OnChoiceSelected -= HandleDebugChoice;
        }

        private void ShowDebugExpressionMenu()
        {
            if (!_interaction.IsInteracting || _interaction.IsClosing || _dialogueModal == null) return;
            DetachDialogueChoiceHandler();
            _dialogueModal.OnChoiceSelected += HandleDebugChoice;
            _dialogueModal.OnClose += HandleTreeDialogueClosed;
            var choices = new List<UiDialogueChoice>
            {
                new UiDialogueChoice("Neutro", DebugExprPrefix + nameof(NpcExpression.Neutral)),
                new UiDialogueChoice("Felicidade", DebugExprPrefix + nameof(NpcExpression.Happiness)),
                new UiDialogueChoice("Amor", DebugExprPrefix + nameof(NpcExpression.Love)),
                new UiDialogueChoice("Desdem", DebugExprPrefix + nameof(NpcExpression.Disdain)),
                new UiDialogueChoice("Odio", DebugExprPrefix + nameof(NpcExpression.Hatred)),
                new UiDialogueChoice("Voltar", "dbg_back"),
            };
            _dialogueModal.ShowWithChoices("[Debug] Trocar expressao:", choices);
        }

        private void HandleDebugChoice(UiDialogueChoice choice)
        {
            DetachDialogueChoiceHandler();
            _dialogueModal.OnClose -= HandleTreeDialogueClosed;

            if (choice == null)
            {
                BeginCloseInteraction();
                return;
            }

            if (choice.ChoiceId == "dbg_back")
            {
                if (IsThalindra()) ShowThalindraQuestShopDialogue(); else ShowRootShopDialogue();
                return;
            }

            if (choice.ChoiceId != null && choice.ChoiceId.StartsWith(DebugExprPrefix, System.StringComparison.Ordinal))
            {
                var name = choice.ChoiceId.Substring(DebugExprPrefix.Length);
                if (_npcData != null && System.Enum.TryParse<NpcExpression>(name, out var expr))
                {
                    GameEventBus.Publish(new NpcExpressionOverrideEvent(_npcData.NpcId, expr));
                }
                ShowDebugExpressionMenu();
                return;
            }

            if (IsThalindra()) ShowThalindraQuestShopDialogue(); else ShowRootShopDialogue();
        }

        // ─── Dialogue tree support for shop NPCs ─────────────────────────────
        // Shop NPCs now expose their authored DialogueTreeSO ("Conversar") in addition
        // to the buy/sell flow, so the expanded WAVE25 dialogue content is reachable.

        private readonly Dictionary<string, DialogueChoice> _treeChoiceMap = new Dictionary<string, DialogueChoice>();

        private void ShowRootShopDialogue()
        {
            if (!_interaction.IsInteracting || _interaction.IsClosing || _dialogueModal == null) return;

            DetachDialogueChoiceHandler();
            _dialogueModal.OnChoiceSelected += HandleRootShopChoice;
            _dialogueModal.OnClose += HandleTreeDialogueClosed;

            TryGetCityServiceChoice(out var serviceChoice);
            var choices = ToUiChoices(NpcShopDialogueChoicePolicy.BuildRootChoices(
                IsBrumdar() && CindarsHope.Economy.TemperingForgeAccess.IsGateOpen(),
                serviceChoice != null ? serviceChoice.Label : null,
                BuildNpcServiceChoices(),
                UnityEngine.Debug.isDebugBuild));

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

        // ─── fable_25: serviços únicos do NPC no Conversar ───────────────────────────────────────

        /// <summary>
        /// Adiciona uma opção por serviço único deste NPC. Habilitada ⇒ rótulo simples; gated ⇒ rótulo +
        /// motivo (descoberta > ocultação). ChoiceId = "svc:&lt;serviceId&gt;". A opção NUNCA some.
        /// </summary>
        private List<NpcShopServiceChoiceDefinition> BuildNpcServiceChoices()
        {
            var choices = new List<NpcShopServiceChoiceDefinition>();
            var npcId = _npcData != null ? _npcData.NpcId : null;
            if (!CindarsHope.NPC.Services.NpcServiceAccess.HasServices(npcId))
                return choices;

            var options = CindarsHope.NPC.Services.NpcServiceAccess.BuildOptions(npcId);
            foreach (var option in options)
            {
                if (string.IsNullOrEmpty(option.ServiceId)) continue;
                choices.Add(new NpcShopServiceChoiceDefinition(option.Label, option.ServiceId));
            }

            return choices;
        }

        private static List<UiDialogueChoice> ToUiChoices(IReadOnlyList<NpcShopChoiceDefinition> definitions)
        {
            var choices = new List<UiDialogueChoice>(definitions.Count);
            for (var i = 0; i < definitions.Count; i++)
                choices.Add(new UiDialogueChoice(definitions[i].Label, definitions[i].ChoiceId));
            return choices;
        }

        /// <summary>
        /// Executa (ou recusa com motivo) um serviço único selecionado no Conversar. Opção gated ⇒
        /// toast com o requisito (não cobra, não executa). Habilitada ⇒ NpcServiceAccess.Execute valida
        /// gate/limite/custo e despacha o efeito; feedback por toast.
        /// </summary>
        private void HandleNpcServiceChoice(string serviceId)
        {
            _dialogueModal.Hide();

            var npcId = _npcData != null ? _npcData.NpcId : null;
            // Revalida o estado da opção para um motivo honesto quando gated (descoberta > ocultação).
            var options = CindarsHope.NPC.Services.NpcServiceAccess.BuildOptions(npcId);
            foreach (var option in options)
            {
                if (option.ServiceId != serviceId) continue;
                if (!option.Enabled)
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        string.IsNullOrEmpty(option.DisabledReason) ? "Servico indisponivel." : option.DisabledReason));
                    BeginCloseInteraction();
                    return;
                }

                break;
            }

            var result = CindarsHope.NPC.Services.NpcServiceAccess.Execute(serviceId);
            var message = !string.IsNullOrEmpty(result.Message) ? result.Message : "Servico concluido.";
            GameEventBus.Publish(new PlayerActionFeedbackEvent(message));
            BeginCloseInteraction();
        }

        private void HandleRootShopChoice(UiDialogueChoice choice)
        {
            DetachDialogueChoiceHandler();

            if (choice == null)
            {
                BeginCloseInteraction();
                return;
            }

            // fable_25: serviços únicos (ChoiceId "svc:<serviceId>").
            if (choice.ChoiceId != null && choice.ChoiceId.StartsWith(NpcShopDialogueChoicePolicy.NpcServiceChoicePrefix, System.StringComparison.Ordinal))
            {
                HandleNpcServiceChoice(choice.ChoiceId.Substring(NpcShopDialogueChoicePolicy.NpcServiceChoicePrefix.Length));
                return;
            }

            if (choice.ChoiceId == NpcShopDialogueChoicePolicy.DebugExpressionChoiceId)
            {
                ShowDebugExpressionMenu();
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

                case "gift":
                    // fable_72: ponto de entrada do ato de dar presente. O seletor de item Giftable do
                    // inventário é UI diferida (sem seletor reusável no projeto; wiring/Play Mode
                    // humano). Quando houver seletor, a seleção chama
                    // GiftGivingService.Instance.TryGiveGift(npcId, itemId) (classifica gosto, aplica
                    // delta via F26, consome 1 unidade, publica NpcGiftReactionEvent). Por ora, feedback
                    // honesto por toast; fluxo coberto por testes EditMode.
                    _dialogueModal.Hide();
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        "Escolha um presente no inventario para oferecer. (Seletor em breve.)"));
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

            if (node.HasExpressionOverride && _npcData != null)
            {
                GameEventBus.Publish(new NpcExpressionOverrideEvent(_npcData.NpcId, node.ExpressionOverride));
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

            if (npcChoice.HasExpressionOverride && _npcData != null)
            {
                GameEventBus.Publish(new NpcExpressionOverrideEvent(_npcData.NpcId, npcChoice.ExpressionOverride));
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
            if (!_interaction.IsInteracting || _interaction.IsClosing || _shopMenuModal == null || !TryEnsureShopInitialized("ShowShopMenu"))
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
            if (!_interaction.TryBeginClosing()) return;
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
            if (!_interaction.IsQuestOfferHandoff)
                _modalManager?.ClearAllModals();
            _interaction.Complete();
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
