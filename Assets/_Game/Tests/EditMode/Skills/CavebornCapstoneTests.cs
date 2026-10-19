using CindarsHope.Combat.StatusEffect;
using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class CavebornCapstoneTests
    {
        [TestCase(1, 8f, .70f, .20f)]
        [TestCase(2, 9f, .62f, .25f)]
        [TestCase(3, 10f, .55f, .30f)]
        public void RankTables_MatchApprovedValues(int rank, float duration,
            float costMultiplier, float resistance)
        {
            Assert.That(CavebornCapstoneResolver.DurationSeconds(rank), Is.EqualTo(duration));
            Assert.That(CavebornCapstoneResolver.CostMultiplier(rank), Is.EqualTo(costMultiplier).Within(.0001f));
            Assert.That(CavebornCapstoneResolver.ResistanceFraction(rank), Is.EqualTo(resistance).Within(.0001f));
        }

        [Test]
        public void Activation_UsesStrictThresholds_AndAnyEligibleResource()
        {
            Assert.That(CavebornCapstoneResolver.ShouldActivate(1, "run-a", false,
                24, 100, 100, 100, false), Is.True);
            Assert.That(CavebornCapstoneResolver.ShouldActivate(1, "run-a", false,
                100, 100, 14, 100, false), Is.True);
            Assert.That(CavebornCapstoneResolver.ShouldActivate(1, "run-a", false,
                100, 100, 100, 100, true), Is.True);
            Assert.That(CavebornCapstoneResolver.ShouldActivate(1, "run-a", false,
                25, 100, 15, 100, false), Is.False,
                "The contract is HP<25% or STA<15%, so equality does not trigger.");
            Assert.That(CavebornCapstoneResolver.ShouldActivate(1, "run-a", true,
                1, 100, 1, 100, true), Is.False);
            Assert.That(CavebornCapstoneResolver.ShouldActivate(1, string.Empty, false,
                1, 100, 1, 100, true), Is.False);
        }

        [Test]
        public void State_ConsumesOncePerOpaqueRun_AndNewRunRearms()
        {
            var state = new SurvivalSkillState();
            Assert.That(state.TryActivateCaveborn("opaque-run-a", 2, true), Is.True);
            Assert.That(state.ActiveCavebornRemainingSeconds, Is.EqualTo(9f));
            state.ClearActiveCaveborn();

            Assert.That(state.TryActivateCaveborn("opaque-run-a", 2, true), Is.False);
            Assert.That(state.TryActivateCaveborn("opaque-run-b", 2, true), Is.True);
            Assert.That(state.CavebornConsumedRunId, Is.EqualTo("opaque-run-b"));
        }

        [Test]
        public void R3_RegenDoublesOnlyAfterCombatExit_AndEndsWithBuff()
        {
            var state = new SurvivalSkillState();
            Assert.That(state.TryActivateCaveborn("run-a", 3, false), Is.True);
            Assert.That(state.CavebornRegenDoubled, Is.False);
            state.ObserveCavebornCombatState(true);
            Assert.That(state.CavebornRegenDoubled, Is.False);
            Assert.That(state.ObserveCavebornCombatState(false), Is.True);
            Assert.That(state.CavebornRegenDoubled, Is.True);

            state.AdvanceTime(10f);
            Assert.That(state.IsCavebornActive, Is.False);
            Assert.That(state.CavebornRegenDoubled, Is.False);
        }

        [Test]
        public void DamageAndStatusResistance_OnlyAffectApprovedFamilies()
        {
            Assert.That(CavebornCapstoneResolver.ResolveIncomingDamage(10, DamageType.Ice, 1), Is.EqualTo(8));
            Assert.That(CavebornCapstoneResolver.ResolveIncomingDamage(10, DamageType.Fire, 2), Is.EqualTo(8));
            Assert.That(CavebornCapstoneResolver.ResolveIncomingDamage(10, DamageType.Toxic, 3), Is.EqualTo(7));
            Assert.That(CavebornCapstoneResolver.ResolveIncomingDamage(10, DamageType.Physical, 3), Is.EqualTo(10));
            Assert.That(CavebornCapstoneResolver.ResolveIncomingDamage(10, DamageType.Lightning, 3), Is.EqualTo(10));
            Assert.That(CavebornCapstoneResolver.ResolveIncomingDamage(10, DamageType.Arcane, 3), Is.EqualTo(10));

            Assert.That(CavebornCapstoneResolver.ResolveStatusDuration(10f, CavebornStatusFamily.Fear, 3), Is.EqualTo(7f).Within(.001f));
            Assert.That(CavebornCapstoneResolver.ResolveStatusDuration(10f, CavebornStatusFamily.Confusion, 2), Is.EqualTo(7.5f).Within(.001f));
            Assert.That(PlayerStatusReceiver.CavebornFamilyFor(StatusEffectType.Poison), Is.EqualTo(CavebornStatusFamily.Toxic));
            Assert.That(PlayerStatusReceiver.CavebornFamilyFor(StatusEffectType.Burn), Is.EqualTo(CavebornStatusFamily.Other));
            Assert.That(PlayerStatusReceiver.CavebornFamilyFor(StatusEffectType.Stun), Is.EqualTo(CavebornStatusFamily.Other));
            Assert.That(PlayerStatusReceiver.CavebornFamilyFor(StatusEffectType.Root), Is.EqualTo(CavebornStatusFamily.Other));
            Assert.That(PlayerStatusReceiver.CavebornFamilyFor(StatusEffectType.Corruption), Is.EqualTo(CavebornStatusFamily.Other));
        }

        [Test]
        public void Save_RestoresSameRunWithoutRearming_AndDropsForeignTransientBuff()
        {
            var source = new SurvivalSkillState();
            source.TryActivateCaveborn("run-a", 3, true);
            source.AdvanceTime(2f);
            source.ObserveCavebornCombatState(false);
            var data = source.CaptureSaveData();

            var sameRun = new SurvivalSkillState();
            sameRun.BeginRestore();
            sameRun.RestoreFromSaveData(data, "run-a", 7);
            sameRun.EndRestore();
            Assert.That(sameRun.IsCavebornActive, Is.True);
            Assert.That(sameRun.ActiveCavebornRemainingSeconds, Is.EqualTo(8f));
            Assert.That(sameRun.CavebornRegenDoubled, Is.True);
            Assert.That(sameRun.TryActivateCaveborn("run-a", 3, false), Is.False);

            var otherRun = new SurvivalSkillState();
            otherRun.RestoreFromSaveData(data, "run-b", 7);
            Assert.That(otherRun.IsCavebornActive, Is.False);
            Assert.That(otherRun.CavebornConsumedRunId, Is.EqualTo("run-a"));
            Assert.That(otherRun.TryActivateCaveborn("run-b", 3, false), Is.True);
        }

        [Test]
        public void Respec_ClearsBuffButPreservesRunConsumption_AndLastBreathStateIsIndependent()
        {
            var state = new SurvivalSkillState();
            state.BeginOrJoinEncounter("run-a", 2, "enemy-a");
            Assert.That(state.TryActivateCaveborn("run-a", 1, true), Is.True);
            Assert.That(state.TryArmLastBreath(5f), Is.True,
                "Caveborn resolves first but does not consume the separate Last Breath window.");

            state.OnCavebornRespec();
            Assert.That(state.IsCavebornActive, Is.False);
            Assert.That(state.CavebornConsumedRunId, Is.EqualTo("run-a"));
            Assert.That(state.TryActivateCaveborn("run-a", 1, false), Is.False);
        }
    }
}
