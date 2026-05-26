using System.Collections.Generic;
using CindarsHope.Equipment;
using CindarsHope.Skills;

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
            public int MaxMana;
            public float ManaRegen;
            public float BowRange;
            public float BowDamageBonus;
            public float CraftTimeReduction;
            public float RepairEfficiencyBonus;
            public float HungerDrainReduction;
        }

        public static DerivedStats Calculate(
            int baseMaxHP,
            int baseAttack,
            int baseDefense,
            float baseMoveSpeed,
            int baseMaxStamina,
            float baseStaminaRegen,
            float baseAttackSpeed,
            Dictionary<EquipmentSlot, EquipmentDataSO> equippedItems,
            IList<SkillPassiveModifier> passiveModifiers = null)
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
                HeatResistance = 0,
                MaxMana = 0,
                ManaRegen = 0f,
                BowRange = 0f,
                BowDamageBonus = 0f
            };

            if (equippedItems != null)
            {
                foreach (var equipment in equippedItems.Values)
                {
                    if (equipment == null) continue;
                    stats.Attack += equipment.StrengthBonus;
                    stats.Defense += equipment.BaseDefense;
                    stats.ToxicResistance += equipment.BreathBonus;
                    stats.ColdResistance += equipment.ColdResistance;
                    stats.HeatResistance += equipment.HeatResistance;
                }
            }

            if (passiveModifiers != null)
            {
                foreach (var mod in passiveModifiers)
                {
                    switch (mod.ModifierType)
                    {
                        case SkillModifierType.AttackFlat:          stats.Attack += (int)mod.Value; break;
                        case SkillModifierType.DefenseFlat:         stats.Defense += (int)mod.Value; break;
                        case SkillModifierType.MaxHPFlat:           stats.MaxHP += (int)mod.Value; break;
                        case SkillModifierType.MaxStaminaFlat:      stats.MaxStamina += (int)mod.Value; break;
                        case SkillModifierType.MaxManaFlat:         stats.MaxMana += (int)mod.Value; break;
                        case SkillModifierType.ManaRegenFlat:       stats.ManaRegen += mod.Value; break;
                        case SkillModifierType.BowRangeFlat:        stats.BowRange += mod.Value; break;
                        case SkillModifierType.BowDamageFlat:       stats.BowDamageBonus += mod.Value; break;
                        case SkillModifierType.ToxicResistanceBonus: stats.ToxicResistance += (int)mod.Value; break;
                        case SkillModifierType.ColdResistanceBonus:  stats.ColdResistance += (int)mod.Value; break;
                        case SkillModifierType.HeatResistanceBonus:  stats.HeatResistance += (int)mod.Value; break;
                        case SkillModifierType.MoveSpeedBonus:       stats.MoveSpeed += mod.Value; break;
                        case SkillModifierType.AttackSpeedBonus:     stats.AttackSpeed += mod.Value; break;
                        case SkillModifierType.CraftTimeReductionPercent: stats.CraftTimeReduction += mod.Value; break;
                        case SkillModifierType.RepairEfficiencyBonus:     stats.RepairEfficiencyBonus += mod.Value; break;
                        case SkillModifierType.HungerDrainReduction:      stats.HungerDrainReduction += mod.Value; break;
                    }
                }
            }

            return stats;
        }
    }
}
