using System;

namespace CindarsHope.Companions
{
    [Flags]
    public enum CompanionRole
    {
        None = 0,
        FarmCompanion = 1,
        CaveCompanion = 2,
        QuestCompanion = 4,
        SocialCompanion = 8
    }

    public enum UnlockState
    {
        Locked,
        Eligible,
        Temporary,
        Unlocked,
        Scheduled,
        StoryOnly,
        Unavailable
    }

    public enum InjuryState
    {
        Healthy,
        Injured,
        Incapacitated,
        Recovering
    }
}
