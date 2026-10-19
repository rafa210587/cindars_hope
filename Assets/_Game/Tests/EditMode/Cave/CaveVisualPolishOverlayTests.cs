using CindarsHope.Cave.Art;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// spec_cave_visual_polish_runtime (CV04), T003/T004 — cobre as funções PURAS extraídas de
    /// CaveTileMaterializer (sem instanciar GameObjects): a escolha determinística de peça de overlay de
    /// borda de rocha (TryResolveWallEdgeKind) e as decisões de hit determinístico de GroundScatter/
    /// WallSurface (ShouldPlaceGroundScatter/ShouldPlaceWallSurfaceDecor). Critério 14.1/14.2/14.3: mesma
    /// célula do mesmo level/seed sempre produz o mesmo resultado (stable-run); nunca usa
    /// Random/GetHashCode.
    /// </summary>
    [TestFixture]
    public class CaveVisualPolishOverlayTests
    {
        // ── TryResolveWallEdgeKind (T003) ───────────────────────────────────────────────────────

        private static CaveGeneratedLevel BuildMinimalWallLevel(Vector2Int wallCell, bool south, bool east, bool west)
        {
            var level = new CaveGeneratedLevel { CaveLevel = 1, Width = 6, Height = 6 };
            level.WallTiles.Add(wallCell);
            if (south) level.WalkableTiles.Add(wallCell + Vector2Int.down);
            if (east) level.WalkableTiles.Add(wallCell + Vector2Int.right);
            if (west) level.WalkableTiles.Add(wallCell + Vector2Int.left);
            return level;
        }

        [Test]
        public void TryResolveWallEdgeKind_SouthOnly_ReturnsTop_NoMirror()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: true, east: false, west: false);

            var resolved = CaveTileMaterializer.TryResolveWallEdgeKind(new Vector2Int(2, 2), level, out var kind, out var mirrorX);

            Assert.IsTrue(resolved);
            Assert.AreEqual(CaveWallEdgeKind.Top, kind);
            Assert.IsFalse(mirrorX);
        }

        [Test]
        public void TryResolveWallEdgeKind_EastOnly_ReturnsSide_NoMirror()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: false, east: true, west: false);

            var resolved = CaveTileMaterializer.TryResolveWallEdgeKind(new Vector2Int(2, 2), level, out var kind, out var mirrorX);

            Assert.IsTrue(resolved);
            Assert.AreEqual(CaveWallEdgeKind.Side, kind);
            Assert.IsFalse(mirrorX, "Lado leste não deve espelhar (orientação original da peça 'side').");
        }

        [Test]
        public void TryResolveWallEdgeKind_WestOnly_ReturnsSide_Mirrored()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: false, east: false, west: true);

            var resolved = CaveTileMaterializer.TryResolveWallEdgeKind(new Vector2Int(2, 2), level, out var kind, out var mirrorX);

            Assert.IsTrue(resolved);
            Assert.AreEqual(CaveWallEdgeKind.Side, kind);
            Assert.IsTrue(mirrorX, "Lado oeste deve espelhar em X a mesma peça 'side' usada para o leste.");
        }

        [Test]
        public void TryResolveWallEdgeKind_SouthAndEast_ReturnsCornerA()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: true, east: true, west: false);

            var resolved = CaveTileMaterializer.TryResolveWallEdgeKind(new Vector2Int(2, 2), level, out var kind, out _);

            Assert.IsTrue(resolved);
            Assert.AreEqual(CaveWallEdgeKind.CornerA, kind);
        }

        [Test]
        public void TryResolveWallEdgeKind_SouthAndWest_ReturnsCornerB()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: true, east: false, west: true);

            var resolved = CaveTileMaterializer.TryResolveWallEdgeKind(new Vector2Int(2, 2), level, out var kind, out _);

            Assert.IsTrue(resolved);
            Assert.AreEqual(CaveWallEdgeKind.CornerB, kind);
        }

        [Test]
        public void TryResolveWallEdgeKind_NoRelevantWalkableNeighbor_ReturnsFalse()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: false, east: false, west: false);

            var resolved = CaveTileMaterializer.TryResolveWallEdgeKind(new Vector2Int(2, 2), level, out _, out _);

            Assert.IsFalse(resolved, "Miolo de parede (sem vizinho S/E/O andável) não deve gerar overlay de borda.");
        }

        [Test]
        public void TryResolveWallEdgeKind_IsDeterministic_SameCellSameLevel_AlwaysSameResult()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: true, east: true, west: false);

            var first = CaveTileMaterializer.TryResolveWallEdgeKind(new Vector2Int(2, 2), level, out var kindFirst, out var mirrorFirst);
            var second = CaveTileMaterializer.TryResolveWallEdgeKind(new Vector2Int(2, 2), level, out var kindSecond, out var mirrorSecond);

            Assert.AreEqual(first, second);
            Assert.AreEqual(kindFirst, kindSecond);
            Assert.AreEqual(mirrorFirst, mirrorSecond);
        }

        [Test]
        public void TryResolveWallEdgeKind_NullLevel_ReturnsFalse_DoesNotThrow()
        {
            var result = true;
            Assert.DoesNotThrow(() => result = CaveTileMaterializer.TryResolveWallEdgeKind(Vector2Int.zero, null, out _, out _));
            Assert.IsFalse(result);
        }

        // ── ShouldPlaceGroundScatter (T004) ─────────────────────────────────────────────────────

        private static CaveGeneratedLevel BuildOpenFloorLevel()
        {
            // Sala 5x5 aberta (sem parede) para garantir que a célula central seja FloorCluster
            // (nenhum vizinho de WallTiles) — GroundScatter-elegível.
            var level = new CaveGeneratedLevel { CaveLevel = 3, Width = 9, Height = 9 };
            for (var x = 1; x <= 7; x++)
            {
                for (var y = 1; y <= 7; y++)
                {
                    level.WalkableTiles.Add(new Vector2Int(x, y));
                }
            }
            return level;
        }

        [Test]
        public void ShouldPlaceGroundScatter_ZeroDensity_AlwaysFalse()
        {
            var level = BuildOpenFloorLevel();
            var cell = new Vector2Int(4, 4);

            Assert.IsFalse(CaveTileMaterializer.ShouldPlaceGroundScatter(cell, level, "world", "run", 0f));
        }

        [Test]
        public void ShouldPlaceGroundScatter_NonEligibleCell_AlwaysFalseEvenWithFullDensity()
        {
            var level = BuildOpenFloorLevel();
            level.WallTiles.Add(new Vector2Int(3, 4)); // vizinho de parede -> (4,4) vira WallHug, não FloorCluster.

            Assert.IsFalse(CaveTileMaterializer.ShouldPlaceGroundScatter(new Vector2Int(4, 4), level, "world", "run", 1f),
                "Célula WallHug (adjacente a parede) nunca é GroundScatter-elegível, mesmo com densidade 1.0.");
        }

        [Test]
        public void ShouldPlaceGroundScatter_IsDeterministic_SameInputs_AlwaysSameResult()
        {
            var level = BuildOpenFloorLevel();
            var cell = new Vector2Int(4, 4);

            var first = CaveTileMaterializer.ShouldPlaceGroundScatter(cell, level, "world_1", "run_1", 0.35f);
            var second = CaveTileMaterializer.ShouldPlaceGroundScatter(cell, level, "world_1", "run_1", 0.35f);

            Assert.AreEqual(first, second, "Mesmo (worldSeed, runSeed, level, célula, densidade) deve sempre produzir o mesmo resultado (stable-run).");
        }

        // ── ShouldPlaceWallSurfaceDecor (T004) ──────────────────────────────────────────────────

        [Test]
        public void ShouldPlaceWallSurfaceDecor_ZeroChance_AlwaysFalse()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: true, east: false, west: false);

            Assert.IsFalse(CaveTileMaterializer.ShouldPlaceWallSurfaceDecor(new Vector2Int(2, 2), level, "world", "run", 0f));
        }

        [Test]
        public void ShouldPlaceWallSurfaceDecor_NonWallSurfaceCell_AlwaysFalseEvenWithFullChance()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: false, east: false, west: false);

            Assert.IsFalse(CaveTileMaterializer.ShouldPlaceWallSurfaceDecor(new Vector2Int(2, 2), level, "world", "run", 1f),
                "Célula de parede sem nenhum vizinho andável (miolo) nunca é WallSurface-elegível, mesmo com chance 1.0.");
        }

        [Test]
        public void ShouldPlaceWallSurfaceDecor_IsDeterministic_SameInputs_AlwaysSameResult()
        {
            var level = BuildMinimalWallLevel(new Vector2Int(2, 2), south: true, east: false, west: false);

            var first = CaveTileMaterializer.ShouldPlaceWallSurfaceDecor(new Vector2Int(2, 2), level, "world_1", "run_1", 0.12f);
            var second = CaveTileMaterializer.ShouldPlaceWallSurfaceDecor(new Vector2Int(2, 2), level, "world_1", "run_1", 0.12f);

            Assert.AreEqual(first, second, "Mesmo (worldSeed, runSeed, level, célula, chance) deve sempre produzir o mesmo resultado (stable-run).");
        }
    }
}
