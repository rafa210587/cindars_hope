using CindarsHope.Farm;
using CindarsHope.Farm.Scene;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    public class FarmEnclosedValleyBoundaryTests
    {
        [Test]
        public void ExpansionPreservesExistingSoilIdentityAndSupportsNegativeTiles()
        {
            var grid = new FarmTileGrid(new FarmNonArableZones());
            grid.SetBounds(0, 0, 64, 44, -32f, -22f);
            Assert.That(grid.TryRegisterTilledTile(32, 22, out var soil), Is.True);
            var previousPosition = grid.TileToWorldCenter(32, 22);
            grid.SetBounds(FarmLevel1LayoutContract.GridOriginTileX, FarmLevel1LayoutContract.GridOriginTileY,
                (int)FarmLevel1LayoutContract.Level1WidthTiles, (int)FarmLevel1LayoutContract.Level1HeightTiles,
                FarmLevel1LayoutContract.MinX, FarmLevel1LayoutContract.MinY);
            Assert.That(grid.TileToWorldCenter(32, 22), Is.EqualTo(previousPosition));
            Assert.That(grid.WorldToTile(0.5f, 0.5f), Is.EqualTo(new Vector2Int(32, 22)));
            Assert.That(grid.IsTilled(32, 22), Is.True);
            Assert.That(grid.TryRegisterTilledTile(32, 22, out var existing), Is.False);
            Assert.That(existing, Is.SameAs(soil));
            var westernExtension = grid.TileToWorldCenter(-4, -3);
            Assert.That(westernExtension, Is.EqualTo(new Vector2(-35.5f, -24.5f)));
            Assert.That(grid.WorldToTile(westernExtension.x, westernExtension.y), Is.EqualTo(new Vector2Int(-4, -3)));
            var newWesternEdge = grid.TileToWorldCenter(-8, -3);
            Assert.That(newWesternEdge, Is.EqualTo(new Vector2(-39.5f, -24.5f)));
            Assert.That(grid.TryRegisterTilledTile(-8, -3, out var westernSoil), Is.True);
            Assert.That(grid.WorldToTile(newWesternEdge.x, newWesternEdge.y), Is.EqualTo(new Vector2Int(-8, -3)));
            Assert.That(grid.TileToWorldCenter(67, 46), Is.EqualTo(new Vector2(35.5f, 24.5f)), "Existing eastern edge stays fixed.");
        }

        [Test]
        public void ClearingContainsArrivalsButExcludesBeyondEveryMapSide()
        {
            Assert.That(FarmEnclosedValleyBoundaryContract.ContainsClearing(new Vector2(
                FarmLevel1LayoutContract.DefaultSpawnX, FarmLevel1LayoutContract.DefaultSpawnY)), Is.True);
            Assert.That(FarmEnclosedValleyBoundaryContract.ContainsClearing(new Vector2(
                FarmLevel1LayoutContract.CityExitX, FarmLevel1LayoutContract.CityExitY)), Is.True);
            Assert.That(FarmEnclosedValleyBoundaryContract.ContainsClearing(new Vector2(
                FarmLevel1LayoutContract.SpawnFromCaveX, FarmLevel1LayoutContract.SpawnFromCaveY)), Is.True);
            foreach (var point in new[] { new Vector2(-41f, 0f), new Vector2(37f, 0f),
                new Vector2(0f, -26f), new Vector2(0f, 26f) })
                Assert.That(FarmEnclosedValleyBoundaryContract.ContainsClearing(point), Is.False, point.ToString());
        }

        [Test]
        public void SolidBandsShareTheirEdgesWithoutOpenSeams()
        {
            var boundary = FarmEnclosedValleyBoundaryContract.InnerBoundary;
            for (int i = 0; i < boundary.Count; i++)
            {
                var band = FarmEnclosedValleyBoundaryContract.GetSolidBand(i);
                var next = FarmEnclosedValleyBoundaryContract.GetSolidBand((i + 1) % boundary.Count);
                Assert.That(band, Does.Contain(boundary[i]), "start " + i);
                Assert.That(band, Does.Contain(boundary[(i + 1) % boundary.Count]), "end " + i);
                int shared = 0;
                foreach (var vertex in band)
                    foreach (var other in next)
                        if (Vector2.Distance(vertex, other) < 0.0001f) shared++;
                Assert.That(shared, Is.GreaterThanOrEqualTo(2), "Physical bands must meet along an edge: " + i);
            }
        }

        [Test]
        public void TownTransitionHasOneVisibleGateRatherThanAnUnboundedExit()
        {
            int gates = 0;
            var boundary = FarmEnclosedValleyBoundaryContract.InnerBoundary;
            for (int i = 0; i < boundary.Count; i++)
            {
                if (!FarmEnclosedValleyBoundaryContract.IsTownGateSegment(i)) continue;
                gates++;
                var a = boundary[i];
                var b = boundary[(i + 1) % boundary.Count];
                Assert.That(Mathf.Min(a.y, b.y), Is.LessThan(FarmLevel1LayoutContract.CityExitY));
                Assert.That(Mathf.Max(a.y, b.y), Is.GreaterThan(FarmLevel1LayoutContract.CityExitY));
                Assert.That(a.x, Is.GreaterThan(FarmLevel1LayoutContract.CityExitX));
            }
            Assert.That(gates, Is.EqualTo(1));
        }
    }
}
