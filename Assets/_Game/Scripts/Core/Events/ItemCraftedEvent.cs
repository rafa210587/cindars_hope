namespace CindarsHope.Core.Events
{
    public readonly struct ItemCraftedEvent
    {
        public string RecipeId { get; }
        public string ItemId { get; }
        public int Amount { get; }

        public ItemCraftedEvent(string recipeId, string itemId, int amount)
        {
            RecipeId = recipeId;
            ItemId = itemId;
            Amount = amount;
        }
    }
}
