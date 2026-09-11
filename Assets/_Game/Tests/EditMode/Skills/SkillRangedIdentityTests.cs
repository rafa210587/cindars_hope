using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class SkillRangedIdentityTests
    {
        [Test]
        public void Charge_EarlyReleaseCancels_AndMinimumMaximumMatchContract()
        {
            var charge = new ChargedSkillCastState();
            var authored = new ChargedSkillProfile(.20f, 1.10f, 8, 20, 5f, 9f, .5f, 1.25f, 12f, 26f);
            charge.Begin(authored);
            charge.Tick(.19f);
            Assert.That(charge.Release().CanCommit, Is.False);

            charge.Begin(authored);
            charge.Tick(.20f);
            var minimum = charge.Release();
            Assert.That(minimum.CanCommit, Is.True);
            Assert.That(minimum.Damage, Is.EqualTo(8));
            Assert.That(minimum.Range, Is.EqualTo(5f));
            Assert.That(minimum.PostureMultiplier, Is.EqualTo(.5f));
            Assert.That(minimum.StaminaCost, Is.EqualTo(12));

            charge.Begin(authored);
            charge.Tick(10f);
            var maximum = charge.Release();
            Assert.That(maximum.NormalizedCharge, Is.EqualTo(1f));
            Assert.That(maximum.Damage, Is.EqualTo(20));
            Assert.That(maximum.Range, Is.EqualTo(9f));
            Assert.That(maximum.PostureMultiplier, Is.EqualTo(1.25f));
            Assert.That(maximum.StaminaCost, Is.EqualTo(26));
        }

        [Test]
        public void Charge_ResolutionUsesTheAuthoredProfile()
        {
            var charge = new ChargedSkillCastState();
            var authored = new ChargedSkillProfile(.4f, .8f, 3, 17, 2f, 13f, .2f, 2f, 4f, 19f);
            charge.Begin(authored);
            charge.Tick(.8f);

            var result = charge.Release();

            Assert.That(result.CanCommit, Is.True);
            Assert.That(result.Damage, Is.EqualTo(17));
            Assert.That(result.Range, Is.EqualTo(13f));
            Assert.That(result.PostureMultiplier, Is.EqualTo(2f));
            Assert.That(result.StaminaCost, Is.EqualTo(19));
        }

        [Test]
        public void LinePiercer_DeduplicatesAndUsesFiveStepFalloff()
        {
            var policy = new RangedProjectileHitPolicy(RangedProjectilePolicyKind.LinePiercer);
            var expected = new[] { 1f, .8f, .64f, .51f, .41f };
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(policy.TryResolveHit(i + 1, out float multiplier), Is.True);
                Assert.That(multiplier, Is.EqualTo(expected[i]));
                Assert.That(policy.TryResolveHit(i + 1, out _), Is.False, "The same target must only count once.");
            }
            Assert.That(policy.TryResolveHit(99, out _), Is.False, "A sixth target must be rejected.");
        }

        [Test]
        public void TripleFan_SameTargetGetsFullThenHalfThenRejects()
        {
            var policy = new RangedProjectileHitPolicy(RangedProjectilePolicyKind.TripleFan);
            Assert.That(policy.TryResolveHit(7, out float first), Is.True);
            Assert.That(first, Is.EqualTo(1f));
            Assert.That(policy.TryResolveHit(7, out float second), Is.True);
            Assert.That(second, Is.EqualTo(.5f));
            Assert.That(policy.TryResolveHit(7, out _), Is.False);
            Assert.That(policy.TryResolveHit(8, out float other), Is.True);
            Assert.That(other, Is.EqualTo(1f));
        }

        [Test]
        public void MarkedPrey_RankCurvesAndCasterOwnershipAreExact()
        {
            var target = new GameObject("Marked prey state fixture");
            try
            {
                var action = ScriptableObject.CreateInstance<SkillActionSO>();
                action.EffectDurationSeconds = 6f;
                action.EffectDurationPerRank = 2f;
                action.RangedDamageBonusFraction = .08f;
                action.RangedDamageBonusPerRank = .04f;
                var mark = target.AddComponent<MarkedPreyState>();
                mark.Apply(10, action.ResolveEffectDuration(3), action.ResolveRangedDamageBonus(3));
                Assert.That(action.ResolveEffectDuration(1), Is.EqualTo(6f));
                Assert.That(action.ResolveEffectDuration(2), Is.EqualTo(8f));
                Assert.That(action.ResolveEffectDuration(3), Is.EqualTo(10f));
                Assert.That(action.ResolveRangedDamageBonus(1), Is.EqualTo(.08f));
                Assert.That(action.ResolveRangedDamageBonus(2), Is.EqualTo(.12f));
                Assert.That(action.ResolveRangedDamageBonus(3), Is.EqualTo(.16f));
                Assert.That(mark.ResolveDamageMultiplier(10), Is.EqualTo(1.16f));
                Assert.That(mark.ResolveDamageMultiplier(11), Is.EqualTo(1f));
                Assert.That(mark.IsRevealed, Is.True);
                mark.Clear();
                Assert.That(mark.IsRevealed, Is.False);
                Object.DestroyImmediate(action);
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void CanonicalChargedAndMarkedActionsAreExecutable()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                var charged = actions.Find(item => item.SkillActionId == "skill_ranged_charged_shot");
                var marked = actions.Find(item => item.SkillActionId == "skill_ranged_marked_prey");
                Assert.That(charged.NotYetExecutable, Is.False);
                Assert.That(charged.StaminaCost, Is.EqualTo(12f));
                Assert.That(charged.Range, Is.EqualTo(5f));
                Assert.That(charged.ChargeTimeSeconds, Is.EqualTo(1.10f));
                Assert.That(charged.ChargeMinimumHoldSeconds, Is.EqualTo(.20f));
                Assert.That(charged.ChargeMaximumDamage, Is.EqualTo(20));
                Assert.That(charged.ChargeMaximumRange, Is.EqualTo(9f));
                Assert.That(charged.ChargeMaximumStaminaCost, Is.EqualTo(26f));
                Assert.That(marked.NotYetExecutable, Is.False);
                Assert.That(marked.StaminaCost, Is.EqualTo(10f));
                Assert.That(marked.Range, Is.EqualTo(8f));
                Assert.That(marked.ResolveEffectDuration(3), Is.EqualTo(10f));
                Assert.That(marked.ResolveRangedDamageBonus(3), Is.EqualTo(.16f));
            }
            finally
            {
                foreach (var action in actions) Object.DestroyImmediate(action);
            }
        }

        [Test]
        public void CanonicalChargedAndMarkedNodesAreNotDormant()
        {
            var nodes = DefaultSkillCatalog.BuildAllNodes();
            try
            {
                var charged = nodes.Find(item => item.SkillNodeId == "ranged_charged_shot");
                var marked = nodes.Find(item => item.SkillNodeId == "ranged_marked_prey");
                Assert.That(charged, Is.Not.Null);
                Assert.That(marked, Is.Not.Null);
                Assert.That(charged.NotYetExecutable, Is.False);
                Assert.That(marked.NotYetExecutable, Is.False);
            }
            finally
            {
                foreach (var node in nodes) Object.DestroyImmediate(node);
            }
        }
    }
}
