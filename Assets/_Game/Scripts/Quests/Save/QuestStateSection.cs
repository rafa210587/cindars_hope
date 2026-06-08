using System.Collections.Generic;

namespace CindarsHope.Quests.Save
{
    // Top-level quest save section — separate from MainProgressionSection and FonteAnyaSection
    public class QuestFlagRecord
    {
        public string FlagId { get; set; }
        public string Value { get; set; }
        public bool WasGranted { get; set; }
    }

    public class QuestStateSection
    {
        public int Version { get; set; } = 1;
        public List<QuestStateRecord> QuestStates { get; set; } = new List<QuestStateRecord>();
        public List<QuestFlagRecord> QuestFlags { get; set; } = new List<QuestFlagRecord>();
        public List<string> GlobalKnownHints { get; set; } = new List<string>();
        public int LastQuestStateNormalizationVersion { get; set; } = 0;
        public int? DebugLastValidatedAt { get; set; }

        public QuestStateRecord GetQuestState(string questId)
        {
            foreach (var q in QuestStates)
                if (q.QuestId == questId) return q;
            return null;
        }

        public bool IsRewardGranted(string questId, string rewardId)
        {
            var qs = GetQuestState(questId);
            return qs != null && qs.GrantedRewardIds.Contains(rewardId);
        }

        public bool IsFlagGranted(string questId, string flagId)
        {
            var qs = GetQuestState(questId);
            return qs != null && qs.GrantedFlagIds.Contains(flagId);
        }
    }
}
