using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Save.Providers;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public sealed class SurvivalEncounterGeneralizationPlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private ICaveRunContext _previousCaveContext;
        private SurvivalSkillState _previousState;
        private SurvivalSkillState _state;
        private SurvivalSkillRuntimeCoordinator _runtime;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null)
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;

            if (SurvivalSkillRuntimeCoordinator.Instance != null)
            {
                Object.Destroy(SurvivalSkillRuntimeCoordinator.Instance.gameObject);
                yield return null;
            }

            _previousCaveContext = DomainManagerRegistry.Get<ICaveRunContext>();
            _previousState = DomainManagerRegistry.Get<SurvivalSkillState>();
            DomainManagerRegistry.Unregister<ICaveRunContext>();
            DomainManagerRegistry.Unregister<SurvivalSkillState>();
            _state = new SurvivalSkillState();
            DomainManagerRegistry.Register(_state);
            var owner = new GameObject("Phase19C scene encounter fixture");
            _owned.Add(owner);
            _runtime = SurvivalSkillRuntimeCoordinator.Install(owner.transform);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            foreach (var item in _owned)
                if (item != null) Object.Destroy(item);
            _owned.Clear();
            yield return null;

            DomainManagerRegistry.Unregister<SurvivalSkillState>();
            DomainManagerRegistry.Unregister<ICaveRunContext>();
            if (_previousState != null)
                DomainManagerRegistry.Register(_previousState);
            if (_previousCaveContext != null)
                DomainManagerRegistry.Register(_previousCaveContext);
        }

        [UnityTest]
        public IEnumerator SceneEncounter_AggroStartsWithFirstEnemy_ReinforcementAndAllDeadResolve()
        {
            SurvivalEncounterStartedEvent started = default;
            var joined = new List<string>();
            SurvivalEncounterResolvedEvent resolved = default;
            int starts = 0;
            int resolutions = 0;
            System.Action<SurvivalEncounterStartedEvent> onStarted = evt =>
            {
                starts++;
                started = evt;
            };
            System.Action<SurvivalEncounterEnemyJoinedEvent> onJoined = evt =>
                joined.Add(evt.EnemyInstanceId);
            System.Action<SurvivalEncounterResolvedEvent> onResolved = evt =>
            {
                resolutions++;
                resolved = evt;
            };
            GameEventBus.Subscribe(onStarted);
            GameEventBus.Subscribe(onJoined);
            GameEventBus.Subscribe(onResolved);
            try
            {
                Enemy("scene-enemy-b");
                GameEventBus.Publish(new EnemyAggroStartedEvent("scene-enemy-a", "scene-pack"));
                GameEventBus.Publish(new EnemyPackAlertedEvent(
                    "scene-pack", Vector2.zero, 2,
                    new[] { "scene-enemy-a", "scene-enemy-b" }));

                Assert.That(starts, Is.EqualTo(1));
                Assert.That(started.FirstEnemyInstanceId, Is.EqualTo("scene-enemy-a"));
                Assert.That(started.CaveLevel, Is.Zero);
                Assert.That(started.ScopeId, Does.StartWith("scene:"));
                Assert.That(_state.ActiveEncounterId, Does.StartWith(started.ScopeId));
                Assert.That(joined, Is.EquivalentTo(new[] { "scene-enemy-b" }));

                GameEventBus.Publish(Killed("scene-enemy-a"));
                Assert.That(_state.HasActiveEncounter, Is.True);
                Assert.That(resolutions, Is.Zero);
                GameEventBus.Publish(Killed("scene-enemy-b"));

                Assert.That(_state.HasActiveEncounter, Is.False);
                Assert.That(resolutions, Is.EqualTo(1));
                Assert.That(resolved.EncounterId, Is.EqualTo(started.EncounterId));
                Assert.That(resolved.Reason, Is.EqualTo("all_dead"));
            }
            finally
            {
                GameEventBus.Unsubscribe(onStarted);
                GameEventBus.Unsubscribe(onJoined);
                GameEventBus.Unsubscribe(onResolved);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneEncounter_AllMembersLeashed_ResolvesAfterEightQuietSeconds()
        {
            Enemy("leash-b");
            GameEventBus.Publish(new EnemyAggroStartedEvent("leash-a", "leash-pack"));
            GameEventBus.Publish(new EnemyPackAlertedEvent(
                "leash-pack", Vector2.zero, 2, new[] { "leash-a", "leash-b" }));
            string encounterId = _state.ActiveEncounterId;
            SurvivalEncounterResolvedEvent resolved = default;
            int resolutions = 0;
            System.Action<SurvivalEncounterResolvedEvent> onResolved = evt =>
            {
                resolutions++;
                resolved = evt;
            };
            GameEventBus.Subscribe(onResolved);
            try
            {
                GameEventBus.Publish(new EnemyPackLeashCompletedEvent(
                    "leash-pack", new[] { "leash-a", "leash-b" }));
                Assert.That(_state.EncounterQuietRemainingSeconds, Is.EqualTo(8f));
                Assert.That(_state.HasActiveEncounter, Is.True);

                Time.timeScale = 20f;
                yield return new WaitForSeconds(8.1f);

                Assert.That(_state.HasActiveEncounter, Is.False);
                Assert.That(resolutions, Is.EqualTo(1));
                Assert.That(resolved.EncounterId, Is.EqualTo(encounterId));
                Assert.That(resolved.Reason, Is.EqualTo("permanent_escape"));
            }
            finally
            {
                Time.timeScale = 1f;
                GameEventBus.Unsubscribe(onResolved);
            }
        }

        [UnityTest]
        public IEnumerator SceneEncounter_SaveRestore_IsSilentAndKeepsMembership()
        {
            Enemy("restore-b");
            GameEventBus.Publish(new EnemyAggroStartedEvent("restore-a", "restore-pack"));
            GameEventBus.Publish(new EnemyPackAlertedEvent(
                "restore-pack", Vector2.zero, 2, new[] { "restore-a", "restore-b" }));
            string scopeId = _state.ActiveRunId;
            string encounterId = _state.ActiveEncounterId;
            var sourceProvider = new SurvivalSkillSectionProvider(_state);
            object section = sourceProvider.Capture(null);
            var restored = new SurvivalSkillState();
            var restoredProvider = new SurvivalSkillSectionProvider(restored);
            int starts = 0;
            int joins = 0;
            int resolutions = 0;
            System.Action<SurvivalEncounterStartedEvent> onStarted = _ => starts++;
            System.Action<SurvivalEncounterEnemyJoinedEvent> onJoined = _ => joins++;
            System.Action<SurvivalEncounterResolvedEvent> onResolved = _ => resolutions++;
            GameEventBus.Subscribe(onStarted);
            GameEventBus.Subscribe(onJoined);
            GameEventBus.Subscribe(onResolved);
            try
            {
                restoredProvider.RestoreForContext(section, scopeId, 0);

                Assert.That(restored.ActiveEncounterId, Is.EqualTo(encounterId));
                Assert.That(restored.ActiveEnemyInstanceIds,
                    Is.EquivalentTo(new[] { "restore-a", "restore-b" }));
                Assert.That(starts, Is.Zero);
                Assert.That(joins, Is.Zero);
                Assert.That(resolutions, Is.Zero);
            }
            finally
            {
                GameEventBus.Unsubscribe(onStarted);
                GameEventBus.Unsubscribe(onJoined);
                GameEventBus.Unsubscribe(onResolved);
            }

            yield return null;
        }

        private static EnemyKilledEvent Killed(string instanceId) => new EnemyKilledEvent(
            "enemy_fixture", string.Empty, 0, Vector3.zero, 0,
            instanceId, string.Empty, 0, false, false);

        private EnemyHealth Enemy(string instanceId)
        {
            var go = new GameObject(instanceId);
            _owned.Add(go);
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = "enemy_fixture";
            data.DisplayName = instanceId;
            data.maxHp = 100;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            health.ConfigureLootContext(instanceId, "phase19c-fixture");
            return health;
        }
    }
}
