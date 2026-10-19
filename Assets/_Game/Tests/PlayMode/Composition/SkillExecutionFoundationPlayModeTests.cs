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
using CindarsHope.Player.Movement;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public class SkillExecutionFoundationPlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private PlayerController _temporarilyDisabledAvatar;

        [UnitySetUp] public IEnumerator PrepareGameplay()
        {
            // Runtime services require the game's bootstrap, even in isolated physics fixtures.
            if (GameBootstrap.Instance == null)
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = 1f;
            foreach (var item in _owned) if (item != null) Object.Destroy(item);
            _owned.Clear();
            yield return null;
            if (_temporarilyDisabledAvatar != null) _temporarilyDisabledAvatar.gameObject.SetActive(true);
            _temporarilyDisabledAvatar = null;
        }

        private GameObject Body(string name, Vector2 position)
        {
            var go = new GameObject(name); _owned.Add(go);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; rb.freezeRotation = true;
            go.AddComponent<BoxCollider2D>().size = Vector2.one * .4f;
            return go;
        }

        [UnityTest] public IEnumerator Displacement_StopsAtWall_ThenDisableReleasesOwnership()
        {
            var body = Body("Lunge fixture", new Vector2(2000, 2000));
            _temporarilyDisabledAvatar = PlayerController.ActiveInstance;
            if (_temporarilyDisabledAvatar != null) _temporarilyDisabledAvatar.gameObject.SetActive(false);
            var player = body.AddComponent<PlayerController>();
            var resolver = body.AddComponent<PlayerMovementDisplacementResolver>();
            var wall = new GameObject("Wall"); _owned.Add(wall);
            wall.transform.position = body.transform.position + Vector3.right;
            wall.AddComponent<BoxCollider2D>().size = new Vector2(.2f, 3f);
            Physics2D.SyncTransforms();
            bool completed = false;
            Assert.That(resolver.TryDisplace(Vector2.right, 3, .1f, () => completed = true), Is.True);
            Assert.That(player.IsBeingDisplaced, Is.True);
            Assert.That(player.SpeedComposer.HasFactor(SpeedFactorKind.Displacement), Is.True);
            for (int i = 0; i < 15; i++) yield return new WaitForFixedUpdate();
            Assert.That(completed, Is.True);
            Assert.That(body.GetComponent<Rigidbody2D>().position.x, Is.InRange(2000.5f, 2000.71f));
            Assert.That(resolver.IsDisplacing, Is.False);
            Assert.That(player.IsBeingDisplaced, Is.False);
            Assert.That(player.SpeedComposer.HasFactor(SpeedFactorKind.Displacement), Is.False);
            completed = false;
            Assert.That(resolver.TryDisplace(Vector2.left, 2, 1, () => completed = true), Is.True);
            yield return new WaitForFixedUpdate();
            resolver.enabled = false;
            Assert.That(resolver.IsDisplacing, Is.False);
            Assert.That(player.IsBeingDisplaced, Is.False);
            Assert.That(player.SpeedComposer.HasFactor(SpeedFactorKind.Displacement), Is.False);
            var stoppedAt = body.GetComponent<Rigidbody2D>().position;
            for (int i = 0; i < 3; i++) yield return new WaitForFixedUpdate();
            // A pending MovePosition may commit once; it must not continue the full displacement.
            Assert.That(Vector2.Distance(stoppedAt, body.GetComponent<Rigidbody2D>().position), Is.LessThan(.1f));
            Assert.That(completed, Is.False);
            resolver.enabled = true;
            Assert.That(resolver.TryDisplace(Vector2.left, .2f, .1f, null), Is.True);
        }

        [UnityTest] public IEnumerator Lunge_HitsOnlyAfterPhysicsMovement()
        {
            var body = Body("Lunge caster", new Vector2(2100, 2100));
            body.AddComponent<PlayerMovementDisplacementResolver>();
            var target = Body("Lunge receiver", new Vector2(2101.6f, 2100));
            target.GetComponent<Collider2D>().isTrigger = true;
            var data = ScriptableObject.CreateInstance<EnemyDataSO>(); _owned.Add(data);
            data.enemyId = "lunge_fixture"; data.maxHp = 100;
            var health = target.AddComponent<EnemyHealth>(); health.Configure(data);
            Physics2D.SyncTransforms();
            var executor = new MeleeStrikeSkillEffectExecutor("fixture", "fixture", 10, .7f, 360, 0, 1, 0);
            Assert.That(executor.Execute(new SkillEffectContext { Caster = body }).Success, Is.True);
            Assert.That(health.CurrentHp, Is.EqualTo(100), "The hit must not precede the lunge.");
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(body.GetComponent<Rigidbody2D>().position.x, Is.GreaterThan(2100.8f));
            Assert.That(health.CurrentHp, Is.EqualTo(90));
        }

        [UnityTest] public IEnumerator WhirlCut_RankThree_HitsSixOfSeven_WithFalloffAfterThird()
        {
            var caster = Body("Whirl caster", new Vector2(2150, 2150));
            var action = DefaultSkillActionCatalog.BuildAll().Find(candidate =>
                candidate.SkillActionId == "skill_melee_whirl_cut");
            Assert.That(action, Is.Not.Null);
            _owned.Add(action);

            var stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            stamina.Initialize(100, 100);
            var targets = new List<EnemyHealth>();
            for (int i = 0; i < 7; i++)
            {
                // Same species, distance and position force the runtime to use the stable instance id.
                var target = Body($"Whirl receiver {i}", (Vector2)caster.transform.position + Vector2.right * 0.5f);
                var data = ScriptableObject.CreateInstance<EnemyDataSO>();
                _owned.Add(data);
                data.enemyId = "whirl_fixture_species";
                data.maxHp = 100;
                var health = target.AddComponent<EnemyHealth>();
                health.Configure(data);
                health.ConfigureLootContext($"whirl_instance_{i:D2}", "fixture_run");
                targets.Add(health);
            }

            Physics2D.SyncTransforms();
            var executor = new MeleeStrikeSkillEffectExecutor("fixture.whirl", "Corte Giratório", action);
            var result = executor.Execute(new SkillEffectContext
            {
                Caster = caster,
                ActionData = action,
                Rank = 3
            });

            Assert.That(result.Success, Is.True, result.FailureReason);
            for (int i = 0; i < 3; i++) Assert.That(targets[i].CurrentHp, Is.EqualTo(86), $"target {i}");
            for (int i = 3; i < 6; i++) Assert.That(targets[i].CurrentHp, Is.EqualTo(90), $"target {i}");
            Assert.That(targets[6].CurrentHp, Is.EqualTo(100), "The seventh target must remain untouched.");
            yield return null;
        }

        [UnityTest] public IEnumerator Projectile_RealCollision_AppliesGuaranteedStatusAndRejectsZeroChance()
        {
            for (int chance = 0; chance <= 1; chance++)
            {
                var position = new Vector2(2200, 2200 + chance * 5);
                var target = Body("Status receiver", position + Vector2.right);
                var data = ScriptableObject.CreateInstance<EnemyDataSO>(); _owned.Add(data);
                data.enemyId = "status_fixture_" + chance; data.maxHp = 100;
                var health = target.AddComponent<EnemyHealth>(); health.Configure(data);
                var status = ScriptableObject.CreateInstance<StatusEffectSO>(); _owned.Add(status);
                status.Id = "fixture_status"; status.DurationTurns = 10;
                Physics2D.SyncTransforms();
                var result = ProjectileSpawnService.SpawnProjectile(new ProjectileSpawnRequest(
                    null, position, Vector2.right, 5, 4, 1, DamageType.Physical, 0,
                    statusEffect: status, statusApplyChance: chance));
                Assert.That(result.Success, Is.True); _owned.Add(result.Projectile);
                for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
                Assert.That(health.CurrentHp, Is.LessThan(100), "A real collision must occur in both cases.");
                Assert.That(health.StatusEffects.HasStatusEffect(status.Id), Is.EqualTo(chance == 1));
            }
        }

        [UnityTest] public IEnumerator ProjectileExecutor_UsesAuthoredStaminaAndPreservesMana()
        {
            var caster = Body("Bomb caster", new Vector2(2250, 2250));
            var action = DefaultSkillActionCatalog.BuildAll().Find(candidate =>
                candidate.SkillActionId == "skill_crafting_bomba_improvisada");
            Assert.That(action, Is.Not.Null);
            _owned.Add(action);
            action.StaminaCost = 20;
            var stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            var mana = (ManaManager)GameBootstrap.Instance.ManaManager;
            stamina.Initialize(100, 100);
            mana.SetMana(100);
            var before = SceneProjectiles();

            var executor = new ProjectileSkillEffectExecutor("fixture.bomb", "Bomba Improvisada", action);
            var result = executor.Execute(new SkillEffectContext
            {
                Caster = caster,
                WorldPosition = caster.transform.position,
                ActionData = action,
                Rank = 1
            });

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(80));
            Assert.That(mana.CurrentMana, Is.EqualTo(100));
            foreach (var projectile in SceneProjectiles())
                if (!before.Contains(projectile)) _owned.Add(projectile.gameObject);
            yield return null;
        }

        [UnityTest] public IEnumerator DormantEquippedAction_ConsumesNothingAndStartsNoCooldown()
        {
            var skills = SkillTreeManager.Instance;
            var controller = ActiveSkillExecutionController.Instance;
            Assert.That(skills, Is.Not.Null);
            Assert.That(controller, Is.Not.Null);
            var fixture = new SkillTreeSaveData();
            fixture.NodeRanks.Add(new SkillNodeRankEntry("ranged_marked_prey", 1));
            skills.RestoreFromSaveData(fixture, 100);
            Assert.That(skills.TryAssignActiveSlot(0, "skill_ranged_marked_prey"), Is.True);
            var stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            var mana = (ManaManager)GameBootstrap.Instance.ManaManager;
            stamina.Initialize(100, 100);
            mana.SetMana(100);
            int projectileCount = SceneProjectiles().Count;

            controller.TryUseSlot(0);

            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            Assert.That(mana.CurrentMana, Is.EqualTo(100));
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);
            Assert.That(SceneProjectiles().Count, Is.EqualTo(projectileCount));
            yield return null;
        }

        [UnityTest] public IEnumerator CatalogStatusEffects_ApplyAllFourUsingSceneDatabaseAndRealCollision()
        {
            var effects = new[] { "combat.ranged.bleeding_arrow", "combat.magic.ice_bind", "combat.magic.toxic_cloud", "magic.rajada_gelida" };
            var statuses = new[] { "status_bleed", "status_chill", "status_poison", "status_chill" };
            var registry = ActiveSkillExecutorCatalog.CreateRegistry();
            var mana = (ManaManager)GameBootstrap.Instance.ManaManager;
            var stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            for (int i = 0; i < effects.Length; i++)
            {
                mana.SetMana(mana.MaxMana); stamina.Initialize(100, 100);
                var caster = Body("Catalog caster", new Vector2(2300, 2300 + i * 5));
                var target = Body("Catalog receiver", (Vector2)caster.transform.position + Vector2.right * 1.5f);
                var data = ScriptableObject.CreateInstance<EnemyDataSO>(); _owned.Add(data);
                data.enemyId = "catalog_fixture_" + i; data.maxHp = 100;
                var health = target.AddComponent<EnemyHealth>(); health.Configure(data);
                Physics2D.SyncTransforms();
                var before = SceneProjectiles();
                var result = registry.Resolve(effects[i]).Execute(new SkillEffectContext
                    { Caster = caster, Target = target, WorldPosition = caster.transform.position });
                Assert.That(result.Success, Is.True, effects[i] + ": " + result.FailureReason);
                foreach (var projectile in SceneProjectiles()) if (!before.Contains(projectile)) _owned.Add(projectile.gameObject);
                int frames = effects[i] == "combat.magic.toxic_cloud" ? 70 : 20;
                for (int frame = 0; frame < frames; frame++) yield return new WaitForFixedUpdate();
                Assert.That(health.CurrentHp, Is.LessThan(100), effects[i]);
                Assert.That(health.StatusEffects.HasStatusEffect(statuses[i]), Is.True, effects[i]);
            }
        }

        [UnityTest] public IEnumerator GameplayScenes_BindAvatarAndRefreshAfterSameSceneReplacement()
        {
            foreach (string scene in new[] { "FarmScene", "TownScene", "CaveScene" })
            {
                yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
                yield return null;
                var avatar = PlayerController.ActiveInstance;
                var controller = ActiveSkillExecutionController.Instance;
                Assert.That(avatar, Is.Not.Null, scene);
                Assert.That(controller, Is.Not.Null, scene);
                Assert.That(controller.BoundAvatar, Is.SameAs(avatar.gameObject), scene);
                Assert.That(GameBootstrap.Instance.PlayerManager.gameObject, Is.Not.SameAs(avatar.gameObject));
                var skills = SkillTreeManager.Instance;
                var skillFixture = new SkillTreeSaveData();
                skillFixture.NodeRanks.Add(new SkillNodeRankEntry("magic_fire_spark", 1));
                skillFixture.NodeRanks.Add(new SkillNodeRankEntry("magic_ice_bind", 1));
                skills.RestoreFromSaveData(skillFixture, 100);
                Assert.That(skills.TryAssignActiveSlot(0, "magic_fire_spark"), Is.True); // legacy node alias
                Assert.That(skills.TryAssignActiveSlot(1, "skill_magic_fire_spark"), Is.True);
                Assert.That(skills.TryAssignActiveSlot(2, "skill_magic_ice_bind"), Is.True);
                yield return new WaitForSeconds(3.1f); // prior scene's shared cooldown must expire naturally
                var mana = (ManaManager)GameBootstrap.Instance.ManaManager;
                mana.SetMana(0);
                int countBefore = SceneProjectiles().Count;
                controller.TryUseSlot(0);
                Assert.That(mana.CurrentMana, Is.Zero);
                Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);
                Assert.That(SceneProjectiles().Count, Is.EqualTo(countBefore));
                mana.SetMana(100);
                int fullMana = mana.CurrentMana;
                var existing = SceneProjectiles();
                controller.TryUseSlot(0);
                yield return new WaitForSeconds(.25f);
                Assert.That(mana.CurrentMana, Is.EqualTo(fullMana - 10));
                var afterCast = SceneProjectiles();
                Assert.That(afterCast.Count, Is.EqualTo(existing.Count + 1));
                var projectile = afterCast.Find(p => !existing.Contains(p));
                Assert.That(Vector2.Distance(projectile.transform.position, avatar.transform.position),
                    Is.GreaterThanOrEqualTo(.5f).And.LessThan(3f));
                CaptureIfRequested(scene);
                Assert.That(controller.GetSlotCooldownRemaining(1), Is.EqualTo(controller.GetSlotCooldownRemaining(0)).Within(.001f));
                Assert.That(controller.GetSlotCooldownRemaining(2), Is.Zero);
                controller.TryUseSlot(1);
                Assert.That(mana.CurrentMana, Is.EqualTo(fullMana - 10));
                Assert.That(SceneProjectiles().Count, Is.EqualTo(afterCast.Count));
                skills.TryClearActiveSlot(0);
                Assert.That(skills.TryAssignActiveSlot(3, "magic_fire_spark"), Is.True);
                Assert.That(controller.GetSlotCooldownRemaining(3), Is.GreaterThan(0));
                var modal = GameBootstrap.Instance.ModalManager;
                Assert.That(modal.PushModal(ModalType.SkillTree), Is.True);
                float beforeModalWait = controller.GetSlotCooldownRemaining(3);
                yield return new WaitForSeconds(.1f);
                Assert.That(controller.GetSlotCooldownRemaining(3), Is.LessThan(beforeModalWait));
                int beforeBlockedClick = mana.CurrentMana;
                controller.TryUseSlot(2);
                Assert.That(mana.CurrentMana, Is.EqualTo(beforeBlockedClick));
                Assert.That(controller.GetSlotCooldownRemaining(2), Is.Zero);
                Time.timeScale = 0;
                float paused = controller.GetSlotCooldownRemaining(3);
                for (int i = 0; i < 3; i++) yield return null;
                Assert.That(controller.GetSlotCooldownRemaining(3), Is.EqualTo(paused));
                Time.timeScale = 1;
                modal.TryPopModal(ModalType.SkillTree, out _);
                // Empty slot still exercises the normal pre-execution avatar refresh.
                SkillTreeManager.Instance.TryClearActiveSlot(3);
                var replacement = Body("Replacement avatar fixture", (Vector2)avatar.transform.position);
                avatar.gameObject.SetActive(false);
                replacement.AddComponent<PlayerController>();
                controller.TryUseSlot(3);
                Assert.That(controller.BoundAvatar, Is.SameAs(replacement));
                replacement.SetActive(false);
                controller.TryUseSlot(3);
                Assert.That(controller.BoundAvatar, Is.Null);
                avatar.gameObject.SetActive(true);
                controller.TryUseSlot(3);
                Assert.That(controller.BoundAvatar, Is.SameAs(avatar.gameObject));
                controller.enabled = false;
                Assert.That(controller.BoundAvatar, Is.Null);
                var substitute = ActiveSkillExecutionController.Install(); _owned.Add(substitute.gameObject);
                controller.enabled = true;
                Assert.That(controller.enabled, Is.False, "An installed replacement must remain the only active controller.");
                Assert.That(ActiveSkillExecutionController.Instance, Is.SameAs(substitute));
                substitute.enabled = false;
                controller.enabled = true;
                Assert.That(ActiveSkillExecutionController.Instance, Is.SameAs(controller));
                controller.TryUseSlot(3);
                Assert.That(controller.BoundAvatar, Is.SameAs(avatar.gameObject));
                Object.Destroy(replacement);
            }
        }

        [UnityTest, Order(78)]
        public IEnumerator MeleeValidate_IsReadOnlyBeforeTimedCommit()
        {
            var caster = Body("Validate-only caster", new Vector2(3100, 3100));
            var target = Body("Validate-only target", new Vector2(3100.5f, 3100));
            target.GetComponent<Collider2D>().isTrigger = true;
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = "validate_only_fixture";
            data.maxHp = 100;
            var health = target.AddComponent<EnemyHealth>();
            health.Configure(data);
            var actions = DefaultSkillActionCatalog.BuildAll();
            foreach (var candidate in actions) _owned.Add(candidate);
            var action = actions.Find(candidate => candidate.SkillActionId == "skill_melee_whirl_cut");
            var stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            stamina.Initialize(100, 100);
            var executor = new MeleeStrikeSkillEffectExecutor("fixture.validate", "Validate", action);
            var context = new SkillEffectContext
            {
                SkillActionId = action.SkillActionId,
                Caster = caster,
                ActionData = action,
                Rank = 1
            };
            Physics2D.SyncTransforms();

            Assert.That(executor.Validate(context).Success, Is.True);
            Assert.That(executor.Validate(context).Success, Is.True);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            Assert.That(health.CurrentHp, Is.EqualTo(100));
            yield return null;
        }

        [UnityTest, Order(79)]
        public IEnumerator ModalDuringWindup_CancelsWithoutCommit()
        {
            var controller = PrepareTimedWhirl(out var stamina);
            controller.TryUseSlot(0);
            Assert.That(GameBootstrap.Instance.ModalManager.PushModal(ModalType.SkillTree), Is.True);
            yield return null;
            Assert.That(controller.HasActiveCast, Is.False);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);
            GameBootstrap.Instance.ModalManager.TryPopModal(ModalType.SkillTree, out _);
        }

        [UnityTest, Order(80)]
        public IEnumerator DamageDuringWindup_CancelsWithoutCommit()
        {
            var controller = PrepareTimedWhirl(out var stamina);
            controller.TryUseSlot(0);
            Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Windup));
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));

            GameEventBus.Publish(new PlayerDamagedEvent(1, Vector3.zero, "fixture"));

            Assert.That(controller.HasActiveCast, Is.False);
            Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Idle));
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);
            yield return null;
        }

        [UnityTest, Order(81)]
        public IEnumerator SceneChangeDuringWindup_CancelsWithoutCommit()
        {
            var controller = PrepareTimedWhirl(out var stamina);
            Assert.That(SkillTreeManager.Instance.TryResolveActionData("melee_whirl_cut", out var action, out _, out _), Is.True);
            float originalWindup = action.WindupSeconds;
            action.WindupSeconds = 5f;
            try
            {
                controller.TryUseSlot(0);
                Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Windup));

                yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
                yield return null;

                controller = ActiveSkillExecutionController.Instance;
                Assert.That(controller, Is.Not.Null);
                Assert.That(controller.HasActiveCast, Is.False);
                Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Idle));
                Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
                Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);
            }
            finally
            {
                action.WindupSeconds = originalWindup;
            }
        }

        [UnityTest, Order(82)]
        public IEnumerator InputDuringRecovery_DoesNotStartSecondSkill()
        {
            var controller = PrepareTimedWhirl(out var stamina);
            controller.TryUseSlot(0);
            yield return new WaitForSeconds(.42f);
            Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Recovery));
            Assert.That(stamina.CurrentStamina, Is.EqualTo(78));
            float cooldown = controller.GetSlotCooldownRemaining(0);

            controller.TryUseSlot(0);

            Assert.That(stamina.CurrentStamina, Is.EqualTo(78));
            Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Recovery));
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.LessThanOrEqualTo(cooldown));
            GameEventBus.Publish(new PlayerDamagedEvent(1, Vector3.zero, "fixture"));
            Assert.That(controller.HasActiveCast, Is.False);
            Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Idle));
            Assert.That(stamina.CurrentStamina, Is.EqualTo(78), "A cancellation after commit must not refund stamina.");
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.GreaterThan(0));
            yield return new WaitForSeconds(6.1f);
        }

        [UnityTest, Order(83)]
        public IEnumerator Whirl_Whiff_CommitsCostCooldownAndRecovery()
        {
            var priorAvatar = PlayerController.ActiveInstance;
            if (priorAvatar != null)
            {
                priorAvatar.gameObject.SetActive(false);
                _temporarilyDisabledAvatar = priorAvatar;
            }
            var isolatedAvatar = Body("Timed whirl isolated avatar", new Vector2(3200, 3200));
            isolatedAvatar.AddComponent<PlayerController>();
            var controller = PrepareTimedWhirl(out var stamina);
            var phases = new List<string>();
            System.Action<SkillCastPhaseChangedEvent> phaseHandler = evt =>
            {
                if (evt.SkillActionId == "skill_melee_whirl_cut") phases.Add(evt.PhaseId);
            };
            GameEventBus.Subscribe(phaseHandler);
            try
            {
                controller.TryUseSlot(0);
                Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Windup));
                Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
                Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);

                yield return new WaitForSeconds(.22f);

                Assert.That(stamina.CurrentStamina, Is.EqualTo(78));
                Assert.That(controller.GetSlotCooldownRemaining(0), Is.GreaterThan(0));
                Assert.That(phases, Does.Contain("Windup"));
                Assert.That(phases, Does.Contain("Active"));
                yield return new WaitForSeconds(.20f);
                Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Recovery));
                Assert.That(phases, Does.Contain("Recovery"));
                yield return new WaitForSeconds(.40f);
                Assert.That(controller.CurrentCastPhase, Is.EqualTo(SkillCastPhase.Idle));
            }
            finally
            {
                GameEventBus.Unsubscribe(phaseHandler);
            }
            yield return new WaitForSeconds(5.5f);
        }

        private static ActiveSkillExecutionController PrepareTimedWhirl(out StaminaManager stamina)
        {
            var skills = SkillTreeManager.Instance;
            var fixture = new SkillTreeSaveData();
            fixture.NodeRanks.Add(new SkillNodeRankEntry("melee_whirl_cut", 1));
            skills.RestoreFromSaveData(fixture, 100);
            Assert.That(skills.TryAssignActiveSlot(0, "skill_melee_whirl_cut"), Is.True);
            stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            stamina.Initialize(100, 100);
            var controller = ActiveSkillExecutionController.Instance;
            Assert.That(controller, Is.Not.Null);
            return controller;
        }

        private static List<ProjectileBehaviour> SceneProjectiles()
        {
            var projectiles = new List<ProjectileBehaviour>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                projectiles.AddRange(root.GetComponentsInChildren<ProjectileBehaviour>());
            return projectiles;
        }

        private static void CaptureIfRequested(string scene)
        {
            string directory = System.Environment.GetEnvironmentVariable("CINDARS_SKILL_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) return;
            var camera = UnityEngine.Camera.main;
            Assert.That(camera, Is.Not.Null);
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
            var priorTarget = camera.targetTexture;
            var priorActive = RenderTexture.active;
            var target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); texture.Apply();
                System.IO.Directory.CreateDirectory(directory);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory, scene + "-fire-spark.png"), texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = priorTarget; RenderTexture.active = priorActive;
                if (texture != null) Object.Destroy(texture);
                target.Release(); Object.Destroy(target);
            }
        }
    }
}
