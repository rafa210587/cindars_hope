using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor
{
    /// <summary>
    /// SPEC 14A-FIX11: single orchestrator that consolidates the previously polluted CindarsHope menu.
    ///
    /// Top-level exposes only:
    ///   - CindarsHope/Repair and Validate Project   (the safe one-button command)
    ///   - CindarsHope/Open Main Scenes              (quick scene access)
    ///   - CindarsHope/Advanced/...                  (power-user sub-commands)
    ///
    /// Legacy validators and generators still exist as classes; their [MenuItem] attributes were
    /// moved under CindarsHope/Archive/* so they are no longer the first thing a user sees.
    /// </summary>
    public static class CindarsHopeProjectMaintenanceMenu
    {
        // Known database asset paths used by repair + validate sweeps.
        private static readonly string[] RegistryAssetPaths =
        {
            "Assets/_Game/Data/Registries/ItemDatabase.asset",
            "Assets/_Game/Data/Combat/WeaponDatabase.asset",
            "Assets/_Game/Data/Combat/EnemyDatabase.asset",
            "Assets/_Game/Data/Combat/EnemyMovementProfileDatabase.asset",
            "Assets/_Game/Data/Combat/EnemyActionSetDatabase.asset",
            "Assets/_Game/Data/Combat/EnemyActionDatabase.asset",
            "Assets/_Game/Data/Combat/EnemyTelegraphProfileDatabase.asset",
            "Assets/_Game/Data/Combat/EnemyVulnerabilityProfileDatabase.asset",
            "Assets/_Game/Data/Combat/EnemySizeProfileDatabase.asset",
        };

        private const string ItemDatabaseAssetPath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string CombatRegistryAssetPath = "Assets/_Game/Resources/CombatRuntimeDatabasesRegistry.asset";
        private const string CaveSceneAssetPath = "Assets/_Game/Scenes/CaveScene.unity";
        private const string FarmSceneAssetPath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string TownSceneAssetPath = "Assets/_Game/Scenes/TownScene.unity";

        // ── Top-level commands ──────────────────────────────────────────────────

        public static void RepairAndValidateProject()
        {
            int totalRepaired = 0;
            int totalWarnings = 0;
            int totalErrors = 0;
            var actionsLeft = new List<string>();

            Debug.Log("=== CindarsHope: Repair and Validate Project ===");

            // 1. Repair null entries in all known registries.
            int repaired = RemoveNullsFromAllRegistries(out var registryReports);
            totalRepaired += repaired;
            foreach (var report in registryReports)
            {
                if (!report.HasIssues)
                {
                    Debug.Log($"  PASS: {report.Summarize()}");
                }
                else
                {
                    if (report.NullEntries.Count > 0) totalErrors += report.NullEntries.Count;
                    if (report.EmptyIdEntries.Count > 0) totalErrors += report.EmptyIdEntries.Count;
                    if (report.DuplicateIds.Count > 0) totalErrors += report.DuplicateIds.Count;
                    Debug.LogError($"  FAIL: {report.Summarize()}");
                    var registry = AssetDatabase.LoadAssetAtPath<ScriptableObject>(report.RegistryAssetPath);
                    LogRegistryIssueDetails(registry, report);
                }
            }

            // 2. Validate combat runtime registry.
            var combatRegistry = AssetDatabase.LoadAssetAtPath<CombatRuntimeDatabasesRegistrySO>(CombatRegistryAssetPath);
            if (combatRegistry == null)
            {
                Debug.LogError($"  FAIL: CombatRuntimeDatabasesRegistry missing at {CombatRegistryAssetPath}.");
                totalErrors++;
                actionsLeft.Add("Recreate CombatRuntimeDatabasesRegistry.asset under Assets/_Game/Resources/.");
            }
            else
            {
                int missing = 0;
                if (combatRegistry.EnemyDatabase == null)               { missing++; Debug.LogError("  FAIL: CombatRegistry.EnemyDatabase is null."); }
                if (combatRegistry.MovementProfileDatabase == null)     { missing++; Debug.LogError("  FAIL: CombatRegistry.MovementProfileDatabase is null."); }
                if (combatRegistry.ActionSetDatabase == null)           { missing++; Debug.LogError("  FAIL: CombatRegistry.ActionSetDatabase is null."); }
                if (combatRegistry.ActionDatabase == null)              { missing++; Debug.LogError("  FAIL: CombatRegistry.ActionDatabase is null."); }
                if (combatRegistry.TelegraphDatabase == null)           { missing++; Debug.LogError("  FAIL: CombatRegistry.TelegraphDatabase is null."); }
                if (combatRegistry.VulnerabilityProfileDatabase == null){ missing++; Debug.LogError("  FAIL: CombatRegistry.VulnerabilityProfileDatabase is null."); }
                if (combatRegistry.SizeProfileDatabase == null)         { missing++; Debug.LogError("  FAIL: CombatRegistry.SizeProfileDatabase is null."); }
                if (combatRegistry.ItemDatabase == null)                { missing++; Debug.LogError("  FAIL: CombatRegistry.ItemDatabase is null."); }
                if (combatRegistry.WeaponDatabase == null)              { missing++; Debug.LogError("  FAIL: CombatRegistry.WeaponDatabase is null."); }
                if (missing == 0) Debug.Log("  PASS: CombatRuntimeDatabasesRegistry has every database wired.");
                else { totalErrors += missing; actionsLeft.Add("Open CombatRuntimeDatabasesRegistry.asset and assign missing databases."); }
            }

            // 3. Validate that equippable items reference resolvable weapons.
            int orphanWeaponItems = ValidateWeaponItemReferences(combatRegistry);
            if (orphanWeaponItems > 0)
            {
                totalErrors += orphanWeaponItems;
                actionsLeft.Add($"Fix {orphanWeaponItems} item(s) whose WeaponId does not resolve in WeaponDatabase.");
            }

            // 4. Save modifications.
            if (totalRepaired > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"  Repaired {totalRepaired} registry slot(s) and saved.");
            }

            // 5. Final summary.
            string status = (totalErrors == 0) ? "PASS" : "FAIL";
            Debug.Log($"=== Repair and Validate Project: {status} ===");
            Debug.Log($"     Repaired={totalRepaired}, Warnings={totalWarnings}, Errors={totalErrors}");
            if (actionsLeft.Count > 0)
            {
                Debug.Log($"     Manual actions left ({actionsLeft.Count}):");
                foreach (var a in actionsLeft) Debug.Log($"       - {a}");
            }
            EditorUtility.DisplayDialog("Repair and Validate Project",
                $"{status}\nRepaired: {totalRepaired}\nErrors: {totalErrors}\nManual actions: {actionsLeft.Count}\n\nSee console for details.",
                "OK");
        }

        public static void OpenCaveScene() => OpenScene(CaveSceneAssetPath);

        public static void OpenFarmScene() => OpenScene(FarmSceneAssetPath);

        public static void OpenTownScene() => OpenScene(TownSceneAssetPath);

        // ── Advanced sub-menu ───────────────────────────────────────────────────

        public static void GenerateRuntimeAssets()
        {
            CindarsHope.Editor.EnemyTaxonomy.GenerateAndWireSpec13GAssets.GenerateAndWire();
        }

        public static void ValidateRegistries()
        {
            int total = 0;
            int problems = 0;
            foreach (var path in RegistryAssetPaths)
            {
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null)
                {
                    Debug.LogError($"ValidateRegistries: registry asset missing at {path}.");
                    problems++;
                    continue;
                }
                var report = InvokeValidateRegistry(so);
                if (report == null) continue;
                total++;
                if (report.HasIssues)
                {
                    problems++;
                    Debug.LogError($"  FAIL: {report.Summarize()}");
                    foreach (var idx in report.NullEntries) Debug.LogError($"    Null at index {idx}");
                    foreach (var idx in report.EmptyIdEntries) Debug.LogError($"    Empty Id at index {idx}");
                    foreach (var dup in report.DuplicateIds) Debug.LogError($"    Duplicate Id '{dup.Id}' at index {dup.Index} (first seen at {dup.FirstIndex})");
                    LogDuplicateAssetDetails(so, report);
                }
                else
                {
                    Debug.Log($"  PASS: {report.Summarize()}");
                }
            }
            Debug.Log($"ValidateRegistries: {total - problems}/{total} registries clean.");
        }

        // Reparo de Ids duplicados na ItemDatabase: assets legados com nome PT (Item_Cenoura.asset etc.)
        // compartilham o mesmo Id dos canonicos (item_crop_carrot.asset etc.). A registry referencia AMBOS,
        // o que faz o DataRegistry falhar com "Duplicate Id". Mantemos a entrada cujo nome de ARQUIVO == "{Id}.asset"
        // (a canonica) e removemos a outra (a legada) APENAS da registry — o arquivo .asset legado NAO e deletado.
        public static void RemoveDuplicateItemIds()
        {
            var database = AssetDatabase.LoadAssetAtPath<ScriptableObject>(ItemDatabaseAssetPath);
            if (database == null)
            {
                Debug.LogError($"[Repair] ItemDatabase nao encontrado em {ItemDatabaseAssetPath}.");
                return;
            }

            var serialized = new SerializedObject(database);
            var items = serialized.FindProperty("_items");
            if (items == null || !items.isArray)
            {
                Debug.LogError($"[Repair] Propriedade '_items' nao encontrada em {ItemDatabaseAssetPath}.");
                return;
            }

            // Agrupa indices por Id; so trata Ids com mais de uma entrada.
            var indicesById = new Dictionary<string, List<int>>();
            for (int i = 0; i < items.arraySize; i++)
            {
                var reference = items.GetArrayElementAtIndex(i).objectReferenceValue;
                if (reference == null) continue;
                var idData = reference as IIdentifiedData;
                if (idData == null || string.IsNullOrWhiteSpace(idData.Id)) continue;
                if (!indicesById.TryGetValue(idData.Id, out var list))
                {
                    list = new List<int>();
                    indicesById[idData.Id] = list;
                }
                list.Add(i);
            }

            // Coleta os indices a remover (a entrada NAO-canonica de cada Id duplicado).
            var indicesToRemove = new List<int>();
            var legacyPaths = new List<string>();
            int duplicatesRemoved = 0;
            foreach (var pair in indicesById)
            {
                if (pair.Value.Count < 2) continue;

                string id = pair.Key;
                string canonicalAssetName = $"{id}.asset";
                int keepIndex = -1;
                string canonicalPath = null;

                // Prefere manter a entrada cujo nome de ARQUIVO == "{Id}.asset".
                foreach (int idx in pair.Value)
                {
                    var reference = items.GetArrayElementAtIndex(idx).objectReferenceValue;
                    var path = AssetDatabase.GetAssetPath(reference);
                    if (!string.IsNullOrEmpty(path) && System.IO.Path.GetFileName(path) == canonicalAssetName)
                    {
                        keepIndex = idx;
                        canonicalPath = path;
                        break;
                    }
                }

                // Fallback: se nenhum arquivo casa o padrao canonico, mantem a primeira entrada.
                if (keepIndex < 0)
                {
                    keepIndex = pair.Value[0];
                    canonicalPath = AssetDatabase.GetAssetPath(items.GetArrayElementAtIndex(keepIndex).objectReferenceValue);
                }

                foreach (int idx in pair.Value)
                {
                    if (idx == keepIndex) continue;
                    var reference = items.GetArrayElementAtIndex(idx).objectReferenceValue;
                    var legacyPath = AssetDatabase.GetAssetPath(reference);
                    indicesToRemove.Add(idx);
                    legacyPaths.Add(legacyPath);
                    duplicatesRemoved++;
                    Debug.Log($"[Repair] Removida entrada duplicada da registry: {legacyPath} (Id={id}; mantida a canonica {canonicalPath})");
                }
            }

            if (duplicatesRemoved == 0)
            {
                Debug.Log("[Repair] ItemDatabase: nenhum Id duplicado encontrado. Nada a fazer.");
                return;
            }

            // Remove de indice mais alto para mais baixo para nao invalidar os indices.
            indicesToRemove.Sort();
            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            {
                items.DeleteArrayElementAtIndex(indicesToRemove[i]);
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Repair] ItemDatabase: {duplicatesRemoved} entrada(s) duplicada(s) removida(s) da registry.");
            Debug.LogWarning("[Repair] Os arquivos .asset legados abaixo continuam em disco (NAO foram deletados). " +
                             "Recomenda-se deleta-los manualmente no Project window do Editor:");
            foreach (var path in legacyPaths)
            {
                Debug.LogWarning($"  - {path}");
            }
        }

        public static void ValidateCaveRuntime()
        {
            CindarsHope.Editor.Validation.ValidateSpec14AEnemyRuntimeIntegration.RunValidation();
            CindarsHope.Editor.Validation.ValidateEnemyCaveSpawnCoverage.Validate();
        }

        public static void RunPlayModeSmokeValidation()
        {
            Debug.Log("PlayMode smoke: opening CaveScene + reporting current state.");
            OpenScene(CaveSceneAssetPath);
            ValidateRegistries();
            ValidateCaveRuntime();
            Debug.Log("PlayMode smoke: finished static checks. Enter Play Mode manually to confirm combat log sequence (EnemyDatabasesWiringStatus, PlayerAttackStarted, etc.).");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Overload publico parameterless: remove entradas null de todas as registries conhecidas
        /// e devolve quantos slots foram reparados. Usado pelo menu unico (Reparar e Reconstruir).
        /// </summary>
        public static int RemoveNullsFromAllRegistries()
        {
            return RemoveNullsFromAllRegistries(out _);
        }

        private static int RemoveNullsFromAllRegistries(out List<RegistryValidationReport> reports)
        {
            reports = new List<RegistryValidationReport>();
            int totalRepaired = 0;
            foreach (var path in RegistryAssetPaths)
            {
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null) continue;
                int removed = InvokeRemoveNullEntries(so);
                if (removed > 0)
                {
                    Debug.Log($"  Repaired {removed} null slot(s) in {path}.");
                    totalRepaired += removed;
                }
                var report = InvokeValidateRegistry(so);
                if (report != null) reports.Add(report);
            }
            return totalRepaired;
        }

        // Reflection helpers - DataRegistrySO<T> is generic so we can't call its methods directly
        // from a non-generic context without knowing T. SerializedObject path works for any T.
        private static int InvokeRemoveNullEntries(ScriptableObject so)
        {
            var method = so.GetType().GetMethod("RemoveNullEntries", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (method == null) return 0;
            var result = method.Invoke(so, null);
            return result is int i ? i : 0;
        }

        private static RegistryValidationReport InvokeValidateRegistry(ScriptableObject so)
        {
            var method = so.GetType().GetMethod("ValidateRegistry", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (method == null) return null;
            return method.Invoke(so, null) as RegistryValidationReport;
        }

        private static void LogRegistryIssueDetails(ScriptableObject registry, RegistryValidationReport report)
        {
            foreach (var idx in report.NullEntries)
            {
                Debug.LogError($"    Null at index {idx}");
            }

            foreach (var idx in report.EmptyIdEntries)
            {
                Debug.LogError($"    Empty Id at index {idx}");
                LogRegistryEntryAssetDetails(registry, idx, "empty-id");
            }

            foreach (var dup in report.DuplicateIds)
            {
                Debug.LogError($"    Duplicate Id '{dup.Id}' at index {dup.Index} (first seen at {dup.FirstIndex})");
            }

            LogDuplicateAssetDetails(registry, report);
        }

        private static void LogDuplicateAssetDetails(ScriptableObject registry, RegistryValidationReport report)
        {
            foreach (var dup in report.DuplicateIds)
            {
                LogRegistryEntryAssetDetails(registry, dup.FirstIndex, $"duplicate-first:{dup.Id}");
                LogRegistryEntryAssetDetails(registry, dup.Index, $"duplicate-current:{dup.Id}");
            }
        }

        private static void LogRegistryEntryAssetDetails(ScriptableObject registry, int index, string context)
        {
            if (registry == null)
            {
                return;
            }

            var serialized = new SerializedObject(registry);
            var items = serialized.FindProperty("_items");
            if (items == null || !items.isArray || index < 0 || index >= items.arraySize)
            {
                Debug.LogError($"      EntryDetail[{context}] Registry='{AssetDatabase.GetAssetPath(registry)}' Index={index} Asset=<unavailable>");
                return;
            }

            var reference = items.GetArrayElementAtIndex(index).objectReferenceValue;
            var assetPath = reference != null ? AssetDatabase.GetAssetPath(reference) : "<null>";
            var assetName = reference != null ? reference.name : "<null>";
            Debug.LogError($"      EntryDetail[{context}] Registry='{AssetDatabase.GetAssetPath(registry)}' Index={index} AssetName='{assetName}' AssetPath='{assetPath}'");
        }

        private static int ValidateWeaponItemReferences(CombatRuntimeDatabasesRegistrySO combatRegistry)
        {
            if (combatRegistry == null || combatRegistry.ItemDatabase == null || combatRegistry.WeaponDatabase == null) return 0;
            int orphans = 0;
            foreach (var item in combatRegistry.ItemDatabase.All)
            {
                if (item == null || string.IsNullOrEmpty(item.WeaponId)) continue;
                if (!combatRegistry.WeaponDatabase.TryGetById(item.WeaponId, out var weapon) || weapon == null)
                {
                    Debug.LogError($"  FAIL: Item '{item.Id}' references WeaponId='{item.WeaponId}' but no WeaponDataSO matches.");
                    orphans++;
                }
            }
            if (orphans == 0) Debug.Log("  PASS: All item.WeaponId references resolve to a WeaponDataSO.");
            return orphans;
        }

        private static void OpenScene(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError($"OpenScene: scene not found at {path}.");
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }
        }
    }
}
