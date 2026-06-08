using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Flags;

namespace CindarsHope.Tests.EditMode.Quests
{
    [TestFixture]
    public class QuestRewardIdempotencyTests
    {
        private QuestRewardApplicator _applicator;
        private QuestFlagRegistry _registry;
        private QuestFlagService _flagService;

        [SetUp]
        public void SetUp()
        {
            _registry = new QuestFlagRegistry();
            _flagService = new QuestFlagService(_registry);
            _registry.Register(new QuestFlagDefinition
            {
                FlagId = "flag_shop_night_unlocked", FlagType = QuestFlagType.Boolean,
                Scope = QuestFlagScope.Shop, Visibility = QuestFlagVisibility.PublicKnown,
                OwnerSystem = "RewardEngine", CanBeGrantedByReward = true
            });
            _applicator = new QuestRewardApplicator(_flagService);
        }

        private QuestRewardApplicationContext Ctx(string questId = "quest_01") => new QuestRewardApplicationContext
        {
            QuestId = questId, Day = 1, AlreadyGrantedRewardIds = new HashSet<string>(),
            AlreadyGrantedFlagIds = new HashSet<string>()
        };

        [Test]
        public void Reward_Gold_Applied()
        {
            var reward = new QuestRewardDefinition { RewardId = "rw_gold_100", RewardType = QuestRewardType.Gold, Quantity = 100 };
            var result = _applicator.Apply(reward, Ctx());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(100, result.GrantedGold);
        }

        [Test]
        public void Reward_AlreadyGranted_Skipped()
        {
            var reward = new QuestRewardDefinition { RewardId = "rw_gold_100", RewardType = QuestRewardType.Gold };
            var ctx = Ctx();
            ctx.AlreadyGrantedRewardIds.Add("rw_gold_100");
            var result = _applicator.Apply(reward, ctx);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.SkippedAlreadyGranted);
        }

        [Test]
        public void Reward_Item_GrantedList()
        {
            var reward = new QuestRewardDefinition { RewardId = "rw_ironore_5", RewardType = QuestRewardType.Item, TargetId = "item_ironore", Quantity = 5 };
            var result = _applicator.Apply(reward, Ctx());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, result.GrantedItems.Count);
        }

        [Test]
        public void Reward_FlagGrant_IdempotentOnAlreadyGranted()
        {
            var reward = new QuestRewardDefinition
            { RewardId = "rw_flag_shop", RewardType = QuestRewardType.QuestFlagGrant, GrantedFlagId = "flag_shop_night_unlocked" };
            var ctx = Ctx();
            ctx.AlreadyGrantedFlagIds.Add("flag_shop_night_unlocked");
            var result = _applicator.Apply(reward, ctx);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.SkippedAlreadyGranted);
        }

        [Test]
        public void Reward_FlagGrant_Applied()
        {
            var reward = new QuestRewardDefinition
            { RewardId = "rw_flag_shop", RewardType = QuestRewardType.QuestFlagGrant, GrantedFlagId = "flag_shop_night_unlocked" };
            var result = _applicator.Apply(reward, Ctx());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.GrantedFlagIds.Contains("flag_shop_night_unlocked"));
        }

        [Test]
        public void Reward_FutureType_Deferred()
        {
            var reward = new QuestRewardDefinition { RewardId = "rw_pet_future", RewardType = QuestRewardType.PetUnlockFuture };
            var result = _applicator.Apply(reward, Ctx());
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.ResidualRisk);
        }

        [Test]
        public void Reward_FonteUpgrade_RequiresAdapter()
        {
            var reward = new QuestRewardDefinition { RewardId = "rw_fonte_upgrade", RewardType = QuestRewardType.FonteUpgrade };
            var ctx = Ctx();
            ctx.HasFonteAdapter = false;
            var result = _applicator.Apply(reward, ctx);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("FonteAnya"));
        }

        [Test]
        public void Reward_UnlockShop_GrantedUnlockedIds()
        {
            var reward = new QuestRewardDefinition { RewardId = "rw_shop_unlock", RewardType = QuestRewardType.ShopUnlock, TargetId = "shop_yael_night" };
            var result = _applicator.Apply(reward, Ctx());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.UnlockedIds.Contains("shop_yael_night"));
        }

        [Test]
        public void Reward_DryRun_NoSave()
        {
            var reward = new QuestRewardDefinition { RewardId = "rw_gold_dry", RewardType = QuestRewardType.Gold, Quantity = 50 };
            var ctx = Ctx();
            ctx.DryRun = true;
            var result = _applicator.Apply(reward, ctx);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.RequiresSave);
        }

        [Test]
        public void Reward_NullDefinition_Fail()
        {
            var result = _applicator.Apply(null, Ctx());
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Reward_FlagGrant_NoFlagId_Fail()
        {
            var reward = new QuestRewardDefinition { RewardId = "rw_flag_bad", RewardType = QuestRewardType.QuestFlagGrant };
            var result = _applicator.Apply(reward, Ctx());
            Assert.IsFalse(result.Success);
        }
    }
}
