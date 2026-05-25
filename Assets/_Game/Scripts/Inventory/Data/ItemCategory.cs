namespace CindarsHope.Inventory.Data
{
    public enum ItemCategory
    {
        // Keep legacy values stable because Unity serializes enum values by integer.
        Seed = 0,
        Crop = 1,
        Food = 2,
        Material = 3,
        Tool = 4,
        Fish = 5,
        Misc = 6,

        // New taxonomy values must be appended with explicit high values.
        None = 100,
        Consumable = 101,
        Weapon = 102,
        Magic = 103,
        Ammo = 104,
        Ore = 105,
        Gem = 106,
        MonsterDrop = 107,
        Quest = 108,
        KeyItem = 109,
        Furniture = 110
    }

    public enum ConsumableSubtype
    {
        None = 0,
        Potion = 1,
        Food = 2,
        BuffFood = 3,
        RepairKit = 4
    }
}