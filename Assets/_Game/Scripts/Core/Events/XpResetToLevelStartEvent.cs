namespace CindarsHope.Core.Events
{
    public struct XpResetToLevelStartEvent
    {
        public int Level { get; set; }
        public int XpLost { get; set; }
    }
}
