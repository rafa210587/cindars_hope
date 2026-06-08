using System.Collections.Generic;

namespace CindarsHope.Crafting
{
    public class RecipeIngredient
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public bool IsProtected { get; set; } = false;
    }

    public class QualityInfluenceRules
    {
        public bool PropagateHighestInputQuality { get; set; } = true;
        public bool AverageInputQuality { get; set; } = false;
        public int FixedQualityOverride { get; set; } = -1;
    }

    public class RecipeDefinition
    {
        public string RecipeId { get; set; }
        public string DisplayName { get; set; }
        public RecipeType RecipeType { get; set; } = RecipeType.CraftRecipe;
        public string OutputItemId { get; set; }
        public int OutputQuantity { get; set; } = 1;
        public string RequiredStation { get; set; }
        public int RequiredStationLevel { get; set; } = 1;
        public List<RecipeIngredient> RequiredIngredients { get; set; } = new List<RecipeIngredient>();
        public List<RecipeIngredient> OptionalIngredients { get; set; } = new List<RecipeIngredient>();
        public int RequiredGold { get; set; } = 0;
        public int RequiredTimeTicks { get; set; } = 0;
        public string RequiredSkillTreeOrNode { get; set; }
        public string RequiredRecipeUnlock { get; set; }
        public int RequiredReputation { get; set; } = 0;
        public string RequiredQuestFlag { get; set; }
        public int RequiredFarmLevel { get; set; } = 0;
        public int RequiredCaveProgress { get; set; } = 0;
        public QualityInfluenceRules QualityInfluenceRules { get; set; } = new QualityInfluenceRules();
        public List<string> DebugTags { get; set; } = new List<string>();

        public bool IsProcessing => RequiredTimeTicks > 0;
        public bool RequiresGating =>
            !string.IsNullOrEmpty(RequiredSkillTreeOrNode) ||
            !string.IsNullOrEmpty(RequiredRecipeUnlock) ||
            !string.IsNullOrEmpty(RequiredQuestFlag) ||
            RequiredReputation > 0 || RequiredFarmLevel > 0 || RequiredCaveProgress > 0;
    }
}
