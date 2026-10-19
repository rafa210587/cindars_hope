using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
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
    public sealed class RangedLunarCapstonePlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private readonly List<PlayerCombatStatsProvider> _statsProviders =
            new List<PlayerCombatStatsProvider>();
        private readonly List<DamageRequest> _secondaryRequests = new List<DamageRequest>();
        private SurvivalSkillState _state;
        private SurvivalSkillRuntimeCoordinator _encounters;
        private RangedLunarRuntime _lunar;
        private string _variant;
        private int _rank;
        private IRangedLunarModifierSource _previousLunarSource;
        private ICaveRunContext _previousCaveContext;
        private SurvivalSkillState _previousSurvivalState;
        private InventoryManager _inventory;
        private EquipmentManager _equipment;
        private StaminaManager _stamina;
        private InventorySaveData _inventorySnapshot;
        private EquipmentSaveData _equipmentSnapshot;
        private int _staminaMaxSnapshot;
        private int _staminaCurrentSnapshot;
        private bool _staminaInitializedSnapshot;
        private bool _hadCoordinator;
        private Transform _previousCoordinatorParent;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null)
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;

            if (SurvivalSkillRuntimeCoordinator.Instance != null)
            {
                _hadCoordinator = true;
                _previousCoordinatorParent = SurvivalSkillRuntimeCoordinator.Instance.transform.parent;
                Object.Destroy(SurvivalSkillRuntimeCoordinator.Instance.gameObject);
                yield return null;
            }

            _previousCaveContext = DomainManagerRegistry.Get<ICaveRunContext>();
            _previousSurvivalState = DomainManagerRegistry.Get<SurvivalSkillState>();
            _previousLunarSource = RangedLunarModifierProvider.Source;
            DomainManagerRegistry.Unregister<ICaveRunContext>();
            DomainManagerRegistry.Unregister<SurvivalSkillState>();
            _state = new SurvivalSkillState();
            DomainManagerRegistry.Register(_state);
            var owner = new GameObject("Phase19C lunar fixture");
            _owned.Add(owner);
            _encounters = SurvivalSkillRuntimeCoordinator.Install(owner.transform);

            _variant = RangedLunarRuntime.AlihanaVariant;
            _rank = 3;
            _lunar = new RangedLunarRuntime(
                _state, () => _rank, () => _variant,
                applySecondaryDamage: ApplyObservedSecondaryDamage);
            _lunar.Enable();
            RangedLunarModifierProvider.Source = _lunar;

            var bootstrap = GameBootstrap.Instance;
            _inventory = bootstrap.InventoryManager as InventoryManager;
            _equipment = EquipmentManager.Instance;
            _stamina = bootstrap.StaminaManager as StaminaManager;
            Assert.That(_inventory, Is.Not.Null);
            Assert.That(_equipment, Is.Not.Null);
            Assert.That(_stamina, Is.Not.Null);
            _inventorySnapshot = _inventory.CaptureSaveData();
            _equipmentSnapshot = _equipment.CaptureSaveData();
            _staminaMaxSnapshot = _stamina.MaxStamina;
            _staminaCurrentSnapshot = _stamina.CurrentStamina;
            _staminaInitializedSnapshot = _stamina.IsInitialized;
            _stamina.Initialize(100, 100);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _lunar?.Dispose();
            RangedLunarModifierProvider.Source = _previousLunarSource;
            foreach (var stats in _statsProviders) stats.Dispose();
            _statsProviders.Clear();
            _inventory?.RestoreFromSaveData(_inventorySnapshot);
            _equipment?.RestoreFromSaveData(_equipmentSnapshot);
            if (_stamina != null)
            {
                if (_staminaInitializedSnapshot)
                    _stamina.Initialize(_staminaMaxSnapshot, _staminaCurrentSnapshot);
                else
                    _stamina.Shutdown();
            }
            foreach (var item in _owned)
                if (item != null) Object.Destroy(item);
            _owned.Clear();
            _secondaryRequests.Clear();
            yield return null;

            DomainManagerRegistry.Unregister<SurvivalSkillState>();
            DomainManagerRegistry.Unregister<ICaveRunContext>();
            if (_previousSurvivalState != null)
                DomainManagerRegistry.Register(_previousSurvivalState);
            if (_previousCaveContext != null)
                DomainManagerRegistry.Register(_previousCaveContext);
            if (_hadCoordinator && SurvivalSkillRuntimeCoordinator.Instance == null)
                SurvivalSkillRuntimeCoordinator.Install(_previousCoordinatorParent);
        }

        [UnityTest]
        public IEnumerator MarkedPrey_FirstEncounterTargetCannotChangeAfterExpiryRespecDeathOrLoad()
        {
            Vector2 origin = new Vector2(8100f, 8100f);
            var caster = Body("lunar-mark-caster", origin, false);
            EnemyHealth first = Enemy("lunar-first", origin + Vector2.right, 300);
            EnemyHealth second = Enemy("lunar-second", origin + Vector2.right * 2f, 300);
            EnemyHealth third = Enemy("lunar-third", origin + Vector2.right * 3f, 300);
            EnemyHealth fourth = Enemy("lunar-fourth", origin + Vector2.right * 4f, 300);
            BeginEncounter(first, second, third, fourth);
            SkillActionSO markedPrey = MarkedPreyAction();
            var executor = new MarkedPreySkillEffectExecutor(markedPrey);
            var context = new SkillEffectContext
            {
                SkillActionId = "skill_ranged_marked_prey",
                Caster = caster,
                ActionData = markedPrey,
                Rank = 3
            };

            Assert.That(executor.Execute(context).Success, Is.True);
            Assert.That(_state.LunarConsumedFirstTargetInstanceId, Is.EqualTo(first.EnemyInstanceId));
            Assert.That(_state.ActiveLunarTargetInstanceId, Is.EqualTo(first.EnemyInstanceId));

            first.transform.position = origin + Vector2.right * 20f;
            _lunar.Tick(RangedLunarRules.FocusDuration(3));
            Assert.That(_state.HasActiveLunarFocus, Is.False);
            Assert.That(executor.Execute(context).Success, Is.True,
                "Marked Prey remains usable, but the lunar first-target benefit is encounter-bound.");
            AssertConsumedFirst(first);

            GameEventBus.Publish(new SkillTreeRespecCompletedEvent(1, 1));
            second.transform.position = origin + Vector2.right * 20f;
            Assert.That(executor.Execute(context).Success, Is.True);
            AssertConsumedFirst(first);

            first.TakeDamage(ConfirmedDamage(
                first, 9999, DamageSourceKind.PlayerRanged, DamageType.Physical, "kill-first"));
            third.transform.position = origin + Vector2.right * 20f;
            Assert.That(executor.Execute(context).Success, Is.True);
            AssertConsumedFirst(first);

            var provider = new SurvivalSkillSectionProvider(_state);
            object section = provider.Capture(null);
            var restored = new SurvivalSkillState();
            new SurvivalSkillSectionProvider(restored).RestoreForContext(
                section, _state.ActiveRunId, _state.ActiveCaveLevel);
            using var restoredRuntime = new RangedLunarRuntime(
                restored, () => 3, () => RangedLunarRuntime.AlihanaVariant);

            Assert.That(restored.LunarConsumedFirstTargetInstanceId,
                Is.EqualTo(first.EnemyInstanceId));
            Assert.That(restoredRuntime.TryActivateFirstTarget(
                fourth.EnemyInstanceId, "after-load"), Is.False);
            Assert.That(restored.LunarConsumedFirstTargetInstanceId,
                Is.EqualTo(first.EnemyInstanceId));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Alihana_ExistingArrowKeepsLaunchValues_NewArrowGetsRangeAndSpeed()
        {
            _variant = RangedLunarRuntime.AlihanaVariant;
            Vector2 origin = new Vector2(8200f, 8200f);
            BowFixture bow = PrepareBow(4);
            float baseRange = Mathf.Min(14f,
                bow.Stats.FinalBowRange(bow.Weapon.Range, EquipmentSlot.RightHand));
            float focusedRange = Mathf.Min(14f, baseRange * 1.16f);
            Assert.That(focusedRange, Is.GreaterThan(baseRange),
                "The fixture bow must remain below the global range cap.");
            float targetTravel = Mathf.Lerp(baseRange, focusedRange, .75f);
            EnemyHealth focusTarget = Enemy(
                "alihana-focus", origin + Vector2.right * (.5f + targetTravel), 300);
            BeginEncounter(focusTarget);
            Physics2D.SyncTransforms();

            var before = ProjectileIds();
            Assert.That(bow.Service.TryFire(
                EquipmentSlot.LeftHand, bow.Ammo, -1000f, Vector2.right, origin).Success, Is.True);
            ProjectileBehaviour existing = NewProjectile(before);
            _owned.Add(existing.gameObject);
            float existingInitialSpeed = existing.GetComponent<Rigidbody2D>().linearVelocity.magnitude;

            Assert.That(_lunar.TryActivateFirstTarget(
                focusTarget.EnemyInstanceId, "alihana-focus-token"), Is.True);
            for (int i = 0; i < 180 && existing != null; i++)
                yield return new WaitForFixedUpdate();
            Assert.That(existing == null, Is.True,
                "The pre-focus arrow must expire at its original range.");
            Assert.That(focusTarget.CurrentHp, Is.EqualTo(300));

            before = ProjectileIds();
            Assert.That(bow.Service.TryFire(
                EquipmentSlot.LeftHand, bow.Ammo, -1000f, Vector2.right, origin).Success, Is.True);
            ProjectileBehaviour focused = NewProjectile(before);
            _owned.Add(focused.gameObject);
            float focusedInitialSpeed = focused.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
            Assert.That(focusedInitialSpeed,
                Is.EqualTo(existingInitialSpeed * 1.15f).Within(.02f));

            for (int i = 0; i < 180 && focusTarget.CurrentHp == 300; i++)
                yield return new WaitForFixedUpdate();
            Assert.That(focusTarget.CurrentHp, Is.LessThan(300),
                "Only the arrow launched under Alihana may reach the target beyond base range.");
        }

        [UnityTest]
        public IEnumerator Nyx_IsolationAtImpactTogglesAcrossBoundary_OneCritRollPerArrow()
        {
            _variant = RangedLunarRuntime.NyxVariant;
            Vector2 origin = new Vector2(8300f, 8300f);
            EnemyHealth target = Enemy("nyx-focus", origin + Vector2.right * 2f, 500);
            EnemyHealth neighbor = Enemy("nyx-neighbor",
                target.transform.position + Vector3.up * RangedLunarRules.IsolationRadiusTiles, 500);
            BeginEncounter(target, neighbor);
            Assert.That(_lunar.TryActivateFirstTarget(target.EnemyInstanceId, "nyx-token"), Is.True);
            int rolls = 0;
            BowFixture bow = PrepareBow(3, () =>
            {
                rolls++;
                return .10f;
            });
            var targetHits = new List<DamageResult>();
            System.Action<DamageAppliedEvent> onDamage = evt =>
            {
                if (evt.DamageResult.SourceKind == DamageSourceKind.PlayerRanged &&
                    evt.DamageResult.TargetInstanceId == target.EnemyInstanceId)
                    targetHits.Add(evt.DamageResult);
            };
            GameEventBus.Subscribe(onDamage);
            try
            {
                Physics2D.SyncTransforms();
                Assert.That(bow.Service.TryFire(
                    EquipmentSlot.LeftHand, bow.Ammo, -1000f, Vector2.right, origin).Success, Is.True);
                for (int i = 0; i < 60 && targetHits.Count < 1; i++)
                    yield return new WaitForFixedUpdate();
                Assert.That(targetHits, Has.Count.EqualTo(1));
                Assert.That(targetHits[0].IsCritical, Is.False,
                    "A hostile exactly 2.5 tiles away keeps Nyx disabled.");
                Assert.That(rolls, Is.EqualTo(1));

                neighbor.transform.position = target.transform.position + Vector3.up * 2.51f;
                Physics2D.SyncTransforms();
                Assert.That(bow.Service.TryFire(
                    EquipmentSlot.LeftHand, bow.Ammo, -1000f, Vector2.right, origin).Success, Is.True);
                for (int i = 0; i < 60 && targetHits.Count < 2; i++)
                    yield return new WaitForFixedUpdate();

                Assert.That(targetHits, Has.Count.EqualTo(2));
                Assert.That(targetHits[1].IsCritical, Is.True,
                    "At 2.51 tiles the target is isolated and the +15pp bonus makes roll .10 critical.");
                Assert.That(rolls, Is.EqualTo(2),
                    "Each projectile performs its only critical roll at its confirmed impact.");
            }
            finally
            {
                GameEventBus.Unsubscribe(onDamage);
            }
        }

        [UnityTest]
        public IEnumerator Senya_RequiresOffensiveMagicHistory_SecondaryHitsTargetWithAllGatesOff()
        {
            _variant = RangedLunarRuntime.SenyaVariant;
            EnemyHealth target = Enemy("senya-focus", new Vector2(8402f, 8400f), 300);
            BeginEncounter(target);
            Assert.That(_lunar.TryActivateFirstTarget(target.EnemyInstanceId, "senya-token"), Is.True);
            var secondaryResults = new List<DamageResult>();
            System.Action<DamageAppliedEvent> onDamage = evt =>
            {
                if (evt.DamageResult.SourceKind == DamageSourceKind.CapstoneSecondary)
                    secondaryResults.Add(evt.DamageResult);
            };
            GameEventBus.Subscribe(onDamage);
            try
            {
                target.TakeDamage(ConfirmedDamage(
                    target, 20, DamageSourceKind.PlayerRanged, DamageType.Physical, "ranged-no-history"));
                Assert.That(_secondaryRequests, Is.Empty);
                Assert.That(secondaryResults, Is.Empty);

                CommitMagic("none-toxic", SpellDiscipline.None, DamageType.Toxic);
                CommitMagic("spiritual-ice", SpellDiscipline.Spiritual, DamageType.Ice);
                target.TakeDamage(ConfirmedDamage(
                    target, 20, DamageSourceKind.PlayerRanged, DamageType.Physical,
                    "ranged-after-spiritual"));
                Assert.That(_secondaryRequests, Is.Empty,
                    "Spiritual and None magic cannot author Senya's offensive history.");
                Assert.That(_state.LastOffensiveMagicDamageType, Is.EqualTo(DamageType.Physical));

                CommitMagic("offensive-fire", SpellDiscipline.Offensive, DamageType.Fire);
                target.TakeDamage(ConfirmedDamage(
                    target, 20, DamageSourceKind.PlayerRanged, DamageType.Physical, "ranged-with-history"));

                Assert.That(_secondaryRequests, Has.Count.EqualTo(1));
                DamageRequest request = _secondaryRequests[0];
                Assert.That(request.TargetInstanceId, Is.EqualTo(target.EnemyInstanceId));
                Assert.That(request.BaseDamage, Is.EqualTo(4), "ceil(20 x 16%) = 4.");
                Assert.That(request.DamageType, Is.EqualTo(DamageType.Fire));
                Assert.That(request.SourceKind, Is.EqualTo(DamageSourceKind.CapstoneSecondary));
                Assert.That(request.IsPrimaryDamage, Is.False);
                Assert.That(request.IsCritical, Is.False);
                Assert.That(request.CanTriggerCapstones, Is.False);
                Assert.That(request.CanTriggerStatusEffects, Is.False);
                Assert.That(request.CanTriggerReactions, Is.False);
                Assert.That(request.CanTriggerVulnerability, Is.False);
                Assert.That(request.KnockbackForce, Is.Zero);
                Assert.That(secondaryResults, Has.Count.EqualTo(1),
                    "The secondary traverses EnemyHealth once and cannot recurse into another secondary.");
                Assert.That(secondaryResults[0].FinalDamage, Is.EqualTo(4));
            }
            finally
            {
                GameEventBus.Unsubscribe(onDamage);
            }

            yield return null;
        }

        private void AssertConsumedFirst(EnemyHealth first)
        {
            Assert.That(_state.LunarConsumedFirstTargetInstanceId, Is.EqualTo(first.EnemyInstanceId));
            Assert.That(_state.ActiveLunarTargetInstanceId, Is.Empty);
            Assert.That(_state.HasConsumedLunarFirstTargetForActiveEncounter, Is.True);
        }

        private void BeginEncounter(params EnemyHealth[] enemies)
        {
            Assert.That(enemies, Is.Not.Empty);
            GameEventBus.Publish(new EnemyAggroStartedEvent(enemies[0].EnemyInstanceId, "lunar-pack"));
            var ids = new string[enemies.Length];
            for (int i = 0; i < enemies.Length; i++) ids[i] = enemies[i].EnemyInstanceId;
            GameEventBus.Publish(new EnemyPackAlertedEvent(
                "lunar-pack", enemies[0].transform.position, enemies.Length, ids));
            Assert.That(_state.ActiveEnemyInstanceIds, Is.EquivalentTo(ids));
        }

        private SkillActionSO MarkedPreyAction()
        {
            foreach (var action in DefaultSkillActionCatalog.BuildAll())
            {
                _owned.Add(action);
                if (action.SkillActionId == "skill_ranged_marked_prey") return action;
            }
            Assert.Fail("Canonical Marked Prey action is missing.");
            return null;
        }

        private BowFixture PrepareBow(int arrows, System.Func<float> critRoll = null)
        {
            var bootstrap = GameBootstrap.Instance;
            var itemDatabase = bootstrap.ItemDatabase as ItemDatabaseSO;
            Assert.That(itemDatabase, Is.Not.Null);
            Assert.That(itemDatabase.TryGetById("item_ammo_arrow_wood", out var ammo), Is.True);
            Assert.That(itemDatabase.TryGetById("item_weapon_bow_basic", out var bowItem), Is.True);
            Assert.That(bootstrap.WeaponDatabase.TryGetById(bowItem.WeaponId, out var weapon), Is.True);
            Assert.That(_inventory.AddItem(ammo.Id, arrows), Is.True);
            _equipment.EquipItem(EquipmentSlot.LeftHand, ammo.Id);
            _equipment.EquipItem(EquipmentSlot.RightHand, bowItem.Id);
            _stamina.Initialize(100, 100);
            var resolver = new EquippedItemResolver(
                itemDatabase, bootstrap.WeaponDatabase, bootstrap.SpellDatabase,
                new[] { weapon });
            var stats = new PlayerCombatStatsProvider(
                () => 0, () => null, critRoll: critRoll ?? (() => 1f));
            _statsProviders.Add(stats);
            var service = new BowArrowAttackService(
                _equipment, _inventory, _stamina, itemDatabase, resolver, 0f,
                bootstrap.StatusEffectDatabase)
            {
                StatsProvider = stats,
                SourceCasterRuntimeId = 9193
            };
            return new BowFixture(service, stats, ammo, weapon);
        }

        private EnemyHealth Enemy(string instanceId, Vector2 position, int hp)
        {
            var go = Body(instanceId, position, true);
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = "enemy_lunar_fixture";
            data.DisplayName = instanceId;
            data.maxHp = hp;
            data.defense = 0;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            health.ConfigureLootContext(instanceId, "phase19c-playmode");
            return health;
        }

        private GameObject Body(string name, Vector2 position, bool trigger)
        {
            var go = new GameObject(name);
            _owned.Add(go);
            go.transform.position = position;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Static;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * .35f;
            collider.isTrigger = trigger;
            return go;
        }

        private static DamageRequest ConfirmedDamage(EnemyHealth target, int amount,
            DamageSourceKind kind, DamageType type, string token,
            SpellDiscipline spellDiscipline = SpellDiscipline.None)
            => new DamageRequest(target.EnemyId, amount, type, "player")
            {
                SourceKind = kind,
                SpellDiscipline = spellDiscipline,
                SourceInstanceId = "player",
                ActionToken = token,
                IsPrimaryDamage = true,
                CanTriggerCapstones = true,
                CanTriggerStatusEffects = true,
                CanTriggerReactions = true
            };

        private static void CommitMagic(string token, SpellDiscipline discipline,
            DamageType damageType)
        {
            var preparation = new SpellCastPreparation(new SpellCastPreparationRequest(
                token, token, discipline, 1), 1);
            var transaction = new SpellCastTransaction(preparation);
            Assert.That(transaction.TryReserve(_ => true), Is.True);
            Assert.That(PlayerMagicCastCommit.TryCommit(transaction, 100,
                damageType.ToString()), Is.True);
        }

        private void ApplyObservedSecondaryDamage(string targetInstanceId, DamageRequest request)
        {
            _secondaryRequests.Add(request);
            foreach (var enemy in EnemyHealth.ActiveInstances)
            {
                if (enemy != null && !enemy.IsDead &&
                    enemy.EnemyInstanceId == targetInstanceId)
                {
                    enemy.TakeDamage(request);
                    return;
                }
            }
        }

        private static HashSet<int> ProjectileIds()
        {
            var result = new HashSet<int>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var projectile in root.GetComponentsInChildren<ProjectileBehaviour>())
                    result.Add(projectile.GetEntityId().GetHashCode());
            return result;
        }

        private static ProjectileBehaviour NewProjectile(HashSet<int> before)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var projectile in root.GetComponentsInChildren<ProjectileBehaviour>())
                    if (!before.Contains(projectile.GetEntityId().GetHashCode())) return projectile;
            Assert.Fail("Expected a newly materialized projectile.");
            return null;
        }

        private readonly struct BowFixture
        {
            public BowArrowAttackService Service { get; }
            public PlayerCombatStatsProvider Stats { get; }
            public ItemDataSO Ammo { get; }
            public WeaponDataSO Weapon { get; }

            public BowFixture(BowArrowAttackService service, PlayerCombatStatsProvider stats,
                ItemDataSO ammo, WeaponDataSO weapon)
            {
                Service = service;
                Stats = stats;
                Ammo = ammo;
                Weapon = weapon;
            }
        }
    }
}
