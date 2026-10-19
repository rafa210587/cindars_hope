using System.Collections;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Save.Providers;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public sealed class CavebornCapstonePlayModeTests
    {
        private const string RunId = "phase20s-run-a";

        private readonly List<Object> _owned = new List<Object>();
        private SurvivalSkillState _previousState;
        private ICaveRunContext _previousCaveContext;
        private ISkillTreeRuntime _previousSkillTree;
        private System.Func<bool> _previousCombatState;
        private SurvivalSkillState _state;
        private SurvivalSkillRuntimeCoordinator _runtime;
        private CaveRunContextFixture _cave;
        private PlayerManager _player;
        private StaminaManager _stamina;
        private int _savedMaxHp;
        private int _savedHp;
        private int _savedMaxStamina;
        private int _savedStamina;

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

            _previousState = DomainManagerRegistry.Get<SurvivalSkillState>();
            _previousCaveContext = DomainManagerRegistry.Get<ICaveRunContext>();
            _previousSkillTree = DomainManagerRegistry.Get<ISkillTreeRuntime>();
            _previousCombatState = CombatStateProvider.IsInCombat;
            DomainManagerRegistry.Unregister<SurvivalSkillState>();
            DomainManagerRegistry.Unregister<ICaveRunContext>();
            DomainManagerRegistry.Unregister<ISkillTreeRuntime>();

            _player = GameBootstrap.Instance.PlayerManager as PlayerManager;
            _stamina = GameBootstrap.Instance.StaminaManager as StaminaManager;
            Assert.That(_player, Is.Not.Null);
            Assert.That(_stamina, Is.Not.Null);
            _savedMaxHp = _player.MaxHP;
            _savedHp = _player.CurrentHP;
            _savedMaxStamina = _stamina.MaxStamina;
            _savedStamina = _stamina.CurrentStamina;
            _player.SetMaxHP(100, false);
            _player.SetHP(100);
            _stamina.Initialize(100, 100);

            _state = new SurvivalSkillState();
            _cave = new CaveRunContextFixture(RunId, "phase20s-seed", 3);
            DomainManagerRegistry.Register(_state);
            DomainManagerRegistry.Register<ICaveRunContext>(_cave);
            DomainManagerRegistry.Register<ISkillTreeRuntime>(new SkillTreeRuntimeFixture(3));
            CombatStateProvider.IsInCombat = () => false;

            var owner = new GameObject("Phase20S Caveborn composition fixture");
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
            DomainManagerRegistry.Unregister<ISkillTreeRuntime>();
            if (_previousState != null) DomainManagerRegistry.Register(_previousState);
            if (_previousCaveContext != null)
                DomainManagerRegistry.Register<ICaveRunContext>(_previousCaveContext);
            if (_previousSkillTree != null)
                DomainManagerRegistry.Register<ISkillTreeRuntime>(_previousSkillTree);
            CombatStateProvider.IsInCombat = _previousCombatState;

            _player.SetMaxHP(_savedMaxHp, false);
            _player.SetHP(_savedHp);
            _stamina.Initialize(_savedMaxStamina, _savedStamina);
        }

        [UnityTest]
        public IEnumerator HpCrossing_ActivatesCavebornBeforeLastBreath_OncePerRun()
        {
            GameEventBus.Publish(new EnemyAggroStartedEvent("phase20s-enemy", "phase20s-pack"));
            Assert.That(_state.HasActiveEncounter, Is.True);
            _player.SetHP(30);

            int activations = 0;
            bool lastBreathWasAlreadyArmedAtActivation = true;
            System.Action<CavebornCapstoneActivatedEvent> onActivated = evt =>
            {
                activations++;
                lastBreathWasAlreadyArmedAtActivation =
                    _state.LastBreathArmedRemainingSeconds > 0f;
                Assert.That(evt.RunId, Is.EqualTo(RunId));
                Assert.That(evt.Rank, Is.EqualTo(3));
            };
            GameEventBus.Subscribe(onActivated);
            try
            {
                _player.SetHP(24);

                Assert.That(activations, Is.EqualTo(1));
                Assert.That(lastBreathWasAlreadyArmedAtActivation, Is.False,
                    "The HP handler publishes Caveborn activation before arming Last Breath.");
                Assert.That(_state.IsCavebornActive, Is.True);
                Assert.That(_state.ActiveCavebornRemainingSeconds, Is.EqualTo(10f));
                Assert.That(_state.LastBreathArmedRemainingSeconds, Is.EqualTo(5f));
                Assert.That(_state.CavebornConsumedRunId, Is.EqualTo(RunId));

                _player.SetHP(20);
                _stamina.Initialize(100, 10);
                GameEventBus.Publish(new PlayerFatigueChangedEvent(95f, 4));
                Assert.That(activations, Is.EqualTo(1),
                    "HP, stamina and Exhausted triggers share the same once-per-run charge.");
                Assert.That(_state.CavebornConsumedRunId, Is.EqualTo(RunId));
            }
            finally
            {
                GameEventBus.Unsubscribe(onActivated);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerVitalsApplier_HealsOnePerSecond_ThenTwoAfterRank3CombatExit()
        {
            PlayerVitalsApplierBootstrap.Install(null);
            yield return null;
            Assert.That(PlayerVitalsApplier.Instance, Is.Not.Null);
            Assert.That(PlayerController.ActiveInstance, Is.Not.Null);
            Assert.That(PlayerController.ActiveInstance.NeedsBalance, Is.Not.Null);
            Assert.That(PlayerController.ActiveInstance.NeedsBalance.BaseNaturalHealthRegenPerSecond,
                Is.EqualTo(1f));

            bool inCombat = true;
            CombatStateProvider.IsInCombat = () => inCombat;
            _player.SetHP(50);
            yield return null;

            inCombat = false;
            int baselineHp = _player.CurrentHP;
            yield return new WaitForSeconds(1.05f);
            Assert.That(_player.CurrentHP - baselineHp, Is.EqualTo(1),
                "The real PlayerVitalsApplier uses the authored 1 HP/s baseline out of combat.");

            inCombat = true;
            _player.SetHP(50);
            yield return null;
            Assert.That(_state.TryActivateCaveborn(RunId, 3, true), Is.True);
            yield return null;
            inCombat = false;
            yield return null;
            Assert.That(_state.CavebornRegenDoubled, Is.True);

            int cavebornHp = _player.CurrentHP;
            yield return new WaitForSeconds(1.05f);
            Assert.That(_player.CurrentHP - cavebornHp, Is.EqualTo(2),
                "After the first combat exit, rank 3 doubles the real natural healing to 2 HP/s.");
        }

        [UnityTest]
        public IEnumerator CaveDefeat_EndsBuff_AndNewRunRearmsWithoutChangingSeed()
        {
            string seedBefore = _cave.CaveRunSeed;
            Assert.That(_state.TryActivateCaveborn(RunId, 3, false), Is.True);

            GameEventBus.Publish(new CavePlayerDefeatedEvent(3));

            Assert.That(_state.IsCavebornActive, Is.False);
            Assert.That(_state.CavebornConsumedRunId, Is.EqualTo(RunId));
            _stamina.Initialize(100, 10);
            Assert.That(_state.IsCavebornActive, Is.False,
                "The defeated run cannot rearm from another eligible resource trigger.");

            _cave.SetRunId("phase20s-run-b");
            GameEventBus.Publish(new CaveRunIdentityStartedEvent(
                _cave.CaveRunId, _cave.CaveRunSeed, _cave.CurrentCaveLevel, "new_run"));

            Assert.That(_state.IsCavebornActive, Is.True);
            Assert.That(_state.ActiveCavebornRunId, Is.EqualTo("phase20s-run-b"));
            Assert.That(_state.CavebornConsumedRunId, Is.EqualTo("phase20s-run-b"));
            Assert.That(_cave.CaveRunSeed, Is.EqualTo(seedBefore),
                "Capstone lifecycle never writes cave generation seed or snapshots.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActiveRank3_ProvidesApprovedModifiers_AndSameRunRestoreDoesNotRearm()
        {
            string seedBefore = _cave.CaveRunSeed;
            Assert.That(_state.TryActivateCaveborn(RunId, 3, true), Is.True);
            Assert.That(CavebornCapstoneProvider.Source, Is.SameAs(_runtime));
            Assert.That(CavebornCapstoneProvider.ResolveCostMultiplier(),
                Is.EqualTo(.55f).Within(.001f));
            Assert.That(CavebornCapstoneProvider.ResolveDodgeDistanceBonus(),
                Is.EqualTo(.4f).Within(.001f));
            Assert.That(CavebornCapstoneProvider.ResolveIncomingDamage(10, DamageType.Ice),
                Is.EqualTo(7));
            Assert.That(CavebornCapstoneProvider.ResolveIncomingDamage(10, DamageType.Physical),
                Is.EqualTo(10));
            Assert.That(CavebornCapstoneProvider.ResolveStatusDuration(
                10f, CavebornStatusFamily.Fear), Is.EqualTo(7f).Within(.001f));
            Assert.That(CavebornCapstoneProvider.ResolveStatusDuration(
                10f, CavebornStatusFamily.Other), Is.EqualTo(10f).Within(.001f));

            Assert.That(_state.ObserveCavebornCombatState(false), Is.True);
            Assert.That(NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.HealthRegen, 0f, 0f), Is.EqualTo(2f));
            Assert.That(NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.StaminaRegen, 0f, 0f), Is.EqualTo(1f));

            object section = new SurvivalSkillSectionProvider(_state).Capture(null);
            var restored = new SurvivalSkillState();
            new SurvivalSkillSectionProvider(restored).RestoreForContext(section, RunId, 3);

            Assert.That(restored.IsCavebornActive, Is.True);
            Assert.That(restored.CavebornRegenDoubled, Is.True);
            Assert.That(restored.CavebornConsumedRunId, Is.EqualTo(RunId));
            Assert.That(restored.TryActivateCaveborn(RunId, 3, false), Is.False,
                "Load and same-run revisit preserve consumption instead of granting a new charge.");
            Assert.That(_cave.CaveRunSeed, Is.EqualTo(seedBefore),
                "Caveborn reads opaque run identity and never mutates the stable-run seed.");

            yield return null;
        }

        private sealed class CaveRunContextFixture : ICaveRunContext
        {
            public string CaveRunId { get; private set; }
            public string CaveRunSeed { get; }
            public int CurrentCaveLevel { get; }

            public CaveRunContextFixture(string runId, string seed, int level)
            {
                CaveRunId = runId;
                CaveRunSeed = seed;
                CurrentCaveLevel = level;
            }

            public void SetRunId(string runId) => CaveRunId = runId;
        }

        private sealed class SkillTreeRuntimeFixture : ISkillTreeRuntime
        {
            private readonly int _cavebornRank;

            public SkillTreeRuntimeFixture(int cavebornRank)
            {
                _cavebornRank = cavebornRank;
            }

            public void RebindProgressionManager() { }
            public List<SkillPassiveModifier> GetAllActivePassiveModifiers()
                => new List<SkillPassiveModifier>();
            public int GetPointsSpentInTree(string treeId) => 0;
            public int GetRank(string nodeId)
                => nodeId == CavebornCapstoneResolver.NodeId ? _cavebornRank : 0;
            public string GetChosenVariant(string nodeId) => string.Empty;
        }
    }
}
