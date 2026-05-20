namespace CindarsHope.Core.Events
{
    public readonly struct CaveCheckpointUnlockedEvent
    {
        public readonly int CaveLevel;

        public CaveCheckpointUnlockedEvent(int caveLevel)
        {
            CaveLevel = caveLevel;
        }
    }
}
