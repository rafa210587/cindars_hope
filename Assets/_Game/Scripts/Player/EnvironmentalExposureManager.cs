using System.Collections.Generic;
using CindarsHope.Core.Events;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.Player
{

    [System.Serializable]
    public class EnvironmentalExposure
    {
        public HazardType HazardType;
        public float RemainingSeconds;
        public string StatusEffectId;

        public EnvironmentalExposure(HazardType hazardType, float duration, string statusEffectId)
        {
            HazardType = hazardType;
            RemainingSeconds = duration;
            StatusEffectId = statusEffectId;
        }
    }

    public class EnvironmentalExposureManager : MonoBehaviour
    {
        private Dictionary<HazardType, EnvironmentalExposure> _activeExposures = new();

        public int GetTotalResistance(HazardType hazardType, int equipmentResistance)
        {
            return equipmentResistance;
        }

        public void ApplyExposure(HazardType hazardType, float duration, string statusEffectId)
        {
            _activeExposures[hazardType] = new EnvironmentalExposure(hazardType, duration, statusEffectId);
            GameEventBus.Publish(new EnvironmentalExposureStartedEvent(hazardType, statusEffectId));
        }

        public void ClearExposure(HazardType hazardType)
        {
            if (_activeExposures.Remove(hazardType))
            {
                GameEventBus.Publish(new EnvironmentalExposureEndedEvent(hazardType));
            }
        }

        public bool IsExposed(HazardType hazardType)
        {
            return _activeExposures.ContainsKey(hazardType);
        }

        public EnvironmentalExposureSaveData CaptureSaveData()
        {
            var data = new EnvironmentalExposureSaveData();
            foreach (var kvp in _activeExposures)
            {
                data.Exposures.Add(new EnvironmentalExposureEntry
                {
                    HazardType = kvp.Key,
                    RemainingSeconds = kvp.Value.RemainingSeconds,
                    StatusEffectId = kvp.Value.StatusEffectId
                });
            }
            return data;
        }

        public void RestoreFromSaveData(EnvironmentalExposureSaveData saveData)
        {
            _activeExposures.Clear();
            if (saveData?.Exposures != null)
            {
                foreach (var entry in saveData.Exposures)
                {
                    _activeExposures[entry.HazardType] = new EnvironmentalExposure(
                        entry.HazardType,
                        entry.RemainingSeconds,
                        entry.StatusEffectId
                    );
                }
            }
        }
    }

    [System.Serializable]
    public class EnvironmentalExposureSaveData
    {
        public List<EnvironmentalExposureEntry> Exposures = new List<EnvironmentalExposureEntry>();
    }

    [System.Serializable]
    public class EnvironmentalExposureEntry
    {
        public HazardType HazardType;
        public float RemainingSeconds;
        public string StatusEffectId;
    }
}
