namespace CindarsHope.Core.Events
{
    public readonly struct PlayerLevelChangedEvent
    {
        public int OldLevel { get; }
        public int NewLevel { get; }
        public int GrantedAttributePoints { get; }
        public int GrantedSkillPoints { get; }

        public PlayerLevelChangedEvent(int oldLevel, int newLevel, int grantedAttributePoints, int grantedSkillPoints)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
            GrantedAttributePoints = grantedAttributePoints;
            GrantedSkillPoints = grantedSkillPoints;
        }
    }
}
