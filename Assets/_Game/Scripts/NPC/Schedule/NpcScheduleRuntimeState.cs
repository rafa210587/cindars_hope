namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// Transient in-memory record of an NPC's current schedule resolution state.
    /// Not persisted — reconstructed fresh on each DayStartedEvent.
    /// </summary>
    [System.Serializable]
    public class NpcScheduleRuntimeState
    {
        public string NpcId;
        public string CurrentScheduleId;
        public string CurrentScheduleBlock;
        public string CurrentAnchorId;
        public bool IsAvailable;
    }
}
