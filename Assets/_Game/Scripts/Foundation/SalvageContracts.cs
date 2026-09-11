using System;
using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    public readonly struct SalvageIngredient
    {
        public SalvageIngredient(string itemId, int amount) { ItemId = itemId; Amount = amount; }
        public string ItemId { get; }
        public int Amount { get; }
    }

    public sealed class SalvageRecipeDefinition
    {
        public string RecipeId = string.Empty;
        public string OutputItemId = string.Empty;
        public int OutputAmount;
        public readonly List<SalvageIngredient> Ingredients = new List<SalvageIngredient>();
    }

    public readonly struct SalvageReward
    {
        public SalvageReward(string itemId, int amount) { ItemId = itemId; Amount = amount; }
        public string ItemId { get; }
        public int Amount { get; }
    }

    public interface ISalvageRecipeProvider
    {
        bool TryGetCanonicalSalvageRecipe(string outputItemId, out SalvageRecipeDefinition recipe);
    }

    public readonly struct ItemSalvagedEvent
    {
        public ItemSalvagedEvent(string itemId, string itemInstanceId, string recipeId,
            IReadOnlyList<SalvageReward> rewards)
        {
            ItemId = itemId;
            ItemInstanceId = itemInstanceId;
            RecipeId = recipeId;
            Rewards = rewards;
        }
        public string ItemId { get; }
        public string ItemInstanceId { get; }
        public string RecipeId { get; }
        public IReadOnlyList<SalvageReward> Rewards { get; }
    }
}
