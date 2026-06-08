using System.Collections.Generic;

namespace CindarsHope.Quests.Triggers
{
    public class QuestEventEnvelope
    {
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string SourceSystem { get; set; }
        public string TargetId { get; set; }
        public int Amount { get; set; } = 1;
        public string ActorId { get; set; }
        public int Day { get; set; }
        public int Time { get; set; }
        public string SceneId { get; set; }
        public int? RunSeed { get; set; }
        public List<string> PayloadIds { get; set; } = new List<string>();
        public string DeduplicationKey { get; set; }

        public string GetEffectiveDedupeKey() =>
            !string.IsNullOrEmpty(DeduplicationKey) ? DeduplicationKey : $"{EventName}:{TargetId}:{EventId}";
    }
}
