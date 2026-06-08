namespace CindarsHope.Farm.Expansion
{
    public class FarmExpansionZone
    {
        public string ZoneId { get; set; }
        public string DisplayName { get; set; }
        public FarmZoneType ZoneType { get; set; }
        public FarmPropertyLevel RequiredLevel { get; set; } = FarmPropertyLevel.Level1_InicialPlot;
        public int BoundsMinX { get; set; }
        public int BoundsMinY { get; set; }
        public int BoundsMaxX { get; set; }
        public int BoundsMaxY { get; set; }
        public bool IsUnlocked { get; set; } = false;
        public bool AllowBuilding => ZoneType == FarmZoneType.Free && IsUnlocked;
        public bool IsLoreReserved => ZoneType == FarmZoneType.Lore || ZoneType == FarmZoneType.Endgame;
        public bool IsMustClearAccess => ZoneType == FarmZoneType.Path;

        public bool ContainsTile(int tileX, int tileY) =>
            tileX >= BoundsMinX && tileX <= BoundsMaxX &&
            tileY >= BoundsMinY && tileY <= BoundsMaxY;
    }

    public class FarmExpansionState
    {
        public FarmPropertyLevel CurrentLevel { get; set; } = FarmPropertyLevel.Level1_InicialPlot;
        public int LevelAsInt => (int)CurrentLevel;
    }
}
