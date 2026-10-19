using System.Collections.Generic;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Save;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Aggregated outcome of granting a quest's rewards on turn-in: gold/items/flags actually
    /// given and fable_34 scaled XP given (all excluding rewards skipped as already-granted).
    /// </summary>
    public sealed class QuestTurnInGrantResult
    {
        public int GoldGiven;
        public int XpGiven;
        public List<string> ItemsGiven = new List<string>();
        public List<string> FlagsGranted = new List<string>();
    }

    /// <summary>
    /// Applies a quest's reward list idempotently on turn-in. Delegates per-reward application to
    /// <see cref="QuestRewardApplicator"/>, then grants gold/items through the injected access
    /// ports, records granted reward/flag ids on the record (idempotency guard via
    /// GrantedRewardIds/GrantedFlagIds), and applies the fable_34 scaled-XP hook for dynamic
    /// instances via a synthetic reward id. Extracted from QuestService.TurnIn to isolate the
    /// reward-granting concern from accept/progress/turn-in lifecycle orchestration.
    /// </summary>
    public sealed class QuestTurnInRewardGranter
    {
        private const string InstanceXpRewardId = "reward_instance_xp";

        private readonly QuestRewardApplicator _rewardApplicator;
        private readonly IQuestInventoryAccess _inventoryAccess;
        private readonly IQuestGoldAccess _goldAccess;
        private readonly IQuestProgressionAccess _progressionAccess;

        public QuestTurnInRewardGranter(
            QuestRewardApplicator rewardApplicator,
            IQuestInventoryAccess inventoryAccess,
            IQuestGoldAccess goldAccess,
            IQuestProgressionAccess progressionAccess)
        {
            _rewardApplicator = rewardApplicator;
            _inventoryAccess = inventoryAccess;
            _goldAccess = goldAccess;
            _progressionAccess = progressionAccess;
        }

        public QuestTurnInGrantResult Grant(string questId, IReadOnlyList<QuestRewardDefinition> rewards, QuestStateRecord record)
        {
            var result = new QuestTurnInGrantResult();
            var alreadyGranted = new HashSet<string>(record.GrantedRewardIds);
            var alreadyGrantedFlags = new HashSet<string>(record.GrantedFlagIds);

            foreach (var reward in rewards)
            {
                var ctx = new QuestRewardApplicationContext
                {
                    QuestId = questId,
                    RewardId = reward.RewardId,
                    AlreadyGrantedRewardIds = alreadyGranted,
                    AlreadyGrantedFlagIds = alreadyGrantedFlags
                };
                var applied = _rewardApplicator.Apply(reward, ctx);
                if (!applied.Success || applied.SkippedAlreadyGranted) continue;

                // Apply Gold
                if (applied.GrantedGold > 0 && _goldAccess != null)
                {
                    _goldAccess.AddGold(applied.GrantedGold);
                    result.GoldGiven += applied.GrantedGold;
                }

                // Apply Items
                foreach (var itemId in applied.GrantedItems)
                {
                    if (_inventoryAccess != null && _inventoryAccess.TryAddItem(itemId, 1))
                        result.ItemsGiven.Add(itemId);
                }

                // Record reward as granted (idempotency)
                if (!string.IsNullOrEmpty(applied.GrantedRewardId) && !record.GrantedRewardIds.Contains(applied.GrantedRewardId))
                    record.GrantedRewardIds.Add(applied.GrantedRewardId);

                // Record flags
                foreach (var flagId in applied.GrantedFlagIds)
                {
                    if (!record.GrantedFlagIds.Contains(flagId))
                        record.GrantedFlagIds.Add(flagId);
                    result.FlagsGranted.Add(flagId);
                }
            }

            // fable_34 — scaled XP for dynamic instances (board contracts). Idempotent via a
            // synthetic reward id recorded in GrantedRewardIds, so a reload + re-turn-in cannot
            // re-grant XP. XP is not a generic QuestRewardType, so it is applied through the
            // progression hook here (single point: the amount was scaled once at generation).
            if (record.IsDynamicInstance && record.InstanceRewardXp > 0 &&
                !record.GrantedRewardIds.Contains(InstanceXpRewardId))
            {
                _progressionAccess?.AddXp(record.InstanceRewardXp);
                result.XpGiven = record.InstanceRewardXp;
                record.GrantedRewardIds.Add(InstanceXpRewardId);
            }

            return result;
        }
    }
}
