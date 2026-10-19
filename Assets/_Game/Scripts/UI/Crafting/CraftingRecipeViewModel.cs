using CindarsHope.Craft.Data;

namespace CindarsHope.UI.Crafting
{
    /// <summary>
    /// SPEC 04: Recipe state projection for UI display.
    /// Determines if recipe is craftable, locked, hidden, or missing requirements.
    /// </summary>
    public class CraftingRecipeViewModel
    {
        public enum RecipeState
        {
            KnownCraftable,
            KnownMissingMaterials,
            KnownMissingStation,
            KnownLockedBySkill,
            KnownLockedByQuest,
            UnknownHidden,
            ProcessingActive,
            ReadyToCollect
        }

        public string RecipeId { get; set; }
        public string DisplayName { get; set; }
        public string OutputItemId { get; set; }
        public int OutputAmount { get; set; }
        public float CraftTimeSeconds { get; set; }

        public RecipeState State { get; set; } = RecipeState.UnknownHidden;
        public bool CanCraft => State == RecipeState.KnownCraftable;
        public bool IsLocked => State == RecipeState.KnownLockedBySkill || State == RecipeState.KnownLockedByQuest;
        public bool IsHidden => State == RecipeState.UnknownHidden;
        public bool IsProcessing => State == RecipeState.ProcessingActive;
        public bool IsReady => State == RecipeState.ReadyToCollect;

        public string StateDescription { get; set; }
        public string LockReason { get; set; }
    }

    /// <summary>
    /// Recipe state evaluator - determines craftability.
    /// </summary>
    public static class RecipeStateEvaluator
    {
        /// <summary>
        /// Evaluate if recipe can be crafted based on materials and station availability.
        /// Called by UI after consulting backend for material/station state.
        /// </summary>
        public static CraftingRecipeViewModel.RecipeState EvaluateState(
            bool hasMaterials,
            bool hasStation,
            bool isLocked,
            bool isHidden,
            bool isProcessing,
            bool isReadyToCollect)
        {
            if (isHidden)
                return CraftingRecipeViewModel.RecipeState.UnknownHidden;
            if (isLocked)
                return CraftingRecipeViewModel.RecipeState.KnownLockedBySkill;
            if (isProcessing)
                return CraftingRecipeViewModel.RecipeState.ProcessingActive;
            if (isReadyToCollect)
                return CraftingRecipeViewModel.RecipeState.ReadyToCollect;
            if (!hasStation)
                return CraftingRecipeViewModel.RecipeState.KnownMissingStation;
            if (!hasMaterials)
                return CraftingRecipeViewModel.RecipeState.KnownMissingMaterials;
            return CraftingRecipeViewModel.RecipeState.KnownCraftable;
        }
    }
}
