using CindarsHope.Equipment;
using UnityEngine;

namespace CindarsHope.Combat
{
    public static class DamageCalculator
    {
        public static DamageResult CalculateDirectDamage(
            int baseDamage,
            int attributeBonus = 0,
            float typeMultiplier = 1f,
            EquipmentManager equipmentManager = null,
            float durabilityDamageMultiplier = 0.1f)
        {
            baseDamage = Mathf.Max(0, baseDamage);
            attributeBonus = Mathf.Max(0, attributeBonus);
            typeMultiplier = Mathf.Max(0f, typeMultiplier);

            if (baseDamage <= 0)
            {
                return new DamageResult(0, baseDamage, attributeBonus, typeMultiplier, false, false);
            }

            if (Mathf.Approximately(typeMultiplier, 0f))
            {
                return new DamageResult(0, baseDamage, attributeBonus, typeMultiplier, false, true);
            }

            int scaledBase = baseDamage + attributeBonus;
            int finalDamage = Mathf.RoundToInt(scaledBase * typeMultiplier);
            bool reducedToMinimum = finalDamage < 1;
            finalDamage = Mathf.Max(1, finalDamage);

            // Apply durability damage if equipment manager provided
            if (equipmentManager != null)
            {
                equipmentManager.RegisterEquipmentUsage();
            }

            return new DamageResult(finalDamage, baseDamage, attributeBonus, typeMultiplier, reducedToMinimum, false);
        }
    }
}
