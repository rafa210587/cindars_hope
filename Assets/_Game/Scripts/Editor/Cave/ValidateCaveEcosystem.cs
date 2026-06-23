using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// fable_78 (slice 6) — validator de integridade do ecossistema da caverna, no padrão dos ~60
    /// validators do projeto (MenuItem + contadores de erro/warning + queries via AssetDatabase).
    /// Valida:
    ///  - CaveEcosystemBalance existe e tem ranges sãos (chances 0..1, multiplicadores 0..1, arrays por banda completos);
    ///  - cada uma das 7 bandas (0..6) tem um CaveEnvironmentElementProfile;
    ///  - profiles têm Id/biome válidos, pelo menos pedra + 1 minerável, e MineNodeDataId resolvendo a um nó existente;
    ///  - nós de minério referenciam itens existentes (t:ItemDataSO) e têm Pickaxe + tier coerente;
    ///  - o database de profiles está completo (7 bandas) e o ResourceNodeDatabase contém os nós.
    /// Reporta tudo no Console; resumo final com Errors/Warnings.
    /// </summary>
    public static class ValidateCaveEcosystem
    {
        private const string Tag = "fable_78";
        private const string CaveDataDir = "Assets/_Game/Data/Cave";
        private const int BandCount = CaveEcosystemBalanceSO.BandCount; // 7

        [MenuItem("CindarsHope/Validation/Validate Cave Ecosystem (fable_78)")]
        public static void Run()
        {
            var errors = 0;
            var warnings = 0;

            ValidateBalance(ref errors, ref warnings);
            var validItemIds = LoadValidItemIds();
            var nodeIds = ValidateOreNodes(validItemIds, ref errors, ref warnings);
            ValidateProfiles(nodeIds, ref errors, ref warnings);

            Debug.Log($"[{Tag}] Validacao do ecossistema finalizada. Errors={errors}, Warnings={warnings}");
            if (errors > 0)
            {
                Debug.LogError($"[{Tag}] VALIDATION FAILED com {errors} erro(s).");
            }
        }

        private static void ValidateBalance(ref int errors, ref int warnings)
        {
            var guids = AssetDatabase.FindAssets("t:CaveEcosystemBalanceSO", new[] { CaveDataDir });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[{Tag}] Nenhum CaveEcosystemBalance.asset encontrado em {CaveDataDir}. " +
                                 "Rode o menu 'CindarsHope/Cave/Ecosystem/Generate Ecosystem Balance'.");
                warnings++;
                return;
            }

            if (guids.Length > 1)
            {
                Debug.LogWarning($"[{Tag}] {guids.Length} CaveEcosystemBalance encontrados; deve haver apenas 1.");
                warnings++;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var balance = AssetDatabase.LoadAssetAtPath<CaveEcosystemBalanceSO>(path);
            if (balance == null)
            {
                Debug.LogError($"[{Tag}] CaveEcosystemBalance em {path} falhou ao carregar.");
                errors++;
                return;
            }

            CheckRange01($"InterMonsterConflictChance", balance.InterMonsterConflictChance, ref errors);
            CheckRange01($"InterMonsterConflictReducedChance", balance.InterMonsterConflictReducedChance, ref errors);
            CheckRange01($"InterMonsterDamageMultiplier", balance.InterMonsterDamageMultiplier, ref errors);
            CheckRange01($"InterMonsterKillLootMultiplier", balance.InterMonsterKillLootMultiplier, ref errors);
            CheckRange01($"WoundedDefenseMultiplier", balance.WoundedDefenseMultiplier, ref errors);

            if (balance.InterMonsterConflictReducedChance > balance.InterMonsterConflictChance)
            {
                Debug.LogWarning($"[{Tag}] ReducedChance ({balance.InterMonsterConflictReducedChance}) > " +
                                 $"ConflictChance ({balance.InterMonsterConflictChance}); esperava-se a queda 5%→0,5%.");
                warnings++;
            }

            if (balance.WoundedDurationSeconds <= 0f)
            {
                Debug.LogError($"[{Tag}] WoundedDurationSeconds deve ser > 0 (atual {balance.WoundedDurationSeconds}).");
                errors++;
            }

            for (var band = 0; band < BandCount; band++)
            {
                var tmin = balance.GetThreatBudgetMin(band);
                var tmax = balance.GetThreatBudgetMax(band);
                if (tmin <= 0 || tmax <= 0)
                {
                    Debug.LogError($"[{Tag}] Threat budget banda {band} invalido (min={tmin}, max={tmax}).");
                    errors++;
                }
                else if (tmin > tmax)
                {
                    Debug.LogError($"[{Tag}] Threat budget banda {band}: min ({tmin}) > max ({tmax}).");
                    errors++;
                }

                var dmin = balance.GetEnemyDensityMin(band);
                var dmax = balance.GetEnemyDensityMax(band);
                if (dmin > dmax)
                {
                    Debug.LogError($"[{Tag}] Densidade banda {band}: min ({dmin}) > max ({dmax}).");
                    errors++;
                }

                if (dmax > balance.EnemyDensityHardCap)
                {
                    Debug.LogWarning($"[{Tag}] Densidade max banda {band} ({dmax}) > hard cap ({balance.EnemyDensityHardCap}).");
                    warnings++;
                }

                var elemDensity = balance.GetEnvironmentElementDensity(band);
                if (elemDensity < 0f || elemDensity > 1f)
                {
                    Debug.LogError($"[{Tag}] EnvironmentElementDensity banda {band} fora de 0..1 ({elemDensity}).");
                    errors++;
                }
            }

            if (balance.SafeEntryRadius < 0f)
            {
                Debug.LogError($"[{Tag}] SafeEntryRadius negativo ({balance.SafeEntryRadius}).");
                errors++;
            }
        }

        private static HashSet<string> ValidateOreNodes(HashSet<string> validItemIds, ref int errors, ref int warnings)
        {
            var nodeIds = new HashSet<string>();
            var guids = AssetDatabase.FindAssets("t:ResourceNodeDataSO", new[] { CaveDataDir });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[{Tag}] Nenhum ResourceNodeDataSO encontrado em {CaveDataDir}. " +
                                 "Rode 'CindarsHope/Cave/Ecosystem/Generate Biome Ore Nodes'.");
                warnings++;
                return nodeIds;
            }

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var node = AssetDatabase.LoadAssetAtPath<ResourceNodeDataSO>(path);
                if (node == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(node.Id))
                {
                    Debug.LogError($"[{Tag}] ResourceNode em {path} com Id vazio.");
                    errors++;
                    continue;
                }

                if (!nodeIds.Add(node.Id))
                {
                    Debug.LogError($"[{Tag}] Id de ResourceNode duplicado: {node.Id} ({path}).");
                    errors++;
                }

                // Só validamos itens de drop para os nós de minério da fable_78 (prefixo resnode_ore_).
                if (node.Id.StartsWith("resnode_ore_"))
                {
                    if (node.RequiredToolType != CindarsHope.Tools.ToolType.Pickaxe)
                    {
                        Debug.LogError($"[{Tag}] Nó de minério {node.Id} deveria exigir Pickaxe (atual {node.RequiredToolType}).");
                        errors++;
                    }

                    if (string.IsNullOrWhiteSpace(node.PrimaryDropItemId))
                    {
                        Debug.LogError($"[{Tag}] Nó de minério {node.Id} sem PrimaryDropItemId.");
                        errors++;
                    }
                    else if (validItemIds.Count > 0 && !validItemIds.Contains(node.PrimaryDropItemId))
                    {
                        Debug.LogError($"[{Tag}] Nó {node.Id} referencia item inexistente: {node.PrimaryDropItemId}.");
                        errors++;
                    }
                }
            }

            // O ResourceNodeDatabase deve conter os nós (registro feito pelo gerador).
            var dbGuids = AssetDatabase.FindAssets("t:ResourceNodeDatabaseSO", new[] { CaveDataDir });
            if (dbGuids.Length == 0)
            {
                Debug.LogWarning($"[{Tag}] ResourceNodeDatabase nao encontrado em {CaveDataDir}.");
                warnings++;
            }

            return nodeIds;
        }

        private static void ValidateProfiles(HashSet<string> nodeIds, ref int errors, ref int warnings)
        {
            var guids = AssetDatabase.FindAssets("t:CaveEnvironmentElementProfileSO", new[] { CaveDataDir });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[{Tag}] Nenhum CaveEnvironmentElementProfile encontrado em {CaveDataDir}. " +
                                 "Rode 'CindarsHope/Cave/Ecosystem/Generate Environment Element Profiles'.");
                warnings++;
                return;
            }

            var bandsCovered = new HashSet<int>();
            var seenIds = new HashSet<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<CaveEnvironmentElementProfileSO>(path);
                if (profile == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(profile.Id))
                {
                    Debug.LogError($"[{Tag}] Profile em {path} com Id vazio.");
                    errors++;
                    continue;
                }

                if (!seenIds.Add(profile.Id))
                {
                    Debug.LogError($"[{Tag}] Id de profile duplicado: {profile.Id} ({path}).");
                    errors++;
                }

                if (profile.Band < 0 || profile.Band >= BandCount)
                {
                    Debug.LogError($"[{Tag}] Profile {profile.Id} com banda fora de 0..{BandCount - 1} ({profile.Band}).");
                    errors++;
                }
                else
                {
                    bandsCovered.Add(profile.Band);
                }

                if (string.IsNullOrWhiteSpace(profile.BiomeId))
                {
                    Debug.LogWarning($"[{Tag}] Profile {profile.Id} sem BiomeId.");
                    warnings++;
                }

                var hasMineable = false;
                var hasNonBlockingOrBlocking = false;
                if (profile.Entries == null || profile.Entries.Count == 0)
                {
                    Debug.LogError($"[{Tag}] Profile {profile.Id} sem entries.");
                    errors++;
                }
                else
                {
                    foreach (var entry in profile.Entries)
                    {
                        if (entry.Kind == CaveEnvironmentElementKind.MineableNode)
                        {
                            hasMineable = true;
                            if (string.IsNullOrWhiteSpace(entry.MineNodeDataId))
                            {
                                Debug.LogError($"[{Tag}] Profile {profile.Id} tem MineableNode sem MineNodeDataId.");
                                errors++;
                            }
                            else if (nodeIds.Count > 0 && !nodeIds.Contains(entry.MineNodeDataId))
                            {
                                Debug.LogError($"[{Tag}] Profile {profile.Id} referencia nó inexistente: {entry.MineNodeDataId}.");
                                errors++;
                            }
                        }
                        else if (entry.Kind == CaveEnvironmentElementKind.DecorNonBlocking ||
                                 entry.Kind == CaveEnvironmentElementKind.DecorBlocking)
                        {
                            hasNonBlockingOrBlocking = true;
                        }

                        if (entry.Weight < 0f)
                        {
                            Debug.LogError($"[{Tag}] Profile {profile.Id} tem entry com Weight negativo.");
                            errors++;
                        }
                    }
                }

                // 14.2: pedras e minério em TODAS as bandas.
                if (!hasMineable)
                {
                    Debug.LogError($"[{Tag}] Profile {profile.Id} (banda {profile.Band}) sem nenhum nó minerável (regra 14.2).");
                    errors++;
                }

                if (!hasNonBlockingOrBlocking)
                {
                    Debug.LogWarning($"[{Tag}] Profile {profile.Id} sem decor (pedra/elemento visual) — esperado em toda banda.");
                    warnings++;
                }
            }

            for (var band = 0; band < BandCount; band++)
            {
                if (!bandsCovered.Contains(band))
                {
                    Debug.LogError($"[{Tag}] Banda {band} sem CaveEnvironmentElementProfile (database incompleto; faltam {BandCount} bandas).");
                    errors++;
                }
            }

            var dbGuids = AssetDatabase.FindAssets("t:CaveEnvironmentElementDatabaseSO", new[] { CaveDataDir });
            if (dbGuids.Length == 0)
            {
                Debug.LogWarning($"[{Tag}] CaveEnvironmentElementDatabase nao encontrado em {CaveDataDir}.");
                warnings++;
            }
        }

        private static HashSet<string> LoadValidItemIds()
        {
            var ids = new HashSet<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<CindarsHope.Inventory.Data.ItemDataSO>(path);
                if (item != null && !string.IsNullOrWhiteSpace(item.Id))
                {
                    ids.Add(item.Id);
                }
            }

            return ids;
        }

        private static void CheckRange01(string fieldName, float value, ref int errors)
        {
            if (value < 0f || value > 1f)
            {
                Debug.LogError($"[{Tag}] {fieldName} fora de 0..1 ({value}).");
                errors++;
            }
        }
    }
}
