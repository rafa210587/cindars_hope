namespace CindarsHope.City.Services
{
    public enum CityServiceType
    {
        Unknown = 0,
        ShopGeneral,
        ShopSeeds,
        ShopNight,
        BlacksmithRepair,
        BlacksmithUpgrade,
        CarpentryBuild,
        CarpentryMove,
        AlchemyPotion,
        AlchemyFertilizer,
        TavernFood,
        TavernRumor,
        InnLodging,
        RoadsGuildContract,
        RoadsGuildMap,
        TownHallLicense,
        TownHallContract,
        TempleBlessing,
        TempleOath,
        ArchiveResearch,
        ArchiveTranslation,
        TailorBagUpgrade,
        TailorClothing,
        RanchAnimals,
        RanchFeed,
        HerbalistAntidote,
        HerbalistForage
    }

    public enum ContractRepeatPolicy { OneTime = 0, Daily, Weekly, Seasonal, Unlimited }
}
