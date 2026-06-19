using System;
using System.Collections.Generic;
using CindarsHope.Craft.Data;

namespace CindarsHope.Craft
{
    public enum CraftingJobStatus
    {
        InProgress,
        Completed,
        Cancelled
    }

    public class CraftingJob
    {
        public string JobId { get; private set; }
        public string StationInstanceId { get; private set; }
        public RecipeDataSO Recipe { get; private set; }
        public string RecipeId { get; private set; }
        public string OutputItemId { get; private set; }
        public int OutputAmount { get; private set; }
        public List<CraftingIngredientSaveData> IngredientsConsumed { get; private set; }
        public CraftingJobStatus Status { get; set; }
        public float RemainingSeconds { get; set; }

        public CraftingJob(string stationInstanceId, RecipeDataSO recipe)
            : this(stationInstanceId, recipe, 1f)
        {
        }

        /// <summary>
        /// fable_47 (follow-up 2): craftTimeMultiplier aplica CraftTimeReduction derivada (F18)
        /// no ÚNICO ponto de criação do job. 1f = sem alteração (caminho padrão/save-load).
        /// </summary>
        public CraftingJob(string stationInstanceId, RecipeDataSO recipe, float craftTimeMultiplier)
        {
            JobId = Guid.NewGuid().ToString();
            StationInstanceId = stationInstanceId;
            Recipe = recipe;
            RecipeId = recipe.Id;
            OutputItemId = recipe.OutputItemId;
            OutputAmount = recipe.OutputAmount;
            IngredientsConsumed = CaptureIngredients(recipe.Ingredients);
            Status = CraftingJobStatus.InProgress;
            RemainingSeconds = UnityEngine.Mathf.Max(0f, recipe.CraftTimeSeconds * craftTimeMultiplier);
        }

        public CraftingJob(CraftingJobSaveData saveData, RecipeDataSO recipe)
        {
            JobId = saveData.JobId;
            StationInstanceId = saveData.StationInstanceId;
            Recipe = recipe;
            RecipeId = saveData.RecipeId;
            OutputItemId = saveData.OutputItemId;
            OutputAmount = saveData.OutputAmount;
            IngredientsConsumed = saveData.IngredientsConsumed ?? new List<CraftingIngredientSaveData>();
            Status = (CraftingJobStatus)saveData.Status;
            RemainingSeconds = saveData.RemainingSeconds;
        }

        public bool IsComplete => RemainingSeconds <= 0f && Status == CraftingJobStatus.InProgress;

        public void Update(float deltaTime)
        {
            if (Status != CraftingJobStatus.InProgress)
                return;

            RemainingSeconds -= deltaTime;
            if (RemainingSeconds <= 0f)
            {
                RemainingSeconds = 0f;
            }
        }

        public void Complete()
        {
            Status = CraftingJobStatus.Completed;
            RemainingSeconds = 0f;
        }

        public void Cancel()
        {
            Status = CraftingJobStatus.Cancelled;
        }

        public CraftingJobSaveData CaptureSaveData()
        {
            return new CraftingJobSaveData
            {
                JobId = JobId,
                StationInstanceId = StationInstanceId,
                RecipeId = RecipeId,
                Status = (int)Status,
                RemainingSeconds = RemainingSeconds,
                OutputItemId = OutputItemId,
                OutputAmount = OutputAmount,
                IngredientsConsumed = new List<CraftingIngredientSaveData>(IngredientsConsumed)
            };
        }

        private static List<CraftingIngredientSaveData> CaptureIngredients(RecipeIngredient[] ingredients)
        {
            var data = new List<CraftingIngredientSaveData>();
            if (ingredients == null)
            {
                return data;
            }

            foreach (var ingredient in ingredients)
            {
                if (!string.IsNullOrWhiteSpace(ingredient.ItemId) && ingredient.Amount > 0)
                {
                    data.Add(new CraftingIngredientSaveData { ItemId = ingredient.ItemId, Amount = ingredient.Amount });
                }
            }

            return data;
        }
    }

    [Serializable]
    public class CraftingIngredientSaveData
    {
        public string ItemId;
        public int Amount;
    }

    [Serializable]
    public class CraftingJobSaveData
    {
        public string JobId;
        public string StationInstanceId;
        public string RecipeId;
        public int Status;
        public float RemainingSeconds;
        public string OutputItemId;
        public int OutputAmount;
        public List<CraftingIngredientSaveData> IngredientsConsumed = new List<CraftingIngredientSaveData>();
    }
}
