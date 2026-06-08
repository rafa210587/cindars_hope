using System.Collections.Generic;

namespace CindarsHope.Farm.Fishing
{
    public class FarmFishingService
    {
        private readonly Dictionary<string, FarmFishingSpotDefinition> _spots;

        public FarmFishingService(Dictionary<string, FarmFishingSpotDefinition> spots)
        {
            _spots = spots ?? new Dictionary<string, FarmFishingSpotDefinition>();
        }

        public FishCatchResult AttemptCatch(string spotId, FishingSpotDailyState dailyState,
            int currentDay, string currentSeason = null, string rodTier = "Basic",
            int farmLevel = 0, int randomSeed = 0)
        {
            if (!_spots.TryGetValue(spotId, out var spot))
                return FishCatchResult.Fail("SpotNotFound");

            if (spot.IsLoreProtected)
                return FishCatchResult.Fail("LoreProtected");

            if (spot.RequiredFarmLevel > farmLevel)
                return FishCatchResult.Fail("FarmLevelInsufficient");

            if (spot.AllowedSeasons.Count > 0 && !string.IsNullOrEmpty(currentSeason))
            {
                if (!spot.AllowedSeasons.Contains(currentSeason))
                    return FishCatchResult.Fail("WrongSeason");
            }

            if (!string.IsNullOrEmpty(spot.RequiredRodTier) && rodTier != spot.RequiredRodTier)
                return FishCatchResult.Fail("RodTierInsufficient");

            // Ensure daily state is for correct day
            if (dailyState != null && dailyState.Day != currentDay)
            {
                dailyState.Day = currentDay;
                dailyState.CatchesToday = 0;
            }

            if (dailyState != null && dailyState.SoftLimitReached(spot))
                return FishCatchResult.Fail("DailySoftLimitReached");

            // Resolve catch (deterministic by seed)
            var (itemId, rarity) = ResolveCatch(spot.CatchTableId, spot.AllowsEndgameFish, randomSeed + currentDay);

            if (dailyState != null)
                dailyState.CatchesToday++;

            return new FishCatchResult
            {
                Success = true,
                FishItemId = itemId,
                Quantity = 1,
                Rarity = rarity
            };
        }

        private (string, FishRarity) ResolveCatch(string catchTableId, bool allowsEndgame, int seed)
        {
            // Never produce endgame/lunar fish unless explicitly enabled
            if (!allowsEndgame)
                return (string.IsNullOrEmpty(catchTableId) ? "item_fish_common" : $"item_{catchTableId}_common", FishRarity.Common);

            return ("item_fish_rare", FishRarity.Rare);
        }
    }
}
