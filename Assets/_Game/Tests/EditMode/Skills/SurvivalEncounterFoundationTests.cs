using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class SurvivalEncounterFoundationTests
    {
        [TearDown]
        public void TearDown()
        {
            GameEventBus.Clear<SurvivalEncounterStartedEvent>();
        }

        [Test]
        public void SceneScope_FirstAggroStartsAndOnlyNewEnemiesJoin()
        {
            var state = new SurvivalSkillState();
            const string scope = "scene:Assets/_Game/Scenes/Town.unity";

            Assert.That(state.RegisterEnemyAggro(scope, 0, "enemy-first"),
                Is.EqualTo(SurvivalSkillState.EncounterMembershipChange.Started));
            Assert.That(state.ActiveEncounterId,
                Is.EqualTo(scope + "|level:0|encounter:1"));
            Assert.That(state.RegisterEnemyAggro(scope, 0, "enemy-first"),
                Is.EqualTo(SurvivalSkillState.EncounterMembershipChange.AlreadyMember));
            Assert.That(state.RegisterEnemyAggro(scope, 0, "enemy-reinforcement"),
                Is.EqualTo(SurvivalSkillState.EncounterMembershipChange.Joined));
            Assert.That(state.ActiveEnemyInstanceIds.Count, Is.EqualTo(2));
        }

        [Test]
        public void QuietLeash_RequiresEveryLiveEnemyAndResolvesAtEightSeconds()
        {
            var state = new SurvivalSkillState();
            state.RegisterEnemyAggro("run-a", 2, "enemy-a");
            state.RegisterEnemyAggro("run-a", 2, "enemy-b");
            string encounterId = state.ActiveEncounterId;

            Assert.That(state.MarkEnemyLeashed("enemy-a", 8f), Is.False);
            Assert.That(state.EncounterQuietRemainingSeconds, Is.Zero);
            Assert.That(state.MarkEnemyLeashed("enemy-b", 8f), Is.True);
            Assert.That(state.AdvanceEncounterQuietTime(7.5f, out _), Is.False);
            Assert.That(state.HasActiveEncounter, Is.True);
            Assert.That(state.AdvanceEncounterQuietTime(.5f, out string resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(encounterId));
            Assert.That(state.HasActiveEncounter, Is.False);
        }

        [Test]
        public void ReinforcementOrRenewedAggro_CancelsQuietCountdownUntilAllLeashAgain()
        {
            var state = new SurvivalSkillState();
            state.RegisterEnemyAggro("run-a", 2, "enemy-a");
            Assert.That(state.MarkEnemyLeashed("enemy-a", 8f), Is.True);
            state.AdvanceEncounterQuietTime(3f, out _);

            Assert.That(state.RegisterEnemyAggro("run-a", 2, "enemy-b"),
                Is.EqualTo(SurvivalSkillState.EncounterMembershipChange.Joined));
            Assert.That(state.EncounterQuietRemainingSeconds, Is.Zero);
            Assert.That(state.LeashedEnemyInstanceIds, Is.Empty);

            Assert.That(state.MarkEnemiesLeashed(
                new[] { "enemy-a", "enemy-b" }, 8f), Is.True);
            Assert.That(state.EncounterQuietRemainingSeconds, Is.EqualTo(8f));
        }

        [Test]
        public void DeathOfLastNonLeashedEnemy_StartsQuietForRemainingLeashedEnemy()
        {
            var state = new SurvivalSkillState();
            state.RegisterEnemyAggro("run-a", 2, "enemy-leashed");
            state.RegisterEnemyAggro("run-a", 2, "enemy-fighting");
            state.MarkEnemyLeashed("enemy-leashed", 8f);

            Assert.That(state.RemoveEnemyAndResolveIfEmpty(
                "enemy-fighting", 8f, out _), Is.False);
            Assert.That(state.HasActiveEncounter, Is.True);
            Assert.That(state.EncounterQuietRemainingSeconds, Is.EqualTo(8f));
        }

        [Test]
        public void SaveRestore_PreservesLiveMembershipAndQuietTimerWithoutPublishingStart()
        {
            var source = new SurvivalSkillState();
            const string scope = "scene:Assets/_Game/Scenes/Town.unity";
            source.RegisterEnemyAggro(scope, 0, "enemy-b");
            source.RegisterEnemyAggro(scope, 0, "enemy-a");
            source.MarkEnemiesLeashed(new[] { "enemy-b", "enemy-a" }, 8f);
            source.AdvanceEncounterQuietTime(2.5f, out _);
            SurvivalSkillSaveData saved = source.CaptureSaveData();

            int startedEvents = 0;
            GameEventBus.Subscribe<SurvivalEncounterStartedEvent>(_ => startedEvents++);
            var restored = new SurvivalSkillState();
            restored.BeginRestore();
            restored.RestoreFromSaveData(saved, scope, 0);
            restored.EndRestore();

            Assert.That(startedEvents, Is.Zero);
            Assert.That(saved.Version, Is.EqualTo(4));
            Assert.That(saved.ActiveEnemyInstanceIds,
                Is.EqualTo(new[] { "enemy-a", "enemy-b" }));
            Assert.That(saved.LeashedEnemyInstanceIds,
                Is.EqualTo(new[] { "enemy-a", "enemy-b" }));
            Assert.That(restored.ActiveEncounterId, Is.EqualTo(source.ActiveEncounterId));
            Assert.That(restored.EncounterQuietRemainingSeconds, Is.EqualTo(5.5f));
        }

        [Test]
        public void StartedEvent_ExposesScopeAndFirstEnemyWhileLegacyConstructorStaysNeutral()
        {
            var current = new SurvivalEncounterStartedEvent(
                "encounter-1", "scene:Town", 0, "enemy-first");
            Assert.That(current.ScopeId, Is.EqualTo("scene:Town"));
            Assert.That(current.RunId, Is.EqualTo("scene:Town"));
            Assert.That(current.FirstEnemyInstanceId, Is.EqualTo("enemy-first"));

            var legacy = new SurvivalEncounterStartedEvent("encounter-old", "run-old", 3);
            Assert.That(legacy.ScopeId, Is.EqualTo("run-old"));
            Assert.That(legacy.FirstEnemyInstanceId, Is.Empty);
        }

        [TestCase("Assets\\_Game\\Scenes\\Town.unity", "Town",
            "scene:Assets/_Game/Scenes/Town.unity")]
        [TestCase("", "Farm", "scene:Farm")]
        [TestCase("", "", "")]
        public void SceneScopeId_UsesStablePathThenName(
            string path,
            string name,
            string expected)
        {
            Assert.That(SurvivalSkillRuntimeCoordinator.ComposeSceneScopeId(path, name),
                Is.EqualTo(expected));
        }
    }
}
