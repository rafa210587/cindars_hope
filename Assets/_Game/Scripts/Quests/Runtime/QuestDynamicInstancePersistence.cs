using System.Collections.Generic;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Save;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>Maps dynamic quest runtime data to simple save records and restores pending rewards.</summary>
    public static class QuestDynamicInstancePersistence
    {
        public static void HydrateRecord(QuestStateRecord record, QuestInstance instance)
        {
            if (record == null || instance == null) return;
            record.IsDynamicInstance = true;
            record.Source = (int)instance.Source;
            record.TemplateId = instance.QuestTemplateId;
            record.InstanceTargetId = instance.TargetId;
            record.InstanceQuantity = instance.Quantity;
            record.QuestLevel = instance.QuestLevel;
            record.InstanceRewardGold = instance.RewardGold;
            record.InstanceRewardXp = instance.RewardXp;
            record.GeneratedForDay = instance.GeneratedForDay;
            if (record.DynamicRewards == null)
                record.DynamicRewards = new List<QuestDynamicRewardRecord>();
            else
                record.DynamicRewards.Clear();

            if (instance.AdditionalRewards == null) return;
            foreach (QuestRewardDefinition reward in instance.AdditionalRewards)
            {
                if (reward == null) continue;
                record.DynamicRewards.Add(new QuestDynamicRewardRecord
                {
                    RewardId = reward.RewardId,
                    RewardType = (int)reward.RewardType,
                    TargetId = reward.TargetId,
                    Quantity = reward.Quantity,
                    GrantedFlagId = reward.GrantedFlagId,
                    IdempotencyPolicy = (int)reward.IdempotencyPolicy
                });
            }
        }

        public static List<QuestRewardDefinition> RestoreRewards(QuestStateRecord record)
        {
            var rewards = new List<QuestRewardDefinition>();
            if (record == null) return rewards;

            if (record.DynamicRewards != null && record.DynamicRewards.Count > 0)
            {
                foreach (QuestDynamicRewardRecord saved in record.DynamicRewards)
                {
                    if (saved == null || string.IsNullOrEmpty(saved.RewardId)) continue;
                    rewards.Add(new QuestRewardDefinition
                    {
                        RewardId = saved.RewardId,
                        RewardType = (QuestRewardType)saved.RewardType,
                        TargetId = saved.TargetId,
                        Quantity = saved.Quantity,
                        GrantedFlagId = saved.GrantedFlagId,
                        IdempotencyPolicy = (RewardIdempotencyPolicy)saved.IdempotencyPolicy
                    });
                }
                return rewards;
            }

            RestoreLegacyCatalogRewards(record, rewards);
            return rewards;
        }

        private static void RestoreLegacyCatalogRewards(QuestStateRecord record,
            List<QuestRewardDefinition> rewards)
        {
            if ((QuestSource)record.Source == QuestSource.Npc)
            {
                var step = NpcChains.NpcQuestChainCatalog.FindByQuestId(record.QuestId);
                var rebuilt = step == null ? null : NpcChains.NpcQuestChainCatalog.BuildInstance(step);
                if (rebuilt?.AdditionalRewards != null) rewards.AddRange(rebuilt.AdditionalRewards);
                return;
            }

            if ((QuestSource)record.Source != QuestSource.CaveContract) return;

            if ((record.TemplateId ?? string.Empty).StartsWith(CaveContracts.CaveContractCatalog.MilestonePrefix) &&
                int.TryParse(record.InstanceTargetId, out var depth))
            {
                rewards.AddRange(CaveContracts.CaveContractCatalog.BuildMilestoneInstance(depth).AdditionalRewards);
            }
            else if (record.TemplateId == CaveContracts.CaveContractCatalog.BossRematchId)
            {
                rewards.Add(new QuestRewardDefinition
                {
                    RewardId = "reward_" + record.QuestId + "_essence",
                    RewardType = QuestRewardType.Item,
                    TargetId = CaveContracts.CaveContractCatalog.EssenceItemForBoss(record.InstanceTargetId),
                    Quantity = 1,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                });
            }
            else if (record.TemplateId == CaveContracts.CaveContractCatalog.NoHitFloorId &&
                     int.TryParse(record.InstanceTargetId, out var level))
            {
                rewards.Add(new QuestRewardDefinition
                {
                    RewardId = "reward_" + record.QuestId + "_title",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = CaveContracts.CaveContractCatalog.NoHitTitleFlag(level),
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                });
                rewards.Add(new QuestRewardDefinition
                {
                    RewardId = "reward_" + record.QuestId + "_charm",
                    RewardType = QuestRewardType.Item,
                    TargetId = "item_accessory_charm_no_hit",
                    Quantity = 1,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                });
            }
        }
    }
}
