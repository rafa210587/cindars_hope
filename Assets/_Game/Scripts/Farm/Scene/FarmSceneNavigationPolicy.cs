namespace CindarsHope.Farm.Scene
{
    /// <summary>Pure collision and reachability rules shared by the scene generator and editor validation.</summary>
    public static class FarmSceneNavigationPolicy
    {
        public static bool IsExpectedWalkable(FarmSpatialUse use)
        {
            return use == FarmSpatialUse.Walkable || use == FarmSpatialUse.CropField ||
                   use == FarmSpatialUse.TriggerOnly || use == FarmSpatialUse.Spawn;
        }

        public static bool RequiresSolidCollider(FarmSceneFootprint footprint)
        {
            return footprint.BlocksMovement && !IsExpectedWalkable(footprint.Use);
        }

        public static bool RequiresReachableApproach(FarmSceneFootprint footprint)
        {
            return footprint.Use == FarmSpatialUse.Building || footprint.Use == FarmSpatialUse.Water ||
                   footprint.Use == FarmSpatialUse.TriggerOnly;
        }

        public static bool RequiresPolygonCollider(FarmSceneFootprint footprint)
        {
            return footprint.Use == FarmSpatialUse.Water || footprint.Use == FarmSpatialUse.Solid;
        }

        public static bool RequiresMaterializedTerrainCollider(FarmSceneFootprint footprint)
        {
            return RequiresSolidCollider(footprint) &&
                   (footprint.Use == FarmSpatialUse.Water || footprint.Use == FarmSpatialUse.Solid);
        }

        public static bool RequiresExistingSolidCollider(FarmSceneFootprint footprint)
        {
            return footprint.BlocksMovement && footprint.Use == FarmSpatialUse.Building;
        }
    }
}
