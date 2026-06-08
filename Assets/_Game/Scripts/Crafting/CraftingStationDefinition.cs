using System.Collections.Generic;

namespace CindarsHope.Crafting
{
    public class CraftingStationDefinition
    {
        public string StationId { get; set; }
        public string DisplayName { get; set; }
        public StationType StationType { get; set; } = StationType.Workbench;
        public string RequiredBuildingId { get; set; }
        public int RequiredFarmLevel { get; set; } = 0;
        public string RequiredQuestFlag { get; set; }
        public int StationLevel { get; set; } = 1;
        public List<RecipeType> AllowedRecipeTypes { get; set; } = new List<RecipeType>();
        public int InputCapacity { get; set; } = 10;
        public int OutputCapacity { get; set; } = 5;
        public int ProcessingSlots { get; set; } = 1;

        public bool AcceptsRecipe(RecipeType recipeType) => AllowedRecipeTypes.Contains(recipeType);
    }

    public class PlayerKnownRecipes
    {
        public HashSet<string> KnownRecipeIds { get; set; } = new HashSet<string>();
        public HashSet<string> UnlockedBlueprintIds { get; set; } = new HashSet<string>();

        public bool Knows(string recipeId) => KnownRecipeIds.Contains(recipeId);
        public bool HasBlueprint(string recipeId) => UnlockedBlueprintIds.Contains(recipeId);
        public bool CanCraft(string recipeId) => Knows(recipeId) || HasBlueprint(recipeId);
        public void Learn(string recipeId) => KnownRecipeIds.Add(recipeId);
    }
}
