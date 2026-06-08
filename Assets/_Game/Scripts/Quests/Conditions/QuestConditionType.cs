namespace CindarsHope.Quests.Conditions
{
    public enum QuestConditionType
    {
        QuestCondition = 0,
        PlayerCondition = 1,
        InventoryCondition = 2,
        WorldCondition = 3,
        TimeCondition = 4,
        WeatherCondition = 5,
        LunarCondition = 6,
        NpcCondition = 7,
        DialogueCondition = 8,
        FarmCondition = 9,
        CaveCondition = 10,
        CombatCondition = 11,
        FonteCondition = 12,
        BestiaryCondition = 13,
        // Future conditions — enum values only, no runtime implementation
        SocialConditionFuture = 90,
        PetConditionFuture = 91,
        CompanionConditionFuture = 92
    }

    public enum QuestConditionOperator
    {
        Equals = 0,
        GreaterThanOrEqual = 1,
        LessThanOrEqual = 2,
        Contains = 3,
        NotContains = 4,
        IsSet = 5,
        IsNotSet = 6
    }

    public enum QuestTriggerDeduplicationPolicy
    {
        ByEventId = 0,
        ByDayTarget = 1,
        ByRunSeedTarget = 2,
        ByObjectiveCompletion = 3,
        ManualAllowRepeat = 4
    }

    public enum QuestRetroactivePolicy
    {
        NotAllowed = 0,
        AllowedIfStateTracked = 1,
        AllowedAlways = 2
    }
}
