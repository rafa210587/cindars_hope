using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Save;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Equipment
{
    [DisallowMultipleComponent]
    public class EquipmentManager : MonoBehaviour
    {
        [SerializeField] private ItemDatabaseSO _itemDatabase;

        // Legacy fields - kept for backward compat in save/load only
        [SerializeField] private string _equippedToolId = string.Empty;
        [SerializeField] private ToolType _equippedToolType = ToolType.None;
        [SerializeField] private ToolTier _equippedToolTier = ToolTier.None;

        private EquipmentDurabilityTracker _durabilityTracker;
        private Dictionary<EquipmentSlot, string> _slots = new(); // itemInstanceId per slot

        // Legacy properties - deprecated, use GetEquippedItem() instead
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

        // LEGACY - use EquipItem instead
        public void EquipTool(string toolId, ToolType toolType, ToolTier tier)
        {
            _equippedToolId = toolId ?? string.Empty;
            _equippedToolType = toolType;
            _equippedToolTier = tier;
            EquipItem(EquipmentSlot.LeftHand, toolId);
            Debug.Log($"EquipmentManager: equipped tool {_equippedToolId} ({_equippedToolType}/{_equippedToolTier}) to LeftHand via legacy EquipTool.", this);
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

        public List<EquippedItemSnapshot> GetAllEquippedItems()
        {
            var result = new List<EquippedItemSnapshot>();

            foreach (var kvp in _slots)
            {
                var slot = kvp.Key;
                var itemInstanceId = kvp.Value;

                if (string.IsNullOrEmpty(itemInstanceId))
                {
                    continue;
                }

                var durData = _durabilityTracker?.GetDurability(itemInstanceId);

                result.Add(new EquippedItemSnapshot
                {
                    ItemId = itemInstanceId,
                    ItemInstanceId = itemInstanceId,
                    DurabilityCurrent = durData?.CurrentDurability ?? 1f,
                    DurabilityMax = durData?.MaxDurability ?? 1f,
                    IsBroken = durData?.IsBroken ?? false,
                    SlotType = slot,
                    SlotIndex = (int)slot
                });
            }

            return result;
        }

        public void UnequipAll()
        {
            var slotsToUnequip = new List<EquipmentSlot>(_slots.Keys);
            foreach (var slot in slotsToUnequip)
            {
                UnequipSlot(slot);
            }
        }

        public void RegisterEquipmentUsage()
        {
            RegisterEquipmentUsage(GetEquippedItem(EquipmentSlot.LeftHand) ?? string.Empty);
            RegisterEquipmentUsage(GetEquippedItem(EquipmentSlot.RightHand) ?? string.Empty);
        }

        public void RegisterEquipmentUsage(string itemInstanceId)
        {
            if (string.IsNullOrEmpty(itemInstanceId) || _durabilityTracker == null)
                return;

            _durabilityTracker.TryRegisterUsage(itemInstanceId);

            // Check if broken - auto-unequip
            var durData = _durabilityTracker.GetDurability(itemInstanceId);
            if (durData != null && durData.IsBroken)
            {
                AutoUnequipBrokenItem(itemInstanceId);
            }
        }

        private void AutoUnequipBrokenItem(string itemInstanceId)
        {
            var slotsToUnequip = new List<EquipmentSlot>();
            foreach (var kvp in _slots)
            {
                if (kvp.Value == itemInstanceId)
                {
                    slotsToUnequip.Add(kvp.Key);
                }
            }

            foreach (var slot in slotsToUnequip)
            {
                UnequipSlot(slot);
                Debug.Log($"EquipmentManager: auto-unequipped broken item {itemInstanceId} from {slot}.", this);
            }
        }

        public void RepairItem(string itemInstanceId, int restoreAmount)
        {
            if (_durabilityTracker == null)
                return;

            _durabilityTracker.RepairEquipment(itemInstanceId, restoreAmount);
        }

        public DurabilityData GetItemDurability(string itemInstanceId)
        {
            return _durabilityTracker?.GetDurability(itemInstanceId);
        }

        public bool IsItemBroken(string itemInstanceId)
        {
            var durData = _durabilityTracker?.GetDurability(itemInstanceId);
            return durData?.IsBroken ?? false;
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

            // Check primary equipment slots (LeftHand/RightHand for tools)
            var leftHand = GetEquippedItem(EquipmentSlot.LeftHand);
            var rightHand = GetEquippedItem(EquipmentSlot.RightHand);

            if (!string.IsNullOrEmpty(leftHand) && InferToolTypeFromId(leftHand) == requiredTool)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(rightHand) && InferToolTypeFromId(rightHand) == requiredTool)
            {
                return true;
            }

            // Fallback to legacy system for backward compat
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
            string currentTool = GetEquippedItem(EquipmentSlot.LeftHand) ?? string.Empty;
            ToolType currentType = InferToolTypeFromId(currentTool);

            switch (currentType)
            {
                case ToolType.None:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_hoe_basic");
                    break;
                case ToolType.Hoe:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_watering_can_basic");
                    break;
                case ToolType.WateringCan:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_axe_basic");
                    break;
                case ToolType.Axe:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_pickaxe_basic");
                    break;
                case ToolType.Pickaxe:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_fishing_rod_basic");
                    break;
                default:
                    UnequipSlot(EquipmentSlot.LeftHand);
                    break;
            }
        }

        private void InferEquippedToolFromId()
        {
            _equippedToolType = InferToolTypeFromId(_equippedToolId);
            _equippedToolTier = string.IsNullOrWhiteSpace(_equippedToolId) ? ToolTier.None : ToolTier.Basic;
        }

        private ToolType InferToolTypeFromId(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId))
                return ToolType.None;

            if (toolId.Contains("hoe"))
                return ToolType.Hoe;
            if (toolId.Contains("axe"))
                return ToolType.Axe;
            if (toolId.Contains("pickaxe"))
                return ToolType.Pickaxe;
            if (toolId.Contains("fishing_rod"))
                return ToolType.FishingRod;
            if (toolId.Contains("watering_can"))
                return ToolType.WateringCan;
            if (toolId.Contains("sickle"))
                return ToolType.Sickle;

            return ToolType.None;
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

    public sealed class EquippedItemSnapshot
    {
        public string ItemId;
        public string ItemInstanceId;
        public float DurabilityCurrent;
        public float DurabilityMax;
        public bool IsBroken;
        public EquipmentSlot SlotType;
        public int SlotIndex;
    }
}
