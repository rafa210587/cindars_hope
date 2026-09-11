using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public class SkillRangedIdentityPlayModeTests
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
            foreach (var item in _owned)
                if (item != null) Object.Destroy(item);
            _owned.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChargedShot_EarlyReleaseAndDamageCancelCostNothing_MaxReleasePaysBeforeSpawn()
        {
            var skills = SkillTreeManager.Instance;
            var save = new SkillTreeSaveData();
            save.NodeRanks.Add(new SkillNodeRankEntry("ranged_charged_shot", 1));
            skills.RestoreFromSaveData(save, 100);
            Assert.That(skills.TryAssignActiveSlot(0, "skill_ranged_charged_shot"), Is.True);
            var controller = ActiveSkillExecutionController.Instance;
            var stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            stamina.Initialize(100, 100);
            int before = SceneProjectiles().Count;

            controller.BeginChargedSlot(0);
            yield return new WaitForSeconds(.10f);
            controller.ReleaseChargedSlot();
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);
            Assert.That(SceneProjectiles().Count, Is.EqualTo(before));

            controller.BeginChargedSlot(0);
            yield return new WaitForSeconds(.10f);
            GameEventBus.Publish(new PlayerDamagedEvent(1, Vector3.zero, "fixture"));
            Assert.That(controller.IsChargingSkill, Is.False);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));

            string failureKey = string.Empty;
            System.Action<PlayerActionFeedbackEvent> feedback = evt => failureKey = evt.FailureKey;
            var previousBlockProvider = ActionBlockProvider.IsActionBlocked;
            GameEventBus.Subscribe(feedback);
            try
            {
                controller.BeginChargedSlot(0);
                ActionBlockProvider.IsActionBlocked = () => true;
                yield return null;
                Assert.That(controller.IsChargingSkill, Is.False,
                    "A standalone stagger/action block must cancel charge even without damage.");
                Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
                Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);
                Assert.That(failureKey, Is.EqualTo("ActionBlocked"));

                ActionBlockProvider.IsActionBlocked = previousBlockProvider;
                stamina.Initialize(100, 0);
                failureKey = string.Empty;
                controller.BeginChargedSlot(0);
                Assert.That(controller.IsChargingSkill, Is.False);
                Assert.That(failureKey, Is.EqualTo("InsufficientStamina"));
            }
            finally
            {
                ActionBlockProvider.IsActionBlocked = previousBlockProvider;
                GameEventBus.Unsubscribe(feedback);
            }

            stamina.Initialize(100, 100);
            controller.BeginChargedSlot(0);
            yield return new WaitForSeconds(1.15f);
            Assert.That(SceneProjectiles().Count, Is.EqualTo(before));
            controller.ReleaseChargedSlot();
            Assert.That(stamina.CurrentStamina, Is.EqualTo(74));
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.GreaterThan(0));
            var after = SceneProjectiles();
            Assert.That(after.Count, Is.EqualTo(before + 1));
            foreach (var projectile in after) _owned.Add(projectile.gameObject);
        }

        [UnityTest]
        public IEnumerator LinePiercer_AppliesFiveStepFalloffOncePerTarget_AndStopsAtObstacle()
        {
            var origin = new Vector2(4200, 4200);
            var targets = new List<EnemyHealth>();
            for (int i = 0; i < 5; i++)
                targets.Add(Enemy($"Line target {i}", origin + Vector2.right * (1f + i * .7f), 200, i == 0));
            Physics2D.SyncTransforms();
            var request = Request(origin, 100, 12f, 8f);
            request.MaxHits = 5;
            request.StopOnSolidObstacle = true;
            request.HitPolicy = new RangedProjectileHitPolicy(RangedProjectilePolicyKind.LinePiercer);
            var spawned = ProjectileSpawnService.SpawnProjectile(request);
            Assert.That(spawned.Success, Is.True);
            _owned.Add(spawned.Projectile);
            for (int i = 0; i < 40; i++) yield return new WaitForFixedUpdate();
            var expectedHp = new[] { 100, 120, 136, 149, 159 };
            for (int i = 0; i < 5; i++) Assert.That(targets[i].CurrentHp, Is.EqualTo(expectedHp[i]), $"target {i}");

            var wall = Body("Line wall", origin + Vector2.up * 3f + Vector2.right, false);
            wall.GetComponent<BoxCollider2D>().size = new Vector2(.2f, 2f);
            var behind = Enemy("Behind line wall", origin + Vector2.up * 3f + Vector2.right * 2f, 100, false);
            var blocked = Request(origin + Vector2.up * 3f, 20, 12f, 5f);
            blocked.StopOnSolidObstacle = true;
            blocked.HitPolicy = new RangedProjectileHitPolicy(RangedProjectilePolicyKind.LinePiercer);
            var blockedSpawn = ProjectileSpawnService.SpawnProjectile(blocked);
            _owned.Add(blockedSpawn.Projectile);
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(behind.CurrentHp, Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator TripleFanAndBleed_EnforceOverlapAndRefreshContracts()
        {
            var origin = new Vector2(4300, 4300);
            var target = Enemy("Fan and bleed target", origin + Vector2.right * 1.5f, 100, false);
            Physics2D.SyncTransforms();
            var fanPolicy = new RangedProjectileHitPolicy(RangedProjectilePolicyKind.TripleFan);
            for (int i = 0; i < 3; i++)
            {
                var request = Request(origin, 10, 10f, 4f);
                request.HitPolicy = fanPolicy;
                var spawn = ProjectileSpawnService.SpawnProjectile(request);
                _owned.Add(spawn.Projectile);
            }
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHp, Is.EqualTo(85));

            var bleed = ScriptableObject.CreateInstance<StatusEffectSO>();
            _owned.Add(bleed);
            bleed.Id = "status_bleed_fixture";
            bleed.DurationTurns = 10;
            bleed.DamagePerTurn = 2;
            int refreshes = 0;
            System.Action<StatusRefreshedEvent> handler = evt =>
            {
                if (evt.StatusId == bleed.Id) refreshes++;
            };
            GameEventBus.Subscribe(handler);
            try
            {
                for (int shot = 0; shot < 2; shot++)
                {
                    var request = Request(origin, 1, 10f, 4f);
                    request.StatusEffect = bleed;
                    request.StatusApplyChance = 1f;
                    var spawn = ProjectileSpawnService.SpawnProjectile(request);
                    _owned.Add(spawn.Projectile);
                    for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
                }
                int activeBleeds = target.StatusEffects.GetActiveEffects()
                    .FindAll(item => item.StatusEffectId == bleed.Id).Count;
                Assert.That(activeBleeds, Is.EqualTo(1));
                Assert.That(refreshes, Is.EqualTo(1));
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
            }
        }

        [UnityTest]
        public IEnumerator MarkedPrey_ReapplyMovesSingleMarkAndSpendsOncePerCommit()
        {
            var caster = Body("Mark caster", new Vector2(4400, 4400), false);
            var firstTarget = Enemy("First marked prey", new Vector2(4401, 4400), 100, false);
            var secondTarget = Enemy("Second marked prey", new Vector2(4403, 4400), 100, false);
            var actions = DefaultSkillActionCatalog.BuildAll();
            SkillActionSO action = null;
            foreach (var candidate in actions)
            {
                _owned.Add(candidate);
                if (candidate.SkillActionId == "skill_ranged_marked_prey") action = candidate;
            }
            Assert.That(action, Is.Not.Null);
            var stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            stamina.Initialize(100, 100);
            var executor = new MarkedPreySkillEffectExecutor(action);
            var context = new SkillEffectContext
            {
                SkillActionId = action.SkillActionId,
                Caster = caster,
                ActionData = action,
                Rank = 2
            };

            Assert.That(executor.Execute(context).Success, Is.True);
            Assert.That(firstTarget.GetComponent<MarkedPreyState>().IsRevealed, Is.True);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(90));
            firstTarget.transform.position = new Vector2(4410, 4400);
            secondTarget.transform.position = new Vector2(4401, 4400);
            Assert.That(executor.Execute(context).Success, Is.True);
            Assert.That(firstTarget.GetComponent<MarkedPreyState>().IsRevealed, Is.False);
            Assert.That(secondTarget.GetComponent<MarkedPreyState>().IsRevealed, Is.True);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(80));

            firstTarget.transform.position = new Vector2(4408, 4400);
            secondTarget.transform.position = new Vector2(4409, 4400);
            Assert.That(executor.Execute(context).Success, Is.True, "The authored 8-tile boundary is inclusive.");
            Assert.That(firstTarget.GetComponent<MarkedPreyState>().IsRevealed, Is.True);
            Assert.That(secondTarget.GetComponent<MarkedPreyState>().IsRevealed, Is.False);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(70));

            firstTarget.transform.position = new Vector2(4408.1f, 4400);
            secondTarget.transform.position = new Vector2(4409, 4400);
            var outOfRange = executor.Execute(context);
            Assert.That(outOfRange.Success, Is.False);
            Assert.That(outOfRange.FailureReason, Is.EqualTo("NoTarget"));
            Assert.That(stamina.CurrentStamina, Is.EqualTo(70), "A target failure cannot spend stamina.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator MarkedPrey_BoostsAnyRangedProjectileFromOwningCasterOnly()
        {
            var origin = new Vector2(4450, 4450);
            var target = Enemy("Marked bow target", origin + Vector2.right * 1.5f, 100, false);
            var mark = target.gameObject.AddComponent<MarkedPreyState>();
            mark.Apply(101, 10f, .16f);
            Physics2D.SyncTransforms();

            var owningShot = Request(origin, 25, 10f, 4f);
            owningShot.VisualStyle = ProjectileVisualStyle.Arrow;
            owningShot.SourceCasterRuntimeId = 101;
            var owningSpawn = ProjectileSpawnService.SpawnProjectile(owningShot);
            Assert.That(owningSpawn.Success, Is.True);
            _owned.Add(owningSpawn.Projectile);
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHp, Is.EqualTo(71), "25 × 1.16 must round to 29 damage.");

            var foreignShot = Request(origin, 10, 10f, 4f);
            foreignShot.VisualStyle = ProjectileVisualStyle.Arrow;
            foreignShot.SourceCasterRuntimeId = 202;
            var foreignSpawn = ProjectileSpawnService.SpawnProjectile(foreignShot);
            Assert.That(foreignSpawn.Success, Is.True);
            _owned.Add(foreignSpawn.Projectile);
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHp, Is.EqualTo(61), "A different caster must receive no marked bonus.");
        }

        private ProjectileSpawnRequest Request(Vector2 origin, int damage, float speed, float range)
        {
            return new ProjectileSpawnRequest(null, origin, Vector2.right, speed, range, damage,
                DamageType.Physical, 0f, spawnOffset: 0f)
            {
                VisualStyle = ProjectileVisualStyle.SkillBolt,
                MaxHits = 1
            };
        }

        private EnemyHealth Enemy(string name, Vector2 position, int hp, bool secondCollider)
        {
            var go = Body(name, position, true);
            if (secondCollider)
            {
                var child = new GameObject("Second collider");
                child.transform.SetParent(go.transform, false);
                child.AddComponent<CircleCollider2D>().isTrigger = true;
            }
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = name;
            data.maxHp = hp;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            return health;
        }

        private GameObject Body(string name, Vector2 position, bool trigger)
        {
            var go = new GameObject(name);
            _owned.Add(go);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Static;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * .35f;
            collider.isTrigger = trigger;
            return go;
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
