using System;

namespace CindarsHope.Economy
{
    public enum SellContext
    {
        Shipping,
        NpcBuy,
        EventStall
    }

    public static class ItemPriceResolver
    {
        public static int ResolveSellingPrice(int baseValue, SellContext ctx,
                                              EconomyBalanceConfigSO config = null)
        {
            if (baseValue <= 0) return 0;

            float mult;
            switch (ctx)
            {
                case SellContext.NpcBuy:
                    mult = config != null ? config.NpcSellMultiplier : 0.9f;
                    break;
                case SellContext.EventStall:
                case SellContext.Shipping:
                default:
                    mult = config != null ? config.ShippingBuybackMultiplier : 1.0f;
                    break;
            }

            // Multiplicadores decimais como 0.9f podem produzir 89.99999 para base 100.
            // A regra deste resolver e arredondamento monetario, nao truncamento binario.
            return Math.Max(0, (int)Math.Round(baseValue * (double)mult, MidpointRounding.AwayFromZero));
        }
    }
}
