namespace CindarsHope.Economy
{
    public static class SellableItemPolicy
    {
        public static bool IsSellable(string itemId)
        {
            switch (itemId)
            {
                case "item_crop_wheat":
                case "item_crop_carrot":
                case "item_fish_common":
                case "item_wood":
                    return true;
                default:
                    return false;
            }
        }
    }
}
