using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Player.Progression;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Core
{
    /// <summary>
    /// F03 — baselines por arma (ASPD/scaling/custos canônicos) e armadura funcional.
    /// </summary>
    public class WeaponBaselineAndArmorTests
    {
        private static WeaponDataSO MakeWeapon(System.Action<WeaponDataSO> setup = null)
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
            weapon.Id = "weapon_test";
            weapon.BaseDamage = 10;
            weapon.StaminaCost = 25f;
            setup?.Invoke(weapon);
            return weapon;
        }

        // ------------------------------------------------ defaults neutros

        [Test]
        public void NewFields_HaveNeutralDefaults()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
            Assert.AreEqual(0f, weapon.PrimaryAttributeWeight, "Sem scaling até o gerador rodar.");
            Assert.AreEqual(0f, weapon.BaseLightStaminaCost, "0 = fallback às razões F02.");
            Assert.AreEqual(1f, weapon.PostureDamageModifier);
            Assert.AreEqual(1f, weapon.AttackSpeedMultiplier);
        }

        // ------------------------------------------------ stamina por arma (emenda)

        [Test]
        public void StaminaCost_UsesCanonicalFieldsWhenAuthored()
        {
            var sword = MakeWeapon(w =>
            {
                w.BaseLightStaminaCost = 25f;
                w.BaseHeavyStaminaCost = 40f;
                w.BaseChargedStaminaCost = 48f;
            });

            Assert.AreEqual(25, PlayerCombatStatsProvider.WeaponStaminaCost(sword, AttackWeight.Light));
            Assert.AreEqual(40, PlayerCombatStatsProvider.WeaponStaminaCost(sword, AttackWeight.Heavy));
            Assert.AreEqual(48, PlayerCombatStatsProvider.WeaponStaminaCost(sword, AttackWeight.ChargedShort));
            Assert.AreEqual(48, PlayerCombatStatsProvider.WeaponStaminaCost(sword, AttackWeight.ChargedLong));
        }

        [Test]
        public void StaminaCost_FallsBackToRatiosWhenNotAuthored()
        {
            var legacy = MakeWeapon();
            Assert.AreEqual(25, PlayerCombatStatsProvider.WeaponStaminaCost(legacy, AttackWeight.Light));
            Assert.AreEqual(40, PlayerCombatStatsProvider.WeaponStaminaCost(legacy, AttackWeight.Heavy), "25 × 1.6.");
        }

        // ------------------------------------------------ ASPD e scaling

        [Test]
        public void Aspd_ReducesCooldown()
        {
            using var provider = new PlayerCombatStatsProvider(() => 0, () => null, critRoll: () => 1f);
            var fast = MakeWeapon(w => w.AttackSpeedMultiplier = 1.5f);
            Assert.AreEqual(1f / 1.5f, provider.FinalCooldown(1f, fast), 0.001f, "ASPD 1.5 → −33%.");
        }

        [Test]
        public void Scaling_AddsAttributeBonus()
        {
            using var provider = new PlayerCombatStatsProvider(() => 0, () => null, critRoll: () => 1f);
            provider.AttributeSource = type => type == PlayerAttributeType.Dexterity ? 10 : 0;

            var dagger = MakeWeapon(w =>
            {
                w.PrimaryAttribute = PlayerAttributeType.Dexterity;
                w.PrimaryAttributeWeight = 0.8f;
            });

            Assert.AreEqual(8, provider.WeaponScalingBonus(dagger), "Dex 10 × 0.8.");
            Assert.AreEqual(18, provider.FinalDamage(dagger, AttackWeight.Light, false, out _), "10 base + 8 scaling.");
        }

        // ------------------------------------------------ armadura

        [Test]
        public void ArmorReduction_FlatWithFloorOne()
        {
            Assert.AreEqual(7, PlayerDamageReceiver.CalculateReducedDamage(10, 3));
            Assert.AreEqual(1, PlayerDamageReceiver.CalculateReducedDamage(10, 15), "Floor mínimo 1.");
            Assert.AreEqual(0, PlayerDamageReceiver.CalculateReducedDamage(0, 5), "Sem dano = sem floor.");
            Assert.AreEqual(10, PlayerDamageReceiver.CalculateReducedDamage(10, 0), "Defense 0 = dano cru.");
        }
    }
}
