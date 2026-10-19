using System.Collections.Generic;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Economy.Pricing
{
    public class PriceRequest
    {
        public string ItemId { get; set; }
        public ItemCategory Category { get; set; } = ItemCategory.Misc;
        public int BaseValue { get; set; } = 0;
        public int Quantity { get; set; } = 1;
        public int Quality { get; set; } = 0;
        public int Rarity { get; set; } = 0;
        public PriceChannel Channel { get; set; } = PriceChannel.SellPoint;
        public string ShopId { get; set; }
        public string NpcId { get; set; }
        public int ReputationTier { get; set; } = 0;
        public string Season { get; set; }
        public float DemandModifierOverride { get; set; } = 1f;
        public List<string> ActiveStoryFlags { get; set; } = new List<string>();
        public float StockScarcityOverride { get; set; } = 1f;
        public bool IsOrderRequest { get; set; } = false;
        public string ServiceType { get; set; }

        // Protection flags set by ItemDefinition or policy
        public bool IsQuestItem { get; set; } = false;
        public bool IsKeyItem { get; set; } = false;
        public bool IsLoreLocked { get; set; } = false;
        public bool IsNonSellable { get; set; } = false;
        public bool IsUnique { get; set; } = false;
    }
}
