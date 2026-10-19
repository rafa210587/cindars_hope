namespace CindarsHope.Farm.Expansion
{
    public enum FarmZoneType
    {
        Fixed = 0,       // Fixed anchors, paths, entrances — cannot be modified
        Free = 1,        // Player can build/modify
        Blocked = 2,     // Not yet unlocked
        Lore = 3,        // Narrative/lore protected (Fonte, Mana root)
        Endgame = 4,     // Endgame reserved (quarry, arcane)
        Path = 5,        // Must remain clear for access
        Water = 6        // Lake/water — no land buildings
    }

    public enum FarmPropertyLevel
    {
        Level1_InicialPlot = 1,     // 40x32 tiles
        Level2_OpenFarm = 2,        // 56x40
        Level3_ProductiveFarm = 3,  // 72x52
        Level4_SpecializedFarm = 4, // 88x64
        Level5_FullFarm = 5         // 104x72
    }
}
