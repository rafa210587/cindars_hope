#if UNITY_EDITOR
using System.Collections.Generic;
using CindarsHope.World.Fishing;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.World
{
    /// <summary>
    /// fable_50 — gera os 5 assets FishingTableSO v1 a partir de <see cref="CanonicalFishingTables"/>
    /// (fonte única; sem conteúdo duplicado no Editor). Mesmo padrão dos demais geradores
    /// (EnsureFolder + CreateAsset/overwrite + AssetDatabase.SaveAssets/Refresh).
    /// </summary>
    public static class GenerateFishingTables
    {
        private const string TablesFolderParent = "Assets/_Game/Data";
        private const string TablesFolderName = "Fishing";
        private const string TablesPath = "Assets/_Game/Data/Fishing/";

        public static void Generate()
        {
            EnsureFolder(TablesFolderParent, TablesFolderName);

            foreach (var kvp in CanonicalFishingTables.BuildAll())
            {
                WriteTable(kvp.Key, kvp.Value);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GenerateFishingTables] 5 tabelas de pesca v1 geradas em " + TablesPath + ".");
        }

        private static void WriteTable(string tableId, FishingTableModel model)
        {
            var assetPath = TablesPath + tableId + ".asset";
            var table = AssetDatabase.LoadAssetAtPath<FishingTableSO>(assetPath);
            var isNew = table == null;
            if (isNew)
            {
                table = ScriptableObject.CreateInstance<FishingTableSO>();
            }

            table.TableId = tableId;
            table.DisplayName = tableId;

            var entries = new List<FishingTableEntry>();
            foreach (var entry in model.Entries)
            {
                entries.Add(new FishingTableEntry
                {
                    ItemId = entry.ItemId,
                    Weight = entry.Weight,
                    Rarity = entry.Rarity,
                    BaseQuality = entry.BaseQuality,
                    Seasons = entry.Seasons,
                    Weathers = entry.Weathers,
                    NightOnly = entry.NightOnly
                });
            }

            table.SetEntries(entries);

            if (isNew)
            {
                AssetDatabase.CreateAsset(table, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(table);
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
