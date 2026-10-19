using System.Collections.Generic;

namespace CindarsHope.Farm.Fishing
{
    public enum FishRarity { Common = 0, Uncommon = 1, Rare = 2, LunarFuture = 10 }

    public class FarmFishingSpotDefinition
    {
        public string FishingSpotId { get; set; }
        public string DisplayName { get; set; }
        public string ZoneId { get; set; }
        public string WaterBodyId { get; set; }
        public List<string> AllowedSeasons { get; set; } = new List<string>();
        public List<string> AllowedWeather { get; set; } = new List<string>();
        public string CatchTableId { get; set; }
        public string RequiredRodTier { get; set; }
        public int RequiredFarmLevel { get; set; } = 0;
        public int DailyCatchSoftLimit { get; set; } = 0; // 0 = no limit
        public bool IsLoreProtected { get; set; } = false;
        public bool AllowsEndgameFish { get; set; } = false; // must be false for early farm lake
    }

    public class FishingSpotDailyState
    {
        public string FishingSpotId { get; set; }
        public int Day { get; set; }
        public int CatchesToday { get; set; } = 0;
        public bool SoftLimitReached(FarmFishingSpotDefinition def) =>
            def.DailyCatchSoftLimit > 0 && CatchesToday >= def.DailyCatchSoftLimit;
    }
}
