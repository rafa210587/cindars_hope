using System.Collections.Generic;
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
            int skipped = 0;

            // Fase 1 (leitura, fora de batching): resolve NpcDataSO -> (field, pngPath) existentes
            // e enfileira reimport so para os PNGs ainda sem os settings desejados.
            var toAssign = new List<(NpcDataSO npcData, string field, string pngPath)>();
            var toReimport = new List<TextureImporter>();

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

                bool anyFound = false;
                foreach (var (expr, field) in ExpressionFields)
                {
                    var pngPath = $"{PortraitRoot}/{artFolder}_{expr}.png";
                    if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "..", pngPath)))
                    {
                        continue;
                    }

                    anyFound = true;
                    if (TryConfigureImporter(pngPath, out var importer))
                    {
                        toReimport.Add(importer);
                    }

                    toAssign.Add((npcData, field, pngPath));
                }

                if (!anyFound)
                {
                    skipped++;
                }
            }

            // Fase 2 (escrita em lote): 1 unico Asset Pipeline Refresh para todos os PNGs pendentes.
            if (toReimport.Count > 0)
            {
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var importer in toReimport)
                    {
                        importer.SaveAndReimport();
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            // Fase 3 (leitura pos-import + atribuicao no SO).
            int assigned = 0;
            NpcDataSO currentNpc = null;
            SerializedObject currentSo = null;
            bool currentAny = false;

            void FlushCurrent()
            {
                if (currentNpc == null) return;
                if (currentAny)
                {
                    currentSo.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(currentNpc);
                    assigned++;
                }
                else
                {
                    skipped++;
                }
            }

            foreach (var (npcData, field, pngPath) in toAssign)
            {
                if (!ReferenceEquals(npcData, currentNpc))
                {
                    FlushCurrent();
                    currentNpc = npcData;
                    currentSo = new SerializedObject(npcData);
                    currentAny = false;
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
                if (sprite == null)
                {
                    Debug.LogWarning($"[AssignNpcPortraitSprites] LoadAssetAtPath<Sprite> null para {pngPath} (NPC {npcData.NpcId}).");
                    continue;
                }

                currentSo.FindProperty(field).objectReferenceValue = sprite;
                currentAny = true;
            }
            FlushCurrent();

            AssetDatabase.SaveAssets();
            Debug.Log($"[AssignNpcPortraitSprites] Concluido. NPCs com >=1 retrato: {assigned} | " +
                      $"Reimportados: {toReimport.Count} | sem retrato/sem-PNG: {skipped}.");
        }

        /// <summary>
        /// Le os settings atuais do importer e, se ja corretos (idempotencia), nao enfileira
        /// reimport. Retorna true (com o importer) quando ha mudanca pendente.
        /// </summary>
        private static bool TryConfigureImporter(string pngPath, out TextureImporter importer)
        {
            importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (importer == null) return false;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            bool typeOk = importer.textureType == TextureImporterType.Sprite;
            bool modeOk = importer.spriteImportMode == SpriteImportMode.Single;
            bool pivotOk = importer.spritePivot == new Vector2(0.5f, 0.5f);
            bool ppuOk = Mathf.Approximately(importer.spritePixelsPerUnit, 100f);
            bool filterOk = importer.filterMode == FilterMode.Point;
            bool mipmapOk = !importer.mipmapEnabled;
            bool alphaOk = importer.alphaIsTransparency;
            bool maxSizeOk = importer.maxTextureSize == 1024;
            bool compressionOk = importer.textureCompression == TextureImporterCompression.Uncompressed;
            bool meshTypeOk = settings.spriteMeshType == SpriteMeshType.FullRect;
            bool alignmentOk = settings.spriteAlignment == (int)SpriteAlignment.Center;

            if (typeOk && modeOk && pivotOk && ppuOk && filterOk && mipmapOk && alphaOk
                && maxSizeOk && compressionOk && meshTypeOk && alignmentOk)
            {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            return true;
        }
    }
}
