using System.Collections.Generic;

namespace CindarsHope.Quests.Flags
{
    public class QuestFlagValidationIssue
    {
        public string FlagId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class QuestFlagValidator
    {
        // Flag IDs that look like they're trying to swallow MainProgression state
        private static readonly HashSet<string> MainProgressionKeywords = new HashSet<string>
        { "mainprogression", "main_progression", "mainstory_progress", "storyprogress_main" };

        // Flag IDs that look like FonteAnya state replacement
        private static readonly HashSet<string> FonteStateKeywords = new HashSet<string>
        { "fonte_state", "fonteanya_state", "fonte_progress_level", "fonteanya_unlocked_full" };

        public List<QuestFlagValidationIssue> Validate(QuestFlagDefinition def)
        {
            var issues = new List<QuestFlagValidationIssue>();
            if (def == null) { issues.Add(new QuestFlagValidationIssue { Code = "FLAG_NULL", Message = "Definition is null", IsBlocker = true }); return issues; }

            if (string.IsNullOrEmpty(def.FlagId))
                issues.Add(new QuestFlagValidationIssue { FlagId = def.FlagId, Code = "FLAG_NO_ID", Message = "FlagId is empty", IsBlocker = true });

            if (string.IsNullOrEmpty(def.OwnerSystem))
                issues.Add(new QuestFlagValidationIssue { FlagId = def.FlagId, Code = "FLAG_NO_OWNER", Message = $"Flag '{def.FlagId}' has no OwnerSystem", IsBlocker = false });

            // Guard: flag scope = FonteReferenceOnly means it's a trigger, not FonteAnya state owner
            if (def.Scope == QuestFlagScope.FonteReferenceOnly && def.OwnerSystem == "FonteAnyaSystem")
                issues.Add(new QuestFlagValidationIssue { FlagId = def.FlagId, Code = "FLAG_FONTE_SCOPE_OWNER_CONFLICT", Message = $"FonteReferenceOnly flag '{def.FlagId}' must not be owned by FonteAnyaSystem; source of truth lives in FonteAnyaSection", IsBlocker = true });

            // Guard: flag ID looks like MainProgression state replacement
            var idLower = (def.FlagId ?? "").ToLowerInvariant();
            foreach (var kw in MainProgressionKeywords)
                if (idLower.Contains(kw) && def.Scope != QuestFlagScope.MainProgressionReferenceOnly)
                    issues.Add(new QuestFlagValidationIssue { FlagId = def.FlagId, Code = "FLAG_MAIN_PROGRESSION_SWALLOW", Message = $"Flag '{def.FlagId}' looks like MainProgression state; use MainProgressionSection or MainProgressionReferenceOnly scope", IsBlocker = true });

            // Guard: flag ID looks like FonteAnya state replacement
            foreach (var kw in FonteStateKeywords)
                if (idLower.Contains(kw) && def.Scope != QuestFlagScope.FonteReferenceOnly)
                    issues.Add(new QuestFlagValidationIssue { FlagId = def.FlagId, Code = "FLAG_FONTE_STATE_SWALLOW", Message = $"Flag '{def.FlagId}' looks like FonteAnya state; use FonteAnyaSection or FonteReferenceOnly scope", IsBlocker = true });

            // Guard: deprecated flag must have replacement
            if (def.IsDeprecated && string.IsNullOrEmpty(def.ReplacementFlagId))
                issues.Add(new QuestFlagValidationIssue { FlagId = def.FlagId, Code = "FLAG_DEPRECATED_NO_REPLACEMENT", Message = $"Deprecated flag '{def.FlagId}' has no ReplacementFlagId", IsBlocker = false });

            // Guard: DebugOnly flags must not be persisted
            if (def.Visibility == QuestFlagVisibility.DebugOnly && def.Persists)
                issues.Add(new QuestFlagValidationIssue { FlagId = def.FlagId, Code = "FLAG_DEBUG_PERSISTS", Message = $"DebugOnly flag '{def.FlagId}' should not persist", IsBlocker = false });

            return issues;
        }

        public List<QuestFlagValidationIssue> ValidateRegistry(QuestFlagRegistry registry)
        {
            var issues = new List<QuestFlagValidationIssue>();
            foreach (var def in registry.GetAllVisible())
                issues.AddRange(Validate(def));
            return issues;
        }
    }
}
