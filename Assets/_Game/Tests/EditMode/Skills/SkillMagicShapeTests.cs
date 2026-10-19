using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Foundation;
using CindarsHope.Enemy;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class SkillMagicShapeTests
    {
        [TearDown]
        public void TearDown()
        {
            ElementalWardState.Clear();
            IncomingDamageModifierProvider.Source = null;
        }

        [Test]
        public void CanonicalMagicActions_HaveAuthoredIdentityTimingAndControl()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                AssertTiming(actions, "skill_magic_fire_spark", .18f, 0f, .25f);
                AssertTiming(actions, "skill_magic_ice_bind", .25f, 0f, .25f);
                AssertTiming(actions, "skill_magic_chama_breve", .30f, .10f, .30f);
                AssertTiming(actions, "skill_magic_rajada_gelida", .30f, 0f, .30f);
                AssertTiming(actions, "skill_magic_lightning_chain", .35f, 0f, .30f);
                AssertTiming(actions, "skill_magic_toxic_cloud", .70f, .10f, .40f);
                AssertTiming(actions, "skill_magic_elemental_ward", .55f, .10f, .40f);
                AssertTiming(actions, "skill_magic_slowing_sigils", .70f, .10f, .40f);

                var bind = Find(actions, "skill_magic_ice_bind");
                Assert.That(bind.StrongSlowFraction, Is.EqualTo(.70f));
                Assert.That(bind.StrongSlowDurationSeconds, Is.EqualTo(1f));
                var cloud = Find(actions, "skill_magic_toxic_cloud");
                Assert.That(cloud.PulseCount, Is.EqualTo(4));
                Assert.That(cloud.TargetingRange, Is.EqualTo(6f));
                var chain = Find(actions, "skill_magic_lightning_chain");
                Assert.That(chain.TargetDamageMultipliers, Is.EqualTo(new[] { 1f, .75f, .55f, .40f }));
                var ward = Find(actions, "skill_magic_elemental_ward");
                Assert.That(ward.NotYetExecutable, Is.False);
                Assert.That(ward.EffectHitCharges, Is.EqualTo(2));
                Assert.That(ward.ResolveControlStrength(3), Is.EqualTo(.40f));
            }
            finally
            {
                foreach (var action in actions) Object.DestroyImmediate(action);
            }
        }

        [TestCase(24, 6, 6, 6, 6)]
        [TestCase(30, 7, 7, 7, 9)]
        [TestCase(48, 12, 12, 12, 12)]
        public void ToxicCloud_PulsesPreserveExactBudget(int total, int first, int second, int third, int fourth)
        {
            var expected = new[] { first, second, third, fourth };
            for (int i = 0; i < expected.Length; i++)
                Assert.That(PersistentDamageZone.ResolvePulseDamage(total, 4, i), Is.EqualTo(expected[i]));
        }

        [Test]
        public void ChainFalloff_RoundsOncePerTarget()
        {
            var falloff = new[] { 1f, .75f, .55f, .40f };
            Assert.That(ChainSkillEffectExecutor.ResolveDamage(18, falloff, 0), Is.EqualTo(18));
            Assert.That(ChainSkillEffectExecutor.ResolveDamage(18, falloff, 1), Is.EqualTo(14));
            Assert.That(ChainSkillEffectExecutor.ResolveDamage(18, falloff, 2), Is.EqualTo(10));
            Assert.That(ChainSkillEffectExecutor.ResolveDamage(18, falloff, 3), Is.EqualTo(7));
        }

        [Test]
        public void ElementalWard_ConsumesOnlyEligibleHitsAndExpires()
        {
            ElementalWardState.Cast(.40f, 2, 5f, 10f);
            Assert.That(IncomingDamageModifierProvider.Resolve(10, DamageType.Fire, 11f, false), Is.EqualTo(6));
            Assert.That(ElementalWardState.Active.RemainingHits, Is.EqualTo(1));
            ElementalWardState.Cast(.40f, 2, 5f, 10f);
            Assert.That(ElementalWardState.ResolveIncomingDamage(10, DamageType.Fire, 11f, true), Is.EqualTo(10));
            Assert.That(ElementalWardState.Active.RemainingHits, Is.EqualTo(2));
            Assert.That(ElementalWardState.ResolveIncomingDamage(10, DamageType.Physical, 11f), Is.EqualTo(10));
            Assert.That(ElementalWardState.Active.RemainingHits, Is.EqualTo(2));
            Assert.That(ElementalWardState.ResolveIncomingDamage(10, DamageType.Fire, 11f), Is.EqualTo(6));
            Assert.That(ElementalWardState.Active.RemainingHits, Is.EqualTo(1));
            Assert.That(ElementalWardState.ResolveIncomingDamage(10, DamageType.Lightning, 11f), Is.EqualTo(6));
            Assert.That(ElementalWardState.Active, Is.Null);

            ElementalWardState.Cast(.25f, 2, 1f, 20f);
            ElementalWardState.ExpireIfNeeded(21f);
            Assert.That(ElementalWardState.Active, Is.Null);
            Assert.That(IncomingDamageModifierProvider.Source, Is.Null);
        }

        [Test]
        public void MagicControl_EnforcesEliteBudgetAndBossWindowMagnitude()
        {
            var elite = new GameObject("Magic control elite");
            var boss = new GameObject("Magic control boss");
            try
            {
                elite.AddComponent<EnemyPostureState>().Configure(EnemyDifficulty.Elite);
                Assert.That(MagicControlRules.TryResolveField(elite, 8f, .35f, "status_slow", 10f,
                    out float eliteDuration, out float eliteReduction, out bool eliteBound), Is.True);
                Assert.That(eliteDuration, Is.EqualTo(3.2f));
                Assert.That(eliteReduction, Is.EqualTo(.35f));
                Assert.That(eliteBound, Is.False);

                boss.AddComponent<EnemyPostureState>().Configure(EnemyDifficulty.Boss);
                Assert.That(MagicControlRules.TryResolveField(boss, 8f, .35f, "status_slow", 10f,
                    out _, out _, out _), Is.False);
                var window = boss.AddComponent<EnemyVulnerabilityState>();
                window.Initialize("boss_fixture");
                window.OpenWindow(10f, 1.5f, 0f);
                Assert.That(MagicControlRules.TryResolveField(boss, 8f, .35f, "status_slow", Time.time,
                    out float bossDuration, out float bossReduction, out bool bossBound), Is.True);
                Assert.That(bossDuration, Is.EqualTo(2.4f).Within(.01f));
                Assert.That(bossReduction, Is.EqualTo(.10f));
                Assert.That(bossBound, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(elite);
                Object.DestroyImmediate(boss);
            }
        }

        [Test]
        public void PreciseChill_ExpiresStatusTokenAtExactDuration()
        {
            var target = new GameObject("Precise chill target");
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            var chill = ScriptableObject.CreateInstance<CindarsHope.Combat.StatusEffect.StatusEffectSO>();
            try
            {
                data.enemyId = "precise_chill_target";
                data.maxHp = 10;
                var health = target.AddComponent<EnemyHealth>();
                health.Configure(data);
                chill.Id = "status_chill";
                chill.DurationTurns = 3;
                chill.Type = CindarsHope.Combat.StatusEffect.StatusEffectType.Chill;
                health.ApplyStatusEffect(chill, "external_chill");
                MagicSkillRuntimeUtility.RefreshStatus(
                    health, chill, 1.8f, "test:chill");

                var state = MagicSlowState.GetOrCreate(target);
                state.Apply("test:chill", .50f, 1.8f, false, 0f, chill.Id);
                Assert.That(state.SuppressesStatusMovement(chill.Id, "test:chill"), Is.True);
                Assert.That(state.SuppressesStatusMovement(chill.Id, "external_chill"), Is.False);
                Assert.That(state.HasActiveStatus(chill.Id, 1.79f), Is.True);
                Assert.That(state.HasActiveStatus(chill.Id, 1.8f), Is.False);
                Assert.That(health.StatusEffects.HasStatusEffect(chill.Id), Is.True,
                    "Expiring the skill lease must preserve Chill owned by another system.");
                var remaining = health.StatusEffects.GetActiveEffects();
                Assert.That(remaining.Count, Is.EqualTo(1));
                Assert.That(remaining[0].SourceId, Is.EqualTo("external_chill"));
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(data);
                Object.DestroyImmediate(chill);
            }
        }

        [Test]
        public void MagicBaseline_RespectsThroughputAndManaEnvelopes()
        {
            var spark = LoadAction(SkillActionEffectCatalog.MagicFireSparkActionId);
            var flame = LoadAction(SkillActionEffectCatalog.MagicBriefFlameActionId);
            var bind = LoadAction(SkillActionEffectCatalog.MagicIceBindActionId);
            var cloud = LoadAction(SkillActionEffectCatalog.MagicToxicCloudActionId);
            var sigils = LoadAction(SkillActionEffectCatalog.MagicSlowingSigilsActionId);
            var burn = LoadStatus(flame.StatusEffectId);
            var poison = LoadStatus(cloud.StatusEffectId);
            var elite = new GameObject("Magic benchmark elite");
            var boss = new GameObject("Magic benchmark boss");
            try
            {
                int sparkDirect = spark.ResolveRank(5).Damage;
                int flameDirect = flame.ResolveRank(3).Damage;
                int flameDot = burn.DamagePerTurn * burn.DurationTurns;
                int cloudDirect = cloud.ResolveRank(5).Damage;
                int cloudDot = poison.DamagePerTurn * poison.DurationTurns;
                float sparkRate = sparkDirect / Cycle(spark);
                float flameRate = (flameDirect + flameDot) / Cycle(flame);
                float cloudRate = (cloudDirect + cloudDot) / Cycle(cloud);
                Assert.That(flameRate / sparkRate, Is.LessThanOrEqualTo(1.10f));
                Assert.That(cloudRate / sparkRate, Is.LessThanOrEqualTo(1.15f));

                float rotationDrainPerSecond = spark.ResolveRank(5).ManaCost / Cycle(spark)
                    + flame.ResolveRank(3).ManaCost / Cycle(flame)
                    + bind.ResolveRank(1).ManaCost / Cycle(bind)
                    + cloud.ResolveRank(5).ManaCost / Cycle(cloud);
                Assert.That(rotationDrainPerSecond, Is.GreaterThan(5f));
                Assert.That(spark.ResolveRank(5).ManaCost / Cycle(spark), Is.LessThanOrEqualTo(5f));

                elite.AddComponent<EnemyPostureState>().Configure(EnemyDifficulty.Elite);
                Assert.That(MagicControlRules.TryResolveField(elite,
                    sigils.ResolveEffectDuration(5), sigils.ResolveControlStrength(5), sigils.StatusEffectId,
                    0f, out float eliteDuration, out float eliteReduction, out _), Is.True);
                Assert.That(eliteDuration / sigils.ResolveRank(5).CooldownSeconds,
                    Is.LessThanOrEqualTo(.40f));

                boss.AddComponent<EnemyPostureState>().Configure(EnemyDifficulty.Boss);
                var window = boss.AddComponent<EnemyVulnerabilityState>();
                window.Initialize("magic_benchmark_boss");
                Assert.That(MagicControlRules.TryResolveField(boss,
                    sigils.ResolveEffectDuration(5), sigils.ResolveControlStrength(5), sigils.StatusEffectId,
                    0f, out _, out _, out _), Is.False);
                window.OpenWindow(2f, 1.5f, 0f);
                Assert.That(MagicControlRules.TryResolveField(boss,
                    sigils.ResolveEffectDuration(5), sigils.ResolveControlStrength(5), sigils.StatusEffectId,
                    0f, out float bossDuration, out float bossReduction, out _), Is.True);
                Assert.That(bossReduction, Is.EqualTo(.10f));

                int flameTrioDirect = flameDirect
                    + 2 * Mathf.RoundToInt(flameDirect * flame.AdditionalTargetDamageMultiplier);
                RecordBenchmark("neutral", "Fagulha R5", sparkDirect, 0, spark.ResolveRank(5).ManaCost,
                    Cycle(spark), 100, 0f);
                RecordBenchmark("neutral", "Chama R3", flameDirect, flameDot, flame.ResolveRank(3).ManaCost,
                    Cycle(flame), 100, 0f);
                RecordBenchmark("neutral", "Nuvem R5", cloudDirect, cloudDot, cloud.ResolveRank(5).ManaCost,
                    Cycle(cloud), 100, 0f);
                RecordBenchmark("trio", "Chama R3", flameTrioDirect, flameDot * 3,
                    flame.ResolveRank(3).ManaCost,
                    Cycle(flame), 300, 0f);
                RecordBenchmark("trio", "Nuvem R5", cloudDirect * 3, cloudDot * 3,
                    cloud.ResolveRank(5).ManaCost,
                    Cycle(cloud), 300, 0f);
                RecordBenchmark("elite", "Sigilos R5", 0, 0, sigils.ResolveRank(5).ManaCost,
                    Cycle(sigils), 100, eliteDuration / sigils.ResolveRank(5).CooldownSeconds);
                RecordBenchmark("boss fora", "Sigilos R5", 0, 0, sigils.ResolveRank(5).ManaCost,
                    Cycle(sigils), 100, 0f);
                RecordBenchmark("boss janela", "Sigilos R5", 0, 0, sigils.ResolveRank(5).ManaCost,
                    Cycle(sigils), 100, bossReduction);

                Assert.That(EffectiveTtk(100, sparkDirect, Cycle(spark)), Is.EqualTo(Cycle(spark) * 5f));
                Assert.That(EffectiveTtk(100, cloudDirect + cloudDot, Cycle(cloud)), Is.EqualTo(Cycle(cloud) * 2f));
                Assert.That(EffectiveTtk(300, (cloudDirect + cloudDot) * 3, Cycle(cloud)),
                    Is.EqualTo(Cycle(cloud) * 2f));
            }
            finally
            {
                Object.DestroyImmediate(elite);
                Object.DestroyImmediate(boss);
            }
        }

        private static float Cycle(SkillActionSO action)
            => action.WindupSeconds + action.ActiveSeconds + action.RecoverySeconds + action.CooldownSeconds;

        private static float EffectiveTtk(int hp, int damagePerCycle, float cycle)
            => damagePerCycle <= 0 ? float.PositiveInfinity : Mathf.Ceil(hp / (float)damagePerCycle) * cycle;

        private static void RecordBenchmark(string scenario, string action, int direct, int dot,
            float mana, float cycle, int hp, float control)
        {
            int total = direct + dot;
            float dps = cycle > 0f ? total / cycle : 0f;
            float manaPerSecond = cycle > 0f ? mana / cycle : 0f;
            float ttk = EffectiveTtk(hp, total, cycle);
            TestContext.WriteLine(
                $"MAGIC_BALANCE|{scenario}|{action}|direct={direct}|dot={dot}|mp={mana:0.##}" +
                $"|cycle={cycle:0.##}|dps={dps:0.##}|mp_s={manaPerSecond:0.##}" +
                $"|control={control:0.##}|ttk={(float.IsPositiveInfinity(ttk) ? "INF" : ttk.ToString("0.##"))}");
        }

        private static SkillActionSO Find(List<SkillActionSO> actions, string id)
            => actions.Find(action => action.SkillActionId == id);

        private static SkillActionSO LoadAction(string id)
        {
            var action = AssetDatabase.LoadAssetAtPath<SkillActionSO>(
                $"Assets/_Game/Data/Skills/Actions/SkillAction_{id}.asset");
            Assert.That(action, Is.Not.Null, id);
            return action;
        }

        private static CindarsHope.Combat.StatusEffect.StatusEffectSO LoadStatus(string id)
        {
            var status = AssetDatabase.LoadAssetAtPath<CindarsHope.Combat.StatusEffect.StatusEffectSO>(
                $"Assets/_Game/Data/Combat/StatusEffects/{id}.asset");
            Assert.That(status, Is.Not.Null, id);
            return status;
        }

        private static void AssertTiming(List<SkillActionSO> actions, string id,
            float windup, float active, float recovery)
        {
            var action = Find(actions, id);
            Assert.That(action, Is.Not.Null, id);
            Assert.That(action.WindupSeconds, Is.EqualTo(windup), id);
            Assert.That(action.ActiveSeconds, Is.EqualTo(active), id);
            Assert.That(action.RecoverySeconds, Is.EqualTo(recovery), id);
        }
    }
}
