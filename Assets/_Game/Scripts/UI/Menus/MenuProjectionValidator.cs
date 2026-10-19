using CindarsHope.UI.Shop;
using CindarsHope.UI.Fonte;

namespace CindarsHope.UI.Menus
{
    public static class MenuProjectionValidator
    {
        // Shop: buy mode requires item + stock > 0 + player can afford
        public static MenuCommandValidationResult ValidateShopBuy(ShopMenuViewModel shop)
        {
            if (shop?.SelectedItemDetails == null) return MenuCommandValidationResult.InvalidMissingTarget;
            if (shop.SelectedItemDetails.Stock <= 0) return MenuCommandValidationResult.InvalidBlocked;
            if (shop.TotalPrice > shop.PlayerGold) return MenuCommandValidationResult.InvalidInsufficientResources;
            return MenuCommandValidationResult.Valid;
        }

        // Shop: sell mode requires sellable item (no quest/key)
        public static MenuCommandValidationResult ValidateShopSell(ShopMenuViewModel shop)
        {
            if (shop?.SelectedItemDetails == null) return MenuCommandValidationResult.InvalidMissingTarget;
            if (shop.SelectedItemDetails.IsQuestItem || shop.SelectedItemDetails.IsKeyItem)
                return MenuCommandValidationResult.InvalidBlocked;
            if (!shop.SelectedItemDetails.IsSellable) return MenuCommandValidationResult.InvalidBlocked;
            return MenuCommandValidationResult.Valid;
        }

        // Fonte: function not unlocked
        public static MenuCommandValidationResult ValidateFonteFunction(FonteMenuViewModel vm, FonteUnlockedFunction function)
        {
            if (vm == null) return MenuCommandValidationResult.InvalidMissingTarget;
            if (!vm.UnlockedFunctions.Contains(function)) return MenuCommandValidationResult.InvalidSpoilerGate;
            if (!string.IsNullOrEmpty(vm.BlockedReason)) return MenuCommandValidationResult.InvalidBlocked;
            return MenuCommandValidationResult.Valid;
        }
    }
}
