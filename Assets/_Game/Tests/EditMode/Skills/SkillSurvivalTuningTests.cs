using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class SkillSurvivalTuningTests
    {
        [Test]
        public void CanonicalSurvivalActions_ExposeReviewedTimingCostsAndRankValues()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                var breath = Find(actions, "skill_survival_last_breath");
                Assert.That(breath.ResolveEffectMagnitude(5), Is.EqualTo(.45f));
                Assert.That(breath.ResolveRank(1).CooldownSeconds, Is.EqualTo(90f));
                Assert.That(breath.RecoverySeconds, Is.EqualTo(.25f));

                var signal = Find(actions, "skill_survival_sinal_retirada");
                Assert.That(signal.ResolveRank(1).StaminaCost, Is.EqualTo(15f));
                Assert.That(signal.ResolveEffectMagnitude(3), Is.EqualTo(.30f));
                Assert.That(signal.ResolveSecondaryMagnitude(3), Is.EqualTo(.12f));
                Assert.That(signal.ResolveSecondaryDuration(3), Is.EqualTo(7f));

                var lure = Find(actions, "skill_survival_isca_improvisada");
                Assert.That(lure.Range, Is.EqualTo(6f));
                Assert.That(lure.EffectRadius, Is.EqualTo(5f));
                Assert.That(lure.MaxTargets, Is.EqualTo(5));
                Assert.That(lure.ResolveEffectMagnitude(2), Is.EqualTo(6f));
                Assert.That(lure.ResolveSecondaryDuration(2), Is.EqualTo(1.5f));

                var kit = Find(actions, "skill_survival_kit_emergencia");
                Assert.That(kit.WindupSeconds, Is.EqualTo(.80f));
                Assert.That(kit.ResolveEffectMagnitude(5), Is.EqualTo(.30f));

                var instinct = Find(actions, "skill_survival_instinto_sobrevivencia");
                Assert.That(instinct.EffectRadius, Is.EqualTo(7f));
                Assert.That(instinct.ResolveSecondaryDuration(5), Is.EqualTo(8f));

                var camp = Find(actions, "skill_survival_campo_seguro");
                Assert.That(camp.EffectRadius, Is.EqualTo(2.5f));
                Assert.That(camp.ResolveEffectMagnitude(1), Is.EqualTo(.50f));
                Assert.That(camp.ResolveSecondaryMagnitude(5), Is.EqualTo(.50f));
                Assert.That(camp.ResolveSecondaryDuration(5), Is.EqualTo(16f));
            }
            finally
            {
                foreach (var action in actions) Object.DestroyImmediate(action);
            }
        }

        [TestCase(101, .25f, 26)]
        [TestCase(101, .30f, 31)]
        [TestCase(100, .45f, 45)]
        public void PercentHealing_AlwaysRoundsUp(int maxHp, float fraction, int expected)
            => Assert.That(SurvivalSkillActionRules.ResolvePercentHeal(maxHp, fraction), Is.EqualTo(expected));

        [Test]
        public void RetreatBenefit_RequiresCommittedDirectionAwayFromThreat()
        {
            Assert.That(SurvivalSkillActionRules.IsMovingAway(Vector2.right, Vector2.zero, Vector2.right), Is.True);
            Assert.That(SurvivalSkillActionRules.IsMovingAway(Vector2.right, Vector2.zero, Vector2.left), Is.False);
            Assert.That(SurvivalSkillActionRules.IsMovingAway(Vector2.right, Vector2.zero,
                new Vector2(.5f, .8660254f)), Is.True);
        }

        [Test]
        public void LureDuration_RejectsBossAndUsesShortEliteDiversion()
        {
            Assert.That(SurvivalSkillActionRules.ResolveLureDuration(EnemyDifficulty.Normal, false, 8f, 2f), Is.EqualTo(8f));
            Assert.That(SurvivalSkillActionRules.ResolveLureDuration(EnemyDifficulty.Elite, false, 8f, 2f), Is.EqualTo(2f));
            Assert.That(SurvivalSkillActionRules.ResolveLureDuration(EnemyDifficulty.MiniBoss, false, 8f, 2f), Is.EqualTo(2f));
            Assert.That(SurvivalSkillActionRules.ResolveLureDuration(EnemyDifficulty.Normal, true, 8f, 2f), Is.EqualTo(2f));
            Assert.That(SurvivalSkillActionRules.ResolveLureDuration(EnemyDifficulty.Boss, false, 8f, 2f), Is.Zero);
        }

        private static SkillActionSO Find(List<SkillActionSO> actions, string id)
        {
            var action = actions.Find(candidate => candidate.SkillActionId == id);
            Assert.That(action, Is.Not.Null, id);
            return action;
        }
    }
}
