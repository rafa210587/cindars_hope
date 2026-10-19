using System.Collections.Generic;

namespace CindarsHope.Economy.Pricing
{
    public class PricingProfile
    {
        public string ProfileId { get; set; } = "default";
        public float BaseGlobalSellMultiplier { get; set; } = 1f;
        public float BaseGlobalBuyMultiplier { get; set; } = 1f;

        // Channel sell multipliers: SellPoint x0.90, GenericShop x0.70, Specialized x0.85
        public Dictionary<PriceChannel, float> ChannelSellMultipliers { get; set; } = new Dictionary<PriceChannel, float>
        {
            { PriceChannel.SellPoint, 0.90f },
            { PriceChannel.GenericShopBuyFromPlayer, 0.70f },
            { PriceChannel.SpecializedShopBuyFromPlayer, 0.85f },
            { PriceChannel.ShopSellToPlayer, 1.30f },
            { PriceChannel.OrderReward, 1.10f },
            { PriceChannel.FestivalReward, 1.20f }
        };

        // Quality multipliers: Q0 x1.00, Q1 x1.15, Q2 x1.35, Q3 x1.70, Q4 x2.20
        public float[] QualityMultipliers { get; set; } = { 1.00f, 1.15f, 1.35f, 1.70f, 2.20f };

        // Rarity sell modifiers indexed by rarity (0=Common..5=Unique)
        public float[] RaritySellModifiers { get; set; } = { 1.00f, 1.10f, 1.25f, 1.50f, 2.00f, 3.00f };
        public float[] RarityBuyModifiers { get; set; } = { 1.00f, 1.15f, 1.35f, 1.60f, 2.20f, 3.50f };

        public float DemandModifier { get; set; } = 1f;
        public float SeasonModifier { get; set; } = 1f;
        public float ReputationSellModifier { get; set; } = 1f;
        public float ReputationDiscountModifier { get; set; } = 1f;
        public float StockScarcityModifier { get; set; } = 1f;
        public float StoryFlagModifier { get; set; } = 1f;

        public int MinPrice { get; set; } = 1;
        public RoundingRule RoundingRule { get; set; } = RoundingRule.Floor;

        public static PricingProfile Default() => new PricingProfile();
    }
}
