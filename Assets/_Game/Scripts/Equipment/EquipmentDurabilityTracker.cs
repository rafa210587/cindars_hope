using System;
using System.Collections.Generic;
using CindarsHope.Save;
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

            return durability.TakeDamage(1);
        }

        public void RepairEquipment(string equipmentId, int amount)
        {
            if (_equipmentDurabilities.TryGetValue(equipmentId, out var durability))
            {
                durability.Repair(amount);
            }
        }

        public void FullRepairEquipment(string equipmentId)
        {
            if (_equipmentDurabilities.TryGetValue(equipmentId, out var durability))
            {
                durability.FullRepair();
            }
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
                data.EquipmentDurabilities[kvp.Key] = new DurabilityEntry
                {
                    CurrentDurability = kvp.Value.CurrentDurability,
                    MaxDurability = kvp.Value.MaxDurability
                };
            }
            return data;
        }

        public void LoadFromSaveData(EquipmentDurabilitySaveData saveData)
        {
            _equipmentDurabilities.Clear();

            if (saveData?.EquipmentDurabilities == null)
                return;

            foreach (var kvp in saveData.EquipmentDurabilities)
            {
                var data = new DurabilityData(kvp.Value.MaxDurability);
                data.CurrentDurability = kvp.Value.CurrentDurability;
                _equipmentDurabilities[kvp.Key] = data;
            }
        }
    }
}
