using System.Collections.Generic;

namespace CindarsHope.Quests.Save
{
    // Quest save DTO — no Unity refs, only simple types per save-dto-simple-types-only rule
    public class QuestObjectiveStateRecord
    {
        public string ObjectiveId { get; set; }
        public int CurrentProgress { get; set; }
        public int RequiredProgress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsFailed { get; set; }
        public bool IsKnown { get; set; } = true;
    }

    public class QuestChoiceRecord
    {
        public string StepId { get; set; }
        public string ChoiceId { get; set; }
        public int ChosenAtDay { get; set; }
    }

    // Serializable-by-mapper snapshot of a pending dynamic reward. Enum values are stored as ints
    // so this save-layer record remains independent from runtime reward implementations.
    public class QuestDynamicRewardRecord
    {
        public string RewardId { get; set; }
        public int RewardType { get; set; }
        public string TargetId { get; set; }
        public int Quantity { get; set; }
        public string GrantedFlagId { get; set; }
        public int IdempotencyPolicy { get; set; }
    }

    public class QuestStateRecord
    {
        public string QuestId { get; set; }
        public int State { get; set; } = 0; // serialized QuestStateStatus int
        public string CurrentStepId { get; set; }
        public List<string> CompletedStepIds { get; set; } = new List<string>();
        public List<string> FailedStepIds { get; set; } = new List<string>();
        public List<QuestObjectiveStateRecord> ObjectiveStates { get; set; } = new List<QuestObjectiveStateRecord>();
        public List<string> KnownObjectiveIds { get; set; } = new List<string>();
        public List<string> KnownHints { get; set; } = new List<string>();
        public int StartedAtDay { get; set; }
        public int StartedAtTime { get; set; }
        public int? CompletedAtDay { get; set; }
        public int? ExpiresAtDay { get; set; }
        public bool Tracked { get; set; } = false;
        public bool Discovered { get; set; } = false;
        public string FailureReason { get; set; }
        public List<QuestChoiceRecord> ChoiceHistory { get; set; } = new List<QuestChoiceRecord>();
        public List<string> GrantedRewardIds { get; set; } = new List<string>();
        public List<string> GrantedFlagIds { get; set; } = new List<string>();
        public string RepeatInstanceId { get; set; }

        // ─── fable_34 — quest source channel + dynamic instance params (additive, simple types) ───
        // Source channel (Board/Npc/Mural/CaveSecret/Main/CaveContract) serialized as int.
        // Default 1 == QuestSource.Npc so legacy saves load with a safe non-dynamic source.
        public int Source { get; set; } = (int)QuestSource.Npc;

        // True when this record was produced by a board/procedural template (QuestInstance).
        public bool IsDynamicInstance { get; set; } = false;
        // Template id (e.g. bd_cull_slime) — empty for authored quests.
        public string TemplateId { get; set; }
        // Instance objective parameters (also captured so a board contract restores standalone).
        public string InstanceTargetId { get; set; }
        public int InstanceQuantity { get; set; }
        public int QuestLevel { get; set; }
        public int InstanceRewardGold { get; set; }
        public int InstanceRewardXp { get; set; }
        public int GeneratedForDay { get; set; }
        public List<QuestDynamicRewardRecord> DynamicRewards { get; set; } = new List<QuestDynamicRewardRecord>();
    }
}
