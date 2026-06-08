using System;
using System.Collections.Generic;

namespace CindarsHope.Economy.Pricing
{
    public class EconomyPricingService
    {
        private readonly PricingProfile _profile;

        public EconomyPricingService(PricingProfile profile = null)
        {
            _profile = profile ?? PricingProfile.Default();
        }

        public PriceResult CalculatePrice(PriceRequest req)
        {
            if (req == null) return PriceResult.Fail("PriceRequest is null");

            // Protection gates
            if (req.IsQuestItem || req.IsKeyItem)
                return PriceResult.Blocked("Quest/Key items cannot be priced for trade",
                    req.IsQuestItem ? PriceProtectionFlag.QuestItemBlocked : PriceProtectionFlag.KeyItemBlocked);
            if (req.IsLoreLocked)
                return PriceResult.Blocked("Lore-locked items cannot be priced for trade", PriceProtectionFlag.LoreLockedBlocked);
            if (req.IsNonSellable && IsSellChannel(req.Channel))
                return PriceResult.Blocked("NonSellable item in sell channel", PriceProtectionFlag.NonSellableBlocked);

            if (req.BaseValue <= 0) return PriceResult.Fail("BaseValue must be > 0");
            if (req.Quantity <= 0) return PriceResult.Fail("Quantity must be > 0");

            float channelMult = GetChannelMultiplier(req.Channel);
            float qualityMult = GetQualityMultiplier(req.Quality);
            float rarityMod = IsBuyChannel(req.Channel)
                ? GetRarityModifier(req.Rarity, _profile.RarityBuyModifiers)
                : GetRarityModifier(req.Rarity, _profile.RaritySellModifiers);

            float demandMod = req.DemandModifierOverride * _profile.DemandModifier;
            float seasonMod = _profile.SeasonModifier;
            float reputMod = IsBuyChannel(req.Channel) ? _profile.ReputationDiscountModifier : _profile.ReputationSellModifier;
            float stockMod = IsBuyChannel(req.Channel) ? req.StockScarcityOverride * _profile.StockScarcityModifier : 1f;
            float storyMod = _profile.StoryFlagModifier;

            float rawPrice = req.BaseValue * channelMult * qualityMult * rarityMod * demandMod * reputMod * seasonMod * stockMod * storyMod;

            int unitPrice = ApplyRounding(rawPrice, _profile.RoundingRule);
            unitPrice = Math.Max(unitPrice, _profile.MinPrice);

            var applied = new List<string>
            {
                $"BaseValue={req.BaseValue}",
                $"Channel={channelMult:F2}",
                $"Quality={qualityMult:F2}",
                $"Rarity={rarityMod:F2}",
                $"Demand={demandMod:F2}",
                $"Season={seasonMod:F2}",
                $"Repute={reputMod:F2}",
                $"Stock={stockMod:F2}",
                $"Story={storyMod:F2}"
            };

            return new PriceResult
            {
                Success = true,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * req.Quantity,
                AppliedMultipliers = applied,
                RoundingApplied = _profile.RoundingRule != RoundingRule.Floor || rawPrice != (int)rawPrice,
                DebugExplanation = $"raw={rawPrice:F2} -> unit={unitPrice}"
            };
        }

        private float GetChannelMultiplier(PriceChannel channel)
        {
            if (_profile.ChannelSellMultipliers.TryGetValue(channel, out float m)) return m;
            return 1f;
        }

        private float GetQualityMultiplier(int quality)
        {
            var q = _profile.QualityMultipliers;
            if (quality < 0 || quality >= q.Length) return 1f;
            return q[quality];
        }

        private float GetRarityModifier(int rarity, float[] modifiers)
        {
            if (rarity < 0 || rarity >= modifiers.Length) return 1f;
            return modifiers[rarity];
        }

        private static int ApplyRounding(float value, RoundingRule rule)
        {
            return rule switch
            {
                RoundingRule.Ceil => (int)Math.Ceiling(value),
                RoundingRule.Round => (int)Math.Round(value),
                _ => (int)Math.Floor(value)
            };
        }

        private static bool IsSellChannel(PriceChannel channel) =>
            channel == PriceChannel.SellPoint ||
            channel == PriceChannel.GenericShopBuyFromPlayer ||
            channel == PriceChannel.SpecializedShopBuyFromPlayer ||
            channel == PriceChannel.OrderReward ||
            channel == PriceChannel.FestivalReward;

        private static bool IsBuyChannel(PriceChannel channel) =>
            channel == PriceChannel.ShopSellToPlayer ||
            channel == PriceChannel.ServicePrice;
    }
}
