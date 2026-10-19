using System.Collections.Generic;
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
            int varridos = 0;
            int jaCorretos = 0;
            var pendentes = new List<TextureImporter>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                varridos++;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);

                bool typeOk = importer.textureType == TextureImporterType.Sprite;
                bool modeOk = importer.spriteImportMode == SpriteImportMode.Single;
                bool filterOk = importer.filterMode == FilterMode.Bilinear;
                bool mipmapOk = !importer.mipmapEnabled;
                bool alphaOk = !importer.alphaIsTransparency;
                bool maxSizeOk = importer.maxTextureSize == 512;
                bool compressionOk = importer.textureCompression == TextureImporterCompression.Uncompressed;
                bool meshTypeOk = settings.spriteMeshType == SpriteMeshType.FullRect;
                bool alignmentOk = settings.spriteAlignment == (int)SpriteAlignment.Center;

                if (typeOk && modeOk && filterOk && mipmapOk && alphaOk && maxSizeOk && compressionOk
                    && meshTypeOk && alignmentOk)
                {
                    jaCorretos++;
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = false;
                importer.maxTextureSize = 512;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);
                pendentes.Add(importer);
            }

            if (pendentes.Count > 0)
            {
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var importer in pendentes)
                    {
                        importer.SaveAndReimport();
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[AssignNpcPortraitBackgrounds] Fundos varridos: {varridos}. " +
                      $"Configurados como Sprite: {pendentes.Count}. Ja corretos (pulados): {jaCorretos}.");
        }
    }
}
