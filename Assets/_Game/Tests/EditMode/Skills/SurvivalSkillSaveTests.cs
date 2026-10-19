using CindarsHope.Cave.Runtime;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class SurvivalSkillSaveTests
    {
        [Test]
        public void Encounter_ReinforcementKeepsOldestIdentity_AndResolvesAfterLastEnemy()
        {
            var state = new SurvivalSkillState();

            Assert.That(state.BeginOrJoinEncounter("run-a", 3, "enemy-b"), Is.True);
            string oldestEncounterId = state.ActiveEncounterId;
            Assert.That(state.BeginOrJoinEncounter("run-a", 3, "enemy-a"), Is.False);
            Assert.That(state.ActiveEncounterId, Is.EqualTo(oldestEncounterId));
            Assert.That(state.ActiveEnemyInstanceIds.Count, Is.EqualTo(2));

            Assert.That(state.RemoveEnemyAndResolveIfEmpty("enemy-a", out _), Is.False);
            Assert.That(state.RemoveEnemyAndResolveIfEmpty("enemy-b", out string resolvedId), Is.True);
            Assert.That(resolvedId, Is.EqualTo(oldestEncounterId));
            Assert.That(state.HasActiveEncounter, Is.False);

            Assert.That(state.BeginOrJoinEncounter("run-b", 3, "enemy-c"), Is.True);
            Assert.That(state.ActiveEncounterOrdinal, Is.EqualTo(1),
                "Encounter ordinals restart under a new opaque run id.");
        }

        [Test]
        public void LastBreath_ConsumptionIsPersistedAndIdempotent()
        {
            var state = new SurvivalSkillState();
            state.BeginOrJoinEncounter("run-a", 2, "enemy-a");
            state.TryArmLastBreath(5f);

            Assert.That(state.TryConsumeLastBreathForActiveEncounter(), Is.True);
            Assert.That(state.TryConsumeLastBreathForActiveEncounter(), Is.False);

            var restored = new SurvivalSkillState();
            restored.BeginRestore();
            restored.RestoreFromSaveData(state.CaptureSaveData(), "run-a", 2);
            restored.EndRestore();

            Assert.That(restored.IsLastBreathConsumed(state.ActiveEncounterId), Is.True);
            Assert.That(restored.LastBreathArmedRemainingSeconds, Is.Zero);
            Assert.That(restored.TryConsumeLastBreathForActiveEncounter(), Is.False);
        }

        [Test]
        public void Restore_SameRunAndLevelPreservesTimers_AndCancelsPreCommitChannel()
        {
            var original = new SurvivalSkillState();
            original.BeginOrJoinEncounter("run-a", 4, "enemy-a");
            original.TryArmLastBreath(3.5f);
            original.TryMarkCampUsed("run-a");
            original.ActivateCamp("run-a", 4, 11f, 6.5f, -2f);
            var data = original.CaptureSaveData();

            var restored = new SurvivalSkillState();
            restored.BeginTransientChannel("skill_survival_field_kit");
            restored.BeginRestore();
            restored.RestoreFromSaveData(data, "run-a", 4);

            Assert.That(restored.IsRestoreInProgress, Is.True);
            Assert.That(restored.PendingChannelActionId, Is.Empty);
            Assert.That(restored.ActiveEncounterId, Is.EqualTo(original.ActiveEncounterId));
            Assert.That(restored.LastBreathArmedRemainingSeconds, Is.EqualTo(3.5f));
            Assert.That(restored.ActiveCampRemainingSeconds, Is.EqualTo(11f));
            Assert.That(restored.ActiveCampPositionX, Is.EqualTo(6.5f));
            Assert.That(restored.ActiveCampPositionY, Is.EqualTo(-2f));

            restored.EndRestore();
            Assert.That(restored.IsRestoreInProgress, Is.False);
        }

        [Test]
        public void Restore_DifferentContextDropsActiveEffects_ButKeepsIdempotencyFlags()
        {
            var original = new SurvivalSkillState();
            original.BeginOrJoinEncounter("run-a", 4, "enemy-a");
            original.TryArmLastBreath(5f);
            original.TryConsumeLastBreathForActiveEncounter();
            original.TryMarkCampUsed("run-a");
            original.ActivateCamp("run-a", 4, 11f, 1f, 2f);
            string consumedEncounterId = original.ActiveEncounterId;

            var restored = new SurvivalSkillState();
            restored.RestoreFromSaveData(original.CaptureSaveData(), "run-b", 4);

            Assert.That(restored.HasActiveEncounter, Is.False);
            Assert.That(restored.LastBreathArmedRemainingSeconds, Is.Zero);
            Assert.That(restored.ActiveCampRemainingSeconds, Is.Zero);
            Assert.That(restored.IsLastBreathConsumed(consumedEncounterId), Is.True);
            Assert.That(restored.CampUsedRunId, Is.EqualTo("run-a"));
            Assert.That(restored.TryMarkCampUsed("run-b"), Is.True);
        }

        [Test]
        public void Provider_RoundTripAndLegacyFallbackPreserveRealState()
        {
            var source = new SurvivalSkillState();
            source.BeginOrJoinEncounter("run-a", 5, "enemy-a");
            source.TryArmLastBreath(4f);
            var provider = new SurvivalSkillSectionProvider(source);

            var target = new SurvivalSkillState();
            var targetProvider = new SurvivalSkillSectionProvider(target);
            target.BeginRestore();
            targetProvider.RestoreForContext(provider.Capture(null), "run-a", 5);
            target.EndRestore();

            Assert.That(target.ActiveEncounterId, Is.EqualTo(source.ActiveEncounterId));
            Assert.That(target.LastBreathArmedRemainingSeconds, Is.EqualTo(4f));

            var legacyFallback = new SurvivalSkillSaveData { CampUsedRunId = "run-legacy" };
            var existing = new GameSaveData { SurvivalSkills = legacyFallback };
            var missingRuntimeProvider = new SurvivalSkillSectionProvider(null);
            Assert.That(missingRuntimeProvider.Capture(existing), Is.SameAs(legacyFallback));
            Assert.DoesNotThrow(() => targetProvider.Restore(null));
            Assert.DoesNotThrow(() => targetProvider.Restore(new object()));
        }

        [Test]
        public void CaveRunMapper_RunIdIsIndependentFromSeed_AndInactiveStateKeepsReplaySeed()
        {
            var active = new CaveRuntimeState
            {
                HasActiveRun = true,
                CaveWorldSeed = "world",
                CaveRunSeed = "shared-seed",
                CaveRunId = "opaque-run-a",
                CurrentCaveLevel = 3,
                DeepestLayerReached = 3
            };

            var activeData = CaveRunSaveMapper.ToSaveData(active);
            Assert.That(activeData.RunSeed, Is.EqualTo("shared-seed"));
            Assert.That(activeData.RunId, Is.EqualTo("opaque-run-a"));
            Assert.That(CaveRunSaveMapper.FromSaveData(activeData).CaveRunId,
                Is.EqualTo("opaque-run-a"));

            active.HasActiveRun = false;
            active.CaveRunId = string.Empty;
            var inactiveData = CaveRunSaveMapper.ToSaveData(active);
            var inactiveRestored = CaveRunSaveMapper.FromSaveData(inactiveData);
            Assert.That(inactiveData.HasActiveRun, Is.False);
            Assert.That(inactiveData.RunSeed, Is.EqualTo("shared-seed"));
            Assert.That(inactiveRestored, Is.Not.Null);
            Assert.That(inactiveRestored.HasActiveRun, Is.False);
            Assert.That(inactiveRestored.CaveRunSeed, Is.EqualTo("shared-seed"));
            Assert.That(inactiveRestored.CaveRunId, Is.Empty);
        }
    }
}
