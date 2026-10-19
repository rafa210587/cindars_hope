using System.Collections.Generic;

namespace CindarsHope.Dialogue
{
    public class ConditionalDialogueLine
    {
        public string LineId { get; set; }
        public string TextKey { get; set; }
        public DialogueCondition Condition { get; set; }
    }

    public class DialogueSetDefinition
    {
        public string DialogueSetId { get; set; }
        public string NpcId { get; set; }
        public List<string> DefaultLineTextKeys { get; set; } = new List<string>();
        public List<ConditionalDialogueLine> ContextualLines { get; set; } = new List<ConditionalDialogueLine>();
        public List<string> RumorPoolIds { get; set; } = new List<string>();
        public List<string> QuestDialogueRefs { get; set; } = new List<string>();
        public List<string> RelationshipDialogueRefs { get; set; } = new List<string>();
        public List<string> FarmVisitDialogueRefs { get; set; } = new List<string>();
        public List<string> LoreGateTags { get; set; } = new List<string>();
        public List<string> DebugTags { get; set; } = new List<string>();
    }
}
