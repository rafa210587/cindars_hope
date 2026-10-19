namespace CindarsHope.Core.Events
{
    public readonly struct PlayerXpChangedEvent
    {
        public int Delta { get; }
        public int CurrentXp { get; }
        public int XpToNextLevel { get; }
        public int Level { get; }

        public PlayerXpChangedEvent(int delta, int currentXp, int xpToNextLevel, int level)
        {
            Delta = delta;
            CurrentXp = currentXp;
            XpToNextLevel = xpToNextLevel;
            Level = level;
        }
    }
}
