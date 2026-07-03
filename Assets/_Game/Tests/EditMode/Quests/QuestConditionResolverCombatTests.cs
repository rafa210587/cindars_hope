using NUnit.Framework;
using CindarsHope.Quests.Conditions;

namespace CindarsHope.Tests.EditMode.Quests
{
    // Covers spec_codex_02_quest_condition_honesty: CombatCondition must never return an
    // unconditional Pass, and IsFutureCondition() must keep passing the 3 future types
    // (anti-regression) while now being auditable via a one-shot dev log.
    [TestFixture]
    public class QuestConditionResolverCombatTests
    {
        private QuestConditionResolver _resolver;
        private QuestConditionContext _ctx;

        [SetUp]
        public void SetUp()
        {
            _resolver = new QuestConditionResolver();
            _ctx = new QuestConditionContext();
        }

        private QuestConditionDefinition CombatCondition(string id, string expectedValue, QuestConditionOperator op) =>
            new QuestConditionDefinition
            {
                ConditionId = id,
                ConditionType = QuestConditionType.CombatCondition,
                ExpectedValue = expectedValue,
                Operator = op
            };

        [Test]
        public void CombatCondition_HpAboveThreshold_GreaterThanOrEqual_Pass()
        {
            _ctx.PlayerCurrentHp = 80;
            _ctx.PlayerMaxHp = 100;
            var c = CombatCondition("c_hp_high", "50", QuestConditionOperator.GreaterThanOrEqual);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void CombatCondition_HpBelowThreshold_GreaterThanOrEqual_Fail()
        {
            _ctx.PlayerCurrentHp = 20;
            _ctx.PlayerMaxHp = 100;
            var c = CombatCondition("c_hp_high", "50", QuestConditionOperator.GreaterThanOrEqual);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailedConditionIds.Contains("c_hp_high"));
        }

        [Test]
        public void CombatCondition_HpAtOrBelowThreshold_LessThanOrEqual_Pass()
        {
            _ctx.PlayerCurrentHp = 15;
            _ctx.PlayerMaxHp = 100;
            var c = CombatCondition("c_hp_low", "20", QuestConditionOperator.LessThanOrEqual);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void CombatCondition_HpAboveThreshold_LessThanOrEqual_Fail()
        {
            _ctx.PlayerCurrentHp = 90;
            _ctx.PlayerMaxHp = 100;
            var c = CombatCondition("c_hp_low", "20", QuestConditionOperator.LessThanOrEqual);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void CombatCondition_NoSnapshotInContext_ExplicitFailure_NeverTrue()
        {
            // Context not populated with PlayerCurrentHp/PlayerMaxHp — must fail explicitly,
            // never silently pass like the old "=> true" behavior.
            var c = CombatCondition("c_hp_any", "50", QuestConditionOperator.GreaterThanOrEqual);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailedConditionIds.Contains("c_hp_any"));
            Assert.IsTrue(result.KnownFailureReasons.Exists(r => r.Contains("Combat snapshot indisponivel")));
        }

        [Test]
        public void CombatCondition_ZeroMaxHp_TreatedAsNoSnapshot_ExplicitFailure()
        {
            _ctx.PlayerCurrentHp = 0;
            _ctx.PlayerMaxHp = 0;
            var c = CombatCondition("c_hp_any", "50", QuestConditionOperator.GreaterThanOrEqual);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void CombatCondition_InvalidExpectedValue_ExplicitFailure_NotTrue()
        {
            _ctx.PlayerCurrentHp = 50;
            _ctx.PlayerMaxHp = 100;
            var c = CombatCondition("c_hp_bad", "not_a_number", QuestConditionOperator.GreaterThanOrEqual);
            var result = _resolver.Evaluate(c, _ctx);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void FutureConditions_StillPass_AntiRegression_SocialPetCompanion()
        {
            var social = new QuestConditionDefinition { ConditionId = "c_social", ConditionType = QuestConditionType.SocialConditionFuture };
            var pet = new QuestConditionDefinition { ConditionId = "c_pet", ConditionType = QuestConditionType.PetConditionFuture };
            var companion = new QuestConditionDefinition { ConditionId = "c_companion", ConditionType = QuestConditionType.CompanionConditionFuture };

            Assert.IsTrue(_resolver.Evaluate(social, _ctx).Success, "SocialConditionFuture must still Pass (anti-regression)");
            Assert.IsTrue(_resolver.Evaluate(pet, _ctx).Success, "PetConditionFuture must still Pass (anti-regression)");
            Assert.IsTrue(_resolver.Evaluate(companion, _ctx).Success, "CompanionConditionFuture must still Pass (anti-regression)");
        }
    }
}
