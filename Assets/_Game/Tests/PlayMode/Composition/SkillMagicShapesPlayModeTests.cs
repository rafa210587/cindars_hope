using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Enemy;
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
    public class SkillMagicShapesPlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private ISpellCastPreparationPolicy _previousSpellCastPolicy;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null)
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
            _previousSpellCastPolicy = SpellCastPreparationProvider.Source;
            SpellCastPreparationProvider.Source = null;
            var mana = (ManaManager)GameBootstrap.Instance.ManaManager;
            mana.SetMaxMana(100);
            mana.Initialize();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            ElementalWardState.Clear();
            SpellCastPreparationProvider.Source = _previousSpellCastPolicy;
            foreach (var item in _owned)
                if (item != null) Object.Destroy(item);
            _owned.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator FlameCone_OrdersTargetsAppliesFalloffAndBurnOnce()
        {
            var origin = new Vector2(5100, 5100);
            var caster = Body("Magic cone caster", origin, false);
            var first = Enemy("Cone first", origin + Vector2.right, 100);
            var second = Enemy("Cone second", origin + new Vector2(1.5f, .2f), 100);
            var outside = Enemy("Cone outside", origin + Vector2.up, 100);
            var action = Action("skill_magic_chama_breve");
            var executor = new ConeSkillEffectExecutor("magic.chama_breve", "Chama Breve", action);
            var result = executor.Execute(Context(caster, action, 3, origin));

            Assert.That(result.Success, Is.True);
            Assert.That(first.CurrentHp, Is.EqualTo(88));
            Assert.That(second.CurrentHp, Is.EqualTo(92));
            Assert.That(outside.CurrentHp, Is.EqualTo(100));
            Assert.That(first.StatusEffects.HasStatusEffect("status_burn_minor"), Is.True);
            Assert.That(second.StatusEffects.GetActiveEffects().FindAll(x => x.StatusEffectId == "status_burn_minor").Count,
                Is.EqualTo(1));
            Assert.That(((ManaManager)GameBootstrap.Instance.ManaManager).CurrentMana, Is.EqualTo(92));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ToxicCloud_PulsesGloballyWithoutCatchUpOnReentry()
        {
            var center = new Vector2(5200, 5200);
            var fullStay = Enemy("Cloud full stay", center, 100);
            var lateEntry = Enemy("Cloud late entry", center + Vector2.right * 4f, 100);
            var zoneObject = new GameObject("Cloud zone fixture");
            _owned.Add(zoneObject);
            zoneObject.transform.position = center;
            var zone = zoneObject.AddComponent<PersistentDamageZone>();
            zone.Configure("skill_magic_toxic_cloud", 5, 48, DamageType.Toxic, 2f, 4f, 4, null, 100f);
            Assert.That(fullStay.CurrentHp, Is.EqualTo(88));
            Assert.That(lateEntry.CurrentHp, Is.EqualTo(100));

            lateEntry.transform.position = center + Vector2.right;
            zone.Advance(101f);
            zone.Advance(102f);
            zone.Advance(103f);

            Assert.That(fullStay.CurrentHp, Is.EqualTo(52));
            Assert.That(lateEntry.CurrentHp, Is.EqualTo(64), "Late entry receives three pulses, never catch-up damage.");
            Assert.That(zone.PulsesApplied, Is.EqualTo(4));
            yield return null;
        }

        [UnityTest]
        public IEnumerator LightningChain_UsesStableFalloffAndPostureOnlyForLightningVulnerability()
        {
            var origin = new Vector2(5300, 5300);
            var caster = Body("Chain caster", origin, false);
            var first = Enemy("Chain first", origin + Vector2.right, 100, "Construct");
            var second = Enemy("Chain second", origin + Vector2.right * 3f, 100, "Beast");
            var third = Enemy("Chain third", origin + Vector2.right * 5f, 100);
            var fourth = Enemy("Chain fourth", origin + Vector2.right * 7f, 100);
            var vulnerability = ScriptableObject.CreateInstance<EnemyVulnerabilityProfileSO>();
            _owned.Add(vulnerability);
            vulnerability.ElementMultipliers = new[]
            {
                new ElementMultiplier { DamageType = DamageType.Lightning, Multiplier = 1.5f }
            };
            first.ConfigureVulnerabilityMatrix(vulnerability);
            second.ConfigureVulnerabilityMatrix(vulnerability);
            var vulnerablePosture = first.gameObject.AddComponent<EnemyPostureState>();
            vulnerablePosture.Configure(EnemyDifficulty.Normal);
            var neutralPosture = second.gameObject.AddComponent<EnemyPostureState>();
            neutralPosture.Configure(EnemyDifficulty.Normal);

            var action = Action("skill_magic_lightning_chain");
            var executor = new ChainSkillEffectExecutor("combat.magic.lightning_chain", "Corrente", action);
            var result = executor.Execute(Context(caster, action, 1, origin));

            Assert.That(result.Success, Is.True);
            Assert.That(first.CurrentHp, Is.EqualTo(82));
            Assert.That(second.CurrentHp, Is.EqualTo(86));
            Assert.That(third.CurrentHp, Is.EqualTo(93));
            Assert.That(fourth.CurrentHp, Is.EqualTo(95));
            Assert.That(vulnerablePosture.CurrentPosture, Is.EqualTo(vulnerablePosture.MaxPosture - 9f));
            Assert.That(neutralPosture.CurrentPosture, Is.EqualTo(neutralPosture.MaxPosture));
            Assert.That(((ManaManager)GameBootstrap.Instance.ManaManager).CurrentMana, Is.EqualTo(80));
            yield return null;
        }

        [UnityTest]
        public IEnumerator IceProjectiles_ApplyLightChillStrongFollowupAndVolleyOverlapOnce()
        {
            var origin = new Vector2(5400, 5400);
            var caster = Body("Ice caster", origin, false);
            var bindTarget = Enemy("Ice bind target", origin + Vector2.right * 1.5f, 100);
            var bind = Action("skill_magic_ice_bind");
            var bindExecutor = new ProjectileSkillEffectExecutor("combat.magic.ice_bind", "Laço", bind);

            Assert.That(bindExecutor.Execute(Context(caster, bind, 1, origin)).Success, Is.True);
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(bindTarget.CurrentHp, Is.EqualTo(90));
            Assert.That(bindTarget.StatusEffects.HasStatusEffect("status_chill"), Is.True);
            Assert.That(bindTarget.GetComponent<MagicSlowState>().CurrentReduction, Is.EqualTo(.50f));

            Assert.That(bindExecutor.Execute(Context(caster, bind, 1, origin)).Success, Is.True);
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(bindTarget.GetComponent<MagicSlowState>().CurrentReduction, Is.EqualTo(.70f));
            Assert.That(bindTarget.GetComponent<EnemySkillReactionAdapter>().ControlDr.EligibleApplications, Is.EqualTo(1));

            var volleyOrigin = new Vector2(5420, 5400);
            caster.transform.position = volleyOrigin;
            var volleyTarget = Enemy("Ice volley target", volleyOrigin + Vector2.right * 1.5f, 100);
            var volley = Action("skill_magic_rajada_gelida");
            var volleyExecutor = new ProjectileSkillEffectExecutor("magic.rajada_gelida", "Rajada", volley);
            Assert.That(volleyExecutor.Execute(Context(caster, volley, 1, volleyOrigin)).Success, Is.True);
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(volleyTarget.CurrentHp, Is.EqualTo(91));
            Assert.That(volleyTarget.StatusEffects.GetActiveEffects().FindAll(x => x.StatusEffectId == "status_chill").Count,
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SlowingSigils_BossOutsideWindowRemainsEligibleWhenWindowOpens()
        {
            var center = new Vector2(5460, 5400);
            var boss = Enemy("Sigil boss", center, 100);
            boss.gameObject.AddComponent<EnemyPostureState>().Configure(EnemyDifficulty.Boss);
            var window = boss.gameObject.AddComponent<EnemyVulnerabilityState>();
            window.Initialize("sigil_boss_fixture");

            var fieldObject = new GameObject("Sigil field fixture");
            _owned.Add(fieldObject);
            fieldObject.transform.position = center;
            fieldObject.AddComponent<MagicSlowField>().Configure(
                "skill_magic_slowing_sigils", 2.5f, 4f, .35f, "status_slow", Time.time);
            Assert.That(boss.GetComponent<MagicSlowState>(), Is.Null);

            window.OpenWindow(2f, 1.5f, Time.time);
            yield return new WaitForSeconds(.12f);

            var slow = boss.GetComponent<MagicSlowState>();
            Assert.That(slow, Is.Not.Null);
            Assert.That(slow.CurrentReduction, Is.EqualTo(.10f));
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

        private static SkillEffectContext Context(GameObject caster, SkillActionSO action, int rank, Vector2 origin)
            => new SkillEffectContext
            {
                SkillActionId = action.SkillActionId,
                Caster = caster,
                ActionData = action,
                Rank = rank,
                WorldPosition = origin
            };

        private EnemyHealth Enemy(string name, Vector2 position, int hp, string family = "")
        {
            var go = Body(name, position, true);
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = name;
            data.CreatureFamily = family;
            data.maxHp = hp;
            data.defense = 0;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            return health;
        }

        private GameObject Body(string name, Vector2 position, bool trigger)
        {
            var go = new GameObject(name);
            _owned.Add(go);
            go.transform.position = position;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = trigger;
            return go;
        }
    }
}
