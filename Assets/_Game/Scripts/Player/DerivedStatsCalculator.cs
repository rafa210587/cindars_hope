using System.Collections.Generic;
using CindarsHope.Equipment;

namespace CindarsHope.Player
{
    public class DerivedStatsCalculator
    {
        public class DerivedStats
        {
            public int MaxHP;
            public int Attack;
            public int Defense;
            public float MoveSpeed;
            public int MaxStamina;
            public float StaminaRegen;
            public float AttackSpeed;
            public int ToxicResistance;
            public int ColdResistance;
            public int HeatResistance;
        }

        public static DerivedStats Calculate(
            int baseMaxHP,
            int baseAttack,
            int baseDefense,
            float baseMoveSpeed,
            int baseMaxStamina,
            float baseStaminaRegen,
            float baseAttackSpeed,
            Dictionary<EquipmentSlot, EquipmentDataSO> equippedItems)
        {
            var stats = new DerivedStats
            {
                MaxHP = baseMaxHP,
                Attack = baseAttack,
                Defense = baseDefense,
                MoveSpeed = baseMoveSpeed,
                MaxStamina = baseMaxStamina,
                StaminaRegen = baseStaminaRegen,
                AttackSpeed = baseAttackSpeed,
                ToxicResistance = 0,
                ColdResistance = 0,
                HeatResistance = 0
            };

            foreach (var equipment in equippedItems.Values)
            {
                if (equipment == null) continue;

                stats.Attack += equipment.StrengthBonus;
                stats.Defense += equipment.BaseDefense;
                stats.ToxicResistance += equipment.BreathBonus;
                stats.ColdResistance += equipment.ColdResistance;
                stats.HeatResistance += equipment.HeatResistance;
            }

            return stats;
        }
    }
}
