using System.Collections.Generic;
using CindarsHope.Quests;

namespace CindarsHope.Quests.Save
{
    public class QuestNormalizationIssue
    {
        public string QuestId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public bool WasRecovered { get; set; }
    }

    // Normalizes QuestStateSection on load — recovers inconsistent states, never hides errors
    public class QuestStateNormalizer
    {
        public List<QuestNormalizationIssue> Normalize(QuestStateSection section)
        {
            var issues = new List<QuestNormalizationIssue>();
            if (section == null) return issues;

            var seenIds = new HashSet<string>();
            foreach (var qs in section.QuestStates)
            {
                if (string.IsNullOrEmpty(qs.QuestId))
                {
                    issues.Add(new QuestNormalizationIssue { Code = "QUEST_NULL_ID", Message = "QuestState has null QuestId", WasRecovered = false });
                    continue;
                }

                if (seenIds.Contains(qs.QuestId))
                {
                    issues.Add(new QuestNormalizationIssue { QuestId = qs.QuestId, Code = "QUEST_DUPLICATE_ID", Message = $"Duplicate QuestId '{qs.QuestId}' in save; second entry kept", WasRecovered = true });
                    continue;
                }
                seenIds.Add(qs.QuestId);

                var state = (QuestStateStatus)qs.State;

                // Terminal state cannot have active step
                if ((state == QuestStateStatus.Completed || state == QuestStateStatus.Failed || state == QuestStateStatus.Expired)
                    && !string.IsNullOrEmpty(qs.CurrentStepId))
                {
                    qs.CurrentStepId = null;
                    issues.Add(new QuestNormalizationIssue { QuestId = qs.QuestId, Code = "QUEST_TERMINAL_HAS_ACTIVE_STEP", Message = $"Quest '{qs.QuestId}' is terminal but had active step; cleared", WasRecovered = true });
                }

                // Completed quest must have IsRewardGiven or at least GrantedRewardIds
                if (state == QuestStateStatus.Completed && qs.CompletedAtDay == null)
                {
                    qs.CompletedAtDay = qs.StartedAtDay; // use start day as fallback
                    issues.Add(new QuestNormalizationIssue { QuestId = qs.QuestId, Code = "QUEST_COMPLETED_NO_DAY", Message = $"Quest '{qs.QuestId}' completed but CompletedAtDay was null; set to StartedAtDay", WasRecovered = true });
                }

                // State cannot be Active without any step
                if (state == QuestStateStatus.Active && string.IsNullOrEmpty(qs.CurrentStepId) && qs.CompletedStepIds.Count == 0)
                {
                    qs.State = (int)QuestStateStatus.Available;
                    issues.Add(new QuestNormalizationIssue { QuestId = qs.QuestId, Code = "QUEST_ACTIVE_NO_STEP", Message = $"Quest '{qs.QuestId}' was Active with no step; demoted to Available", WasRecovered = true });
                }
            }

            section.LastQuestStateNormalizationVersion++;
            return issues;
        }

        public bool HasUnrecoverableIssues(List<QuestNormalizationIssue> issues)
        {
            foreach (var i in issues)
                if (!i.WasRecovered) return true;
            return false;
        }
    }
}
