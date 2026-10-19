using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests.Festivals;
using CindarsHope.Quests.CaveContracts;

namespace CindarsHope.Tests.EditMode.Quests
{
    [TestFixture]
    public class FestivalCaveContractAdapterTests
    {
        private FestivalQuestAdapter _festivalAdapter;
        private CaveContractValidator _contractValidator;

        [SetUp]
        public void SetUp()
        {
            _festivalAdapter = new FestivalQuestAdapter();
            _contractValidator = new CaveContractValidator();
        }

        private FestivalQuestDefinition FestivalDef(FestivalExpiryPolicyType policy = FestivalExpiryPolicyType.ExpireAtFestivalEnd) =>
            new FestivalQuestDefinition
            {
                FestivalQuestId = "fq_001", QuestId = "quest_fq_001", FestivalId = "festival_harvest",
                KnownStartDay = 10, KnownEndDay = 15, ExpiryPolicy = policy
            };

        private CaveContractDefinition BasicContract() => new CaveContractDefinition
        {
            CaveContractId = "cc_001", QuestId = "quest_cc_001",
            ContractType = CaveContractType.DefeatEnemyFamily,
            RequiredEnemyFamilyId = "family_spider", RequiredQuantity = 5,
            FallbackPolicy = CaveContractFallbackPolicy.GuaranteedSpawn
        };

        // ---- Festival Tests ----

        [Test]
        public void Festival_ExpireAtFestivalEnd_WhenFestivalEnded()
        {
            var def = FestivalDef(FestivalExpiryPolicyType.ExpireAtFestivalEnd);
            Assert.IsTrue(_festivalAdapter.IsExpired(def, currentDay: 16, festivalEndDay: 15, festivalEnded: true));
        }

        [Test]
        public void Festival_ExpireAtFestivalEnd_NotExpiredWhileActive()
        {
            var def = FestivalDef(FestivalExpiryPolicyType.ExpireAtFestivalEnd);
            Assert.IsFalse(_festivalAdapter.IsExpired(def, currentDay: 12, festivalEndDay: 15, festivalEnded: false));
        }

        [Test]
        public void Festival_RemainAvailableForTurnIn_NeverExpires()
        {
            var def = FestivalDef(FestivalExpiryPolicyType.RemainAvailableForTurnIn);
            Assert.IsFalse(_festivalAdapter.IsExpired(def, currentDay: 100, festivalEndDay: 15, festivalEnded: true));
        }

        [Test]
        public void Festival_NeverExpireStoryOnly_NeverExpires()
        {
            var def = FestivalDef(FestivalExpiryPolicyType.NeverExpireStoryOnly);
            Assert.IsFalse(_festivalAdapter.IsExpired(def, currentDay: 9999, festivalEndDay: 1, festivalEnded: true));
        }

        [Test]
        public void Festival_KnownTiming_ProjectedCorrectly()
        {
            var def = FestivalDef();
            var (start, end) = _festivalAdapter.GetKnownTiming(def);
            Assert.AreEqual(10, start);
            Assert.AreEqual(15, end);
        }

        [Test]
        public void Festival_CanExpire_WhenPolicyIsExpiry()
        {
            Assert.IsTrue(FestivalDef(FestivalExpiryPolicyType.ExpireAtFestivalEnd).CanExpire());
            Assert.IsFalse(FestivalDef(FestivalExpiryPolicyType.NeverExpireStoryOnly).CanExpire());
        }

        // ---- Cave Contract Tests ----

        [Test]
        public void CaveContract_ObjectiveAdapter_DefeatFamily_MapsCorrectly()
        {
            Assert.AreEqual("DefeatEnemyFamily", CaveContractObjectiveAdapter.GetObjectiveType(CaveContractType.DefeatEnemyFamily));
            Assert.AreEqual("OnEnemyFamilyDefeated", CaveContractObjectiveAdapter.GetTriggerEventName(CaveContractType.DefeatEnemyFamily));
        }

        [Test]
        public void CaveContract_ObjectiveAdapter_NeverMutatesCaveRunSeed()
        {
            // All contract types must NEVER mutate CaveRunSeed
            foreach (CaveContractType t in System.Enum.GetValues(typeof(CaveContractType)))
                Assert.IsFalse(CaveContractObjectiveAdapter.MutatesCaveRunSeed(t), $"Type {t} must not mutate seed");
        }

        [Test]
        public void CaveContract_Validator_Valid_NoIssues()
        {
            var issues = _contractValidator.Validate(BasicContract());
            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void CaveContract_Validator_NoQuestId_Blocker()
        {
            var c = BasicContract();
            c.QuestId = null;
            Assert.IsTrue(_contractValidator.Validate(c).Exists(i => i.Code == "CONTRACT_NO_QUEST_ID" && i.IsBlocker));
        }

        [Test]
        public void CaveContract_Validator_DefeatNoEnemyId_Blocker()
        {
            var c = BasicContract();
            c.RequiredEnemyFamilyId = null;
            Assert.IsTrue(_contractValidator.Validate(c).Exists(i => i.Code == "CONTRACT_DEFEAT_NO_ENEMY_ID" && i.IsBlocker));
        }

        [Test]
        public void CaveContract_Validator_EliteNoFallback_Warning()
        {
            var c = BasicContract();
            c.ContractType = CaveContractType.DefeatElite;
            c.RequiredEnemyFamilyId = "family_elite";
            c.FallbackPolicy = CaveContractFallbackPolicy.None;
            var issues = _contractValidator.Validate(c);
            Assert.IsTrue(issues.Exists(i => i.Code == "CONTRACT_ELITE_NO_FALLBACK" && !i.IsBlocker));
        }

        [Test]
        public void CaveContract_Validator_DepthContractZeroDepth_Blocker()
        {
            var c = new CaveContractDefinition
            {
                CaveContractId = "cc_depth", QuestId = "quest_cc_depth",
                ContractType = CaveContractType.ReachCaveDepth, RequiredCaveAccessDepth = 0, RequiredQuantity = 1
            };
            Assert.IsTrue(_contractValidator.Validate(c).Exists(i => i.Code == "CONTRACT_DEPTH_ZERO" && i.IsBlocker));
        }

        [Test]
        public void CaveContract_Validator_BossRepeatNoUniqueReward_Warning()
        {
            var c = BasicContract();
            c.ContractType = CaveContractType.DefeatBoss;
            c.RequiredBossId = "boss_vaelrion";
            c.RequiredEnemyFamilyId = null;
            c.RepeatPolicy = new CaveContractRepeatPolicy { CanRepeat = true, UniqueRewardOnFirstOnly = false };
            var issues = _contractValidator.Validate(c);
            Assert.IsTrue(issues.Exists(i => i.Code == "CONTRACT_BOSS_REPEAT_UNIQUE_REWARD"));
        }

        [Test]
        public void CaveContract_RequiresFallback_EliteWithoutFallback()
        {
            var c = BasicContract();
            c.ContractType = CaveContractType.DefeatElite;
            c.FallbackPolicy = CaveContractFallbackPolicy.None;
            Assert.IsTrue(c.RequiresFallback());
        }
    }
}
