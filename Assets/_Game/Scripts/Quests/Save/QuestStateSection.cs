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

        // fable_34 — cave-secret quests discovered by the player (offered by peaceful creature /
        // wandering merchant). A secret quest is listed in the Quest Log only after discovery.
        public List<string> DiscoveredSecretQuestIds { get; set; } = new List<string>();
        // fable_34 — main-quest acts already rewarded with +1 skill point (idempotency guard).
        public List<string> RewardedMainActIds { get; set; } = new List<string>();

        /// <summary>fable_34 — records a secret quest as discovered (idempotent). Returns true if newly added.</summary>
        public bool MarkSecretDiscovered(string questId)
        {
            if (string.IsNullOrEmpty(questId) || DiscoveredSecretQuestIds.Contains(questId)) return false;
            DiscoveredSecretQuestIds.Add(questId);
            return true;
        }

        public bool IsSecretDiscovered(string questId)
            => !string.IsNullOrEmpty(questId) && DiscoveredSecretQuestIds.Contains(questId);

        /// <summary>fable_34 — true if the main act was already rewarded (idempotent skill point).</summary>
        public bool IsActRewarded(string actId)
            => !string.IsNullOrEmpty(actId) && RewardedMainActIds.Contains(actId);

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
