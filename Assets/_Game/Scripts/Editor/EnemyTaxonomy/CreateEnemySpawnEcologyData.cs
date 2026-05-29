using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    public static class CreateEnemySpawnEcologyData
    {
        private const string SpawnFolder = "Assets/_Game/Data/EnemySpawn";
        private const string ProfilesFolder = SpawnFolder + "/Profiles";
        private const string PacksFolder = SpawnFolder + "/Packs";
        private const string LocksFolder = SpawnFolder + "/FactionLocks";

        [MenuItem("CindarsHope/SPEC 13/Create Spawn Resolver Ecology Data")]
        public static void CreateData()
        {
            EnsureFolder(ProfilesFolder);
            EnsureFolder(PacksFolder);
            EnsureFolder(LocksFolder);

            int profiles = 0;
            foreach (var definition in BuildProfileDefinitions())
            {
                var asset = LoadOrCreate<EnemySpawnProfileSO>($"{ProfilesFolder}/{definition.EnemyId}.asset", ref profiles);
                PopulateProfile(asset, definition);
                EditorUtility.SetDirty(asset);
            }

            int packs = 0;
            foreach (var definition in BuildPackDefinitions())
            {
                var asset = LoadOrCreate<EnemySpawnPackSO>($"{PacksFolder}/{definition.PackId}.asset", ref packs);
                PopulatePack(asset, definition);
                EditorUtility.SetDirty(asset);
            }

            int locks = 0;
            foreach (var definition in BuildLockDefinitions())
            {
                var asset = LoadOrCreate<EnemyFactionLockSO>($"{LocksFolder}/{definition.LockId}.asset", ref locks);
                PopulateLock(asset, definition);
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SPEC 13F] Spawn ecology data ensured. Profiles created: {profiles}, Packs created: {packs}, Faction locks created: {locks}.");
        }

        private static T LoadOrCreate<T>(string path, ref int created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            created++;
            return asset;
        }

        private static void PopulateProfile(EnemySpawnProfileSO asset, ProfileDefinition definition)
        {
            asset.SpawnProfileId = "spawn_" + definition.EnemyId;
            asset.DisplayName = ToTitle(definition.EnemyId.Replace("enemy_", string.Empty));
            asset.EnemyId = definition.EnemyId;
            asset.CaveLevelMin = definition.MinLevel;
            asset.CaveLevelMax = definition.MaxLevel;
            asset.BiomeTags = new[] { definition.BiomeTag };
            asset.EnvironmentTags = definition.EnvironmentTags;
            asset.FactionId = definition.FactionId;
            asset.FactionLockId = definition.FactionLockId;
            asset.RequiredBossGateProgress = definition.RequiredGateId;
            asset.Weight = definition.Weight;
            asset.MaxCountPerRoom = definition.MaxCountPerRoom;
            asset.CanSpawnAsElite = definition.CanSpawnAsElite;
            asset.SizeClass = definition.SizeClass;
            asset.MinimumRoomSizeForSizeClass = MinimumRoomForSize(definition.SizeClass);
            asset.AllowedRoomTags = definition.AllowedRoomTags;
            asset.DeniedRoomTags = definition.DeniedRoomTags;
            asset.PackIds = definition.PackIds;
            asset.IsEnabled = true;
        }

        private static void PopulatePack(EnemySpawnPackSO asset, PackDefinition definition)
        {
            asset.PackId = definition.PackId;
            asset.DisplayName = ToTitle(definition.PackId.Replace("pack_", string.Empty));
            asset.CaveLevelMin = definition.MinLevel;
            asset.CaveLevelMax = definition.MaxLevel;
            asset.BiomeTags = new[] { definition.BiomeTag };
            asset.EnvironmentTags = definition.EnvironmentTags;
            asset.RequiredFactionIds = definition.RequiredFactionIds;
            asset.Entries = definition.Entries.Select(e => new EnemySpawnPackEntry
            {
                EnemyId = e.EnemyId,
                MinCount = e.MinCount,
                MaxCount = e.MaxCount,
                Weight = e.Weight,
                IsRequired = e.IsRequired,
                RequiresUnlockedFactionLock = e.RequiresUnlockedFactionLock
            }).ToArray();
            asset.Weight = definition.Weight;
            asset.MinimumRoomSize = definition.MinimumRoomSize;
            asset.MaxTotalEnemies = definition.MaxTotalEnemies;
            asset.IsEnabled = definition.IsEnabled;
        }

        private static void PopulateLock(EnemyFactionLockSO asset, LockDefinition definition)
        {
            asset.FactionLockId = definition.LockId;
            asset.DisplayName = ToTitle(definition.LockId.Replace("lock_", string.Empty));
            asset.RequiredBossGateId = definition.RequiredBossGateId;
            asset.RequiredStoryFlagId = definition.RequiredStoryFlagId;
            asset.RequiredCaveLevelMin = definition.RequiredCaveLevelMin;
            asset.UnlocksFactionIds = definition.UnlocksFactionIds;
            asset.UnlocksPackIds = definition.UnlocksPackIds;
            asset.IsUnlockedByDefault = definition.IsUnlockedByDefault;
        }

        public static List<ProfileDefinition> BuildProfileDefinitions()
        {
            var packsByEnemy = BuildPackDefinitions()
                .SelectMany(p => p.Entries.Select(e => new { p.PackId, e.EnemyId }))
                .GroupBy(x => x.EnemyId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.PackId).Distinct().ToArray());

            return new List<ProfileDefinition>
            {
                P("enemy_cave_mite",1,10,"stone","beast",EnemySizeClass.Tiny,4),
                P("enemy_stone_rat",1,10,"stone","beast",EnemySizeClass.Small,3),
                P("enemy_cave_bat",1,10,"stone","beast",EnemySizeClass.Small,2),
                P("enemy_goblin_grashnaar_scavenger",1,10,"stone","goblin",EnemySizeClass.Small,3),
                P("enemy_kobold_scout",1,10,"stone","kobold",EnemySizeClass.Small,2),
                P("enemy_mossling",1,25,"fungal","fungal",EnemySizeClass.Small,2),
                P("enemy_cracked_bone",1,10,"stone","undead_weak",EnemySizeClass.Small,2),
                P("enemy_blackroot_sprout",1,25,"fungal","fungal",EnemySizeClass.Medium,1),
                P("enemy_spore_imp",11,25,"fungal","fungal",EnemySizeClass.Small,2),
                P("enemy_rootsnare",11,25,"fungal","fungal",EnemySizeClass.Medium,1),
                P("enemy_hollow_stagling",11,25,"fungal","beast",EnemySizeClass.Medium,1),
                P("enemy_goblin_urudakh_trapper",11,25,"fungal","goblin",EnemySizeClass.Small,1),
                P("enemy_thorn_archer",11,25,"fungal","goblin",EnemySizeClass.Small,1),
                P("enemy_orc_nyx_stalker",11,25,"fungal","orc_nyx",EnemySizeClass.Medium,1),
                P("enemy_mycobulwark",11,25,"fungal","fungal",EnemySizeClass.Large,1),
                P("enemy_nyx_moth",11,25,"fungal","beast",EnemySizeClass.Small,2),
                P("enemy_frost_gnawer",26,40,"ice","beast",EnemySizeClass.Small,2,"lock_after_gate_15","boss_gate_level_15"),
                P("enemy_duergar_frostdelver",26,40,"ice","duergar",EnemySizeClass.Medium,2,"lock_after_gate_15","boss_gate_level_15"),
                P("enemy_duergar_shieldbreaker",26,40,"ice","duergar",EnemySizeClass.Medium,1,"lock_after_gate_15","boss_gate_level_15"),
                P("enemy_icebound_sentinel",26,40,"ice","ice_cult",EnemySizeClass.Large,1,"lock_after_gate_15","boss_gate_level_15"),
                P("enemy_glassbone",26,40,"ice","undead_stronger",EnemySizeClass.Medium,1,"lock_after_gate_15","boss_gate_level_15"),
                P("enemy_cold_cult_acolyte",26,40,"ice","ice_cult",EnemySizeClass.Medium,1,"lock_after_gate_15","boss_gate_level_15"),
                P("enemy_crystal_leaper",26,40,"ice","beast",EnemySizeClass.Medium,1,"lock_after_gate_15","boss_gate_level_15"),
                P("enemy_frost_wailer",26,40,"ice","undead_stronger",EnemySizeClass.Medium,1,"lock_after_gate_15","boss_gate_level_15"),
                P("enemy_ember_tick",41,55,"fire","elemental_fire",EnemySizeClass.Tiny,5,"lock_after_gate_30","boss_gate_level_30"),
                P("enemy_ash_crawler",41,55,"fire","beast",EnemySizeClass.Medium,1,"lock_after_gate_30","boss_gate_level_30"),
                P("enemy_orc_kaand_berserker",41,55,"fire","orc_kaand",EnemySizeClass.Medium,1,"lock_after_gate_30","boss_gate_level_30"),
                P("enemy_orc_kaand_ashcaller",41,55,"fire","orc_kaand",EnemySizeClass.Medium,1,"lock_after_gate_30","boss_gate_level_30"),
                P("enemy_lava_bulwark",41,55,"fire","elemental_fire",EnemySizeClass.Huge,1,"lock_after_gate_30","boss_gate_level_30"),
                P("enemy_cinder_spitter",41,55,"fire","elemental_fire",EnemySizeClass.Small,2,"lock_after_gate_30","boss_gate_level_30"),
                P("enemy_scorched_cultist",41,55,"fire","furnace_construct",EnemySizeClass.Medium,1,"lock_after_gate_30","boss_gate_level_30"),
                P("enemy_furnace_warden",41,55,"fire","furnace_construct",EnemySizeClass.Large,1,"lock_after_gate_30","boss_gate_level_30"),
                P("enemy_rune_shard",56,70,"ruins","stronger_construct",EnemySizeClass.Small,4,"lock_after_gate_45","boss_gate_level_45"),
                P("enemy_clockwork_guard",56,70,"ruins","stronger_construct",EnemySizeClass.Medium,1,"lock_after_gate_45","boss_gate_level_45"),
                P("enemy_gnome_gem_madcap",56,70,"ruins","gnome_ruins",EnemySizeClass.Small,1,"lock_after_gate_45","boss_gate_level_45"),
                P("enemy_gnomorin_rune_tinker",56,70,"ruins","gnome_ruins",EnemySizeClass.Small,2,"lock_after_gate_45","boss_gate_level_45"),
                P("enemy_sealed_knight",56,70,"ruins","oathless_undead",EnemySizeClass.Large,1,"lock_after_gate_45","boss_gate_level_45"),
                P("enemy_mirror_adept",56,70,"ruins","gnome_ruins",EnemySizeClass.Medium,1,"lock_after_gate_45","boss_gate_level_45"),
                P("enemy_puzzle_golem",56,70,"ruins","stronger_construct",EnemySizeClass.Huge,1,"lock_after_gate_45","boss_gate_level_45"),
                P("enemy_oathless_shade",56,70,"ruins","oathless_undead",EnemySizeClass.Medium,1,"lock_after_gate_45","boss_gate_level_45"),
            }.Select(p =>
            {
                p.PackIds = packsByEnemy.TryGetValue(p.EnemyId, out var ids) ? ids : new string[0];
                return p;
            }).ToList();
        }

        public static List<PackDefinition> BuildPackDefinitions()
        {
            return new List<PackDefinition>
            {
                // Band 1-10: increased MaxTotal and per-entry counts to reach density target 14-24
                Pack("pack_stone_fauna_basic",1,10,"stone",14,E("enemy_cave_mite",3,8),E("enemy_stone_rat",2,5),E("enemy_cave_bat",1,3,false)),
                Pack("pack_grashnaar_kobold_scouts",1,10,"stone",14,E("enemy_goblin_grashnaar_scavenger",2,6),E("enemy_kobold_scout",2,5)),
                Pack("pack_low_undead",1,10,"stone",8,E("enemy_cracked_bone",3,6),E("enemy_cave_bat",1,2,false)),
                Pack("pack_blackroot_growth",1,25,"fungal",10,E("enemy_mossling",2,4),E("enemy_blackroot_sprout",1,3,false)),
                // Band 11-25: increased MaxTotal for density and added beast_mid pack
                Pack("pack_fungal_colony",11,25,"fungal",14,E("enemy_spore_imp",2,5),E("enemy_mossling",2,4),E("enemy_rootsnare",1,2,false)),
                Pack("pack_urudakh_trappers",11,25,"fungal",12,E("enemy_goblin_urudakh_trapper",1,3),E("enemy_thorn_archer",1,3),E("enemy_goblin_grashnaar_scavenger",1,3,false)),
                Pack("pack_nyx_ambush",11,25,"fungal",10,E("enemy_orc_nyx_stalker",1,2),E("enemy_nyx_moth",2,4)),
                Pack("pack_beast_mid",11,25,"fungal",10,E("enemy_hollow_stagling",1,3),E("enemy_nyx_moth",2,4),E("enemy_rootsnare",1,2,false)),
                Pack("pack_frozen_beasts",26,40,"ice",12,E("enemy_frost_gnawer",2,4),E("enemy_glassbone",1,2)),
                Pack("pack_duergar_patrol",26,40,"ice",12,E("enemy_duergar_frostdelver",2,4),E("enemy_duergar_shieldbreaker",1,2,false)),
                Pack("pack_ice_guardians",26,40,"ice",8,E("enemy_icebound_sentinel",1,2),E("enemy_cold_cult_acolyte",1,2)),
                Pack("pack_ember_swarm",41,55,"fire",14,E("enemy_ember_tick",4,8),E("enemy_ash_crawler",1,2)),
                Pack("pack_kaand_warband",41,55,"fire",10,E("enemy_orc_kaand_berserker",1,3),E("enemy_orc_kaand_ashcaller",1,3)),
                Pack("pack_lava_guard",41,55,"fire",10,E("enemy_lava_bulwark",1,1),E("enemy_cinder_spitter",2,5)),
                Pack("pack_furnace_guard",41,55,"fire",10,E("enemy_furnace_warden",1,2),E("enemy_scorched_cultist",1,3)),
                Pack("pack_rune_shards",56,70,"ruins",14,E("enemy_rune_shard",3,6),E("enemy_clockwork_guard",1,2)),
                Pack("pack_gnome_ruin_tinkerers",56,70,"ruins",10,E("enemy_gnome_gem_madcap",1,2),E("enemy_gnomorin_rune_tinker",2,4)),
                Pack("pack_oathless_dead",56,70,"ruins",8,E("enemy_sealed_knight",1,2),E("enemy_oathless_shade",1,3)),
                Pack("pack_puzzle_guardians",56,70,"ruins",8,E("enemy_puzzle_golem",1,1),E("enemy_mirror_adept",1,2)),
            };
        }

        public static List<LockDefinition> BuildLockDefinitions()
        {
            return new List<LockDefinition>
            {
                new LockDefinition("lock_default_low_tier", true, "", 0, new[] { "beast", "fungal", "goblin", "kobold", "undead_weak", "orc_nyx" }, new[] { "pack_stone_fauna_basic", "pack_grashnaar_kobold_scouts", "pack_blackroot_growth", "pack_low_undead", "pack_fungal_colony", "pack_urudakh_trappers", "pack_nyx_ambush", "pack_beast_mid" }),
                new LockDefinition("lock_after_gate_15", false, "boss_gate_level_15", 16, new[] { "duergar", "ice_cult", "undead_stronger" }, new[] { "pack_frozen_beasts", "pack_duergar_patrol", "pack_ice_guardians" }),
                new LockDefinition("lock_after_gate_30", false, "boss_gate_level_30", 31, new[] { "orc_kaand", "elemental_fire", "furnace_construct" }, new[] { "pack_ember_swarm", "pack_kaand_warband", "pack_lava_guard", "pack_furnace_guard" }),
                new LockDefinition("lock_after_gate_45", false, "boss_gate_level_45", 46, new[] { "gnome_ruins", "stronger_construct", "oathless_undead" }, new[] { "pack_rune_shards", "pack_gnome_ruin_tinkerers", "pack_oathless_dead", "pack_puzzle_guardians" }),
                new LockDefinition("lock_after_gate_60", false, "boss_gate_level_60", 61, new[] { "drow", "abyssal", "void" }, new[] { "pack_drow_court" }),
                new LockDefinition("lock_after_gate_75", false, "boss_gate_level_75", 76, new[] { "ninrorin", "corrupted", "draconic" }, new[] { "pack_ninrorin_broken_echoes", "pack_corrupted_draconic_nest" }),
                new LockDefinition("lock_after_gate_90", false, "boss_gate_level_90", 91, new[] { "blackstone_wyvern" }, new[] { "pack_blackstone_wyvern_arena" }),
            };
        }

        private static ProfileDefinition P(string enemyId, int min, int max, string biome, string faction, EnemySizeClass size, int maxCount, string lockId = "", string gateId = "")
        {
            return new ProfileDefinition
            {
                EnemyId = enemyId,
                MinLevel = min,
                MaxLevel = max,
                BiomeTag = biome,
                EnvironmentTags = new[] { biome },
                FactionId = faction,
                FactionLockId = lockId,
                RequiredGateId = gateId,
                SizeClass = size,
                MaxCountPerRoom = maxCount,
                Weight = maxCount,
                CanSpawnAsElite = size >= EnemySizeClass.Medium && size < EnemySizeClass.Boss,
                AllowedRoomTags = size == EnemySizeClass.Boss ? new[] { "boss" } : new string[0],
                DeniedRoomTags = size >= EnemySizeClass.Huge ? new[] { "corridor" } : new string[0]
            };
        }

        private static PackDefinition Pack(string id, int min, int max, string biome, int maxTotal, params EntryDefinition[] entries)
        {
            var factions = entries.Select(e => GuessFaction(e.EnemyId)).Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().ToArray();
            var largest = entries.Select(e => GuessSize(e.EnemyId)).DefaultIfEmpty(EnemySizeClass.Small).Max();
            return new PackDefinition
            {
                PackId = id,
                MinLevel = min,
                MaxLevel = max,
                BiomeTag = biome,
                EnvironmentTags = new[] { biome },
                RequiredFactionIds = factions,
                Entries = entries,
                Weight = 1,
                MinimumRoomSize = MinimumRoomForSize(largest),
                MaxTotalEnemies = maxTotal,
                IsEnabled = true
            };
        }

        private static EntryDefinition E(string enemyId, int min, int max, bool required = true)
        {
            return new EntryDefinition(enemyId, min, max, 1, required, string.Empty);
        }

        private static string GuessFaction(string enemyId)
        {
            return BuildProfileDefinitionsNoPacks().FirstOrDefault(p => p.EnemyId == enemyId)?.FactionId ?? string.Empty;
        }

        private static EnemySizeClass GuessSize(string enemyId)
        {
            return BuildProfileDefinitionsNoPacks().FirstOrDefault(p => p.EnemyId == enemyId)?.SizeClass ?? EnemySizeClass.Medium;
        }

        private static List<ProfileDefinition> BuildProfileDefinitionsNoPacks()
        {
            return new List<ProfileDefinition>
            {
                P("enemy_cave_mite",1,10,"stone","beast",EnemySizeClass.Tiny,4),
                P("enemy_stone_rat",1,10,"stone","beast",EnemySizeClass.Small,3),
                P("enemy_goblin_grashnaar_scavenger",1,10,"stone","goblin",EnemySizeClass.Small,3),
                P("enemy_kobold_scout",1,10,"stone","kobold",EnemySizeClass.Small,2),
                P("enemy_mossling",1,25,"fungal","fungal",EnemySizeClass.Small,2),
                P("enemy_blackroot_sprout",1,25,"fungal","fungal",EnemySizeClass.Medium,1),
                P("enemy_spore_imp",11,25,"fungal","fungal",EnemySizeClass.Small,2),
                P("enemy_rootsnare",11,25,"fungal","fungal",EnemySizeClass.Medium,1),
                P("enemy_goblin_urudakh_trapper",11,25,"fungal","goblin",EnemySizeClass.Small,1),
                P("enemy_thorn_archer",11,25,"fungal","goblin",EnemySizeClass.Small,1),
                P("enemy_orc_nyx_stalker",11,25,"fungal","orc_nyx",EnemySizeClass.Medium,1),
                P("enemy_nyx_moth",11,25,"fungal","beast",EnemySizeClass.Small,2),
                P("enemy_frost_gnawer",26,40,"ice","beast",EnemySizeClass.Small,2),
                P("enemy_duergar_frostdelver",26,40,"ice","duergar",EnemySizeClass.Medium,2),
                P("enemy_duergar_shieldbreaker",26,40,"ice","duergar",EnemySizeClass.Medium,1),
                P("enemy_icebound_sentinel",26,40,"ice","ice_cult",EnemySizeClass.Large,1),
                P("enemy_glassbone",26,40,"ice","undead_stronger",EnemySizeClass.Medium,1),
                P("enemy_cold_cult_acolyte",26,40,"ice","ice_cult",EnemySizeClass.Medium,1),
                P("enemy_ember_tick",41,55,"fire","elemental_fire",EnemySizeClass.Tiny,5),
                P("enemy_ash_crawler",41,55,"fire","beast",EnemySizeClass.Medium,1),
                P("enemy_orc_kaand_berserker",41,55,"fire","orc_kaand",EnemySizeClass.Medium,1),
                P("enemy_orc_kaand_ashcaller",41,55,"fire","orc_kaand",EnemySizeClass.Medium,1),
                P("enemy_lava_bulwark",41,55,"fire","elemental_fire",EnemySizeClass.Huge,1),
                P("enemy_cinder_spitter",41,55,"fire","elemental_fire",EnemySizeClass.Small,2),
                P("enemy_scorched_cultist",41,55,"fire","furnace_construct",EnemySizeClass.Medium,1),
                P("enemy_furnace_warden",41,55,"fire","furnace_construct",EnemySizeClass.Large,1),
                P("enemy_rune_shard",56,70,"ruins","stronger_construct",EnemySizeClass.Small,4),
                P("enemy_clockwork_guard",56,70,"ruins","stronger_construct",EnemySizeClass.Medium,1),
                P("enemy_gnome_gem_madcap",56,70,"ruins","gnome_ruins",EnemySizeClass.Small,1),
                P("enemy_gnomorin_rune_tinker",56,70,"ruins","gnome_ruins",EnemySizeClass.Small,2),
                P("enemy_sealed_knight",56,70,"ruins","oathless_undead",EnemySizeClass.Large,1),
                P("enemy_mirror_adept",56,70,"ruins","gnome_ruins",EnemySizeClass.Medium,1),
                P("enemy_puzzle_golem",56,70,"ruins","stronger_construct",EnemySizeClass.Huge,1),
                P("enemy_oathless_shade",56,70,"ruins","oathless_undead",EnemySizeClass.Medium,1),
            };
        }

        private static EnemyRoomSizeClass MinimumRoomForSize(EnemySizeClass size)
        {
            return size switch
            {
                EnemySizeClass.Tiny => EnemyRoomSizeClass.Corridor,
                EnemySizeClass.Small => EnemyRoomSizeClass.Corridor,
                EnemySizeClass.Medium => EnemyRoomSizeClass.Small,
                EnemySizeClass.Large => EnemyRoomSizeClass.Medium,
                EnemySizeClass.Huge => EnemyRoomSizeClass.Large,
                EnemySizeClass.Boss => EnemyRoomSizeClass.Arena,
                _ => EnemyRoomSizeClass.Small
            };
        }

        private static string ToTitle(string value)
        {
            return string.Join(" ", value.Split('_').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
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

        public class ProfileDefinition
        {
            public string EnemyId;
            public int MinLevel;
            public int MaxLevel;
            public string BiomeTag;
            public string[] EnvironmentTags;
            public string FactionId;
            public string FactionLockId;
            public string RequiredGateId;
            public int Weight;
            public int MaxCountPerRoom;
            public bool CanSpawnAsElite;
            public EnemySizeClass SizeClass;
            public string[] AllowedRoomTags = new string[0];
            public string[] DeniedRoomTags = new string[0];
            public string[] PackIds = new string[0];
        }

        public class PackDefinition
        {
            public string PackId;
            public int MinLevel;
            public int MaxLevel;
            public string BiomeTag;
            public string[] EnvironmentTags;
            public string[] RequiredFactionIds;
            public EntryDefinition[] Entries;
            public int Weight;
            public EnemyRoomSizeClass MinimumRoomSize;
            public int MaxTotalEnemies;
            public bool IsEnabled;
        }

        public class EntryDefinition
        {
            public string EnemyId;
            public int MinCount;
            public int MaxCount;
            public int Weight;
            public bool IsRequired;
            public string RequiresUnlockedFactionLock;

            public EntryDefinition(string enemyId, int minCount, int maxCount, int weight, bool isRequired, string requiresUnlockedFactionLock)
            {
                EnemyId = enemyId;
                MinCount = minCount;
                MaxCount = maxCount;
                Weight = weight;
                IsRequired = isRequired;
                RequiresUnlockedFactionLock = requiresUnlockedFactionLock;
            }
        }

        public class LockDefinition
        {
            public string LockId;
            public bool IsUnlockedByDefault;
            public string RequiredBossGateId;
            public string RequiredStoryFlagId;
            public int RequiredCaveLevelMin;
            public string[] UnlocksFactionIds;
            public string[] UnlocksPackIds;

            public LockDefinition(string lockId, bool isUnlockedByDefault, string requiredBossGateId, int requiredCaveLevelMin, string[] unlocksFactionIds, string[] unlocksPackIds)
            {
                LockId = lockId;
                IsUnlockedByDefault = isUnlockedByDefault;
                RequiredBossGateId = requiredBossGateId;
                RequiredStoryFlagId = string.Empty;
                RequiredCaveLevelMin = requiredCaveLevelMin;
                UnlocksFactionIds = unlocksFactionIds;
                UnlocksPackIds = unlocksPackIds;
            }
        }
    }
}
