using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Equipment
{
    public class EquipmentDurabilityTracker
    {
        private Dictionary<string, DurabilityData> _equipmentDurabilities = new();

        public void InitializeEquipment(string equipmentId, int maxDurability)
        {
            if (!_equipmentDurabilities.ContainsKey(equipmentId))
            {
                _equipmentDurabilities[equipmentId] = new DurabilityData(maxDurability);
            }
        }

        public DurabilityData GetDurability(string equipmentId)
        {
            if (_equipmentDurabilities.TryGetValue(equipmentId, out var durability))
            {
                return durability;
            }

            return null;
        }

        public bool TryRegisterUsage(string equipmentId)
        {
            if (!_equipmentDurabilities.TryGetValue(equipmentId, out var durability))
            {
                return false;
            }

            bool wasBroken = durability.IsBroken;
            durability.TakeDamage(1);

            GameEventBus.Publish(new DurabilityChangedEvent(equipmentId, durability.CurrentDurability, durability.MaxDurability));

            if (!wasBroken && durability.IsBroken)
            {
                GameEventBus.Publish(new ItemBrokenEvent(equipmentId, EquipmentSlot.None));
            }

            return !durability.IsBroken;
        }

        public void RepairEquipment(string equipmentId, int amount)
        {
            if (!_equipmentDurabilities.TryGetValue(equipmentId, out var durability))
                return;

            int oldDurability = durability.CurrentDurability;
            durability.Repair(amount);
            int restored = durability.CurrentDurability - oldDurability;

            GameEventBus.Publish(new DurabilityChangedEvent(equipmentId, durability.CurrentDurability, durability.MaxDurability));
            GameEventBus.Publish(new ItemRepairedEvent(equipmentId, restored));
        }

        public void FullRepairEquipment(string equipmentId)
        {
            if (!_equipmentDurabilities.TryGetValue(equipmentId, out var durability))
                return;

            durability.FullRepair();
            GameEventBus.Publish(new DurabilityChangedEvent(equipmentId, durability.CurrentDurability, durability.MaxDurability));
            GameEventBus.Publish(new ItemRepairedEvent(equipmentId, durability.MaxDurability));
        }

        public void RemoveEquipment(string equipmentId)
        {
            _equipmentDurabilities.Remove(equipmentId);
        }

        public EquipmentDurabilitySaveData CaptureSaveData()
        {
            var data = new EquipmentDurabilitySaveData();
            foreach (var kvp in _equipmentDurabilities)
            {
                data.EquipmentDurabilities.Add(new DurabilityEntryData
                {
                    ItemInstanceId = kvp.Key,
                    CurrentDurability = kvp.Value.CurrentDurability,
                    MaxDurability = kvp.Value.MaxDurability
                });
            }
            return data;
        }

        public void LoadFromSaveData(EquipmentDurabilitySaveData saveData)
        {
            _equipmentDurabilities.Clear();

            if (saveData?.EquipmentDurabilities == null)
                return;

            foreach (var entry in saveData.EquipmentDurabilities)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemInstanceId))
                    continue;

                var data = new DurabilityData(entry.MaxDurability);
                data.CurrentDurability = entry.CurrentDurability;
                _equipmentDurabilities[entry.ItemInstanceId] = data;
            }
        }
    }
}
