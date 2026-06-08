namespace CindarsHope.Farm.Tools
{
    public enum FarmToolType
    {
        None = 0,
        Hoe = 1,
        AxeTool = 2,
        Pickaxe = 3,
        WateringCan = 4,
        FishingRod = 5,
        HammerToolFuture = 99
    }

    // Extended tier mapping aligned to equipment direction
    // Existing CindarsHope.Tools.ToolTier is preserved; this enum is farm-specific
    public enum FarmToolTier
    {
        Tier0_Improvised = 0,
        Tier1_Copper = 1,
        Tier2_Iron = 2,
        Tier3_Steel = 3,
        Tier4_Reinforced = 4,
        Tier5_Mithril = 5,
        Tier6_Meteoric = 6
    }
}
