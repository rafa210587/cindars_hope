using System;
using CindarsHope.Core.Data;
using CindarsHope.Craft.Data;
using CindarsHope.Core.Data;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Craft
{
    public class CraftingStation
    {
        public string StationInstanceId { get; private set; }
        public WorkshopType StationType { get; private set; }
        public int StationLevel { get; set; }

        private CraftingJob _activeJob;
        private string _pendingOutputItemId;
        private int _pendingOutputAmount;
        private ItemDatabaseSO _itemDatabase;
        private RecipeDatabaseSO _recipeDatabase;

        public CraftingStation(string stationInstanceId, WorkshopType stationType, ItemDatabaseSO itemDatabase, RecipeDatabaseSO recipeDatabase)
        {
            StationInstanceId = stationInstanceId;
            StationType = stationType;
            StationLevel = 1;
            _itemDatabase = itemDatabase;
            _recipeDatabase = recipeDatabase;
        }

        public CraftingJob GetActiveJob() => _activeJob;
        public bool HasOutput => !string.IsNullOrEmpty(_pendingOutputItemId);
        public bool HasActiveJob => _activeJob != null && _activeJob.Status == CraftingJobStatus.InProgress;

        public bool CanStartCraft(RecipeDataSO recipe, out string failureReason)
        {
            failureReason = string.Empty;

            if (recipe == null)
            {
                failureReason = "Recipe is null";
                return false;
            }

            if (recipe.WorkshopType != StationType)
            {
                failureReason = $"Recipe requires {recipe.WorkshopType}, station is {StationType}";
                return false;
            }

            if (recipe.RequiredWorkshopLevel > StationLevel)
            {
                failureReason = $"Recipe requires level {recipe.RequiredWorkshopLevel}, station is level {StationLevel}";
                return false;
            }

            if (!recipe.IsUnlocked)
            {
                failureReason = "Recipe is not unlocked";
                return false;
            }

            if (HasActiveJob || HasOutput)
            {
                failureReason = "Station is busy";
                return false;
            }

            return true;
        }

        public bool TryStartCraft(RecipeDataSO recipe, InventoryManager inventory, out string failureReason)
        {
            failureReason = string.Empty;

            if (!CanStartCraft(recipe, out failureReason))
                return false;

            if (!ValidateIngredients(recipe, inventory, out failureReason))
                return false;

            // Consume ingredients
            ConsumeIngredients(recipe, inventory);

            if (recipe.IsInstantaneous)
            {
                // Instantaneous craft - set pending output
                _pendingOutputItemId = recipe.OutputItemId;
                _pendingOutputAmount = recipe.OutputAmount;
            }
            else
            {
                // Start timed job
                _activeJob = new CraftingJob(StationInstanceId, recipe);
            }

            return true;
        }

        public bool TryCollectOutput(InventoryManager inventory, out string failureReason)
        {
            failureReason = string.Empty;

            if (!HasOutput)
            {
                failureReason = "No output to collect";
                return false;
            }

            if (!inventory.AddItem(_pendingOutputItemId, _pendingOutputAmount))
            {
                failureReason = "Inventory is full";
                return false;
            }

            _pendingOutputItemId = string.Empty;
            _pendingOutputAmount = 0;
            return true;
        }

        public bool TryCancelJob(InventoryManager inventory, out string failureReason)
        {
            failureReason = string.Empty;

            if (!HasActiveJob)
            {
                failureReason = "No active job to cancel";
                return false;
            }

            var recipe = _activeJob.Recipe;

            // Check if we have space for ingredients
            int totalIngredients = 0;
            if (recipe.Ingredients != null)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    if (!string.IsNullOrWhiteSpace(ingredient.ItemId))
                        totalIngredients += ingredient.Amount;
                }
            }

            if (inventory.Capacity - inventory.Items.Count < totalIngredients)
            {
                failureReason = "Not enough inventory space to return ingredients";
                return false;
            }

            // Return ingredients
            ReturnIngredients(recipe, inventory);
            _activeJob.Cancel();
            _activeJob = null;

            return true;
        }

        public void Update(float deltaTime)
        {
            if (_activeJob != null)
            {
                _activeJob.Update(deltaTime);

                if (_activeJob.IsComplete)
                {
                    _activeJob.Complete();
                    _pendingOutputItemId = _activeJob.Recipe.OutputItemId;
                    _pendingOutputAmount = _activeJob.Recipe.OutputAmount;
                }
            }
        }

        public void LoadFromSaveData(CraftingStationSaveData saveData, RecipeDatabaseSO recipeDatabase)
        {
            if (saveData == null)
                return;

            StationLevel = saveData.StationLevel;

            if (saveData.ActiveJob != null && !string.IsNullOrEmpty(saveData.ActiveJob.RecipeId))
            {
                if (recipeDatabase.TryGetById(saveData.ActiveJob.RecipeId, out var recipe))
                {
                    _activeJob = new CraftingJob(saveData.ActiveJob.StationInstanceId, recipe);
                    _activeJob.RemainingSeconds = saveData.ActiveJob.RemainingSeconds;
                    _activeJob.Status = (CraftingJobStatus)saveData.ActiveJob.Status;
                }
            }

            _pendingOutputItemId = saveData.PendingOutputItemId;
            _pendingOutputAmount = saveData.PendingOutputAmount;
        }

        public CraftingStationSaveData CaptureSaveData()
        {
            var data = new CraftingStationSaveData
            {
                StationInstanceId = StationInstanceId,
                StationType = (int)StationType,
                StationLevel = StationLevel,
                PendingOutputItemId = _pendingOutputItemId,
                PendingOutputAmount = _pendingOutputAmount
            };

            if (_activeJob != null && _activeJob.Status == CraftingJobStatus.InProgress)
            {
                data.ActiveJob = new CraftingJobSaveData
                {
                    JobId = _activeJob.JobId,
                    StationInstanceId = _activeJob.StationInstanceId,
                    RecipeId = _activeJob.Recipe.Id,
                    Status = (int)_activeJob.Status,
                    RemainingSeconds = _activeJob.RemainingSeconds
                };
            }

            return data;
        }

        private bool ValidateIngredients(RecipeDataSO recipe, InventoryManager inventory, out string failureReason)
        {
            failureReason = string.Empty;

            if (recipe.Ingredients == null || recipe.Ingredients.Length == 0)
                return true;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.ItemId))
                    continue;

                if (!inventory.HasItem(ingredient.ItemId, ingredient.Amount))
                {
                    failureReason = $"Missing {ingredient.ItemId} x{ingredient.Amount}";
                    return false;
                }
            }

            return true;
        }

        private void ConsumeIngredients(RecipeDataSO recipe, InventoryManager inventory)
        {
            if (recipe.Ingredients == null)
                return;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (!string.IsNullOrWhiteSpace(ingredient.ItemId))
                {
                    inventory.RemoveItem(ingredient.ItemId, ingredient.Amount);
                }
            }
        }

        private void ReturnIngredients(RecipeDataSO recipe, InventoryManager inventory)
        {
            if (recipe.Ingredients == null)
                return;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (!string.IsNullOrWhiteSpace(ingredient.ItemId))
                {
                    inventory.AddItem(ingredient.ItemId, ingredient.Amount);
                }
            }
        }
    }

    [System.Serializable]
    public class CraftingStationSaveData
    {
        public string StationInstanceId;
        public int StationType;
        public int StationLevel;
        public string PendingOutputItemId;
        public int PendingOutputAmount;
        public CraftingJobSaveData ActiveJob;
    }
}
