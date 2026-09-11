using System;

namespace CindarsHope.Foundation
{
    public enum MarketPriceDirection
    {
        None = 0,
        BuyFromShop = 1,
        SellToShop = 2
    }

    /// <summary>Neutral bridge from the live skill state to economy pricing.</summary>
    public static class MarketSkillModifierProvider
    {
        public const string NodeId = "crafting_shop_sense";
        public const string BuyVariantId = "buy";
        public const string SellVariantId = "sell";
        public const float PerRank = .05f;
        public const float Cap = .15f;

        public static Func<MarketPriceDirection, float> MultiplierSource;

        public static float Resolve(MarketPriceDirection direction)
            => Math.Max(0f, MultiplierSource?.Invoke(direction) ?? 1f);

        public static float Resolve(int rank, string variant, MarketPriceDirection direction)
        {
            float value = Math.Min(Cap, Math.Max(0, rank) * PerRank);
            if (direction == MarketPriceDirection.BuyFromShop &&
                string.Equals(variant, BuyVariantId, StringComparison.Ordinal))
                return 1f - value;
            if (direction == MarketPriceDirection.SellToShop &&
                string.Equals(variant, SellVariantId, StringComparison.Ordinal))
                return 1f + value;
            return 1f;
        }
    }
}
