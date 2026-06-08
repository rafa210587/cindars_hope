namespace CindarsHope.Farm.Resources
{
    public enum ResourceNodeType
    {
        Tree = 0,
        Stump = 1,
        Rock = 2,
        Boulder = 3,
        Forage = 4,
        FishingSpot = 5,
        WaterResource = 6,
        ClayPatch = 7,
        FiberPatch = 8,
        OreLight = 9,
        SpecialLoreNode = 20,
        EndgameNode = 21
    }

    public enum ResourceNodeCurrentState
    {
        Available = 0,
        Harvested = 1,
        Depleted = 2,
        Regrowing = 3,
        Blocked = 4,
        Hidden = 5,
        Reserved = 6
    }
}
