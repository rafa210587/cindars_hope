using System.Collections.Generic;

namespace CindarsHope.Quests.Flags
{
    public class QuestFlagDefinition
    {
        public string FlagId { get; set; }
        public string DisplayNameKey { get; set; }
        public string DescriptionKey { get; set; }
        public QuestFlagType FlagType { get; set; } = QuestFlagType.Boolean;
        public QuestFlagScope Scope { get; set; }
        public QuestFlagVisibility Visibility { get; set; }
        public int SpoilerTier { get; set; } = 0;
        public string OwnerSystem { get; set; }
        public List<string> AllowedSetters { get; set; } = new List<string>();
        public List<string> AllowedClearers { get; set; } = new List<string>();
        public bool Persists { get; set; } = true;
        public string DefaultValue { get; set; } = "false";
        public bool CanAppearInQuestLog { get; set; } = false;
        public bool CanBeUsedByConditions { get; set; } = true;
        public bool CanBeGrantedByReward { get; set; } = true;
        public bool IsDeprecated { get; set; } = false;
        public string ReplacementFlagId { get; set; }
        public List<string> DebugTags { get; set; } = new List<string>();

        public bool IsHidden() => Visibility == QuestFlagVisibility.HiddenInternal ||
                                   Visibility == QuestFlagVisibility.DebugOnly ||
                                   Visibility == QuestFlagVisibility.SpoilerLocked;

        // Fonte/Main flags are reference-only and cannot own their own source state
        public bool IsReferenceOnly() => Scope == QuestFlagScope.FonteReferenceOnly ||
                                          Scope == QuestFlagScope.MainProgressionReferenceOnly;
    }
}
