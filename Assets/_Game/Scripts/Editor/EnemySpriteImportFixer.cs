using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    /// <summary>
    /// Dev tool: forca PixelsPerUnit + config de pixel art em TODOS os sprites de
    /// Assets/_Game/Resources/EnemySprites e reimporta. Deterministico (seta direto no
    /// TextureImporter e SaveAndReimport), nao depende do AssetPostprocessor nem de timing.
    /// 1 tile do cave = 1 unidade; sprite 64px / PPU => altura base, x EnemyScaleResolver (medium=2x).
    /// </summary>
    public static class EnemySpriteImportFixer
    {
        private const string Folder = "Assets/_Game/Resources/EnemySprites";

        // 64px / 128 = 0.5 un base; medium (2x) => 1 un = 1 tile. Suba p/ menores, desca p/ maiores.
        private const float Ppu = 128f;

        [MenuItem("CindarsHope/Dev/Fix Enemy Sprite Size (reimport)")]
        public static void FixSize()
        {
            if (!Directory.Exists(Folder))
            {
                Debug.LogWarning("[EnemySpriteImportFixer] pasta nao encontrada: " + Folder);
                return;
            }

            int done = 0;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Folder });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;

                imp.textureType = TextureImporterType.Sprite;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.mipmapEnabled = false;
                imp.filterMode = FilterMode.Point;
                imp.spritePixelsPerUnit = Ppu;
                imp.spriteImportMode = SpriteImportMode.Single;

                var s = new TextureImporterSettings();
                imp.ReadTextureSettings(s);
                s.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                s.spritePixelsPerUnit = Ppu;
                imp.SetTextureSettings(s);

                imp.SaveAndReimport();
                done++;
            }
            AssetDatabase.Refresh();
            Debug.Log($"[EnemySpriteImportFixer] Reimportados {done} sprites com PPU {Ppu}. " +
                      $"De Play na CaveScene: inimigo medio ~1 tile (~2x player).");
        }
    }
}
