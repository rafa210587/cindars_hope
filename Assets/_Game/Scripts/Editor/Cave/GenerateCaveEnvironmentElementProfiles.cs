using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Cave
{
    /// <summary>
    /// fable_78 (slice 6) — cria 1 <see cref="CaveEnvironmentElementProfileSO"/> por bioma/banda
    /// (Stone, Fungal, Ice, Fire, Ruins, Deep, Void) com perfil temático, e registra todos no
    /// <c>CaveEnvironmentElementDatabase.asset</c>. Pedras e minério aparecem em TODAS as bandas
    /// (mistura/raridade varia); fungos no Fungal; lago + cristais no Ice; lava/obsidiana no Fire;
    /// destroços no Ruins; mithril/arcane raros no Deep/Void. <c>HasWater</c> liga criaturas aquáticas
    /// (Ice; outros conforme tema). Os <c>MineNodeDataId</c> referenciam os nós gerados por
    /// <see cref="GenerateCaveBiomeOreNodes"/> (mesmo Id estável — rode-o ANTES ou junto).
    ///
    /// IDs estáveis: <c>cave_elem_profile_&lt;biome&gt;</c>. Idempotente (merge por Id). Best-effort, loga.
    /// Campos privados são preenchidos via SerializedObject (o SO expõe só getters).
    /// </summary>
    public static class GenerateCaveEnvironmentElementProfiles
    {
        private const string Tag = "fable_78/ElementProfiles";
        private const string DataRoot = "Assets/_Game/Data";
        private const string CaveDir = DataRoot + "/Cave";
        private const string ProfilesDir = CaveDir + "/ElementProfiles";
        private const string DatabasePath = CaveDir + "/CaveEnvironmentElementDatabase.asset";
        // Bugfix 2026-07-04: CaveScene nunca teve _environmentElementDatabase wireado (fileID: 0) ->
        // decor de fable_78 nao aparecia em Play Mode. Mesmo precedente de
        // GenerateCaveBiomeArtProfiles/CaveBiomeArtProfileRegistry.asset (fallback via Resources.Load
        // em CaveRuntimeMaterializer quando o serialized field da cena esta vazio).
        private const string ResourcesDir = "Assets/_Game/Resources";
        private const string ResourcesDatabasePath = ResourcesDir + "/CaveEnvironmentElementDatabase.asset";

        private struct ProfileSpec
        {
            public int Band;
            public string BiomeKey;       // sufixo do Id/biomeId (ex.: "stone")
            public bool HasWater;
            public string[] AllowedOreTiers;
            public List<EntrySpec> Entries;
        }

        private struct EntrySpec
        {
            public CaveEnvironmentElementKind Kind;
            public float Weight;
            public string MineNodeDataId; // vazio quando não-minerável
        }

        public static void Generate()
        {
            EnsureFolders();

            var specs = BuildProfileSpecs();
            var created = 0;
            var updated = 0;
            var assets = new List<CaveEnvironmentElementProfileSO>();

            foreach (var spec in specs)
            {
                var id = $"cave_elem_profile_{spec.BiomeKey}";
                var biomeId = $"biome_cave_{spec.BiomeKey}";
                var path = $"{ProfilesDir}/{id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<CaveEnvironmentElementProfileSO>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<CaveEnvironmentElementProfileSO>();
                    AssetDatabase.CreateAsset(asset, path);
                    created++;
                }
                else
                {
                    updated++;
                }

                PopulateProfile(asset, id, biomeId, spec);
                EditorUtility.SetDirty(asset);
                assets.Add(asset);
            }

            RegisterInDatabase(assets);
            var resourcesStatus = MaterializeResourcesDatabase(assets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[{Tag}] {created} criados, {updated} atualizados, {assets.Count} registrados no " +
                      $"CaveEnvironmentElementDatabase. Pasta: {ProfilesDir}. " +
                      $"Resources/CaveEnvironmentElementDatabase {resourcesStatus} ({assets.Count} profile(s)) — " +
                      "fallback de runtime quando a CaveScene nao tiver o campo _environmentElementDatabase wireado.");
        }

        // Bugfix 2026-07-04: copia idempotente do database (mesmos assets de perfil, por referência
        // GUID) para Assets/_Game/Resources, para que CaveRuntimeMaterializer.ResolveEnvironmentElementDatabase()
        // encontre via Resources.Load quando a cena não wireou o serialized field.
        private static string MaterializeResourcesDatabase(List<CaveEnvironmentElementProfileSO> profileAssets)
        {
            EnsureFolder(ResourcesDir);

            var database = AssetDatabase.LoadAssetAtPath<CaveEnvironmentElementDatabaseSO>(ResourcesDatabasePath);
            var isNew = database == null;
            if (isNew)
            {
                database = ScriptableObject.CreateInstance<CaveEnvironmentElementDatabaseSO>();
            }

            var serialized = new SerializedObject(database);
            var itemsProperty = serialized.FindProperty("_items");
            itemsProperty.arraySize = profileAssets.Count;
            for (var i = 0; i < profileAssets.Count; i++)
            {
                itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = profileAssets[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (isNew)
            {
                AssetDatabase.CreateAsset(database, ResourcesDatabasePath);
            }
            else
            {
                EditorUtility.SetDirty(database);
            }

            return isNew ? "criado" : "atualizado";
        }

        // Public static para -executeMethod em batchmode.
        public static void GenerateBatch() => Generate();

        private static void PopulateProfile(CaveEnvironmentElementProfileSO asset, string id, string biomeId, ProfileSpec spec)
        {
            var so = new SerializedObject(asset);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_biomeId").stringValue = biomeId;
            so.FindProperty("_band").intValue = spec.Band;
            so.FindProperty("_hasWater").boolValue = spec.HasWater;

            var oreTiers = so.FindProperty("_allowedOreTiers");
            oreTiers.arraySize = spec.AllowedOreTiers.Length;
            for (var i = 0; i < spec.AllowedOreTiers.Length; i++)
            {
                oreTiers.GetArrayElementAtIndex(i).stringValue = spec.AllowedOreTiers[i];
            }

            var entries = so.FindProperty("_entries");
            entries.arraySize = spec.Entries.Count;
            for (var i = 0; i < spec.Entries.Count; i++)
            {
                var element = entries.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Kind").enumValueIndex = (int)spec.Entries[i].Kind;
                element.FindPropertyRelative("Weight").floatValue = spec.Entries[i].Weight;
                element.FindPropertyRelative("MineNodeDataId").stringValue = spec.Entries[i].MineNodeDataId ?? string.Empty;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static List<ProfileSpec> BuildProfileSpecs()
        {
            return new List<ProfileSpec>
            {
                // 0 — Stone: pedras + cobre/ferro (sem água).
                new ProfileSpec
                {
                    Band = 0, BiomeKey = "stone", HasWater = false,
                    AllowedOreTiers = new[] { "copper", "iron" },
                    Entries = new List<EntrySpec>
                    {
                        Decor(3f), Block(1f),
                        Mine(2f, "copper", 0), Mine(1f, "iron", 0),
                    },
                },
                // 1 — Fungal: fungos gigantes + cogumelos (decor) + pedras + cobre/ferro.
                new ProfileSpec
                {
                    Band = 1, BiomeKey = "fungal", HasWater = false,
                    AllowedOreTiers = new[] { "copper", "iron" },
                    Entries = new List<EntrySpec>
                    {
                        Decor(4f), Block(2f),
                        Mine(2f, "copper", 1), Mine(1f, "iron", 1),
                    },
                },
                // 2 — Ice: lago congelado (água → aquáticos) + cristais (decor) + ferro/prata.
                new ProfileSpec
                {
                    Band = 2, BiomeKey = "ice", HasWater = true,
                    AllowedOreTiers = new[] { "iron", "silver" },
                    Entries = new List<EntrySpec>
                    {
                        Decor(3f), Block(1f), Water(2f),
                        Mine(2f, "iron", 2), Mine(1f, "silver", 2),
                    },
                },
                // 3 — Fire: veios de obsidiana/lava (block) + minério ígneo (ferro/prata), sem água.
                new ProfileSpec
                {
                    Band = 3, BiomeKey = "fire", HasWater = false,
                    AllowedOreTiers = new[] { "iron", "silver" },
                    Entries = new List<EntrySpec>
                    {
                        Decor(2f), Block(3f),
                        Mine(2f, "iron", 3), Mine(1f, "silver", 3),
                    },
                },
                // 4 — Ruins: destroços (decor/block) + prata + cristal arcano raro.
                new ProfileSpec
                {
                    Band = 4, BiomeKey = "ruins", HasWater = false,
                    AllowedOreTiers = new[] { "silver", "arcane_crystal" },
                    Entries = new List<EntrySpec>
                    {
                        Decor(3f), Block(2f),
                        Mine(2f, "silver", 4), Mine(0.6f, "arcane_crystal", 4),
                    },
                },
                // 5 — Deep: pedras + mithril/arcane raros + um lago profundo (água).
                new ProfileSpec
                {
                    Band = 5, BiomeKey = "deep", HasWater = true,
                    AllowedOreTiers = new[] { "arcane_crystal", "mithril" },
                    Entries = new List<EntrySpec>
                    {
                        Decor(2f), Block(2f), Water(1f),
                        Mine(0.8f, "arcane_crystal", 5), Mine(0.5f, "mithril", 5),
                    },
                },
                // 6 — Void: pedras + mithril/arcane muito raros (sem água).
                new ProfileSpec
                {
                    Band = 6, BiomeKey = "void", HasWater = false,
                    AllowedOreTiers = new[] { "arcane_crystal", "mithril" },
                    Entries = new List<EntrySpec>
                    {
                        Decor(2f), Block(2f),
                        Mine(0.6f, "arcane_crystal", 6), Mine(0.4f, "mithril", 6),
                    },
                },
            };
        }

        private static EntrySpec Decor(float weight) =>
            new EntrySpec { Kind = CaveEnvironmentElementKind.DecorNonBlocking, Weight = weight, MineNodeDataId = string.Empty };

        private static EntrySpec Block(float weight) =>
            new EntrySpec { Kind = CaveEnvironmentElementKind.DecorBlocking, Weight = weight, MineNodeDataId = string.Empty };

        private static EntrySpec Water(float weight) =>
            new EntrySpec { Kind = CaveEnvironmentElementKind.WaterTile, Weight = weight, MineNodeDataId = string.Empty };

        private static EntrySpec Mine(float weight, string oreKey, int band) =>
            new EntrySpec
            {
                Kind = CaveEnvironmentElementKind.MineableNode,
                Weight = weight,
                MineNodeDataId = GenerateCaveBiomeOreNodes.BuildNodeId(oreKey, band),
            };

        private static void RegisterInDatabase(List<CaveEnvironmentElementProfileSO> profileAssets)
        {
            var database = AssetDatabase.LoadAssetAtPath<CaveEnvironmentElementDatabaseSO>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CaveEnvironmentElementDatabaseSO>();
                AssetDatabase.CreateAsset(database, DatabasePath);
                Debug.Log($"[{Tag}] CaveEnvironmentElementDatabase criado em {DatabasePath}.");
            }

            var serialized = new SerializedObject(database);
            var itemsProperty = serialized.FindProperty("_items");

            var merged = new List<CaveEnvironmentElementProfileSO>();
            for (var i = 0; i < itemsProperty.arraySize; i++)
            {
                var existing = itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue as CaveEnvironmentElementProfileSO;
                if (existing != null && !profileAssets.Exists(a => a.Id == existing.Id))
                {
                    merged.Add(existing);
                }
            }

            merged.AddRange(profileAssets);

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

            if (!AssetDatabase.IsValidFolder(ProfilesDir))
            {
                AssetDatabase.CreateFolder(CaveDir, "ElementProfiles");
                Debug.Log($"[{Tag}] Pasta criada: {ProfilesDir}");
            }
        }

        // Bugfix 2026-07-04: Assets/_Game/Resources já existe no projeto (CombatRuntimeDatabasesRegistry,
        // CaveBiomeArtProfileRegistry), mas o helper é mínimo/best-effort caso não exista ainda.
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
