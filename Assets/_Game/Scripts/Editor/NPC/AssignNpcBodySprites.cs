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
        private static readonly Dictionary<string, string> NpcArtFolder = new()
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
            int assigned = 0;
            int skipped  = 0;

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

                ConfigureImporter(pngPath);

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
                $"[AssignNpcBodySprites] Concluido. Atribuidos: {assigned} | Pulados/sem-PNG: {skipped}.");
        }

        private static void ConfigureImporter(string pngPath)
        {
            var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePivot         = new Vector2(0.5f, 0f);   // BottomCenter
            importer.spritePixelsPerUnit = 128f;
            importer.filterMode          = FilterMode.Point;
            importer.mipmapEnabled       = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize      = 256;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType   = SpriteMeshType.FullRect;
            settings.spriteAlignment  = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }
    }
}
