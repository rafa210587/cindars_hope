using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm.Scene
{
    public readonly struct FarmSolidRect
    {
        public FarmSolidRect(string id, Vector2 center, Vector2 size)
        {
            Id = id;
            Center = center;
            Size = size;
        }

        public string Id { get; }
        public Vector2 Center { get; }
        public Vector2 Size { get; }
        public Rect Bounds => new Rect(Center - Size * 0.5f, Size);
    }

    /// <summary>Physical fence runs shared by scene art and collision, with open settlement approaches.</summary>
    public static class FarmSettlementPhysicsContract
    {
        public static readonly Vector2 OrchardGate = new Vector2(-16.625f, -1.75f);
        public static readonly Vector2 PastureGate = new Vector2(-19.5f, -8.625f);
        public static readonly Vector2 WellClearingApproach = new Vector2(-4.8f, 11.7f);

        public const float GreenhouseSideBaseThickness = 0.25f;
        public const float GreenhouseSideBaseInset = 0.35f;
        public const float GreenhouseSouthBaseInset = 0.25f;
        public const float GreenhouseNorthBaseInset = 0.35f;

        public static Rect GreenhouseInteriorBounds
        {
            get
            {
                if (!FarmSceneSpatialContract.TryGet(FarmSceneSpatialContract.Greenhouse, out var footprint))
                    throw new System.InvalidOperationException("Greenhouse footprint is missing.");
                var bounds = footprint.Bounds;
                var side = GreenhouseSideBaseInset + GreenhouseSideBaseThickness * 0.5f;
                return Rect.MinMaxRect(bounds.min.x + side, bounds.min.y + GreenhouseSouthBaseInset,
                    bounds.max.x - side, bounds.max.y - GreenhouseNorthBaseInset - GreenhouseSideBaseThickness * 0.5f);
            }
        }

        public static readonly FarmSolidRect FountainBasin = Segment("FountainBasin", -28f, 4.25f, 4.6f, 1.6f);
        public static readonly FarmSolidRect CentralWell = Segment("CentralWell", -7.44f, 12.84f, 1.8f, 1.25f);

        public static readonly FarmSolidRect ShippingCounter = Segment("ShippingCounter", 9.6f, 6.5f, 2.2f, 1f);
        public static readonly FarmSolidRect SalesCounter = Segment("SalesCounter", 13.5f, 6.45f, 2.6f, 0.8f);
        public static readonly FarmSolidRect EvolutionBoard = Segment("EvolutionBoard", 19.2f, 6.35f, 2.2f, 0.65f);
        public static readonly FarmSolidRect PottingBench = Segment("PottingBench", 23.5f, 6.55f, 2.5f, 1f);
        public static IReadOnlyList<FarmSolidRect> ServiceBodies { get; } = new[]
        {
            ShippingCounter, SalesCounter, EvolutionBoard, PottingBench
        };

        private static readonly FarmSolidRect[] FenceSegments =
        {
            Segment("OrchardNorth", -16.625f, 9.2f, 11f, 0.28f),
            Segment("OrchardWest", -22.5f, 3.725f, 0.28f, 10.5f),
            Segment("OrchardEast", -10.75f, 3.725f, 0.28f, 10.5f),
            Segment("OrchardSouthWest", -19.975f, -1.75f, 4.15f, 0.28f),
            Segment("OrchardSouthEast", -13.275f, -1.75f, 4.15f, 0.28f),
            Segment("PastureNorth", -28.45f, -4.25f, 17f, 0.28f),
            Segment("PastureSouth", -28.45f, -13f, 17f, 0.28f),
            Segment("PastureWest", -37.4f, -8.625f, 0.28f, 7.5f),
            Segment("PastureEastNorth", -19.5f, -6.1f, 0.28f, 2.4f),
            Segment("PastureEastSouth", -19.5f, -11.15f, 0.28f, 2.4f),
            Segment("WellNorth", -4.2f, 14.3f, 2.7f, 0.28f),
            Segment("WellEast", -1.8f, 13.1f, 0.28f, 1.05f)
        };

        public static IReadOnlyList<FarmSolidRect> AllFenceSegments => FenceSegments;

        private static FarmSolidRect Segment(string id, float x, float y, float width, float height)
            => new FarmSolidRect(id, new Vector2(x, y), new Vector2(width, height));
    }
}
