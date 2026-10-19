using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Tools;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Cave
{
    /// <summary>
    /// fable_78 (slice 6) — cria os <see cref="ResourceNodeDataSO"/> de minério por banda (cobre → ferro →
    /// prata → cristal arcano → mithril, conforme a profundidade) e os registra no
    /// <c>ResourceNodeDatabase.asset</c>. Reusa o tipo de nó existente (regra de não-duplicação §13);
    /// NÃO cria um segundo sistema de loot/minério. IDs estáveis: <c>resnode_ore_&lt;ore&gt;_&lt;band&gt;</c>.
    ///
    /// Idempotente: nós existentes (por Id) são atualizados, não duplicados; o database faz merge por Id.
    /// Os <c>PrimaryDropItemId</c> são os ores canônicos do fable_32 (item_material_*_ore / _arcane_crystal).
    /// Best-effort, loga contagem de criados/atualizados/registrados.
    /// </summary>
    public static class GenerateCaveBiomeOreNodes
    {
        private const string Tag = "fable_78/OreNodes";
        private const string DataRoot = "Assets/_Game/Data";
        private const string CaveDir = DataRoot + "/Cave";
        private const string NodesDir = CaveDir + "/ResourceNodes";
        private const string DatabasePath = CaveDir + "/ResourceNodeDatabase.asset";

        // Banda 0..6 (Stone, Fungal, Ice, Fire, Ruins, Deep, Void). Progressão de minério por profundidade.
        private struct OreNodeSpec
        {
            public int Band;
            public string OreKey;        // sufixo do Id (ex.: "copper")
            public string DisplayName;
            public string DropItemId;    // ore canônico fable_32
            public ToolTier RequiredTier;
            public int HitsRequired;
        }

        public static void Generate()
        {
            EnsureFolders();

            var specs = BuildOreSpecs();
            var created = 0;
            var updated = 0;
            var assets = new List<ResourceNodeDataSO>();

            foreach (var spec in specs)
            {
                var id = BuildNodeId(spec.OreKey, spec.Band);
                var path = $"{NodesDir}/{id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<ResourceNodeDataSO>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<ResourceNodeDataSO>();
                    AssetDatabase.CreateAsset(asset, path);
                    created++;
                }
                else
                {
                    updated++;
                }

                asset.Id = id;
                asset.DisplayName = spec.DisplayName;
                asset.RequiredToolType = ToolType.Pickaxe;
                asset.RequiredToolTier = spec.RequiredTier;
                asset.StaminaCost = 1;
                asset.HitsRequired = spec.HitsRequired;
                asset.PrimaryDropItemId = spec.DropItemId;
                asset.PrimaryDropAmount = 1;
                asset.RespawnsDaily = false;
                asset.FallbackItemId = "item_material_stone";
                asset.FallbackAmount = 1;
                asset.FallbackDepletesNode = false;
                EditorUtility.SetDirty(asset);
                assets.Add(asset);
            }

            RegisterInDatabase(assets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[{Tag}] {created} criados, {updated} atualizados, {assets.Count} registrados no ResourceNodeDatabase. " +
                      $"Pasta: {NodesDir}. DEFERRED_UNITY: garanta o database ligado nos consumidores da CaveScene.");
        }

        // Public static para -executeMethod em batchmode (rule unity-assets / generated-asset-evidence).
        public static void GenerateBatch() => Generate();

        private static List<OreNodeSpec> BuildOreSpecs()
        {
            // Cada banda ganha 1-2 nós: o minério "base" da banda + um veio raro mais profundo onde faz sentido.
            // Tier de pickaxe cresce com a banda; cristal arcano/mithril ficam nas bandas Deep/Void (raros).
            return new List<OreNodeSpec>
            {
                // Banda 0 — Stone (1-10): cobre comum, ferro como veio melhor.
                new OreNodeSpec { Band = 0, OreKey = "copper", DisplayName = "Veio de Cobre", DropItemId = "item_material_copper_ore", RequiredTier = ToolTier.Basic, HitsRequired = 2 },
                new OreNodeSpec { Band = 0, OreKey = "iron", DisplayName = "Veio de Ferro", DropItemId = "item_material_iron_ore", RequiredTier = ToolTier.Copper, HitsRequired = 3 },

                // Banda 1 — Fungal (11-25): cobre + ferro.
                new OreNodeSpec { Band = 1, OreKey = "copper", DisplayName = "Veio de Cobre", DropItemId = "item_material_copper_ore", RequiredTier = ToolTier.Basic, HitsRequired = 2 },
                new OreNodeSpec { Band = 1, OreKey = "iron", DisplayName = "Veio de Ferro", DropItemId = "item_material_iron_ore", RequiredTier = ToolTier.Copper, HitsRequired = 3 },

                // Banda 2 — Ice (26-40): ferro + prata.
                new OreNodeSpec { Band = 2, OreKey = "iron", DisplayName = "Veio de Ferro", DropItemId = "item_material_iron_ore", RequiredTier = ToolTier.Copper, HitsRequired = 3 },
                new OreNodeSpec { Band = 2, OreKey = "silver", DisplayName = "Veio de Prata", DropItemId = "item_material_silver_ore", RequiredTier = ToolTier.Iron, HitsRequired = 3 },

                // Banda 3 — Fire (41-55): ferro + prata.
                new OreNodeSpec { Band = 3, OreKey = "iron", DisplayName = "Veio de Ferro Igneo", DropItemId = "item_material_iron_ore", RequiredTier = ToolTier.Copper, HitsRequired = 3 },
                new OreNodeSpec { Band = 3, OreKey = "silver", DisplayName = "Veio de Prata", DropItemId = "item_material_silver_ore", RequiredTier = ToolTier.Iron, HitsRequired = 4 },

                // Banda 4 — Ruins (56-70): prata + cristal arcano (começa raro).
                new OreNodeSpec { Band = 4, OreKey = "silver", DisplayName = "Veio de Prata", DropItemId = "item_material_silver_ore", RequiredTier = ToolTier.Iron, HitsRequired = 4 },
                new OreNodeSpec { Band = 4, OreKey = "arcane_crystal", DisplayName = "Veio de Cristal Arcano", DropItemId = "item_material_arcane_crystal", RequiredTier = ToolTier.Gold, HitsRequired = 5 },

                // Banda 5 — Deep (71-85): cristal arcano + mithril (raros).
                new OreNodeSpec { Band = 5, OreKey = "arcane_crystal", DisplayName = "Veio de Cristal Arcano", DropItemId = "item_material_arcane_crystal", RequiredTier = ToolTier.Gold, HitsRequired = 5 },
                new OreNodeSpec { Band = 5, OreKey = "mithril", DisplayName = "Veio de Mithril", DropItemId = "item_material_mithril_ore", RequiredTier = ToolTier.Diamond, HitsRequired = 6 },

                // Banda 6 — Void (86-101+): cristal arcano + mithril (raros, mais profundos).
                new OreNodeSpec { Band = 6, OreKey = "arcane_crystal", DisplayName = "Veio de Cristal Arcano", DropItemId = "item_material_arcane_crystal", RequiredTier = ToolTier.Gold, HitsRequired = 6 },
                new OreNodeSpec { Band = 6, OreKey = "mithril", DisplayName = "Veio de Mithril", DropItemId = "item_material_mithril_ore", RequiredTier = ToolTier.Diamond, HitsRequired = 6 },
            };
        }

        internal static string BuildNodeId(string oreKey, int band) => $"resnode_ore_{oreKey}_b{band}";

        private static void RegisterInDatabase(List<ResourceNodeDataSO> nodeAssets)
        {
            var database = AssetDatabase.LoadAssetAtPath<ResourceNodeDatabaseSO>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ResourceNodeDatabaseSO>();
                AssetDatabase.CreateAsset(database, DatabasePath);
                Debug.Log($"[{Tag}] ResourceNodeDatabase criado em {DatabasePath}.");
            }

            var serialized = new SerializedObject(database);
            var itemsProperty = serialized.FindProperty("_items");

            // Preserva entradas não-fable_78; substitui/insere as nossas por Id (merge idempotente).
            var merged = new List<ResourceNodeDataSO>();
            for (var i = 0; i < itemsProperty.arraySize; i++)
            {
                var existing = itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue as ResourceNodeDataSO;
                if (existing != null && !nodeAssets.Exists(a => a.Id == existing.Id))
                {
                    merged.Add(existing);
                }
            }

            merged.AddRange(nodeAssets);

            itemsProperty.arraySize = merged.Count;
            for (var i = 0; i < merged.Count; i++)
            {
                itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = merged[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(DataRoot))
            {
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            }

            if (!AssetDatabase.IsValidFolder(CaveDir))
            {
                AssetDatabase.CreateFolder(DataRoot, "Cave");
            }

            if (!AssetDatabase.IsValidFolder(NodesDir))
            {
                AssetDatabase.CreateFolder(CaveDir, "ResourceNodes");
                Debug.Log($"[{Tag}] Pasta criada: {NodesDir}");
            }
        }
    }
}
