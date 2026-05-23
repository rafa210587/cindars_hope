using System;

namespace CindarsHope.Equipment
{
    [Serializable]
    public class DurabilityManager
    {
        public int CurrentDurability = 100;
        private int _usageCount;
        private const int UsesPerDurabilityLoss = 3;

        public DurabilityManager(int maxDurability = 100)
        {
            CurrentDurability = maxDurability;
            _usageCount = 0;
        }

        public void RegisterUsage()
        {
            _usageCount++;
            if (_usageCount >= UsesPerDurabilityLoss)
            {
                CurrentDurability = UnityEngine.Mathf.Max(0, CurrentDurability - 1);
                _usageCount = 0;
            }
        }

        public bool IsBroken => CurrentDurability <= 0;

        public float GetDurabilityPercentage(int maxDurability)
        {
            return (float)CurrentDurability / maxDurability;
        }

        public void Repair(int maxDurability)
        {
            CurrentDurability = maxDurability;
            _usageCount = 0;
        }
    }
}
