using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Foundation;
using CindarsHope.Player;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Core
{
    /// <summary>
    /// F02 — pesos de ataque canônicos, crítico por chance/garantido e provider de stats.
    /// </summary>
    public class AttackChargeAndStatsTests
    {
        // ------------------------------------------------ thresholds

        [TestCase(0.1f, AttackWeight.Light)]
        [TestCase(0.24f, AttackWeight.Light)]
        [TestCase(0.25f, AttackWeight.Heavy)]
        [TestCase(0.89f, AttackWeight.Heavy)]
        [TestCase(0.9f, AttackWeight.ChargedShort)]
        [TestCase(1.49f, AttackWeight.ChargedShort)]
        [TestCase(1.5f, AttackWeight.ChargedLong)]
        public void ResolveWeight_CanonicalThresholds(float hold, AttackWeight expected)
        {
            Assert.AreEqual(expected, AttackChargeRules.ResolveWeight(hold));
        }

        [Test]
        public void Tracker_BeginRelease_Lifecycle()
        {
            var tracker = new AttackChargeTracker();
            Assert.IsFalse(tracker.IsCharging);

            tracker.Begin(10f);
            Assert.IsTrue(tracker.IsCharging);
            Assert.AreEqual(0.5f, tracker.HoldSeconds(10.5f), 0.001f);

            var weight = tracker.Release(11f);
            Assert.AreEqual(AttackWeight.ChargedShort, weight);
            Assert.IsFalse(tracker.IsCharging, "Release encerra a carga.");
        }

        // ------------------------------------------------ multiplicadores canônicos (emenda)

        [Test]
        public void Multipliers_AreCanonical()
        {
            Assert.AreEqual(1.00f, AttackChargeRules.DamageMultiplier(AttackWeight.Light), 0.001f);
            Assert.AreEqual(1.45f, AttackChargeRules.DamageMultiplier(AttackWeight.Heavy), 0.001f);
            Assert.AreEqual(1.65f, AttackChargeRules.DamageMultiplier(AttackWeight.ChargedShort), 0.001f);
            Assert.AreEqual(1.90f, AttackChargeRules.DamageMultiplier(AttackWeight.ChargedLong), 0.001f);

            Assert.AreEqual(1.00f, AttackChargeRules.PostureMultiplier(AttackWeight.Light), 0.001f);
            Assert.AreEqual(1.60f, AttackChargeRules.PostureMultiplier(AttackWeight.Heavy), 0.001f);
            Assert.AreEqual(1.80f, AttackChargeRules.PostureMultiplier(AttackWeight.ChargedShort), 0.001f);
            Assert.AreEqual(2.20f, AttackChargeRules.PostureMultiplier(AttackWeight.ChargedLong), 0.001f);
        }

        // ------------------------------------------------ provider (stats sintéticos)

        [Test]
        public void Provider_AttackBonus_IncreasesFinalDamage()
        {
            using var provider = new PlayerCombatStatsProvider(
                baseAttackSource: () => 0,
                passivesSource: () => new List<SkillPassiveModifier> { new SkillPassiveModifier(SkillModifierType.AttackFlat, 5f) },
                critRoll: () => 1f); // nunca crita

            var damage = provider.FinalDamage(10, AttackWeight.Light, false, out var isCrit);
            Assert.IsFalse(isCrit);
            Assert.AreEqual(15, damage, "base 10 + Attack 5.");
        }

        [Test]
        public void Provider_WeightMultiplier_Applied()
        {
            using var provider = new PlayerCombatStatsProvider(() => 0, () => null, critRoll: () => 1f);
            Assert.AreEqual(15, provider.FinalDamage(10, AttackWeight.Heavy, false, out _), "10 × 1.45 = 14.5 → 15.");
            Assert.AreEqual(19, provider.FinalDamage(10, AttackWeight.ChargedLong, false, out _), "10 × 1.90.");
        }

        [Test]
        public void Provider_CritByChance_And_GuaranteedByWindow()
        {
            using var critProvider = new PlayerCombatStatsProvider(() => 0, () => null, critRoll: () => 0f); // sempre crita
            var damage = critProvider.FinalDamage(10, AttackWeight.Light, false, out var isCrit);
            Assert.IsTrue(isCrit, "Chance normal de crítico (canon).");
            Assert.AreEqual(15, damage, "10 × 1.5.");

            using var noRollProvider = new PlayerCombatStatsProvider(() => 0, () => null, critRoll: () => 1f);
            noRollProvider.FinalDamage(10, AttackWeight.Light, guaranteedCrit: true, out var guaranteed);
            Assert.IsTrue(guaranteed, "Janela de vulnerabilidade GARANTE crítico (emenda).");
        }

        [Test]
        public void Provider_AttackSpeed_ReducesCooldown()
        {
            using var provider = new PlayerCombatStatsProvider(
                () => 0,
                () => new List<SkillPassiveModifier> { new SkillPassiveModifier(SkillModifierType.AttackSpeedBonus, 0.5f) },
                critRoll: () => 1f);

            Assert.AreEqual(1f / 1.5f, provider.FinalCooldown(1f), 0.001f, "AttackSpeed 1.5 → cooldown /1.5.");
        }

        // ------------------------------------------------ posture (tabela por dificuldade)

        [Test]
        public void Posture_TableScalesWithDifficulty()
        {
            Assert.Less(
                EnemyPostureState.MaxPostureFor(EnemyDifficulty.Easy),
                EnemyPostureState.MaxPostureFor(EnemyDifficulty.Boss));
            Assert.AreEqual(60f, EnemyPostureState.MaxPostureFor(EnemyDifficulty.Normal));
        }

        [Test]
        public void Posture_TwoChargedBreak_NormalEnemy()
        {
            // Normal = 60; charged longo em arma 15 de dano: 15 × 2.2 = 33/golpe → 2 golpes = 66 ≥ 60.
            var perHit = 15f * AttackChargeRules.PostureMultiplier(AttackWeight.ChargedLong);
            Assert.GreaterOrEqual(perHit * 2f, EnemyPostureState.MaxPostureFor(EnemyDifficulty.Normal));
            // Light não quebra nem com 3 golpes: 15 × 1.0 × 3 = 45 < 60.
            Assert.Less(15f * 3f, EnemyPostureState.MaxPostureFor(EnemyDifficulty.Normal));
        }
    }
}
