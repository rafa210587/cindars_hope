using UnityEngine;

namespace CindarsHope.Farm.Scene
{
    /// <summary>Deterministic geometry predicates used by editor-only navigation validation.</summary>
    public static class FarmSceneNavigationRaster
    {
        public static bool IsBlocked(Vector2 point)
        {
            for (var i = 0; i < FarmSceneSpatialContract.All.Count; i++)
            {
                var footprint = FarmSceneSpatialContract.All[i];
                if (!FarmSceneNavigationPolicy.RequiresSolidCollider(footprint)) continue;
                if (footprint.Id == FarmSceneSpatialContract.River && IsInsideBridgeCorridor(point)) continue;
                if (footprint.Id == FarmSceneSpatialContract.River && point.y <= FarmLevel1LayoutContract.BridgeMinY)
                {
                    // Below the bridge, physical support includes the measured confluence rock.
                    // Preserve the existing bridge/upper-river predicates and visual water footprint.
                    if (ContainsPolygon(FarmSceneSpatialContract.RiverBelowBridgeCollisionPath, point)) return true;
                    continue;
                }
                if (footprint.Id == FarmSceneSpatialContract.Lake)
                {
                    if (ContainsPolygon(FarmSceneSpatialContract.LakeCollisionPath, point)) return true;
                    continue;
                }
                if (Contains(footprint, point)) return true;
            }

            return false;
        }

        public static bool Contains(FarmSceneFootprint footprint, Vector2 point)
            => ContainsPolygon(footprint.Polygon, point);

        private static bool ContainsPolygon(System.Collections.Generic.IReadOnlyList<Vector2> polygon, Vector2 point)
        {
            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if ((a.y > point.y) == (b.y > point.y)) continue;
                var intersectionX = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;
                if (point.x < intersectionX) inside = !inside;
            }

            return inside;
        }

        private static bool IsInsideBridgeCorridor(Vector2 point)
        {
            if (!FarmSceneSpatialContract.TryGet(FarmSceneSpatialContract.Bridge, out var bridge)) return false;
            return bridge.Bounds.Contains(point);
        }
    }
}
