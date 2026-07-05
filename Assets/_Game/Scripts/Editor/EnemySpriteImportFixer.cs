using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    /// <summary>
    /// Forca PixelsPerUnit + config de pixel art em TODOS os sprites de
    /// Assets/_Game/Resources/EnemySprites e reimporta. Deterministico (seta direto no
    /// TextureImporter e SaveAndReimport), nao depende do AssetPostprocessor nem de timing.
    /// 1 tile do cave = 1 unidade; sprite 64px / PPU => altura base, x EnemyScaleResolver (medium=2x).
    /// Idempotente. Tambem corrige overrides de platform (Standalone/WebGL) que podem ter
    /// ficado com compression != Uncompressed quando o PNG foi copiado no disco fora do Editor
    /// (o AssetPostprocessor GeneratedSpriteImporter roda o Default mas overrides de platform
    /// especifica podem sobreviver de um import anterior).
    /// </summary>
    public static class EnemySpriteImportFixer
    {
        private const string Folder = "Assets/_Game/Resources/EnemySprites";

        // Altura-base do CONTEUDO (na escala 1.0) que todo sprite deve ter, em unidades de mundo.
        // Igual ao legado (64px de conteudo @ PPU128 = 0.5 un). Depois o EnemyScaleResolver aplica
        // 2.0 (regua do player) x ratio(size) x role — entao Medium = 1 un = tamanho do player.
        private const float BaseContentUnits = 0.5f;
        private const float FallbackPpu = 128f;
        private const byte AlphaThreshold = 10;

        // Sem [MenuItem]: agora roda como RunStep dentro de "Inicializar Projeto" e "Reparar e
        // Reconstruir" (regra editor-generation-orchestration: nada de comando avulso).
        public static void FixSize()
        {
            if (!Directory.Exists(Folder))
            {
                Debug.LogWarning("[EnemySpriteImportFixer] pasta nao encontrada: " + Folder);
                return;
            }

            int varridos = 0;
            int jaCorretos = 0;
            var pendentes = new List<TextureImporter>();
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Folder });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;

                varridos++;

                // PPU por sprite: normaliza a ALTURA DO CONTEUDO para BaseContentUnits, para que
                // canvases de tamanhos diferentes nao facam sprites ficarem gigantes/minusculos.
                // A diferenca de tamanho entre inimigos vem do BestiarySize (EnemyScaleResolver),
                // nunca de quao grande o PNG foi desenhado.
                float ppu = ComputePpuForContent(path);

                var s = new TextureImporterSettings();
                imp.ReadTextureSettings(s);

                bool typeOk = imp.textureType == TextureImporterType.Sprite;
                bool compressionOk = imp.textureCompression == TextureImporterCompression.Uncompressed;
                bool mipmapOk = !imp.mipmapEnabled;
                bool filterOk = imp.filterMode == FilterMode.Point;
                bool ppuOk = Mathf.Approximately(imp.spritePixelsPerUnit, ppu);
                bool modeOk = imp.spriteImportMode == SpriteImportMode.Single;
                bool alignmentOk = s.spriteAlignment == (int)SpriteAlignment.BottomCenter;
                bool platformOk = true;
                foreach (var platform in new[] { "Standalone", "WebGL" })
                {
                    var existing = imp.GetPlatformTextureSettings(platform);
                    if (existing.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        platformOk = false;
                        break;
                    }
                }

                if (typeOk && compressionOk && mipmapOk && filterOk && ppuOk && modeOk && alignmentOk && platformOk)
                {
                    jaCorretos++;
                    continue;
                }

                imp.textureType = TextureImporterType.Sprite;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.mipmapEnabled = false;
                imp.filterMode = FilterMode.Point;
                imp.spritePixelsPerUnit = ppu;
                imp.spriteImportMode = SpriteImportMode.Single;

                s.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                s.spritePixelsPerUnit = ppu;
                imp.SetTextureSettings(s);

                // Overrides de platform (Standalone/WebGL) podem ter ficado com compression != None
                // de um import anterior ao AssetPostprocessor rodar; forca Uncompressed nelas tambem.
                foreach (var platform in new[] { "Standalone", "WebGL" })
                {
                    var platformSettings = imp.GetPlatformTextureSettings(platform);
                    if (platformSettings.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        platformSettings.overridden = true;
                        platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                        imp.SetPlatformTextureSettings(platformSettings);
                    }
                }

                pendentes.Add(imp);
            }

            if (pendentes.Count > 0)
            {
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var imp in pendentes)
                    {
                        imp.SaveAndReimport();
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            Debug.Log($"[EnemySpriteImportFixer] Sprites varridos: {varridos}. " +
                      $"Reimportados com PPU por-sprite: {pendentes.Count} " +
                      $"(altura de conteudo normalizada p/ {BaseContentUnits} un base). " +
                      $"Ja corretos (pulados): {jaCorretos}. " +
                      $"Tamanho final = base x EnemyScaleResolver(BestiarySize).");
        }

        /// <summary>
        /// Le o PNG, mede a altura do conteudo opaco (bbox de alpha) e devolve o PPU que faz essa
        /// altura valer <see cref="BaseContentUnits"/> na escala 1.0. Fallback = <see cref="FallbackPpu"/>.
        /// </summary>
        private static float ComputePpuForContent(string path)
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(bytes))
                {
                    Object.DestroyImmediate(tex);
                    return FallbackPpu;
                }

                int w = tex.width, h = tex.height;
                var px = tex.GetPixels32();
                Object.DestroyImmediate(tex);

                int minY = h, maxY = -1;
                for (int y = 0; y < h; y++)
                {
                    int row = y * w;
                    for (int x = 0; x < w; x++)
                    {
                        if (px[row + x].a > AlphaThreshold)
                        {
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                            break;
                        }
                    }
                }

                if (maxY < minY) return FallbackPpu; // totalmente transparente
                int contentH = maxY - minY + 1;
                float ppu = contentH / BaseContentUnits; // contentH / ppu = BaseContentUnits
                return Mathf.Clamp(ppu, 32f, 8192f);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EnemySpriteImportFixer] falha ao medir '{path}': {e.Message}. Usando PPU {FallbackPpu}.");
                return FallbackPpu;
            }
        }
    }
}
