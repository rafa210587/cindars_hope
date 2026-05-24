using System.Collections.Generic;
using CindarsHope.Interaction;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using UnityEngine;

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

        private bool _isInteracting = false;
        private DialogueTreeSO _currentDialogueTree;
        private DialogueNode _currentNode;

        public string InteractionPrompt => $"Conversar com {_npcData?.DisplayName ?? "NPC"}";

        private void OnEnable()
        {
            if (_dialogueModal != null)
            {
                _dialogueModal.OnChoiceSelected += HandleChoiceSelected;
                _dialogueModal.OnClose += HandleDialogueClosed;
            }
        }

        private void OnDisable()
        {
            if (_dialogueModal != null)
            {
                _dialogueModal.OnChoiceSelected -= HandleChoiceSelected;
                _dialogueModal.OnClose -= HandleDialogueClosed;
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return _npcData != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor) || _isInteracting)
                return;

            _isInteracting = true;

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
                _isInteracting = false;
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
                _isInteracting = false;
            }
        }

        private void ShowSimpleDialogue()
        {
            _dialogueModal.Show(_npcData.OpeningLine);
        }

        private void ShowDialogueNode(DialogueNode node)
        {
            _currentNode = node;
            string text = node.Text;

            if (!string.IsNullOrEmpty(text) && node.RandomLinePool.Count > 0)
            {
                text = node.RandomLinePool[Random.Range(0, node.RandomLinePool.Count)];
            }

            if (node.Choices != null && node.Choices.Count > 0)
            {
                _dialogueModal.ShowWithChoices(text, node.Choices);
            }
            else
            {
                _dialogueModal.Show(text);
            }
        }

        private void HandleChoiceSelected(DialogueChoice choice)
        {
            if (choice.ActionType == DialogueActionType.OpenShop)
            {
                OpenShop(choice.ActionPayload);
                return;
            }

            if (choice.ActionType == DialogueActionType.CloseDialogue)
            {
                ShowClosingLine();
                return;
            }

            if (!string.IsNullOrEmpty(choice.NextNodeId))
            {
                var nextNode = _currentDialogueTree.GetNodeById(choice.NextNodeId);
                if (nextNode != null)
                {
                    ShowDialogueNode(nextNode);
                    return;
                }
            }

            ShowClosingLine();
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

        private void OpenShop(string shopId)
        {
            Debug.Log($"Opening shop: {shopId}");
            _modalManager?.ClearAllModals();
            _isInteracting = false;
        }

        private void HandleDialogueClosed()
        {
            _isInteracting = false;
            _currentNode = null;
            _currentDialogueTree = null;
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
        }
    }
}
