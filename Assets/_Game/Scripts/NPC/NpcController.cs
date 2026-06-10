using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using UnityEngine;
using NpcDialogueChoice = CindarsHope.NPC.DialogueChoice;
using UiDialogueChoice = CindarsHope.UI.Dialogue.DialogueChoice;
using QuestGiverInteractionMode = CindarsHope.Core.Events.QuestGiverInteractionMode;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public class NpcController : MonoBehaviour, IInteractable
    {
        [SerializeField] private NpcDataSO _npcData;
        [SerializeField] private DialogueModal _dialogueModal;
        [SerializeField] private ModalManager _modalManager;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private NpcWanderer _wanderer;

        private bool _isInteracting;
        private bool _hasMet;
        private DialogueTreeSO _currentDialogueTree;
        private DialogueNode _currentNode;
        private readonly Dictionary<string, NpcDialogueChoice> _choiceMap = new Dictionary<string, NpcDialogueChoice>();

        public string InteractionPrompt => $"Conversar com {_npcData?.DisplayName ?? "NPC"}";
        public NpcDataSO NpcData => _npcData;
        public bool HasMet => _hasMet;

        private void OnEnable()
        {
            if (_dialogueModal != null)
            {
                _dialogueModal.OnClose += HandleDialogueClosed;
                _dialogueModal.OnChoiceSelected += HandleChoiceSelected;
            }
        }

        private void OnDisable()
        {
            if (_dialogueModal != null)
            {
                _dialogueModal.OnClose -= HandleDialogueClosed;
                _dialogueModal.OnChoiceSelected -= HandleChoiceSelected;
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return _npcData != null && !_isInteracting;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            _isInteracting = true;
            _hasMet = true;
            _wanderer?.SetInteractionPaused(true);
            GameEventBus.Publish(new NpcInteractionStartedEvent(_npcData.NpcId));

            if (_npcData.DialogueTree != null)
            {
                StartDialogueTree();
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

        private void StartDialogueTree()
        {
            _currentDialogueTree = _npcData.DialogueTree;
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

        private void ShowDialogueNode(DialogueNode node)
        {
            _currentNode = node;
            string text = node.Text;

            if (node.RandomLinePool != null && node.RandomLinePool.Count > 0)
            {
                text = node.RandomLinePool[Random.Range(0, node.RandomLinePool.Count)];
            }

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

            return uiChoices;
        }

        private void HandleChoiceSelected(UiDialogueChoice choice)
        {
            if (!_isInteracting || choice == null || !_choiceMap.TryGetValue(choice.ChoiceId, out var npcChoice))
            {
                return;
            }

            if (npcChoice.ActionType == DialogueActionType.OfferQuest)
            {
                var questId = npcChoice.ActionPayload ?? "";
                _dialogueModal?.Hide();
                GameEventBus.Publish(new QuestGiverInteractedEvent(
                    _npcData.NpcId,
                    questId,
                    QuestGiverInteractionMode.Offer));
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
