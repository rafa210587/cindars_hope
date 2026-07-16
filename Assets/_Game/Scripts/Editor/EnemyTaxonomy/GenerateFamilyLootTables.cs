using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Loot;
using UnityEditor;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    /// <summary>
    /// fable_06 â€” Gera as TABELAS DE LOOT por famÃ­lia (EMENDA 2026-06-12: 9 famÃ­lias do catÃ¡logo) +
    /// o <see cref="LootTableDatabaseSO"/>, e atribui <c>lootTableId</c> + <c>VulnerabilityMatrixProfileId</c>
    /// a cada inimigo do roster. TambÃ©m autora os perfis de MATRIZ de vulnerabilidade por famÃ­lia
    /// (Element/Material/Status) seguindo as fichas do bestiÃ¡rio (Undead fraco Fire/Silver/Radiant e
    /// imune Poison/Bleed; Construct fraco Hammer/Lightning, etc.).
    ///
    /// Itens de drop: nomes nominais do ITEM_CATALOG Â§11 / E2.8 (item_material_*, item_essence_*).
    /// EssÃªncias: 8% comum / +25% elite / 100% miniboss-boss (ITEM_CATALOG Â§10).
    /// NÃ£o cria um segundo sistema de loot â€” usa LootTableSO/LootTableDatabaseSO existentes.
    ///
    /// Run via: CindarsHope > Generate > Loot > Generate Family Loot Tables
    /// Batchmode: -executeMethod CindarsHope.Editor.EnemyTaxonomy.GenerateFamilyLootTables.Execute
    /// </summary>
    public static class GenerateFamilyLootTables
    {
        private const string RosterFolder = "Assets/_Game/Data/Enemies/Roster";
        private const string LootFolder = "Assets/_Game/Data/Loot";
        private const string VulnFolder = "Assets/_Game/Data/Enemies/VulnerabilityProfiles";
        private const string LootDatabasePath = "Assets/_Game/Data/Loot/LootTableDatabase.asset";
        private const string VulnDatabasePath = "Assets/_Game/Data/Combat/EnemyVulnerabilityProfileDatabase.asset";

        // â”€â”€ FamÃ­lia canÃ´nica (EMENDA: 9 famÃ­lias do catÃ¡logo) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public enum Family { Insect, Plant, Humanoid, Beast, Undead, Construct, Elemental, Dragon, Aberration }

        public static void Execute() => Generate();

        public static void Generate()
        {
            EnsureFolder(LootFolder);

            // 1) Perfis de matriz de vulnerabilidade por famÃ­lia (Element/Material/Status).
            var matrixByFamily = CreateFamilyVulnerabilityMatrices();

            // 2) Tabelas de loot por famÃ­lia.
            var tableByFamily = CreateFamilyLootTables();

            // 2b) Tabelas de loot DEDICADAS de boss (lootTableId explÃ­cito no EnemyDataSO do boss,
            //     ex.: enemy_meteor_ooze_king â†’ loot_boss_meteor_ooze_king). Sem isto, o boss referencia
            //     uma LootTableSO inexistente (erro de validaÃ§Ã£o). Reusa item ids canÃ´nicos do Â§11.
            var bossTables = CreateBossLootTables();

            // 3) Bancos (registries) â€” popula com as 9 tabelas de famÃ­lia + as tabelas de boss + os 9 perfis.
            var allTables = tableByFamily.Values.Concat(bossTables.Values).ToArray();
            var lootDb = EnsureLootDatabase(allTables);
            AddMatrixProfilesToVulnerabilityDatabase(matrixByFamily.Values.ToArray());

            // 4) Atribui lootTableId + VulnerabilityMatrixProfileId a cada inimigo do roster.
            int assigned = AssignToRoster(tableByFamily, matrixByFamily);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[fable_06] Family loot generation complete. FamilyTables={tableByFamily.Count}, " +
                      $"BossTables={bossTables.Count}, MatrixProfiles={matrixByFamily.Count}, " +
                      $"EnemiesAssigned={assigned}, LootDatabase='{lootDb.name}'.");
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Family resolution from FactionId (roster uses faction_* ids)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public static Family ResolveFamily(string factionId, string enemyId)
        {
            string f = (factionId ?? string.Empty).ToLowerInvariant();
            string e = (enemyId ?? string.Empty).ToLowerInvariant();

            if (f.Contains("undead")) return Family.Undead;
            if (f.Contains("construct")) return Family.Construct;
            if (f.Contains("elemental")) return Family.Elemental;
            if (f.Contains("draconic") || e.Contains("dragon") || e.Contains("wyrm") || e.Contains("wyvern")) return Family.Dragon;
            if (f.Contains("abyssal") || f.Contains("ninrorin") || f.Contains("corrupted") || f.Contains("cultist")) return Family.Aberration;
            if (f.Contains("fungal")) return Family.Plant;
            if (f.Contains("beast"))
            {
                // Beasts that are clearly insectoid map to Insect (chitin drops).
                if (e.Contains("mite") || e.Contains("beetle") || e.Contains("grub") || e.Contains("maggot") ||
                    e.Contains("tick") || e.Contains("crawler"))
                {
                    return Family.Insect;
                }
                return Family.Beast;
            }
            // Goblin/kobold/orc/duergar/drow/gnome â†’ Humanoid.
            return Family.Humanoid;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Loot tables (ITEM_CATALOG Â§11 / E2.8 nominal ids)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static Dictionary<Family, LootTableSO> CreateFamilyLootTables()
        {
            var map = new Dictionary<Family, LootTableSO>();

            map[Family.Insect] = BuildTable("loot_family_insect", "insect",
                guaranteed: new[] { E("item_material_chitin", 1, 2) },
                weighted: new[] { W("item_material_chitin", 6, 1, 3), W("item_material_chitin_plate", 2, 1, 1, rare: true), W("item_material_sinew", 3, 1, 1) },
                essence: null);

            map[Family.Plant] = BuildTable("loot_family_plant", "plant",
                guaranteed: new[] { E("item_material_spores", 1, 2) },
                weighted: new[] { W("item_material_spores", 6, 1, 3), W("item_material_glowcap", 4, 1, 2), W("item_material_mycel_thread", 3, 1, 1), W("item_material_mycel_heart", 1, 1, 1, rare: true) },
                essence: "item_essence_toxic");

            map[Family.Humanoid] = BuildTable("loot_family_humanoid", "humanoid",
                guaranteed: new[] { E("item_material_leather", 1, 1) },
                weighted: new[] { W("item_material_leather", 5, 1, 2), W("item_material_fiber", 4, 1, 2), W("item_material_copper_ore", 2, 1, 1), W("item_material_iron_ore", 1, 1, 1, rare: true) },
                essence: null);

            map[Family.Beast] = BuildTable("loot_family_beast", "beast",
                guaranteed: new[] { E("item_material_leather", 1, 2) },
                weighted: new[] { W("item_material_leather", 6, 1, 3), W("item_material_sinew", 4, 1, 2), W("item_material_white_pelt", 2, 1, 1, rare: true), W("item_material_grub_meat", 3, 1, 2) },
                essence: null);

            map[Family.Undead] = BuildTable("loot_family_undead", "undead",
                guaranteed: new[] { E("item_material_rot_gland", 1, 1) },
                weighted: new[] { W("item_material_rot_gland", 5, 1, 2), W("item_material_shade_ash", 3, 1, 1), W("item_material_night_essence", 1, 1, 1, rare: true), W("item_material_phantom_essence", 1, 1, 1, rare: true) },
                essence: "item_essence_arcane");

            map[Family.Construct] = BuildTable("loot_family_construct", "construct",
                guaranteed: new[] { E("item_material_gears", 1, 2) },
                weighted: new[] { W("item_material_gears", 6, 1, 3), W("item_material_spark_dust", 4, 1, 2), W("item_material_turret_core", 2, 1, 1, rare: true), W("item_material_warden_core", 1, 1, 1, rare: true) },
                essence: "item_essence_lightning");

            map[Family.Elemental] = BuildTable("loot_family_elemental", "elemental",
                guaranteed: new[] { E("item_material_stone", 1, 2) },
                weighted: new[] { W("item_material_frost_core", 4, 1, 2), W("item_material_ember_fang", 4, 1, 2), W("item_material_spark_dust", 3, 1, 2), W("item_material_magma_chitin", 1, 1, 1, rare: true) },
                essence: "item_essence_fire");

            map[Family.Dragon] = BuildTable("loot_family_dragon", "dragon",
                guaranteed: new[] { E("item_material_wyrmling_scale", 1, 1) },
                weighted: new[] { W("item_material_wyrmling_scale", 4, 1, 2), W("item_material_ember_fang", 3, 1, 1), W("item_material_abyssal_fang", 2, 1, 1, rare: true) },
                essence: "item_essence_fire");

            map[Family.Aberration] = BuildTable("loot_family_aberration", "aberration",
                guaranteed: new[] { E("item_material_void_ichor", 1, 1) },
                weighted: new[] { W("item_material_void_ichor", 4, 1, 1), W("item_material_lurker_eye", 2, 1, 1, rare: true), W("item_material_night_essence", 3, 1, 1), W("item_material_abyssal_fang", 1, 1, 1, rare: true) },
                essence: "item_essence_void");

            return map;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Boss loot tables (lootTableId dedicado no EnemyDataSO do boss)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Bosses cujo EnemyDataSO referencia um lootTableId prÃ³prio (nÃ£o-famÃ­lia). Drop garantido de
        // essÃªncia elemental + materiais raros (recompensa de boss). Item ids canÃ´nicos do Â§11 jÃ¡ usados
        // nas tabelas de famÃ­lia â€” nenhum item novo inventado.
        private static Dictionary<string, LootTableSO> CreateBossLootTables()
        {
            var map = new Dictionary<string, LootTableSO>();

            // enemy_meteor_ooze_king (boss elemental/Ã­gneo de meteoro). lootTableId: loot_boss_meteor_ooze_king.
            map["loot_boss_meteor_ooze_king"] = BuildTable("loot_boss_meteor_ooze_king", "boss",
                guaranteed: new[]
                {
                    E("item_essence_fire", 2, 3),
                    E("item_material_magma_chitin", 1, 2),
                },
                weighted: new[]
                {
                    W("item_material_ember_fang", 5, 2, 4),
                    W("item_material_frost_core", 3, 1, 2),
                    W("item_material_wyrmling_scale", 2, 1, 2, rare: true),
                    W("item_material_arcane_crystal", 1, 1, 1, rare: true),
                },
                essence: "item_essence_fire");

            return map;
        }

        private static LootTableSO BuildTable(
            string tableId, string familyId, LootTableEntry[] guaranteed, LootTableEntry[] weighted, string essence)
        {
            string path = $"{LootFolder}/{tableId}.asset";
            var so = AssetDatabase.LoadAssetAtPath<LootTableSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<LootTableSO>();
                AssetDatabase.CreateAsset(so, path);
            }

            so.TableId = tableId;
            so.FamilyId = familyId;
            so.GuaranteedEntries = guaranteed ?? new LootTableEntry[0];
            so.Entries = weighted ?? new LootTableEntry[0];
            so.EssenceItemId = essence ?? string.Empty;
            so.EssenceCommonChance = 0.08f;
            so.EssenceEliteBonusChance = 0.25f;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static LootTableEntry E(string itemId, int min, int max) => new LootTableEntry
        {
            ItemId = itemId, MinAmount = min, MaxAmount = max, Weight = 1, DropChance = 1f
        };

        private static LootTableEntry W(string itemId, int weight, int min, int max, bool rare = false) => new LootTableEntry
        {
            ItemId = itemId, MinAmount = min, MaxAmount = max, Weight = weight,
            DropChance = rare ? 0.5f : 1f, IsRare = rare
        };

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Family vulnerability matrices (per CAVE_BESTIARY fichas)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static Dictionary<Family, EnemyVulnerabilityProfileSO> CreateFamilyVulnerabilityMatrices()
        {
            var map = new Dictionary<Family, EnemyVulnerabilityProfileSO>();
            EnsureFolder(VulnFolder);

            // Undead: fraco Fire/Silver/Radiant(â†’arcane proxy); imune Poison/Bleed(â†’Toxic).
            map[Family.Undead] = BuildMatrix("vulnmatrix_undead",
                elements: new[] { Elem(DamageType.Fire, 1.5f), Elem(DamageType.Arcane, 1.4f), Elem(DamageType.Toxic, 0f) },
                materials: new[] { Mat("silver", 1.5f), Mat("radiant", 1.5f) },
                statuses: new[] { Stat("status_poison", 0f), Stat("status_bleed", 0f), Stat("status_fear", 0f) });

            // Construct: fraco Hammer/Lightning; resiste Poison/Bleed.
            map[Family.Construct] = BuildMatrix("vulnmatrix_construct",
                elements: new[] { Elem(DamageType.Lightning, 1.5f), Elem(DamageType.Toxic, 0.5f) },
                materials: new[] { Mat("hammer", 1.5f) },
                statuses: new[] { Stat("status_poison", 0f), Stat("status_bleed", 0f), Stat("status_chill", 0.5f) });

            // Elemental: temÃ¡tico â€” fogo fraco a gelo e vice-versa (cobertura genÃ©rica via gelo).
            map[Family.Elemental] = BuildMatrix("vulnmatrix_elemental",
                elements: new[] { Elem(DamageType.Ice, 1.4f), Elem(DamageType.Physical, 0.85f) },
                materials: new MaterialMultiplier[0],
                statuses: new[] { Stat("status_chill", 1.5f) });

            // Plant/Fungal: fraco Fire; resiste Toxic.
            map[Family.Plant] = BuildMatrix("vulnmatrix_plant",
                elements: new[] { Elem(DamageType.Fire, 1.5f), Elem(DamageType.Toxic, 0.5f) },
                materials: new MaterialMultiplier[0],
                statuses: new[] { Stat("status_burn", 1.5f), Stat("status_poison", 0.5f) });

            // Beast: fraco a sangramento; neutro elemental.
            map[Family.Beast] = BuildMatrix("vulnmatrix_beast",
                elements: new ElementMultiplier[0],
                materials: new MaterialMultiplier[0],
                statuses: new[] { Stat("status_bleed", 1.5f) });

            // Insect: fraco Fire/esmagamento (hammer); fraco a veneno.
            map[Family.Insect] = BuildMatrix("vulnmatrix_insect",
                elements: new[] { Elem(DamageType.Fire, 1.3f) },
                materials: new[] { Mat("hammer", 1.3f) },
                statuses: new[] { Stat("status_poison", 1.5f) });

            // Humanoid: fraco a sangramento e veneno; sem fraqueza elemental marcante.
            map[Family.Humanoid] = BuildMatrix("vulnmatrix_humanoid",
                elements: new ElementMultiplier[0],
                materials: new MaterialMultiplier[0],
                statuses: new[] { Stat("status_bleed", 1.3f), Stat("status_poison", 1.3f) });

            // Dragon: resiste Fire (fortemente); fraco Ice.
            map[Family.Dragon] = BuildMatrix("vulnmatrix_dragon",
                elements: new[] { Elem(DamageType.Fire, 0.5f), Elem(DamageType.Ice, 1.4f) },
                materials: new MaterialMultiplier[0],
                statuses: new[] { Stat("status_fear", 0f) });

            // Aberration/Void: fraco a radiante (â†’arcane proxy); imune a fear/charm.
            map[Family.Aberration] = BuildMatrix("vulnmatrix_aberration",
                elements: new[] { Elem(DamageType.Arcane, 1.3f) },
                materials: new[] { Mat("radiant", 1.5f) },
                statuses: new[] { Stat("status_fear", 0f), Stat("status_charm", 0f) });

            return map;
        }

        private static EnemyVulnerabilityProfileSO BuildMatrix(
            string id, ElementMultiplier[] elements, MaterialMultiplier[] materials, StatusVulnerability[] statuses)
        {
            string path = $"{VulnFolder}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<EnemyVulnerabilityProfileSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<EnemyVulnerabilityProfileSO>();
                AssetDatabase.CreateAsset(so, path);
            }

            so.VulnerabilityProfileId = id;
            so.TriggerMode = VulnerabilityTriggerMode.AlwaysForTest; // matriz Ã© sempre-ativa (nÃ£o janela).
            so.ElementMultipliers = elements ?? new ElementMultiplier[0];
            so.MaterialMultipliers = materials ?? new MaterialMultiplier[0];
            so.StatusVulnerabilities = statuses ?? new StatusVulnerability[0];
            EditorUtility.SetDirty(so);
            return so;
        }

        private static ElementMultiplier Elem(DamageType t, float m) => new ElementMultiplier { DamageType = t, Multiplier = m };
        private static MaterialMultiplier Mat(string tag, float m) => new MaterialMultiplier { MaterialTag = tag, Multiplier = m };
        private static StatusVulnerability Stat(string id, float m) => new StatusVulnerability { StatusId = id, DurationMultiplier = m };

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Roster assignment
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static int AssignToRoster(
            Dictionary<Family, LootTableSO> tables, Dictionary<Family, EnemyVulnerabilityProfileSO> matrices)
        {
            var guids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { RosterFolder });
            int assigned = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                if (enemy == null) continue;

                var family = ResolveFamily(enemy.FactionId, enemy.enemyId);
                enemy.lootTableId = tables[family].TableId;
                enemy.VulnerabilityMatrixProfileId = matrices[family].VulnerabilityProfileId;
                EditorUtility.SetDirty(enemy);
                assigned++;
            }
            return assigned;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Registries
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static LootTableDatabaseSO EnsureLootDatabase(LootTableSO[] tables)
        {
            EnsureFolder("Assets/_Game/Data/Loot");
            var db = AssetDatabase.LoadAssetAtPath<LootTableDatabaseSO>(LootDatabasePath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<LootTableDatabaseSO>();
                AssetDatabase.CreateAsset(db, LootDatabasePath);
            }

            SetRegistryItems(db, tables.Cast<Object>().ToArray());
            EditorUtility.SetDirty(db);
            return db;
        }

        private static void AddMatrixProfilesToVulnerabilityDatabase(EnemyVulnerabilityProfileSO[] matrices)
        {
            var db = AssetDatabase.LoadAssetAtPath<EnemyVulnerabilityProfileDatabaseSO>(VulnDatabasePath);
            if (db == null)
            {
                Debug.LogWarning($"[fable_06] VulnerabilityProfileDatabase not found at '{VulnDatabasePath}'. " +
                                 "Run GenerateAndWireSpec13GAssets first. Matrix profiles created as assets but not registered.");
                return;
            }

            // Merge: existing items + new matrix profiles (avoid duplicates by id).
            var existing = LoadRegistryItems(db);
            var byId = new Dictionary<string, Object>();
            foreach (var item in existing)
            {
                if (item is EnemyVulnerabilityProfileSO p && !string.IsNullOrEmpty(p.VulnerabilityProfileId))
                    byId[p.VulnerabilityProfileId] = item;
            }
            foreach (var m in matrices)
            {
                byId[m.VulnerabilityProfileId] = m;
            }

            SetRegistryItems(db, byId.Values.ToArray());
            EditorUtility.SetDirty(db);
        }

        private static Object[] LoadRegistryItems(Object registry)
        {
            var so = new SerializedObject(registry);
            var prop = so.FindProperty("_items");
            if (prop == null || !prop.isArray) return new Object[0];
            var list = new List<Object>();
            for (int i = 0; i < prop.arraySize; i++)
            {
                list.Add(prop.GetArrayElementAtIndex(i).objectReferenceValue);
            }
            return list.ToArray();
        }

        private static void SetRegistryItems(Object registry, Object[] items)
        {
            var so = new SerializedObject(registry);
            var prop = so.FindProperty("_items");
            if (prop == null || !prop.isArray) return;
            prop.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
            so.ApplyModifiedProperties();
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
