using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Magic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Save.Providers;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public sealed class MagicConfluenceCapstonePlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private readonly HashSet<int> _preexistingProjectileIds = new HashSet<int>();
        private CombatCapstoneState _ownerState;
        private MagicConfluenceRuntime _runtime;
        private string _variant;
        private int _rank;
        private ISpellCastPreparationPolicy _previousPreparationSource;
        private ManaManager _mana;
        private StaminaManager _stamina;
        private int _savedMaxMana;
        private int _savedMana;
        private int _savedMaxStamina;
        private int _savedStamina;
        private SpellCastService _spells;
        private ItemDataSO _fireballItem;
        private ItemDataSO _barrierItem;
        private SkillActionSO _fireSpark;
        private SkillActionSO _toxicCloud;
        private GameObject _caster;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null)
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;

            foreach (var projectile in SceneProjectiles())
                _preexistingProjectileIds.Add(projectile.GetEntityId().GetHashCode());

            var bootstrap = GameBootstrap.Instance;
            _mana = bootstrap.ManaManager as ManaManager;
            _stamina = bootstrap.StaminaManager as StaminaManager;
            var itemDatabase = bootstrap.ItemDatabase as ItemDatabaseSO;
            Assert.That(_mana, Is.Not.Null);
            Assert.That(_stamina, Is.Not.Null);
            Assert.That(itemDatabase, Is.Not.Null);
            Assert.That(itemDatabase.TryGetById(
                "item_spell_fireball_test", out _fireballItem), Is.True);
            Assert.That(itemDatabase.TryGetById(
                "item_consumable_scroll_cast_barrier", out _barrierItem), Is.True);

            _savedMaxMana = _mana.MaxMana;
            _savedMana = _mana.CurrentMana;
            _savedMaxStamina = _stamina.MaxStamina;
            _savedStamina = _stamina.CurrentStamina;
            _mana.SetMaxMana(100);
            _mana.Initialize();
            _stamina.Initialize(100, 50);

            _variant = MagicConfluenceState.SenyaVariant;
            _rank = 3;
            _ownerState = new CombatCapstoneState();
            _runtime = new MagicConfluenceRuntime(
                _ownerState.MagicConfluence, () => _rank, () => _variant,
                amount => _stamina.AddStamina(amount));
            _previousPreparationSource = SpellCastPreparationProvider.Source;
            SpellCastPreparationProvider.Source = _runtime;

            var resolver = new EquippedItemResolver(
                itemDatabase, bootstrap.WeaponDatabase, bootstrap.SpellDatabase);
            _spells = new SpellCastService(
                _mana, null, resolver, 0f, bootstrap.StatusEffectDatabase)
            {
                PlayerManager = bootstrap.PlayerManager as PlayerManager,
                StaminaManager = _stamina
            };
            _fireSpark = FindAction("skill_magic_fire_spark");
            _toxicCloud = FindAction("skill_magic_toxic_cloud");
            _caster = new GameObject("Phase19D active magic caster");
            _caster.transform.position = new Vector2(9100f, 9100f);
            _owned.Add(_caster);
            PlayerBarrierState.Clear();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SpellCastPreparationProvider.Source = _previousPreparationSource;
            PlayerBarrierState.Clear();
            _mana.SetMaxMana(_savedMaxMana);
            _mana.SetMana(_savedMana);
            _stamina.Initialize(_savedMaxStamina, _savedStamina);
            foreach (var projectile in SceneProjectiles())
                if (!_preexistingProjectileIds.Contains(projectile.GetEntityId().GetHashCode()))
                    Object.Destroy(projectile.gameObject);
            foreach (var item in _owned)
                if (item != null) Object.Destroy(item);
            _owned.Clear();
            _preexistingProjectileIds.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SpellAndActiveMagic_ThresholdArmsWithoutAutoConsume_WrongClassKeepsSeed()
        {
            int armedEvents = 0;
            int consumedEvents = 0;
            System.Action<MagicConfluenceArmedEvent> onArmed = _ => armedEvents++;
            System.Action<MagicConfluenceConsumedEvent> onConsumed = _ => consumedEvents++;
            GameEventBus.Subscribe(onArmed);
            GameEventBus.Subscribe(onConsumed);
            try
            {
                Assert.That(Cast(_fireballItem).Success, Is.True);
                Assert.That(_ownerState.MagicConfluence.LedgerManaSpent, Is.EqualTo(25));

                SkillEffectResult crossing = ExecuteFireSpark();
                Assert.That(crossing.Success, Is.True);
                Assert.That(_ownerState.MagicConfluence.IsArmed, Is.True,
                    "25 MP from SpellCastService plus 10 MP from active magic reaches ceil(100 x 35%).");
                Assert.That(_ownerState.MagicConfluence.ArmedVariant,
                    Is.EqualTo(MagicConfluenceState.SenyaVariant));
                Assert.That(_ownerState.MagicConfluence.ArmedRemainingSeconds,
                    Is.EqualTo(8f));
                Assert.That(_ownerState.MagicConfluence.LockoutRemainingSeconds, Is.Zero,
                    "The cast crossing the threshold arms and cannot consume its own seed.");
                Assert.That(armedEvents, Is.EqualTo(1));
                Assert.That(consumedEvents, Is.Zero);
                Assert.That(_mana.CurrentMana, Is.EqualTo(65));

                Assert.That(Cast(_barrierItem).Success, Is.True,
                    "A Spiritual cast still resolves while Senya is armed.");
                Assert.That(_ownerState.MagicConfluence.IsArmed, Is.True,
                    "The wrong discipline must not reserve or consume Senya.");
                Assert.That(_mana.CurrentMana, Is.EqualTo(45));
                Assert.That(consumedEvents, Is.Zero);

                SkillEffectResult consuming = ExecuteFireSpark();
                Assert.That(consuming.Success, Is.True);
                Assert.That(_mana.CurrentMana, Is.EqualTo(34),
                    "Senya R3 atomically charges 10 base MP plus ceil(10 x 10%) surcharge.");
                Assert.That(_ownerState.MagicConfluence.IsArmed, Is.False);
                Assert.That(_ownerState.MagicConfluence.LockoutRemainingSeconds,
                    Is.EqualTo(25f));
                Assert.That(consumedEvents, Is.EqualTo(1));
            }
            finally
            {
                GameEventBus.Unsubscribe(onArmed);
                GameEventBus.Unsubscribe(onConsumed);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Senya_InsufficientSurchargeFailsAtomically_InBothMagicPaths()
        {
            ArmWithTwoFireballs();
            _mana.SetMana(27);

            AttackResult spellFailure = Cast(_fireballItem);
            Assert.That(spellFailure.Success, Is.False);
            Assert.That(spellFailure.ErrorCode, Is.EqualTo("InsufficientMana"));
            Assert.That(_mana.CurrentMana, Is.EqualTo(27));
            Assert.That(_ownerState.MagicConfluence.IsArmed, Is.True);
            Assert.That(_ownerState.MagicConfluence.HasReservation, Is.False);
            Assert.That(_ownerState.MagicConfluence.LockoutRemainingSeconds, Is.Zero);

            _mana.SetMana(10);
            SkillEffectResult activeFailure = ExecuteFireSpark();
            Assert.That(activeFailure.Success, Is.False);
            Assert.That(activeFailure.FailureReason, Is.EqualTo("InsufficientMana"));
            Assert.That(_mana.CurrentMana, Is.EqualTo(10));
            Assert.That(_ownerState.MagicConfluence.IsArmed, Is.True);
            Assert.That(_ownerState.MagicConfluence.HasReservation, Is.False,
                "The active-skill preview and failed reservation must both release the seed.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SpellCastInterruption_RefundsReservationWithoutLedgerOrArm()
        {
            _mana.SetMana(100);
            int interruptions = 0;
            int refunded = 0;
            System.Action<SpellCastInterruptedEvent> onInterrupted = evt =>
            {
                interruptions++;
                refunded = evt.RefundedMana;
            };
            GameEventBus.Subscribe(onInterrupted);
            try
            {
                AttackResult begun = _spells.TryBeginCast(
                    EquipmentSlot.RightHand, _fireballItem, -1000f,
                    Vector2.right, _caster.transform.position, out var plan);
                Assert.That(begun.Success, Is.True);
                Assert.That(plan, Is.Not.Null);
                Assert.That(plan.Transaction.State, Is.EqualTo(SpellCastTransactionState.Reserved));
                Assert.That(_mana.CurrentMana, Is.EqualTo(75));
                Assert.That(_ownerState.MagicConfluence.LedgerManaSpent, Is.Zero,
                    "Reserved mana is not a confirmed cast and cannot enter the ledger.");

                _spells.RefundCast(plan);

                Assert.That(plan.Transaction.State, Is.EqualTo(SpellCastTransactionState.Cancelled));
                Assert.That(_mana.CurrentMana, Is.EqualTo(100));
                Assert.That(_ownerState.MagicConfluence.LedgerManaSpent, Is.Zero);
                Assert.That(_ownerState.MagicConfluence.IsArmed, Is.False);
                Assert.That(_ownerState.MagicConfluence.HasReservation, Is.False);
                Assert.That(interruptions, Is.EqualTo(1));
                Assert.That(refunded, Is.EqualTo(25));
            }
            finally
            {
                GameEventBus.Unsubscribe(onInterrupted);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Anya_BarrierOutputAndEchoUseDiscountedCost_LockoutSurvivesSaveLoad()
        {
            _variant = MagicConfluenceState.AnyaVariant;
            _rank = 3;
            _mana.SetMana(100);
            _stamina.Initialize(100, 40);
            Assert.That(Cast(_fireballItem).Success, Is.True);
            Assert.That(Cast(_fireballItem).Success, Is.True);
            Assert.That(_ownerState.MagicConfluence.IsArmed, Is.True);
            int manaBefore = _mana.CurrentMana;

            Assert.That(Cast(_barrierItem).Success, Is.True);

            Assert.That(manaBefore - _mana.CurrentMana, Is.EqualTo(10),
                "Anya R3 uses ceil(20 x (1 - 50%)).");
            Assert.That(PlayerBarrierState.ActiveInstance, Is.Not.Null);
            Assert.That(PlayerBarrierState.ActiveInstance.RemainingAbsorb, Is.EqualTo(54),
                "Barrier output receives +35%: round(40 x 1.35) = 54.");
            Assert.That(_ownerState.MagicConfluence.EchoTotal, Is.EqualTo(3),
                "Echo uses floor(10 discounted MP x 35%).");
            Assert.That(_ownerState.MagicConfluence.EchoDurationSeconds, Is.EqualTo(4f));
            Assert.That(_ownerState.MagicConfluence.LockoutRemainingSeconds, Is.EqualTo(25f));

            _runtime.Tick(2f);
            Assert.That(_stamina.CurrentStamina, Is.EqualTo(41));
            _runtime.Tick(2f);
            Assert.That(_stamina.CurrentStamina, Is.EqualTo(43));
            Assert.That(_ownerState.MagicConfluence.EchoDelivered, Is.EqualTo(3));

            var sourceProvider = new CombatCapstoneSectionProvider(_ownerState);
            object section = sourceProvider.Capture(null);
            var restoredOwner = new CombatCapstoneState();
            int armedEvents = 0;
            int consumedEvents = 0;
            System.Action<MagicConfluenceArmedEvent> onArmed = _ => armedEvents++;
            System.Action<MagicConfluenceConsumedEvent> onConsumed = _ => consumedEvents++;
            GameEventBus.Subscribe(onArmed);
            GameEventBus.Subscribe(onConsumed);
            try
            {
                new CombatCapstoneSectionProvider(restoredOwner).Restore(section);

                Assert.That(restoredOwner.MagicConfluence.LockoutRemainingSeconds,
                    Is.EqualTo(21f).Within(.001f));
                Assert.That(restoredOwner.MagicConfluence.IsArmed, Is.False);
                Assert.That(restoredOwner.MagicConfluence.LedgerManaSpent, Is.Zero);
                Assert.That(armedEvents, Is.Zero);
                Assert.That(consumedEvents, Is.Zero,
                    "Restore rehydrates state without replaying the consumption trigger.");
            }
            finally
            {
                GameEventBus.Unsubscribe(onArmed);
                GameEventBus.Unsubscribe(onConsumed);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Senya_ToxicCloudCarriesOnePreparationAcrossEveryPulse_WithoutRecursion()
        {
            ArmWithTwoFireballs();
            EnemyHealth target = Enemy("senya-toxic-cloud-target",
                (Vector2)_caster.transform.position + Vector2.right * _toxicCloud.TargetingRange,
                500);
            var damageEvents = new List<DamageResult>();
            int consumedEvents = 0;
            string castToken = string.Empty;
            System.Action<DamageAppliedEvent> onDamage = evt =>
            {
                DamageResult result = evt.DamageResult;
                if (result != null && result.TargetInstanceId == target.EnemyInstanceId &&
                    result.SourceKind == DamageSourceKind.PlayerMagic)
                {
                    damageEvents.Add(result);
                    if (string.IsNullOrEmpty(castToken)) castToken = result.ActionToken;
                }
            };
            System.Action<MagicConfluenceConsumedEvent> onConsumed = _ => consumedEvents++;
            GameEventBus.Subscribe(onDamage);
            GameEventBus.Subscribe(onConsumed);
            try
            {
                Physics2D.SyncTransforms();
                var executor = new PersistentZoneSkillEffectExecutor(
                    "combat.magic.toxic_cloud", "Nuvem Toxica", _toxicCloud);
                SkillEffectResult result = executor.Execute(new SkillEffectContext
                {
                    SkillActionId = _toxicCloud.SkillActionId,
                    EffectId = "combat.magic.toxic_cloud",
                    Rank = 3,
                    Caster = _caster,
                    Target = target.gameObject,
                    ActionData = _toxicCloud,
                    WorldPosition = _caster.transform.position
                });

                Assert.That(result.Success, Is.True);
                Assert.That(damageEvents, Has.Count.EqualTo(1),
                    "PersistentDamageZone applies its first real pulse during Configure.");
                Assert.That(castToken, Is.Not.Empty);
                Assert.That(consumedEvents, Is.EqualTo(1),
                    "The active cast consumes Senya once when committed, not once per damage tick.");
                Assert.That(_ownerState.MagicConfluence.IsArmed, Is.False);
                Assert.That(_ownerState.MagicConfluence.LockoutRemainingSeconds, Is.EqualTo(25f));

                PersistentDamageZone zone = FindZone(_toxicCloud.SkillActionId);
                Assert.That(zone, Is.Not.Null);
                _owned.Add(zone.gameObject);
                zone.Advance(Time.time + _toxicCloud.ResolveEffectDuration(3) + .1f);

                Assert.That(damageEvents, Has.Count.EqualTo(_toxicCloud.PulseCount));
                for (int pulseIndex = 0; pulseIndex < damageEvents.Count; pulseIndex++)
                {
                    DamageResult pulse = damageEvents[pulseIndex];
                    int authoredPulse = PersistentDamageZone.ResolvePulseDamage(
                        _toxicCloud.ResolveRank(3).Damage, _toxicCloud.PulseCount, pulseIndex);
                    Assert.That(pulse.ActionToken, Is.EqualTo(castToken),
                        "Every delayed pulse keeps the preparation payload captured by the cast.");
                    Assert.That(pulse.SpellDiscipline, Is.EqualTo(SpellDiscipline.Offensive));
                    Assert.That(pulse.BaseDamage,
                        Is.GreaterThanOrEqualTo(Mathf.RoundToInt(authoredPulse * 1.35f)),
                        "Each pulse retains Senya R3's +35% direct-damage preparation; a critical may raise it further.");
                    Assert.That(pulse.IsPrimaryDamage, Is.False);
                    Assert.That(pulse.CanTriggerCapstones, Is.False);
                    Assert.That(pulse.CanTriggerStatusEffects, Is.False);
                    Assert.That(pulse.CanTriggerReactions, Is.False);
                }
                Assert.That(consumedEvents, Is.EqualTo(1),
                    "Damage-over-time pulses cannot recursively consume or re-arm Confluence.");
                Assert.That(_ownerState.MagicConfluence.LedgerManaSpent, Is.Zero);
            }
            finally
            {
                GameEventBus.Unsubscribe(onDamage);
                GameEventBus.Unsubscribe(onConsumed);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SpellCastService_PositiveCostWithoutManaManagerRejectsBeforeLedgerOrSpawn()
        {
            var bootstrap = GameBootstrap.Instance;
            var itemDatabase = bootstrap.ItemDatabase as ItemDatabaseSO;
            var resolver = new EquippedItemResolver(
                itemDatabase, bootstrap.WeaponDatabase, bootstrap.SpellDatabase);
            var serviceWithoutMana = new SpellCastService(
                null, null, resolver, 0f, bootstrap.StatusEffectDatabase)
            {
                PlayerManager = bootstrap.PlayerManager as PlayerManager,
                StaminaManager = _stamina
            };
            int projectilesBefore = SceneProjectiles().Count;
            int commits = 0;
            System.Action<PlayerMagicCastCommittedEvent> onCommitted = _ => commits++;
            GameEventBus.Subscribe(onCommitted);
            try
            {
                AttackResult result = serviceWithoutMana.TryCast(
                    EquipmentSlot.RightHand, _fireballItem, -1000f,
                    Vector2.right, _caster.transform.position);

                Assert.That(result.Success, Is.False);
                Assert.That(result.ErrorCode, Is.EqualTo("InsufficientMana"));
                Assert.That(SceneProjectiles(), Has.Count.EqualTo(projectilesBefore));
                Assert.That(commits, Is.Zero);
                Assert.That(_ownerState.MagicConfluence.LedgerManaSpent, Is.Zero);
                Assert.That(_ownerState.MagicConfluence.IsArmed, Is.False);
                Assert.That(_ownerState.MagicConfluence.HasReservation, Is.False);
            }
            finally
            {
                GameEventBus.Unsubscribe(onCommitted);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ActiveProjectile_TotalSpawnFailureRefundsAndKeepsArmedSeedUnconsumed()
        {
            ArmWithTwoFireballs();
            int manaBefore = _mana.CurrentMana;
            int projectilesBefore = SceneProjectiles().Count;
            int spawnAttempts = 0;
            int commits = 0;
            int consumedEvents = 0;
            var executor = new ProjectileSkillEffectExecutor(
                "combat.magic.fire_spark", "Fagulha Ignea", _fireSpark, request =>
                {
                    spawnAttempts++;
                    return ProjectileSpawnResult.CreateError(
                        "FixtureSpawnFailure", "Deterministic spawn failure fixture.");
                });
            System.Action<PlayerMagicCastCommittedEvent> onCommitted = _ => commits++;
            System.Action<MagicConfluenceConsumedEvent> onConsumed = _ => consumedEvents++;
            GameEventBus.Subscribe(onCommitted);
            GameEventBus.Subscribe(onConsumed);
            try
            {
                SkillEffectResult result = executor.Execute(new SkillEffectContext
                {
                    SkillActionId = _fireSpark.SkillActionId,
                    EffectId = "combat.magic.fire_spark",
                    Rank = 3,
                    Caster = _caster,
                    ActionData = _fireSpark,
                    WorldPosition = _caster.transform.position
                });

                Assert.That(result.Success, Is.False);
                Assert.That(result.FailureReason, Is.EqualTo("ProjectileSpawnFailed"));
                Assert.That(spawnAttempts, Is.EqualTo(Mathf.Max(1, _fireSpark.ProjectileCount)));
                Assert.That(SceneProjectiles(), Has.Count.EqualTo(projectilesBefore));
                Assert.That(_mana.CurrentMana, Is.EqualTo(manaBefore),
                    "The reserved base cost plus Senya surcharge are refunded atomically.");
                Assert.That(commits, Is.Zero);
                Assert.That(consumedEvents, Is.Zero);
                Assert.That(_ownerState.MagicConfluence.IsArmed, Is.True);
                Assert.That(_ownerState.MagicConfluence.HasReservation, Is.False);
                Assert.That(_ownerState.MagicConfluence.LockoutRemainingSeconds, Is.Zero);
                Assert.That(_ownerState.MagicConfluence.LedgerManaSpent, Is.Zero);
            }
            finally
            {
                GameEventBus.Unsubscribe(onCommitted);
                GameEventBus.Unsubscribe(onConsumed);
            }

            yield return null;
        }

        private AttackResult Cast(ItemDataSO item)
            => _spells.TryCast(EquipmentSlot.RightHand, item, -1000f,
                Vector2.right, _caster.transform.position);

        private SkillEffectResult ExecuteFireSpark()
        {
            var executor = new ProjectileSkillEffectExecutor(
                "combat.magic.fire_spark", "Fagulha Ígnea", _fireSpark);
            return executor.Execute(new SkillEffectContext
            {
                SkillActionId = _fireSpark.SkillActionId,
                EffectId = "combat.magic.fire_spark",
                Rank = 3,
                Caster = _caster,
                ActionData = _fireSpark,
                WorldPosition = _caster.transform.position
            });
        }

        private void ArmWithTwoFireballs()
        {
            _mana.SetMana(100);
            Assert.That(Cast(_fireballItem).Success, Is.True);
            Assert.That(Cast(_fireballItem).Success, Is.True);
            Assert.That(_ownerState.MagicConfluence.IsArmed, Is.True);
            Assert.That(_ownerState.MagicConfluence.LockoutRemainingSeconds, Is.Zero);
        }

        private EnemyHealth Enemy(string instanceId, Vector2 position, int hp)
        {
            var go = new GameObject(instanceId);
            _owned.Add(go);
            go.transform.position = position;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Static;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * .35f;
            collider.isTrigger = true;
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = "enemy_magic_confluence_fixture";
            data.DisplayName = instanceId;
            data.maxHp = hp;
            data.defense = 0;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            health.ConfigureLootContext(instanceId, "phase19d-playmode");
            return health;
        }

        private static PersistentDamageZone FindZone(string actionId)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var zone in root.GetComponentsInChildren<PersistentDamageZone>())
                    if (zone.ActionId == actionId) return zone;
            return null;
        }

        private SkillActionSO FindAction(string actionId)
        {
            SkillActionSO found = null;
            foreach (var action in DefaultSkillActionCatalog.BuildAll())
            {
                _owned.Add(action);
                if (action.SkillActionId == actionId) found = action;
            }
            Assert.That(found, Is.Not.Null, actionId);
            return found;
        }

        private static List<ProjectileBehaviour> SceneProjectiles()
        {
            var result = new List<ProjectileBehaviour>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                result.AddRange(root.GetComponentsInChildren<ProjectileBehaviour>());
            return result;
        }
    }
}
