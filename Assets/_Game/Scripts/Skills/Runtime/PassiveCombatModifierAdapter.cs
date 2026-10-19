using System;
using System.Collections.Generic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>
    /// Resolves passive combat modifiers whose identity cannot be represented by a generic stat.
    /// Equipment gates are evaluated from an immutable snapshot supplied by Combat.
    /// </summary>
    public static class PassiveCombatModifierAdapter
    {
        public const string IronGripNodeId = "melee_iron_grip";
        public const string GuardedStanceNodeId = "melee_guarded_stance";
        public const string DualWieldFlowNodeId = "melee_dual_wield_flow";
        public const string TwoHandedMomentumNodeId = "melee_two_handed_momentum";
        public const string SteadyHandNodeId = "ranged_steady_hand";
        public const string LongSightNodeId = "ranged_long_sight";
        public const string QuickNockNodeId = "ranged_quick_nock";
        public const string KitingStepsNodeId = "ranged_kiting_steps";
        public const string ProjectileTuningNodeId = "ranged_projectile_tuning";
        public const string ArcaneEdgeNodeId = "magic_arcane_edge";
        public const string ArcaneBoltMasteryNodeId = "magic_arcane_bolt_mastery";
        public const string ArcaneBoltSpellId = "arcane_projectile";

        public readonly struct EquipmentSnapshot
        {
            public readonly WeaponDataSO MainHand;
            public readonly WeaponDataSO OffHand;
            public readonly bool MainHandBroken;
            public readonly bool OffHandBroken;
            public readonly bool OffHandIsShield;
            public readonly bool OffHandEmpty;

            public EquipmentSnapshot(
                WeaponDataSO mainHand,
                WeaponDataSO offHand,
                bool mainHandBroken = false,
                bool offHandBroken = false,
                bool offHandIsShield = false,
                bool offHandEmpty = true)
            {
                MainHand = mainHand;
                OffHand = offHand;
                MainHandBroken = mainHandBroken;
                OffHandBroken = offHandBroken;
                OffHandIsShield = offHandIsShield;
                OffHandEmpty = offHandEmpty;
            }
        }

        public static bool IsMelee(WeaponDataSO weapon)
        {
            return weapon != null
                && weapon.Type != WeaponType.None
                && weapon.Type != WeaponType.Bow
                && weapon.Type != WeaponType.Staff
                && weapon.Type != WeaponType.Wand
                && weapon.Type != WeaponType.Tool;
        }

        public static bool IsValidMelee(WeaponDataSO weapon, bool broken) => IsMelee(weapon) && !broken;

        public static bool IsValidBow(WeaponDataSO weapon, bool broken)
            => weapon != null && weapon.Type == WeaponType.Bow && !broken;

        public static float MeleeAttackFlat(IList<SkillPassiveModifier> modifiers, EquipmentSnapshot equipment)
            => MeleeAttackFlat(Sum(modifiers, SkillModifierType.MeleeAttackFlat), equipment);

        public static float MeleeAttackFlat(float bonus, EquipmentSnapshot equipment)
            => IsValidMelee(equipment.MainHand, equipment.MainHandBroken) ? Math.Max(0f, bonus) : 0f;

        public static float MagicAttackFlat(IList<SkillPassiveModifier> modifiers)
            => Sum(modifiers, SkillModifierType.MagicAttackFlat, ArcaneEdgeNodeId);

        public static float ArcaneBoltDamageFlat(IList<SkillPassiveModifier> modifiers, string spellId)
            => string.Equals(spellId, ArcaneBoltSpellId, StringComparison.Ordinal)
                ? Sum(modifiers, SkillModifierType.ArcaneBoltDamageFlat, ArcaneBoltMasteryNodeId)
                : 0f;

        public static float GuardedDefenseFlat(IList<SkillPassiveModifier> modifiers, EquipmentSnapshot equipment)
            => GuardedDefenseFlat(Sum(modifiers, SkillModifierType.GuardedDefenseFlat), equipment);

        public static float GuardedDefenseFlat(float bonus, EquipmentSnapshot equipment)
        {
            bool valid = IsValidMelee(equipment.MainHand, equipment.MainHandBroken)
                && equipment.OffHandIsShield
                && !equipment.OffHandBroken;
            return valid ? Math.Max(0f, bonus) : 0f;
        }

        public static float DualWieldRecoveryMultiplier(IList<SkillPassiveModifier> modifiers, EquipmentSnapshot equipment)
            => DualWieldRecoveryMultiplier(Sum(modifiers, SkillModifierType.DualWieldRecoverySpeed), equipment);

        public static float DualWieldRecoveryMultiplier(float recovery, EquipmentSnapshot equipment)
        {
            bool valid = IsValidMelee(equipment.MainHand, equipment.MainHandBroken)
                && IsValidMelee(equipment.OffHand, equipment.OffHandBroken)
                && equipment.MainHand.WeightClass == WeaponWeightClass.Light
                && equipment.OffHand.WeightClass == WeaponWeightClass.Light;
            recovery = valid ? Math.Max(0f, recovery) : 0f;
            return Math.Max(0.1f, 1f - recovery);
        }

        public static float TwoHandedDamageMultiplier(IList<SkillPassiveModifier> modifiers, EquipmentSnapshot equipment)
            => TwoHandedDamageMultiplier(Sum(modifiers, SkillModifierType.TwoHandedDamageBonus), equipment);

        public static float TwoHandedDamageMultiplier(float bonus, EquipmentSnapshot equipment)
        {
            bool valid = IsValidMelee(equipment.MainHand, equipment.MainHandBroken)
                && equipment.MainHand.RequiresTwoHands
                && equipment.OffHandEmpty;
            bonus = valid ? Math.Max(0f, bonus) : 0f;
            return 1f + Math.Max(0f, bonus);
        }

        public static float BowDamageFlat(IList<SkillPassiveModifier> modifiers, EquipmentSnapshot equipment)
            => BowFlat(Sum(modifiers, SkillModifierType.BowDamageFlat), equipment);

        public static float BowRangeFlat(IList<SkillPassiveModifier> modifiers, EquipmentSnapshot equipment)
            => BowFlat(Sum(modifiers, SkillModifierType.BowRangeFlat), equipment);

        public static float BowRecoveryMultiplier(IList<SkillPassiveModifier> modifiers, EquipmentSnapshot equipment)
            => BowRecoveryMultiplier(Sum(modifiers, SkillModifierType.BowRecoverySpeed), equipment);

        public static float BowRecoveryMultiplier(float recovery, EquipmentSnapshot equipment)
        {
            recovery = IsValidBow(equipment.MainHand, equipment.MainHandBroken) ? Math.Max(0f, recovery) : 0f;
            return Math.Max(0.1f, 1f - recovery);
        }

        public static float BowProjectileSpeedFlat(IList<SkillPassiveModifier> modifiers, EquipmentSnapshot equipment)
            => BowFlat(Sum(modifiers, SkillModifierType.BowProjectileSpeedFlat), equipment);

        public static float BowFlat(float bonus, EquipmentSnapshot equipment)
            => IsValidBow(equipment.MainHand, equipment.MainHandBroken) ? Math.Max(0f, bonus) : 0f;

        public static float KitingMoveSpeedBonus(IList<SkillPassiveModifier> modifiers)
            => Math.Max(0f, Sum(modifiers, SkillModifierType.KitingMoveSpeedBonus, KitingStepsNodeId));

        public static float DodgeCostMultiplier(
            IList<SkillPassiveModifier> modifiers,
            float retreatCostMultiplier = 1f)
            => DodgeCostModifierProvider.Resolve(modifiers, retreatCostMultiplier);

        public static bool IsEquipmentGatedDerivedModifier(SkillPassiveModifier modifier)
        {
            if (modifier == null) return false;
            return string.Equals(modifier.SourceNodeId, GuardedStanceNodeId, StringComparison.Ordinal)
                || string.Equals(modifier.SourceNodeId, DualWieldFlowNodeId, StringComparison.Ordinal)
                || string.Equals(modifier.SourceNodeId, TwoHandedMomentumNodeId, StringComparison.Ordinal)
                || string.Equals(modifier.SourceNodeId, KitingStepsNodeId, StringComparison.Ordinal);
        }

        private static float Sum(
            IList<SkillPassiveModifier> modifiers,
            SkillModifierType type,
            string sourceNodeId)
        {
            if (modifiers == null) return 0f;

            float total = 0f;
            for (int i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier != null
                    && modifier.ModifierType == type
                    && string.Equals(modifier.SourceNodeId, sourceNodeId, StringComparison.Ordinal))
                {
                    total += modifier.Value;
                }
            }

            return total;
        }

        private static float Sum(IList<SkillPassiveModifier> modifiers, SkillModifierType type)
        {
            if (modifiers == null) return 0f;
            float total = 0f;
            for (int i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier != null && modifier.ModifierType == type)
                    total += modifier.Value;
            }
            return total;
        }
    }
}
