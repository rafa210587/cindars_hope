namespace CindarsHope.Farm.Shipping
{
    public class ShippingPriceInput
    {
        public string ItemId { get; set; }
        public float BaseValue { get; set; }
        public int QualityTier { get; set; } = 0;
        public float ChannelMultiplier { get; set; } = 0.9f; // SellPoint: x0.90-x1.00
    }

    public class ShippingPriceResolver
    {
        // SellPoint channel multiplier — not final economy value, always less than shop specialized
        private const float SellPointChannelMultiplier = 0.95f;

        public float Resolve(ShippingPriceInput input)
        {
            if (input == null || input.BaseValue <= 0f) return 0f;

            float price = input.BaseValue * input.ChannelMultiplier;

            // Quality modifier: +5% per tier
            if (input.QualityTier > 0)
                price *= (1f + input.QualityTier * 0.05f);

            return price;
        }
    }
}
