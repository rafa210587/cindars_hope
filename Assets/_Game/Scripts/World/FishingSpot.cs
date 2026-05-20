using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Tools;
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
            return _inventoryManager != null;
        }

        public void Interact(GameObject interactor)
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning("FishingSpot cannot fish because InventoryManager is missing.", this);
                return;
            }

            if (!HasRequiredTool())
            {
                const string message = "Requires Fishing Rod.";
                GameEventBus.Publish(new PlayerActionFeedbackEvent(message));
                Debug.Log($"FishingSpot blocked fishing. {message} Expected tool id hint '{_requiredToolId}'.", this);
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

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning("FishingSpot received null InventoryManager for rebind.", this);
                return;
            }

            _inventoryManager = inventoryManager;
        }

        private void OnValidate()
        {
            _fishAmount = Mathf.Max(1, _fishAmount);
        }

        private static bool HasRequiredTool()
        {
            EquipmentManager equipmentManager = null;
            if (GameBootstrap.Instance != null)
            {
                equipmentManager = GameBootstrap.Instance.EquipmentManager;
            }

            return equipmentManager != null && equipmentManager.HasTool(ToolType.FishingRod, ToolTier.Basic);
        }
    }
}
