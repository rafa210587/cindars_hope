using System.Collections.Generic;

namespace CindarsHope.Quests
{
    public class QuestDefinitionValidationIssue
    {
        public string QuestId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class QuestDefinitionValidator
    {
        public List<QuestDefinitionValidationIssue> Validate(QuestDefinition def)
        {
            var issues = new List<QuestDefinitionValidationIssue>();
            if (def == null) { issues.Add(new QuestDefinitionValidationIssue { Code = "QUEST_NULL", IsBlocker = true }); return issues; }

            if (string.IsNullOrEmpty(def.QuestId))
                issues.Add(new QuestDefinitionValidationIssue { QuestId = def.QuestId, Code = "QUEST_NO_ID", Message = "QuestId is empty", IsBlocker = true });

            // Main quest must not expire
            if (def.Category == QuestCategory.Main && def.ExpiryRuleIds.Count > 0)
                issues.Add(new QuestDefinitionValidationIssue { QuestId = def.QuestId, Code = "QUEST_MAIN_EXPIRABLE", Message = $"Main quest '{def.QuestId}' cannot have expiry rules", IsBlocker = true });

            // Future categories must not have objectives yet
            if ((def.Category == QuestCategory.SocialFuture || def.Category == QuestCategory.PetFuture) && def.StepIds.Count > 0)
                issues.Add(new QuestDefinitionValidationIssue { QuestId = def.QuestId, Code = "QUEST_FUTURE_HAS_STEPS", Message = $"Future category quest '{def.QuestId}' should not have steps yet", IsBlocker = false });

            // Hidden quests should have spoiler tier
            if (def.IsHidden && def.SpoilerTier == 0)
                issues.Add(new QuestDefinitionValidationIssue { QuestId = def.QuestId, Code = "QUEST_HIDDEN_NO_SPOILER", Message = $"Hidden quest '{def.QuestId}' should have SpoilerTier > 0", IsBlocker = false });

            // Main progression flag reference only
            if (def.Category == QuestCategory.Main)
            {
                foreach (var flagId in def.QuestFlagGrantIds)
                    if (flagId != null && (flagId.ToLowerInvariant().Contains("mainprogression") || flagId.ToLowerInvariant().Contains("main_progression")))
                        issues.Add(new QuestDefinitionValidationIssue { QuestId = def.QuestId, Code = "QUEST_FLAG_MAIN_PROGRESSION_SWALLOW", Message = $"Quest '{def.QuestId}' grants flag '{flagId}' that looks like MainProgression state; use MainProgressionSection instead", IsBlocker = true });
            }

            return issues;
        }
    }
}
