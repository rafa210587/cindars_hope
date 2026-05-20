using CindarsHope.Cave.Data;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Cave.Resources
{
    [DisallowMultipleComponent]
    public sealed class ResourceNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _nodeInstanceId;
        [SerializeField] private ResourceNodeDataSO _nodeData;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private int _hitsTaken;
        private bool _isDepleted;

        public string NodeInstanceId => _nodeInstanceId;
        public bool IsDepleted => _isDepleted;
        public string InteractionPrompt => _isDepleted ? "Depleted" : "Gather";

        public void Configure(
            string nodeInstanceId,
            ResourceNodeDataSO nodeData,
            InventoryManager inventoryManager,
            EquipmentManager equipmentManager,
            CaveRunManager caveRunManager,
            SpriteRenderer spriteRenderer)
        {
            _nodeInstanceId = nodeInstanceId;
            _nodeData = nodeData;
            _inventoryManager = inventoryManager;
            _equipmentManager = equipmentManager;
            _caveRunManager = caveRunManager;
            _spriteRenderer = spriteRenderer;
            RestoreDepletedStateFromRun();
            UpdateVisual();
        }

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            RebindFromBootstrapIfNeeded();
            RestoreDepletedStateFromRun();
            UpdateVisual();
        }

        public bool CanInteract(GameObject interactor)
        {
            return !_isDepleted && _nodeData != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            RebindFromBootstrapIfNeeded();
            var equippedTool = _equipmentManager != null ? _equipmentManager.EquippedToolType : Tools.ToolType.None;
            var equippedTier = _equipmentManager != null ? _equipmentManager.EquippedToolTier : Tools.ToolTier.None;
            var toolResult = ResourceNodeRules.CheckTool(_nodeData, equippedTool, equippedTier);

            if (!toolResult.CanHarvest)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(toolResult.Message));
                Debug.Log($"ResourceNode '{_nodeInstanceId}': {toolResult.Message}", this);
                UpdateVisual();
                return;
            }

            _hitsTaken++;
            Debug.Log($"ResourceNode '{_nodeInstanceId}' hit {_hitsTaken}/{_nodeData.HitsRequired}.", this);
            if (_hitsTaken < _nodeData.HitsRequired)
            {
                UpdateVisual();
                return;
            }

            DeliverResult(ResourceNodeRules.BuildPrimaryDrop(_nodeData));
        }

        public void RestoreDepletedStateFromRun()
        {
            if (_caveRunManager != null && _caveRunManager.IsNodeDepleted(_nodeInstanceId))
            {
                _isDepleted = true;
                _hitsTaken = _nodeData != null ? _nodeData.HitsRequired : _hitsTaken;
            }
        }

        private void DeliverResult(ResourceNodeInteractionResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(result.Message));
                Debug.Log($"ResourceNode '{_nodeInstanceId}': {result.Message}", this);
            }

            if (result.DeliveredItem && _inventoryManager != null)
            {
                _inventoryManager.AddItem(result.ItemId, result.Amount);
            }

            if (!result.DepletedNode)
            {
                UpdateVisual();
                return;
            }

            _isDepleted = true;
            _hitsTaken = _nodeData != null ? _nodeData.HitsRequired : _hitsTaken;
            _caveRunManager?.RegisterDepletedNode(_nodeInstanceId);
            GameEventBus.Publish(new ResourceNodeDepletedEvent(_nodeInstanceId, _nodeData != null ? _nodeData.Id : string.Empty, result.ItemId));
            UpdateVisual();
        }

        private void RebindFromBootstrapIfNeeded()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return;
            }

            if (_inventoryManager == null)
            {
                _inventoryManager = bootstrap.InventoryManager;
            }

            if (_equipmentManager == null)
            {
                _equipmentManager = bootstrap.EquipmentManager;
            }
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            if (_isDepleted)
            {
                _spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.45f);
                return;
            }

            var hitRatio = _nodeData == null || _nodeData.HitsRequired <= 0
                ? 0f
                : Mathf.Clamp01((float)_hitsTaken / _nodeData.HitsRequired);
            _spriteRenderer.color = Color.Lerp(new Color(0.45f, 0.45f, 0.55f), new Color(0.75f, 0.75f, 0.85f), hitRatio);
        }
    }
}
