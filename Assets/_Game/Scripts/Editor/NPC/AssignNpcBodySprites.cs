using System.Collections.Generic;
using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.NPC
{
    /// <summary>
    /// Atribui NpcDataSO.BodySprite a partir dos PNGs gerados em
    /// Assets/_Game/Resources/NpcSprites/&lt;art_folder&gt;.png.
    /// Configura o TextureImporter para pixel-art (PPU 128, BottomCenter, Point, mipmaps off).
    /// Deve rodar DEPOIS da geracao de dados de NPC e ANTES de recriar as cenas
    /// (para que CreateMvpTownScene ja leia BodySprite ao montar os prefabs de NPC).
    ///
    /// Registro no orquestrador: RunStep em InicializarProjeto (FASE A, apos geracao de NPC assets).
    /// NAO expoe [MenuItem] proprio — use CindarsHope/Inicializar Projeto.
    /// </summary>
    public static class AssignNpcBodySprites
    {
        private const string NpcDataRoot = "Assets/_Game/Data/NPCs";
        private const string SpriteRoot  = "Assets/_Game/Resources/NpcSprites";

        // Tabela NpcId -> art_folder (baseada em NPC_PROMPTS_REVIEW.md + roster de CreateMvpTownScene).
        // NPCs sem art_folder (ex.: npc_corvus, npc_vaalara_wanderer) sao pulados graciosamente.
        internal static readonly Dictionary<string, string> NpcArtFolder = new()
        {
            { "npc_brumdar",  "brumdar_ferro_quieto"  },
            { "npc_dagna",    "dagna_rocha_morna"      },
            { "npc_nimble",   "nimble_galhobaixo"      },
            { "npc_pip",      "pip_semente_solta"      },
            { "npc_ozzra",    "ozzra_fumacazul"        },
            { "npc_renko",    "renko_tres_sorrisos"    },
            { "npc_gruta",    "gruta_panela_funda"     },
            { "npc_mara",     "mara_vellum"            },
            { "npc_sylveth",  "sylveth"                },
            { "npc_yael",     "yael_noite_mansa"       },
            { "npc_thalindra","thalindra_veu_de_lua"   },
            { "npc_maelor",   "maelor_cinza"           },
            { "npc_velorin",  "anciao_velorin"         },
            { "npc_gurd",     "gurd_carvalho_torto"    },
            { "npc_hund",     "hund_carvalho_torto"    },
            { "npc_zrix",     "zrix_das_estradas"      },
            { "npc_savra",    "savra_escama_verde"      },
            { "npc_alaric",   "ser_alaric_veyr"        },
            { "npc_mirela",   "mirela_dos_lacos"       },
            { "npc_orlan",    "orlan_pouso_curto"      },
            { "npc_mella",    "mella_forno_quente"     },
            { "npc_eiran",    "eiran_valeclaro"        },
            { "npc_liora",    "liora_canta_rio"        },
            { "npc_sael",     "sael_mare_quieta"       },
            { "npc_hess",     "hess_couro_fundo"       },
            { "npc_tibbet",   "tibbet_vela_torta"      },
            { "npc_tovin",    "tovin_maos_de_selo"     },
        };

        /// <summary>
        /// Ponto de entrada chamado pelo orquestrador (RunStep em InicializarProjeto).
        /// Idempotente: re-atribui sempre que o PNG existir, pula se nao existir.
        /// </summary>
        public static void AssignAll()
        {
            var guids = AssetDatabase.FindAssets("t:NpcDataSO", new[] { NpcDataRoot });
            int skipped = 0;

            // Fase 1 (leitura, fora de batching): resolve NpcDataSO -> pngPath e so enfileira
            // reimport para os PNGs que ainda nao estao com os settings desejados.
            var toAssign = new List<(NpcDataSO npcData, string pngPath)>();
            var toReimport = new List<TextureImporter>();

            foreach (var guid in guids)
            {
                var soPath = AssetDatabase.GUIDToAssetPath(guid);
                var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(soPath);
                if (npcData == null) continue;

                if (!NpcArtFolder.TryGetValue(npcData.NpcId, out var artFolder))
                {
                    // NPC sem art_folder mapeado (ex.: npc_corvus, npc_vaalara_wanderer).
                    skipped++;
                    continue;
                }

                var pngPath = $"{SpriteRoot}/{artFolder}.png";
                if (!System.IO.File.Exists(
                    System.IO.Path.Combine(Application.dataPath, "..", pngPath)))
                {
                    Debug.LogWarning(
                        $"[AssignNpcBodySprites] PNG nao encontrado para {npcData.NpcId} " +
                        $"(art_folder={artFolder}); esperado em {pngPath}. Pule e gere o sprite primeiro.");
                    skipped++;
                    continue;
                }

                if (TryConfigureImporter(pngPath, out var importer))
                {
                    toReimport.Add(importer);
                }

                toAssign.Add((npcData, pngPath));
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

            // Fase 3 (leitura pos-import): so agora os resultados do reimport estao materializados.
            int assigned = 0;
            foreach (var (npcData, pngPath) in toAssign)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
                if (sprite == null)
                {
                    Debug.LogWarning(
                        $"[AssignNpcBodySprites] AssetDatabase.LoadAssetAtPath<Sprite> retornou null " +
                        $"para {pngPath} (NPC {npcData.NpcId}). Reimport pode ter falhado.");
                    skipped++;
                    continue;
                }

                var so = new SerializedObject(npcData);
                so.FindProperty("BodySprite").objectReferenceValue = sprite;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(npcData);
                assigned++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log(
                $"[AssignNpcBodySprites] Concluido. Atribuidos: {assigned} | " +
                $"Reimportados: {toReimport.Count} | Pulados/sem-PNG: {skipped}.");
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
            bool pivotOk = importer.spritePivot == new Vector2(0.5f, 0f);
            bool ppuOk = Mathf.Approximately(importer.spritePixelsPerUnit, 234f);
            bool filterOk = importer.filterMode == FilterMode.Point;
            bool mipmapOk = !importer.mipmapEnabled;
            bool alphaOk = importer.alphaIsTransparency;
            bool maxSizeOk = importer.maxTextureSize == 256;
            bool compressionOk = importer.textureCompression == TextureImporterCompression.Uncompressed;
            bool meshTypeOk = settings.spriteMeshType == SpriteMeshType.FullRect;
            bool alignmentOk = settings.spriteAlignment == (int)SpriteAlignment.BottomCenter;

            if (typeOk && modeOk && pivotOk && ppuOk && filterOk && mipmapOk && alphaOk
                && maxSizeOk && compressionOk && meshTypeOk && alignmentOk)
            {
                return false;
            }

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePivot         = new Vector2(0.5f, 0f);   // BottomCenter
            // Sprites sao re-cortados a 128px de altura (LANCZOS). A figura do player tem ~70px @ 128 PPU
            // @ scale 2.0 = ~1.09u. PPU 234 faz a figura de NPC de 128px render na MESMA altura do player
            // (128/234*2.0 ~= 1.09u), entao um NPC 1.0x = altura do player e as razoes por raca (anao 0.8,
            // halfling/goblin 0.7, orc 1.2) ficam exatas em relacao ao player.
            importer.spritePixelsPerUnit = 234f;
            importer.filterMode          = FilterMode.Point;
            importer.mipmapEnabled       = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize      = 256;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;

            settings.spriteMeshType   = SpriteMeshType.FullRect;
            settings.spriteAlignment  = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);

            return true;
        }
    }
}
