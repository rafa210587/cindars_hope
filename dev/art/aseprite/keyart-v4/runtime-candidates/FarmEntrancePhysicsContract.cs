using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm.Scene
{
    /// <summary>Farm-side entrance feet, solid bases and interaction regions in world units.</summary>
    public static class FarmEntrancePhysicsContract
    {
        // cave_entrance.png:531x435, alpha bounds(8,8)..(522,426), bottom-center pivot.
        // Source measurements below use bottom-left coordinates. Keep these synchronized with
        // the existing6.75u opaque-height visual; no invisible full rectangle spans the doorway.
        public const float CaveOpaqueHeight = 6.75f;
        public const float CaveOpaqueHeightPixels = 419f;
        public const float CaveCanvasHeightPixels = 435f;
        public const float CaveWorldUnitsPerPixel = CaveOpaqueHeight / CaveOpaqueHeightPixels;
        public static readonly Vector2 CaveSourcePivot = new Vector2(265.5f, 0f);

        public static Vector2 CaveSourceToWorld(Vector2 sourcePixel)
            => new Vector2(FarmLevel1LayoutContract.CaveEntranceX, FarmLevel1LayoutContract.CaveEntranceY)
               + (sourcePixel - CaveSourcePivot) * CaveWorldUnitsPerPixel;

        // Basal volumes include the adjacent ore/rock masses. The shallow pixels below y43 are
        // contact shadow/foreground chips, not an additional full-width wall across the threshold.
        // The rear extends into the existing cliff band, closing the route behind either jamb.
        public static readonly FarmSolidRect CaveWestBase = CaveSourceRect("CaveWestBase", 12f, 43f, 200f, 205f);
        public static readonly FarmSolidRect CaveEastBase = CaveSourceRect("CaveEastBase", 337f, 43f, 515f, 205f);
        public static IReadOnlyList<FarmSolidRect> CaveSideBases { get; } =
            System.Array.AsReadOnly(new[] { CaveWestBase, CaveEastBase });

        public static FarmSolidRect CaveInteractionTrigger
        {
            get
            {
                var minimum = new Vector2(CaveWestBase.Bounds.xMax, FarmLevel1LayoutContract.CaveMouthMinY);
                var maximum = new Vector2(CaveEastBase.Bounds.xMin, CaveSourceToWorld(new Vector2(337f, 115f)).y);
                return new FarmSolidRect("CaveInteraction", (minimum + maximum) * 0.5f, maximum - minimum);
            }
        }

        // Preserve the old effective1.8x1.8 trigger, without coupling it to CheckpointPortal art scale.
        public static readonly FarmSolidRect TownInteractionTrigger = new FarmSolidRect("TownInteraction",
            new Vector2(FarmLevel1LayoutContract.CityExitX, FarmLevel1LayoutContract.CityExitY),
            new Vector2(1.8f, 1.8f));

        // Visual reference only: GetSolidBand already owns this physical boundary. Never add a second body.
        public static FarmSolidRect TownGateVisualRun
        {
            get
            {
                var boundary = FarmEnclosedValleyBoundaryContract.InnerBoundary;
                var a = boundary[boundary.Count - 1];
                var b = boundary[0];
                var depth = FarmEnclosedValleyBoundaryContract.TownGateDepth;
                return new FarmSolidRect("TownGate", (a + b) * 0.5f + Vector2.right * (depth * 0.5f),
                    new Vector2(depth, Mathf.Abs(a.y - b.y)));
            }
        }

        private static FarmSolidRect CaveSourceRect(string id, float left, float bottom, float right, float top)
        {
            var minimum = CaveSourceToWorld(new Vector2(left, bottom));
            var maximum = CaveSourceToWorld(new Vector2(right, top));
            return new FarmSolidRect(id, (minimum + maximum) * 0.5f, maximum - minimum);
        }
    }
}
