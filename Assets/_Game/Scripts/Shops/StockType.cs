namespace CindarsHope.Shops
{
    public enum StockType
    {
        BaseStock = 0,
        SeasonalStock,
        ReputationStock,
        QuestUnlockedStock,
        CaveProgressStock,
        FarmLevelStock,
        LimitedStock,
        RotatingStock,
        UniqueStock,
        EventStock,
        TravelingMerchantStock,
        PlayerSoldStockFuture
    }

    public enum RestockPolicyType
    {
        DailyMorning = 0,
        Weekly,
        SeasonStart,
        OnQuestFlagChanged,
        OnReputationTierChanged,
        OnCaveProgressChanged,
        OnFarmLevelChanged,
        EventStart,
        TravelingMerchantVisit,
        ManualStoryOnly,
        Never
    }
}
