using System;
using System.Collections.Generic;
using CindarsHope.Editor.Art;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Town ground uses one consistent half-unit raster for paths, banks and water.</summary>
    public static class TownKeyartGround
    {
        public const float CellSize = .5f;
        private static readonly Dictionary<Tile, Tile[]> CobblePatterns = new Dictionary<Tile, Tile[]>();
        public static Tilemap Layer(Transform parent, string name, int order)
        {
            return WorldTilemapGround.GetOrCreateLayer(parent, "TownUnitGrid", name, CellSize, order, "Ground");
        }

        public static Tile TileFor(Sprite sprite)
        {
            if (sprite == null) throw new InvalidOperationException("Town ground sprite is missing.");
            string path = "Assets/_Game/Art/Generated/World/props/town_tile_" + sprite.name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null) throw new InvalidOperationException("Pre-authored Town tile is missing; run the explicit tile authoring step: "+path);
            Vector3 size = sprite.bounds.size;
            var scale = new Vector3(CellSize / size.x, CellSize / size.y, 1f);
            var expectedTransform = Matrix4x4.TRS(-Vector3.Scale(sprite.bounds.center, scale), Quaternion.identity, scale);
            RequireTile(tile,sprite,expectedTransform,path);
            if ((sprite.name == "town_cobble_keyart" || sprite.name == "ground_water_keyart_v4") && !CobblePatterns.ContainsKey(tile))
            {
                string patternId=sprite.name=="town_cobble_keyart"?"cobble":"water_v4";
                var pattern = new Tile[16];
                for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                {
                    int index = y * 4 + x;
                    string slicePath = "Assets/_Game/Art/Generated/World/props/town_tile_"+patternId+"_slice_" + index.ToString("00") + ".asset";
                    var part = AssetDatabase.LoadAssetAtPath<Tile>(slicePath);
                    if (part == null) throw new InvalidOperationException("Pre-authored Town pattern tile is missing; run the explicit tile authoring step: "+slicePath);
                    Sprite slice = null;
                    foreach (var subasset in AssetDatabase.LoadAllAssetsAtPath(slicePath))
                        if (subasset is Sprite storedSprite) slice = storedSprite;
                    if (slice == null || !slice.name.EndsWith("Slice_"+index,StringComparison.OrdinalIgnoreCase) || slice.texture != sprite.texture ||
                        !Mathf.Approximately(slice.rect.width,16)||!Mathf.Approximately(slice.rect.height,16))
                        throw new InvalidOperationException("Town pattern slice contract mismatch: "+slicePath);
                    RequireTile(part,slice,Matrix4x4.identity,slicePath);
                    pattern[index] = part;
                }
                CobblePatterns.Add(tile, pattern);
            }
            return tile;
        }

        private static void RequireTile(Tile tile,Sprite sprite,Matrix4x4 expectedTransform,string path)
        {
            bool matrix=true;
            for(int row=0;row<4;row++)for(int column=0;column<4;column++)
                matrix&=Mathf.Abs(tile.transform[row,column]-expectedTransform[row,column])<=.0001f;
            if(tile.sprite!=sprite||tile.colliderType!=Tile.ColliderType.None||tile.color!=Color.white||!matrix)
                throw new InvalidOperationException("Town tile contract mismatch; generation will not mutate Art: "+path);
        }

        public static void Set(Tilemap map, Vector2 worldPoint, Tile template)
        {
            var cell = map.WorldToCell(worldPoint);
            if (CobblePatterns.TryGetValue(template, out var pattern))
            {
                int x = ((Mathf.FloorToInt(worldPoint.x / CellSize) % 4) + 4) % 4;
                int y = ((Mathf.FloorToInt(worldPoint.y / CellSize) % 4) + 4) % 4;
                map.SetTile(cell, pattern[y * 4 + x]);
            }
            else map.SetTile(cell, template);
        }

        public static void PaintPolygon(Tilemap map, Tile tile, Vector2[] polygon)
        {
            for (int x = -Mathf.CeilToInt(TownDistrictLayout.HalfWidth / CellSize); x < TownDistrictLayout.HalfWidth / CellSize; x++)
            for (int y = -Mathf.CeilToInt(TownDistrictLayout.HalfHeight / CellSize); y < TownDistrictLayout.HalfHeight / CellSize; y++)
            {
                Vector2 point = new Vector2(x + .5f, y + .5f) * CellSize;
                if (Contains(point, polygon)) Set(map, point, tile);
            }
        }

        public static bool Contains(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                Vector2 a = polygon[i], b = polygon[j];
                if ((a.y > point.y) != (b.y > point.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside;
        }

        public static void PaintShore(Tilemap map, Tile tile, Vector2[] polygon)
        {
            for (int x = -Mathf.CeilToInt(TownDistrictLayout.HalfWidth / CellSize); x < 0; x++)
            for (int y = -Mathf.CeilToInt(TownDistrictLayout.HalfHeight / CellSize); y < TownDistrictLayout.HalfHeight / CellSize; y++)
            {
                Vector2 point = new Vector2(x + .5f, y + .5f) * CellSize;
                for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
                {
                    Vector2 a = polygon[i], delta = polygon[j] - a;
                    Vector2 closest = a + delta * Mathf.Clamp01(Vector2.Dot(point - a, delta) / delta.sqrMagnitude);
                    if ((point - closest).sqrMagnitude > 2.25f) continue;
                    Set(map, point, tile);
                    break;
                }
            }
        }

        public static void PaintCurve(Tilemap map, Tile tile, float width, params Vector2[] points)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 a = points[Mathf.Max(0, i - 1)], b = points[i], c = points[i + 1], d = points[Mathf.Min(points.Length - 1, i + 2)];
                int samples = Mathf.Max(4, Mathf.CeilToInt(Vector2.Distance(b, c) / .2f));
                for (int s = 0; s <= samples; s++)
                {
                    float t = s / (float)samples;
                    Vector2 p = .5f * ((2 * b) + (-a + c) * t + (2 * a - 5 * b + 4 * c - d) * t * t + (-a + 3 * b - 3 * c + d) * t * t * t);
                    PaintDisk(map, tile, p, width * .5f);
                }
            }
        }

        public static void PaintRoad(Tilemap map, Tile tile, TownRoadSegment road)
        {
            for(int x=Mathf.FloorToInt(road.MinX/CellSize);x<=Mathf.CeilToInt(road.MaxX/CellSize);x++)
            for(int y=Mathf.FloorToInt(road.MinY/CellSize);y<=Mathf.CeilToInt(road.MaxY/CellSize);y++)
            {
                var point=new Vector2(x+.5f,y+.5f)*CellSize;
                if(road.Contains(point))Set(map,point,tile);
            }
        }

        public static void PaintDisk(Tilemap map, Tile tile, Vector2 center, float radius)
        {
            for (float x = Mathf.Floor((center.x - radius) / CellSize) * CellSize; x <= center.x + radius; x += CellSize)
            for (float y = Mathf.Floor((center.y - radius) / CellSize) * CellSize; y <= center.y + radius; y += CellSize)
            {
                var p = new Vector2(x + CellSize * .5f, y + CellSize * .5f);
                if ((p - center).sqrMagnitude <= radius * radius && TownKeyartGeometry.ClearPoint(p, TownCityLayout.AllBuildings, .05f))
                    Set(map, p, tile);
            }
        }
    }
}
