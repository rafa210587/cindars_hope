using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Random;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime;
using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// F02 — provider único de stats derivados para o combate. Consome o
    /// DerivedStatsCalculator (WAVE 05, antes órfão) com cache invalidado por eventos.
    /// Crítico canônico (emenda): chance normal de crítico (×1.5); janela de
    /// vulnerabilidade GARANTE crítico (decidido no call site que conhece o alvo).
    /// </summary>
    public class PlayerCombatStatsProvider : IDisposable
    {
        public const float CritMultiplier = 1.5f;
        public const float BaseCritChance = 0.05f;

        private readonly Func<int> _baseAttackSource;
        private readonly Func<List<SkillPassiveModifier>> _passivesSource;
        private readonly Func<Dictionary<EquipmentSlot, EquipmentDataSO>> _equipmentSource;
        private readonly Func<float> _critRoll;
        private Func<EquipmentSlot, Weapon.WeaponDataSO> _weaponSource;
        private Func<EquipmentSlot, bool> _brokenSource;
        private Func<EquipmentSlot, bool> _shieldSource;
        private Func<EquipmentSlot, bool> _emptySource;

        private DerivedStatsCalculator.DerivedStats _cached;
        private bool _dirty = true;
        private bool _subscribed;

        public PlayerCombatStatsProvider(
            Func<int> baseAttackSource,
            Func<List<SkillPassiveModifier>> passivesSource,
            Func<Dictionary<EquipmentSlot, EquipmentDataSO>> equipmentSource = null,
            Func<float> critRoll = null)
        {
            _baseAttackSource = baseAttackSource ?? (() => 0);
            _passivesSource = passivesSource ?? (() => null);
            _equipmentSource = equipmentSource ?? (() => null);
            _critRoll = critRoll ?? UnityGameplayRandomSource.Shared.NextFloat;

            GameEventBus.Subscribe<EquipmentSlotChangedEvent>(OnInvalidatingEvent);
            GameEventBus.Subscribe<SkillDerivedStatsChangedEvent>(OnInvalidatingEvent);
            _subscribed = true;
        }

        public void ConfigureCombatEquipment(
            Func<EquipmentSlot, Weapon.WeaponDataSO> weaponSource,
            Func<EquipmentSlot, bool> brokenSource,
            Func<EquipmentSlot, bool> shieldSource,
            Func<EquipmentSlot, bool> emptySource)
        {
            _weaponSource = weaponSource;
            _brokenSource = brokenSource;
            _shieldSource = shieldSource;
            _emptySource = emptySource;
            Invalidate();
        }

        public DerivedStatsCalculator.DerivedStats Current
        {
            get
            {
                if (_dirty || _cached == null)
                {
                    Recalculate();
                }

                return _cached;
            }
        }

        public float CritChance => BaseCritChance;

        public void Invalidate()
        {
            _dirty = true;
        }

        /// <summary>
        /// Dano final: (base + Attack derivado) × peso × (crítico ×1.5).
        /// guaranteedCrit = janela de vulnerabilidade/CoreExposed aberta no alvo (canon).
        /// </summary>
        public int FinalDamage(int baseDamage, AttackWeight weight, bool guaranteedCrit, out bool isCrit)
        {
            isCrit = guaranteedCrit || _critRoll() < CritChance;
            var withAttack = baseDamage + Mathf.Max(0, Current.Attack);
            var withWeight = withAttack * AttackChargeRules.DamageMultiplier(weight);
            var final = isCrit ? withWeight * CritMultiplier : withWeight;
            return Mathf.Max(1, (int)System.Math.Round(final, System.MidpointRounding.AwayFromZero));
        }

        /// <summary>Cooldown final: base / AttackSpeed (floor 0.5×, cap 3×).</summary>
        public float FinalCooldown(float baseCooldown)
        {
            var speed = Mathf.Clamp(Current.AttackSpeed, 0.5f, 3f);
            return baseCooldown / speed;
        }

        // ─── F03: overloads cientes da arma (ASPD + scaling por atributo) ───

        /// <summary>Fonte de valores de atributo (injetada pelo controller; F03).</summary>
        // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) —
        // PlayerAttributeType agora vive em CindarsHope.Foundation (already imported).
        public Func<PlayerAttributeType, int> AttributeSource { get; set; }

        /// <summary>Bônus de scaling da arma: peso primário/secundário × atributo (F03).</summary>
        public int WeaponScalingBonus(Weapon.WeaponDataSO weapon)
        {
            if (weapon == null || AttributeSource == null)
            {
                return 0;
            }

            var bonus = 0f;
            if (weapon.PrimaryAttributeWeight > 0f)
            {
                bonus += AttributeSource(weapon.PrimaryAttribute) * weapon.PrimaryAttributeWeight;
            }

            if (weapon.SecondaryAttributeWeight > 0f)
            {
                bonus += AttributeSource(weapon.SecondaryAttribute) * weapon.SecondaryAttributeWeight;
            }

            return Mathf.RoundToInt(bonus);
        }

        public int FinalDamage(Weapon.WeaponDataSO weapon, AttackWeight weight, bool guaranteedCrit, out bool isCrit)
        {
            var baseDamage = (weapon != null ? weapon.BaseDamage : 0) + WeaponScalingBonus(weapon);
            return FinalDamage(baseDamage, weight, guaranteedCrit, out isCrit);
        }

        public int FinalMeleeDamage(
            Weapon.WeaponDataSO weapon,
            EquipmentSlot slot,
            AttackWeight weight,
            bool guaranteedCrit,
            out bool isCrit)
        {
            var equipment = Snapshot(slot);
            float attack = Mathf.Max(0, Current.Attack)
                + PassiveCombatModifierAdapter.MeleeAttackFlat(Current.MeleeAttackBonus, equipment);
            float baseDamage = (weapon != null ? weapon.BaseDamage : 0) + WeaponScalingBonus(weapon) + attack;
            float weighted = baseDamage * AttackChargeRules.DamageMultiplier(weight);
            weighted *= PassiveCombatModifierAdapter.TwoHandedDamageMultiplier(Current.TwoHandedDamageBonus, equipment);
            weighted *= CombatCapstoneModifierProvider.ResolveDirectMeleeDamageMultiplier();
            return ApplyCritical(weighted, guaranteedCrit, out isCrit,
                CombatCapstoneModifierProvider.ResolveMeleeCriticalDamageBonus());
        }

        public int FinalBowDamage(
            Weapon.WeaponDataSO weapon,
            int arrowDamage,
            EquipmentSlot bowSlot,
            bool guaranteedCrit,
            out bool isCrit)
            => FinalBowDamage(weapon, arrowDamage, bowSlot, guaranteedCrit,
                0f, 0f, out isCrit);

        public int FinalBowDamage(
            Weapon.WeaponDataSO weapon,
            int arrowDamage,
            EquipmentSlot bowSlot,
            bool guaranteedCrit,
            float criticalChanceBonus,
            float criticalDamageBonus,
            out bool isCrit)
        {
            var equipment = Snapshot(bowSlot);
            float damage = (weapon != null ? weapon.BaseDamage : 0)
                + WeaponScalingBonus(weapon)
                + arrowDamage
                + Mathf.Max(0, Current.Attack)
                + PassiveCombatModifierAdapter.BowFlat(Current.BowDamageBonus, equipment);
            return ApplyCritical(damage, guaranteedCrit, out isCrit,
                criticalDamageBonus, criticalChanceBonus);
        }

        public int FinalSpellDamage(int baseDamage, string spellId, bool guaranteedCrit, out bool isCrit)
            => FinalSpellDamage(baseDamage, spellId, guaranteedCrit, 1f, 0f, out isCrit);

        public int FinalSpellDamage(int baseDamage, string spellId, bool guaranteedCrit,
            float damageMultiplier, float criticalChanceBonus, out bool isCrit)
        {
            float damage = (baseDamage + Current.MagicAttackBonus
                + (string.Equals(spellId, PassiveCombatModifierAdapter.ArcaneBoltSpellId, StringComparison.Ordinal)
                    ? Current.ArcaneBoltDamageBonus
                    : 0f)) * Mathf.Max(0f, damageMultiplier);
            return ApplyCritical(damage, guaranteedCrit, out isCrit, 0f, criticalChanceBonus);
        }

        public float FinalCooldown(float baseCooldown, Weapon.WeaponDataSO weapon)
        {
            var aspd = weapon != null ? Mathf.Clamp(weapon.AttackSpeedMultiplier, 0.25f, 3f) : 1f;
            return FinalCooldown(baseCooldown) / aspd;
        }

        public float FinalMeleeRecovery(float baseRecovery, EquipmentSlot slot)
        {
            float attackSpeed = Mathf.Clamp(Current.AttackSpeed, 0.5f, 3f);
            return baseRecovery / attackSpeed
                * PassiveCombatModifierAdapter.DualWieldRecoveryMultiplier(Current.DualWieldRecoverySpeed, Snapshot(slot));
        }

        public float FinalBowRecovery(float baseRecovery, EquipmentSlot bowSlot)
        {
            float attackSpeed = Mathf.Clamp(Current.AttackSpeed, 0.5f, 3f);
            return baseRecovery / attackSpeed
                * PassiveCombatModifierAdapter.BowRecoveryMultiplier(Current.BowRecoverySpeed, Snapshot(bowSlot));
        }

        public float FinalBowRange(float baseRange, EquipmentSlot bowSlot)
            => baseRange + Mathf.Max(0f,
                PassiveCombatModifierAdapter.BowFlat(Current.BowRange, Snapshot(bowSlot)));

        public float FinalBowProjectileSpeed(float baseSpeed, EquipmentSlot bowSlot)
            => Mathf.Max(0.1f, baseSpeed + PassiveCombatModifierAdapter.BowFlat(
                Current.BowProjectileSpeed, Snapshot(bowSlot)));

        public float KitingMoveSpeedBonus
            => Mathf.Max(0f, Current.KitingMoveSpeedBonus);

        /// <summary>Custo de stamina por arma e peso: campos canônicos se autorados, senão razões F02.</summary>
        public static int WeaponStaminaCost(Weapon.WeaponDataSO weapon, AttackWeight weight)
        {
            if (weapon == null)
            {
                return 0;
            }

            float canonical;
            switch (weight)
            {
                case AttackWeight.Heavy:
                    canonical = weapon.BaseHeavyStaminaCost;
                    break;
                case AttackWeight.ChargedShort:
                case AttackWeight.ChargedLong:
                    canonical = weapon.BaseChargedStaminaCost;
                    break;
                default:
                    canonical = weapon.BaseLightStaminaCost;
                    break;
            }

            if (canonical > 0f)
            {
                return Mathf.RoundToInt(canonical);
            }

            return Mathf.RoundToInt(weapon.StaminaCost * AttackChargeRules.StaminaMultiplier(weight));
        }

        public void Dispose()
        {
            if (_subscribed)
            {
                GameEventBus.Unsubscribe<EquipmentSlotChangedEvent>(OnInvalidatingEvent);
                GameEventBus.Unsubscribe<SkillDerivedStatsChangedEvent>(OnInvalidatingEvent);
                _subscribed = false;
            }
        }

        private void OnInvalidatingEvent(EquipmentSlotChangedEvent evt)
        {
            _dirty = true;
        }

        private void OnInvalidatingEvent(SkillDerivedStatsChangedEvent evt)
        {
            _dirty = true;
        }

        private void Recalculate()
        {
            // Bases neutras: o provider mede BÔNUS sobre o dano base da arma.
            _cached = DerivedStatsCalculator.Calculate(
                baseMaxHP: 0,
                baseAttack: _baseAttackSource(),
                baseDefense: 0,
                baseMoveSpeed: 0f,
                baseMaxStamina: 0,
                baseStaminaRegen: 0f,
                baseAttackSpeed: 1f,
                equippedItems: _equipmentSource(),
                passiveModifiers: Passives);

            _cached.Defense += Mathf.RoundToInt(Mathf.Max(
                PassiveCombatModifierAdapter.GuardedDefenseFlat(_cached.GuardedDefenseBonus, Snapshot(EquipmentSlot.LeftHand)),
                PassiveCombatModifierAdapter.GuardedDefenseFlat(_cached.GuardedDefenseBonus, Snapshot(EquipmentSlot.RightHand))));
            _dirty = false;
        }

        private List<SkillPassiveModifier> Passives => _passivesSource() ?? s_emptyPassives;

        private PassiveCombatModifierAdapter.EquipmentSnapshot Snapshot(EquipmentSlot mainSlot)
        {
            EquipmentSlot offSlot = mainSlot == EquipmentSlot.LeftHand
                ? EquipmentSlot.RightHand
                : EquipmentSlot.LeftHand;
            return new PassiveCombatModifierAdapter.EquipmentSnapshot(
                _weaponSource?.Invoke(mainSlot),
                _weaponSource?.Invoke(offSlot),
                _brokenSource?.Invoke(mainSlot) ?? false,
                _brokenSource?.Invoke(offSlot) ?? false,
                _shieldSource?.Invoke(offSlot) ?? false,
                _emptySource?.Invoke(offSlot) ?? true);
        }

        private int ApplyCritical(float damage, bool guaranteedCrit, out bool isCrit,
            float criticalDamageBonus = 0f, float criticalChanceBonus = 0f)
        {
            isCrit = guaranteedCrit || _critRoll() <
                Mathf.Clamp01(CritChance + Mathf.Max(0f, criticalChanceBonus));
            float final = isCrit ? damage * (CritMultiplier + Mathf.Max(0f, criticalDamageBonus)) : damage;
            return Mathf.Max(1, (int)Math.Round(final, MidpointRounding.AwayFromZero));
        }

        private static readonly List<SkillPassiveModifier> s_emptyPassives =
            new List<SkillPassiveModifier>(0);
    }
}
