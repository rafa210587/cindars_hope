using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Cave
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01), T008 — validador SOMENTE LEITURA (não gera, não
    /// repara, não muta assets) registrado como RunStep em CindarsHopeMenu.ValidarProjeto.
    ///
    /// Regra de severidade (critério 14.1/14.5 da spec): 8 profiles ausentes ou biomeId
    /// desalinhado com CaveBiomeRegistrySO = ERROR (quebra o contrato de dados); campos de
    /// sprite/tile vazios (arte ainda não produzida) = WARNING, NUNCA ERROR — ausência de arte é o
    /// estado esperado antes do lote 2.
    /// </summary>
    public static class ValidateCaveBiomeArtProfiles
    {
        private const string ProfileDir = "Assets/_Game/Data/Cave/Biomes";
        private const string ArtFolderRoot = "Assets/_Game/Art/Generated/World/cave";
        private const int ExpectedProfileCount = 8;

        public static void Validate()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var passed = new List<string>();

            var profiles = LoadAllProfiles();
            if (profiles.Count != ExpectedProfileCount)
            {
                errors.Add($"Esperado {ExpectedProfileCount} CaveBiomeArtProfileSO em {ProfileDir}, encontrado {profiles.Count}. Rode CindarsHope/Inicializar Projeto.");
            }
            else
            {
                passed.Add($"{ExpectedProfileCount} profiles encontrados em {ProfileDir}.");
            }

            var registry = FindBiomeRegistry();
            var registryIds = new HashSet<string>();
            if (registry == null)
            {
                warnings.Add("CaveBiomeRegistrySO não encontrado no projeto — checagem de biomeId contra o registry pulada.");
            }
            else
            {
                foreach (var biome in registry.All)
                {
                    if (biome != null && !string.IsNullOrWhiteSpace(biome.Id))
                    {
                        registryIds.Add(biome.Id);
                    }
                }
            }

            var seenBandProfiles = new Dictionary<int, CaveBiomeArtProfileSO>();
            var emptyFieldProfiles = 0;

            foreach (var profile in profiles)
            {
                if (profile == null)
                {
                    errors.Add("Entrada null encontrada entre os CaveBiomeArtProfileSO carregados.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(profile.BiomeId))
                {
                    errors.Add($"Profile '{profile.name}' sem biomeId.");
                }
                else if (registryIds.Count > 0 && !registryIds.Contains(profile.BiomeId))
                {
                    errors.Add($"Profile '{profile.name}': biomeId '{profile.BiomeId}' não existe em CaveBiomeRegistrySO.");
                }

                if (seenBandProfiles.TryGetValue(profile.BandId, out var previousProfile))
                {
                    if (IsKnownFinaleBandAlias(previousProfile, profile))
                    {
                        warnings.Add(
                            $"Profile '{profile.name}': bandId {profile.BandId} compartilhado com '{previousProfile.name}' " +
                            "(biome_core/biome_final usam a mesma banda 7 por CaveBandScaling.BandForLevel; o resolver por BandId usa o primeiro profile).");
                    }
                    else
                    {
                        errors.Add($"Profile '{profile.name}': bandId {profile.BandId} duplicado entre profiles (cada banda deve ter exatamente 1, exceto biome_core/biome_final na banda 7).");
                    }
                }
                else
                {
                    seenBandProfiles.Add(profile.BandId, profile);
                }

                if (IsVisuallyEmpty(profile))
                {
                    emptyFieldProfiles++;
                }

                CollectEmptyObjectSpriteWarnings(profile, warnings);
            }

            if (emptyFieldProfiles > 0)
            {
                warnings.Add($"{emptyFieldProfiles} profile(s) sem NENHUM tile/sprite preenchido — esperado antes do lote 2 de arte (fallback aos placeholders atuais).");
            }
            else if (profiles.Count > 0)
            {
                passed.Add("Todos os profiles têm ao menos 1 campo de arte preenchido.");
            }

            // Fix pós-Play-Mode 2026-07-04: registry em Resources é o fallback que
            // CaveRuntimeMaterializer usa quando o array serializado da cena está vazio (cenas
            // criadas antes desta spec). Ausência não é ERROR (a cena pode ter o array wireado
            // manualmente), mas é um sinal forte de que o bioma pode não aparecer em Play Mode.
            var registryPath = "Assets/_Game/Resources/CaveBiomeArtProfileRegistry.asset";
            var registryAsset = AssetDatabase.LoadAssetAtPath<CaveBiomeArtProfileRegistrySO>(registryPath);
            if (registryAsset == null)
            {
                warnings.Add($"CaveBiomeArtProfileRegistry não encontrado em {registryPath} — fallback via Resources indisponível se a cena não tiver o array _biomeArtProfiles wireado. Rode CindarsHope/Inicializar Projeto.");
            }
            else if (registryAsset.Profiles == null || registryAsset.Profiles.Count != ExpectedProfileCount)
            {
                warnings.Add($"CaveBiomeArtProfileRegistry em {registryPath} tem {(registryAsset.Profiles?.Count ?? 0)} profile(s), esperado {ExpectedProfileCount}. Rode CindarsHope/Inicializar Projeto.");
            }
            else
            {
                passed.Add($"CaveBiomeArtProfileRegistry encontrado em {registryPath} com {ExpectedProfileCount} profile(s).");
            }

            if (warnings.Count > 0)
            {
                Debug.LogWarning($"[CV01 art profile validation] {warnings.Count} warning(s):\n - " + string.Join("\n - ", warnings));
            }

            if (errors.Count > 0)
            {
                Debug.LogError($"[CV01 art profile validation] FAILED com {errors.Count} erro(s):\n - " + string.Join("\n - ", errors));
            }
            else
            {
                Debug.Log($"[CV01 art profile validation] PASS. {passed.Count} check(s) OK:\n - " + string.Join("\n - ", passed));
            }
        }

        /// <summary>Lote 2 de arte — WARNING granular (nunca ERROR) por campo de sprite de objeto
        /// (chest/hazard/exit) ainda vazio, para o humano saber exatamente qual PNG falta na
        /// convenção de pasta em vez de só "profile vazio".</summary>
        private static void CollectEmptyObjectSpriteWarnings(CaveBiomeArtProfileSO profile, List<string> warnings)
        {
            if (profile.ChestClosedSprite == null) warnings.Add($"Profile '{profile.name}': ChestClosedSprite vazio (chest_closed.png ausente).");
            if (profile.ChestOpenSprite == null) warnings.Add($"Profile '{profile.name}': ChestOpenSprite vazio (chest_open.png ausente).");
            if (profile.FalseChestRevealedSprite == null) warnings.Add($"Profile '{profile.name}': FalseChestRevealedSprite vazio (chest_false.png ausente).");
            if (profile.ToxicPoolSprite == null) warnings.Add($"Profile '{profile.name}': ToxicPoolSprite vazio (hazard_toxic.png ausente).");
            if (profile.IceSlickSprite == null) warnings.Add($"Profile '{profile.name}': IceSlickSprite vazio (hazard_ice.png ausente).");
            if (profile.FallingRockSprite == null) warnings.Add($"Profile '{profile.name}': FallingRockSprite vazio (hazard_rock.png ausente).");
            if (profile.ExitDownSprite == null) warnings.Add($"Profile '{profile.name}': ExitDownSprite vazio (exit_down.png ausente).");
            if (profile.ExitUpSprite == null) warnings.Add($"Profile '{profile.name}': ExitUpSprite vazio (exit_up.png ausente).");

            // spec_cave_decor_placement_runtime (CV02): WARNING (nunca ERROR) se os pools de decor
            // do fable_78 estiverem vazios num bioma que já tem pasta de arte (Bioma 1 hoje) —
            // ausência é o estado esperado nos biomas 2-8 até terem arte própria.
            var hasArtFolder = AssetDatabase.IsValidFolder($"{ArtFolderRoot}/{profile.BiomeId}");
            if (hasArtFolder && profile.DecorNonBlockingSprites.Count == 0)
            {
                warnings.Add($"Profile '{profile.name}': DecorNonBlockingSprites vazio apesar de haver pasta de arte ({profile.BiomeId}). Rode CindarsHope/Inicializar Projeto.");
            }

            if (hasArtFolder && profile.DecorBlockingSprites.Count == 0)
            {
                warnings.Add($"Profile '{profile.name}': DecorBlockingSprites vazio apesar de haver pasta de arte ({profile.BiomeId}). Rode CindarsHope/Inicializar Projeto.");
            }

            // spec_cave_decor_composition_runtime (CV03): mesma regra de severidade (WARNING, nunca
            // ERROR) para os pools por CONTEXTO — ausência é o estado esperado nos biomas 2-8 até terem
            // arte própria; no bioma 1 (com pasta de arte) sinaliza que o gerador precisa rodar de novo.
            if (hasArtFolder && profile.CeilingSprites.Count == 0)
            {
                warnings.Add($"Profile '{profile.name}': CeilingSprites vazio apesar de haver pasta de arte ({profile.BiomeId}). Rode CindarsHope/Inicializar Projeto.");
            }

            if (hasArtFolder && profile.WallHugSprites.Count == 0)
            {
                warnings.Add($"Profile '{profile.name}': WallHugSprites vazio apesar de haver pasta de arte ({profile.BiomeId}). Rode CindarsHope/Inicializar Projeto.");
            }

            if (hasArtFolder && profile.FloorClusterSprites.Count == 0)
            {
                warnings.Add($"Profile '{profile.name}': FloorClusterSprites vazio apesar de haver pasta de arte ({profile.BiomeId}). Rode CindarsHope/Inicializar Projeto.");
            }

            if (hasArtFolder && profile.BlockingSprites.Count == 0)
            {
                warnings.Add($"Profile '{profile.name}': BlockingSprites (CV03) vazio apesar de haver pasta de arte ({profile.BiomeId}). Rode CindarsHope/Inicializar Projeto.");
            }
        }

        private static bool IsVisuallyEmpty(CaveBiomeArtProfileSO profile)
        {
            return profile.FloorTiles.Length == 0
                && profile.FloorDetailTile == null
                && profile.WallFaceTile == null
                && profile.WallTopTile == null
                && profile.ToxicPoolSprite == null
                && profile.IceSlickSprite == null
                && profile.FallingRockSprite == null
                && profile.TrapSprites.Length == 0
                && profile.ChestClosedSprite == null
                && profile.ChestOpenSprite == null
                && profile.FalseChestRevealedSprite == null
                && profile.ExitDownSprite == null
                && profile.ExitUpSprite == null;
        }

        private static bool IsKnownFinaleBandAlias(CaveBiomeArtProfileSO first, CaveBiomeArtProfileSO second)
        {
            if (first == null || second == null || first.BandId != 7 || second.BandId != 7)
            {
                return false;
            }

            var ids = new HashSet<string> { first.BiomeId, second.BiomeId };
            return ids.Contains("biome_core") && ids.Contains("biome_final");
        }

        private static List<CaveBiomeArtProfileSO> LoadAllProfiles()
        {
            var result = new List<CaveBiomeArtProfileSO>();
            if (!AssetDatabase.IsValidFolder(ProfileDir))
            {
                return result;
            }

            var guids = AssetDatabase.FindAssets("t:CaveBiomeArtProfileSO", new[] { ProfileDir });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<CaveBiomeArtProfileSO>(path);
                if (profile != null)
                {
                    result.Add(profile);
                }
            }

            return result;
        }

        private static CaveBiomeRegistrySO FindBiomeRegistry()
        {
            var guids = AssetDatabase.FindAssets("t:CaveBiomeRegistrySO");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var registry = AssetDatabase.LoadAssetAtPath<CaveBiomeRegistrySO>(path);
                if (registry != null)
                {
                    return registry;
                }
            }

            return null;
        }
    }
}
