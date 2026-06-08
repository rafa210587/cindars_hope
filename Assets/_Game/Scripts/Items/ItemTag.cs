using System;

namespace CindarsHope.Items
{
    [Flags]
    public enum ItemTag : long
    {
        None = 0,
        Sellable = 1L << 0,
        CraftingMaterial = 1L << 1,
        CookingIngredient = 1L << 2,
        AlchemyIngredient = 1L << 3,
        UpgradeMaterial = 1L << 4,
        RepairMaterial = 1L << 5,
        Giftable = 1L << 6,
        QuestRequired = 1L << 7,
        OrderEligible = 1L << 8,
        FestivalEligible = 1L << 9,
        EquipmentMaterial = 1L << 10,
        Consumable = 1L << 11,
        Placeable = 1L << 12,
        BuildMaterial = 1L << 13,
        LoreLocked = 1L << 14,
        NonSellable = 1L << 15,
        UniqueProtected = 1L << 16,
        Stackable = 1L << 17,
        PerishableFuture = 1L << 18
    }
}
