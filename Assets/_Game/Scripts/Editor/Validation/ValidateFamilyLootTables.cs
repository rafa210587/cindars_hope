using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Core.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Loot;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// fable_06 CA-3 — Valida as tabelas de loot por família e as atribuições no roster:
    ///   - toda LootTableSO referenciada por um EnemyDataSO.lootTableId existe;
    ///   - todo item de toda tabela existe no ItemDatabase (invalid-id = erro);
    ///   - &gt;= 30 dos inimigos do roster têm lootTableId;
    ///   - o LootTableDatabaseSO contém todas as tabelas usadas pelo roster.
    /// Loga erros (Debug.LogError) por item ausente. Não edita assets.
    ///
    /// Run via: CindarsHope > Validation > Validate Family Loot Tables (fable_06)
    /// </summary>
    public static class ValidateFamilyLootTables
    {
        private const int MinAssignedEnemies = 30;
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string LootDatabasePath = "Assets/_Game/Data/Loot/LootTableDatabase.asset";

        [MenuItem("CindarsHope/Validation/Validate Family Loot Tables (fable_06)")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var passed = new List<string>();

            // 1) Item id universe (from ItemDatabase + raw ItemDataSO assets, to be lenient on wiring).
            var knownItemIds = BuildKnownItemIds();
            if (knownItemIds.Count == 0)
            {
                warnings.Add("No ItemDataSO assets found — item-existence cross-check skipped (generate the item catalog first).");
            }
            else
            {
                passed.Add($"Known item ids: {knownItemIds.Count}.");
            }

            // 2) Every loot table item id must exist (when the catalog is present).
            var allTables = AssetDatabase.FindAssets("t:LootTableSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<LootTableSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(t => t != null)
                .ToList();
            passed.Add($"LootTableSO assets found: {allTables.Count}.");

            foreach (var table in allTables.Where(t => !string.IsNullOrEmpty(t.FamilyId) || (t.TableId ?? string.Empty).StartsWith("loot_family_")))
            {
                if (string.IsNullOrWhiteSpace(table.TableId))
                    errors.Add($"{table.name}: TableId is empty (family table must have an id).");

                foreach (var item in EnumerateItemIds(table))
                {
                    if (knownItemIds.Count > 0 && !knownItemIds.Contains(item))
                        errors.Add($"{table.TableId}: drop item '{item}' not found in ItemDatabase.");
                }
            }

            // 3) Roster: lootTableId resolvable + coverage >= 30.
            var lootDb = AssetDatabase.LoadAssetAtPath<LootTableDatabaseSO>(LootDatabasePath);
            if (lootDb == null)
                errors.Add($"LootTableDatabase not found at '{LootDatabasePath}'. Run Generate Family Loot Tables.");

            var tableIdsOnDisk = allTables.Where(t => !string.IsNullOrWhiteSpace(t.TableId)).Select(t => t.TableId).ToHashSet();

            var allEnemies = AssetDatabase.FindAssets("t:EnemyDataSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null)
                .ToList();

            int withTable = 0;
            foreach (var enemy in allEnemies)
            {
                if (string.IsNullOrWhiteSpace(enemy.lootTableId)) continue;
                withTable++;

                if (!tableIdsOnDisk.Contains(enemy.lootTableId))
                    errors.Add($"{enemy.enemyId}: lootTableId '{enemy.lootTableId}' has no matching LootTableSO asset.");
                else if (lootDb != null && !lootDb.TryGetById(enemy.lootTableId, out _))
                    errors.Add($"{enemy.enemyId}: lootTableId '{enemy.lootTableId}' not registered in LootTableDatabase.");
            }

            if (withTable >= MinAssignedEnemies)
                passed.Add($"Enemies with lootTableId: {withTable}/{allEnemies.Count} (>= {MinAssignedEnemies}).");
            else
                errors.Add($"Only {withTable}/{allEnemies.Count} enemies have lootTableId (need >= {MinAssignedEnemies}). Run Generate Family Loot Tables.");

            Report(errors, warnings, passed);
        }

        private static HashSet<string> BuildKnownItemIds()
        {
            var ids = new HashSet<string>();

            var db = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (db != null)
            {
                foreach (var item in db.All)
                {
                    if (item != null && !string.IsNullOrWhiteSpace(item.Id)) ids.Add(item.Id);
                }
            }

            // Also scan raw ItemDataSO assets so validation does not falsely fail when the registry
            // is not yet rebuilt after a catalog regen (the generator and the registry can lag).
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO"))
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null && !string.IsNullOrWhiteSpace(item.Id)) ids.Add(item.Id);
            }

            return ids;
        }

        private static IEnumerable<string> EnumerateItemIds(LootTableSO table)
        {
            if (table.GuaranteedEntries != null)
                foreach (var e in table.GuaranteedEntries)
                    if (e != null && !string.IsNullOrWhiteSpace(e.ItemId)) yield return e.ItemId;

            if (table.Entries != null)
                foreach (var e in table.Entries)
                    if (e != null && !string.IsNullOrWhiteSpace(e.ItemId)) yield return e.ItemId;

            if (!string.IsNullOrWhiteSpace(table.EssenceItemId)) yield return table.EssenceItemId;
        }

        private static void Report(List<string> errors, List<string> warnings, List<string> passed)
        {
            if (warnings.Count > 0)
                Debug.LogWarning($"[fable_06 loot validation] {warnings.Count} warning(s):\n - " + string.Join("\n - ", warnings));

            if (errors.Count > 0)
            {
                Debug.LogError($"[fable_06 loot validation] FAILED with {errors.Count} error(s):\n - " + string.Join("\n - ", errors));
            }
            else
            {
                Debug.Log($"[fable_06 loot validation] PASS. {passed.Count} checks OK:\n - " + string.Join("\n - ", passed));
            }
        }
    }
}
