using System;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Save;

namespace CindarsHope.Craft
{
    public sealed class CraftingStation
    {
        private CraftingJob _job;

        public CraftingStation(string stationInstanceId, WorkshopType stationType)
        {
            StationInstanceId = stationInstanceId;
            StationType = stationType;
            StationLevel = 1;
        }

        public string StationInstanceId { get; }
        public WorkshopType StationType { get; }
        public int StationLevel { get; set; }
        public CraftingJob Job => _job;
        public bool HasActiveJob => _job != null && _job.Status == CraftingJobStatus.InProgress;
        public bool HasCompletedOutput => _job != null && _job.Status == CraftingJobStatus.Completed;
        public bool IsBusy => HasActiveJob || HasCompletedOutput;

        public bool CanStartCraft(RecipeDataSO recipe, out string failureReason)
        {
            if (recipe == null)
            {
                failureReason = "Recipe is unavailable.";
                return false;
            }

            if (recipe.RequiredStationType != StationType)
            {
                failureReason = $"Requires {recipe.RequiredStationType}.";
                return false;
            }

            if (recipe.RequiredWorkshopLevel > StationLevel)
            {
                failureReason = $"Requires station level {recipe.RequiredWorkshopLevel}.";
                return false;
            }

            if (!recipe.IsUnlockedByDefault)
            {
                failureReason = "Recipe is locked.";
                return false;
            }

            if (IsBusy)
            {
                failureReason = "Station already has a pending job or output.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        public bool TryStartCraft(RecipeDataSO recipe, InventoryManager inventory, out string failureReason, StaminaManager staminaManager = null, float craftTimeMultiplier = 1f)
        {
            if (!CanStartCraft(recipe, out failureReason) || !ValidateIngredients(recipe, inventory, out failureReason))
            {
                PublishFailure(recipe, failureReason);
                return false;
            }

            if (staminaManager != null && recipe.StaminaCost > 0 && !staminaManager.TrySpendStamina(recipe.StaminaCost))
            {
                failureReason = "Not enough stamina.";
                PublishFailure(recipe, failureReason);
                return false;
            }

            var inventoryBeforeCraft = inventory.CaptureSaveData();
            if (!ConsumeIngredients(recipe, inventory))
            {
                inventory.RestoreFromSaveData(inventoryBeforeCraft);
                failureReason = "Ingredients could not be consumed safely.";
                PublishFailure(recipe, failureReason);
                return false;
            }

            if (recipe.IsInstantaneous)
            {
                if (!inventory.AddItem(recipe.OutputItemId, recipe.OutputAmount))
                {
                    inventory.RestoreFromSaveData(inventoryBeforeCraft);
                    failureReason = "Inventory is full for crafted output.";
                    PublishFailure(recipe, failureReason);
                    return false;
                }

                GameEventBus.Publish(new ItemCraftedEvent(recipe.Id, recipe.OutputItemId, recipe.OutputAmount));
                GameEventBus.Publish(new CraftingOutputCollectedEvent(StationInstanceId, recipe.Id, recipe.OutputItemId, recipe.OutputAmount));
                failureReason = string.Empty;
                return true;
            }

            // fable_47 (follow-up 2): aplica o multiplicador de tempo de craft no ponto único.
            _job = new CraftingJob(StationInstanceId, recipe, craftTimeMultiplier);
            GameEventBus.Publish(new CraftingJobStartedEvent(StationInstanceId, recipe.Id, _job.JobId));
            failureReason = string.Empty;
            return true;
        }

        public bool TryCollectOutput(InventoryManager inventory, out string failureReason)
        {
            if (!HasCompletedOutput)
            {
                failureReason = "No completed output to collect.";
                return false;
            }

            if (!inventory.AddItem(_job.OutputItemId, _job.OutputAmount))
            {
                failureReason = "Inventory is full. Output remains at the station.";
                GameEventBus.Publish(new CraftingFailedEvent(StationInstanceId, _job.RecipeId, failureReason));
                return false;
            }

            GameEventBus.Publish(new ItemCraftedEvent(_job.RecipeId, _job.OutputItemId, _job.OutputAmount));
            GameEventBus.Publish(new CraftingOutputCollectedEvent(StationInstanceId, _job.RecipeId, _job.OutputItemId, _job.OutputAmount));
            _job = null;
            failureReason = string.Empty;
            return true;
        }

        public bool TryCancelJob(InventoryManager inventory, out string failureReason)
        {
            if (!HasActiveJob)
            {
                failureReason = "Only an in-progress job can be cancelled.";
                return false;
            }

            var inventoryBeforeCancel = inventory.CaptureSaveData();
            foreach (var ingredient in _job.IngredientsConsumed)
            {
                if (!inventory.AddItem(ingredient.ItemId, ingredient.Amount))
                {
                    inventory.RestoreFromSaveData(inventoryBeforeCancel);
                    failureReason = "Inventory has no room to return all ingredients.";
                    GameEventBus.Publish(new CraftingFailedEvent(StationInstanceId, _job.RecipeId, failureReason));
                    return false;
                }
            }

            var cancelledJobId = _job.JobId;
            var recipeId = _job.RecipeId;
            _job.Cancel();
            _job = null;
            GameEventBus.Publish(new CraftingJobCancelledEvent(StationInstanceId, recipeId, cancelledJobId));
            failureReason = string.Empty;
            return true;
        }

        public void Update(float deltaTime)
        {
            if (!HasActiveJob)
            {
                return;
            }

            _job.Update(deltaTime);
            if (_job.IsComplete)
            {
                _job.Complete();
                GameEventBus.Publish(new CraftingJobCompletedEvent(StationInstanceId, _job.RecipeId, _job.JobId));
            }
        }

        public void LoadFromSaveData(CraftingStationSaveData saveData, RecipeDatabaseSO recipeDatabase)
        {
            if (saveData == null)
            {
                return;
            }

            StationLevel = saveData.StationLevel > 0 ? saveData.StationLevel : 1;
            if (saveData.Job == null || string.IsNullOrWhiteSpace(saveData.Job.RecipeId))
            {
                return;
            }

            recipeDatabase.TryGetById(saveData.Job.RecipeId, out var recipe);
            _job = new CraftingJob(saveData.Job, recipe);
            if (recipe == null)
            {
                UnityEngine.Debug.LogError($"Crafting station '{StationInstanceId}' loaded missing recipe '{saveData.Job.RecipeId}'. Saved output and ingredients are retained.");
            }
        }

        public CraftingStationSaveData CaptureSaveData()
        {
            return new CraftingStationSaveData
            {
                StationInstanceId = StationInstanceId,
                StationType = (int)StationType,
                StationLevel = StationLevel,
                Job = _job?.CaptureSaveData()
            };
        }

        private bool ValidateIngredients(RecipeDataSO recipe, InventoryManager inventory, out string failureReason)
        {
            if (inventory == null)
            {
                failureReason = "Inventory system is unavailable.";
                return false;
            }

            if (recipe.Ingredients == null || recipe.Ingredients.Length == 0)
            {
                failureReason = "Recipe has no ingredients.";
                return false;
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.ItemId) || ingredient.Amount <= 0 || !inventory.HasItem(ingredient.ItemId, ingredient.Amount))
                {
                    failureReason = $"Missing ingredient {ingredient.ItemId}.";
                    return false;
                }
            }

            failureReason = string.Empty;
            return true;
        }

        private static bool ConsumeIngredients(RecipeDataSO recipe, InventoryManager inventory)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!inventory.RemoveItem(ingredient.ItemId, ingredient.Amount))
                {
                    return false;
                }
            }

            return true;
        }

        private void PublishFailure(RecipeDataSO recipe, string failureReason)
        {
            GameEventBus.Publish(new CraftingFailedEvent(StationInstanceId, recipe != null ? recipe.Id : string.Empty, failureReason));
        }
    }

    [Serializable]
    public class CraftingStationSaveData
    {
        public string StationInstanceId;
        public int StationType;
        public int StationLevel;
        public CraftingJobSaveData Job;
    }
}
