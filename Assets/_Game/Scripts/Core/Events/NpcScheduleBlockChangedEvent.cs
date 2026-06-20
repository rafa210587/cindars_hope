namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_11 (CA-1) — published by <c>NpcScheduleService</c> whenever an NPC's resolved
    /// time block changes (Work / Social / Home / Night). Consumers (UI, availability,
    /// telemetry) react via the event bus instead of polling the schedule service.
    /// Block re-derives from the hour; it is never persisted (see city_rules.md "What Persists").
    /// </summary>
    public readonly struct NpcScheduleBlockChangedEvent
    {
        public readonly string NpcId;

        /// <summary>Resolved block name (e.g. "Work", "Social", "Home", "Night").</summary>
        public readonly string Block;

        /// <summary>True when the NPC is available for shop/dialogue interaction in this block.</summary>
        public readonly bool IsAvailable;

        public NpcScheduleBlockChangedEvent(string npcId, string block, bool isAvailable)
        {
            NpcId = npcId ?? string.Empty;
            Block = block ?? string.Empty;
            IsAvailable = isAvailable;
        }
    }
}
