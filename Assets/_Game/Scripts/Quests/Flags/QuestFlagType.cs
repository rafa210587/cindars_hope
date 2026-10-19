namespace CindarsHope.Quests.Flags
{
    public enum QuestFlagType
    {
        Boolean = 0,
        Integer = 1,
        String = 2,
        Enum = 3,
        Counter = 4,
        Timestamp = 5
    }

    public enum QuestFlagScope
    {
        QuestLocal = 0,
        GlobalStory = 1,
        City = 2,
        Farm = 3,
        Cave = 4,
        FonteReferenceOnly = 5,
        MainProgressionReferenceOnly = 6,
        Shop = 7,
        Dialogue = 8,
        Festival = 9,
        Debug = 10
    }

    public enum QuestFlagVisibility
    {
        PublicKnown = 0,
        PlayerKnownAfterDiscovery = 1,
        HiddenInternal = 2,
        DebugOnly = 3,
        SpoilerLocked = 4
    }
}
