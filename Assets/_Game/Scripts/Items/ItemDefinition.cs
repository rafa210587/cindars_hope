using System.Collections.Generic;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Items
{
    // Pure C# item definition — NOT a ScriptableObject
    // Exists alongside ItemDataSO (which remains for Unity scene wiring)
    public enum ItemStackBehavior { Stackable = 0, InstanceOnly = 1 }

    public class ItemDefinition
    {
        public string ItemId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ItemCategory Category { get; set; } = ItemCategory.Misc;
        public ItemTag Tags { get; set; } = ItemTag.None;
        public ItemRarity Rarity { get; set; } = ItemRarity.Common;
        public int BaseValue { get; set; } = 0;
        public int MaxStack { get; set; } = 1;
        public bool QualityEnabled { get; set; } = false;
        public ItemStackBehavior StackBehavior { get; set; } = ItemStackBehavior.Stackable;
        public bool IsQuestItem { get; set; } = false;
        public bool IsKeyItem { get; set; } = false;
        public bool IsUnique { get; set; } = false;
        public List<string> LoreTags { get; set; } = new List<string>();
        public string IconId { get; set; }
        public string SpriteId { get; set; }

        public ItemEconomicFlags EconomicFlags
        {
            get
            {
                if (IsKeyItem) return ItemEconomicFlags.ForKeyItem();
                if (IsQuestItem) return ItemEconomicFlags.ForQuestItem();
                if (IsUnique) return ItemEconomicFlags.ForUnique();
                if (Tags.HasFlag(ItemTag.LoreLocked)) return ItemEconomicFlags.ForLoreItem();
                return new ItemEconomicFlags
                {
                    CanSell = Tags.HasFlag(ItemTag.Sellable) && !Tags.HasFlag(ItemTag.NonSellable),
                    CanBuy = true,
                    CanGift = Tags.HasFlag(ItemTag.Giftable),
                    CanDiscard = true,
                    CanCraftWith = Tags.HasFlag(ItemTag.CraftingMaterial),
                    CanCookWith = Tags.HasFlag(ItemTag.CookingIngredient),
                    CanUseAsIngredient = Tags.HasFlag(ItemTag.CraftingMaterial) || Tags.HasFlag(ItemTag.AlchemyIngredient)
                };
            }
        }

        public bool HasTag(ItemTag tag) => Tags.HasFlag(tag);
        public bool IsStackable => StackBehavior == ItemStackBehavior.Stackable;
        public bool IsInstance => StackBehavior == ItemStackBehavior.InstanceOnly;
    }

    public class ItemDefinitionValidator
    {
        public static List<string> Validate(ItemDefinition def)
        {
            var errors = new List<string>();
            if (def == null) { errors.Add("ItemDefinition is null"); return errors; }
            if (string.IsNullOrEmpty(def.ItemId)) errors.Add("ItemId is required");
            if (def.EconomicFlags.CanSell && def.BaseValue <= 0) errors.Add($"{def.ItemId}: CanSell=true but BaseValue<=0");
            if (def.EconomicFlags.CanBuy && def.BaseValue <= 0) errors.Add($"{def.ItemId}: CanBuy=true but BaseValue<=0");
            if (def.IsKeyItem && def.EconomicFlags.CanSell) errors.Add($"{def.ItemId}: KeyItem should not be sellable");
            if (def.IsQuestItem && def.EconomicFlags.CanSell) errors.Add($"{def.ItemId}: QuestItem should not be sellable");
            return errors;
        }
    }
}
