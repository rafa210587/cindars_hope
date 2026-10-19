using System.Collections.Generic;
using CindarsHope.Farm;
using CindarsHope.Farm.Scene;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    public class FarmSceneSpatialContractTests
    {
        [Test]
        public void AllFootprints_HaveUniqueIdsAndAreInBounds()
        {
            var ids = new HashSet<string>();
            Assert.That(FarmSceneSpatialContract.All.Count, Is.EqualTo(11));
            foreach (var footprint in FarmSceneSpatialContract.All)
            {
                Assert.That(ids.Add(footprint.Id), Is.True, footprint.Id);
                Assert.That(footprint.Polygon.Count, Is.GreaterThanOrEqualTo(3), footprint.Id);
                foreach (var point in footprint.Polygon)
                {
                    Assert.That(point.x, Is.InRange(FarmLevel1LayoutContract.MinX, FarmLevel1LayoutContract.MaxX), footprint.Id);
                    Assert.That(point.y, Is.InRange(FarmLevel1LayoutContract.MinY, FarmLevel1LayoutContract.MaxY), footprint.Id);
                }
            }
        }

        [Test]
        public void BlockingFootprints_AreNonArable()
        {
            var zones = new FarmNonArableZones();
            var grid = new FarmTileGrid(zones);
            grid.SetBounds(-4, -3, 72, 50, FarmLevel1LayoutContract.MinX, FarmLevel1LayoutContract.MinY);

            foreach (var footprint in FarmSceneSpatialContract.All)
            {
                if (!footprint.BlocksTilling) continue;
                var bounds = footprint.Bounds;
                var tileX = grid.WorldToTile(bounds.min.x, bounds.min.y).x;
                var tileY = grid.WorldToTile(bounds.min.x, bounds.min.y).y;
                zones.RegisterBlockedRect(tileX, tileY, Mathf.CeilToInt(bounds.size.x), Mathf.CeilToInt(bounds.size.y));
                Assert.That(grid.IsTillable(tileX, tileY), Is.False, footprint.Id);
            }

            Assert.That(FarmSceneSpatialContract.TryGet(FarmSceneSpatialContract.Bridge, out var bridge), Is.True);
            Assert.That(bridge.BlocksMovement, Is.False);
            Assert.That(FarmSceneNavigationRaster.IsBlocked(bridge.Bounds.center), Is.False);

        }


        [Test]
        public void KeyartHomestead_IsWestOfRiver_AndGreenhouseIsRightOfHouse()
        {
            Assert.That(FarmSceneSpatialContract.TryGet(FarmSceneSpatialContract.House, out var house), Is.True);
            Assert.That(FarmSceneSpatialContract.TryGet(FarmSceneSpatialContract.Greenhouse, out var greenhouse), Is.True);
            Assert.That(house.Bounds.max.x, Is.LessThan(greenhouse.Bounds.min.x));
            Assert.That(FarmSceneNavigationRaster.IsBlocked(new Vector2(FarmLevel1LayoutContract.BridgeCenterX, FarmLevel1LayoutContract.BridgeCenterY + 5f)), Is.True);
            Assert.That(FarmSceneNavigationRaster.IsBlocked(new Vector2(FarmLevel1LayoutContract.DefaultSpawnX, FarmLevel1LayoutContract.DefaultSpawnY)), Is.False, "Spawn approach must remain clear.");
        }

        [Test]
        public void UsesHaveExplicitBlockingClassification()
        {
            foreach (var footprint in FarmSceneSpatialContract.All)
            {
                if (footprint.Use == FarmSpatialUse.Water || footprint.Use == FarmSpatialUse.Solid ||
                    footprint.Use == FarmSpatialUse.Building)
                {
                    Assert.That(footprint.BlocksTilling, Is.True, footprint.Id);
                }
            }
        }

        [Test]
        public void SettlementFences_BlockTheirRunsAndLeaveAuthoredGatesOpen()
        {
            Assert.That(FarmSettlementPhysicsContract.AllFenceSegments.Count, Is.EqualTo(12));
            foreach (var segment in FarmSettlementPhysicsContract.AllFenceSegments)
            {
                Assert.That(segment.Size.x, Is.GreaterThan(0f), segment.Id);
                Assert.That(segment.Size.y, Is.GreaterThan(0f), segment.Id);
                Assert.That(segment.Bounds.Contains(FarmSettlementPhysicsContract.OrchardGate), Is.False, segment.Id);
                Assert.That(segment.Bounds.Contains(FarmSettlementPhysicsContract.PastureGate), Is.False, segment.Id);
                Assert.That(segment.Bounds.Contains(FarmSettlementPhysicsContract.WellClearingApproach), Is.False, segment.Id);
            }
        }
    }
}
