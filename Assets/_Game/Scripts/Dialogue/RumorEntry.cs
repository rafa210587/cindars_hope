using System.Collections.Generic;

namespace CindarsHope.Dialogue
{
    public enum RumorTruthLevel { True = 0, HalfTrue, False, Misleading, Unknown }

    public class RumorEntry
    {
        public string RumorId { get; set; }
        public string TextKey { get; set; }
        public List<string> NpcIdsAllowed { get; set; } = new List<string>();
        public List<string> LocationTags { get; set; } = new List<string>();
        public DialogueCondition RequiredCondition { get; set; }
        public SpoilerLevel SpoilerLevel { get; set; } = SpoilerLevel.None;
        public RumorTruthLevel TruthLevel { get; set; } = RumorTruthLevel.Unknown;
        public List<string> TopicTags { get; set; } = new List<string>();
        public bool OnceOnly { get; set; } = false;
        public int CooldownDays { get; set; } = 0;
        // If true, this rumor can advance knowledge/lore (via adapter only)
        public bool CanAdvanceKnowledge { get; set; } = false;
        // If true, this rumor can start a quest (via quest adapter only)
        public bool CanStartQuest { get; set; } = false;
    }

    public class RumorPool
    {
        public string PoolId { get; set; }
        public List<RumorEntry> Entries { get; set; } = new List<RumorEntry>();
        public string DefaultLocationTag { get; set; }

        public List<RumorEntry> GetAvailable(DialogueContext ctx, HashSet<string> seenOnceOnly)
        {
            var result = new List<RumorEntry>();
            foreach (var rumor in Entries)
            {
                if (rumor.OnceOnly && seenOnceOnly.Contains(rumor.RumorId)) continue;
                if (rumor.NpcIdsAllowed.Count > 0 && !rumor.NpcIdsAllowed.Contains(ctx.NpcId)) continue;
                if (rumor.RequiredCondition != null && !rumor.RequiredCondition.IsMet(ctx)) continue;
                result.Add(rumor);
            }
            return result;
        }
    }
}
