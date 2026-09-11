using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.Art
{
    /// <summary>
    /// Chao de mundo via Tilemap (best practice 2D top-down) em vez de SpriteRenderer Tiled — que
    /// estoura o mesh ("Cannot generate 9 slice") em areas grandes e gera grade feia. Ver skill
    /// tilemap-world-rendering. Cria um Grid + Tilemap e pinta regioes retangulares com uma tile.
    /// Rule Tiles / variacao aleatoria (pacote 2D Tilemap Extras) sao um passo futuro; a tile de
    /// chao ja e seamless (reseam), entao um Tilemap dela da campo uniforme sem seam.
    /// </summary>
    public static class WorldTilemapGround
    {
        private const string TileAssetDir = "Assets/_Game/Art/Generated/World/_TileAssets";

        public enum TerrainTransitionKind { GrassToDirt, WaterToRock }

        /// <summary>Cria (ou reobtem) um Grid+Tilemap filho de parent. cellSize deve casar o tile em unidades.</summary>
        public static Tilemap GetOrCreateLayer(Transform parent, string gridName, string layerName,
            float cellSize, int sortingOrder, string sortingLayerName)
        {
            Grid grid;
            var gridTf = parent.Find(gridName);
            if (gridTf == null)
            {
                var gridGo = new GameObject(gridName);
                gridGo.transform.SetParent(parent);
                gridGo.transform.position = Vector3.zero;
                grid = gridGo.AddComponent<Grid>();
                grid.cellSize = new Vector3(cellSize, cellSize, 0f);
            }
            else
            {
                grid = gridTf.GetComponent<Grid>();
            }

            var layerTf = grid.transform.Find(layerName);
            if (layerTf != null)
            {
                return layerTf.GetComponent<Tilemap>();
            }

            var tmGo = new GameObject(layerName);
            tmGo.transform.SetParent(grid.transform);
            tmGo.transform.localPosition = Vector3.zero;
            var tm = tmGo.AddComponent<Tilemap>();
            var tr = tmGo.AddComponent<TilemapRenderer>();
            tr.mode = TilemapRenderer.Mode.Chunk; // base de chao num unico depth
            tr.sortingOrder = sortingOrder;
            TrySetSortingLayer(tr, sortingLayerName);
            return tm;
        }

        /// <summary>Tile asset (persistente) para um sprite; cacheado por nome. colliderType None (chao).</summary>
        public static Tile GetTile(Sprite sprite)
        {
            EnsureFolder();
            var path = $"{TileAssetDir}/tile_{sprite.name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.None;
                AssetDatabase.CreateAsset(tile, path);
            }
            return tile;
        }

        /// <summary>Pinta um retangulo (world units: centro + tamanho) na Tilemap com o sprite dado.</summary>
        public static void PaintRect(Tilemap tm, Sprite sprite, Vector2 center, Vector2 sizeUnits)
        {
            var tile = GetTile(sprite);
            float cs = tm.layoutGrid != null ? tm.layoutGrid.cellSize.x : 1f;
            if (cs <= 0f) cs = 1f;
            int xmin = Mathf.FloorToInt((center.x - sizeUnits.x * 0.5f) / cs);
            int xmax = Mathf.CeilToInt((center.x + sizeUnits.x * 0.5f) / cs);
            int ymin = Mathf.FloorToInt((center.y - sizeUnits.y * 0.5f) / cs);
            int ymax = Mathf.CeilToInt((center.y + sizeUnits.y * 0.5f) / cs);
            for (int x = xmin; x < xmax; x++)
            {
                for (int y = ymin; y < ymax; y++)
                {
                    tm.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        /// <summary>Tamanho de um sprite em unidades (para casar o cellSize do Grid).</summary>
        public static float SpriteWorldSize(Sprite sprite)
        {
            return sprite.pixelsPerUnit > 0f ? sprite.rect.width / sprite.pixelsPerUnit : 1f;
        }

        // Pinta um retangulo escolhendo por celula entre varios sprites por PESO, de forma
        // DETERMINISTICA (hash de x,y) — quebra a repeticao sem RNG dependente de ordem.
        public static void PaintRectWeighted(Tilemap tm, Sprite[] sprites, int[] weights, Vector2 center, Vector2 sizeUnits)
        {
            if (sprites == null || sprites.Length == 0) return;
            var tiles = new Tile[sprites.Length];
            int total = 0;
            for (int i = 0; i < sprites.Length; i++) { tiles[i] = GetTile(sprites[i]); total += Mathf.Max(1, weights[i]); }
            float cs = tm.layoutGrid != null ? tm.layoutGrid.cellSize.x : 1f;
            if (cs <= 0f) cs = 1f;
            int xmin = Mathf.FloorToInt((center.x - sizeUnits.x * 0.5f) / cs);
            int xmax = Mathf.CeilToInt((center.x + sizeUnits.x * 0.5f) / cs);
            int ymin = Mathf.FloorToInt((center.y - sizeUnits.y * 0.5f) / cs);
            int ymax = Mathf.CeilToInt((center.y + sizeUnits.y * 0.5f) / cs);
            for (int x = xmin; x < xmax; x++)
            {
                for (int y = ymin; y < ymax; y++)
                {
                    int h = ((x * 73856093) ^ (y * 19349663)) & 0x7fffffff;
                    int r = h % total;
                    int idx = 0;
                    while (idx < weights.Length - 1 && r >= Mathf.Max(1, weights[idx])) { r -= Mathf.Max(1, weights[idx]); idx++; }
                    tm.SetTile(new Vector3Int(x, y, 0), tiles[idx]);
                }
            }
        }

        // Grama com VARIACAO (best practice SLYNYRD): base + variantes sutis + flores/pedrinhas
        // esparsas, pintadas via scatter ponderado. Variantes ausentes caem para so a base.
        public static void PaintGrass(Transform parent, string gridName, int sortingOrder, string sortingLayer, Vector2 center, Vector2 sizeUnits)
        {
            var baseG = WorldSpriteLibrary.Ground("ground_grass");
            if (baseG == null) return;
            float cs = SpriteWorldSize(baseG);
            var tm = GetOrCreateLayer(parent, gridName, "Ground", cs, sortingOrder, sortingLayer);
            var defs = new (string n, int w)[]
            {
                ("ground_grass", 60), ("ground_grass_a", 15), ("ground_grass_b", 15),
                ("ground_grass_flower", 6), ("ground_grass_pebble", 4),
            };
            var sprites = new List<Sprite>();
            var weights = new List<int>();
            foreach (var d in defs)
            {
                var s = WorldSpriteLibrary.Ground(d.n);
                if (s != null) { sprites.Add(s); weights.Add(d.w); }
            }
            PaintRectWeighted(tm, sprites.ToArray(), weights.ToArray(), center, sizeUnits);
        }

        // Agua via Tilemap (lago/rio) — substitui SpriteRenderer esticado + tint alpha por tiles
        // OPACOS do sprite ground_water real, no mesmo Grid do chao. sortingOrder 2: ACIMA da grama (0)
        // e da margem/Shore (1) — a agua cobre o miolo e a areia so aparece no anel da borda.
        // Sem mascarar: se ground_water estiver ausente, nao pinta nada (o chamador decide fallback).
        public static void PaintWater(Transform parent, string gridName, Vector2 center, Vector2 sizeUnits)
        {
            var waterSprite = WorldSpriteLibrary.Ground("ground_water");
            if (waterSprite == null) return;
            float cs = SpriteWorldSize(waterSprite);
            var tm = GetOrCreateLayer(parent, gridName, "Water", cs, sortingOrder: 3, "Ground");
            PaintRect(tm, waterSprite, center, sizeUnits);
        }

        // Pinta um retangulo de qualquer tile de CHAO (WorldSpriteLibrary.Ground) numa layer generica
        // do mesmo Grid — usado para caminhos de terra/pedra, penhasco, etc (nao apenas grama/agua).
        // Se o tile nao existir na biblioteca, nao pinta nada (sem mascarar — wiring-error fica a
        // cargo do chamador, que ja loga quando ground_* falta em outros pontos deste arquivo).
        public static void PaintTile(Transform parent, string gridName, string layerName, int sortingOrder,
            string groundTileName, Vector2 center, Vector2 sizeUnits)
        {
            var tileSprite = WorldSpriteLibrary.Ground(groundTileName);
            if (tileSprite == null) return;
            float cs = SpriteWorldSize(tileSprite);
            var tm = GetOrCreateLayer(parent, gridName, layerName, cs, sortingOrder, "Ground");
            PaintRect(tm, tileSprite, center, sizeUnits);
        }

        /// <summary>Pinta as celulas cujo centro pertence ao poligono, sem preencher o bounding box.</summary>
        public static void PaintPolygon(Transform parent, string gridName, string layerName, int sortingOrder,
            string tileName, IReadOnlyList<Vector2> polygon)
        {
            var tileSprite = WorldSpriteLibrary.Ground(tileName);
            if (tileSprite == null)
            {
                throw new InvalidOperationException("WorldTilemapGround: missing terrain tile '" + tileName + "' for polygon paint.");
            }

            var cellSize = SpriteWorldSize(tileSprite);
            var tilemap = GetOrCreateLayer(parent, gridName, layerName, cellSize, sortingOrder, "Ground");
            var tile = GetTile(tileSprite);
            var cells = RasterizePolygonCells(polygon, cellSize);
            for (var i = 0; i < cells.Count; i++) tilemap.SetTile(cells[i], tile);
        }

        /// <summary>
        /// Paints a deterministic eight-neighbour transition ring outside the polygon. The current
        /// world kit has no authored edge sprites yet; it deliberately uses the nearest existing
        /// dirt/rock tiles so the missing edge art remains visible to the validator and report.
        /// </summary>
        public static void PaintTransitionRing(Transform parent, string gridName, string layerName, int sortingOrder,
            IReadOnlyList<Vector2> polygon, TerrainTransitionKind kind)
        {
            var tileName = kind == TerrainTransitionKind.WaterToRock ? "ground_cliff_rock" : "ground_path_dirt";
            var tileSprite = WorldSpriteLibrary.Ground(tileName);
            if (tileSprite == null)
            {
                throw new InvalidOperationException("WorldTilemapGround: missing fallback tile '" + tileName + "' for " + kind + " transition.");
            }

            var cellSize = SpriteWorldSize(tileSprite);
            var tilemap = GetOrCreateLayer(parent, gridName, layerName, cellSize, sortingOrder, "Ground");
            var tile = GetTile(tileSprite);
            var inside = new HashSet<Vector3Int>(RasterizePolygonCells(polygon, cellSize));
            var ring = RasterizeTransitionRingCells(inside);
            foreach (var cell in ring) tilemap.SetTile(cell, tile);
        }

        /// <summary>Pure deterministic polygon rasterizer used by the terrain painter and EditMode tests.</summary>
        public static IReadOnlyList<Vector3Int> RasterizePolygonCells(IReadOnlyList<Vector2> polygon, float cellSize)
        {
            if (polygon == null) throw new ArgumentNullException(nameof(polygon));
            if (polygon.Count < 3) throw new ArgumentException("A terrain polygon needs at least three points.", nameof(polygon));
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize));

            var min = polygon[0];
            var max = min;
            for (var i = 1; i < polygon.Count; i++)
            {
                min = Vector2.Min(min, polygon[i]);
                max = Vector2.Max(max, polygon[i]);
            }

            var xmin = Mathf.FloorToInt(min.x / cellSize);
            var xmax = Mathf.CeilToInt(max.x / cellSize);
            var ymin = Mathf.FloorToInt(min.y / cellSize);
            var ymax = Mathf.CeilToInt(max.y / cellSize);
            var cells = new List<Vector3Int>();
            for (var x = xmin; x < xmax; x++)
            {
                for (var y = ymin; y < ymax; y++)
                {
                    var center = new Vector2((x + 0.5f) * cellSize, (y + 0.5f) * cellSize);
                    if (ContainsPoint(polygon, center)) cells.Add(new Vector3Int(x, y, 0));
                }
            }

            return cells;
        }

        public static IReadOnlyCollection<Vector3Int> RasterizeTransitionRingCells(IReadOnlyCollection<Vector3Int> filledCells)
        {
            var filled = new HashSet<Vector3Int>(filledCells);
            var ring = new HashSet<Vector3Int>();
            foreach (var cell in filled)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        var neighbour = new Vector3Int(cell.x + dx, cell.y + dy, cell.z);
                        if (!filled.Contains(neighbour)) ring.Add(neighbour);
                    }
                }
            }

            return ring;
        }

        // Pinta um ANEL de margem (areia) ao redor de um retangulo de agua: o rect EXPANDIDO
        // (center, size + 2*ring) e pintado na layer "Shore" (sortingOrder 1, mesma ordem da agua).
        // O chamador deve pintar a agua DEPOIS (mesmo center/size do rect base, sem o ring) para que
        // ela cubra o miolo do rect expandido e a areia so fique visivel na borda.
        public static void PaintShoreRing(Transform parent, string gridName, string shoreTileName,
            Vector2 center, Vector2 sizeUnits, float ringUnits)
        {
            var expandedSize = new Vector2(sizeUnits.x + 2f * ringUnits, sizeUnits.y + 2f * ringUnits);
            PaintTile(parent, gridName, "Shore", sortingOrder: 1, shoreTileName, center, expandedSize);
        }

        private static void EnsureFolder()
        {
            if (AssetDatabase.IsValidFolder(TileAssetDir)) return;
            const string parent = "Assets/_Game/Art/Generated/World";
            if (!AssetDatabase.IsValidFolder(parent))
            {
                // Cria a cadeia minima se necessario.
                AssetDatabase.CreateFolder("Assets/_Game/Art/Generated", "World");
            }
            AssetDatabase.CreateFolder(parent, "_TileAssets");
        }

        private static bool ContainsPoint(IReadOnlyList<Vector2> polygon, Vector2 point)
        {
            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var current = polygon[i];
                var previous = polygon[j];
                if ((current.y > point.y) == (previous.y > point.y)) continue;
                var crossingX = (previous.x - current.x) * (point.y - current.y) /
                                (previous.y - current.y) + current.x;
                if (point.x < crossingX) inside = !inside;
            }

            return inside;
        }

        private static void TrySetSortingLayer(Renderer r, string layerName)
        {
            foreach (var l in SortingLayer.layers)
            {
                if (l.name == layerName)
                {
                    r.sortingLayerName = layerName;
                    return;
                }
            }
            // layer inexistente: mantem default (nao loga warning do Unity).
        }
    }
}
