using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Enemy;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Player.Movement;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public class SkillMeleeMovementPlayModeTests
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
        public IEnumerator BattleDash_CrossesLightEnemyAndStopsAtWall()
        {
            var origin = new Vector2(3500, 3500);
            var caster = Body("Battle dash caster", origin, false);
            caster.AddComponent<PlayerMovementDisplacementResolver>();
            var crossed = Enemy("Light enemy solid body", origin + Vector2.right * .8f, EnemyDifficulty.Easy, false);
            crossed.GetComponent<EnemyBrain>().enabled = false;
            var impactTarget = Enemy("Dash impact target", origin + Vector2.right * 1.85f, EnemyDifficulty.Easy);
            impactTarget.GetComponent<EnemyBrain>().enabled = false;
            var wall = Body("Battle dash wall", origin + Vector2.right * 2f, false);
            wall.GetComponent<BoxCollider2D>().size = new Vector2(.2f, 3f);
            var action = FindAction("skill_melee_battle_dash");
            Stamina().Initialize(100, 100);
            Physics2D.SyncTransforms();

            var result = new MeleeStrikeSkillEffectExecutor("fixture.battle_dash", "Arrancada", action)
                .Execute(new SkillEffectContext
                {
                    SkillActionId = action.SkillActionId,
                    Caster = caster,
                    ActionData = action,
                    Rank = 1,
                    WorldPosition = origin
                });

            Assert.That(result.Success, Is.True, result.FailureReason);
            var resolver = caster.GetComponent<PlayerMovementDisplacementResolver>();
            for (int i = 0; i < 60 && resolver.IsDisplacing; i++)
                yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(resolver.IsDisplacing, Is.False, "Dash must finish before impact assertions.");
            float travelled = caster.transform.position.x - origin.x;
            Assert.That(travelled, Is.GreaterThan(1f), "A trigger enemy must not block the dash.");
            Assert.That(travelled, Is.LessThan(2f), "The solid wall must stop the dash.");
            Assert.That(impactTarget.CurrentHp, Is.LessThan(100),
                $"Impact after movement must still resolve. caster={caster.transform.position}, target={impactTarget.transform.position}");
        }

        [UnityTest]
        public IEnumerator Leap_InvalidLandingRefusesBeforeCost()
        {
            var origin = new Vector2(3550, 3550);
            var caster = Body("Leap caster", origin, false);
            caster.AddComponent<PlayerMovementDisplacementResolver>();
            Body("Blocked landing", origin + Vector2.right * 2.2f, false);
            var action = FindAction("skill_melee_leap_attack");
            Stamina().Initialize(100, 100);
            Physics2D.SyncTransforms();
            var context = new SkillEffectContext
            {
                SkillActionId = action.SkillActionId,
                Caster = caster,
                ActionData = action,
                Rank = 1,
                WorldPosition = origin + Vector2.right * 2.2f
            };
            var executor = new MeleeStrikeSkillEffectExecutor("fixture.leap", "Salto", action);

            var validation = executor.Validate(context);
            var result = executor.Execute(context);

            Assert.That(validation.Success, Is.False);
            Assert.That(validation.FailureReason, Is.EqualTo("InvalidLandingPoint"));
            Assert.That(result.Success, Is.False);
            Assert.That(Stamina().CurrentStamina, Is.EqualTo(100));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Taunt_EliteBossAndDrFollowSharedPerTargetRule()
        {
            var elite = Enemy("Elite taunt target", new Vector2(3600, 3600), EnemyDifficulty.Elite);
            var eliteAdapter = EnemySkillReactionAdapter.GetOrCreate(elite.gameObject);
            var first = eliteAdapter.ApplyTaunt(Vector2.zero, 5f, 10f);
            var second = eliteAdapter.ApplyTaunt(Vector2.zero, 5f, 11f);
            var third = eliteAdapter.ApplyTaunt(Vector2.zero, 5f, 12f);
            var immune = eliteAdapter.ApplyTaunt(Vector2.zero, 5f, 13f);
            Assert.That(first.EffectiveDurationSeconds, Is.EqualTo(3f));
            Assert.That(second.EffectiveDurationSeconds, Is.EqualTo(1.8f).Within(.001f));
            Assert.That(third.EffectiveDurationSeconds, Is.EqualTo(.9f).Within(.001f));
            Assert.That(immune.CanApply, Is.False);

            var boss = Enemy("Boss taunt target", new Vector2(3610, 3600), EnemyDifficulty.Boss);
            var bossAdapter = EnemySkillReactionAdapter.GetOrCreate(boss.gameObject);
            Assert.That(bossAdapter.ApplyTaunt(Vector2.zero, 5f, 20f).CanApply, Is.False);
            boss.GetComponent<EnemyVulnerabilityState>().OpenWindow(1f, 1.2f, 0f);
            Assert.That(bossAdapter.ApplyTaunt(Vector2.zero, 5f, 20f).EffectiveDurationSeconds,
                Is.EqualTo(1.5f).Within(.001f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Taunt_ClearsConflictRivalImmediately()
        {
            var source = Enemy("Taunted conflict enemy", new Vector2(3620, 3600), EnemyDifficulty.Elite);
            var rival = Enemy("Conflict rival", new Vector2(3621, 3600), EnemyDifficulty.Easy);
            var brain = source.GetComponent<EnemyBrain>();
            brain.BindConflictSource((_, __) => rival, () => 1);
            yield return null;
            Assert.That(brain.IsTargetingConflictRival, Is.True);

            var adapter = EnemySkillReactionAdapter.GetOrCreate(source.gameObject);
            var result = adapter.ApplyTaunt(Vector2.zero, 3f, Time.time);

            Assert.That(result.CanApply, Is.True);
            Assert.That(brain.IsTargetingConflictRival, Is.False,
                "Taunt must clear the stale rival damage route before prioritizing the caster.");
        }

        private GameObject Body(string name, Vector2 position, bool trigger)
        {
            var go = new GameObject(name);
            _owned.Add(go);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * .4f;
            collider.isTrigger = trigger;
            return go;
        }

        private EnemyHealth Enemy(string name, Vector2 position, EnemyDifficulty difficulty, bool trigger = true)
        {
            var go = Body(name, position, trigger);
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = name;
            data.maxHp = 100;
            data.baseDifficulty = difficulty;
            var vulnerability = go.AddComponent<EnemyVulnerabilityState>();
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            var brain = go.AddComponent<EnemyBrain>();
            brain.Configure(data);
            var posture = go.AddComponent<EnemyPostureState>();
            posture.Configure(difficulty);
            vulnerability.Initialize(data.enemyId);
            return health;
        }

        private SkillActionSO FindAction(string id)
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            SkillActionSO result = null;
            foreach (var action in actions)
            {
                _owned.Add(action);
                if (action.SkillActionId == id) result = action;
            }
            Assert.That(result, Is.Not.Null, id);
            return result;
        }

        private static StaminaManager Stamina() => (StaminaManager)GameBootstrap.Instance.StaminaManager;
    }
}
