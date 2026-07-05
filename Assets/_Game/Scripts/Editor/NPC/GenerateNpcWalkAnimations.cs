using System.Collections.Generic;
using System.IO;
using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.NPC
{
    /// <summary>
    /// Fatia as folhas de sprite de caminhada dos NPCs (grid 5 colunas x 5 linhas: 5 frames de walk x
    /// 5 direcoes geradas — down, downleft, right, up, upleft; as outras 3 direcoes sao espelhadas em
    /// runtime por <see cref="CindarsHope.NPC.NpcWalkAnimator"/>) a partir dos PNGs crus em
    /// art/npc_anim_gpt/raw/gpt_&lt;nome_curto&gt;_walk.png (fora de Assets, raiz do repo).
    ///
    /// As folhas cruas vem sem canal alpha, com um fundo opaco de xadrez claro (checkerboard).
    /// Antes de copiar, remove esse fundo para alpha transparente (mascara pixel claro/dessaturado +
    /// flood-fill 4-conexo a partir das bordas, preservando areas claras internas como roupa branca) e
    /// escreve o PNG RGBA resultante em Assets/_Game/Resources/NpcWalkSprites/&lt;npcId&gt;_walk.png.
    /// Em seguida configura o TextureImporter em modo Multiple (25 sub-sprites nomeados
    /// "&lt;npcId&gt;_walk_r{row}_c{col}", row/col 0-4) e popula NpcDataSO.WalkAnimResourcesPath.
    ///
    /// Idempotente: pode rodar de novo sem duplicar (sobrescreve a copia e o spritesheet).
    /// NAO expoe [MenuItem] proprio — registrado como RunStep em CindarsHopeMenu.InicializarProjeto,
    /// logo apos AssignNpcBodySprites (rule editor-generation-orchestration).
    /// </summary>
    public static class GenerateNpcWalkAnimations
    {
        // Fonte PREFERIDA: folhas NORMALIZADAS (fundo transparente + 25 personagens re-centrados no
        // eixo dos pes e alinhados numa baseline uniforme, grade 5x5 de celulas iguais). Geradas pelo
        // normalizador tools/npc_walk (Python) a partir das folhas cruas. Corrige o desalinhamento das
        // folhas cruas do gpt-image-1 (personagens fora de centro / cabeca cruzando a celula vizinha),
        // que causava frames cortados e "tremidos". Se a pasta normalized nao existir, cai para raw.
        private const string NormalizedRoot = "art/npc_anim_gpt/normalized";
        private const string RawRoot = "art/npc_anim_gpt/raw";
        private const string DestRoot = "Assets/_Game/Resources/NpcWalkSprites";
        private const string NpcDataRoot = "Assets/_Game/Data/NPCs";

        private const int Columns = 5; // 5 frames de walk
        private const int Rows = 5;    // down, downleft, right, up, upleft (nessa ordem, topo->baixo)

        // Sufixos de nome-curto de arquivo que sao intermediarios/descartados — nunca usar como fonte.
        private static readonly string[] IgnoredSuffixes =
        {
            "_bad.png",
            "_v1_bad.png",
            "_backup.png",
            "_base.png",
        };

        // Nome-curto do arquivo (gpt_<nome_curto>_walk.png) -> npcId canonico. Cruzado manualmente com
        // AssignNpcBodySprites.NpcArtFolder (o art_folder de cada NPC contem o nome curto correspondente).
        // "cat" nao mapeia a nenhum NpcDataSO (companion, nao NPC de dialogo) — pulado com aviso.
        private static readonly Dictionary<string, string> ShortNameToNpcId = new()
        {
            { "alaric",       "npc_alaric" },
            { "brumdar",      "npc_brumdar" },
            { "dagna",        "npc_dagna" },
            { "eiran",        "npc_eiran" },
            { "gruta",        "npc_gruta" },
            { "gurd",         "npc_gurd" },
            { "hess",         "npc_hess" },
            { "hund",         "npc_hund" },
            { "liora",        "npc_liora" },
            { "maelor",       "npc_maelor" },
            { "mara",         "npc_mara" },
            { "mella",        "npc_mella" },
            { "mirela",       "npc_mirela" },
            { "nimble",       "npc_nimble" },
            { "orlan",        "npc_orlan" },
            { "ozzra",        "npc_ozzra" },
            { "pip",          "npc_pip" },
            { "renko",        "npc_renko" },
            { "sael",         "npc_sael" },
            { "savra",        "npc_savra" },
            { "sylveth",      "npc_sylveth" },
            { "thalindra",    "npc_thalindra" },
            { "tibbet",       "npc_tibbet" },
            { "tovin",        "npc_tovin" },
            { "velorin",      "npc_velorin" },
            { "yael",         "npc_yael" },
            { "zrix_goblin",  "npc_zrix" },
        };

        /// <summary>Ponto de entrada chamado pelo orquestrador (RunStep em InicializarProjeto).</summary>
        public static void GenerateAll()
        {
            var normDir = Path.Combine(Application.dataPath, "..", NormalizedRoot);
            var rawDir = Path.Combine(Application.dataPath, "..", RawRoot);
            // Prefere as folhas normalizadas (cortes corretos); cai para as cruas so se normalized nao existir.
            var sourceDir = Directory.Exists(normDir) ? normDir : rawDir;
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogWarning($"[GenerateNpcWalkAnimations] Pasta nao encontrada: {sourceDir}. Nada a gerar.");
                return;
            }
            Debug.Log($"[GenerateNpcWalkAnimations] Fonte das folhas: {sourceDir}" +
                      (sourceDir == normDir ? " (normalizadas)" : " (cruas - normalized ausente)"));

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", DestRoot));

            int okCount = 0;
            int skipCount = 0;

            // Fase 1 (leitura + escrita de PNG no disco, fora de batching de import): resolve
            // cada arquivo fonte -> (npcId, npcData, destAssetPath) e escreve o PNG sem fundo.
            // Nao importa/reimporta ainda — isso acontece em lote na Fase 2.
            var pending = new List<(string npcId, NpcDataSO npcData, string destAssetPath)>();

            foreach (var srcFile in Directory.GetFiles(sourceDir, "gpt_*_walk*.png"))
            {
                var fileName = Path.GetFileName(srcFile);

                if (IsIgnored(fileName))
                {
                    continue; // intermediarios (_bad/_backup/_base) — nem contam como skip.
                }

                var shortName = ExtractShortName(fileName);
                if (shortName == null || !ShortNameToNpcId.TryGetValue(shortName, out var npcId))
                {
                    Debug.LogWarning($"[GenerateNpcWalkAnimations] Nome curto '{shortName}' (arquivo {fileName}) " +
                                      "nao mapeia a nenhum npcId conhecido. Pulando.");
                    skipCount++;
                    continue;
                }

                var npcData = FindNpcData(npcId);
                if (npcData == null)
                {
                    Debug.LogWarning($"[GenerateNpcWalkAnimations] NpcDataSO nao encontrado para {npcId} " +
                                      $"(arquivo {fileName}). Pulando.");
                    skipCount++;
                    continue;
                }

                var destAssetPath = $"{DestRoot}/{npcId}_walk.png";
                var destFullPath = Path.Combine(Application.dataPath, "..", destAssetPath);
                WriteWithTransparentBackground(srcFile, destFullPath);

                pending.Add((npcId, npcData, destAssetPath));
            }

            if (pending.Count == 0)
            {
                Debug.Log($"[GenerateNpcWalkAnimations] Concluido. Fatiados/atribuidos: 0 | Pulados: {skipCount}.");
                return;
            }

            // Fase 2 (escrita em lote #1): importa todos os PNGs recem-escritos e configura os
            // settings base (npotScale=None etc.) num unico Asset Pipeline Refresh. Necessario
            // ANTES de ler width/height na Fase 3 (o tamanho nativo so materializa apos este import).
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var (_, _, destAssetPath) in pending)
                {
                    AssetDatabase.ImportAsset(destAssetPath, ImportAssetOptions.ForceUpdate);
                    ConfigureBaseImporter(destAssetPath);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            // Fase 3 (leitura pos-import): agora texture.width/height refletem o tamanho nativo
            // (npotScale=None ja aplicado), entao da pra calcular os SpriteMetaData de cada folha.
            var sliceWork = new List<(string npcId, string destAssetPath, SpriteMetaData[] metas)>();
            foreach (var (npcId, _, destAssetPath) in pending)
            {
                sliceWork.Add((npcId, destAssetPath, BuildSpriteMetas(destAssetPath, npcId)));
            }

            // Fase 4 (escrita em lote #2): aplica o spritesheet fatiado em todos os assets, tambem
            // num unico Asset Pipeline Refresh.
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var (_, destAssetPath, metas) in sliceWork)
                {
                    ApplySpriteMetas(destAssetPath, metas);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            // Fase 5: atribui o path de recurso no NpcDataSO (sem custo de import).
            foreach (var (npcId, npcData, _) in pending)
            {
                var so = new SerializedObject(npcData);
                so.FindProperty("WalkAnimResourcesPath").stringValue = $"NpcWalkSprites/{npcId}_walk";
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(npcData);
                okCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[GenerateNpcWalkAnimations] Concluido. Fatiados/atribuidos: {okCount} | Pulados: {skipCount}.");
        }

        private static bool IsIgnored(string fileName)
        {
            foreach (var suffix in IgnoredSuffixes)
            {
                if (fileName.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        // "gpt_zrix_goblin_walk.png" -> "zrix_goblin"; "gpt_alaric_walk.png" -> "alaric".
        private static string ExtractShortName(string fileName)
        {
            const string prefix = "gpt_";
            const string suffix = "_walk.png";
            if (!fileName.StartsWith(prefix) || !fileName.EndsWith(suffix))
            {
                return null;
            }
            return fileName.Substring(prefix.Length, fileName.Length - prefix.Length - suffix.Length);
        }

        // Carrega o PNG cru (fundo opaco de xadrez claro) e escreve um PNG RGBA no destino com o
        // fundo removido para alpha=0. Mascara pixels claros dessaturados (o checkerboard) e faz
        // flood-fill 4-conexo a partir das bordas, preservando pixels claros INTERNOS (roupa
        // branca cercada por outline). Algoritmo validado; orientacao (origem bottom-left do
        // GetPixels32) e irrelevante pois o flood parte de todas as 4 bordas.
        private static void WriteWithTransparentBackground(string srcFullPath, string destFullPath)
        {
            byte[] rawBytes = File.ReadAllBytes(srcFullPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(rawBytes))
            {
                Debug.LogWarning($"[GenerateNpcWalkAnimations] Falha ao decodificar {srcFullPath}; copiando cru (fundo opaco).");
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
            // seed: pixels light nas 4 bordas (i = y*w + x)
            for (int x = 0; x < w; x++)
            {
                int top = x;                 // y=0
                int bottom = (h - 1) * w + x; // y=h-1
                if (light[top] && !bg[top]) { bg[top] = true; stack.Push(top); }
                if (light[bottom] && !bg[bottom]) { bg[bottom] = true; stack.Push(bottom); }
            }
            for (int y = 0; y < h; y++)
            {
                int left = y * w;            // x=0
                int right = y * w + (w - 1); // x=w-1
                if (light[left] && !bg[left]) { bg[left] = true; stack.Push(left); }
                if (light[right] && !bg[right]) { bg[right] = true; stack.Push(right); }
            }
            while (stack.Count > 0)
            {
                int i = stack.Pop();
                int y = i / w, x = i - y * w;
                if (x > 0)     { int j = i - 1; if (light[j] && !bg[j]) { bg[j] = true; stack.Push(j); } }
                if (x < w - 1) { int j = i + 1; if (light[j] && !bg[j]) { bg[j] = true; stack.Push(j); } }
                if (y > 0)     { int j = i - w; if (light[j] && !bg[j]) { bg[j] = true; stack.Push(j); } }
                if (y < h - 1) { int j = i + w; if (light[j] && !bg[j]) { bg[j] = true; stack.Push(j); } }
            }
            for (int i = 0; i < n; i++)
            {
                if (bg[i]) { px[i].a = 0; }
            }
            tex.SetPixels32(px);
            tex.Apply(false);
            byte[] outBytes = tex.EncodeToPNG();
            File.WriteAllBytes(destFullPath, outBytes);
            Object.DestroyImmediate(tex);
        }

        private static NpcDataSO FindNpcData(string npcId)
        {
            var guids = AssetDatabase.FindAssets("t:NpcDataSO", new[] { NpcDataRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<NpcDataSO>(path);
                if (data != null && data.NpcId == npcId)
                {
                    return data;
                }
            }
            return null;
        }

        /// <summary>
        /// Fase 2 (dentro do batch #1): configura o TextureImporter em modo Multiple com
        /// npotScale=None (a folha, ex. 750x750, e NPOT — sem isto o Unity reescala para a
        /// potencia de 2 mais proxima e os rects calculados caem fora da textura, falhando TODO
        /// o slice). Nao aplica spritesheet ainda — isso exige ler o tamanho NATIVO pos-reimport,
        /// que so materializa depois do StopAssetEditing() do batch #1 (ver <see cref="BuildSpriteMetas"/>).
        /// </summary>
        private static void ConfigureBaseImporter(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[GenerateNpcWalkAnimations] TextureImporter nao encontrado para {assetPath}.");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            // Mesma escala de personagem usada pelo corpo estatico do NPC (AssignNpcBodySprites).
            importer.spritePixelsPerUnit = 234f;
            // CRITICO: a folha (ex.: 750x750) e NPOT. Sem isto o Unity reescala a textura para a
            // potencia de 2 mais proxima e os rects calculados caem FORA da textura ("rect lies
            // outside of texture"), falhando TODO o slice. Trava tamanho nativo e teto alto.
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 8192;

            importer.SaveAndReimport();
        }

        /// <summary>
        /// Fase 3 (leitura pos-import do batch #1): le texture.width/height, ja no tamanho NATIVO
        /// (npotScale=None aplicado por <see cref="ConfigureBaseImporter"/>), e calcula os
        /// SpriteMetaData do grid 5x5 (celula uniforme = texWidth/5). Nomes previsiveis
        /// "&lt;npcId&gt;_walk_r{row}_c{col}". Nao escreve nada no importer — leitura pura.
        /// </summary>
        private static SpriteMetaData[] BuildSpriteMetas(string assetPath, string npcId)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            int texWidth = texture != null ? texture.width : 1024;
            int texHeight = texture != null ? texture.height : 1024;
            float cellWidth = texWidth / (float)Columns;
            float cellHeight = texHeight / (float)Rows;

            var metas = new SpriteMetaData[Rows * Columns];
            for (int row = 0; row < Rows; row++)
            {
                // Sprites em Unity usam origem Y no canto inferior-esquerdo; a folha foi descrita
                // top->baixo (row 0 = down no topo da imagem), entao inverte pra pegar a linha certa.
                int yFromBottom = Rows - 1 - row;
                for (int col = 0; col < Columns; col++)
                {
                    int index = row * Columns + col;
                    metas[index] = new SpriteMetaData
                    {
                        name = $"{npcId}_walk_r{row}_c{col}",
                        rect = new Rect(col * cellWidth, yFromBottom * cellHeight, cellWidth, cellHeight),
                        pivot = new Vector2(0.5f, 0f), // BottomCenter
                        alignment = (int)SpriteAlignment.BottomCenter,
                    };
                }
            }
            return metas;
        }

        /// <summary>
        /// Fase 4 (dentro do batch #2): aplica o spritesheet ja calculado e reimporta.
        /// </summary>
        private static void ApplySpriteMetas(string assetPath, SpriteMetaData[] metas)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[GenerateNpcWalkAnimations] TextureImporter nao encontrado para {assetPath}.");
                return;
            }

#pragma warning disable CS0618 // TextureImporter.spritesheet e obsoleto porem ainda funcional no Unity 6;
                               // e o caminho minimo (ladder) para slicing multi-sprite via script sem
                               // depender da API de data provider (mais verbosa) so para isto.
            importer.spritesheet = metas;
#pragma warning restore CS0618

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }
    }
}
