using System;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>One-off, idempotent importer repair for the floor tile whose BottomCenter pivot
    /// shifts its opaque 1x1 sprite and leaves transparent strips in the Cave Tilemap.</summary>
    public static class RepairCaveFloorSpriteAlignment
    {
        private const string SpritePath = "Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/floor_c.png";

        // Batch: -executeMethod CindarsHope.Editor.Validation.RepairCaveFloorSpriteAlignment.Repair
        public static void Repair()
        {
            if (AssetImporter.GetAtPath(SpritePath) is not TextureImporter importer)
                throw new InvalidOperationException("TextureImporter ausente: " + SpritePath);

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (importer.spritePivot != new Vector2(0.5f, 0.5f))
                throw new InvalidOperationException("floor_c tem pivot serializado inesperado: " + importer.spritePivot);

            var changed = settings.spriteAlignment != (int)SpriteAlignment.Center;
            if (changed)
            {
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }

            var reloadedImporter = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            var reloadedSettings = new TextureImporterSettings();
            reloadedImporter?.ReadTextureSettings(reloadedSettings);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (reloadedImporter == null || reloadedSettings.spriteAlignment != (int)SpriteAlignment.Center ||
                sprite == null || !Mathf.Approximately(sprite.pivot.x, 64f) ||
                !Mathf.Approximately(sprite.pivot.y, 64f))
                throw new InvalidOperationException("floor_c ainda nao tem pivot central apos reimport. alignment=" +
                    reloadedSettings.spriteAlignment + " pivot=" + (sprite != null ? sprite.pivot.ToString() : "NULL"));

            Debug.Log("[CaveFloorSpriteAlignment] PASS changed=" + changed +
                " pivot=" + sprite.pivot + " bounds=" + sprite.bounds);
        }
    }
}
