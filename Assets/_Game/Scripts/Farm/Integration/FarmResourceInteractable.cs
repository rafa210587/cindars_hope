using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Integration
{
    // TODO_INTEGRATION_NOT_FINAL — temporary resource interaction adapter for playable slice smoke validation.
    // Replace with final tool/tier/service integration when farm resources WAVE is complete.
    // Tree/Rock use existing TreeNode (chopping) / FarmResourceNodeService (generic resource nodes).
    // This adapter provides a simplified single-step interaction for smoke validation purposes only.
    [DisallowMultipleComponent]
    public sealed class FarmResourceInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private FarmResourceInteractableType _resourceType = FarmResourceInteractableType.Unknown;
        [SerializeField] private FarmResourceReward _reward = new FarmResourceReward();
        [SerializeField] private FarmResourceVisualController _visualController;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private string _interactionPrompt = "Interagir";

        private FarmResourceVisualState _state = FarmResourceVisualState.Available;

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return _state == FarmResourceVisualState.Available;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            if (_inventoryManager == null || string.IsNullOrEmpty(_reward.ItemId))
            {
                // TODO_INTEGRATION_NOT_FINAL — feedback-only fallback: inventory not wired or item id empty
                Debug.LogWarning($"[FarmResource] {_resourceType}: cannot collect — inventory not wired or reward item id empty. Resource stays Available.", this);
                return;
            }

            var amount = _reward.ClampedAmount;
            var added = _inventoryManager.AddItem(_reward.ItemId, amount);
            if (added)
            {
                _state = FarmResourceVisualState.Depleted;
                _visualController?.Apply(_state);
                Debug.Log($"[FarmResource] {_resourceType}: added {amount}x {_reward.ItemId} to inventory.", this);
            }
            else
            {
                // TODO_INTEGRATION_NOT_FINAL — AddItem returned false (inventory full or item not found); resource stays Available
                Debug.LogWarning($"[FarmResource] {_resourceType}: AddItem returned false for {amount}x {_reward.ItemId}. Resource stays Available.", this);
            }
        }

        public void ResetResource()
        {
            _state = FarmResourceVisualState.Available;
            _visualController?.Apply(_state);
        }

        private void Start()
        {
            _visualController?.Apply(_state);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_reward != null && _reward.Amount < 1)
                Debug.LogWarning($"[FarmResource] {gameObject.name}: reward amount is {_reward.Amount} — must be >= 1. ClampedAmount will be used at runtime.", this);
        }
#endif
    }
}
