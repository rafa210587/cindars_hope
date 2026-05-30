using System.Collections.Generic;
using System.Linq;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// SPEC 14A-FIX5/FIX6 — Validates enemy spawn coverage across all cave level bands.
    /// Run via: CindarsHope > Validate > Enemy Cave Spawn Coverage
    /// Fails if profiles total < 60, level 30/45/60/75/90 have no spawnable profiles, or stale RequiredBossGateProgress detected.
    /// </summary>
    public static class ValidateEnemyCaveSpawnCoverage
    {
        // Critical levels that previously broke (level 30 / 45 / 60 / 75 / 90 boss gate entries)
        private static readonly int[] CriticalLevels = { 30, 45, 60, 75, 90 };
        private static readonly int[] SampleLevels = { 1, 2, 10, 15, 25, 30, 40, 45, 55, 60, 70, 75, 85, 90, 99 };

        private const string ProfilesFolder = "Assets/_Game/Data/EnemySpawn/Profiles";
        private const string PacksFolder    = "Assets/_Game/Data/EnemySpawn/Packs";
        private const string LocksFolder    = "Assets/_Game/Data/EnemySpawn/FactionLocks";

        [MenuItem("CindarsHope/Validate/Enemy Cave Spawn Coverage")]
        public static void Validate()
        {
            var profiles = LoadAll<EnemySpawnProfileSO>(ProfilesFolder);
            var packs    = LoadAll<EnemySpawnPackSO>(PacksFolder);
            var locks    = LoadAll<EnemyFactionLockSO>(LocksFolder);

            Debug.Log($"[CoverageValidator] Loaded {profiles.Length} profiles, {packs.Length} packs, {locks.Length} locks.");

            int failures = 0;

            // FIX6 invariant: roster must exceed legacy 40
            if (profiles.Length < 60)
            {
                Debug.LogError($"[CoverageValidator] FAIL: ProfilesTotal={profiles.Length}. Expected >= 60 (bands 6-7 should add 20). Run Regenerate All Enemy Data.");
                failures++;
            }

            // FIX6 invariant: no profile may have RequiredBossGateProgress (FIX5 removed this field)
            var staleProfiles = profiles.Where(p => p != null && !string.IsNullOrWhiteSpace(p.RequiredBossGateProgress)).ToArray();
            if (staleProfiles.Length > 0)
            {
                var sample = staleProfiles.Take(5).Select(p => $"{p.EnemyId}->{p.RequiredBossGateProgress}");
                Debug.LogError($"[CoverageValidator] FAIL: {staleProfiles.Length} profiles still have RequiredBossGateProgress set. Sample: {string.Join(", ", sample)}. Re-run Regenerate All Enemy Data to flush stale field.");
                failures++;
            }

            // FIX6 invariant: pack count >= 25 (19 original + 6 bands 6-7)
            if (packs.Length < 25)
            {
                Debug.LogError($"[CoverageValidator] FAIL: PacksTotal={packs.Length}. Expected >= 25 (19 original + 6 new bands 6-7).");
                failures++;
            }

            foreach (var level in SampleLevels)
            {
                var result = CheckLevel(level, profiles, packs, locks);
                bool critical = CriticalLevels.Contains(level);

                if (result.HasErrors)
                {
                    if (critical)
                    {
                        Debug.LogError($"[CoverageValidator] CRITICAL Level {level}: {result.Summary}");
                        failures++;
                    }
                    else
                    {
                        Debug.LogWarning($"[CoverageValidator] Level {level}: {result.Summary}");
                    }
                }
                else
                {
                    Debug.Log($"[CoverageValidator] Level {level}: OK — {result.Summary}");
                }
            }

            if (failures == 0)
                Debug.Log("[CoverageValidator] ALL CHECKS PASSED. Roster covers all sampled levels including 30/45/60/75/90.");
            else
                Debug.LogError($"[CoverageValidator] {failures} critical check(s) FAILED. See errors above.");
        }

        private static LevelCoverageResult CheckLevel(
            int level,
            EnemySpawnProfileSO[] profiles,
            EnemySpawnPackSO[] packs,
            EnemyFactionLockSO[] locks)
        {
            var unlockedLockIds = BuildUnlockedLockIds(level, locks);
            var biomeTag        = LevelToBiomeTag(level);

            var eligibleProfiles = profiles.Where(p =>
                p != null &&
                p.IsEnabled &&
                p.CaveLevelMin <= level &&
                p.CaveLevelMax >= level &&
                (string.IsNullOrWhiteSpace(p.FactionLockId) || unlockedLockIds.Contains(p.FactionLockId)) &&
                (string.IsNullOrWhiteSpace(p.RequiredBossGateProgress)) &&
                (p.BiomeTags == null || p.BiomeTags.Length == 0 || p.BiomeTags.Contains(biomeTag))
            ).ToList();

            var distinctEnemyTypes = eligibleProfiles.Select(p => p.EnemyId).Distinct().Count();
            var errors = new List<string>();

            if (eligibleProfiles.Count == 0)
                errors.Add("no eligible profiles");
            else if (distinctEnemyTypes < 4)
                errors.Add($"only {distinctEnemyTypes} distinct enemy types (need >=4)");

            var eligiblePacks = packs.Where(p =>
                p != null &&
                p.IsEnabled &&
                p.CaveLevelMin <= level &&
                p.CaveLevelMax >= level &&
                (p.BiomeTags == null || p.BiomeTags.Length == 0 || p.BiomeTags.Contains(biomeTag))
            ).ToList();

            if (eligiblePacks.Count == 0)
                errors.Add("no eligible packs");

            var summary = errors.Count == 0
                ? $"biome={biomeTag}, profiles={eligibleProfiles.Count}({distinctEnemyTypes} types), packs={eligiblePacks.Count}, unlocked locks={unlockedLockIds.Count}"
                : string.Join("; ", errors) + $" | biome={biomeTag}, profiles={eligibleProfiles.Count}, packs={eligiblePacks.Count}, unlocked locks={unlockedLockIds.Count}";

            return new LevelCoverageResult { HasErrors = errors.Count > 0, Summary = summary };
        }

        private static HashSet<string> BuildUnlockedLockIds(int level, EnemyFactionLockSO[] locks)
        {
            var result = new HashSet<string>();
            foreach (var l in locks)
            {
                if (l == null) continue;
                if (l.IsUnlockedByDefault) { result.Add(l.FactionLockId); continue; }
                if (l.RequiredCaveLevelMin > 0 && level >= l.RequiredCaveLevelMin)
                    result.Add(l.FactionLockId);
            }
            return result;
        }

        private static string LevelToBiomeTag(int level)
        {
            return level switch
            {
                <= 10 => "stone",
                <= 25 => "fungal",
                <= 40 => "ice",
                <= 55 => "fire",
                <= 70 => "ruins",
                <= 85 => "deep",
                _ => "void"
            };
        }

        private static T[] LoadAll<T>(string folder) where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(a => a != null)
                .ToArray();
        }

        private struct LevelCoverageResult
        {
            public bool HasErrors;
            public string Summary;
        }
    }
}
