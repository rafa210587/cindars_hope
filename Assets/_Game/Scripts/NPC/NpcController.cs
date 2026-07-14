using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Interaction;
using CindarsHope.NPC.Events;
using CindarsHope.NPC.Schedule;
using CindarsHope.Quests.Runtime;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using UnityEngine;
using NpcDialogueChoice = CindarsHope.NPC.DialogueChoice;
using UiDialogueChoice = CindarsHope.UI.Dialogue.DialogueChoice;
using QuestGiverInteractionMode = CindarsHope.Core.Events.QuestGiverInteractionMode;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public class NpcController : MonoBehaviour, IInteractable, INpcIdentity
    {
        private const string ThalindraNpcId = "npc_thalindra";
        private const string FirstSuppliesQuestId = "quest_first_supplies_for_cindar";

        [SerializeField] private NpcDataSO _npcData;
        [SerializeField] private DialogueModal _dialogueModal;
        [SerializeField] private ModalManager _modalManager;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private NpcWanderer _wanderer;

        private static DialogueTreeSO s_thalindraQuestDialogueTree;

        private bool _isInteracting;
        private bool _hasMet;
        private DialogueTreeSO _currentDialogueTree;
        private DialogueNode _currentNode;
        private readonly Dictionary<string, NpcDialogueChoice> _choiceMap = new Dictionary<string, NpcDialogueChoice>();

        // fable_28 — cached world state for deterministic conditional-line selection. Updated only via
        // GameEventBus (no GameObject.Find / FindObjectOfType): day from DayStartedEvent, weather from
        // WeatherChangedEvent, phase from GamePhaseChangedEvent. Defaults keep the fallback selectable
        // before any event arrives.
        private int _currentDay = 1;
        private GamePhaseChangedEvent.GamePhase _currentPhase = GamePhaseChangedEvent.GamePhase.Day;

        public string InteractionPrompt =>
            IsUnavailableBySchedule()
                ? NpcScheduleAvailabilityGate.UnavailablePrompt(_npcData)
                : $"Conversar com {_npcData?.DisplayName ?? "NPC"}";
        public NpcDataSO NpcData => _npcData;

        /// <summary>INpcIdentity (Foundation) — id do NPC, sem expor o tipo concreto a chamadores externos.</summary>
        public string NpcId => _npcData != null ? _npcData.NpcId : string.Empty;
        public bool HasMet => _hasMet;

        private bool IsUnavailableBySchedule() =>
            NpcScheduleAvailabilityGate.IsUnavailable(_npcData);

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnEnable()
        {
            NpcVisualRegistry.Register(_npcData);
            if (_dialogueModal != null)
            {
                _dialogueModal.OnClose += HandleDialogueClosed;
                _dialogueModal.OnChoiceSelected += HandleChoiceSelected;
            }

            // fable_28 — observe world state for conditional dialogue selection.
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Subscribe<GamePhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnDisable()
        {
            if (_npcData != null) NpcVisualRegistry.Unregister(_npcData.NpcId);
            if (_dialogueModal != null)
            {
                _dialogueModal.OnClose -= HandleDialogueClosed;
                _dialogueModal.OnChoiceSelected -= HandleChoiceSelected;
            }

            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Unsubscribe<GamePhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnDayStarted(DayStartedEvent evt) => _currentDay = evt.DayNumber;

        private void OnPhaseChanged(GamePhaseChangedEvent evt)
        {
            if (evt != null) _currentPhase = evt.NewPhase;
        }

        public bool CanInteract(GameObject interactor)
        {
            return _npcData != null && !_isInteracting;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            // fable_11 (CA-4): NPC at Home/Night is not available for dialogue — show the honest
            // reason instead of opening the conversation.
            if (IsUnavailableBySchedule())
            {
                NpcScheduleAvailabilityGate.PublishUnavailableFeedback(_npcData);
                return;
            }

            _isInteracting = true;
            _hasMet = true;
            _wanderer?.SetInteractionPaused(true);
            GameEventBus.Publish(new NpcInteractionStartedEvent(_npcData.NpcId));

            var dialogueTree = ResolveDialogueTreeForInteraction();
            if (dialogueTree != null)
            {
                StartDialogueTree(dialogueTree);
            }
            else if (!string.IsNullOrEmpty(_npcData.OpeningLine))
            {
                ShowSimpleDialogue();
            }
            else
            {
                EndInteraction();
            }
        }

        private DialogueTreeSO ResolveDialogueTreeForInteraction()
        {
            var authoredTree = _npcData != null ? _npcData.DialogueTree : null;
            if (ShouldUseRuntimeThalindraQuestTree(authoredTree))
            {
                return GetOrCreateThalindraQuestDialogueTree();
            }

            return authoredTree;
        }

        private bool ShouldUseRuntimeThalindraQuestTree(DialogueTreeSO authoredTree)
        {
            if (_npcData == null || _npcData.NpcId != ThalindraNpcId)
            {
                return false;
            }

            return authoredTree == null || !DialogueTreeContainsQuestOffer(authoredTree, FirstSuppliesQuestId);
        }

        private static bool DialogueTreeContainsQuestOffer(DialogueTreeSO tree, string questId)
        {
            if (tree == null || tree.Nodes == null)
            {
                return false;
            }

            foreach (var node in tree.Nodes)
            {
                if (node?.Choices == null)
                {
                    continue;
                }

                foreach (var choice in node.Choices)
                {
                    if (choice != null
                        && choice.ActionType == DialogueActionType.OfferQuest
                        && choice.ActionPayload == questId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static DialogueTreeSO GetOrCreateThalindraQuestDialogueTree()
        {
            if (s_thalindraQuestDialogueTree != null)
            {
                return s_thalindraQuestDialogueTree;
            }

            var tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.name = "Runtime_DialogueTree_Thalindra_QuestOffer";
            tree.hideFlags = HideFlags.DontSave;
            tree.Id = "dialogue_thalindra_quest_offer_runtime";
            tree.StartNodeId = "node_greeting";
            tree.Nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    NodeId = "node_greeting",
                    Text = "Cindar, preciso de materiais simples para estabilizar alguns reparos da cidade. Pode me ajudar?",
                    Choices = new List<NpcDialogueChoice>
                    {
                        new NpcDialogueChoice
                        {
                            Label = "Qual é a tarefa?",
                            NextNodeId = string.Empty,
                            ActionType = DialogueActionType.OfferQuest,
                            ActionPayload = FirstSuppliesQuestId
                        },
                        new NpcDialogueChoice
                        {
                            Label = "Por que esses suprimentos importam?",
                            NextNodeId = "node_context",
                            ActionType = DialogueActionType.None
                        },
                        new NpcDialogueChoice
                        {
                            Label = "Não tenho tempo agora.",
                            NextNodeId = string.Empty,
                            ActionType = DialogueActionType.CloseDialogue
                        }
                    }
                },
                new DialogueNode
                {
                    NodeId = "node_context",
                    Text = "Madeira e pedra bastam por enquanto. Não é uma solução final, mas mantém as bancadas e passagens seguras até termos algo melhor.",
                    Choices = new List<NpcDialogueChoice>
                    {
                        new NpcDialogueChoice
                        {
                            Label = "Certo, me diga a tarefa.",
                            NextNodeId = string.Empty,
                            ActionType = DialogueActionType.OfferQuest,
                            ActionPayload = FirstSuppliesQuestId
                        },
                        new NpcDialogueChoice
                        {
                            Label = "Entendi. Volto depois.",
                            NextNodeId = string.Empty,
                            ActionType = DialogueActionType.CloseDialogue
                        }
                    }
                }
            };

            s_thalindraQuestDialogueTree = tree;
            return s_thalindraQuestDialogueTree;
        }

        private void StartDialogueTree(DialogueTreeSO dialogueTree)
        {
            _currentDialogueTree = dialogueTree;
            var startNode = _currentDialogueTree.GetNodeById(_currentDialogueTree.StartNodeId);
            if (startNode != null)
            {
                ShowDialogueNode(startNode);
            }
            else
            {
                EndInteraction();
            }
        }

        private void ShowSimpleDialogue()
        {
            if (_dialogueModal != null)
            {
                _dialogueModal.Show(_npcData.OpeningLine);
            }
            else
            {
                EndInteraction();
            }
        }

        /// <summary>
        /// fable_28 — resolves the line shown for a node. When the node carries a conditional pool
        /// (ConditionalLines), the line is picked deterministically by world conditions + a per-day
        /// stable hash (same line all day; the existing Text/RandomLinePool stays the guaranteed
        /// fallback). Otherwise the legacy behavior is preserved: RandomLinePool with a random pick,
        /// then plain Text. This is the single dialogue selection point (Phase 0 audit).
        /// </summary>
        private string ResolveNodeText(DialogueNode node)
        {
            if (node == null) return string.Empty;

            string npcId = _npcData != null ? _npcData.NpcId : string.Empty;

            // Fallback line: the legacy random pick if a RandomLinePool exists, else the node text.
            string fallback = node.Text;
            if (node.RandomLinePool != null && node.RandomLinePool.Count > 0)
            {
                fallback = node.RandomLinePool[Random.Range(0, node.RandomLinePool.Count)];
            }

            if (node.ConditionalLines == null || node.ConditionalLines.Count == 0)
            {
                return fallback;
            }

            int hour = _currentPhase == GamePhaseChangedEvent.GamePhase.Night ? 20 : 9;
            var ctx = DialogueConditionContext.FromWorld(npcId, _currentDay, hour);
            return DialogueLineSelector.Select(node.ConditionalLines, ctx, npcId, _currentDay, fallback);
        }

        private void ShowDialogueNode(DialogueNode node)
        {
            _currentNode = node;
            if (node.HasExpressionOverride && _npcData != null)
            {
                GameEventBus.Publish(new NpcExpressionOverrideEvent(_npcData.NpcId, node.ExpressionOverride));
            }
            string text = ResolveNodeText(node);

            if (_dialogueModal == null)
            {
                EndInteraction();
                return;
            }

            if (node.Choices != null && node.Choices.Count > 0)
            {
                _dialogueModal.ShowWithChoices(text, BuildUiChoices(node.Choices));
            }
            else
            {
                _dialogueModal.Show(text);
            }
        }


        private List<UiDialogueChoice> BuildUiChoices(List<NpcDialogueChoice> choices)
        {
            _choiceMap.Clear();
            var uiChoices = new List<UiDialogueChoice>();
            for (var i = 0; i < choices.Count; i++)
            {
                var source = choices[i];
                if (source == null)
                {
                    continue;
                }

                var choiceId = $"{_currentNode?.NodeId ?? "node"}_{i}";
                _choiceMap[choiceId] = source;
                var label = source.ActionType == DialogueActionType.OfferQuest
                    ? $"! {source.Label}"
                    : source.Label;
                uiChoices.Add(new UiDialogueChoice(label, choiceId));
            }

            if (UnityEngine.Debug.isDebugBuild)
            {
                uiChoices.Add(new UiDialogueChoice("[Debug] expressao", NpcDebugExpressionChoicePolicy.OpenChoiceId));
            }

            return uiChoices;
        }

        private void HandleChoiceSelected(UiDialogueChoice choice)
        {
            if (_isInteracting && choice != null && TryHandleDebugExpressionChoice(choice))
            {
                return;
            }

            if (!_isInteracting || choice == null || !_choiceMap.TryGetValue(choice.ChoiceId, out var npcChoice))
            {
                return;
            }

            if (npcChoice.HasExpressionOverride && _npcData != null)
            {
                GameEventBus.Publish(new NpcExpressionOverrideEvent(_npcData.NpcId, npcChoice.ExpressionOverride));
            }

            if (npcChoice.ActionType == DialogueActionType.OfferQuest)
            {
                var questId = npcChoice.ActionPayload ?? "";
                var mode = ResolveQuestInteractionMode(questId);
                _dialogueModal?.Hide();
                GameEventBus.Publish(new QuestGiverInteractedEvent(
                    _npcData.NpcId,
                    questId,
                    mode));
                return;
            }

            if (npcChoice.ActionType == DialogueActionType.CloseDialogue)
            {
                _dialogueModal?.Hide();
                return;
            }

            if (npcChoice.ActionType == DialogueActionType.OpenShop)
            {
                Debug.LogWarning($"{nameof(NpcController)} received OpenShop choice for '{_npcData?.NpcId}', but this NPC is not a shop controller.", this);
                _dialogueModal?.Hide();
                return;
            }

            if (string.IsNullOrWhiteSpace(npcChoice.NextNodeId))
            {
                _dialogueModal?.Hide();
                return;
            }

            var nextNode = _currentDialogueTree != null ? _currentDialogueTree.GetNodeById(npcChoice.NextNodeId) : null;
            if (nextNode == null)
            {
                Debug.LogWarning($"{nameof(NpcController)} could not resolve dialogue node '{npcChoice.NextNodeId}' for '{_npcData?.NpcId}'.", this);
                _dialogueModal?.Hide();
                return;
            }

            ShowDialogueNode(nextNode);
        }

        private bool TryHandleDebugExpressionChoice(UiDialogueChoice choice)
        {
            if (!UnityEngine.Debug.isDebugBuild || choice == null || choice.ChoiceId == null)
            {
                return false;
            }

            if (choice.ChoiceId == NpcDebugExpressionChoicePolicy.OpenChoiceId)
            {
                ShowDebugExpressionMenu();
                return true;
            }

            if (choice.ChoiceId == NpcDebugExpressionChoicePolicy.BackChoiceId)
            {
                if (_currentNode != null) ShowDialogueNode(_currentNode); else EndInteraction();
                return true;
            }

            if (NpcDebugExpressionChoicePolicy.TryParseExpressionChoice(choice.ChoiceId, out var expr))
            {
                if (_npcData != null)
                {
                    GameEventBus.Publish(new NpcExpressionOverrideEvent(_npcData.NpcId, expr));
                }
                ShowDebugExpressionMenu();
                return true;
            }

            return false;
        }

        private void ShowDebugExpressionMenu()
        {
            if (_dialogueModal == null) return;
            var choices = NpcShopChoiceUiAdapter.ToUiChoices(NpcDebugExpressionChoicePolicy.BuildExpressionChoices());
            _dialogueModal.ShowWithChoices("[Debug] Trocar expressao:", choices);
        }

        private static QuestGiverInteractionMode ResolveQuestInteractionMode(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return QuestGiverInteractionMode.NoQuest;
            }

            var service = QuestRuntimeBootstrap.QuestService;
            if (service == null)
            {
                return QuestGiverInteractionMode.Offer;
            }

            if (service.CanTurnIn(questId))
            {
                return QuestGiverInteractionMode.TurnIn;
            }

            return service.GetQuestState(questId) == null
                ? QuestGiverInteractionMode.Offer
                : QuestGiverInteractionMode.NoQuest;
        }

        private void ShowClosingLine()
        {
            if (!string.IsNullOrEmpty(_npcData.ClosingLine))
            {
                _dialogueModal.Show(_npcData.ClosingLine);
            }
            else
            {
                HandleDialogueClosed();
            }
        }

        private void HandleDialogueClosed()
        {
            EndInteraction();
        }

        public void RestoreState(bool hasMet)
        {
            _hasMet = hasMet;
        }

        private void EndInteraction()
        {
            if (!_isInteracting)
            {
                return;
            }

            _isInteracting = false;
            _currentNode = null;
            _currentDialogueTree = null;
            _choiceMap.Clear();
            _wanderer?.SetInteractionPaused(false);
            GameEventBus.Publish(new NpcInteractionEndedEvent(_npcData.NpcId));
        }

        private void Reset()
        {
            EnsureComponents();
        }

        private void OnValidate()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_collider == null)
                _collider = GetComponent<Collider2D>();

            if (_wanderer == null)
                _wanderer = GetComponent<NpcWanderer>();
        }
    }
}
