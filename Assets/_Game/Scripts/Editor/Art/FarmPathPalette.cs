using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.Art
{
    /// <summary>World-aligned road atlas: preserves authored pixel density across small geometry cells.</summary>
    public static class FarmPathPalette
    {
        private const string TexturePath = "Assets/_Game/Art/Generated/World/tiles/ground_path_contact_v16.png";
        private const string TileRoot = "Assets/_Game/Art/Generated/World/_TileAssets";
        private const int CellPixels = 4;
        private const int AxisCells = 16;
        private static Tile[] tiles;

        public static Tile At(Vector3Int cell)
        {
            if (tiles == null) Build();
            int x = (cell.x % AxisCells + AxisCells) % AxisCells;
            int y = (cell.y % AxisCells + AxisCells) % AxisCells;
            return tiles[y * AxisCells + x];
        }

        /// <summary>Finish the complete road union, so intersecting paths have no internal seams.</summary>
        public static void FinishEdges(Tilemap tilemap)
        {
            var occupied = new HashSet<Vector3Int>();
            foreach (var cell in tilemap.cellBounds.allPositionsWithin)
                if (tilemap.HasTile(cell)) occupied.Add(cell);
            var fringe = new HashSet<Vector3Int>();
            foreach (var cell in occupied)
            {
                var boundary = false;
                for (var dy = -1; dy <= 1; dy++)
                for (var dx = -1; dx <= 1; dx++)
                {
                    var neighbor = cell + new Vector3Int(dx, dy, 0);
                    if (occupied.Contains(neighbor)) continue;
                    boundary = true;
                    fringe.Add(neighbor);
                }
                var cluster = EdgeCluster(cell);
                tilemap.SetTileFlags(cell, TileFlags.None);
                tilemap.SetColor(cell, new Color(1f, 1f, 1f,
                    boundary ? 0.58f + cluster * 0.28f : 1f));
            }
            foreach (var cell in fringe)
            {
                var cluster = EdgeCluster(cell);
                if (cluster < 0.35f) continue;
                tilemap.SetTile(cell, At(cell));
                tilemap.SetTileFlags(cell, TileFlags.None);
                tilemap.SetColor(cell, new Color(1f, 1f, 1f, 0.16f + cluster * 0.22f));
                tilemap.SetTransformMatrix(cell, Matrix4x4.identity);
            }
        }

        private static float EdgeCluster(Vector3Int cell)
        {
            // Small world-anchored patches avoid white-noise edges and change neither RNG nor saves.
            var x = Mathf.FloorToInt(cell.x / 3f);
            var y = Mathf.FloorToInt(cell.y / 3f);
            unchecked
            {
                var hash = (uint)(x * 73856093 ^ y * 19349663);
                hash ^= hash >> 13;
                return (hash & 255) / 255f;
            }
        }

        private static void Build()
        {
            var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (importer == null) throw new FileNotFoundException("Missing Aseprite road texture", TexturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 32;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (texture.width != 64 || texture.height != 64)
                throw new System.InvalidOperationException("Road atlas must remain 64x64.");
            tiles = new Tile[AxisCells * AxisCells];
            for (int y = 0; y < AxisCells; y++)
            for (int x = 0; x < AxisCells; x++)
            {
                var path = TileRoot + "/farm_road_" + x + "_" + y + ".asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, path);
                }
                var sprite = Sprite.Create(texture, new Rect(x * CellPixels, y * CellPixels, CellPixels, CellPixels),
                    new Vector2(0.5f, 0f), 32, 0, SpriteMeshType.FullRect);
                sprite.name = "RoadCell_" + x + "_" + y;
                if (tile.sprite == null)
                {
                    AssetDatabase.AddObjectToAsset(sprite, tile);
                    tile.sprite = sprite;
                }
                else
                {
                    // Preserve existing tile/subasset identities while refining the geometry grid.
                    EditorUtility.CopySerialized(sprite, tile.sprite);
                    Object.DestroyImmediate(sprite);
                    EditorUtility.SetDirty(tile.sprite);
                }
                tile.colliderType = Tile.ColliderType.None;
                EditorUtility.SetDirty(tile);
                tiles[y * AxisCells + x] = tile;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
