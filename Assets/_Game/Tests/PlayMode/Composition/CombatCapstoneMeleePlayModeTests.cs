using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Magic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Player.Movement;
using CindarsHope.Save.Providers;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public sealed class CombatCapstoneMeleePlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private readonly List<CombatCapstoneRuntime> _runtimes =
            new List<CombatCapstoneRuntime>();
        private CombatCapstoneState _state;
        private CombatCapstoneRuntime _runtime;
        private PlayerManager _player;
        private StaminaManager _stamina;
        private int _savedMaxHp;
        private int _savedHp;
        private int _savedGold;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null || PlayerBlockController.ActiveInstance == null)
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;

            _player = GameBootstrap.Instance.PlayerManager as PlayerManager;
            _stamina = GameBootstrap.Instance.StaminaManager as StaminaManager;
            Assert.That(_player, Is.Not.Null);
            Assert.That(_stamina, Is.Not.Null);
            Assert.That(PlayerBlockController.ActiveInstance, Is.Not.Null,
                "FarmScene must expose the real block receiver used by PlayerDamageReceiver.");

            _savedMaxHp = _player.MaxHP;
            _savedHp = _player.CurrentHP;
            _savedGold = _player.CurrentGold;
            PlayerBarrierState.Clear();
            _player.RestoreState(101, 80, _savedGold);
            _stamina.Initialize(100, 50);

            _state = new CombatCapstoneState();
            _runtime = Runtime(_state, 2, CombatCapstoneState.KanthorVariant);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var block = PlayerBlockController.ActiveInstance;
            if (block != null && block.IsBlocking)
                block.SendMessage("StopBlock", SendMessageOptions.RequireReceiver);

            foreach (var runtime in _runtimes)
                runtime.Dispose();
            _runtimes.Clear();
            PlayerBarrierState.Clear();
            if (_player != null)
                _player.RestoreState(_savedMaxHp, _savedHp, _savedGold);
            foreach (var item in _owned)
                if (item != null) Object.Destroy(item);
            _owned.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator PerfectBlock_ReflectedPostureBreak_ActivatesKanthorOnce()
        {
            while (PlayerDamageReceiver.IsInSpawnGrace)
                yield return null;

            int activations = 0;
            CombatCapstoneActivatedEvent lastActivation = default;
            System.Action<CombatCapstoneActivatedEvent> onActivated = evt =>
            {
                activations++;
                lastActivation = evt;
            };
            GameEventBus.Subscribe(onActivated);
            try
            {
                EnemyHealth attacker = Enemy("kanthor-perfect-block-attacker", 100);
                var posture = attacker.gameObject.AddComponent<EnemyPostureState>();
                posture.Configure(EnemyDifficulty.VeryEasy);
                var block = PlayerBlockController.ActiveInstance;
                if (block.IsBlocking)
                    block.SendMessage("StopBlock", SendMessageOptions.RequireReceiver);
                block.SendMessage("TryStartBlock", SendMessageOptions.RequireReceiver);

                int applied = PlayerDamageReceiver.ApplyDamage(
                    _player, 100, "claw", DamageType.Physical, attacker.gameObject,
                    _player.gameObject);

                Assert.That(applied, Is.Zero);
                Assert.That(posture.IsBroken, Is.True,
                    "The real perfect-block receiver must reflect enough posture to break this fixture.");
                Assert.That(activations, Is.EqualTo(1),
                    "Perfect block and reflected posture break share one resolution and may activate once.");
                Assert.That(lastActivation.Variant, Is.EqualTo(CombatCapstoneState.KanthorVariant));
                Assert.That(_state.RemainingSeconds,
                    Is.EqualTo(CombatCapstoneState.WindowSeconds).Within(.001f));
                Assert.That(_state.HealChargeArmed, Is.True);
            }
            finally
            {
                GameEventBus.Unsubscribe(onActivated);
            }
        }

        [UnityTest]
        public IEnumerator ConfirmedPrimaryMelee_ConsumesOneKanthorHealAcrossRealEnemyHits()
        {
            GameEventBus.Publish(new PlayerPerfectBlockEvent("fixture", 10, "heal-resolution"));
            EnemyHealth target = Enemy("kanthor-heal-target", 100);
            int rewardEvents = 0;
            int healthReward = 0;
            System.Action<CombatCapstoneRewardEvent> onReward = evt =>
            {
                rewardEvents++;
                healthReward += evt.HealthRestored;
            };
            GameEventBus.Subscribe(onReward);
            try
            {
                target.TakeDamage(ConfirmedMeleeRequest(target, 10, "melee-hit-1"));
                target.TakeDamage(ConfirmedMeleeRequest(target, 10, "melee-hit-2"));

                Assert.That(target.CurrentHp, Is.EqualTo(80),
                    "Both requests must traverse EnemyHealth and publish confirmed damage.");
                Assert.That(_player.CurrentHP, Is.EqualTo(84), "ceil(101 x 3%) = 4.");
                Assert.That(healthReward, Is.EqualTo(4));
                Assert.That(rewardEvents, Is.EqualTo(1));
                Assert.That(_state.HealChargeArmed, Is.False);
            }
            finally
            {
                GameEventBus.Unsubscribe(onReward);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Kaand_ActivatesOnlyFromPlayerCausedPostureBreak()
        {
            _runtime.Dispose();
            _state = new CombatCapstoneState();
            _runtime = Runtime(_state, 3, CombatCapstoneState.KaandVariant);

            EnemyPostureState environmental = Enemy("kaand-environmental-break", 100)
                .gameObject.AddComponent<EnemyPostureState>();
            environmental.Configure(EnemyDifficulty.VeryEasy);
            Assert.That(environmental.ApplyPostureDamage(
                100f, "hazard", "trap", false, "environmental-break"), Is.True);
            Assert.That(_state.IsActive, Is.False);

            EnemyPostureState playerCaused = Enemy("kaand-player-break", 100)
                .gameObject.AddComponent<EnemyPostureState>();
            playerCaused.Configure(EnemyDifficulty.VeryEasy);
            Assert.That(playerCaused.ApplyPostureDamage(
                100f, "sword", "player", true, "player-break"), Is.True);

            Assert.That(_state.IsActive, Is.True);
            Assert.That(_runtime.DirectMeleeDamageMultiplier, Is.EqualTo(1.30f).Within(.001f));
            Assert.That(_runtime.MeleeCriticalDamageBonus, Is.EqualTo(.25f).Within(.001f));
            _runtime.Tick(CombatCapstoneState.WindowSeconds);
            Assert.That(_state.IsActive, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatCapstoneSaveRestore_PreservesStateWithoutPublishingTrigger()
        {
            GameEventBus.Publish(new PlayerPerfectBlockEvent("fixture", 10, "saved-resolution"));
            _runtime.Tick(2f);
            var sourceProvider = new CombatCapstoneSectionProvider(_state);
            object section = sourceProvider.Capture(null);
            var restored = new CombatCapstoneState();
            var restoredProvider = new CombatCapstoneSectionProvider(restored);
            int activations = 0;
            int rewards = 0;
            System.Action<CombatCapstoneActivatedEvent> onActivated = _ => activations++;
            System.Action<CombatCapstoneRewardEvent> onReward = _ => rewards++;
            GameEventBus.Subscribe(onActivated);
            GameEventBus.Subscribe(onReward);
            try
            {
                restoredProvider.Restore(section);

                Assert.That(restored.ActiveVariant, Is.EqualTo(CombatCapstoneState.KanthorVariant));
                Assert.That(restored.RemainingSeconds, Is.EqualTo(6f).Within(.001f));
                Assert.That(restored.HealChargeArmed, Is.True);
                Assert.That(restored.TryActivate(
                    CombatCapstoneState.KanthorVariant, 2, "saved-resolution"), Is.False);
                Assert.That(activations, Is.Zero);
                Assert.That(rewards, Is.Zero);
            }
            finally
            {
                GameEventBus.Unsubscribe(onActivated);
                GameEventBus.Unsubscribe(onReward);
            }

            yield return null;
        }

        private CombatCapstoneRuntime Runtime(
            CombatCapstoneState state, int rank, string variant)
        {
            var runtime = new CombatCapstoneRuntime(
                state,
                () => rank,
                () => variant,
                () => _player.CurrentHP,
                () => _player.MaxHP,
                amount => _player.RestoreHP(amount),
                amount => _stamina.AddStamina(amount));
            runtime.Enable();
            _runtimes.Add(runtime);
            return runtime;
        }

        private EnemyHealth Enemy(string instanceId, int hp)
        {
            var go = new GameObject(instanceId);
            _owned.Add(go);
            go.transform.position = new Vector2(7000f, 7000f);
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = "enemy_capstone_fixture";
            data.DisplayName = instanceId;
            data.maxHp = hp;
            data.defense = 0;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            health.ConfigureLootContext(instanceId, "phase19b-playmode");
            return health;
        }

        private static DamageRequest ConfirmedMeleeRequest(
            EnemyHealth target, int damage, string token)
            => new DamageRequest(target.EnemyId, damage)
            {
                SourceKind = DamageSourceKind.PlayerMelee,
                SourceId = "sword",
                SourceInstanceId = "player",
                ActionToken = token,
                IsPrimaryDamage = true,
                CanTriggerCapstones = true,
                CanTriggerStatusEffects = true,
                CanTriggerReactions = true
            };
    }
}
