namespace CindarsHope.Cave.Loot
{
    public enum CaveLootSourceType
    {
        Unknown = 0,
        MiningNode,
        RareVein,
        TreasureChest,
        LockedChest,
        BossGateChest,
        SecretRoomReward,
        SpecialRoomReward,
        FishingSpot,
        LorePoint,
        Level100GateReward,
        Level101LoreReward
    }

    public enum DepletedStatePolicy { DepletedUntilNewRun = 0, DepletedForever, DepletedUntilNextSeason }
    public enum OpenedStatePolicy { OpenedForever = 0, OpenedUntilNewRun }
}
