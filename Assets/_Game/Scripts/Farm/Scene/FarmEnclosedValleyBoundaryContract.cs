using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm.Scene
{
    /// <summary>The clearing edge shared by permanent scenery, collision and traversal probes.</summary>
    public static class FarmEnclosedValleyBoundaryContract
    {
        public const float ExteriorDepth = 7f;
        public const float TownGateDepth = 0.35f;
        public static IReadOnlyList<Vector2> InnerBoundary { get; } = System.Array.AsReadOnly(new[]
        {
            new Vector2(36f, 1.5f), new Vector2(34f, -3f), new Vector2(35f, -9f),
            new Vector2(32f, -17f), new Vector2(28f, -22f), new Vector2(20f, -24f),
            new Vector2(8f, -23f), new Vector2(-4f, -25f), new Vector2(-18f, -24f),
            new Vector2(-28f, -23f), new Vector2(-38f, -18f), new Vector2(-39.5f, -8f),
            new Vector2(-35f, 3f), new Vector2(-36f, 12f), new Vector2(-33f, 18f),
            new Vector2(-29f, 18.5f), new Vector2(-26f, 22.4f), new Vector2(-22.75f, 23.5f),
            new Vector2(-18f, 22.4f), new Vector2(-8f, 18f), new Vector2(0f, 16.7f),
            new Vector2(10f, 19.2f), new Vector2(20f, 20f), new Vector2(29f, 23f),
            new Vector2(34f, 19f), new Vector2(35f, 12f), new Vector2(34f, 7f),
            new Vector2(36f, 4.5f)
        });

        public static bool IsNorthEscarpmentSegment(int segmentIndex) => segmentIndex >= 14 && segmentIndex < 24;

        public static bool IsTownGateSegment(int segmentIndex) => segmentIndex == InnerBoundary.Count - 1;

        public static Vector2[] GetSolidBand(int segmentIndex)
        {
            var a = InnerBoundary[segmentIndex];
            var b = InnerBoundary[(segmentIndex + 1) % InnerBoundary.Count];
            // Shared outer vertices close every corner. This is scenery mass, not a thin hidden wall.
            return new[] { a, b, OuterVertex((segmentIndex + 1) % InnerBoundary.Count), OuterVertex(segmentIndex) };
        }

        private static Vector2 OuterVertex(int index)
        {
            var point = InnerBoundary[index];
            // The terminal fence is a shallow visible obstacle; neighboring forest bands taper
            // to these same vertices so no gap opens between the gate and the natural boundary.
            return index == 0 || index == InnerBoundary.Count - 1
                ? point + Vector2.right * TownGateDepth
                : point + point.normalized * ExteriorDepth;
        }

        public static bool ContainsClearing(Vector2 point)
        {
            var inside = false;
            for (int i = 0, j = InnerBoundary.Count - 1; i < InnerBoundary.Count; j = i++)
            {
                var a = InnerBoundary[i];
                var b = InnerBoundary[j];
                if ((a.y > point.y) != (b.y > point.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            return inside;
        }
    }
}
