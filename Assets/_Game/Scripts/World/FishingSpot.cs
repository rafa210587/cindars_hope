using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class FishingSpot : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private string _requiredToolId = "item_tool_fishing_rod_basic";
        [SerializeField] private string _fishItemId = "item_fish_common";
        [SerializeField] private int _fishAmount = 1;

        public string InteractionPrompt => "Pescar";

        public bool CanInteract(GameObject interactor)
        {
            return _inventoryManager != null && _inventoryManager.HasItem(_requiredToolId);
        }

        public void Interact(GameObject interactor)
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning("FishingSpot cannot fish because InventoryManager is missing.", this);
                return;
            }

            if (!_inventoryManager.HasItem(_requiredToolId))
            {
                Debug.Log($"FishingSpot requires '{_requiredToolId}'.", this);
                return;
            }

            if (!_inventoryManager.AddItem(_fishItemId, _fishAmount))
            {
                Debug.LogWarning($"FishingSpot could not add fish '{_fishItemId}' x{_fishAmount}.", this);
                return;
            }

            GameEventBus.Publish(new FishCaughtEvent(_fishItemId, _fishAmount, Vector2Int.RoundToInt(transform.position)));
            Debug.Log($"FishingSpot caught '{_fishItemId}' x{_fishAmount}.", this);
        }

        private void OnValidate()
        {
            _fishAmount = Mathf.Max(1, _fishAmount);
        }
    }
}
