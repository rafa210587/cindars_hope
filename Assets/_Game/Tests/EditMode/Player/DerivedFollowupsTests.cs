using CindarsHope.Combat;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Foundation;
using CindarsHope.Player;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// fable_47 — fórmulas dos 3 follow-ups da F18: fator MoveSpeed derivado, redução de tempo
    /// de craft, eficiência de reparo e duração de status por resistência (cap 50%).
    /// </summary>
    public class DerivedFollowupsTests
    {
        // -------------------------------------------------- CA-2: fator MoveSpeed derivado

        [Test]
        public void DerivedMoveSpeedFactor_ConvertsBonusToMultiplier()
        {
            // base 5, bônus +0.5 → fator 1.1
            Assert.AreEqual(1.1f, DerivedFollowupFormulas.DerivedMoveSpeedFactor(0.5f, 5f), 0.0001f);
        }

        [Test]
        public void DerivedMoveSpeedFactor_NoBonus_IsNeutral()
        {
            Assert.AreEqual(1f, DerivedFollowupFormulas.DerivedMoveSpeedFactor(0f, 5f), 0.0001f);
        }

        [Test]
        public void DerivedMoveSpeedFactor_ZeroBase_IsNeutral()
        {
            Assert.AreEqual(1f, DerivedFollowupFormulas.DerivedMoveSpeedFactor(0.5f, 0f), 0.0001f, "Base inválida não quebra.");
        }

        [Test]
        public void DerivedMoveSpeedFactor_NegativeBonus_NeverNegative()
        {
            Assert.AreEqual(0f, DerivedFollowupFormulas.DerivedMoveSpeedFactor(-10f, 5f), 0.0001f);
        }

        // -------------------------------------------------- CA-3: craft time

        [Test]
        public void CraftTimeMultiplier_ReducesByReduction()
        {
            Assert.AreEqual(0.8f, DerivedFollowupFormulas.CraftTimeMultiplier(0.2f), 0.0001f, "20% mais rápido.");
        }

        [Test]
        public void CraftTimeMultiplier_NoReduction_IsOne()
        {
            Assert.AreEqual(1f, DerivedFollowupFormulas.CraftTimeMultiplier(0f), 0.0001f);
        }

        [Test]
        public void CraftTimeMultiplier_CappedAt75Percent()
        {
            Assert.AreEqual(0.25f, DerivedFollowupFormulas.CraftTimeMultiplier(0.95f), 0.0001f, "Nunca abaixo de 25% do tempo.");
        }

        [Test]
        public void EffectiveCraftSeconds_AppliesMultiplier()
        {
            Assert.AreEqual(8f, DerivedFollowupFormulas.EffectiveCraftSeconds(10f, 0.2f), 0.0001f, "10s × 0.8 = 8s.");
        }

        // -------------------------------------------------- CA-3: repair

        [Test]
        public void EffectiveRepairAmount_IncreasesWithBonus()
        {
            Assert.AreEqual(15, DerivedFollowupFormulas.EffectiveRepairAmount(10, 0.5f), "10 × 1.5 = 15.");
        }

        [Test]
        public void EffectiveRepairAmount_NoBonus_IsBase()
        {
            Assert.AreEqual(10, DerivedFollowupFormulas.EffectiveRepairAmount(10, 0f));
        }

        [Test]
        public void EffectiveRepairAmount_CappedAt200Percent()
        {
            Assert.AreEqual(30, DerivedFollowupFormulas.EffectiveRepairAmount(10, 5f), "Cap em +200% → 3× base.");
        }

        [Test]
        public void EffectiveRepairAmount_ZeroBase_StaysZero()
        {
            Assert.AreEqual(0, DerivedFollowupFormulas.EffectiveRepairAmount(0, 1f));
        }

        // -------------------------------------------------- CA-4: duração de status por resistência

        [Test]
        public void StatusDurationMultiplier_Resist0_NoReduction()
        {
            Assert.AreEqual(1f, DerivedFollowupFormulas.StatusDurationMultiplier(0), 0.0001f);
        }

        [Test]
        public void StatusDurationMultiplier_Resist10_Is0_8()
        {
            Assert.AreEqual(0.8f, DerivedFollowupFormulas.StatusDurationMultiplier(10), 0.0001f, "10 × 0.02 = 0.2 redução.");
        }

        [Test]
        public void StatusDurationMultiplier_Resist25_Is0_5()
        {
            Assert.AreEqual(0.5f, DerivedFollowupFormulas.StatusDurationMultiplier(25), 0.0001f, "25 × 0.02 = 0.5 (cap).");
        }

        [Test]
        public void StatusDurationMultiplier_Resist50_CapsAt0_5()
        {
            Assert.AreEqual(0.5f, DerivedFollowupFormulas.StatusDurationMultiplier(50), 0.0001f, "Cap 50% mantém ×0.5.");
        }

        [Test]
        public void ApplyStatusDurationReduction_PreservesClamp1To30()
        {
            // base 10s, resist 25 → 5s (dentro do clamp).
            Assert.AreEqual(5f, DerivedFollowupFormulas.ApplyStatusDurationReduction(10f, 25, 1f, 30f), 0.0001f);
            // base 2s, resist 50 → 1s (não cai abaixo do floor 1s).
            Assert.AreEqual(1f, DerivedFollowupFormulas.ApplyStatusDurationReduction(2f, 50, 1f, 30f), 0.0001f, "Floor 1s preservado.");
            // base 30s clampada no teto.
            Assert.AreEqual(15f, DerivedFollowupFormulas.ApplyStatusDurationReduction(30f, 25, 1f, 30f), 0.0001f);
        }

        // -------------------------------------------------- eixo de resistência (não duplica F18)

        [Test]
        public void ResistanceAxis_PoisonIsToxic()
        {
            Assert.AreEqual(DamageType.Toxic, PlayerStatusReceiver.ResistanceAxisFor(StatusEffectType.Poison));
        }

        [Test]
        public void ResistanceAxis_ChillIsIce()
        {
            Assert.AreEqual(DamageType.Ice, PlayerStatusReceiver.ResistanceAxisFor(StatusEffectType.Chill), "Chill mapeia ao eixo gelo (spec).");
        }

        [Test]
        public void ResistanceAxis_ColdStressIsIce()
        {
            Assert.AreEqual(DamageType.Ice, PlayerStatusReceiver.ResistanceAxisFor(StatusEffectType.ColdStress));
        }

        [Test]
        public void ResistanceAxis_BurnIsFire()
        {
            Assert.AreEqual(DamageType.Fire, PlayerStatusReceiver.ResistanceAxisFor(StatusEffectType.Burn));
        }
    }
}
