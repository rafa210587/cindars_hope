using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Enemy
{
    /// <summary>
    /// Fatia as folhas de animacao dos inimigos do batch 1 (movimento + ataques) a partir das folhas
    /// NORMALIZADAS em art/enemy_anim_gpt/normalized/gpt_&lt;slug&gt;_&lt;clip&gt;.png (fallback: raw/), gerando
    /// sub-sprites em Assets/_Game/Resources/EnemyAnimSprites/&lt;slug&gt;_&lt;clip&gt;.png, nomeados
    /// "&lt;slug&gt;_&lt;clip&gt;_r{row}_c{col}" (row 0 = topo da folha). O runtime
    /// <see cref="CindarsHope.Enemy.EnemyAnimator"/> carrega por Resources.LoadAll e mapeia direcao->linha.
    ///
    /// Grade: 5 colunas sempre; N linhas variavel por folha (5 bipede/quadrupede, 3 inseto/voador,
    /// 1 efeito radial). N e inferido do aspecto: o normalizador usa celulas de 320x360, entao
    /// cellW = texW/5 e rows = round(texH / (cellW * 360/320)).
    ///
    /// Folhas normalizadas ja vem RGBA transparentes (o normalizador Python removeu o fundo) — sao
    /// copiadas preservando alpha. So o fallback raw/ (fundo opaco de xadrez) passa pela remocao de
    /// fundo em C# (mesma mascara light + flood-fill de borda do GenerateNpcWalkAnimations).
    ///
    /// Idempotente. NAO expoe [MenuItem] proprio — registrado como RunStep em
    /// CindarsHopeMenu.InicializarProjeto (rule editor-generation-orchestration).
    /// </summary>
    public static class GenerateEnemyWalkAnimations
    {
        private const string NormalizedRoot = "art/enemy_anim_gpt/normalized";
        private const string RawRoot = "art/enemy_anim_gpt/raw";
        private const string DestRoot = "Assets/_Game/Resources/EnemyAnimSprites";

        private const int Columns = 5;
        // Aspecto da celula do normalizador (CELL_H/CELL_W = 360/320). Usado para inferir N linhas.
        private const float CellAspect = 360f / 320f;

        // Clips conhecidos (sufixos de arquivo). Usados para separar slug (pode ter '_') do clip.
        private static readonly string[] Clips =
        {
            "walk", "idle",
            "atk_bite", "atk_slash", "atk_claw", "atk_throw", "atk_scream", "atk_leap",
            "atk_bow", "atk_whip", "atk_nova", "atk_rise",
            // batch 2
            "atk_charge", "atk_thrust", "atk_cast", "atk_blink", "atk_buff",
            "atk_cleave", "atk_slam",
            // batch 2b
            "atk_summon",
        };

        public static void GenerateAll()
        {
            var normDir = Path.Combine(Application.dataPath, "..", NormalizedRoot);
            var rawDir = Path.Combine(Application.dataPath, "..", RawRoot);
            bool useNormalized = Directory.Exists(normDir);
            var sourceDir = useNormalized ? normDir : rawDir;
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogWarning($"[GenerateEnemyWalkAnimations] Pasta nao encontrada: {sourceDir}. Nada a gerar.");
                return;
            }
            Debug.Log($"[GenerateEnemyWalkAnimations] Fonte: {sourceDir}" +
                      (useNormalized ? " (normalizadas)" : " (cruas - normalized ausente)"));

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", DestRoot));

            // Passada UNICA por arquivo (sem dois lotes de StartAssetEditing — o batching adiava o
            // reimport e o segundo ReadTextureSettings lia o textureType default e o reescrevia,
            // deixando o asset como Textura Default sem sub-sprites). Aqui: escreve o PNG, LE as
            // dimensoes do proprio arquivo (sem depender de reimport para o slice), importa e
            // configura textureType=Sprite + Multiple + spritesheet numa unica SaveAndReimport.
            int ok = 0, skip = 0;
            foreach (var srcFile in Directory.GetFiles(sourceDir, "gpt_*.png"))
            {
                var fileName = Path.GetFileName(srcFile);
                var stem = fileName.Substring("gpt_".Length, fileName.Length - "gpt_".Length - ".png".Length);
                if (!TrySplit(stem, out var slug, out var clip))
                {
                    Debug.LogWarning($"[GenerateEnemyWalkAnimations] '{fileName}' nao casa gpt_<slug>_<clip>.png. Pulando.");
                    skip++;
                    continue;
                }

                var destAssetPath = $"{DestRoot}/{slug}_{clip}.png";
                var destFullPath = Path.Combine(Application.dataPath, "..", destAssetPath);
                if (useNormalized)
                    File.Copy(srcFile, destFullPath, overwrite: true); // ja RGBA transparente
                else
                    WriteWithTransparentBackground(srcFile, destFullPath);

                // Dimensoes lidas do PNG no disco (nao do asset importado) — evita depender de reimport.
                if (!TryReadPngSize(destFullPath, out int texW, out int texH))
                {
                    Debug.LogWarning($"[GenerateEnemyWalkAnimations] Nao consegui ler dimensoes de {destAssetPath}. Pulando.");
                    skip++;
                    continue;
                }
                var metas = BuildSpriteMetas(texW, texH, slug, clip);

                AssetDatabase.ImportAsset(destAssetPath, ImportAssetOptions.ForceUpdate);
                ConfigureAndSlice(destAssetPath, metas);
                ok++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[GenerateEnemyWalkAnimations] Concluido. Fatiadas: {ok} | Puladas: {skip}.");
        }

        // Le largura/altura do cabecalho PNG (bytes 16..23, big-endian) sem importar o asset.
        private static bool TryReadPngSize(string fullPath, out int width, out int height)
        {
            width = 0; height = 0;
            try
            {
                var b = File.ReadAllBytes(fullPath);
                if (b.Length < 24 || b[0] != 0x89 || b[1] != 0x50) return false; // assinatura PNG
                width = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19];
                height = (b[20] << 24) | (b[21] << 16) | (b[22] << 8) | b[23];
                return width > 0 && height > 0;
            }
            catch { return false; }
        }

        // "gen_dire_rat_atk_bite" -> slug="gen_dire_rat", clip="atk_bite". Casa o sufixo de clip mais
        // longo (para nao confundir 'atk_bite' com um hipotetico 'bite').
        private static bool TrySplit(string stem, out string slug, out string clip)
        {
            slug = null; clip = null;
            string best = null;
            foreach (var c in Clips)
            {
                if (stem.EndsWith("_" + c) && (best == null || c.Length > best.Length))
                    best = c;
            }
            if (best == null) return false;
            clip = best;
            slug = stem.Substring(0, stem.Length - best.Length - 1);
            return slug.Length > 0;
        }

        // Configura o importer (Sprite + Multiple + PPU + npot) E aplica o spritesheet numa UNICA
        // SaveAndReimport — atomico, sem race entre lotes. Seta tanto as propriedades diretas quanto
        // o TextureImporterSettings (belt-and-suspenders, como o GeneratedSpriteImporter faz).
        private static void ConfigureAndSlice(string assetPath, SpriteMetaData[] metas)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[GenerateEnemyWalkAnimations] TextureImporter nao encontrado para {assetPath}.");
                return;
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 128f; // convencao dos sprites de inimigo (EnemySprites)
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 8192;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.textureType = TextureImporterType.Sprite;
            settings.spriteMode = (int)SpriteImportMode.Multiple;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            settings.spritePixelsPerUnit = 128f;
            settings.filterMode = FilterMode.Point;
            settings.mipmapEnabled = false;
            settings.npotScale = TextureImporterNPOTScale.None;
            importer.SetTextureSettings(settings);

#pragma warning disable CS0618 // spritesheet obsoleto porem funcional no Unity 6; caminho minimo p/ slice.
            importer.spritesheet = metas;
#pragma warning restore CS0618

            importer.SaveAndReimport();
        }

        private static SpriteMetaData[] BuildSpriteMetas(int texW, int texH, string slug, string clip)
        {
            float cellW = texW / (float)Columns;
            int rows = Mathf.Max(1, Mathf.RoundToInt(texH / (cellW * CellAspect)));
            float cellH = texH / (float)rows;

            var metas = new SpriteMetaData[rows * Columns];
            for (int row = 0; row < rows; row++)
            {
                int yFromBottom = rows - 1 - row; // Unity Y origem no rodape; row 0 = topo da folha
                for (int col = 0; col < Columns; col++)
                {
                    int index = row * Columns + col;
                    metas[index] = new SpriteMetaData
                    {
                        name = $"{slug}_{clip}_r{row}_c{col}",
                        rect = new Rect(col * cellW, yFromBottom * cellH, cellW, cellH),
                        pivot = new Vector2(0.5f, 0f), // BottomCenter
                        alignment = (int)SpriteAlignment.BottomCenter,
                    };
                }
            }
            return metas;
        }

        // Fallback so para raw/ (fundo opaco de xadrez claro): mesma remocao do GenerateNpcWalkAnimations.
        private static void WriteWithTransparentBackground(string srcFullPath, string destFullPath)
        {
            byte[] rawBytes = File.ReadAllBytes(srcFullPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(rawBytes))
            {
                File.Copy(srcFullPath, destFullPath, overwrite: true);
                Object.DestroyImmediate(tex);
                return;
            }
            int w = tex.width, h = tex.height;
            Color32[] px = tex.GetPixels32();
            int n = w * h;
            var light = new bool[n];
            for (int i = 0; i < n; i++)
            {
                Color32 c = px[i];
                int mn = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                int mx = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                light[i] = mn >= 215 && (mx - mn) <= 16;
            }
            var bg = new bool[n];
            var stack = new Stack<int>();
            for (int x = 0; x < w; x++)
            {
                int top = x, bottom = (h - 1) * w + x;
                if (light[top] && !bg[top]) { bg[top] = true; stack.Push(top); }
                if (light[bottom] && !bg[bottom]) { bg[bottom] = true; stack.Push(bottom); }
            }
            for (int y = 0; y < h; y++)
            {
                int left = y * w, right = y * w + (w - 1);
                if (light[left] && !bg[left]) { bg[left] = true; stack.Push(left); }
                if (light[right] && !bg[right]) { bg[right] = true; stack.Push(right); }
            }
            while (stack.Count > 0)
            {
                int i = stack.Pop();
                int y = i / w, x = i - y * w;
                if (x > 0) { int j = i - 1; if (light[j] && !bg[j]) { bg[j] = true; stack.Push(j); } }
                if (x < w - 1) { int j = i + 1; if (light[j] && !bg[j]) { bg[j] = true; stack.Push(j); } }
                if (y > 0) { int j = i - w; if (light[j] && !bg[j]) { bg[j] = true; stack.Push(j); } }
                if (y < h - 1) { int j = i + w; if (light[j] && !bg[j]) { bg[j] = true; stack.Push(j); } }
            }
            for (int i = 0; i < n; i++) if (bg[i]) px[i].a = 0;
            tex.SetPixels32(px);
            tex.Apply(false);
            File.WriteAllBytes(destFullPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
    }
}
