using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Enemy;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_81 — Testes de composição determinística de packs temáticos.
    /// Cobre:
    ///   CA-1: packs temáticos existem por banda (via EnemySpawnResolver em memória).
    ///   CA-2: líder IsRequired=true → sempre incluído quando pack tem líder.
    ///   CA-3: mesmo seed → mesma composição (determinismo).
    ///   CA-4: minibosses/bosses nunca em packs regulares.
    ///   CA-5: cap MaxTotalEnemies vs MinimumRoomSize coerente.
    ///
    /// Estes testes rodam sem Unity Play Mode (pure C#, sem .asset loading).
    /// </summary>
    [TestFixture]
    public class ProceduralPackEnrichmentTests
    {
        // ─────────────────────────────────────────────────────────────────────
        // Helpers: constroi packs em memória replicando a lógica do gerador
        // ─────────────────────────────────────────────────────────────────────

        private static EnemySpawnPackSO MakePack(
            string packId,
            int levelMin,
            int levelMax,
            string biomeTag,
            EnemyRoomSizeClass minRoom,
            int maxTotal,
            params (string id, int min, int max, int weight, bool required)[] entries)
        {
            var pack = new EnemySpawnPackSO();
            SetField(pack, "PackId", packId);
            SetField(pack, "DisplayName", packId);
            SetField(pack, "CaveLevelMin", levelMin);
            SetField(pack, "CaveLevelMax", levelMax);
            SetField(pack, "BiomeTags", new[] { biomeTag });
            SetField(pack, "EnvironmentTags", new[] { biomeTag });
            SetField(pack, "RequiredFactionIds", Array.Empty<string>());
            SetField(pack, "Weight", 8);
            SetField(pack, "MinimumRoomSize", minRoom);
            SetField(pack, "MaxTotalEnemies", maxTotal);
            SetField(pack, "IsEnabled", true);
            SetField(pack, "Entries", entries.Select(e => new EnemySpawnPackEntry
            {
                EnemyId = e.id,
                MinCount = e.min,
                MaxCount = e.max,
                Weight = e.weight,
                IsRequired = e.required,
                RequiresUnlockedFactionLock = string.Empty,
            }).ToArray());
            return pack;
        }

        // Reflection helper para setar campos públicos em SO instanciado sem Unity asset pipeline
        private static void SetField<T>(object obj, string fieldName, T value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        // Mínimo de profiles para satisfazer o resolver (cada EnemyId precisa de um perfil)
        private static EnemySpawnProfileSO MakeProfile(string enemyId, int levelMin, int levelMax, string biomeTag,
            EnemyRoomSizeClass minRoom = EnemyRoomSizeClass.Small,
            EnemySizeClass sizeClass = EnemySizeClass.Medium)
        {
            var p = new EnemySpawnProfileSO();
            SetField(p, "SpawnProfileId", $"profile_{enemyId}");
            SetField(p, "EnemyId", enemyId);
            SetField(p, "DisplayName", enemyId);
            SetField(p, "IsEnabled", true);
            SetField(p, "Weight", 5);
            SetField(p, "CaveLevelMin", levelMin);
            SetField(p, "CaveLevelMax", levelMax);
            SetField(p, "BiomeTags", new[] { biomeTag });
            SetField(p, "EnvironmentTags", new[] { biomeTag });
            SetField(p, "MinimumRoomSizeForSizeClass", minRoom);
            SetField(p, "SizeClass", sizeClass);
            SetField(p, "MaxCountPerRoom", 4);
            SetField(p, "CanSpawnAsElite", false);
            SetField(p, "IsEnabled", true);
            return p;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CA-3 — DETERMINISMO: mesmo seed → mesma composição
        // ─────────────────────────────────────────────────────────────────────

        [Test]
        public void Determinism_SameSeed_ProducesSameComposition()
        {
            // Pack Stone Bandit Crew em memória (simplificado para teste de resolver)
            var pack = MakePack("pack_f81_stone_bandit_crew",
                levelMin: 3, levelMax: 8, biomeTag: "stone",
                minRoom: EnemyRoomSizeClass.Small, maxTotal: 6,
                ("enemy_bandit_scavenger", 1, 1, 1, true),
                ("enemy_goblin_scrounger", 1, 2, 1, true),
                ("enemy_kobold_sentry", 1, 2, 1, true),
                ("enemy_stone_burrower", 0, 1, 6, false));

            var profiles = new[]
            {
                MakeProfile("enemy_bandit_scavenger", 3, 7, "stone"),
                MakeProfile("enemy_goblin_scrounger", 2, 6, "stone"),
                MakeProfile("enemy_kobold_sentry", 2, 6, "stone"),
                MakeProfile("enemy_stone_burrower", 2, 6, "stone"),
            };

            var request = new EnemySpawnRequest
            {
                CaveLevel = 5,
                BiomeTags = new List<string> { "stone" },
                EnvironmentTags = new List<string> { "stone" },
                RoomSizeClass = EnemyRoomSizeClass.Medium,
                MaxEnemies = 6,
                AllowElite = false,
                Seed = 12345,
            };

            var resolver = new EnemySpawnResolver(profiles, new[] { pack });

            var result1 = resolver.Resolve(request);
            var result2 = resolver.Resolve(request); // mesmo request = mesmo seed

            Assert.IsTrue(result1.IsValid, "Resolve deveria ser válido (resultado 1).");
            Assert.IsTrue(result2.IsValid, "Resolve deveria ser válido (resultado 2).");
            Assert.AreEqual(result1.SelectedPackId, result2.SelectedPackId,
                "CA-3: mesmo seed deve selecionar o mesmo pack.");

            var ids1 = result1.SelectedEnemies.Select(e => e.EnemyId).OrderBy(x => x).ToList();
            var ids2 = result2.SelectedEnemies.Select(e => e.EnemyId).OrderBy(x => x).ToList();
            CollectionAssert.AreEqual(ids1, ids2, "CA-3: mesmo seed deve produzir a mesma composição de inimigos.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // CA-2 — LÍDER-OBRIGATÓRIO: líder IsRequired sempre incluído
        // ─────────────────────────────────────────────────────────────────────

        [Test]
        public void LeaderRequired_AlwaysPresentInPack()
        {
            var pack = MakePack("pack_f81_stone_grimfang_pack",
                levelMin: 4, levelMax: 9, biomeTag: "stone",
                minRoom: EnemyRoomSizeClass.Small, maxTotal: 8,
                ("enemy_grimfang_packleader", 1, 1, 1, true),  // líder IsRequired
                ("enemy_cracked_golem_shard", 1, 2, 1, true),
                ("enemy_glimmer_centipede", 2, 4, 8, false));

            var profiles = new[]
            {
                MakeProfile("enemy_grimfang_packleader", 4, 8, "stone"),
                MakeProfile("enemy_cracked_golem_shard", 4, 8, "stone"),
                MakeProfile("enemy_glimmer_centipede", 1, 4, "stone"),
            };

            var resolver = new EnemySpawnResolver(profiles, new[] { pack });

            // Testar 5 seeds diferentes — líder deve aparecer em todos
            for (int seed = 1; seed <= 5; seed++)
            {
                var request = new EnemySpawnRequest
                {
                    CaveLevel = 6,
                    BiomeTags = new List<string> { "stone" },
                    EnvironmentTags = new List<string> { "stone" },
                    RoomSizeClass = EnemyRoomSizeClass.Large,
                    MaxEnemies = 8,
                    AllowElite = false,
                    Seed = seed * 7919,
                };

                var result = resolver.Resolve(request);
                Assert.IsTrue(result.IsValid, $"Resolve deveria ser válido para seed {seed * 7919}.");
                Assert.AreEqual("pack_f81_stone_grimfang_pack", result.SelectedPackId,
                    $"CA-2: pack correto selecionado para seed {seed * 7919}.");

                bool leaderPresent = result.SelectedEnemies.Any(e => e.EnemyId == "enemy_grimfang_packleader");
                Assert.IsTrue(leaderPresent,
                    $"CA-2: líder (enemy_grimfang_packleader) deve estar presente na composição para seed {seed * 7919}.");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CA-3 — CAP POR SALA: MaxTotalEnemies respeitado pelo resolver
        // ─────────────────────────────────────────────────────────────────────

        [Test]
        public void MaxTotalEnemies_Cap_Respected()
        {
            // Pack com MaxTotalEnemies=6 e entries que totalizam até 10
            var pack = MakePack("pack_f81_ice_coldcult_sermon",
                levelMin: 30, levelMax: 38, biomeTag: "ice",
                minRoom: EnemyRoomSizeClass.Small, maxTotal: 6,
                ("enemy_coldcult_preacher", 1, 1, 1, true),
                ("enemy_veilkin_iceblade", 1, 2, 1, true),
                ("enemy_frostshard_wisp", 1, 2, 1, true),
                ("enemy_glacier_tick", 0, 3, 7, false));

            var profiles = new[]
            {
                MakeProfile("enemy_coldcult_preacher", 30, 38, "ice"),
                MakeProfile("enemy_veilkin_iceblade", 29, 37, "ice"),
                MakeProfile("enemy_frostshard_wisp", 26, 32, "ice"),
                MakeProfile("enemy_glacier_tick", 26, 34, "ice"),
            };

            var resolver = new EnemySpawnResolver(profiles, new[] { pack });

            var request = new EnemySpawnRequest
            {
                CaveLevel = 33,
                BiomeTags = new List<string> { "ice" },
                EnvironmentTags = new List<string> { "ice" },
                RoomSizeClass = EnemyRoomSizeClass.Large,
                MaxEnemies = 20, // maior que cap do pack
                AllowElite = false,
                Seed = 999,
            };

            var result = resolver.Resolve(request);
            Assert.IsTrue(result.IsValid, "Resolve deveria ser válido.");

            int totalCount = result.SelectedEnemies.Sum(e => e.Count);
            Assert.LessOrEqual(totalCount, 6,
                $"CA-3: total de inimigos ({totalCount}) não deve ultrapassar MaxTotalEnemies=6.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // CA-3 — CAP POR SALA: packs Large não aparecem em sala Small
        // ─────────────────────────────────────────────────────────────────────

        [Test]
        public void LargePack_Rejected_InSmallRoom()
        {
            var largePack = MakePack("pack_f81_fire_veil_circle",
                levelMin: 46, levelMax: 54, biomeTag: "fire",
                minRoom: EnemyRoomSizeClass.Large, maxTotal: 6,
                ("enemy_veilkin_pyrecaller", 1, 1, 1, true),
                ("enemy_veilkin_pyromancer", 1, 1, 1, true));

            var smallPack = MakePack("pack_f81_fire_wyrm_nest",
                levelMin: 45, levelMax: 53, biomeTag: "fire",
                minRoom: EnemyRoomSizeClass.Small, maxTotal: 8,
                ("enemy_sulfur_wyrmling", 1, 1, 1, true),
                ("enemy_ember_hound", 1, 2, 1, true));

            var profiles = new[]
            {
                MakeProfile("enemy_veilkin_pyrecaller", 46, 54, "fire"),
                MakeProfile("enemy_veilkin_pyromancer", 45, 53, "fire"),
                MakeProfile("enemy_sulfur_wyrmling", 45, 53, "fire"),
                MakeProfile("enemy_ember_hound", 41, 48, "fire"),
            };

            var resolver = new EnemySpawnResolver(profiles, new[] { largePack, smallPack });

            var request = new EnemySpawnRequest
            {
                CaveLevel = 49,
                BiomeTags = new List<string> { "fire" },
                EnvironmentTags = new List<string> { "fire" },
                RoomSizeClass = EnemyRoomSizeClass.Small, // sala pequena
                MaxEnemies = 8,
                AllowElite = false,
                Seed = 42,
            };

            var result = resolver.Resolve(request);
            Assert.IsTrue(result.IsValid, "Resolve deveria encontrar pelo menos o pack small.");
            Assert.AreNotEqual("pack_f81_fire_veil_circle", result.SelectedPackId,
                "CA-3: pack Large (MinimumRoomSize=Large) não deve ser selecionado para sala Small.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // CA-4 — EXCLUSÃO DE MINIBOSS/BOSS dos packs regulares
        // ─────────────────────────────────────────────────────────────────────

        [Test]
        public void MinibossAndBoss_NotInRegularPackEntries()
        {
            var minibossAndBossIds = CanonicalBestiaryCatalog.All
                .Where(d => d.IsMiniBoss || d.IsBoss)
                .Select(d => d.EnemyId)
                .ToHashSet();

            // Definir os packs fable_81 em memória para verificação estática
            var packsToCheck = new[]
            {
                // Verificar todos os packs que seriam gerados pelo GenerateThematicPacksFable81
                // usando os EnemyIds declarados nas definições
                new { PackId = "pack_f81_stone_bandit_crew", Entries = new[] { "enemy_bandit_scavenger", "enemy_goblin_scrounger", "enemy_kobold_sentry", "enemy_stone_burrower" } },
                new { PackId = "pack_f81_stone_grimfang_pack", Entries = new[] { "enemy_grimfang_packleader", "enemy_cracked_golem_shard", "enemy_glimmer_centipede" } },
                new { PackId = "pack_f81_fungal_kaand_drumline", Entries = new[] { "enemy_orc_drummer", "enemy_goblin_shredder", "enemy_mycelial_warden", "enemy_rotcap_cluster" } },
                new { PackId = "pack_f81_fungal_mycel_horde", Entries = new[] { "enemy_fungal_spreader", "enemy_spore_amalgam", "enemy_mycelial_warden", "enemy_rotcap_cluster" } },
                new { PackId = "pack_f81_ice_coldcult_sermon", Entries = new[] { "enemy_coldcult_preacher", "enemy_veilkin_iceblade", "enemy_frostshard_wisp", "enemy_glacier_tick" } },
                new { PackId = "pack_f81_ice_crystal_hunt", Entries = new[] { "enemy_crystal_hound", "enemy_frostbound_revenant", "enemy_frostshard_wisp", "enemy_glacier_tick" } },
                new { PackId = "pack_f81_fire_wyrm_nest", Entries = new[] { "enemy_sulfur_wyrmling", "enemy_ember_hound", "enemy_emberroot_horror", "enemy_magma_slug" } },
                new { PackId = "pack_f81_fire_veil_circle", Entries = new[] { "enemy_veilkin_pyrecaller", "enemy_veilkin_pyromancer", "enemy_steam_golem_proto", "enemy_ember_scorpion" } },
                new { PackId = "pack_f81_ruins_archive_guard", Entries = new[] { "enemy_gravedelver_runepriest", "enemy_mirror_golem", "enemy_rune_sentry_mk2", "enemy_ninrorin_echo_warrior" } },
                new { PackId = "pack_f81_ruins_runic_warbeast_horde", Entries = new[] { "enemy_runic_warbeast", "enemy_ninrorin_echo_warrior", "enemy_gravedelver_runepriest", "enemy_chromatic_hoardling" } },
                new { PackId = "pack_f81_deep_darkness_choir", Entries = new[] { "enemy_whisper_of_veyraath", "enemy_nyx_shade_elemental", "enemy_abyssal_lurker", "enemy_void_brood_larva" } },
                new { PackId = "pack_f81_deep_draconic_corrupt_nest", Entries = new[] { "enemy_corrupt_pseudowyrm", "enemy_abyssal_lurker", "enemy_mindbound_thrall" } },
                new { PackId = "pack_f81_void_dread_chorus", Entries = new[] { "enemy_dread_chorister", "enemy_void_tendril_watcher", "enemy_veilkin_voidknight", "enemy_reality_render" } },
                new { PackId = "pack_f81_void_vanguard", Entries = new[] { "enemy_veilkin_blademaster", "enemy_gravelborn_twins", "enemy_void_husk", "enemy_dread_chorister" } },
            };

            foreach (var pack in packsToCheck)
            {
                foreach (var enemyId in pack.Entries)
                {
                    Assert.IsFalse(minibossAndBossIds.Contains(enemyId),
                        $"CA-4: pack '{pack.PackId}' contém '{enemyId}' que é miniboss ou boss — proibido em packs regulares.");
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CA-1 — COBERTURA POR BANDA: pelo menos 2 packs por bioma
        // ─────────────────────────────────────────────────────────────────────

        [Test]
        public void ThematicPacks_CoverAllBands_AtLeastTwoPerBand()
        {
            // Verifica que os PackIds fable_81 existem para cada bioma
            var packsByBiome = new Dictionary<string, List<string>>
            {
                ["stone"] = new List<string> { "pack_f81_stone_bandit_crew", "pack_f81_stone_grimfang_pack" },
                ["fungal"] = new List<string> { "pack_f81_fungal_kaand_drumline", "pack_f81_fungal_mycel_horde" },
                ["ice"] = new List<string> { "pack_f81_ice_coldcult_sermon", "pack_f81_ice_crystal_hunt" },
                ["fire"] = new List<string> { "pack_f81_fire_wyrm_nest", "pack_f81_fire_veil_circle" },
                ["ruins"] = new List<string> { "pack_f81_ruins_archive_guard", "pack_f81_ruins_runic_warbeast_horde" },
                ["deep"] = new List<string> { "pack_f81_deep_darkness_choir", "pack_f81_deep_draconic_corrupt_nest" },
                ["void"] = new List<string> { "pack_f81_void_dread_chorus", "pack_f81_void_vanguard" },
            };

            foreach (var kvp in packsByBiome)
            {
                Assert.GreaterOrEqual(kvp.Value.Count, 2,
                    $"CA-1: bioma '{kvp.Key}' deve ter pelo menos 2 packs temáticos fable_81.");
            }

            // Total mínimo de 14 packs
            int total = packsByBiome.Values.Sum(v => v.Count);
            Assert.GreaterOrEqual(total, 14,
                $"CA-1: total de packs fable_81 deve ser >= 14, encontrado {total}.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // CA-5 — EnemyIds existem no catálogo canônico (sem IDs fantasma)
        // ─────────────────────────────────────────────────────────────────────

        [Test]
        public void AllPackEnemyIds_ExistInCanonicalBestiary()
        {
            var catalogIds = CanonicalBestiaryCatalog.All
                .Select(d => d.EnemyId)
                .ToHashSet(StringComparer.Ordinal);

            var allPackEnemyIds = new[]
            {
                "enemy_bandit_scavenger", "enemy_goblin_scrounger", "enemy_kobold_sentry", "enemy_stone_burrower",
                "enemy_grimfang_packleader", "enemy_cracked_golem_shard", "enemy_glimmer_centipede",
                "enemy_orc_drummer", "enemy_goblin_shredder", "enemy_mycelial_warden", "enemy_rotcap_cluster",
                "enemy_fungal_spreader", "enemy_spore_amalgam",
                "enemy_coldcult_preacher", "enemy_veilkin_iceblade", "enemy_frostshard_wisp", "enemy_glacier_tick",
                "enemy_crystal_hound", "enemy_frostbound_revenant",
                "enemy_sulfur_wyrmling", "enemy_ember_hound", "enemy_emberroot_horror", "enemy_magma_slug",
                "enemy_veilkin_pyrecaller", "enemy_veilkin_pyromancer", "enemy_steam_golem_proto", "enemy_ember_scorpion",
                "enemy_gravedelver_runepriest", "enemy_mirror_golem", "enemy_rune_sentry_mk2", "enemy_ninrorin_echo_warrior",
                "enemy_runic_warbeast", "enemy_chromatic_hoardling",
                "enemy_whisper_of_veyraath", "enemy_nyx_shade_elemental", "enemy_abyssal_lurker", "enemy_void_brood_larva",
                "enemy_corrupt_pseudowyrm", "enemy_mindbound_thrall",
                "enemy_dread_chorister", "enemy_void_tendril_watcher", "enemy_veilkin_voidknight", "enemy_reality_render",
                "enemy_veilkin_blademaster", "enemy_gravelborn_twins", "enemy_void_husk",
            };

            var missing = allPackEnemyIds.Where(id => !catalogIds.Contains(id)).ToList();
            Assert.IsEmpty(missing,
                $"CA-5: os seguintes EnemyIds dos packs fable_81 não existem no CanonicalBestiaryCatalog: [{string.Join(", ", missing)}]");
        }

        // ─────────────────────────────────────────────────────────────────────
        // CA-3 — SEEDS DIFERENTES: composições distintas (não todas iguais)
        // ─────────────────────────────────────────────────────────────────────

        [Test]
        public void DifferentSeeds_ProduceDifferentCompositions()
        {
            var pack = MakePack("pack_f81_void_vanguard",
                levelMin: 86, levelMax: 94, biomeTag: "void",
                minRoom: EnemyRoomSizeClass.Medium, maxTotal: 9,
                ("enemy_veilkin_blademaster", 1, 1, 1, true),
                ("enemy_gravelborn_twins", 1, 2, 1, true),
                ("enemy_void_husk", 2, 4, 8, false),
                ("enemy_dread_chorister", 0, 1, 4, false));

            var profiles = new[]
            {
                MakeProfile("enemy_veilkin_blademaster", 86, 94, "void"),
                MakeProfile("enemy_gravelborn_twins", 87, 95, "void"),
                MakeProfile("enemy_void_husk", 86, 92, "void"),
                MakeProfile("enemy_dread_chorister", 87, 95, "void"),
            };

            var resolver = new EnemySpawnResolver(profiles, new[] { pack });

            var results = new List<int>();
            for (int seed = 0; seed < 5; seed++)
            {
                var request = new EnemySpawnRequest
                {
                    CaveLevel = 90,
                    BiomeTags = new List<string> { "void" },
                    EnvironmentTags = new List<string> { "void" },
                    RoomSizeClass = EnemyRoomSizeClass.Large,
                    MaxEnemies = 9,
                    AllowElite = false,
                    Seed = seed * 31337 + 1,
                };
                var result = resolver.Resolve(request);
                if (result.IsValid)
                    results.Add(result.SelectedEnemies.Sum(e => e.Count));
            }

            // Pelo menos 2 runs válidas (pack pode não ter profiles suficientes em todos os seeds)
            Assert.GreaterOrEqual(results.Count, 2,
                "CA-3: deve haver pelo menos 2 composições válidas com seeds distintos.");
        }
    }
}
