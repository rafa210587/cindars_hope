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

            // fable_19 (CA-2): contrato de registro de fazenda (Mara, 100g) => +5% no preço do
            // shipping da fazenda. Ponto único do hook (fachada CityServiceAccess, fail-closed: sem
            // contrato => preço inalterado). O shipping da fazenda NUNCA é bloqueado por licença
            // urbana — só ganha bônus quando o contrato existe.
            price = CindarsHope.City.Services.CityServiceAccess.ApplyFarmRegistryContract(price);

            return price;
        }
    }
}
