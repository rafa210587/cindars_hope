using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.NPC
{
    /// <summary>
    /// Garante que os PNGs de fundo em Assets/_Game/Resources/NpcPortraitBackgrounds sejam importados
    /// como Sprite (para Resources.Load&lt;Sprite&gt; em runtime no NpcInteractionPortraitHud). Idempotente.
    /// Registrado como RunStep em InicializarProjeto (apos AssignNpcPortraitSprites). Sem [MenuItem] proprio.
    /// </summary>
    public static class AssignNpcPortraitBackgrounds
    {
        private const string Root = "Assets/_Game/Resources/NpcPortraitBackgrounds";

        public static void ConfigureAll()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Root });
            int ok = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = false;
                importer.maxTextureSize = 512;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                ok++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[AssignNpcPortraitBackgrounds] Fundos configurados como Sprite: {ok}.");
        }
    }
}
