using System.Collections.Generic;

namespace CindarsHope.Quests.Rewards
{
    public class QuestRewardApplicationResult
    {
        public bool Success { get; set; }
        public bool SkippedAlreadyGranted { get; set; }
        public string FailureReason { get; set; }
        public string GrantedRewardId { get; set; }
        public List<string> GrantedFlagIds { get; set; } = new List<string>();
        public List<string> GrantedItems { get; set; } = new List<string>();
        public int GrantedGold { get; set; }
        public List<string> UnlockedIds { get; set; } = new List<string>();
        public bool RequiresSave { get; set; } = true;
        public bool RequiresNotification { get; set; } = true;
        public string ResidualRisk { get; set; }

        public static QuestRewardApplicationResult AlreadyGranted(string rewardId) =>
            new QuestRewardApplicationResult { Success = true, SkippedAlreadyGranted = true, GrantedRewardId = rewardId, RequiresSave = false, RequiresNotification = false };

        public static QuestRewardApplicationResult Fail(string reason, string rewardId = null) =>
            new QuestRewardApplicationResult { Success = false, FailureReason = reason, GrantedRewardId = rewardId };

        public static QuestRewardApplicationResult FutureDeferred(string rewardId) =>
            new QuestRewardApplicationResult { Success = true, SkippedAlreadyGranted = false, GrantedRewardId = rewardId, RequiresSave = false, RequiresNotification = false, ResidualRisk = $"Future reward '{rewardId}' deferred until system available" };
    }
}
