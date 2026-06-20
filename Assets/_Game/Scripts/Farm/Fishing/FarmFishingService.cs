using System.Collections.Generic;
using CindarsHope.World.Fishing;

namespace CindarsHope.Farm.Fishing
{
    /// <summary>
    /// fable_50 — serviço de pesca da fazenda. Contratos preservados (estação, rod tier, farm level,
    /// limite diário soft, lore protection). A novidade da v2: a captura NÃO é mais hardcoded —
    /// quando um <see cref="FishingTableModel"/> está registrado para o spot, a resolução passa pela
    /// ÚNICA fonte de verdade (<see cref="FishingCatchResolver"/>), igual à caverna. Sem tabela
    /// registrada, mantém o fallback determinístico legado (peixe comum), preservando os testes atuais.
    /// </summary>
    public class FarmFishingService
    {
        private readonly Dictionary<string, FarmFishingSpotDefinition> _spots;
        private readonly Dictionary<string, FishingTableModel> _tables;

        public FarmFishingService(Dictionary<string, FarmFishingSpotDefinition> spots,
            Dictionary<string, FishingTableModel> tables = null)
        {
            _spots = spots ?? new Dictionary<string, FarmFishingSpotDefinition>();
            _tables = tables ?? new Dictionary<string, FishingTableModel>();
        }

        public FishCatchResult AttemptCatch(string spotId, FishingSpotDailyState dailyState,
            int currentDay, string currentSeason = null, string rodTier = "Basic",
            int farmLevel = 0, int randomSeed = 0,
            string currentWeather = null, int currentHour = 12,
            FishingTimingGrade timingGrade = FishingTimingGrade.Good)
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

            // fable_50: Miss não consome a tentativa diária (não houve captura).
            if (timingGrade == FishingTimingGrade.Miss)
                return FishCatchResult.Fail("MissedTiming");

            // Resolução: tabela (fonte única) quando registrada; senão, fallback determinístico legado.
            var (itemId, rarity, quality, resolved) = ResolveCatch(spot, currentSeason, currentWeather,
                currentHour, timingGrade, randomSeed + currentDay, spotId);

            if (!resolved)
                return FishCatchResult.Fail("NoEligibleEntry");

            if (dailyState != null)
                dailyState.CatchesToday++;

            return new FishCatchResult
            {
                Success = true,
                FishItemId = itemId,
                Quantity = 1,
                Rarity = rarity,
                Quality = quality
            };
        }

        private (string itemId, FishRarity rarity, int quality, bool resolved) ResolveCatch(
            FarmFishingSpotDefinition spot, string season, string weather, int hour,
            FishingTimingGrade grade, int seed, string spotId)
        {
            var tableId = !string.IsNullOrEmpty(spot.CatchTableId) ? spot.CatchTableId : spotId;

            if (_tables.TryGetValue(tableId, out var table) && table != null && table.Entries.Count > 0)
            {
                // O dia já está embutido em 'seed' (randomSeed + currentDay) pelo chamador; o roll
                // determinístico usa seed estável + spotId + castIndex (FishingCatchResolver).
                var context = new FishingContext(seed, 0, spotId, 0, season, weather, hour);
                var outcome = FishingCatchResolver.Resolve(table, context, grade);
                if (!outcome.Success)
                    return (null, FishRarity.Common, 0, false);

                return (outcome.ItemId, MapRarity(outcome.Rarity), outcome.Quality, true);
            }

            // Fallback legado determinístico (sem tabela registrada): peixe comum. Mantém o contrato
            // dos testes atuais (AllowsEndgameFish=false ⇒ Common). Não há mais ramo "endgame" hardcoded.
            var fallbackItem = spot.AllowsEndgameFish
                ? "item_fish_rare"
                : (string.IsNullOrEmpty(spot.CatchTableId) ? "item_fish_common" : $"item_{spot.CatchTableId}_common");
            var fallbackRarity = spot.AllowsEndgameFish ? FishRarity.Rare : FishRarity.Common;
            var quality = grade == FishingTimingGrade.Perfect ? 1 : 0;
            return (fallbackItem, fallbackRarity, quality, true);
        }

        private static FishRarity MapRarity(int rarity)
        {
            switch (rarity)
            {
                case 0: return FishRarity.Common;
                case 1: return FishRarity.Uncommon;
                default: return FishRarity.Rare; // 2+ (Rare/Epic) mapeiam para Rare no enum atual
            }
        }
    }
}
