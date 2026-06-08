using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Combat;
using CindarsHope.Core.Events;
using CindarsHope.Editor.EnemyTaxonomy;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec13SpawnResolverEcology
    {
        [MenuItem("CindarsHope/Archive/Validation/Validate SPEC 13F - Spawn Resolver Ecology")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var passed = new List<string>();

            ValidateTypes(errors, passed);
            ValidateAssets(errors, warnings, passed);
            ValidateResolverScenarios(errors, passed);
            ValidateEvents(errors, passed);
            ValidateForbiddenRuntimeSearch(errors, passed);

            Debug.Log($"[SPEC 13F Validation] PASSED: {passed.Count} | WARNINGS: {warnings.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed) Debug.Log($"  [OK]   {p}");
            foreach (var w in warnings) Debug.LogWarning($"  [WARN] {w}");
            foreach (var e in errors) Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 13F] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 13F] Validation FAILED with {errors.Count} error(s).");
        }

        private static void ValidateTypes(List<string> errors, List<string> passed)
        {
            RequireType(typeof(EnemySpawnResolver), "EnemySpawnResolver", errors, passed);
            RequireType(typeof(EnemySpawnProfileSO), "EnemySpawnProfileSO", errors, passed);
            RequireType(typeof(EnemySpawnPackSO), "EnemySpawnPackSO", errors, passed);
            RequireType(typeof(EnemyFactionLockSO), "EnemyFactionLockSO", errors, passed);
            RequireType(typeof(EnemySpawnRequest), "EnemySpawnRequest", errors, passed);
            RequireType(typeof(EnemySpawnResult), "EnemySpawnResult", errors, passed);
            RequireType(typeof(EnemySpawnCandidate), "EnemySpawnCandidate", errors, passed);
            RequireType(typeof(EnemyRoomSizeClass), "EnemyRoomSizeClass", errors, passed);
        }

        private static void ValidateAssets(List<string> errors, List<string> warnings, List<string> passed)
        {
            var profiles = AssetDatabase.FindAssets("t:EnemySpawnProfileSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemySpawnProfileSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            var packs = AssetDatabase.FindAssets("t:EnemySpawnPackSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemySpawnPackSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            var locks = AssetDatabase.FindAssets("t:EnemyFactionLockSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyFactionLockSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            if (profiles.Count == 0)
                warnings.Add("No EnemySpawnProfileSO assets found. Run CindarsHope > SPEC 13 > Create Spawn Resolver Ecology Data.");
            else
                passed.Add($"EnemySpawnProfileSO assets found: {profiles.Count}.");

            if (packs.Count == 0)
                warnings.Add("No EnemySpawnPackSO assets found. Run CindarsHope > SPEC 13 > Create Spawn Resolver Ecology Data.");
            else
                passed.Add($"EnemySpawnPackSO assets found: {packs.Count}.");

            if (locks.Count == 0)
                warnings.Add("No EnemyFactionLockSO assets found. Run CindarsHope > SPEC 13 > Create Spawn Resolver Ecology Data.");
            else
                passed.Add($"EnemyFactionLockSO assets found: {locks.Count}.");

            var requiredProfileIds = CreateEnemySpawnEcologyData.BuildProfileDefinitions().Select(p => p.EnemyId).ToHashSet();
            var profileIds = profiles.Select(p => p.EnemyId).ToHashSet();
            foreach (var id in requiredProfileIds)
            {
                if (profiles.Count > 0 && !profileIds.Contains(id))
                    errors.Add($"Missing EnemySpawnProfileSO for '{id}'.");
            }

            var requiredPackIds = CreateEnemySpawnEcologyData.BuildPackDefinitions().Select(p => p.PackId).ToHashSet();
            var packIds = packs.Select(p => p.PackId).ToHashSet();
            foreach (var id in requiredPackIds)
            {
                if (packs.Count > 0 && !packIds.Contains(id))
                    errors.Add($"Missing EnemySpawnPackSO '{id}'.");
            }

            foreach (var pack in packs)
            {
                ValidatePack(pack, profileIds, errors);
            }

            var requiredLockIds = CreateEnemySpawnEcologyData.BuildLockDefinitions().Select(l => l.LockId).ToHashSet();
            var lockIds = locks.Select(l => l.FactionLockId).ToHashSet();
            foreach (var id in requiredLockIds)
            {
                if (locks.Count > 0 && !lockIds.Contains(id))
                    errors.Add($"Missing EnemyFactionLockSO '{id}'.");
            }
        }

        private static void ValidatePack(EnemySpawnPackSO pack, HashSet<string> profileEnemyIds, List<string> errors)
        {
            if (pack.Entries == null || pack.Entries.Length == 0)
            {
                errors.Add($"Pack '{pack.PackId}' has no entries.");
                return;
            }

            int requiredMin = 0;
            foreach (var entry in pack.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EnemyId))
                {
                    errors.Add($"Pack '{pack.PackId}' has an empty entry.");
                    continue;
                }

                if (profileEnemyIds.Count > 0 && !profileEnemyIds.Contains(entry.EnemyId))
                    errors.Add($"Pack '{pack.PackId}' references EnemyId '{entry.EnemyId}' without spawn profile.");

                if (entry.MaxCount < entry.MinCount)
                    errors.Add($"Pack '{pack.PackId}' entry '{entry.EnemyId}' MaxCount < MinCount.");

                if (entry.IsRequired)
                    requiredMin += Math.Max(0, entry.MinCount);
            }

            if (requiredMin > pack.MaxTotalEnemies)
                errors.Add($"Pack '{pack.PackId}' required minimum {requiredMin} exceeds MaxTotalEnemies {pack.MaxTotalEnemies}.");
        }

        private static void ValidateResolverScenarios(List<string> errors, List<string> passed)
        {
            var profiles = CreateRuntimeProfiles();
            var packs = CreateRuntimePacks();
            var locks = CreateRuntimeLocks();
            var resolver = new EnemySpawnResolver(profiles, packs, locks);

            var request = Request(5, "stone", EnemyRoomSizeClass.Small, 123, 4);
            var first = resolver.Resolve(request);
            var second = resolver.Resolve(Request(5, "stone", EnemyRoomSizeClass.Small, 123, 4));
            if (first.IsValid && second.IsValid && Serialize(first) == Serialize(second))
                passed.Add("Resolver is deterministic for fixed seed.");
            else
                errors.Add("Resolver is not deterministic for fixed seed.");

            ExpectValid(resolver.Resolve(Request(5, "stone", EnemyRoomSizeClass.Small, 111, 4)), "Band 1-10 stone packs resolve.", errors, passed);
            ExpectValid(resolver.Resolve(Request(18, "fungal", EnemyRoomSizeClass.Medium, 222, 5)), "Band 11-25 fungal packs resolve.", errors, passed);
            ExpectValid(resolver.Resolve(Request(30, "ice", EnemyRoomSizeClass.Medium, 333, 4, "lock_after_gate_15", "boss_gate_level_15")), "Band 26-40 ice packs resolve with gate 15.", errors, passed);
            ExpectValid(resolver.Resolve(Request(45, "fire", EnemyRoomSizeClass.Large, 444, 6, "lock_after_gate_30", "boss_gate_level_30")), "Band 41-55 fire packs resolve with gate 30.", errors, passed);
            ExpectValid(resolver.Resolve(Request(60, "ruins", EnemyRoomSizeClass.Large, 555, 5, "lock_after_gate_45", "boss_gate_level_45")), "Band 56-70 ruins packs resolve with gate 45.", errors, passed);

            var hugeInSmall = new EnemySpawnResolver(
                profiles.Where(p => p.EnemyId == "enemy_lava_bulwark"),
                null,
                locks).Resolve(Request(45, "fire", EnemyRoomSizeClass.Small, 666, 1, "lock_after_gate_30", "boss_gate_level_30"));
            if (!hugeInSmall.IsValid && hugeInSmall.RejectedCandidates.Any(r => r.Reason.Contains("Room", StringComparison.OrdinalIgnoreCase)))
                passed.Add("Large/Huge/Boss size filtering rejects too-small rooms.");
            else
                errors.Add("Huge enemy was not rejected from small room.");

            var locked = resolver.Resolve(Request(30, "ice", EnemyRoomSizeClass.Medium, 777, 3));
            if (!locked.IsValid && locked.RejectedCandidates.Any(r => r.Reason.Contains("locked", StringComparison.OrdinalIgnoreCase) || r.Reason.Contains("missing", StringComparison.OrdinalIgnoreCase)))
                passed.Add("Closed faction/boss lock filters gated candidates.");
            else
                errors.Add("Closed faction/boss lock did not filter gated candidates.");

            var unlocked = resolver.Resolve(Request(30, "ice", EnemyRoomSizeClass.Medium, 777, 3, "lock_after_gate_15", "boss_gate_level_15"));
            ExpectValid(unlocked, "Open faction/boss lock allows gated candidates.", errors, passed);

            var empty = resolver.Resolve(Request(99, "nonexistent_biome", EnemyRoomSizeClass.Small, 888, 3));
            if (!empty.IsValid && empty.Warnings.Count > 0)
                passed.Add("Resolver returns explicit warning when no candidates exist.");
            else
                errors.Add("Resolver did not return clear warning for empty candidate set.");
        }

        private static void ValidateEvents(List<string> errors, List<string> passed)
        {
            ValidateSimpleEvent(typeof(EnemySpawnResolvedEvent), errors, passed);
            ValidateSimpleEvent(typeof(EnemySpawnResolverWarningEvent), errors, passed);
            ValidateSimpleEvent(typeof(EnemySpawnPackSelectedEvent), errors, passed);
        }

        private static void ValidateForbiddenRuntimeSearch(List<string> errors, List<string> passed)
        {
            string[] files =
            {
                "Assets/_Game/Scripts/Enemy/EnemySpawnResolver.cs",
                "Assets/_Game/Scripts/Enemy/EnemySpawnProfileSO.cs",
                "Assets/_Game/Scripts/Enemy/EnemySpawnPackSO.cs",
                "Assets/_Game/Scripts/Enemy/EnemyFactionLockSO.cs",
                "Assets/_Game/Scripts/Enemy/EnemySpawnRequest.cs",
                "Assets/_Game/Scripts/Enemy/EnemySpawnResult.cs"
            };

            foreach (var file in files)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(file);
                if (asset == null)
                {
                    errors.Add($"Runtime file missing: {file}");
                    continue;
                }

                if (asset.text.Contains("GameObject.Find") ||
                    asset.text.Contains("FindObjectOfType") ||
                    asset.text.Contains("FindObjectsByType"))
                {
                    errors.Add($"{file} contains prohibited runtime scene search.");
                }
            }

            passed.Add("SPEC 13F runtime files do not use prohibited scene searches.");
        }

        private static void RequireType(Type type, string name, List<string> errors, List<string> passed)
        {
            if (type != null) passed.Add($"{name} exists.");
            else errors.Add($"{name} missing.");
        }

        private static void ValidateSimpleEvent(Type type, List<string> errors, List<string> passed)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType != typeof(string) &&
                    field.FieldType != typeof(int) &&
                    field.FieldType != typeof(string[]))
                {
                    errors.Add($"{type.Name}.{field.Name} uses non-simple type {field.FieldType.Name}.");
                }
            }

            passed.Add($"{type.Name} payload uses simple fields.");
        }

        private static void ExpectValid(EnemySpawnResult result, string message, List<string> errors, List<string> passed)
        {
            if (result.IsValid && result.SelectedEnemies.Count > 0)
                passed.Add(message);
            else
                errors.Add(message + " FAILED.");
        }

        private static string Serialize(EnemySpawnResult result)
        {
            return $"{result.SelectedPackId}:{string.Join("|", result.SelectedEnemies.Select(e => $"{e.EnemyId}:{e.Count}:{e.SpawnProfileId}:{e.PackId}"))}";
        }

        private static EnemySpawnRequest Request(int level, string biome, EnemyRoomSizeClass room, int seed, int max, string unlockedLock = "", string bossGate = "")
        {
            var request = new EnemySpawnRequest
            {
                CaveLevel = level,
                RoomSizeClass = room,
                Seed = seed,
                MaxEnemies = max,
                DebugReason = "SPEC13F validator"
            };
            request.BiomeTags.Add(biome);
            request.EnvironmentTags.Add(biome);
            if (!string.IsNullOrWhiteSpace(unlockedLock)) request.UnlockedFactionLockIds.Add(unlockedLock);
            if (!string.IsNullOrWhiteSpace(bossGate)) request.BossGateProgressIds.Add(bossGate);
            return request;
        }

        private static List<EnemySpawnProfileSO> CreateRuntimeProfiles()
        {
            return CreateEnemySpawnEcologyData.BuildProfileDefinitions().Select(d =>
            {
                var profile = ScriptableObject.CreateInstance<EnemySpawnProfileSO>();
                profile.SpawnProfileId = "spawn_" + d.EnemyId;
                profile.EnemyId = d.EnemyId;
                profile.CaveLevelMin = d.MinLevel;
                profile.CaveLevelMax = d.MaxLevel;
                profile.BiomeTags = new[] { d.BiomeTag };
                profile.EnvironmentTags = d.EnvironmentTags;
                profile.FactionId = d.FactionId;
                profile.FactionLockId = d.FactionLockId;
                profile.RequiredBossGateProgress = d.RequiredGateId;
                profile.Weight = d.Weight;
                profile.MaxCountPerRoom = d.MaxCountPerRoom;
                profile.CanSpawnAsElite = d.CanSpawnAsElite;
                profile.SizeClass = d.SizeClass;
                profile.MinimumRoomSizeForSizeClass = d.SizeClass switch
                {
                    EnemySizeClass.Tiny => EnemyRoomSizeClass.Corridor,
                    EnemySizeClass.Small => EnemyRoomSizeClass.Corridor,
                    EnemySizeClass.Medium => EnemyRoomSizeClass.Small,
                    EnemySizeClass.Large => EnemyRoomSizeClass.Medium,
                    EnemySizeClass.Huge => EnemyRoomSizeClass.Large,
                    EnemySizeClass.Boss => EnemyRoomSizeClass.Arena,
                    _ => EnemyRoomSizeClass.Small
                };
                profile.AllowedRoomTags = d.AllowedRoomTags;
                profile.DeniedRoomTags = d.DeniedRoomTags;
                profile.PackIds = d.PackIds;
                profile.IsEnabled = true;
                return profile;
            }).ToList();
        }

        private static List<EnemySpawnPackSO> CreateRuntimePacks()
        {
            return CreateEnemySpawnEcologyData.BuildPackDefinitions().Select(d =>
            {
                var pack = ScriptableObject.CreateInstance<EnemySpawnPackSO>();
                pack.PackId = d.PackId;
                pack.CaveLevelMin = d.MinLevel;
                pack.CaveLevelMax = d.MaxLevel;
                pack.BiomeTags = new[] { d.BiomeTag };
                pack.EnvironmentTags = d.EnvironmentTags;
                pack.RequiredFactionIds = d.RequiredFactionIds;
                pack.Entries = d.Entries.Select(e => new EnemySpawnPackEntry
                {
                    EnemyId = e.EnemyId,
                    MinCount = e.MinCount,
                    MaxCount = e.MaxCount,
                    Weight = e.Weight,
                    IsRequired = e.IsRequired,
                    RequiresUnlockedFactionLock = e.RequiresUnlockedFactionLock
                }).ToArray();
                pack.Weight = d.Weight;
                pack.MinimumRoomSize = d.MinimumRoomSize;
                pack.MaxTotalEnemies = d.MaxTotalEnemies;
                pack.IsEnabled = d.IsEnabled;
                return pack;
            }).ToList();
        }

        private static List<EnemyFactionLockSO> CreateRuntimeLocks()
        {
            return CreateEnemySpawnEcologyData.BuildLockDefinitions().Select(d =>
            {
                var factionLock = ScriptableObject.CreateInstance<EnemyFactionLockSO>();
                factionLock.FactionLockId = d.LockId;
                factionLock.IsUnlockedByDefault = d.IsUnlockedByDefault;
                factionLock.RequiredBossGateId = d.RequiredBossGateId;
                factionLock.RequiredStoryFlagId = d.RequiredStoryFlagId;
                factionLock.RequiredCaveLevelMin = d.RequiredCaveLevelMin;
                factionLock.UnlocksFactionIds = d.UnlocksFactionIds;
                factionLock.UnlocksPackIds = d.UnlocksPackIds;
                return factionLock;
            }).ToList();
        }
    }
}
