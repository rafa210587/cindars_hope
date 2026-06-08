using System.Collections.Generic;

namespace CindarsHope.Economy.Pricing
{
    public class ArbitrageCheckResult
    {
        public bool IsValid { get; set; }
        public List<string> Violations { get; set; } = new List<string>();
    }

    public class AntiArbitrageValidator
    {
        private readonly EconomyPricingService _service;

        public AntiArbitrageValidator(EconomyPricingService service)
        {
            _service = service;
        }

        // Checks: ShopSellToPlayer price must be >= ShopBuyFromPlayerPrice unless exception is bounded
        public ArbitrageCheckResult ValidateArbitrage(PriceRequest baseReq, bool isLimitedException = false)
        {
            var result = new ArbitrageCheckResult { IsValid = true };

            var buyReq = Clone(baseReq, PriceChannel.ShopSellToPlayer);
            var sellReq = Clone(baseReq, PriceChannel.GenericShopBuyFromPlayer);

            var buyResult = _service.CalculatePrice(buyReq);
            var sellResult = _service.CalculatePrice(sellReq);

            if (!buyResult.Success || !sellResult.Success)
                return result; // blocked items — not an arbitrage concern

            if (buyResult.UnitPrice <= sellResult.UnitPrice && !isLimitedException)
            {
                result.IsValid = false;
                result.Violations.Add(
                    $"ARBITRAGE: ShopSellToPlayer={buyResult.UnitPrice} <= ShopBuyFromPlayer={sellResult.UnitPrice} " +
                    $"for item={baseReq.ItemId}. Must be bounded exception.");
            }

            return result;
        }

        // Checks that no channel allows buying cheaper than selling back indefinitely
        public ArbitrageCheckResult ValidateNoInfiniteLoop(PriceRequest baseReq)
        {
            var result = new ArbitrageCheckResult { IsValid = true };

            var buy = _service.CalculatePrice(Clone(baseReq, PriceChannel.ShopSellToPlayer));
            var sell = _service.CalculatePrice(Clone(baseReq, PriceChannel.SpecializedShopBuyFromPlayer));

            if (!buy.Success || !sell.Success) return result;

            if (sell.UnitPrice >= buy.UnitPrice)
            {
                result.IsValid = false;
                result.Violations.Add(
                    $"INFINITE_LOOP: SpecializedShopBuyFromPlayer={sell.UnitPrice} >= ShopSellToPlayer={buy.UnitPrice} " +
                    $"for item={baseReq.ItemId}. Infinite buy-sell loop possible.");
            }

            return result;
        }

        private static PriceRequest Clone(PriceRequest req, PriceChannel channel)
        {
            return new PriceRequest
            {
                ItemId = req.ItemId,
                Category = req.Category,
                BaseValue = req.BaseValue,
                Quantity = req.Quantity,
                Quality = req.Quality,
                Rarity = req.Rarity,
                Channel = channel,
                ReputationTier = req.ReputationTier,
                DemandModifierOverride = req.DemandModifierOverride,
                StockScarcityOverride = req.StockScarcityOverride,
                IsQuestItem = req.IsQuestItem,
                IsKeyItem = req.IsKeyItem,
                IsLoreLocked = req.IsLoreLocked,
                IsNonSellable = req.IsNonSellable,
                IsUnique = req.IsUnique
            };
        }
    }
}
