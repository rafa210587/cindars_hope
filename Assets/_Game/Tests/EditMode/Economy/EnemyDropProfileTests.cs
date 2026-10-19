using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Enemy.Drops;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class EnemyDropProfileTests
    {
        private EnemyDropResolver _resolver;

        [SetUp]
        public void SetUp() => _resolver = new EnemyDropResolver();

        private EnemyDropProfile CommonProfile() => new EnemyDropProfile
        {
            EnemyDropProfileId = "profile_slime",
            EnemyFamilyId = "slime",
            NativeFloorMin = 1, NativeFloorMax = 3,
            CommonMaterialTableId = "loot_slime_common",
            ThematicComponentTableId = "loot_slime_thematic",
            RareComponentTableId = "loot_slime_rare"
        };

        private DefeatRewardContext CommonCtx(int caveLevel = 1) => new DefeatRewardContext
        {
            EnemyId = "enemy_slime_green",
            EnemyFamilyId = "slime",
            CaveLevel = caveLevel
        };

        [Test]
        public void CommonEnemy_ReturnsCommon_AndThematic()
        {
            var result = _resolver.ResolveCommon(CommonProfile(), CommonCtx());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.GrantedLootTableIds.Contains("loot_slime_common"));
            Assert.IsTrue(result.GrantedLootTableIds.Contains("loot_slime_thematic"));
        }

        [Test]
        public void CommonEnemy_NoNormalLoot_Fails()
        {
            var p = CommonProfile();
            p.NoNormalLoot = true;
            var result = _resolver.ResolveCommon(p, CommonCtx());
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Elite_ReturnsEliteThematic()
        {
            var elite = new EliteDropProfile
            {
                EliteDropProfileId = "elite_slime",
                BaseEnemyFamilyId = "slime",
                GuaranteedOrNearGuaranteedComponentItemId = "item_slime_core",
                RareDropTableId = "loot_elite_slime_rare"
            };
            var result = _resolver.ResolveElite(elite, CommonCtx());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.GrantedLootTableIds.Contains("elite_thematic_slime"));
        }

        [Test]
        public void Boss_FirstTime_Reward_Applied()
        {
            var profile = new BossRewardProfile
            {
                BossRewardProfileId = "boss_ooze_king",
                BossId = "boss_ooze_king",
                FirstTimeRewardTableId = "loot_boss_ooze_first",
                RepeatRewardTableId = "loot_boss_ooze_repeat",
                GrantedStoryFlags = new List<string> { "flag_ooze_king_defeated" }
            };
            var state = new BossDefeatState { BossId = "boss_ooze_king", IsDefeated = false, FirstTimeRewardConsumed = false };

            var result = _resolver.ResolveBoss(profile, state);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.FirstTimeRewardApplied);
            Assert.IsTrue(result.GrantedLootTableIds.Contains("loot_boss_ooze_first"));
            Assert.IsTrue(result.GrantedStoryFlags.Contains("flag_ooze_king_defeated"));
            Assert.IsTrue(state.FirstTimeRewardConsumed, "State must be marked consumed");
        }

        [Test]
        public void Boss_RepeatKill_UsesRepeatTable()
        {
            var profile = new BossRewardProfile
            {
                BossRewardProfileId = "boss_ooze_king",
                BossId = "boss_ooze_king",
                FirstTimeRewardTableId = "loot_boss_ooze_first",
                RepeatRewardTableId = "loot_boss_ooze_repeat"
            };
            var state = new BossDefeatState { BossId = "boss_ooze_king", FirstTimeRewardConsumed = true, DefeatCount = 1 };

            var result = _resolver.ResolveBoss(profile, state);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.FirstTimeRewardApplied);
            Assert.IsTrue(result.RepeatRewardUsed);
            Assert.IsTrue(result.GrantedLootTableIds.Contains("loot_boss_ooze_repeat"));
            Assert.IsFalse(result.GrantedLootTableIds.Contains("loot_boss_ooze_first"), "First time loot must not appear on repeat");
        }

        [Test]
        public void Boss_FirstTimeReward_NeverGrantedTwice()
        {
            var profile = new BossRewardProfile
            {
                BossId = "boss_ooze_king",
                FirstTimeRewardTableId = "loot_boss_ooze_first",
                RepeatRewardTableId = "loot_boss_ooze_repeat"
            };
            var state = new BossDefeatState { BossId = "boss_ooze_king" };

            _resolver.ResolveBoss(profile, state); // first kill
            var second = _resolver.ResolveBoss(profile, state); // second kill
            Assert.IsFalse(second.FirstTimeRewardApplied, "FirstTime reward must not apply on second kill");
            Assert.IsTrue(second.RepeatRewardUsed);
        }

        [Test]
        public void Boss_LoreRewards_OnlyFirstTime()
        {
            var profile = new BossRewardProfile
            {
                BossId = "boss_ooze_king",
                FirstTimeRewardTableId = "loot_boss_first",
                RepeatRewardTableId = "loot_boss_repeat",
                LoreRewardIds = new List<string> { "lore_ooze_king_fragment" }
            };
            var state = new BossDefeatState { BossId = "boss_ooze_king" };

            var firstResult = _resolver.ResolveBoss(profile, state);
            Assert.IsTrue(firstResult.GrantedLootTableIds.Contains("lore_ooze_king_fragment"));

            var repeatResult = _resolver.ResolveBoss(profile, state);
            Assert.IsFalse(repeatResult.GrantedLootTableIds.Contains("lore_ooze_king_fragment"), "Lore rewards only on first time");
        }

        [Test]
        public void BossDefeatState_DefeatCount_Increments()
        {
            var profile = new BossRewardProfile
            {
                BossId = "boss_test",
                FirstTimeRewardTableId = "t1",
                RepeatRewardTableId = "t2"
            };
            var state = new BossDefeatState { BossId = "boss_test" };
            _resolver.ResolveBoss(profile, state);
            _resolver.ResolveBoss(profile, state);
            Assert.AreEqual(2, state.DefeatCount);
        }
    }
}
