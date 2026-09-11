using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class SkillSurvivalActionsPlayModeTests
    {
        private const string RunId = "playmode-survival-run";

        private readonly List<Object> _owned = new List<Object>();
        private SurvivalSkillState _state;
        private SurvivalSkillRuntimeCoordinator _runtime;
        private ICaveRunContext _previousCaveContext;
        private SurvivalSkillState _previousSurvivalState;
        private Vector3 _previousPlayerPosition;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null || PlayerController.ActiveInstance == null)
            {
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
                yield return null;
            }

            if (SurvivalSkillRuntimeCoordinator.Instance != null)
            {
                Object.Destroy(SurvivalSkillRuntimeCoordinator.Instance.gameObject);
                yield return null;
            }

            _previousCaveContext = DomainManagerRegistry.Get<ICaveRunContext>();
            _previousSurvivalState = DomainManagerRegistry.Get<SurvivalSkillState>();
            _state = new SurvivalSkillState();
            DomainManagerRegistry.Register(_state);
            DomainManagerRegistry.Register<ICaveRunContext>(new CaveRunContextFixture(RunId, 3));
            TemporaryRevealRegistryProvider.Reset();

            var owner = new GameObject("Survival skill PlayMode fixture");
            _owned.Add(owner);
            _runtime = SurvivalSkillRuntimeCoordinator.Install(owner.transform);
            yield return null;

            Assert.That(PlayerController.ActiveInstance, Is.Not.Null,
                "FarmScene must materialize the player used by directional survival checks.");
            _previousPlayerPosition = PlayerController.ActiveInstance.transform.position;
            PlayerController.ActiveInstance.transform.position = new Vector3(6000f, 6000f, 0f);
            Physics2D.SyncTransforms();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (PlayerController.ActiveInstance != null)
                PlayerController.ActiveInstance.transform.position = _previousPlayerPosition;

            foreach (var item in _owned)
                if (item != null) Object.Destroy(item);
            _owned.Clear();
            yield return null;

            TemporaryRevealRegistryProvider.Reset();
            DomainManagerRegistry.Unregister<SurvivalSkillState>();
            DomainManagerRegistry.Unregister<ICaveRunContext>();
            if (_previousSurvivalState != null)
                DomainManagerRegistry.Register(_previousSurvivalState);
            if (_previousCaveContext != null)
                DomainManagerRegistry.Register(_previousCaveContext);
        }

        [UnityTest]
        public IEnumerator EncounterLifecycle_JoinsReinforcementAndResolvesOnlyAfterLastEnemyDies()
        {
            Assert.That(_runtime, Is.Not.Null);
            Assert.That(SurvivalSkillRuntimeCoordinator.Install(null), Is.SameAs(_runtime));
            Assert.That(DirectionalMobilityModifierProvider.Source, Is.SameAs(_runtime));
            Assert.That(NaturalSurvivalRateModifierProvider.Source, Is.SameAs(_runtime));
            Assert.That(_runtime.State, Is.SameAs(_state));

            var started = new List<SurvivalEncounterStartedEvent>();
            var joined = new List<SurvivalEncounterEnemyJoinedEvent>();
            var resolved = new List<SurvivalEncounterResolvedEvent>();
            System.Action<SurvivalEncounterStartedEvent> onStarted = evt => started.Add(evt);
            System.Action<SurvivalEncounterEnemyJoinedEvent> onJoined = evt => joined.Add(evt);
            System.Action<SurvivalEncounterResolvedEvent> onResolved = evt => resolved.Add(evt);
            GameEventBus.Subscribe(onStarted);
            GameEventBus.Subscribe(onJoined);
            GameEventBus.Subscribe(onResolved);
            try
            {
                Enemy("enemy-b", new Vector3(6002f, 6000f, 0f));
                GameEventBus.Publish(new EnemyAggroStartedEvent("enemy-a", "pack-a"));
                GameEventBus.Publish(new EnemyPackAlertedEvent(
                    "pack-a", Vector2.zero, 2, new[] { "enemy-a", "enemy-b" }));

                Assert.That(started, Has.Count.EqualTo(1));
                Assert.That(joined.Exists(evt => evt.EnemyInstanceId == "enemy-b"), Is.True);
                Assert.That(_state.ActiveEnemyInstanceIds, Is.EquivalentTo(new[] { "enemy-a", "enemy-b" }));
                Assert.That(_state.ActiveEncounterId, Does.StartWith(RunId + "|level:3|encounter:"));

                GameEventBus.Publish(Killed("enemy-a"));
                Assert.That(_state.HasActiveEncounter, Is.True);
                Assert.That(_state.ActiveEnemyInstanceIds, Is.EquivalentTo(new[] { "enemy-b" }));
                Assert.That(resolved, Is.Empty);

                GameEventBus.Publish(Killed("enemy-b"));
                Assert.That(_state.HasActiveEncounter, Is.False,
                    $"Remaining: {string.Join(",", _state.ActiveEnemyInstanceIds)}");
                Assert.That(resolved, Has.Count.EqualTo(1));
                Assert.That(resolved[0].Reason, Is.EqualTo("all_dead"));
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
        public IEnumerator LastBreath_ArmsOnlyOnThresholdCrossingAndConsumesOnceInsideWindow()
        {
            GameEventBus.Publish(new EnemyAggroStartedEvent("last-breath-enemy", string.Empty));
            GameEventBus.Publish(new HPChangedEvent(0, 30, 100));
            GameEventBus.Publish(new HPChangedEvent(-6, 24, 100));

            Assert.That(_runtime.CanUseLastBreath, Is.True);
            string encounterId = _state.ActiveEncounterId;
            Assert.That(_runtime.TryConsumeLastBreath(), Is.True);
            Assert.That(_state.IsLastBreathConsumed(encounterId), Is.True);
            Assert.That(_runtime.TryConsumeLastBreath(), Is.False,
                "The encounter is marked consumed before any healing executor can run again.");

            GameEventBus.Publish(new HPChangedEvent(6, 30, 100));
            GameEventBus.Publish(new HPChangedEvent(-6, 24, 100));
            Assert.That(_runtime.CanUseLastBreath, Is.False,
                "A second crossing in the same encounter must remain consumed.");

            GameEventBus.Publish(Killed("last-breath-enemy"));
            GameEventBus.Publish(new EnemyAggroStartedEvent("next-enemy", string.Empty));
            GameEventBus.Publish(new HPChangedEvent(6, 30, 100));
            GameEventBus.Publish(new HPChangedEvent(-6, 24, 100));
            Assert.That(_runtime.CanUseLastBreath, Is.True);

            _state.AdvanceTime(5.01f);
            Assert.That(_runtime.CanUseLastBreath, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RetreatSignal_AppliesOnlyAwayFromNearestThreatAndOffenseCancelsIt()
        {
            var player = PlayerController.ActiveInstance;
            player.transform.position = new Vector3(6100f, 6100f, 0f);
            var threat = Enemy("retreat-threat", player.transform.position + Vector3.right * 2f);
            Physics2D.SyncTransforms();
            GameEventBus.Publish(new EnemyAggroStartedEvent(threat.EnemyInstanceId, string.Empty));

            Assert.That(_runtime.TryActivateRetreat(5f, .30f, .12f), Is.True);
            var away = DirectionalMobilityModifierProvider.Resolve(
                MobilityActionKind.SprintTick, -1f, 0f);
            var toward = DirectionalMobilityModifierProvider.Resolve(
                MobilityActionKind.DodgeCommit, 1f, 0f);
            Assert.That(away.CostMultiplier, Is.EqualTo(.70f).Within(.001f));
            Assert.That(away.SpeedMultiplier, Is.EqualTo(1.12f).Within(.001f));
            Assert.That(toward.CostMultiplier, Is.EqualTo(1f));
            Assert.That(toward.SpeedMultiplier, Is.EqualTo(1f));

            GameEventBus.Publish(new PlayerOffensiveActionCommittedEvent("attack.fixture", "melee"));
            Assert.That(_runtime.IsRetreatActive, Is.False);
            Assert.That(DirectionalMobilityModifierProvider.Resolve(
                MobilityActionKind.SprintTick, -1f, 0f).CostMultiplier, Is.EqualTo(1f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator SafeCamp_ModifiesOnlyInsideInclusiveZoneAndDamageDissolvesIt()
        {
            var center = new Vector2(6200f, 6200f);
            Assert.That(_runtime.TryActivateCamp(RunId, 3, 12f, center), Is.True);

            Assert.That(NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.HungerDrain, center.x + 2.5f, center.y), Is.EqualTo(.5f));
            Assert.That(NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.FatigueGain, center.x, center.y), Is.EqualTo(.5f));
            Assert.That(NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.StaminaRegen, center.x, center.y), Is.EqualTo(1.5f));
            Assert.That(NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.ManaRegen, center.x, center.y), Is.EqualTo(1.5f));
            Assert.That(NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.HealthRegen, center.x + 2.51f, center.y), Is.EqualTo(1f));

            int beforeDamage = _runtime.DamageSequence;
            GameEventBus.Publish(new PlayerDamagedEvent(1, center, "fixture"));
            Assert.That(_runtime.DamageSequence, Is.EqualTo(beforeDamage + 1));
            Assert.That(_state.ActiveCampRemainingSeconds, Is.Zero);
            Assert.That(NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.StaminaRegen, center.x, center.y), Is.EqualTo(1f));
            Assert.That(_runtime.TryActivateCamp(RunId, 3, 12f, center), Is.False,
                "Dissolving the zone must not refund the once-per-run use.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SurvivalInstinct_RevealsOnlyEligibleMaterializedTargetsInsideInclusiveRadius()
        {
            var caster = new GameObject("Survival instinct caster");
            _owned.Add(caster);
            caster.transform.position = new Vector2(6300f, 6300f);
            var edge = new RevealTargetFixture("edge", TemporaryRevealKind.Resource, 6307f, 6300f);
            var outside = new RevealTargetFixture("outside", TemporaryRevealKind.Resource, 6307.01f, 6300f);
            var exhausted = new RevealTargetFixture("exhausted", TemporaryRevealKind.Resource, 6301f, 6300f)
            {
                IsExhaustedValue = true
            };
            TemporaryRevealRegistryProvider.Registry.Register(outside);
            TemporaryRevealRegistryProvider.Registry.Register(exhausted);
            TemporaryRevealRegistryProvider.Registry.Register(edge);

            var action = Action("skill_survival_instinto_sobrevivencia");
            var executor = new SurvivalSkillEffectExecutor(
                "survival.instinto_sobrevivencia", "Instinto", action);
            var result = executor.Execute(new SkillEffectContext
            {
                SkillActionId = action.SkillActionId,
                Caster = caster,
                ActionData = action,
                Rank = 5,
                WorldPosition = caster.transform.position
            });

            Assert.That(result.Success, Is.True);
            Assert.That(edge.ApplyCount, Is.EqualTo(1));
            Assert.That(edge.SourceId, Is.EqualTo(action.SkillActionId));
            Assert.That(edge.ExpiresAt, Is.GreaterThan(Time.time + 7.5f));
            Assert.That(outside.ApplyCount, Is.Zero);
            Assert.That(exhausted.ApplyCount, Is.Zero);
            yield return null;
        }

        private EnemyHealth Enemy(string instanceId, Vector3 position)
        {
            var go = new GameObject(instanceId);
            _owned.Add(go);
            go.transform.position = position;
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = "enemy_fixture";
            data.DisplayName = instanceId;
            data.maxHp = 100;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            health.ConfigureLootContext(instanceId, "fixture-seed");
            return health;
        }

        private SkillActionSO Action(string id)
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            SkillActionSO selected = null;
            foreach (var action in actions)
            {
                _owned.Add(action);
                if (action.SkillActionId == id) selected = action;
            }
            Assert.That(selected, Is.Not.Null, id);
            return selected;
        }

        private static EnemyKilledEvent Killed(string instanceId) => new EnemyKilledEvent(
            "enemy_fixture", string.Empty, 0, Vector3.zero, 0,
            instanceId, string.Empty, 0, false, false);

        private sealed class CaveRunContextFixture : ICaveRunContext
        {
            public string CaveRunId { get; }
            public string CaveRunSeed => "playmode-seed";
            public int CurrentCaveLevel { get; }

            public CaveRunContextFixture(string runId, int caveLevel)
            {
                CaveRunId = runId;
                CurrentCaveLevel = caveLevel;
            }
        }

        private sealed class RevealTargetFixture : ITemporaryRevealTarget
        {
            public string RevealTargetId { get; }
            public TemporaryRevealKind RevealKind { get; }
            public float WorldX { get; }
            public float WorldY { get; }
            public bool IsEnabled => true;
            public bool IsSecret => false;
            public bool IsExhausted => IsExhaustedValue;
            public bool IsActive => true;
            public bool IsExhaustedValue { get; set; }
            public int ApplyCount { get; private set; }
            public string SourceId { get; private set; }
            public float ExpiresAt { get; private set; }

            public RevealTargetFixture(string id, TemporaryRevealKind kind, float x, float y)
            {
                RevealTargetId = id;
                RevealKind = kind;
                WorldX = x;
                WorldY = y;
            }

            public void ApplyTemporaryReveal(string sourceId, float expiresAt)
            {
                ApplyCount++;
                SourceId = sourceId;
                ExpiresAt = expiresAt;
            }
        }
    }
}
