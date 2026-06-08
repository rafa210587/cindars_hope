using System;
using System.Collections.Generic;

namespace CindarsHope.Crafting
{
    public class CraftingService
    {
        private readonly Dictionary<string, ProcessingJob> _activeJobs = new Dictionary<string, ProcessingJob>();

        public CraftingResult ValidateAndStart(
            RecipeDefinition recipe,
            CraftingStationDefinition station,
            CraftingRequest request)
        {
            if (recipe == null) return CraftingResult.Fail("recipe is null");
            if (station == null) return CraftingResult.Fail("station is null");
            if (request == null) return CraftingResult.Fail("request is null");

            // Station type check
            if (!station.AcceptsRecipe(recipe.RecipeType))
                return CraftingResult.Fail($"Station {station.StationId} does not accept {recipe.RecipeType}");

            // Station level check
            if (station.StationLevel < recipe.RequiredStationLevel)
                return CraftingResult.Fail($"Station level {station.StationLevel} < required {recipe.RequiredStationLevel}");

            // Recipe unlock check
            if (request.KnownRecipes != null && !request.KnownRecipes.CanCraft(recipe.RecipeId))
                return CraftingResult.Fail($"Recipe {recipe.RecipeId} not known or unlocked");

            // Gating checks
            if (request.PlayerReputation < recipe.RequiredReputation)
                return CraftingResult.Fail($"Reputation {request.PlayerReputation} < required {recipe.RequiredReputation}");
            if (request.PlayerFarmLevel < recipe.RequiredFarmLevel)
                return CraftingResult.Fail($"FarmLevel {request.PlayerFarmLevel} < required {recipe.RequiredFarmLevel}");
            if (request.PlayerCaveProgress < recipe.RequiredCaveProgress)
                return CraftingResult.Fail($"CaveProgress {request.PlayerCaveProgress} < required {recipe.RequiredCaveProgress}");
            if (!string.IsNullOrEmpty(recipe.RequiredQuestFlag) && request.ActiveQuestFlag != recipe.RequiredQuestFlag)
                return CraftingResult.Fail($"Required quest flag {recipe.RequiredQuestFlag} not active");
            if (request.PlayerGold < recipe.RequiredGold)
                return CraftingResult.Fail($"Gold {request.PlayerGold} < required {recipe.RequiredGold}");

            // Protected ingredients check
            foreach (var ing in recipe.RequiredIngredients)
            {
                if (ing.IsProtected)
                    return CraftingResult.Fail($"Protected ingredient {ing.ItemId} requires explicit authoring gate");
            }

            var consumed = new List<string>();
            foreach (var ing in recipe.RequiredIngredients)
                consumed.Add(ing.ItemId);

            // Processing (async) vs instant craft
            if (recipe.IsProcessing)
            {
                var jobId = Guid.NewGuid().ToString();
                var job = new ProcessingJob
                {
                    ProcessingJobId = jobId,
                    RecipeId = recipe.RecipeId,
                    StationId = station.StationId,
                    OutputItemId = recipe.OutputItemId,
                    OutputQuantity = recipe.OutputQuantity,
                    StartDayTime = request.RequestedDayTime,
                    FinishDayTime = request.RequestedDayTime + recipe.RequiredTimeTicks,
                    State = ProcessingJobState.Processing
                };
                _activeJobs[jobId] = job;
                return new CraftingResult { Success = true, InputsConsumed = consumed, ProcessingJobId = jobId };
            }

            return new CraftingResult
            {
                Success = true,
                InputsConsumed = consumed,
                OutputsCreated = new List<string> { recipe.OutputItemId },
                Quality = 0 // quality propagation via QualityInfluenceRules handled by caller
            };
        }

        public CraftingResult CollectJob(string jobId, int currentDayTime)
        {
            if (!_activeJobs.TryGetValue(jobId, out var job))
                return CraftingResult.Fail("Job not found");
            job.TryAdvance(currentDayTime);
            var result = job.Collect();
            if (result.Success) _activeJobs.Remove(jobId);
            return result;
        }

        public bool HasActiveJob(string jobId) => _activeJobs.ContainsKey(jobId);
    }
}
