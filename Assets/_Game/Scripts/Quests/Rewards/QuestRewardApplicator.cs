using System.Collections.Generic;
using CindarsHope.Quests.Flags;

namespace CindarsHope.Quests.Rewards
{
    public class QuestRewardApplicationContext
    {
        public string QuestId { get; set; }
        public string StepId { get; set; }
        public string ObjectiveId { get; set; }
        public string RewardId { get; set; }
        public string ActorId { get; set; }
        public int Day { get; set; }
        public int Time { get; set; }
        // GrantedRewardIds from QuestState — for idempotency check
        public HashSet<string> AlreadyGrantedRewardIds { get; set; } = new HashSet<string>();
        public HashSet<string> AlreadyGrantedFlagIds { get; set; } = new HashSet<string>();
        public bool DryRun { get; set; } = false;
        // Adapters (null = system not available yet)
        public bool HasFonteAdapter { get; set; } = false;
        public bool HasMainProgressionAdapter { get; set; } = false;
    }

    public class QuestRewardApplicator
    {
        private readonly QuestFlagService _flagService;

        public QuestRewardApplicator(QuestFlagService flagService)
        {
            _flagService = flagService;
        }

        public QuestRewardApplicationResult Apply(QuestRewardDefinition reward, QuestRewardApplicationContext ctx)
        {
            if (reward == null) return QuestRewardApplicationResult.Fail("Reward definition is null");
            if (ctx == null) return QuestRewardApplicationResult.Fail("Context is null", reward.RewardId);

            // Future rewards: defer gracefully
            if (reward.IsFutureReward())
                return QuestRewardApplicationResult.FutureDeferred(reward.RewardId);

            // Idempotency: skip if already granted
            if (ctx.AlreadyGrantedRewardIds.Contains(reward.RewardId))
                return QuestRewardApplicationResult.AlreadyGranted(reward.RewardId);

            // Fonte/MainProgression rewards require adapters
            if (reward.RequiresFonteAdapter() && !ctx.HasFonteAdapter)
                return QuestRewardApplicationResult.Fail($"Fonte reward '{reward.RewardId}' requires FonteAnya adapter; use explicit FonteAnya hooks", reward.RewardId);

            if (ctx.DryRun)
                return new QuestRewardApplicationResult { Success = true, GrantedRewardId = reward.RewardId, RequiresSave = false };

            return reward.RewardType switch
            {
                QuestRewardType.Gold => ApplyGold(reward, ctx),
                QuestRewardType.Item => ApplyItem(reward, ctx),
                QuestRewardType.QuestFlagGrant => ApplyFlagGrant(reward, ctx),
                QuestRewardType.QuestFlagClear => ApplyFlagClear(reward, ctx),
                QuestRewardType.Recipe => ApplyUnlock(reward, ctx),
                QuestRewardType.ToolUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.EquipmentUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.SpellUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.SkillTreeUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.AreaUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.CaveDepthUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.ShopUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.ShopStockUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.DialogueUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.NpcScheduleUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.FestivalUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.BestiaryEntryUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.KnowledgeUnlock => ApplyUnlock(reward, ctx),
                QuestRewardType.SkillPoint => ApplyGold(reward, ctx), // same structure as gold but for skill points
                _ => QuestRewardApplicationResult.Fail($"Unhandled reward type {reward.RewardType}", reward.RewardId)
            };
        }

        private QuestRewardApplicationResult ApplyGold(QuestRewardDefinition r, QuestRewardApplicationContext ctx)
        {
            return new QuestRewardApplicationResult
            {
                Success = true, GrantedRewardId = r.RewardId,
                GrantedGold = r.Quantity, RequiresSave = true, RequiresNotification = true
            };
        }

        private QuestRewardApplicationResult ApplyItem(QuestRewardDefinition r, QuestRewardApplicationContext ctx)
        {
            var items = new List<string>();
            for (int i = 0; i < r.Quantity; i++) items.Add(r.TargetId);
            return new QuestRewardApplicationResult
            {
                Success = true, GrantedRewardId = r.RewardId,
                GrantedItems = items, RequiresSave = true, RequiresNotification = true
            };
        }

        private QuestRewardApplicationResult ApplyFlagGrant(QuestRewardDefinition r, QuestRewardApplicationContext ctx)
        {
            var flagId = r.GrantedFlagId ?? r.TargetId;
            if (string.IsNullOrEmpty(flagId))
                return QuestRewardApplicationResult.Fail("QuestFlagGrant reward has no GrantedFlagId or TargetId", r.RewardId);

            // Idempotent: flag already granted
            if (ctx.AlreadyGrantedFlagIds.Contains(flagId))
                return QuestRewardApplicationResult.AlreadyGranted(r.RewardId);

            // Apply via QuestFlagService if available
            if (_flagService != null)
                _flagService.GrantFlag(flagId, "RewardEngine");

            return new QuestRewardApplicationResult
            {
                Success = true, GrantedRewardId = r.RewardId,
                GrantedFlagIds = new List<string> { flagId }, RequiresSave = true, RequiresNotification = false
            };
        }

        private QuestRewardApplicationResult ApplyFlagClear(QuestRewardDefinition r, QuestRewardApplicationContext ctx)
        {
            var flagId = r.GrantedFlagId ?? r.TargetId;
            if (_flagService != null)
                _flagService.ClearFlag(flagId, "RewardEngine");

            return new QuestRewardApplicationResult
            {
                Success = true, GrantedRewardId = r.RewardId,
                GrantedFlagIds = new List<string>(), RequiresSave = true, RequiresNotification = false
            };
        }

        private QuestRewardApplicationResult ApplyUnlock(QuestRewardDefinition r, QuestRewardApplicationContext ctx)
        {
            return new QuestRewardApplicationResult
            {
                Success = true, GrantedRewardId = r.RewardId,
                UnlockedIds = new List<string> { r.TargetId }, RequiresSave = true, RequiresNotification = true
            };
        }
    }
}
