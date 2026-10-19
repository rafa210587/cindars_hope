using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Data;
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
            return CreatePlan(generatedLevel, runManager, profiles, packs, factionLocks, null, maxEnemies);
        }

        /// <summary>
        /// fable_78 (SLICE 2) — overload com o balance SO do ecossistema: quando fornecido, o alvo de
        /// inimigos do nível passa a ser o THREAT BUDGET por banda (entre piso e teto, ≤ hard cap) e os
        /// spawn points dentro do SafeEntryRadius da entrada são removidos (entrada segura, 14.4). Sem
        /// balance, o comportamento legado (curva 16-32, cap 44) é preservado byte-a-byte.
        /// </summary>
        public CaveEnemySpawnPlan CreatePlan(
            CaveGeneratedLevel generatedLevel,
            CaveRunManager runManager,
            IEnumerable<EnemySpawnProfileSO> profiles,
            IEnumerable<EnemySpawnPackSO> packs,
            IEnumerable<EnemyFactionLockSO> factionLocks,
            CaveEcosystemBalanceSO ecosystemBalance,
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

            // fable_78 (14.4): entrada segura — remove spawn points dentro do SafeEntryRadius da entrada.
            if (ecosystemBalance != null && ecosystemBalance.SafeEntryRadius > 0f)
            {
                var beforeSafe = spawnPoints.Count;
                spawnPoints = ApplySafeEntryRadius(spawnPoints, generatedLevel.Entrance, ecosystemBalance.SafeEntryRadius);
                if (spawnPoints.Count < beforeSafe)
                {
                    plan.Warnings.Add($"SafeEntryRadius removed {beforeSafe - spawnPoints.Count} spawn point(s) near the entrance.");
                }
            }

            if (spawnPoints.Count == 0)
            {
                plan.Warnings.Add($"No valid enemy spawn points for level {generatedLevel.CaveLevel}.");
                Debug.LogWarning($"CaveEnemySpawnPlanner: {plan.Warnings[0]}");
                return plan;
            }

            int targetEnemyCount;
            if (ecosystemBalance != null)
            {
                // fable_78 (14.4): alvo = threat budget por banda (entre piso e teto, ≤ hard cap).
                var band = CaveBandScaling.BandForLevel(generatedLevel.CaveLevel);
                targetEnemyCount = ResolveThreatBudget(band, generatedLevel.CaveLevel, ecosystemBalance);
            }
            else
            {
                targetEnemyCount = ResolveTargetEnemyCount(levelSeed, maxEnemies, generatedLevel.CaveLevel);
            }

            // fable_37 (CA spawn): ponto ÚNICO nomeado do modificador de densidade por evento de mundo
            // (pico de Cinza +30% undead, infestação +20%, dia nublado calmo −15%). Lê WorldEventHooks
            // (resolução do dia); sem evento ativo => multiplicador neutro 1.0. ResolveTargetEnemyCount
            // permanece PURO (testes de densidade inalterados) — o ajuste de evento entra só aqui.
            var eventMultiplier = World.Events.WorldEventHooks.GetCaveSpawnMultiplier();
            if (eventMultiplier != 1f)
            {
                targetEnemyCount = Math.Max(1, (int)Math.Round(targetEnemyCount * eventMultiplier));
            }

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

                // fable_24: deterministic named-elite decision per slot. Independent of the
                // resolver's own elite flag (selection.IsElite) — this is the seeded 8%-from-level-6
                // overlay required by spec CA-3. Same CaveRunSeed/level/slot/enemy → same affix on
                // every revisit (ADR-0005 / cave-stable-run). No GUID/timestamp/unseeded Random.
                bool isNamedElite = Enemy.EliteAffixRules.TryResolveElite(
                    worldSeed,
                    runSeed,
                    generatedLevel.CaveLevel,
                    i,
                    selection.EnemyId ?? string.Empty,
                    out var eliteAffix);

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
                    // Keep the resolver's intrinsic elite flag, OR-ed with the seeded named-elite roll.
                    IsElite = selection.IsElite || isNamedElite,
                    SizeClass = selection.SizeClass ?? string.Empty,
                    FactionId = ResolveFactionId(selection, profilesBySpawnProfile, profilesByEnemy),
                    EliteAffix = eliteAffix
                    // EliteDisplayName is resolved at materialization, where the EnemyDataSO
                    // (and its DisplayName) is available — the planner only knows the EnemyId.
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

        // ===================================================================================
        // fable_78 (SLICE 2) — métodos PUROS e DETERMINÍSTICOS: threat budget (14.4), gating
        // aquático (14.3), distribuição por sala (14.4) e entrada segura (14.4). Tudo lê do
        // CaveEcosystemBalanceSO (rule no-magic-balance-values) e usa StableHash (FNV-1a), sem
        // UnityEngine.Random. As entranhas data-driven do CreatePlan permanecem intactas.
        // ===================================================================================

        /// <summary>
        /// fable_78 (14.4) — orçamento de ameaça DETERMINÍSTICO do nível, ENTRE o piso e o teto da
        /// banda (ThreatBudgetMinByBand / ThreatBudgetMaxByBand do balance SO) e nunca acima do
        /// EnemyDensityHardCap. O budget cresce com a profundidade dentro da banda (interpolação
        /// determinística entre piso e teto pela posição relativa do nível na banda), garantindo
        /// desafio mínimo (piso) sem inflar só a contagem (teto). <paramref name="band"/> é 1..7.
        /// </summary>
        public static int ResolveThreatBudget(int band, int caveLevel, CaveEcosystemBalanceSO balance)
        {
            if (balance == null)
            {
                return MinEnemiesPerLevel;
            }

            var bandIndex = Mathf.Clamp(band - 1, 0, CaveEcosystemBalanceSO.BandCount - 1);
            var floor = balance.GetThreatBudgetMin(bandIndex);
            var ceiling = balance.GetThreatBudgetMax(bandIndex);
            if (ceiling < floor)
            {
                ceiling = floor;
            }

            // Posição relativa do nível dentro da banda (0..1) → interpola piso→teto, monotônico.
            var bandMin = CaveBandScaling.BandMinLevel(band);
            var bandMax = band >= 7 ? bandMin + 15 : CaveBandScaling.BandMinLevel(band + 1) - 1;
            var span = Math.Max(1, bandMax - bandMin);
            var within = Mathf.Clamp01((Math.Max(bandMin, caveLevel) - bandMin) / (float)span);

            var budget = Mathf.RoundToInt(Mathf.Lerp(floor, ceiling, within));
            budget = Mathf.Clamp(budget, floor, ceiling);
            return Math.Min(budget, balance.EnemyDensityHardCap);
        }

        /// <summary>
        /// fable_78 (14.3) — gating aquático: criaturas com <c>IsAquatic=true</c> só são elegíveis
        /// quando o nível tem água. Sem água, são removidas dos candidatos. Pura e determinística
        /// (preserva a ordem de entrada). O caller deriva <paramref name="hasWater"/> de
        /// <see cref="CaveEnvironmentElementProfileSO.HasWater"/> da banda (ou do plano de elementos).
        /// </summary>
        public static List<AquaticCandidate> FilterAquaticEligibility(
            IReadOnlyList<AquaticCandidate> candidates, bool hasWater)
        {
            var result = new List<AquaticCandidate>();
            if (candidates == null)
            {
                return result;
            }

            foreach (var candidate in candidates)
            {
                if (candidate.IsAquatic && !hasWater)
                {
                    continue; // aquático sem lago → não entra
                }

                result.Add(candidate);
            }

            return result;
        }

        /// <summary>
        /// fable_78 (14.4) — distribui um total de inimigos POR SALA (não pela área bruta), de forma
        /// determinística e proporcional ao tamanho da sala, para que mapas grandes (90×90) não fiquem
        /// esparsos. Retorna a contagem por índice de sala; a soma == <paramref name="totalEnemies"/>
        /// (salvo quando não há salas, caso em que retorna lista vazia). Pura, sem Random.
        /// </summary>
        public static List<int> DistributeEnemiesPerRoom(
            IReadOnlyList<CaveRoom> rooms, int totalEnemies)
        {
            var counts = new List<int>();
            if (rooms == null || rooms.Count == 0 || totalEnemies <= 0)
            {
                return counts;
            }

            var areas = new int[rooms.Count];
            long totalArea = 0;
            for (var i = 0; i < rooms.Count; i++)
            {
                areas[i] = Math.Max(1, rooms[i].Width * rooms[i].Height);
                totalArea += areas[i];
            }

            var assigned = 0;
            for (var i = 0; i < rooms.Count; i++)
            {
                counts.Add((int)((long)totalEnemies * areas[i] / totalArea));
                assigned += counts[counts.Count - 1];
            }

            // Distribui o resto determinísticamente para as maiores salas primeiro (estável por índice).
            var remainder = totalEnemies - assigned;
            var order = Enumerable.Range(0, rooms.Count)
                .OrderByDescending(i => areas[i])
                .ThenBy(i => i)
                .ToList();
            for (var r = 0; r < remainder; r++)
            {
                counts[order[r % order.Count]]++;
            }

            return counts;
        }

        /// <summary>
        /// fable_78 (14.4) — entrada segura: remove spawn points dentro do <c>SafeEntryRadius</c> do
        /// spawn de entrada (anti-envelopamento ao entrar num nível denso). Distância euclidiana em
        /// tiles. Pura e determinística (preserva ordem). O número é tunável no balance SO.
        /// </summary>
        public static List<Vector2Int> ApplySafeEntryRadius(
            IReadOnlyList<Vector2Int> spawnPoints, Vector2Int entranceGrid, float safeEntryRadius)
        {
            var result = new List<Vector2Int>();
            if (spawnPoints == null)
            {
                return result;
            }

            var radiusSqr = safeEntryRadius * safeEntryRadius;
            foreach (var point in spawnPoints)
            {
                var dx = point.x - entranceGrid.x;
                var dy = point.y - entranceGrid.y;
                if (dx * dx + dy * dy < radiusSqr)
                {
                    continue; // dentro do raio seguro → sem inimigo
                }

                result.Add(point);
            }

            return result;
        }

        /// <summary>Candidato de spawn com a flag aquática do bestiário (para o gating 14.3).</summary>
        public readonly struct AquaticCandidate
        {
            public readonly string EnemyId;
            public readonly bool IsAquatic;

            public AquaticCandidate(string enemyId, bool isAquatic)
            {
                EnemyId = enemyId ?? string.Empty;
                IsAquatic = isAquatic;
            }
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
