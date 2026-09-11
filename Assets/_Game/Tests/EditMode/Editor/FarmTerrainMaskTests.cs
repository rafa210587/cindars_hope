using CindarsHope.Editor.Art;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Editor
{
    public class FarmTerrainMaskTests
    {
        [Test]
        public void PaintPolygon_ConcaveMask_DoesNotFillBoundingBox()
        {
            var polygon = new[]
            {
                new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 1f),
                new Vector2(1f, 1f), new Vector2(1f, 4f), new Vector2(0f, 4f)
            };

            var cells = WorldTilemapGround.RasterizePolygonCells(polygon, 1f);

            Assert.That(cells.Contains(new Vector3Int(2, 2, 0)), Is.False);
            Assert.That(cells.Contains(new Vector3Int(0, 0, 0)), Is.True);
        }

        [Test]
        public void RasterizeTransitionRing_UsesEightNeighbourExteriorCells()
        {
            var filled = new[] { new Vector3Int(0, 0, 0) };

            var ring = WorldTilemapGround.RasterizeTransitionRingCells(filled);

            Assert.That(ring.Count, Is.EqualTo(8));
            Assert.That(ring.Contains(new Vector3Int(-1, -1, 0)), Is.True);
            Assert.That(ring.Contains(new Vector3Int(1, 1, 0)), Is.True);
        }

        [Test]
        public void RasterizePolygon_SameInput_ProducesSameCells()
        {
            var polygon = new[]
            {
                new Vector2(-2f, -1f), new Vector2(2f, -1f), new Vector2(2f, 2f), new Vector2(-2f, 2f)
            };

            var first = WorldTilemapGround.RasterizePolygonCells(polygon, 1f);
            var second = WorldTilemapGround.RasterizePolygonCells(polygon, 1f);

            CollectionAssert.AreEqual(first, second);
        }
    }
}
