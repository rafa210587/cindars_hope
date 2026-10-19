namespace CindarsHope.Items
{
    public class ItemEconomicFlags
    {
        public bool CanSell { get; set; } = true;
        public bool CanBuy { get; set; } = true;
        public bool CanGift { get; set; } = true;
        public bool CanDiscard { get; set; } = true;
        public bool CanCraftWith { get; set; } = true;
        public bool CanCookWith { get; set; } = false;
        public bool CanUseAsIngredient { get; set; } = true;
        public bool RequiresStrongDiscardConfirmation { get; set; } = false;
        public bool RequiresStrongSellConfirmation { get; set; } = false;

        // Apply protection rules based on item flags
        public static ItemEconomicFlags ForQuestItem() =>
            new ItemEconomicFlags { CanSell = false, CanDiscard = false, RequiresStrongDiscardConfirmation = true };

        public static ItemEconomicFlags ForKeyItem() =>
            new ItemEconomicFlags { CanSell = false, CanDiscard = false, RequiresStrongDiscardConfirmation = true };

        public static ItemEconomicFlags ForUnique() =>
            new ItemEconomicFlags { CanSell = false, RequiresStrongSellConfirmation = true, RequiresStrongDiscardConfirmation = true };

        public static ItemEconomicFlags ForLoreItem() =>
            new ItemEconomicFlags { CanSell = false, CanDiscard = false, LoreLocked = true };

        public bool LoreLocked { get; set; } = false;
    }
}
