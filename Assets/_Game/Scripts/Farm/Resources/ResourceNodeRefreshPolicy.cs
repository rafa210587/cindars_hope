namespace CindarsHope.Farm.Resources
{
    public enum ResourceNodeRefreshPolicy
    {
        None = 0,
        NextDayChance = 1,
        FixedDays = 2,
        SeasonStart = 3,
        WeatherTriggered = 4,
        FarmLevelUnlock = 5,
        ZoneUnlock = 6,
        ManualReplant = 7,
        StoryUnlock = 8,
        EndgameOnly = 9
    }
}
