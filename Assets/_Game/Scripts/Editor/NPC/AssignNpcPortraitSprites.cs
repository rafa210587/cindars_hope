using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.NPC
{
    /// <summary>
    /// Atribui os 5 retratos (busto) de cada NpcDataSO a partir de PNGs em
    /// Assets/_Game/Resources/NpcPortraits/&lt;art_folder&gt;_&lt;expr&gt;.png, onde expr ∈
    /// { neutral, happiness, love, disdain, hatred }. Reusa o mapa NpcId-&gt;art_folder de
    /// AssignNpcBodySprites. Configura o TextureImporter para sprite de UI (Point, FullRect,
    /// uncompressed). Idempotente: pula graciosamente quando o PNG nao existe (sem falhar o lote).
    ///
    /// Registro no orquestrador: RunStep em InicializarProjeto, logo apos AssignNpcBodySprites.
    /// NAO expoe [MenuItem] proprio — use CindarsHope/Inicializar Projeto.
    /// </summary>
    public static class AssignNpcPortraitSprites
    {
        private const string NpcDataRoot = "Assets/_Game/Data/NPCs";
        private const string PortraitRoot = "Assets/_Game/Resources/NpcPortraits";

        // Sufixo do arquivo -> nome do campo Sprite no NpcDataSO.
        private static readonly (string expr, string field)[] ExpressionFields =
        {
            ("neutral", "PortraitNeutral"),
            ("happiness", "PortraitHappiness"),
            ("love", "PortraitLove"),
            ("disdain", "PortraitDisdain"),
            ("hatred", "PortraitHatred"),
        };

        public static void AssignAll()
        {
            var guids = AssetDatabase.FindAssets("t:NpcDataSO", new[] { NpcDataRoot });
            int assigned = 0;
            int skipped = 0;

            foreach (var guid in guids)
            {
                var soPath = AssetDatabase.GUIDToAssetPath(guid);
                var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(soPath);
                if (npcData == null) continue;

                if (!AssignNpcBodySprites.NpcArtFolder.TryGetValue(npcData.NpcId, out var artFolder))
                {
                    skipped++;
                    continue;
                }

                var so = new SerializedObject(npcData);
                bool any = false;
                foreach (var (expr, field) in ExpressionFields)
                {
                    var pngPath = $"{PortraitRoot}/{artFolder}_{expr}.png";
                    if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "..", pngPath)))
                    {
                        continue;
                    }

                    ConfigureImporter(pngPath);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
                    if (sprite == null)
                    {
                        Debug.LogWarning($"[AssignNpcPortraitSprites] LoadAssetAtPath<Sprite> null para {pngPath} (NPC {npcData.NpcId}).");
                        continue;
                    }

                    so.FindProperty(field).objectReferenceValue = sprite;
                    any = true;
                }

                if (any)
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(npcData);
                    assigned++;
                }
                else
                {
                    skipped++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[AssignNpcPortraitSprites] Concluido. NPCs com >=1 retrato: {assigned} | sem retrato/sem-PNG: {skipped}.");
        }

        private static void ConfigureImporter(string pngPath)
        {
            var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }
    }
}
