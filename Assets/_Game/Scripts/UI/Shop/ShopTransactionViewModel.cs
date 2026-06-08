namespace CindarsHope.UI.Shop
{
    public class ShopItemViewModel
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
        public bool InStock => Stock > 0;
    }

    public class ShopTransactionViewModel
    {
        public ShopItemViewModel SelectedItem { get; set; }
        public int QuantitySelected { get; set; } = 1;
        public int PlayerGold { get; set; }

        public int TotalCost => SelectedItem != null ? SelectedItem.Price * QuantitySelected : 0;
        public bool CanAfford => TotalCost <= PlayerGold;
        public bool CanComplete => SelectedItem != null && CanAfford && SelectedItem.InStock;
    }
}
