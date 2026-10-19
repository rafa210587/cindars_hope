namespace CindarsHope.Economy.Pricing
{
    public enum PriceChannel
    {
        Unknown = 0,
        SellPoint,
        GenericShopBuyFromPlayer,
        SpecializedShopBuyFromPlayer,
        ShopSellToPlayer,
        OrderReward,
        FestivalReward,
        ServicePrice,
        ContractFuture,
        Debug
    }

    public enum RoundingRule { Floor = 0, Ceil, Round }

    public enum PriceProtectionFlag
    {
        None = 0,
        QuestItemBlocked,
        KeyItemBlocked,
        LoreLockedBlocked,
        NonSellableBlocked,
        UniqueProtected
    }
}
