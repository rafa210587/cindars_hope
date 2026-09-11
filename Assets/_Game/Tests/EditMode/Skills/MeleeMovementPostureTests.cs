using CindarsHope.Combat;
using CindarsHope.Enemy;
using CindarsHope.Foundation;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class MeleeMovementPostureTests
    {
        [Test]
        public void ControlDr_UsesHundredSixtyThirty_ThenFourSecondImmunity()
        {
            var state = new ControlDiminishingReturnsState();

            var first = state.TryApply(5f, 10f);
            var second = state.TryApply(5f, 11f);
            var third = state.TryApply(5f, 12f);
            var immune = state.TryApply(5f, 15.99f);
            var reset = state.TryApply(5f, 16f);

            Assert.That(first.EffectiveDurationSeconds, Is.EqualTo(5f));
            Assert.That(second.EffectiveDurationSeconds, Is.EqualTo(3f));
            Assert.That(third.EffectiveDurationSeconds, Is.EqualTo(1.5f));
            Assert.That(third.ImmunityUntil, Is.EqualTo(16f));
            Assert.That(immune.CanApply, Is.False);
            Assert.That(reset.CanApply, Is.True);
            Assert.That(reset.Multiplier, Is.EqualTo(1f));
        }

        [Test]
        public void CanonicalMovementSkills_KeepAuthoredDamageDistanceAndPosture()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                AssertAction(actions, "skill_melee_avanco_aco", 12, 3, 2.5f, 1f);
                AssertAction(actions, "skill_melee_battle_dash", 8, 2, 3f, 1f);
                var leap = actions.Find(item => item.SkillActionId == "skill_melee_leap_attack");
                Assert.That(leap.BaseDamage, Is.EqualTo(14));
                Assert.That(leap.DamagePerRank, Is.EqualTo(3));
                Assert.That(leap.LeapDistance, Is.EqualTo(2.2f));
                var shout = actions.Find(item => item.SkillActionId == "skill_melee_grito_desafio");
                Assert.That(shout.ResolveRank(3).Damage, Is.EqualTo(8));
                Assert.That(shout.Range, Is.EqualTo(2.2f));
                var guardBreak = actions.Find(item => item.SkillActionId == "skill_melee_investida_quebra_guarda");
                Assert.That(guardBreak.ResolveRank(3).Damage, Is.EqualTo(24));
                Assert.That(guardBreak.PostureDamageMultiplier, Is.EqualTo(3f));
            }
            finally
            {
                foreach (var action in actions) Object.DestroyImmediate(action);
            }
        }

        [Test]
        public void ReactionAdapter_AddsRecoveryAndBlockingPostureBonuses()
        {
            var target = new GameObject("Reaction target");
            try
            {
                var brain = target.AddComponent<EnemyBrain>();
                target.AddComponent<EnemyPostureState>();
                var adapter = target.AddComponent<EnemySkillReactionAdapter>();

                brain.SetState(EnemyBrainState.AttackRecover);
                Assert.That(adapter.ResolvePostureMultiplier("skill_melee_avanco_aco", 1f), Is.EqualTo(1.25f));
                brain.SetState(EnemyBrainState.GuardHold);
                Assert.That(adapter.ResolvePostureMultiplier("skill_melee_investida_quebra_guarda", 3f), Is.EqualTo(3.25f));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void Taunt_EliteIsReduced_AndBossRequiresExplicitWindow()
        {
            var elite = ReactionFixture(EnemyDifficulty.Elite, out var eliteBrain, out var eliteAdapter, out var eliteData);
            var boss = ReactionFixture(EnemyDifficulty.Boss, out var bossBrain, out var bossAdapter, out var bossData);
            try
            {
                var eliteResult = eliteAdapter.ApplyTaunt(Vector2.zero, 5f, 10f);
                Assert.That(eliteResult.CanApply, Is.True);
                Assert.That(eliteResult.EffectiveDurationSeconds, Is.EqualTo(3f));
                Assert.That(eliteBrain.SkillPriorityUntil, Is.GreaterThan(Time.time));

                Assert.That(bossAdapter.ApplyTaunt(Vector2.zero, 5f, 10f).CanApply, Is.False);
                boss.GetComponent<EnemyVulnerabilityState>().OpenWindow(1f, 1.2f, 0f);
                var bossResult = bossAdapter.ApplyTaunt(Vector2.zero, 5f, 10f);
                Assert.That(bossResult.CanApply, Is.True);
                Assert.That(bossResult.EffectiveDurationSeconds, Is.EqualTo(1.5f));
                Assert.That(bossBrain.SkillPriorityUntil, Is.GreaterThan(Time.time));
            }
            finally
            {
                Object.DestroyImmediate(elite);
                Object.DestroyImmediate(boss);
                Object.DestroyImmediate(eliteData);
                Object.DestroyImmediate(bossData);
            }
        }

        [Test]
        public void Taunt_BossWithoutPosture_UsesExplicitVulnerabilityWindow()
        {
            var boss = new GameObject("Boss reaction fixture without posture");
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            try
            {
                data.baseDifficulty = EnemyDifficulty.Boss;
                var vulnerability = boss.AddComponent<EnemyVulnerabilityState>();
                var brain = boss.AddComponent<EnemyBrain>();
                brain.Configure(data);
                var adapter = EnemySkillReactionAdapter.GetOrCreate(boss);
                Assert.That(boss.GetComponent<EnemyPostureState>(), Is.Not.Null,
                    "The same runtime path used by skills must give a boss posture support.");

                Assert.That(adapter.ApplyTaunt(Vector2.zero, 5f, 10f).CanApply, Is.False);
                vulnerability.OpenWindow(1f, 1.2f, 0f);
                var result = adapter.ApplyTaunt(Vector2.zero, 5f, 10f);

                Assert.That(result.CanApply, Is.True);
                Assert.That(result.EffectiveDurationSeconds, Is.EqualTo(1.5f));
            }
            finally
            {
                Object.DestroyImmediate(boss);
                Object.DestroyImmediate(data);
            }
        }

        [Test]
        public void Taunt_EliteClassificationIncludesDataFlagAndRuntimeAffix()
        {
            var flagged = ReactionFixture(EnemyDifficulty.Easy, out _, out var flaggedAdapter, out var flaggedData);
            var affixed = ReactionFixture(EnemyDifficulty.Easy, out var affixedBrain, out var affixedAdapter, out var affixedData);
            try
            {
                flaggedData.IsElite = true;
                affixedBrain.ConfigureElite(EliteAffix.Frenzied);

                Assert.That(flaggedAdapter.ApplyTaunt(Vector2.zero, 5f, 10f).EffectiveDurationSeconds,
                    Is.EqualTo(3f));
                Assert.That(affixedAdapter.ApplyTaunt(Vector2.zero, 5f, 10f).EffectiveDurationSeconds,
                    Is.EqualTo(3f));
            }
            finally
            {
                Object.DestroyImmediate(flagged);
                Object.DestroyImmediate(affixed);
                Object.DestroyImmediate(flaggedData);
                Object.DestroyImmediate(affixedData);
            }
        }

        private static GameObject ReactionFixture(EnemyDifficulty difficulty,
            out EnemyBrain brain, out EnemySkillReactionAdapter adapter, out EnemyDataSO data)
        {
            var target = new GameObject(difficulty + " reaction fixture");
            data = ScriptableObject.CreateInstance<EnemyDataSO>();
            data.baseDifficulty = difficulty;
            target.AddComponent<EnemyVulnerabilityState>();
            brain = target.AddComponent<EnemyBrain>();
            brain.Configure(data);
            var posture = target.AddComponent<EnemyPostureState>();
            posture.Configure(difficulty);
            adapter = target.AddComponent<EnemySkillReactionAdapter>();
            return target;
        }

        private static void AssertAction(System.Collections.Generic.List<SkillActionSO> actions,
            string id, int damage, int perRank, float dash, float posture)
        {
            var action = actions.Find(item => item.SkillActionId == id);
            Assert.That(action, Is.Not.Null);
            Assert.That(action.BaseDamage, Is.EqualTo(damage));
            Assert.That(action.DamagePerRank, Is.EqualTo(perRank));
            Assert.That(action.DashDistance, Is.EqualTo(dash));
            Assert.That(action.PostureDamageMultiplier, Is.EqualTo(posture));
        }
    }
}
