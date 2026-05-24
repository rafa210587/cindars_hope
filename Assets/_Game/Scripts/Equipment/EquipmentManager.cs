using System.Collections.Generic;
using CindarsHope.Core.Events;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Equipment
{
    [DisallowMultipleComponent]
    public class EquipmentManager : MonoBehaviour
    {
        [SerializeField] private string _equippedToolId = string.Empty;
        [SerializeField] private ToolType _equippedToolType = ToolType.None;
        [SerializeField] private ToolTier _equippedToolTier = ToolTier.None;

        private EquipmentDurabilityTracker _durabilityTracker;
        private Dictionary<EquipmentSlot, string> _slots = new();

        public string EquippedToolId => _equippedToolId;
        public ToolType EquippedToolType => _equippedToolType;
        public ToolTier EquippedToolTier => _equippedToolTier;
        public EquipmentDurabilityTracker DurabilityTracker => _durabilityTracker;

        private void Awake()
        {
            if (_durabilityTracker == null)
            {
                _durabilityTracker = new EquipmentDurabilityTracker();
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<InventoryChangedEvent>(HandleInventoryChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<InventoryChangedEvent>(HandleInventoryChanged);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                CycleDebugTool();
            }
        }

        public void EquipTool(string toolId, ToolType toolType, ToolTier tier)
        {
            _equippedToolId = toolId ?? string.Empty;
            _equippedToolType = toolType;
            _equippedToolTier = tier;
            Debug.Log($"EquipmentManager: equipped tool {_equippedToolId} ({_equippedToolType}/{_equippedToolTier}).", this);
        }

        public void EquipItem(EquipmentSlot slot, string itemInstanceId)
        {
            _slots[slot] = itemInstanceId ?? string.Empty;
            GameEventBus.Publish(new EquipmentSlotChangedEvent(slot, itemInstanceId));
        }

        public void UnequipSlot(EquipmentSlot slot)
        {
            if (_slots.ContainsKey(slot))
            {
                _slots.Remove(slot);
                GameEventBus.Publish(new EquipmentSlotChangedEvent(slot, null));
            }
        }

        public string GetEquippedItem(EquipmentSlot slot)
        {
            return _slots.TryGetValue(slot, out var itemInstanceId) ? itemInstanceId : null;
        }

        public void RegisterEquipmentUsage()
        {
            // Generic registration without specific item - used by combat
        }

        public void RegisterEquipmentUsage(string itemInstanceId)
        {
            if (_durabilityTracker != null)
            {
                _durabilityTracker.TryRegisterUsage(itemInstanceId);
            }
        }

        public bool HasTool(ToolType requiredTool)
        {
            return HasTool(requiredTool, ToolTier.None);
        }

        public bool HasTool(ToolType requiredTool, ToolTier minimumTier)
        {
            if (requiredTool == ToolType.None)
            {
                return true;
            }

            return _equippedToolType == requiredTool && _equippedToolTier >= minimumTier;
        }

        public bool TryGetMissingToolMessage(ToolType requiredTool, ToolTier minimumTier, out string message)
        {
            message = string.Empty;

            if (HasTool(requiredTool, minimumTier))
            {
                return false;
            }

            message = minimumTier > ToolTier.Basic
                ? $"Requires {FormatToolTier(minimumTier)} {FormatToolType(requiredTool)} or better."
                : $"Requires {FormatToolType(requiredTool)}.";
            return true;
        }

        public EquipmentSaveData CaptureSaveData()
        {
            var data = new EquipmentSaveData
            {
                EquippedToolId = _equippedToolId,
                Slots = new List<EquipmentSlotSaveData>()
            };

            foreach (var kvp in _slots)
            {
                data.Slots.Add(new EquipmentSlotSaveData
                {
                    SlotType = kvp.Key,
                    ItemInstanceId = kvp.Value
                });
            }

            return data;
        }

        public void RestoreFromSaveData(EquipmentSaveData saveData)
        {
            if (saveData == null)
            {
                _equippedToolId = string.Empty;
                _equippedToolType = ToolType.None;
                _equippedToolTier = ToolTier.None;
                _slots.Clear();
                return;
            }

            _equippedToolId = saveData.EquippedToolId ?? string.Empty;
            InferEquippedToolFromId();

            _slots.Clear();
            if (saveData.Slots != null)
            {
                foreach (var slotData in saveData.Slots)
                {
                    _slots[slotData.SlotType] = slotData.ItemInstanceId;
                }
            }
        }

        private void HandleInventoryChanged(InventoryChangedEvent evt)
        {
            var slotsToRemove = new List<EquipmentSlot>();
            foreach (var kvp in _slots)
            {
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    slotsToRemove.Add(kvp.Key);
                }
            }

            foreach (var slot in slotsToRemove)
            {
                UnequipSlot(slot);
            }
        }

        private void CycleDebugTool()
        {
            switch (_equippedToolType)
            {
                case ToolType.None:
                    EquipTool("item_tool_hoe_basic", ToolType.Hoe, ToolTier.Basic);
                    break;
                case ToolType.Hoe:
                    EquipTool("item_tool_watering_can_basic", ToolType.WateringCan, ToolTier.Basic);
                    break;
                case ToolType.WateringCan:
                    EquipTool("item_tool_axe_basic", ToolType.Axe, ToolTier.Basic);
                    break;
                case ToolType.Axe:
                    EquipTool("item_tool_pickaxe_basic", ToolType.Pickaxe, ToolTier.Basic);
                    break;
                case ToolType.Pickaxe:
                    EquipTool("item_tool_fishing_rod_basic", ToolType.FishingRod, ToolTier.Basic);
                    break;
                default:
                    EquipTool(string.Empty, ToolType.None, ToolTier.None);
                    break;
            }
        }

        private void InferEquippedToolFromId()
        {
            _equippedToolTier = string.IsNullOrWhiteSpace(_equippedToolId) ? ToolTier.None : ToolTier.Basic;

            if (_equippedToolId.Contains("hoe"))
            {
                _equippedToolType = ToolType.Hoe;
            }
            else if (_equippedToolId.Contains("axe"))
            {
                _equippedToolType = ToolType.Axe;
            }
            else if (_equippedToolId.Contains("pickaxe"))
            {
                _equippedToolType = ToolType.Pickaxe;
            }
            else if (_equippedToolId.Contains("fishing_rod"))
            {
                _equippedToolType = ToolType.FishingRod;
            }
            else if (_equippedToolId.Contains("watering_can"))
            {
                _equippedToolType = ToolType.WateringCan;
            }
            else
            {
                _equippedToolType = ToolType.None;
                _equippedToolTier = ToolTier.None;
            }
        }

        private static string FormatToolType(ToolType toolType)
        {
            switch (toolType)
            {
                case ToolType.Axe:
                    return "Axe";
                case ToolType.Pickaxe:
                    return "Pickaxe";
                case ToolType.FishingRod:
                    return "Fishing Rod";
                case ToolType.Hoe:
                    return "Hoe";
                case ToolType.WateringCan:
                    return "Watering Can";
                case ToolType.Sickle:
                    return "Sickle";
                default:
                    return "Tool";
            }
        }

        private static string FormatToolTier(ToolTier tier)
        {
            switch (tier)
            {
                case ToolTier.Copper:
                    return "Copper";
                case ToolTier.Iron:
                    return "Iron";
                case ToolTier.Gold:
                    return "Gold";
                case ToolTier.Diamond:
                    return "Diamond";
                case ToolTier.Basic:
                    return "Basic";
                default:
                    return string.Empty;
            }
        }
    }
}
