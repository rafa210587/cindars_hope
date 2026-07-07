using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// spec_cave_decor_composition_runtime (CV03) — classificador PURO e DETERMINÍSTICO de contexto de
    /// célula (seção 20, Fase 1). Deriva o <see cref="CaveDecorPlacementContext"/> de uma célula
    /// EXCLUSIVAMENTE a partir de <see cref="CaveGeneratedLevel.WalkableTiles"/> e
    /// <see cref="CaveGeneratedLevel.WallTiles"/> — sem RNG, sem estado, sem I/O. Reclassificar a mesma
    /// célula do mesmo <see cref="CaveGeneratedLevel"/> sempre produz o mesmo contexto (stable-run /
    /// ADR-0005): o contexto é DERIVÁVEL da posição, por isso não precisa ser persistido no save.
    ///
    /// Regras (seção 11 da spec):
    /// - CeilingHang: célula é WallTile E o vizinho SUL (y-1) é walkable (borda superior de parede visível).
    /// - WallHug: célula é walkable E tem >=1 vizinho ortogonal em WallTiles.
    /// - FloorCluster: célula é walkable E 0 vizinhos ortogonais em WallTiles (miolo aberto).
    /// - Uma célula que não é walkable nem satisfaz CeilingHang não tem contexto de decor (null).
    /// </summary>
    public static class CaveDecorContextClassifier
    {
        private static readonly Vector2Int[] OrthogonalDirs =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        /// <summary>Classifica UMA célula. Retorna null quando a célula não é elegível para nenhum
        /// contexto de decor (ex.: WallTile sem vizinho sul andável — parede "interna"/de fundo).</summary>
        public static CaveDecorPlacementContext? Classify(Vector2Int cell, CaveGeneratedLevel level)
        {
            if (level == null || level.WalkableTiles == null || level.WallTiles == null)
            {
                return null;
            }

            var isWalkable = level.WalkableTiles.Contains(cell);
            var isWall = level.WallTiles.Contains(cell);

            if (isWall)
            {
                var south = cell + Vector2Int.down;
                return level.WalkableTiles.Contains(south) ? CaveDecorPlacementContext.CeilingHang : (CaveDecorPlacementContext?)null;
            }

            if (!isWalkable)
            {
                return null;
            }

            return HasAdjacentWall(cell, level.WallTiles)
                ? CaveDecorPlacementContext.WallHug
                : CaveDecorPlacementContext.FloorCluster;
        }

        private static bool HasAdjacentWall(Vector2Int cell, HashSet<Vector2Int> wallTiles)
        {
            foreach (var dir in OrthogonalDirs)
            {
                if (wallTiles.Contains(cell + dir))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Lista, em ordem determinística (x depois y), todas as células walkable do nível que
        /// classificam para o contexto pedido. Usado pelo planner para restringir candidatos por
        /// contexto antes de aplicar peso/cluster.</summary>
        public static List<Vector2Int> CollectCells(CaveGeneratedLevel level, CaveDecorPlacementContext context)
        {
            var result = new List<Vector2Int>();
            if (level == null)
            {
                return result;
            }

            // CeilingHang candidatos vêm de WallTiles; os demais contextos vêm de WalkableTiles.
            IEnumerable<Vector2Int> source = context == CaveDecorPlacementContext.CeilingHang
                ? level.WallTiles
                : level.WalkableTiles;

            if (source == null)
            {
                return result;
            }

            foreach (var cell in source)
            {
                if (Classify(cell, level) == context)
                {
                    result.Add(cell);
                }
            }

            result.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            return result;
        }
    }
}
