namespace CindarsHope.NPC.Schedule
{
    // Simplified time blocks for WAVE25 (intra-day resolution deferred: TIME_BLOCK_DEBT)
    public enum NpcTimeBlock
    {
        Default = 0,
        Morning = 1,
        Midday = 2,
        Evening = 3,
        Night = 4
    }

    [System.Serializable]
    public class NpcScheduleBlock
    {
        public NpcTimeBlock TimeBlock;
        public string AnchorId;
        public string SceneId;
        public string ActivityLabel;
        public bool CanInteract = true;
    }
}
