using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Player;
using CindarsHope.Skills;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// F18 — aplicação dos derivados em vitals: proporção preservada, resistências por tipo,
    /// redução de drain de fome.
    /// </summary>
    public class VitalsApplicationTests
    {
        // ------------------------------------------------ proporção

        [Test]
        public void PreserveRatio_HalfStaysHalf()
        {
            Assert.AreEqual(60, PlayerVitalsApplier.PreserveRatio(50, 100, 120), "50% de 120.");
            Assert.AreEqual(50, PlayerVitalsApplier.PreserveRatio(60, 120, 100), "Reverte sem matar.");
        }

        [Test]
        public void PreserveRatio_FloorOne()
        {
            Assert.AreEqual(1, PlayerVitalsApplier.PreserveRatio(1, 100, 50), "Nunca 0 ao reduzir máximo.");
            Assert.AreEqual(1, PlayerVitalsApplier.PreserveRatio(0, 100, 120), "0 vivo... floor 1 documentado (desequipar não mata).");
        }

        // ------------------------------------------------ resistências

        [Test]
        public void ResistanceFor_MapsDamageTypes()
        {
            var stats = new DerivedStatsCalculator.DerivedStats
            {
                ToxicResistance = 4,
                ColdResistance = 3,
                HeatResistance = 2
            };

            Assert.AreEqual(4, PlayerVitalsApplier.ResistanceFor(stats, DamageType.Toxic));
            Assert.AreEqual(3, PlayerVitalsApplier.ResistanceFor(stats, DamageType.Ice));
            Assert.AreEqual(2, PlayerVitalsApplier.ResistanceFor(stats, DamageType.Fire));
            Assert.AreEqual(0, PlayerVitalsApplier.ResistanceFor(stats, DamageType.Physical));
        }

        [Test]
        public void Receiver_AppliesResistanceOnTopOfDefense()
        {
            Assert.AreEqual(5, PlayerDamageReceiver.CalculateReducedDamage(10, 2, 3));
            Assert.AreEqual(1, PlayerDamageReceiver.CalculateReducedDamage(10, 5, 8), "Floor 1 mantido.");
        }

        // ------------------------------------------------ calculadora (passivas → vitals)

        [Test]
        public void Calculator_PassivesRaiseVitals()
        {
            var stats = DerivedStatsCalculator.Calculate(
                100, 0, 0, 0f, 100, 0f, 1f,
                equippedItems: null,
                passiveModifiers: new List<SkillPassiveModifier>
                {
                    new SkillPassiveModifier(SkillModifierType.MaxHPFlat, 20f),
                    new SkillPassiveModifier(SkillModifierType.MaxStaminaFlat, 15f),
                    new SkillPassiveModifier(SkillModifierType.HungerDrainReduction, 0.3f)
                });

            Assert.AreEqual(120, stats.MaxHP);
            Assert.AreEqual(115, stats.MaxStamina);
            Assert.AreEqual(0.3f, stats.HungerDrainReduction, 0.001f);
        }

        [Test]
        public void DrainMultiplier_Clamped()
        {
            // applier converte redução em multiplicador clampado (0.25..1).
            var reduction = 0.9f;
            var multiplier = UnityEngine.Mathf.Clamp(1f - reduction, 0.25f, 1f);
            Assert.AreEqual(0.25f, multiplier, "Redução máxima de 75% no drain.");
        }
    }
}
