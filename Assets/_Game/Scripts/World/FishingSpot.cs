using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Loot;
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
        [SerializeField] private LootTableSO _lootTable;
        [SerializeField] private float _castDelaySeconds = 0.5f;
        [SerializeField] private float _timingWindowSeconds = 1.25f;

        private bool _isFishing;
        private float _windowOpenTime;

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

            if (!_isFishing)
            {
                StartCoroutine(FishingRoutine());
                return;
            }

            TryConfirmFishing();
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
            _castDelaySeconds = Mathf.Max(0f, _castDelaySeconds);
            _timingWindowSeconds = Mathf.Max(0.1f, _timingWindowSeconds);
        }

        private System.Collections.IEnumerator FishingRoutine()
        {
            _isFishing = true;
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Fishing..."));
            yield return new WaitForSeconds(_castDelaySeconds);
            _windowOpenTime = Time.time;
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Press E now!"));

            while (_isFishing && Time.time - _windowOpenTime <= _timingWindowSeconds)
            {
                yield return null;
            }

            if (_isFishing)
            {
                _isFishing = false;
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Fishing failed."));
            }
        }

        private void TryConfirmFishing()
        {
            if (Time.time - _windowOpenTime > _timingWindowSeconds)
            {
                _isFishing = false;
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Fishing failed."));
                return;
            }

            _isFishing = false;
            var itemId = _fishItemId;
            var amount = _fishAmount;
            if (_lootTable != null && _lootTable.TryRoll(out var rolledItemId, out var rolledAmount))
            {
                itemId = rolledItemId;
                amount = rolledAmount;
            }

            if (!_inventoryManager.AddItem(itemId, amount))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Inventory full. Catch kept in the water."));
                Debug.LogWarning($"FishingSpot could not add fish '{itemId}' x{amount}; catch was not consumed.", this);
                return;
            }

            GameEventBus.Publish(new FishCaughtEvent(itemId, amount, Vector2Int.RoundToInt(transform.position)));
            Debug.Log($"FishingSpot caught '{itemId}' x{amount}.", this);
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
