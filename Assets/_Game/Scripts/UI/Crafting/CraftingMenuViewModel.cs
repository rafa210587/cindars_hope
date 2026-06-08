using System.Collections.Generic;

namespace CindarsHope.UI.Crafting
{
    public class ProcessingJobViewModel
    {
        public string JobId { get; set; }
        public string RecipeId { get; set; }
        public string OutputItemId { get; set; }
        public float ElapsedSeconds { get; set; }
        public float TotalSeconds { get; set; }
        public bool IsReadyToCollect { get; set; }
        public float ProgressPercent => TotalSeconds > 0 ? ElapsedSeconds / TotalSeconds : 0f;
    }

    public class CraftingMenuViewModel
    {
        public string StationId { get; set; }
        public string StationName { get; set; }
        public List<CraftingRecipeViewModel> KnownRecipes { get; set; } = new List<CraftingRecipeViewModel>();
        public List<CraftingRecipeViewModel> BlockedRecipes { get; set; } = new List<CraftingRecipeViewModel>();
        public CraftingRecipeViewModel SelectedRecipe { get; set; }
        public List<CraftingMaterialRequirementViewModel> RequiredInputs { get; set; } = new List<CraftingMaterialRequirementViewModel>();
        public List<int> AvailableAmounts { get; set; } = new List<int>();
        public List<CraftingMaterialRequirementViewModel> MissingInputs { get; set; } = new List<CraftingMaterialRequirementViewModel>();
        public string RequiredStation { get; set; }
        public float RequiredTime { get; set; }
        public string ExpectedOutput { get; set; }
        public string PredictedQuality { get; set; }
        public bool CanCraft { get; set; }
        public bool CanCraftBulk { get; set; }
        public string BlockedReason { get; set; }
        public List<ProcessingJobViewModel> ProcessingJobs { get; set; } = new List<ProcessingJobViewModel>();
        public bool HasProcessingJobs => ProcessingJobs != null && ProcessingJobs.Count > 0;
        public bool HasMissingInputs => MissingInputs != null && MissingInputs.Count > 0;
    }
}
