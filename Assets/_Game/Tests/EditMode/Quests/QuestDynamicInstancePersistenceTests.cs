using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.Quests;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Quests
{
    public class QuestDynamicInstancePersistenceTests
    {
        [Test]
        public void UnknownDynamicCatalog_PreservesPendingRewardsWithoutSpecialCase()
        {
            var instance = new QuestInstance
            {
                QuestId = "future_dynamic_quest",
                QuestTemplateId = "future_template",
                Source = QuestSource.Mural,
                TargetId = "future_target",
                Quantity = 2,
                QuestLevel = 12,
                RewardXp = 90,
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    new QuestRewardDefinition
                    {
                        RewardId = "reward_future_flag",
                        RewardType = QuestRewardType.QuestFlagGrant,
                        GrantedFlagId = "flag_future_dynamic",
                        Quantity = 1,
                        IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                    }
                }
            };
            var record = new QuestStateRecord();

            QuestDynamicInstancePersistence.HydrateRecord(record, instance);
            var restored = QuestDynamicInstancePersistence.RestoreRewards(record);

            Assert.IsTrue(record.IsDynamicInstance);
            Assert.AreEqual(QuestSource.Mural, (QuestSource)record.Source);
            Assert.AreEqual(1, restored.Count);
            Assert.AreEqual("reward_future_flag", restored[0].RewardId);
            Assert.AreEqual("flag_future_dynamic", restored[0].GrantedFlagId);
            Assert.AreEqual(QuestRewardType.QuestFlagGrant, restored[0].RewardType);
        }

        [Test]
        public void LegacyNpcRecord_RebuildsCatalogRewards()
        {
            var record = new QuestStateRecord
            {
                QuestId = "sq_brumdar_1",
                Source = (int)QuestSource.Npc,
                IsDynamicInstance = true
            };

            var restored = QuestDynamicInstancePersistence.RestoreRewards(record);

            Assert.IsNotEmpty(restored);
            Assert.IsTrue(restored.Exists(r => r.GrantedFlagId == "sq_brumdar_1_done"));
        }
    }
}
