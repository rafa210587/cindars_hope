using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Generation;
using CindarsHope.Enemy;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveEnemySpawnPlanner
    {
        private const int DefaultMaxEnemies = 32;
        private const int MinEnemiesPerLevel = 16;
        private const int MaxEnemiesPerLevel = 32;
        // Depth scaling: +1 enemy on min every 12 levels, +1 on max every 8 levels,
        // hard-capped so deep floors stay dense but playable.
        private const int DepthScalingHardCap = 44;
        private const int MinDistanceFromEntrance = 5;
        private const int MinDistanceFromExit = 2;
        private const int MinDistanceBetweenEnemies = 2;

        public CaveEnemySpawnPlan CreatePlan(
            CaveGeneratedLevel generatedLevel,
            CaveRunManager runManager,
            IEnumerable<EnemySpawnProfileSO> profiles,
            IEnumerable<EnemySpawnPackSO> packs,
            IEnumerable<EnemyFactionLockSO> factionLocks,
            int maxEnemies = DefaultMaxEnemies)
        {
            if (generatedLevel == null)
            {
                Debug.LogError("CaveEnemySpawnPlanner: Cannot create spawn plan for null CaveGeneratedLevel.");
                return new CaveEnemySpawnPlan();
            }

            var worldSeed = runManager != null ? runManager.CaveWorldSeed : string.Empty;
            var runSeed = runManager != null ? runManager.CaveRunSeed : string.Empty;
            var seedSource = $"{worldSeed}|{runSeed}|{generatedLevel.CaveLevel}|{generatedLevel.BiomeId}|enemy_spawn_plan";
            var levelSeed = StableHash(seedSource);
            var levelSeedId = $"seed_{Math.Abs(levelSeed):x8}";

            var plan = new CaveEnemySpawnPlan
            {
                CaveLevel = generatedLevel.CaveLevel,
                BiomeId = generatedLevel.BiomeId ?? string.Empty,
                CaveWorldSeed = worldSeed ?? string.Empty,
                CaveRunSeed = runSeed ?? string.Empty,
                LevelSeed = levelSeedId,
                LayoutHash = ResolveLayoutHash(generatedLevel)
            };

            var spawnPoints = ResolveSpawnPoints(generatedLevel);
            if (spawnPoints.Count == 0)
            {
                plan.Warnings.Add($"No valid enemy spawn points for level {generatedLevel.CaveLevel}.");
                Debug.LogWarning($"CaveEnemySpawnPlanner: {plan.Warnings[0]}");
                return plan;
            }

            var targetEnemyCount = ResolveTargetEnemyCount(levelSeed, maxEnemies, generatedLevel.CaveLevel);
            var requestMaxEnemies = Math.Max(1, Math.Min(targetEnemyCount, spawnPoints.Count));

            var resolver = new EnemySpawnResolver(profiles, packs, factionLocks);
            var allSelections = new List<EnemySpawnSelection>();
            var allWarnings = new List<string>();
            int resolvedCount = 0;
            string lastSelectedPackId = string.Empty;

            for (int pass = 0; pass < 4 && resolvedCount < requestMaxEnemies; pass++)
            {
                var passRequest = new EnemySpawnRequest
                {
                    CaveLevel = generatedLevel.CaveLevel,
                    BiomeTags = BuildBiomeTags(generatedLevel),
                    EnvironmentTags = BuildEnvironmentTags(generatedLevel),
                    RoomSizeClass = ResolveRoomSizeClass(generatedLevel),
                    RoomTags = BuildRoomTags(generatedLevel),
                    BossGateProgressIds = BuildBossGateProgressIds(runManager),
                    UnlockedFactionLockIds = BuildUnlockedFactionLockIds(runManager, factionLocks, generatedLevel.CaveLevel),
                    Seed = levelSeed + pass * 13337,
                    MaxEnemies = requestMaxEnemies - resolvedCount,
                    AllowElite = true,
                    DebugReason = $"SPEC14A cave materialization pass {pass}"
                };

                var result = resolver.Resolve(passRequest);
                if (result == null || !result.IsValid || result.SelectedEnemies.Count == 0)
                {
                    if (result?.Warnings != null) allWarnings.AddRange(result.Warnings);
                    break;
                }

                allWarnings.AddRange(result.Warnings ?? new List<string>());
                if (!string.IsNullOrEmpty(result.SelectedPackId))
                    lastSelectedPackId = result.SelectedPackId;
                foreach (var sel in result.SelectedEnemies)
                {
                    allSelections.Add(sel);
                    resolvedCount += sel.Count;
                }
            }

            if (allSelections.Count == 0)
            {
                var warning = allWarnings.Count > 0 ? string.Join("; ", allWarnings) : "no valid pack or profile";
                plan.Warnings.Add($"No enemies resolved for level {generatedLevel.CaveLevel}. {warning}");
                Debug.LogWarning($"CaveEnemySpawnPlanner: No enemies resolved for level {generatedLevel.CaveLevel}. {warning}");
                return plan;
            }

            plan.Warnings.AddRange(allWarnings);
            if (resolvedCount < targetEnemyCount)
                plan.Warnings.Add($"Resolved {resolvedCount} enemies, below target {targetEnemyCount}. Data packs/profiles limited the count.");

            var expanded = ExpandSelections(allSelections, requestMaxEnemies);
            var orderedPoints = OrderSpawnPoints(spawnPoints, levelSeed);
            var selectedPoints = SelectSpawnPointsWithSpacing(orderedPoints, expanded.Count, plan.Warnings);
            var profilesBySpawnProfile = (profiles ?? Array.Empty<EnemySpawnProfileSO>())
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.SpawnProfileId))
                .GroupBy(p => p.SpawnProfileId)
                .ToDictionary(g => g.Key, g => g.First());
            var profilesByEnemy = (profiles ?? Array.Empty<EnemySpawnProfileSO>())
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.EnemyId))
                .GroupBy(p => p.EnemyId)
                .ToDictionary(g => g.Key, g => g.First());

            for (int i = 0; i < expanded.Count && i < selectedPoints.Count; i++)
            {
                var selection = expanded[i];
                var point = selectedPoints[i];
                var roomId = ResolveRoomId(generatedLevel, point, i);
                var instanceId = BuildEnemyInstanceId(
                    generatedLevel.CaveLevel,
                    roomId,
                    i,
                    selection.EnemyId,
                    worldSeed,
                    runSeed);

                plan.Entries.Add(new CaveEnemySpawnPlanEntry
                {
                    EnemyInstanceId = instanceId,
                    EnemyId = selection.EnemyId ?? string.Empty,
                    SpawnProfileId = selection.SpawnProfileId ?? string.Empty,
                    PackId = selection.PackId ?? lastSelectedPackId,
                    GridPosition = point,
                    WorldPosition = GridToWorld(point, generatedLevel),
                    RoomId = roomId,
                    SpawnIndex = i,
                    IsElite = selection.IsElite,
                    SizeClass = selection.SizeClass ?? string.Empty,
                    FactionId = ResolveFactionId(selection, profilesBySpawnProfile, profilesByEnemy)
                });
            }

            if (plan.Entries.Count < expanded.Count)
            {
                plan.Warnings.Add($"Only {plan.Entries.Count} spawn positions were safe for {expanded.Count} resolved enemies.");
            }

            return plan;
        }

        public static string BuildEnemyInstanceId(
            int caveLevel,
            string roomId,
            int spawnIndex,
            string enemyId,
            string worldSeed,
            string runSeed)
        {
            var hash = StableHash($"{worldSeed}|{runSeed}|{caveLevel}|{roomId}|{spawnIndex}|{enemyId}");
            return $"enemy_{caveLevel}_{Sanitize(roomId)}_{spawnIndex}_{enemyId}_{Math.Abs(hash):x8}";
        }

        public static int StableHash(string value)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                foreach (var c in value ?? string.Empty)
                {
                    hash ^= c;
                    hash *= fnvPrime;
                }

                return hash == int.MinValue ? 0 : hash;
            }
        }

        private static List<Vector2Int> ResolveSpawnPoints(CaveGeneratedLevel generatedLevel)
        {
            var explicitPoints = generatedLevel.EnemySpawnPoints
                .Select(p => p.Position)
                .Where(p => IsValidEnemySpawnTile(p, generatedLevel))
                .Distinct()
                .ToList();

            // Supplement with walkable tiles when explicit spawn points are fewer than the minimum target
            if (explicitPoints.Count >= MinEnemiesPerLevel)
                return explicitPoints;

            var all = new HashSet<Vector2Int>(explicitPoints);
            foreach (var tile in generatedLevel.WalkableTiles)
            {
                if (IsValidEnemySpawnTile(tile, generatedLevel))
                    all.Add(tile);
            }
            return all.ToList();
        }

        private static bool IsValidEnemySpawnTile(Vector2Int point, CaveGeneratedLevel generatedLevel)
        {
            if (!generatedLevel.WalkableTiles.Contains(point))
            {
                return false;
            }

            if (point == generatedLevel.Entrance || point == generatedLevel.Exit)
            {
                return false;
            }

            if (ManhattanDistance(point, generatedLevel.Entrance) < MinDistanceFromEntrance)
            {
                return false;
            }

            if (ManhattanDistance(point, generatedLevel.Exit) < MinDistanceFromExit)
            {
                return false;
            }

            return !generatedLevel.WallTiles.Contains(point);
        }

        private static List<Vector2Int> OrderSpawnPoints(List<Vector2Int> points, int seed)
        {
            return points
                .OrderBy(p => StableHash($"{seed}|{p.x}|{p.y}"))
                .ThenBy(p => p.x)
                .ThenBy(p => p.y)
                .ToList();
        }

        private static List<Vector2Int> SelectSpawnPointsWithSpacing(
            List<Vector2Int> orderedPoints,
            int requestedCount,
            List<string> warnings)
        {
            var selected = new List<Vector2Int>();
            foreach (var point in orderedPoints)
            {
                if (selected.Count >= requestedCount)
                {
                    break;
                }

                var isFarEnough = selected.All(existing => ManhattanDistance(existing, point) >= MinDistanceBetweenEnemies);
                if (isFarEnough)
                {
                    selected.Add(point);
                }
            }

            if (selected.Count < requestedCount)
            {
                warnings?.Add($"Safe spawn spacing allowed {selected.Count}/{requestedCount} enemy positions.");
            }

            return selected;
        }

        private static List<EnemySpawnSelection> ExpandSelections(List<EnemySpawnSelection> selections, int maxEnemies)
        {
            var result = new List<EnemySpawnSelection>();
            foreach (var selection in selections)
            {
                var count = Math.Max(0, selection.Count);
                for (int i = 0; i < count && result.Count < maxEnemies; i++)
                {
                    result.Add(selection);
                }
            }

            return result;
        }

        // Deterministic per-level enemy count that grows with cave depth. Scene-serialized
        // _maxEnemiesPerLevel values from older scenes are treated as a base and still get
        // the depth bonus, so density increases without scene regeneration.
        internal static int ResolveTargetEnemyCount(int levelSeed, int configuredMaxEnemies, int caveLevel)
        {
            var depth = Math.Max(0, caveLevel);
            var minBound = Math.Min(DepthScalingHardCap, MinEnemiesPerLevel + depth / 12);
            var maxBase = Math.Max(MaxEnemiesPerLevel, configuredMaxEnemies);
            var maxBound = Math.Min(DepthScalingHardCap, maxBase + depth / 8);
            var upperBound = Math.Max(minBound, maxBound);
            var range = Math.Max(1, upperBound - minBound + 1);
            return minBound + Math.Abs(levelSeed % range);
        }

        private static string ResolveFactionId(
            EnemySpawnSelection selection,
            IReadOnlyDictionary<string, EnemySpawnProfileSO> profilesBySpawnProfile,
            IReadOnlyDictionary<string, EnemySpawnProfileSO> profilesByEnemy)
        {
            if (selection == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(selection.SpawnProfileId)
                && profilesBySpawnProfile != null
                && profilesBySpawnProfile.TryGetValue(selection.SpawnProfileId, out var profileBySpawn)
                && profileBySpawn != null)
            {
                return profileBySpawn.FactionId ?? string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(selection.EnemyId)
                && profilesByEnemy != null
                && profilesByEnemy.TryGetValue(selection.EnemyId, out var profileByEnemy)
                && profileByEnemy != null)
            {
                return profileByEnemy.FactionId ?? string.Empty;
            }

            return string.Empty;
        }

        private static Vector3 GridToWorld(Vector2Int gridPosition, CaveGeneratedLevel level)
        {
            var offsetX = level.Width * 0.5f;
            var offsetY = level.Height * 0.5f;
            return new Vector3(gridPosition.x - offsetX, gridPosition.y - offsetY, 0f);
        }

        private static List<string> BuildBiomeTags(CaveGeneratedLevel level)
        {
            var levelTag = level.CaveLevel switch
            {
                <= 10 => "stone",
                <= 25 => "fungal",
                <= 40 => "ice",
                <= 55 => "fire",
                <= 70 => "ruins",
                <= 85 => "deep",
                _ => "void"
            };

            var normalizedTag = NormalizeBiomeTag(level.BiomeId);
            if (string.IsNullOrWhiteSpace(normalizedTag) || normalizedTag == levelTag)
                return new List<string> { levelTag };

            // Include both the level-range biome and the cave-specific biome so profiles/packs
            // match regardless of which tag they use (e.g. level 15 cave_earth → ["fungal","stone"])
            return new List<string> { levelTag, normalizedTag };
        }

        private static string NormalizeBiomeTag(string biomeId)
        {
            if (string.IsNullOrWhiteSpace(biomeId))
            {
                return string.Empty;
            }

            var value = biomeId.ToLowerInvariant();
            if (value.Contains("fungal") || value.Contains("forest")) return "fungal";
            if (value.Contains("ice") || value.Contains("frost")) return "ice";
            if (value.Contains("fire") || value.Contains("lava")) return "fire";
            if (value.Contains("ruin")) return "ruins";
            if (value.Contains("stone") || value.Contains("earth") || value.Contains("cave")) return "stone";
            return value.Replace("biome_", string.Empty);
        }

        private static List<string> BuildEnvironmentTags(CaveGeneratedLevel level)
        {
            return BuildBiomeTags(level);
        }

        private static EnemyRoomSizeClass ResolveRoomSizeClass(CaveGeneratedLevel level)
        {
            if (level.Rooms == null || level.Rooms.Count == 0)
            {
                return EnemyRoomSizeClass.Small;
            }

            var largest = level.Rooms.Max(r => Math.Min(r.Width, r.Height));
            if (largest >= 12) return EnemyRoomSizeClass.Large;
            if (largest >= 8) return EnemyRoomSizeClass.Medium;
            return EnemyRoomSizeClass.Small;
        }

        private static List<string> BuildRoomTags(CaveGeneratedLevel level)
        {
            var tags = new List<string> { "cave" };
            var size = ResolveRoomSizeClass(level);
            if (size >= EnemyRoomSizeClass.Large)
            {
                tags.Add("large");
            }

            return tags;
        }

        private static List<string> BuildBossGateProgressIds(CaveRunManager runManager)
        {
            var result = new List<string>();
            if (runManager == null)
            {
                return result;
            }

            foreach (var kvp in runManager.State.BossDefeatStates)
            {
                if (kvp.Value != null && kvp.Value.IsDefeated)
                {
                    result.Add(kvp.Key);
                }
            }

            return result;
        }

        private static List<string> BuildUnlockedFactionLockIds(
            CaveRunManager runManager,
            IEnumerable<EnemyFactionLockSO> factionLocks,
            int caveLevel)
        {
            var result = new List<string>();
            foreach (var factionLock in factionLocks ?? Array.Empty<EnemyFactionLockSO>())
            {
                if (factionLock == null)
                {
                    continue;
                }

                if (factionLock.IsUnlockedByDefault)
                {
                    result.Add(factionLock.FactionLockId);
                    continue;
                }

                if (factionLock.RequiredCaveLevelMin > 0 && caveLevel >= factionLock.RequiredCaveLevelMin)
                {
                    result.Add(factionLock.FactionLockId);
                    continue;
                }

                if (runManager != null
                    && !string.IsNullOrWhiteSpace(factionLock.RequiredBossGateId)
                    && runManager.IsBossDefeated(factionLock.RequiredBossGateId))
                {
                    result.Add(factionLock.FactionLockId);
                }
            }

            return result;
        }

        private static string ResolveRoomId(CaveGeneratedLevel level, Vector2Int point, int index)
        {
            if (level.Rooms != null)
            {
                for (int i = 0; i < level.Rooms.Count; i++)
                {
                    var room = level.Rooms[i];
                    if (point.x >= room.X && point.x < room.X + room.Width
                        && point.y >= room.Y && point.y < room.Y + room.Height)
                    {
                        return $"room_{i}";
                    }
                }
            }

            return $"synthetic_{point.x}_{point.y}_{index}";
        }

        private static string ResolveLayoutHash(CaveGeneratedLevel generatedLevel)
        {
            if (generatedLevel == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(generatedLevel.LayoutHash))
            {
                generatedLevel.ComputeLayoutHash();
            }

            return generatedLevel.LayoutHash ?? string.Empty;
        }

        private static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        private static string Sanitize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "room"
                : new string(value.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        }
    }
}
