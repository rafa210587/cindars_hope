using System.IO;
using UnityEditor;
using UnityEngine;
using CindarsHope.Enemy;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    /// <summary>
    /// fable_81 — Gera packs temáticos curados por banda usando o roster expandido (fable_80).
    /// Cada pack tem: líder (IsRequired=true), flankers (PackFlanker/IsRequired), apoio (caster/ranged),
    /// fodder (swarm, opcional por weight), MaxTotalEnemies coerente com MinimumRoomSize.
    /// Minibosses/bosses NUNCA incluídos aqui — ficam nos sistemas de gate/wandering.
    ///
    /// NÃO expõe [MenuItem] próprio: toda geração de assets do projeto roda apenas via os 3
    /// comandos canônicos (rule editor-generation-orchestration). Este gerador é invocado como
    /// um passo de "CindarsHope/Inicializar Projeto" (CindarsHopeMenu.InicializarProjeto).
    /// </summary>
    public static class GenerateThematicPacksFable81
    {
        private const string PacksFolder = "Assets/_Game/Data/EnemySpawn/Packs";

        public static void GeneratePacks()
        {
            EnsureFolder(PacksFolder);

            int created = 0;
            int updated = 0;

            foreach (var def in BuildPackDefinitions())
            {
                string assetPath = $"{PacksFolder}/{def.PackId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<EnemySpawnPackSO>(assetPath);

                if (existing == null)
                {
                    existing = ScriptableObject.CreateInstance<EnemySpawnPackSO>();
                    ApplyDef(existing, def);
                    AssetDatabase.CreateAsset(existing, assetPath);
                    created++;
                }
                else
                {
                    ApplyDef(existing, def);
                    EditorUtility.SetDirty(existing);
                    updated++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[fable_81] GenerateThematicPacksFable81: {created} criados, {updated} atualizados em '{PacksFolder}'.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // DEFINIÇÕES DE PACK
        // Nomenclatura de papéis:
        //   IsRequired=true  → líder / flanker / apoio obrigatório
        //   IsRequired=false → fodder / apoio opcional (por weight)
        // MinimumRoomSize: Small=1, Medium=2, Large=3, Arena=4
        // ─────────────────────────────────────────────────────────────────────
        private static PackDef[] BuildPackDefinitions()
        {
            return new[]
            {
                // ── BAND 1 STONE (1-10) ─────────────────────────────────────────────────────

                // Pack 1: Bando do Bandit (líder: bandit_scavenger, flankers: goblin_scrounger,
                //         apoio: kobold_sentry, fodder: stone_burrower)
                new PackDef
                {
                    PackId = "pack_f81_stone_bandit_crew",
                    DisplayName = "Stone Bandit Crew",
                    CaveLevelMin = 3, CaveLevelMax = 8,
                    BiomeTags = new[] { "stone" },
                    EnvironmentTags = new[] { "stone" },
                    Weight = 8,
                    MinimumRoomSize = EnemyRoomSizeClass.Small,
                    MaxTotalEnemies = 6,
                    Entries = new[]
                    {
                        // Líder: bandit_scavenger (RetreatAndCall — chama reforcos)
                        Entry("enemy_bandit_scavenger", 1, 1, weight: 1, required: true),
                        // Flankers: goblin_scrounger (PackFlanker)
                        Entry("enemy_goblin_scrounger", 1, 2, weight: 1, required: true),
                        // Apoio ranged: kobold_sentry (KiteRanged, alerta o pack)
                        Entry("enemy_kobold_sentry", 1, 2, weight: 1, required: true),
                        // Fodder opcional: stone_burrower (emboscador de abertura)
                        Entry("enemy_stone_burrower", 0, 1, weight: 6, required: false),
                    },
                },

                // Pack 2: Matilha Grimfang (líder: grimfang_packleader, fodder: glimmer_centipede)
                new PackDef
                {
                    PackId = "pack_f81_stone_grimfang_pack",
                    DisplayName = "Grimfang Pack",
                    CaveLevelMin = 4, CaveLevelMax = 9,
                    BiomeTags = new[] { "stone" },
                    EnvironmentTags = new[] { "stone" },
                    Weight = 7,
                    MinimumRoomSize = EnemyRoomSizeClass.Small,
                    MaxTotalEnemies = 8,
                    Entries = new[]
                    {
                        // Líder com PackLeader (howl buffa)
                        Entry("enemy_grimfang_packleader", 1, 1, weight: 1, required: true),
                        // Flankers: cracked_golem_shard (guardam o líder)
                        Entry("enemy_cracked_golem_shard", 1, 2, weight: 1, required: true),
                        // Fodder swarm: glimmer_centipede (preenchem volume do pack)
                        Entry("enemy_glimmer_centipede", 2, 4, weight: 8, required: false),
                    },
                },

                // ── BAND 2 FUNGAL (11-25) ───────────────────────────────────────────────────

                // Pack 3: Kaand Warband v2 com fable_80 (líder: orc_drummer, flankers: goblin_shredder,
                //         tanque: mycelial_warden, fodder: rotcap_cluster)
                new PackDef
                {
                    PackId = "pack_f81_fungal_kaand_drumline",
                    DisplayName = "Kaand Drum Line",
                    CaveLevelMin = 13, CaveLevelMax = 20,
                    BiomeTags = new[] { "fungal" },
                    EnvironmentTags = new[] { "fungal" },
                    Weight = 8,
                    MinimumRoomSize = EnemyRoomSizeClass.Small,
                    MaxTotalEnemies = 7,
                    Entries = new[]
                    {
                        // Líder: orc_drummer (RetreatAndCall, frenesi +15% aliados)
                        Entry("enemy_orc_drummer", 1, 1, weight: 1, required: true),
                        // Flankers: goblin_shredder (PackFlanker)
                        Entry("enemy_goblin_shredder", 1, 3, weight: 1, required: true),
                        // Tanque: mycelial_warden (ancora e regenera perto do líder)
                        Entry("enemy_mycelial_warden", 0, 1, weight: 5, required: false),
                        // Hazard estacionário: rotcap_cluster
                        Entry("enemy_rotcap_cluster", 0, 2, weight: 4, required: false),
                    },
                },

                // Pack 4: Colônia Fungal Densa (lider: fungal_spreader, apoio: spore_amalgam)
                new PackDef
                {
                    PackId = "pack_f81_fungal_mycel_horde",
                    DisplayName = "Mycel Horde",
                    CaveLevelMin = 17, CaveLevelMax = 24,
                    BiomeTags = new[] { "fungal" },
                    EnvironmentTags = new[] { "fungal" },
                    Weight = 6,
                    MinimumRoomSize = EnemyRoomSizeClass.Medium,
                    MaxTotalEnemies = 9,
                    Entries = new[]
                    {
                        // Líder/caster: fungal_spreader (FloatingSlow, zonas de slow)
                        Entry("enemy_fungal_spreader", 1, 1, weight: 1, required: true),
                        // Bruto: spore_amalgam (TankSlowPush, pressão)
                        Entry("enemy_spore_amalgam", 1, 2, weight: 1, required: true),
                        // Cobertura: mycelial_warden
                        Entry("enemy_mycelial_warden", 0, 1, weight: 5, required: false),
                        // Fodder: rotcap_cluster (estacionário, cobre flancos)
                        Entry("enemy_rotcap_cluster", 1, 3, weight: 6, required: false),
                    },
                },

                // ── BAND 3 ICE (26-40) ──────────────────────────────────────────────────────

                // Pack 5: Culto do Frio (líder: coldcult_preacher, flankers: veilkin_iceblade,
                //         apoio: frostshard_wisp, fodder: glacier_tick)
                new PackDef
                {
                    PackId = "pack_f81_ice_coldcult_sermon",
                    DisplayName = "Cold Cult Sermon",
                    CaveLevelMin = 30, CaveLevelMax = 38,
                    BiomeTags = new[] { "ice" },
                    EnvironmentTags = new[] { "ice" },
                    Weight = 8,
                    MinimumRoomSize = EnemyRoomSizeClass.Small,
                    MaxTotalEnemies = 7,
                    Entries = new[]
                    {
                        // Líder/caster: coldcult_preacher (RetreatAndCall, buffa pack com canto)
                        Entry("enemy_coldcult_preacher", 1, 1, weight: 1, required: true),
                        // Flankers técnicos: veilkin_iceblade (circula enquanto líder ataca)
                        Entry("enemy_veilkin_iceblade", 1, 2, weight: 1, required: true),
                        // Apoio orbitador: frostshard_wisp (drena stamina)
                        Entry("enemy_frostshard_wisp", 1, 2, weight: 1, required: true),
                        // Fodder: glacier_tick (cobre conjuradores, drena stamina)
                        Entry("enemy_glacier_tick", 0, 3, weight: 7, required: false),
                    },
                },

                // Pack 6: Matilha de Gelo (líder: crystal_hound, tanque: frostbound_revenant)
                new PackDef
                {
                    PackId = "pack_f81_ice_crystal_hunt",
                    DisplayName = "Crystal Hunt",
                    CaveLevelMin = 28, CaveLevelMax = 38,
                    BiomeTags = new[] { "ice" },
                    EnvironmentTags = new[] { "ice" },
                    Weight = 7,
                    MinimumRoomSize = EnemyRoomSizeClass.Medium,
                    MaxTotalEnemies = 8,
                    Entries = new[]
                    {
                        // Líder: crystal_hound (PackLeader, slow 1s no bote)
                        Entry("enemy_crystal_hound", 1, 1, weight: 1, required: true),
                        // Tanque: frostbound_revenant (ressurge, pressão)
                        Entry("enemy_frostbound_revenant", 1, 1, weight: 1, required: true),
                        // Apoio: frostshard_wisp (drena stamina de alvos lentos)
                        Entry("enemy_frostshard_wisp", 0, 2, weight: 5, required: false),
                        // Fodder: glacier_tick (swarm de cobertura)
                        Entry("enemy_glacier_tick", 1, 3, weight: 7, required: false),
                    },
                },

                // ── BAND 4 FIRE (41-55) ─────────────────────────────────────────────────────

                // Pack 7: Ninho de Wyrmling (líder: sulfur_wyrmling, flankers: ember_hound,
                //         tanque: emberroot_horror, fodder: magma_slug)
                new PackDef
                {
                    PackId = "pack_f81_fire_wyrm_nest",
                    DisplayName = "Wyrm Nest",
                    CaveLevelMin = 45, CaveLevelMax = 53,
                    BiomeTags = new[] { "fire" },
                    EnvironmentTags = new[] { "fire" },
                    Weight = 8,
                    MinimumRoomSize = EnemyRoomSizeClass.Medium,
                    MaxTotalEnemies = 8,
                    Entries = new[]
                    {
                        // Líder/ranged: sulfur_wyrmling (guarda o ninho, cone de sopro)
                        Entry("enemy_sulfur_wyrmling", 1, 1, weight: 1, required: true),
                        // Flankers: ember_hound (PackFlanker, explodem ao morrer)
                        Entry("enemy_ember_hound", 1, 2, weight: 1, required: true),
                        // Tanque-âncora: emberroot_horror (defende a fenda de lava)
                        Entry("enemy_emberroot_horror", 0, 1, weight: 5, required: false),
                        // Bloqueador: magma_slug (hazard de trilha, lento)
                        Entry("enemy_magma_slug", 0, 1, weight: 4, required: false),
                    },
                },

                // Pack 8: Círculo de Fogo do Veil (líder: veilkin_pyrecaller, apoio: veilkin_pyromancer,
                //         construto: steam_golem_proto) — salas grandes
                new PackDef
                {
                    PackId = "pack_f81_fire_veil_circle",
                    DisplayName = "Veil Fire Circle",
                    CaveLevelMin = 46, CaveLevelMax = 54,
                    BiomeTags = new[] { "fire" },
                    EnvironmentTags = new[] { "fire" },
                    Weight = 6,
                    MinimumRoomSize = EnemyRoomSizeClass.Large,
                    MaxTotalEnemies = 6,
                    Entries = new[]
                    {
                        // Líder/caster: veilkin_pyrecaller (RetreatAndCall, chuva de brasas)
                        Entry("enemy_veilkin_pyrecaller", 1, 1, weight: 1, required: true),
                        // Apoio: veilkin_pyromancer (Fire Wall corta rotas)
                        Entry("enemy_veilkin_pyromancer", 1, 1, weight: 1, required: true),
                        // Construto pesado: steam_golem_proto (guarda a câmara)
                        Entry("enemy_steam_golem_proto", 0, 1, weight: 4, required: false),
                        // Fodder: ember_scorpion (carga perseguição)
                        Entry("enemy_ember_scorpion", 0, 2, weight: 5, required: false),
                    },
                },

                // ── BAND 5 RUINS (56-70) ────────────────────────────────────────────────────

                // Pack 9: Guarda das Ruínas (líder: gravedelver_runepriest, tanque: mirror_golem,
                //         ranged: rune_sentry_mk2, fodder: ninrorin_echo_warrior)
                new PackDef
                {
                    PackId = "pack_f81_ruins_archive_guard",
                    DisplayName = "Archive Guard",
                    CaveLevelMin = 60, CaveLevelMax = 67,
                    BiomeTags = new[] { "ruins" },
                    EnvironmentTags = new[] { "ruins" },
                    Weight = 8,
                    MinimumRoomSize = EnemyRoomSizeClass.Medium,
                    MaxTotalEnemies = 7,
                    Entries = new[]
                    {
                        // Líder/caster: gravedelver_runepriest (barreira runica, raio arcano)
                        Entry("enemy_gravedelver_runepriest", 1, 1, weight: 1, required: true),
                        // Tanque: mirror_golem (reflete projéteis)
                        Entry("enemy_mirror_golem", 1, 1, weight: 1, required: true),
                        // Cobertura ranged: rune_sentry_mk2 (torre estacionária)
                        Entry("enemy_rune_sentry_mk2", 0, 2, weight: 5, required: false),
                        // Flankers: ninrorin_echo_warrior (circula, flurry fantasma)
                        Entry("enemy_ninrorin_echo_warrior", 0, 2, weight: 6, required: false),
                    },
                },

                // Pack 10: Horda da Ruína com Runic Warbeast — salas grandes
                new PackDef
                {
                    PackId = "pack_f81_ruins_runic_warbeast_horde",
                    DisplayName = "Runic Warbeast Horde",
                    CaveLevelMin = 62, CaveLevelMax = 69,
                    BiomeTags = new[] { "ruins" },
                    EnvironmentTags = new[] { "ruins" },
                    Weight = 6,
                    MinimumRoomSize = EnemyRoomSizeClass.Large,
                    MaxTotalEnemies = 8,
                    Entries = new[]
                    {
                        // Líder: runic_warbeast (ChargeLine, knockback forte)
                        Entry("enemy_runic_warbeast", 1, 1, weight: 1, required: true),
                        // Flankers espectrais: ninrorin_echo_warrior
                        Entry("enemy_ninrorin_echo_warrior", 1, 2, weight: 1, required: true),
                        // Apoio: gravedelver_runepriest (buffer do pack)
                        Entry("enemy_gravedelver_runepriest", 0, 1, weight: 5, required: false),
                        // Armadilha: chromatic_hoardling (mimico junto ao tesouro)
                        Entry("enemy_chromatic_hoardling", 0, 1, weight: 3, required: false),
                    },
                },

                // ── BAND 6 DEEP (71-85) ─────────────────────────────────────────────────────

                // Pack 11: Coro da Escuridão (líder: whisper_of_veyraath, apoio: nyx_shade_elemental,
                //          flankers: abyssal_lurker, fodder: void_brood_larva)
                new PackDef
                {
                    PackId = "pack_f81_deep_darkness_choir",
                    DisplayName = "Darkness Choir",
                    CaveLevelMin = 75, CaveLevelMax = 83,
                    BiomeTags = new[] { "deep" },
                    EnvironmentTags = new[] { "deep" },
                    Weight = 8,
                    MinimumRoomSize = EnemyRoomSizeClass.Medium,
                    MaxTotalEnemies = 8,
                    Entries = new[]
                    {
                        // Líder: whisper_of_veyraath (caster arcano de alto nível)
                        Entry("enemy_whisper_of_veyraath", 1, 1, weight: 1, required: true),
                        // Controlador de hazard: nyx_shade_elemental (noturno, HazardLure)
                        Entry("enemy_nyx_shade_elemental", 1, 1, weight: 1, required: true),
                        // Flankers: abyssal_lurker (emboscadores)
                        Entry("enemy_abyssal_lurker", 1, 2, weight: 1, required: true),
                        // Fodder larva: void_brood_larva (swarm que cobre a retirada)
                        Entry("enemy_void_brood_larva", 0, 3, weight: 7, required: false),
                    },
                },

                // Pack 12: Wyrm Dracônico (líder: corrupt_pseudowyrm, apoio: corrupt_pseudowyrm x2) — arena
                new PackDef
                {
                    PackId = "pack_f81_deep_draconic_corrupt_nest",
                    DisplayName = "Corrupted Draconic Nest",
                    CaveLevelMin = 76, CaveLevelMax = 83,
                    BiomeTags = new[] { "deep" },
                    EnvironmentTags = new[] { "deep" },
                    Weight = 5,
                    MinimumRoomSize = EnemyRoomSizeClass.Large,
                    MaxTotalEnemies = 6,
                    Entries = new[]
                    {
                        // Líder: corrupt_pseudowyrm (orbitador caster)
                        Entry("enemy_corrupt_pseudowyrm", 1, 1, weight: 1, required: true),
                        // Flankers: abyssal_lurker (flanqueiam enquanto wyrm orbita)
                        Entry("enemy_abyssal_lurker", 1, 2, weight: 1, required: true),
                        // Apoio: mindbound_thrall (subserviente ao wyrm)
                        Entry("enemy_mindbound_thrall", 0, 2, weight: 5, required: false),
                    },
                },

                // ── BAND 7 VOID (86-100) ────────────────────────────────────────────────────

                // Pack 13: Coro do Vazio (líder: dread_chorister, tanque: void_tendril_watcher,
                //          assassino: veilkin_voidknight, render: reality_render) — arena
                new PackDef
                {
                    PackId = "pack_f81_void_dread_chorus",
                    DisplayName = "Dread Chorus",
                    CaveLevelMin = 87, CaveLevelMax = 95,
                    BiomeTags = new[] { "void" },
                    EnvironmentTags = new[] { "void" },
                    Weight = 8,
                    MinimumRoomSize = EnemyRoomSizeClass.Large,
                    MaxTotalEnemies = 7,
                    Entries = new[]
                    {
                        // Líder/caster: dread_chorister (litania de medo, orbita)
                        Entry("enemy_dread_chorister", 1, 1, weight: 1, required: true),
                        // Âncora/tanque: void_tendril_watcher (campo de tentáculos)
                        Entry("enemy_void_tendril_watcher", 1, 1, weight: 1, required: true),
                        // Elite duelista: veilkin_voidknight (circula e carrega)
                        Entry("enemy_veilkin_voidknight", 0, 1, weight: 5, required: false),
                        // Presença: reality_render (blink+carga)
                        Entry("enemy_reality_render", 0, 1, weight: 5, required: false),
                    },
                },

                // Pack 14: Vanguarda do Vazio (líder: veilkin_blademaster, fodder: void_husk)
                new PackDef
                {
                    PackId = "pack_f81_void_vanguard",
                    DisplayName = "Void Vanguard",
                    CaveLevelMin = 86, CaveLevelMax = 94,
                    BiomeTags = new[] { "void" },
                    EnvironmentTags = new[] { "void" },
                    Weight = 7,
                    MinimumRoomSize = EnemyRoomSizeClass.Medium,
                    MaxTotalEnemies = 9,
                    Entries = new[]
                    {
                        // Líder: veilkin_blademaster
                        Entry("enemy_veilkin_blademaster", 1, 1, weight: 1, required: true),
                        // Flankers regulares; minibosses ficam fora das entradas de pack comum.
                        Entry("enemy_starfall_remnant", 1, 2, weight: 1, required: true),
                        // Fodder: void_husk (swarm de cascos)
                        Entry("enemy_void_husk", 2, 4, weight: 8, required: false),
                        // Apoio: dread_chorister (opcional — aplica medo)
                        Entry("enemy_dread_chorister", 0, 1, weight: 4, required: false),
                    },
                },
            };
        }

        // ─────────────────────────────────────────────────────────────────────

        private static EnemySpawnPackEntry Entry(string enemyId, int min, int max, int weight, bool required)
        {
            return new EnemySpawnPackEntry
            {
                EnemyId = enemyId,
                MinCount = min,
                MaxCount = max,
                Weight = weight,
                IsRequired = required,
                RequiresUnlockedFactionLock = string.Empty,
            };
        }

        private static void ApplyDef(EnemySpawnPackSO so, PackDef def)
        {
            so.name = def.PackId;
            so.PackId = def.PackId;
            so.DisplayName = def.DisplayName;
            so.CaveLevelMin = def.CaveLevelMin;
            so.CaveLevelMax = def.CaveLevelMax;
            so.BiomeTags = def.BiomeTags;
            so.EnvironmentTags = def.EnvironmentTags;
            so.RequiredFactionIds = new string[0];
            so.Weight = def.Weight;
            so.MinimumRoomSize = def.MinimumRoomSize;
            so.MaxTotalEnemies = def.MaxTotalEnemies;
            so.IsEnabled = true;
            so.Entries = def.Entries;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets";
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        // ─── DTO interno ──────────────────────────────────────────────────────
        private class PackDef
        {
            public string PackId;
            public string DisplayName;
            public int CaveLevelMin;
            public int CaveLevelMax;
            public string[] BiomeTags = new string[0];
            public string[] EnvironmentTags = new string[0];
            public int Weight = 1;
            public EnemyRoomSizeClass MinimumRoomSize = EnemyRoomSizeClass.Small;
            public int MaxTotalEnemies = 6;
            public EnemySpawnPackEntry[] Entries = new EnemySpawnPackEntry[0];
        }
    }
}
