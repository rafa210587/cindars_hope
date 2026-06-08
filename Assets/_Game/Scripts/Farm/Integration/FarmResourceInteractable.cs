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
            {
                return;
            }

            _state = FarmResourceVisualState.Depleted;
            _visualController?.Apply(_state);

            if (_inventoryManager != null && !string.IsNullOrEmpty(_reward.ItemId))
            {
                var added = _inventoryManager.AddItem(_reward.ItemId, _reward.Amount);
                if (added)
                {
                    Debug.Log($"[FarmResource] {_resourceType}: added {_reward.Amount}x {_reward.ItemId} to inventory.", this);
                }
                else
                {
                    Debug.LogWarning($"[FarmResource] {_resourceType}: inventory full — could not add {_reward.Amount}x {_reward.ItemId}.", this);
                }
            }
            else
            {
                // TODO_INTEGRATION_NOT_FINAL — feedback-only fallback when inventory not wired or item id empty
                Debug.Log($"[FarmResource] {_resourceType}: depleted. Reward {_reward.ItemId} x{_reward.Amount} (inventory not wired or item id empty).", this);
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
    }
}
