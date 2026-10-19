namespace CindarsHope.Farm.Buildings
{
    /// <summary>
    /// Building placement validation states.
    /// </summary>
    public enum PlacementState
    {
        /// <summary>
        /// Placement is valid; building can be placed here.
        /// </summary>
        ValidPlacement = 0,

        /// <summary>
        /// Another building or object blocks this position.
        /// </summary>
        BlockedByObject = 1,

        /// <summary>
        /// Terrain is not valid for this building (wrong terrain tag).
        /// </summary>
        BlockedByTerrain = 2,

        /// <summary>
        /// Position is in a locked or restricted zone.
        /// </summary>
        BlockedByZone = 3,

        /// <summary>
        /// Building blocks player path (entrance is not accessible).
        /// </summary>
        BlockedByPath = 4,

        /// <summary>
        /// Position overlaps with a lore anchor (Fonte, Raiz, etc).
        /// </summary>
        BlockedByLoreAnchor = 5,

        /// <summary>
        /// Insufficient resources to build here.
        /// </summary>
        InsufficientResources = 6,

        /// <summary>
        /// Farm level is too low; building requires higher farm level.
        /// </summary>
        RequiresUpgrade = 7
    }
}
