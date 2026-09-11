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
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public class CombatCapstoneFoundationPlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null)
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            PlayerControlResistanceProvider.Source = null;
            foreach (var item in _owned)
                if (item != null) Object.Destroy(item);
            _owned.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Knockback_PlayerMarkerEnablesResistance_EnemyKeepsOriginalForce()
        {
            var previousAccessorySource = AccessoryEffectRouter.KnockbackResistSource;
            var resistance = new CountingControlResistance(.5f);
            PlayerControlResistanceProvider.Source = resistance;
            AccessoryEffectRouter.KnockbackResistSource = () => 0f;

            try
            {
                var player = new GameObject("phase19a-player-knockback");
                _owned.Add(player);
                player.transform.position = new Vector2(6000f, 6000f);
                player.AddComponent<PlayerManager>();
                var playerKnockback = player.AddComponent<KnockbackController>();

                var enemy = new GameObject("phase19a-enemy-knockback");
                _owned.Add(enemy);
                enemy.transform.position = new Vector2(6000f, 6010f);
                var enemyKnockback = enemy.AddComponent<KnockbackController>();

                Assert.That(playerKnockback.AppliesAccessoryResist, Is.True,
                    "PlayerManager is the runtime marker that opts the player into control resistance.");
                Assert.That(enemyKnockback.AppliesAccessoryResist, Is.False,
                    "Enemy-owned knockback must not read player resistance.");

                Vector2 playerBefore = player.transform.position;
                Vector2 enemyBefore = enemy.transform.position;
                playerKnockback.ApplyKnockback(Vector2.right, 10f);
                enemyKnockback.ApplyKnockback(Vector2.right, 10f);
                yield return new WaitForFixedUpdate();

                float playerDistance = ((Vector2)player.transform.position - playerBefore).x;
                float enemyDistance = ((Vector2)enemy.transform.position - enemyBefore).x;
                Assert.That(enemyDistance, Is.GreaterThan(0f));
                Assert.That(playerDistance, Is.EqualTo(enemyDistance * .5f).Within(.01f));
                Assert.That(resistance.KnockbackReads, Is.EqualTo(1),
                    "Only the player path may consult the player control provider.");
            }
            finally
            {
                AccessoryEffectRouter.KnockbackResistSource = previousAccessorySource;
                PlayerControlResistanceProvider.Source = null;
            }
        }

        [UnityTest]
        public IEnumerator BowFire_ResolvesAttackAndCriticalOnceAtImpact_PreservesDefaultHitRules()
        {
            _criticalRolls = 0;
            var bootstrap = GameBootstrap.Instance;
            var inventory = bootstrap.InventoryManager as InventoryManager;
            var equipment = EquipmentManager.Instance;
            var itemDatabase = bootstrap.ItemDatabase as ItemDatabaseSO;
            var weaponDatabase = bootstrap.WeaponDatabase;
            var stamina = bootstrap.StaminaManager as StaminaManager;

            Assert.That(inventory, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            Assert.That(itemDatabase, Is.Not.Null);
            Assert.That(weaponDatabase, Is.Not.Null);
            Assert.That(stamina, Is.Not.Null);
            Assert.That(itemDatabase.TryGetById("item_ammo_arrow_wood", out var ammo), Is.True);
            Assert.That(itemDatabase.TryGetById("item_weapon_bow_basic", out var bowItem), Is.True);
            Assert.That(weaponDatabase.TryGetById(bowItem.WeaponId, out var bow), Is.True);

            var inventorySnapshot = inventory.CaptureSaveData();
            var equipmentSnapshot = equipment.CaptureSaveData();
            DamageAppliedEvent observed = null;
            int relevantDamageEvents = 0;
            System.Action<DamageAppliedEvent> handler = evt =>
            {
                if (evt.DamageResult.SourceKind != DamageSourceKind.PlayerRanged ||
                    evt.DamageResult.TargetInstanceId != "phase19a-primary-bow-target-instance") return;
                observed = evt;
                relevantDamageEvents++;
            };
            GameEventBus.Subscribe(handler);

            using var stats = new PlayerCombatStatsProvider(
                baseAttackSource: () => 7,
                passivesSource: () => null,
                critRoll: () =>
                {
                    _criticalRolls++;
                    return 1f;
                });

            try
            {
                Assert.That(inventory.AddItem(ammo.Id, 2), Is.True);
                equipment.EquipItem(EquipmentSlot.LeftHand, ammo.Id);
                equipment.EquipItem(EquipmentSlot.RightHand, bowItem.Id);
                stamina.Initialize(100, 100);

                var resolver = new EquippedItemResolver(itemDatabase, weaponDatabase,
                    bootstrap.SpellDatabase, new[] { bow });
                var service = new BowArrowAttackService(equipment, inventory, stamina,
                    itemDatabase, resolver, 0f, bootstrap.StatusEffectDatabase)
                {
                    StatsProvider = stats,
                    SourceCasterRuntimeId = 7001
                };

                Vector2 origin = new Vector2(6100f, 6100f);
                var primary = Enemy("phase19a-primary-bow-target", origin + Vector2.right * 2f, 100);
                var behind = Enemy("phase19a-behind-bow-target", origin + Vector2.right * 3.5f, 100);
                Physics2D.SyncTransforms();

                var beforeProjectiles = SceneProjectileIds();
                int ammoBefore = inventory.GetAmount(ammo.Id);
                var fired = service.TryFire(EquipmentSlot.LeftHand, ammo, -1000f,
                    Vector2.right, origin);
                Assert.That(fired.Success, Is.True);
                Assert.That(inventory.GetAmount(ammo.Id), Is.EqualTo(ammoBefore - 1));
                TrackNewProjectiles(beforeProjectiles);

                for (int i = 0; i < 30 && observed == null; i++)
                    yield return new WaitForFixedUpdate();

                int expectedDamage = bow.BaseDamage
                    + ArrowBallisticsResolver.Resolve(ammo).ArrowDamage
                    + stats.Current.Attack;
                Assert.That(observed, Is.Not.Null, "The real projectile must reach EnemyHealth.");
                Assert.That(relevantDamageEvents, Is.EqualTo(1));
                Assert.That(_criticalRolls, Is.EqualTo(1),
                    "Critical resolution belongs to the single confirmed impact.");
                Assert.That(observed.DamageResult.FinalDamage, Is.EqualTo(expectedDamage),
                    "Current.Attack must still participate in the impact damage.");
                Assert.That(observed.DamageResult.SourceKind, Is.EqualTo(DamageSourceKind.PlayerRanged));
                Assert.That(observed.DamageResult.SourceInstanceId, Is.EqualTo("7001"));
                Assert.That(observed.DamageResult.ActionToken, Is.Not.Empty);
                Assert.That(observed.DamageResult.IsPrimaryDamage, Is.True);
                Assert.That(observed.DamageResult.CanTriggerCapstones, Is.True);
                Assert.That(observed.DamageResult.IsCritical, Is.False);
                Assert.That(primary.CurrentHp, Is.EqualTo(100 - expectedDamage));
                Assert.That(behind.CurrentHp, Is.EqualTo(100),
                    "The ordinary bow projectile keeps its one-target non-piercing behavior.");
                Assert.That(primary.StatusEffects.GetActiveEffects(), Is.Empty,
                    "Wooden arrows keep their status-free payload.");

                int projectilesBeforeCooldownAttempt = SceneProjectileIds().Count;
                int ammoBeforeCooldownAttempt = inventory.GetAmount(ammo.Id);
                var cooldownBlocked = service.TryFire(EquipmentSlot.LeftHand, ammo, Time.time,
                    Vector2.right, origin);
                Assert.That(cooldownBlocked.Success, Is.False);
                Assert.That(cooldownBlocked.ErrorCode, Is.EqualTo("Cooldown"));
                Assert.That(inventory.GetAmount(ammo.Id), Is.EqualTo(ammoBeforeCooldownAttempt));
                Assert.That(SceneProjectileIds().Count, Is.EqualTo(projectilesBeforeCooldownAttempt));
                Assert.That(_criticalRolls, Is.EqualTo(1));
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
                inventory.RestoreFromSaveData(inventorySnapshot);
                equipment.RestoreFromSaveData(equipmentSnapshot);
            }
        }

        private int _criticalRolls;

        private EnemyHealth Enemy(string name, Vector2 position, int hp)
        {
            var go = new GameObject(name);
            _owned.Add(go);
            go.transform.position = position;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Static;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * .4f;
            collider.isTrigger = true;
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = name;
            data.maxHp = hp;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            health.ConfigureLootContext(name + "-instance", string.Empty);
            return health;
        }

        private static HashSet<int> SceneProjectileIds()
        {
            var result = new HashSet<int>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var projectile in root.GetComponentsInChildren<ProjectileBehaviour>())
                    result.Add(projectile.GetEntityId().GetHashCode());
            return result;
        }

        private void TrackNewProjectiles(HashSet<int> before)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var projectile in root.GetComponentsInChildren<ProjectileBehaviour>())
                    if (!before.Contains(projectile.GetEntityId().GetHashCode())) _owned.Add(projectile.gameObject);
        }

        private sealed class CountingControlResistance : IPlayerControlResistanceSource
        {
            private readonly float _knockback;
            public int KnockbackReads { get; private set; }
            public float KnockbackReductionFraction
            {
                get
                {
                    KnockbackReads++;
                    return _knockback;
                }
            }
            public float StunDurationReductionFraction => 0f;

            public CountingControlResistance(float knockback)
            {
                _knockback = knockback;
            }
        }
    }
}
