namespace CindarsHope.Core.Events
{
    public sealed class PlayerXpChangedEvent
    {
        public int XpGained { get; }
        public int TotalCurrentXp { get; }
        public int XpToNextLevel { get; }
        public int CurrentLevel { get; }

        public PlayerXpChangedEvent(int xpGained, int totalCurrentXp, int xpToNextLevel, int currentLevel)
        {
            XpGained = xpGained;
            TotalCurrentXp = totalCurrentXp;
            XpToNextLevel = xpToNextLevel;
            CurrentLevel = currentLevel;
        }
    }

    public sealed class PlayerLevelChangedEvent
    {
        public int OldLevel { get; }
        public int NewLevel { get; }
        public int AttributePointsGranted { get; }
        public int SkillPointsGranted { get; }

        public PlayerLevelChangedEvent(int oldLevel, int newLevel, int attributePointsGranted, int skillPointsGranted)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
            AttributePointsGranted = attributePointsGranted;
            SkillPointsGranted = skillPointsGranted;
        }
    }
}
