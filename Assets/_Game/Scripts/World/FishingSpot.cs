using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Loot;
using CindarsHope.Player;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class FishingSpot : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private string _requiredToolId = "item_tool_fishing_rod_basic";
        [SerializeField] private string _fishItemId = "item_fish_common";
        [SerializeField] private int _fishAmount = 1;
        [SerializeField] private LootTableSO _lootTable;
        [SerializeField] private float _castDelaySeconds = 0.5f;
        [SerializeField] private float _timingWindowSeconds = 1.25f;
        [SerializeField] private bool _requireEdgeInteraction = true;
        [SerializeField] private Vector2 _edgeInteractionOuterHalfExtents = new Vector2(0.45f, 0.45f);
        [SerializeField] private Vector2 _edgeInteractionInnerHalfExtents = new Vector2(0.35f, 0.35f);

        private bool _isFishing;
        private float _windowOpenTime;

        public string InteractionPrompt => "Pescar";

        public bool CanInteract(GameObject interactor)
        {
            if (_inventoryManager == null)
            {
                return false;
            }

            return !_requireEdgeInteraction || IsInteractorOnEdge(interactor);
        }

        public void Interact(GameObject interactor)
        {
            const int fishCastStaminaCost = 20;

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
                if (_staminaManager != null && _staminaManager.CurrentStamina < fishCastStaminaCost)
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Not enough stamina to fish."));
                    return;
                }

                StartCoroutine(FishingRoutine(fishCastStaminaCost));
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

        public void RebindStaminaManager(StaminaManager staminaManager)
        {
            _staminaManager = staminaManager;
        }

        private void OnValidate()
        {
            _fishAmount = Mathf.Max(1, _fishAmount);
            _castDelaySeconds = Mathf.Max(0f, _castDelaySeconds);
            _timingWindowSeconds = Mathf.Max(0.1f, _timingWindowSeconds);
            _edgeInteractionOuterHalfExtents = Max(_edgeInteractionOuterHalfExtents, new Vector2(0.01f, 0.01f));
            _edgeInteractionInnerHalfExtents = Vector2.Min(_edgeInteractionInnerHalfExtents, _edgeInteractionOuterHalfExtents);
            _edgeInteractionInnerHalfExtents = Max(_edgeInteractionInnerHalfExtents, Vector2.zero);
        }

        private bool IsInteractorOnEdge(GameObject interactor)
        {
            if (interactor == null)
            {
                return false;
            }

            var localPosition = transform.InverseTransformPoint(interactor.transform.position);
            var absoluteLocal = new Vector2(Mathf.Abs(localPosition.x), Mathf.Abs(localPosition.y));
            var insideOuter = absoluteLocal.x <= _edgeInteractionOuterHalfExtents.x
                && absoluteLocal.y <= _edgeInteractionOuterHalfExtents.y;
            var outsideInnerWater = absoluteLocal.x >= _edgeInteractionInnerHalfExtents.x
                || absoluteLocal.y >= _edgeInteractionInnerHalfExtents.y;

            return insideOuter && outsideInnerWater;
        }

        private static Vector2 Max(Vector2 value, Vector2 minimum)
        {
            return new Vector2(Mathf.Max(value.x, minimum.x), Mathf.Max(value.y, minimum.y));
        }

        private System.Collections.IEnumerator FishingRoutine(int staminaCost)
        {
            _isFishing = true;
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                _isFishing = false;
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Not enough stamina to fish."));
                yield break;
            }

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
