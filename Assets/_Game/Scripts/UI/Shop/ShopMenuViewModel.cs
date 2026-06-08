using System.Collections.Generic;

namespace CindarsHope.UI.Shop
{
    public enum ShopMode { Buy = 0, Sell = 1 }

    public enum ShopStockState { Normal = 0, LimitedStock = 1, UniqueOnly = 2, OutOfStock = 3, RestockPending = 4 }

    public class ShopRowViewModel
    {
        public string ItemId { get; set; }
        public string DisplayName { get; set; }
        public int UnitPrice { get; set; }
        public int Stock { get; set; }
        public bool IsLimitedStock { get; set; }
        public bool IsUniqueStock { get; set; }
        public bool IsSellable { get; set; }
        public bool IsQuestItem { get; set; }
        public bool IsKeyItem { get; set; }
    }

    public class ShopMenuViewModel
    {
        public string ShopId { get; set; }
        public string ShopName { get; set; }
        public ShopMode Mode { get; set; }
        public int PlayerGold { get; set; }
        public List<ShopRowViewModel> ShopInventoryRows { get; set; } = new List<ShopRowViewModel>();
        public List<ShopRowViewModel> PlayerSellableRows { get; set; } = new List<ShopRowViewModel>();
        public ShopRowViewModel SelectedItemDetails { get; set; }
        public int QuantitySelector { get; set; } = 1;
        public int UnitPrice { get; set; }
        public int TotalPrice => UnitPrice * QuantitySelector;
        public ShopStockState StockState { get; set; }
        public string EmptyStateMessage { get; set; }
        public bool CanConfirm { get; set; }
        public string BlockedReason { get; set; }
        public bool HasShopInventory => ShopInventoryRows != null && ShopInventoryRows.Count > 0;
        public bool HasPlayerSellable => PlayerSellableRows != null && PlayerSellableRows.Count > 0;
    }
}
