using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Art
{
    /// <summary>
    /// Corrige o pivot dos sprites de MUNDO com volume (predios, arvores, props, folhagem,
    /// interior, animais) para BottomCenter, exigido pelo Y-sort global (Transparency Sort Mode =
    /// Custom Axis no eixo Y + spriteSortPoint = Pivot nos renderers da layer "World"). Com pivot
    /// CENTER o objeto "afunda" metade da altura no Y-sort e a ordem de desenho fica errada.
    ///
    /// Nao mexe em "tiles/" (chao/tilemap) nem "crops/" (uso proprio da farm) — ficam com o pivot
    /// que ja tinham.
    ///
    /// Sem [MenuItem] proprio (regra editor-generation-orchestration): registrado como RunStep em
    /// <see cref="CindarsHope.Editor.CindarsHopeMenu.InicializarProjeto"/>, ANTES da recriacao das
    /// 3 cenas (o import precisa acontecer antes da materializacao).
    /// </summary>
    public static class WorldSpritePivotImportStep
    {
        private const string WorldRoot = "Assets/_Game/Art/Generated/World";

        // Subpastas com volume (objetos que "ficam de pe" no mundo) — precisam de pivot na base
        // para o Y-sort funcionar. "building/houses_modular" e subpasta de "building" e entra
        // automaticamente por prefixo de path.
        private static readonly string[] VolumeFolders =
        {
            "building",
            "trees",
            "props",
            "foliage",
            "interior",
            "animals",
            "locations", // prédios bespoke inteiros (landmark) — base do sprite no chão p/ Y-sort
        };

        public static void EnsureWorldSpritePivots()
        {
            int varridos = 0;
            int jaCorretos = 0;
            var pendentes = new List<TextureImporter>();

            foreach (var folder in VolumeFolders)
            {
                var folderPath = $"{WorldRoot}/{folder}";
                if (!Directory.Exists(folderPath))
                {
                    Debug.LogWarning($"[WorldSpritePivotImportStep] pasta nao encontrada: {folderPath}");
                    continue;
                }

                var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                    {
                        continue;
                    }

                    varridos++;

                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);

                    bool alignmentOk = settings.spriteAlignment == (int)SpriteAlignment.BottomCenter;
                    bool filterOk = importer.filterMode == FilterMode.Point;
                    bool compressionOk = importer.textureCompression == TextureImporterCompression.Uncompressed;

                    if (alignmentOk && filterOk && compressionOk)
                    {
                        jaCorretos++;
                        continue;
                    }

                    // PPU preservado por-asset (nao normalizado para um valor fixo): as pastas de
                    // mundo hoje misturam PPU 128 (maioria) e 64 (building/houses_modular); forcar
                    // um PPU unico mudaria a escala visual de dezenas de sprites sem relacao com o
                    // objetivo desta fase (pivot p/ Y-sort). Ver retorno da tarefa.
                    settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                    importer.SetTextureSettings(settings);
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    pendentes.Add(importer);
                }
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

            Debug.Log(
                $"[WorldSpritePivotImportStep] Sprites de mundo varridos: {varridos}. " +
                $"Ajustados p/ BottomCenter/Point/Uncompressed: {pendentes.Count}. " +
                $"Ja corretos (pulados): {jaCorretos}.");
        }
    }
}
