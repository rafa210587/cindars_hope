using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Random;
using CindarsHope.Equipment;
using CindarsHope.Player;
using CindarsHope.Skills;
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
            _subscribed = true;
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
            return Mathf.Max(1, Mathf.RoundToInt(final));
        }

        /// <summary>Cooldown final: base / AttackSpeed (floor 0.5×, cap 3×).</summary>
        public float FinalCooldown(float baseCooldown)
        {
            var speed = Mathf.Clamp(Current.AttackSpeed, 0.5f, 3f);
            return baseCooldown / speed;
        }

        // ─── F03: overloads cientes da arma (ASPD + scaling por atributo) ───

        /// <summary>Fonte de valores de atributo (injetada pelo controller; F03).</summary>
        public Func<Player.Progression.PlayerAttributeType, int> AttributeSource { get; set; }

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

        public float FinalCooldown(float baseCooldown, Weapon.WeaponDataSO weapon)
        {
            var aspd = weapon != null ? Mathf.Clamp(weapon.AttackSpeedMultiplier, 0.25f, 3f) : 1f;
            return FinalCooldown(baseCooldown) / aspd;
        }

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
                _subscribed = false;
            }
        }

        private void OnInvalidatingEvent(EquipmentSlotChangedEvent evt)
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
                passiveModifiers: _passivesSource());
            _dirty = false;
        }
    }
}
