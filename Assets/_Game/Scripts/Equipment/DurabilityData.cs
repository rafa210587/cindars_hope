using UnityEngine;

namespace CindarsHope.Equipment
{
    [System.Serializable]
    public class DurabilityData
    {
        public int CurrentDurability;
        public int MaxDurability;

        public DurabilityData(int maxDurability)
        {
            MaxDurability = Mathf.Max(1, maxDurability);
            CurrentDurability = MaxDurability;
        }

        public float DurabilityPercent => MaxDurability > 0 ? (float)CurrentDurability / MaxDurability : 0f;
        public bool IsBroken => CurrentDurability <= 0;
        public bool IsLowDurability => DurabilityPercent < 0.25f;

        public bool TakeDamage(int amount)
        {
            if (amount <= 0)
                return !IsBroken;

            CurrentDurability = Mathf.Max(0, CurrentDurability - amount);
            return !IsBroken;
        }

        public void Repair(int amount)
        {
            if (amount <= 0)
                return;

            CurrentDurability = Mathf.Min(CurrentDurability + amount, MaxDurability);
        }

        public void FullRepair()
        {
            CurrentDurability = MaxDurability;
        }
    }

    [System.Serializable]
    public class EquipmentDurabilityData
    {
        public int ToolDurability = 100;
        public int WeaponDurability = 100;
        public int ArmorDurability = 150;
    }
}
