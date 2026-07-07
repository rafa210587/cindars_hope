using System.Collections.Generic;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Generation;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// spec_cave_decor_composition_runtime (CV03), T002 — CaveDecorContextClassifier (critério 14.1).
    ///
    /// Cobre: cada contexto respeita EXATAMENTE sua regra geométrica (CeilingHang = WallTile com sul
    /// walkable; WallHug = walkable com >=1 vizinho WallTile; FloorCluster = walkable com 0 vizinhos
    /// WallTile), estalactite/CeilingHang NUNCA aparece em chão aberto, e o classifier é puro/determinístico
    /// (mesma célula do mesmo level sempre classifica igual — sem RNG, sem estado).
    /// </summary>
    [TestFixture]
    public class CaveDecorContextTests
    {
        /// <summary>
        /// Constrói um level minimalista 5x5 manualmente (sem depender do gerador procedural) para
        /// isolar geometria exata:
        /// <code>
        /// y=4: # # # # #   (parede topo)
        /// y=3: # . . . #   (chão aberto, walkable)
        /// y=2: # . . . #
        /// y=1: # . . . #
        /// y=0: # # # # #   (parede base)
        /// </code>
        /// Sala 3x3 (x=1..3, y=1..3) cercada por parede. (1,3)/(2,3)/(3,3) são chão adjacente à parede
        /// de topo (y=4) => WallHug. (2,4) é WallTile com sul (2,3) walkable => CeilingHang. (2,2) é
        /// chão sem nenhum vizinho de parede => FloorCluster.
        /// </summary>
        private static CaveGeneratedLevel BuildRoomLevel()
        {
            var level = new CaveGeneratedLevel { CaveLevel = 1, Width = 5, Height = 5 };

            for (var x = 0; x <= 4; x++)
            {
                for (var y = 0; y <= 4; y++)
                {
                    var isBorder = x == 0 || x == 4 || y == 0 || y == 4;
                    var cell = new Vector2Int(x, y);
                    if (isBorder)
                    {
                        level.WallTiles.Add(cell);
                    }
                    else
                    {
                        level.WalkableTiles.Add(cell);
                    }
                }
            }

            level.Entrance = new Vector2Int(1, 1);
            level.Exit = new Vector2Int(3, 3);
            return level;
        }

        [Test]
        public void Classify_WallTileWithWalkableSouth_IsCeilingHang()
        {
            var level = BuildRoomLevel();

            var result = CaveDecorContextClassifier.Classify(new Vector2Int(2, 4), level);

            Assert.AreEqual(CaveDecorPlacementContext.CeilingHang, result,
                "Parede com vizinho sul andável deve ser CeilingHang (borda superior de parede visível).");
        }

        [Test]
        public void Classify_WallTileWithoutWalkableSouth_IsNotCeilingHang()
        {
            var level = BuildRoomLevel();

            // (0,0) é canto de parede; seu vizinho sul (0,-1) não existe/não é walkable.
            var result = CaveDecorContextClassifier.Classify(new Vector2Int(0, 0), level);

            Assert.AreNotEqual(CaveDecorPlacementContext.CeilingHang, result,
                "Parede sem vizinho sul andável nunca deve classificar como CeilingHang.");
        }

        [Test]
        public void Classify_WalkableAdjacentToWall_IsWallHug()
        {
            var level = BuildRoomLevel();

            var result = CaveDecorContextClassifier.Classify(new Vector2Int(2, 3), level);

            Assert.AreEqual(CaveDecorPlacementContext.WallHug, result,
                "Célula andável com vizinho WallTile deve ser WallHug.");
        }

        [Test]
        public void Classify_WalkableWithNoAdjacentWall_IsFloorCluster()
        {
            var level = BuildRoomLevel();

            var result = CaveDecorContextClassifier.Classify(new Vector2Int(2, 2), level);

            Assert.AreEqual(CaveDecorPlacementContext.FloorCluster, result,
                "Célula andável sem vizinho WallTile (miolo aberto) deve ser FloorCluster.");
        }

        [Test]
        public void Classify_NeverClassifiesFloorCluster_OnWallTile()
        {
            var level = BuildRoomLevel();

            foreach (var wall in level.WallTiles)
            {
                var result = CaveDecorContextClassifier.Classify(wall, level);
                Assert.AreNotEqual(CaveDecorPlacementContext.FloorCluster, result,
                    $"WallTile {wall} nunca pode classificar como FloorCluster (chão aberto).");
                Assert.AreNotEqual(CaveDecorPlacementContext.WallHug, result,
                    $"WallTile {wall} nunca pode classificar como WallHug (contexto walkable).");
            }
        }

        [Test]
        public void Classify_NeverClassifiesCeilingHang_OnWalkableTile()
        {
            var level = BuildRoomLevel();

            foreach (var walkable in level.WalkableTiles)
            {
                var result = CaveDecorContextClassifier.Classify(walkable, level);
                Assert.AreNotEqual(CaveDecorPlacementContext.CeilingHang, result,
                    $"WalkableTile {walkable} nunca pode classificar como CeilingHang (estalactite não cai no chão).");
            }
        }

        [Test]
        public void Classify_IsDeterministic_SameCellSameLevel_AlwaysSameResult()
        {
            var level = BuildRoomLevel();
            var cell = new Vector2Int(2, 3);

            var first = CaveDecorContextClassifier.Classify(cell, level);
            var second = CaveDecorContextClassifier.Classify(cell, level);

            Assert.AreEqual(first, second, "Classificar a mesma célula do mesmo level deve ser determinístico (stable-run).");
        }

        [Test]
        public void Classify_NullLevel_ReturnsNull_DoesNotThrow()
        {
            CaveDecorPlacementContext? result = null;
            Assert.DoesNotThrow(() => result = CaveDecorContextClassifier.Classify(Vector2Int.zero, null));
            Assert.IsNull(result);
        }

        [Test]
        public void CollectCells_CeilingHang_OnlyReturnsWallTilesWithWalkableSouth()
        {
            var level = BuildRoomLevel();

            var cells = CaveDecorContextClassifier.CollectCells(level, CaveDecorPlacementContext.CeilingHang);

            Assert.IsTrue(cells.Count > 0, "Sala de teste deve ter ao menos 1 célula CeilingHang (topo da sala).");
            foreach (var cell in cells)
            {
                Assert.IsTrue(level.WallTiles.Contains(cell), $"{cell} deveria ser WallTile.");
                Assert.IsTrue(level.WalkableTiles.Contains(cell + Vector2Int.down), $"{cell} deveria ter sul walkable.");
            }
        }

        [Test]
        public void CollectCells_FloorCluster_OnlyReturnsWalkableWithNoWallNeighbor()
        {
            var level = BuildRoomLevel();

            var cells = CaveDecorContextClassifier.CollectCells(level, CaveDecorPlacementContext.FloorCluster);

            // Na sala 3x3, só (2,2) é miolo aberto sem nenhum vizinho de parede.
            Assert.AreEqual(1, cells.Count);
            Assert.AreEqual(new Vector2Int(2, 2), cells[0]);
        }

        [Test]
        public void CollectCells_ReturnsDeterministicOrder()
        {
            var level = BuildRoomLevel();

            var firstRun = CaveDecorContextClassifier.CollectCells(level, CaveDecorPlacementContext.WallHug);
            var secondRun = CaveDecorContextClassifier.CollectCells(level, CaveDecorPlacementContext.WallHug);

            CollectionAssert.AreEqual(firstRun, secondRun, "CollectCells deve retornar a mesma ordem em execuções repetidas (sem HashSet iteration order leak).");
        }
    }
}
