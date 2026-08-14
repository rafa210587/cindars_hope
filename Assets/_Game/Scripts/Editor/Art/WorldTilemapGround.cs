using System.Collections.Generic;
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
