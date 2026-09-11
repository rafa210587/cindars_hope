using System;
using System.Linq;
using CindarsHope.World.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.Art
{
    /// <summary>Persists sixteen synchronized water tiles from an authored six-frame Aseprite atlas.</summary>
    internal static class FarmWaterAnimationAuthoring
    {
        internal static TileBase[] BuildAtlas()
        {
            var source = WorldSpriteLibrary.Ground("ground_water_loop_v4");
            if (source == null || source.texture.width != 384 || source.texture.height != 64)
                throw new InvalidOperationException("Farm water loop must contain six64x64 frames in a384x64 sheet.");
            var atlas = new TileBase[16];
            for (var y = 0; y < 4; y++)
            for (var x = 0; x < 4; x++)
            {
                var path = "Assets/_Game/Art/Generated/World/_TileAssets/farm_water_loop_v4_" + x + "_" + y + ".asset";
                var tile = AssetDatabase.LoadAssetAtPath<SpriteAnimationTileSO>(path);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<SpriteAnimationTileSO>();
                    AssetDatabase.CreateAsset(tile, path);
                }
                var existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                var frames = new Sprite[6];
                for (var frame = 0; frame < frames.Length; frame++)
                {
                    var name = "Water_" + x + "_" + y + "_Frame_" + frame;
                    var rect = new Rect(frame * 64 + x * 16, y * 16, 16, 16);
                    var sprite = existing.SingleOrDefault(item => item.name == name);
                    if (sprite != null && (sprite.texture != source.texture || sprite.rect != rect))
                        throw new InvalidOperationException("Existing water sprite contract differs: " + name);
                    if (sprite == null)
                    {
                        sprite = Sprite.Create(source.texture, rect, new Vector2(0.5f, 0f), 16f, 0, SpriteMeshType.FullRect);
                        sprite.name = name;
                        AssetDatabase.AddObjectToAsset(sprite, tile);
                    }
                    frames[frame] = sprite;
                }
                tile.Configure(frames, 5f);
                EditorUtility.SetDirty(tile);
                atlas[y * 4 + x] = tile;
            }
            AssetDatabase.SaveAssets();
            return atlas;
        }
    }
}
