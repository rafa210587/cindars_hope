using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using UnityEngine;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 — planejador PURO e DETERMINÍSTICO de elementos ambientais por bioma (seções 14.2 / 16.2).
    /// ESTENDIDO por spec_cave_decor_composition_runtime (CV03) com colocação de DECOR por CONTEXTO +
    /// clusters (seção 20, Fase 2) — MineableNode/WaterTile/EnsureGuaranteedPresence permanecem no
    /// caminho original do fable_78, só marcando o Context da célula no Placement.
    ///
    /// Pure C# (sem MonoBehaviour, sem cena) → 100% testável em EditMode. Consome um
    /// <see cref="CaveGeneratedLevel"/> já gerado/restaurado + o perfil do bioma + os seeds da run.
    /// Tudo derivado de <see cref="CaveLayoutStableHash"/> (FNV-1a) — revisitar o mesmo
    /// (worldSeed, runSeed, caveLevel) reproduz EXATAMENTE o mesmo plano. Sem GUID/timestamp/Random.
    ///
    /// Invariantes:
    /// - MineableNode/WaterTile/decor garantido só caem em walkable tiles (comportamento original
    ///   intacto); decor de teto (CeilingHang) é a ÚNICA exceção deliberada — fica em WallTiles,
    ///   porque representa uma estalactite pendendo da parede, não um objeto no chão (critério 14.1);
    /// - decor bloqueante e nós mineráveis NUNCA caem no caminho entrance↔exit (BFS 4-conn, mesmo
    ///   espírito do <see cref="CaveHazardPlanner"/>) → sem softlock;
    /// - pedra e minério estão presentes em TODAS as bandas (garantia de presença, EnsureGuaranteedPresence intacto);
    /// - densidade conforme EnvironmentElementDensityByBand do balance SO (ou default por banda);
    /// - decor de chão (FloorCluster) é colocado em CLUSTERS determinísticos (semente + 1-3 vizinhos),
    ///   não singletons espalhados — reduz a densidade percebida ("confete") sem tocar a densidade
    ///   base (que continua vindo do balance SO / default por banda).
    /// </summary>
    public static class CaveEnvironmentElementPlanner
    {
        private const int MinDistanceFromEntrance = 3;
        private const int MinDistanceFromExit = 2;

        // spec_cave_decor_composition_runtime (CV03): tamanho de um cluster de FloorCluster = semente +
        // [MinClusterExtra..MaxClusterExtra] vizinhos, escolhido deterministicamente por hash da semente
        // (nunca Random/GetHashCode). Range replica o critério 14.2 (cluster de 2-4 elementos: 1 semente
        // + 1 a 3 extras).
        private const int MinClusterExtra = 1;
        private const int MaxClusterExtra = 3;

        // Densidade-default por banda (0..6) — espelha EnvironmentElementDensityByBand do balance SO.
        // Usada apenas quando o caller não fornece um CaveEcosystemBalanceSO.
        private static readonly float[] DefaultDensityByBand = { 0.06f, 0.08f, 0.08f, 0.09f, 0.09f, 0.10f, 0.11f };

        public static CaveEnvironmentElementPlan Build(
            CaveGeneratedLevel level,
            CaveEnvironmentElementProfileSO profile,
            string worldSeed,
            string runSeed,
            int caveLevel,
            Vector2Int playerSpawnGrid)
        {
            return Build(level, profile, null, worldSeed, runSeed, caveLevel, playerSpawnGrid);
        }

        /// <param name="balance">Quando não-nulo, a densidade tunável do SO substitui o default por banda.</param>
        public static CaveEnvironmentElementPlan Build(
            CaveGeneratedLevel level,
            CaveEnvironmentElementProfileSO profile,
            CaveEcosystemBalanceSO balance,
            string worldSeed,
            string runSeed,
            int caveLevel,
            Vector2Int playerSpawnGrid)
        {
            if (level == null || level.WalkableTiles == null || level.WalkableTiles.Count == 0 || profile == null)
            {
                return CaveEnvironmentElementPlan.Empty(caveLevel, profile != null ? profile.BiomeId : string.Empty);
            }

            var entries = profile.Entries;
            if (entries == null || entries.Count == 0)
            {
                return new CaveEnvironmentElementPlan(
                    caveLevel, profile.BiomeId, profile.HasWater, new List<CaveEnvironmentElementPlacement>());
            }

            var seed = CaveLayoutStableHash.Compute($"{worldSeed}|{runSeed}|{caveLevel}|env_elements");
            var pathTiles = CaveHazardPlanner.ComputeEntranceExitPathWithBuffer(level);

            // Candidatos: walkable, fora do caminho obrigatório, longe de entrance/exit/player spawn.
            // Ordem determinística por hash semeado por tile (mesmo espírito do hazard planner).
            // Usado, sem mudança, pelo caminho legado de MineableNode/WaterTile e por EnsureGuaranteedPresence.
            var candidates = level.WalkableTiles
                .Where(tile => IsEligible(tile, level, playerSpawnGrid, pathTiles))
                .OrderBy(tile => CaveLayoutStableHash.Compute($"{seed}|tile|{tile.x}|{tile.y}"))
                .ThenBy(tile => tile.x)
                .ThenBy(tile => tile.y)
                .ToList();

            var density = ResolveDensity(profile, balance);
            var targetCount = Mathf.Clamp(Mathf.RoundToInt(candidates.Count * density), 0, candidates.Count);

            var placements = new List<CaveEnvironmentElementPlacement>(targetCount);
            var used = new HashSet<Vector2Int>();

            // Garantia de presença: pedra (decor) + minério (mineável) em TODAS as bandas. Intacto.
            EnsureGuaranteedPresence(level, profile, seed, candidates, used, placements);

            var decorEntries = entries.Where(IsDecorKind).ToList();
            var nonDecorEntries = entries.Where(e => !IsDecorKind(e)).ToList();

            // spec_cave_decor_composition_runtime (CV03): decor visual (Non/Blocking) é colocado POR
            // CONTEXTO — nunca mais "1 elemento por célula andável aleatória" — enquanto MineableNode e
            // WaterTile seguem o loop legado sobre `candidates` (walkable, fora do path, longe da
            // entrada/saída/spawn), inalterado.
            if (decorEntries.Count > 0)
            {
                var floorClusterDensity = density * ResolveFloorClusterDensityMultiplier(balance);
                PlaceDecorByContext(level, decorEntries, seed, pathTiles, playerSpawnGrid, density, floorClusterDensity, used, placements);
            }

            if (nonDecorEntries.Count > 0)
            {
                foreach (var tile in candidates)
                {
                    if (placements.Count >= targetCount)
                    {
                        break;
                    }

                    if (used.Contains(tile))
                    {
                        continue;
                    }

                    var entry = PickEntry(nonDecorEntries, seed, tile);
                    if (!entry.HasValue)
                    {
                        continue;
                    }

                    used.Add(tile);
                    placements.Add(BuildPlacement(level.CaveLevel, tile, entry.Value, CaveDecorPlacementContext.FloorCluster));
                }
            }

            return new CaveEnvironmentElementPlan(caveLevel, profile.BiomeId, profile.HasWater, placements);
        }

        private static float ResolveDensity(CaveEnvironmentElementProfileSO profile, CaveEcosystemBalanceSO balance)
        {
            if (balance != null)
            {
                return balance.GetEnvironmentElementDensity(profile.Band);
            }

            var band = Mathf.Clamp(profile.Band, 0, DefaultDensityByBand.Length - 1);
            return DefaultDensityByBand[band];
        }

        // spec_cave_decor_composition_runtime (CV03): default usado quando o caller não fornece um
        // CaveEcosystemBalanceSO — espelha CaveEcosystemBalanceSO.FloorClusterDensityMultiplier default.
        private const float DefaultFloorClusterDensityMultiplier = 0.5f;

        private static float ResolveFloorClusterDensityMultiplier(CaveEcosystemBalanceSO balance)
        {
            return balance != null ? balance.FloorClusterDensityMultiplier : DefaultFloorClusterDensityMultiplier;
        }

        private static void EnsureGuaranteedPresence(
            CaveGeneratedLevel level,
            CaveEnvironmentElementProfileSO profile,
            int seed,
            List<Vector2Int> candidates,
            HashSet<Vector2Int> used,
            List<CaveEnvironmentElementPlacement> placements)
        {
            if (candidates.Count == 0)
            {
                return;
            }

            // Pedra (decor não-bloqueante) garantida.
            var stoneEntry = new CaveEnvironmentElementProfileSO.ElementEntry
            {
                Kind = CaveEnvironmentElementKind.DecorNonBlocking,
                Weight = 1f,
                MineNodeDataId = string.Empty
            };
            PlaceGuaranteed(level.CaveLevel, level, candidates, used, placements, stoneEntry);

            // Minério (mineável) garantido — usa o primeiro mineNodeDataId declarado no perfil, se houver.
            var oreEntry = new CaveEnvironmentElementProfileSO.ElementEntry
            {
                Kind = CaveEnvironmentElementKind.MineableNode,
                Weight = 1f,
                MineNodeDataId = FindFirstMineNodeId(profile)
            };
            PlaceGuaranteed(level.CaveLevel, level, candidates, used, placements, oreEntry);
        }

        private static void PlaceGuaranteed(
            int caveLevel,
            CaveGeneratedLevel level,
            List<Vector2Int> candidates,
            HashSet<Vector2Int> used,
            List<CaveEnvironmentElementPlacement> placements,
            CaveEnvironmentElementProfileSO.ElementEntry entry)
        {
            foreach (var tile in candidates)
            {
                if (used.Contains(tile))
                {
                    continue;
                }

                used.Add(tile);
                // Classifica a célula real (a garantia pode cair em WallHug tanto quanto em FloorCluster
                // — `candidates` é só "walkable/elegível", não filtrado por contexto). Nunca deveria dar
                // null aqui (célula é sempre walkable), mas o fallback FloorCluster é seguro mesmo assim.
                var context = CaveDecorContextClassifier.Classify(tile, level) ?? CaveDecorPlacementContext.FloorCluster;
                placements.Add(BuildPlacement(caveLevel, tile, entry, context));
                return;
            }
        }

        private static string FindFirstMineNodeId(CaveEnvironmentElementProfileSO profile)
        {
            foreach (var entry in profile.Entries)
            {
                if (entry.Kind == CaveEnvironmentElementKind.MineableNode && !string.IsNullOrWhiteSpace(entry.MineNodeDataId))
                {
                    return entry.MineNodeDataId;
                }
            }

            return string.Empty;
        }

        private static bool IsDecorKind(CaveEnvironmentElementProfileSO.ElementEntry entry)
        {
            return entry.Kind == CaveEnvironmentElementKind.DecorNonBlocking
                || entry.Kind == CaveEnvironmentElementKind.DecorBlocking;
        }

        /// <summary>
        /// spec_cave_decor_composition_runtime (CV03) — orquestra a colocação de decor (Non/Blocking)
        /// por CONTEXTO: CeilingHang e WallHug usam picks determinísticos 1-por-célula (mesmo espírito
        /// do loop legado, restrito à população do contexto); FloorCluster usa sementes + clusters
        /// (2-4 elementos) para reduzir a densidade percebida de singletons no chão aberto. O budget
        /// (targetCount) é o MESMO `density` já resolvido para o perfil/banda — dividido em 3 fatias por
        /// contexto para não fazer o decor de teto/parede competir por peso com o de chão dentro do
        /// mesmo roll (cada população já é fisicamente restrita ao seu conjunto de células elegíveis).
        /// </summary>
        private static void PlaceDecorByContext(
            CaveGeneratedLevel level,
            List<CaveEnvironmentElementProfileSO.ElementEntry> decorEntries,
            int seed,
            HashSet<Vector2Int> pathTiles,
            Vector2Int playerSpawnGrid,
            float density,
            float floorClusterDensity,
            HashSet<Vector2Int> used,
            List<CaveEnvironmentElementPlacement> placements)
        {
            var ceilingCells = FilterFarFromEntranceExit(
                CaveDecorContextClassifier.CollectCells(level, CaveDecorPlacementContext.CeilingHang), level);
            PlaceContextSingletons(
                level, decorEntries, seed, density, used, placements,
                CaveDecorPlacementContext.CeilingHang, ceilingCells);

            var wallHugCells = FilterEligible(
                CaveDecorContextClassifier.CollectCells(level, CaveDecorPlacementContext.WallHug),
                level, playerSpawnGrid, pathTiles);
            PlaceContextSingletons(
                level, decorEntries, seed, density, used, placements,
                CaveDecorPlacementContext.WallHug, wallHugCells);

            var floorCells = FilterEligible(
                CaveDecorContextClassifier.CollectCells(level, CaveDecorPlacementContext.FloorCluster),
                level, playerSpawnGrid, pathTiles);
            // spec_cave_decor_composition_runtime (CV03), critério 14.4: densidade de chão reduzida por
            // CaveEcosystemBalanceSO.FloorClusterDensityMultiplier (default 0.5) — menos "confete";
            // cada semente aceita ainda expande para 2-4 elementos (cluster), então a redução é só na
            // quantidade de AGRUPAMENTOS, não no tamanho de cada um.
            PlaceFloorClusters(level, decorEntries, seed, floorClusterDensity, used, placements, floorCells);
        }

        /// <summary>Filtro de distância aplicado a células de CeilingHang (WallTiles), que não passam
        /// por <see cref="IsEligible"/> (esse exclui WallTiles por definição). Mantém estalactites longe
        /// da entrada/saída, sem depender do path buffer (irrelevante para tiles não-walkable).</summary>
        private static List<Vector2Int> FilterFarFromEntranceExit(List<Vector2Int> cells, CaveGeneratedLevel level)
        {
            var result = new List<Vector2Int>(cells.Count);
            foreach (var cell in cells)
            {
                if (ManhattanDistance(cell, level.Entrance) < MinDistanceFromEntrance)
                {
                    continue;
                }

                if (ManhattanDistance(cell, level.Exit) < MinDistanceFromExit)
                {
                    continue;
                }

                result.Add(cell);
            }

            return result;
        }

        private static List<Vector2Int> FilterEligible(
            List<Vector2Int> cells,
            CaveGeneratedLevel level,
            Vector2Int playerSpawnGrid,
            HashSet<Vector2Int> pathTiles)
        {
            var result = new List<Vector2Int>(cells.Count);
            foreach (var cell in cells)
            {
                if (IsEligible(cell, level, playerSpawnGrid, pathTiles))
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        /// <summary>CeilingHang/WallHug: 1 elemento por célula, ordem determinística por hash, cota =
        /// density * contagem de células do contexto (mesma fração de densidade do loop legado).</summary>
        private static void PlaceContextSingletons(
            CaveGeneratedLevel level,
            List<CaveEnvironmentElementProfileSO.ElementEntry> decorEntries,
            int seed,
            float density,
            HashSet<Vector2Int> used,
            List<CaveEnvironmentElementPlacement> placements,
            CaveDecorPlacementContext context,
            List<Vector2Int> cells)
        {
            if (cells.Count == 0)
            {
                return;
            }

            var ordered = cells
                .OrderBy(tile => CaveLayoutStableHash.Compute($"{seed}|ctx_{context}|{tile.x}|{tile.y}"))
                .ThenBy(tile => tile.x)
                .ThenBy(tile => tile.y)
                .ToList();

            var target = Mathf.Clamp(Mathf.RoundToInt(ordered.Count * density), 0, ordered.Count);
            var placed = 0;

            foreach (var tile in ordered)
            {
                if (placed >= target)
                {
                    break;
                }

                if (used.Contains(tile))
                {
                    continue;
                }

                var entry = PickEntry(decorEntries, seed, tile);
                if (!entry.HasValue)
                {
                    continue;
                }

                used.Add(tile);
                placements.Add(BuildPlacement(level.CaveLevel, tile, entry.Value, context));
                placed++;
            }
        }

        /// <summary>FloorCluster: escolhe sementes deterministicamente (ordem por hash) e expande cada
        /// semente aceita para 1-3 vizinhos ortogonais livres do mesmo contexto (contagem de extras por
        /// hash da própria semente — critério 14.2: cluster de 2-4 elementos). Cota igual ao singleton
        /// path (density * contagem de células do contexto), medida em SEMENTES aceitas (não em
        /// elementos totais) para preservar a proporção de "quantos agrupamentos" versus densidade bruta.</summary>
        private static void PlaceFloorClusters(
            CaveGeneratedLevel level,
            List<CaveEnvironmentElementProfileSO.ElementEntry> decorEntries,
            int seed,
            float density,
            HashSet<Vector2Int> used,
            List<CaveEnvironmentElementPlacement> placements,
            List<Vector2Int> cells)
        {
            if (cells.Count == 0)
            {
                return;
            }

            var cellSet = new HashSet<Vector2Int>(cells);
            var ordered = cells
                .OrderBy(tile => CaveLayoutStableHash.Compute($"{seed}|ctx_floor_seed|{tile.x}|{tile.y}"))
                .ThenBy(tile => tile.x)
                .ThenBy(tile => tile.y)
                .ToList();

            // Cota de ELEMENTOS (não sementes) para manter a densidade final comparável ao antigo
            // "1 por célula": cada semente aceita consome (1 + extras) do budget total.
            var elementBudget = Mathf.Clamp(Mathf.RoundToInt(ordered.Count * density), 0, ordered.Count);
            var placedElements = 0;

            foreach (var seedTile in ordered)
            {
                if (placedElements >= elementBudget)
                {
                    break;
                }

                if (used.Contains(seedTile))
                {
                    continue;
                }

                var seedEntry = PickEntry(decorEntries, seed, seedTile);
                if (!seedEntry.HasValue)
                {
                    continue;
                }

                used.Add(seedTile);
                placements.Add(BuildPlacement(level.CaveLevel, seedTile, seedEntry.Value, CaveDecorPlacementContext.FloorCluster));
                placedElements++;

                var extraCount = ResolveClusterExtraCount(seed, seedTile);
                var neighbors = OrderedFreeNeighbors(seedTile, seed, cellSet, used);

                foreach (var neighbor in neighbors)
                {
                    if (extraCount <= 0 || placedElements >= elementBudget)
                    {
                        break;
                    }

                    var neighborEntry = PickEntry(decorEntries, seed, neighbor);
                    if (!neighborEntry.HasValue)
                    {
                        continue;
                    }

                    used.Add(neighbor);
                    placements.Add(BuildPlacement(level.CaveLevel, neighbor, neighborEntry.Value, CaveDecorPlacementContext.FloorCluster));
                    placedElements++;
                    extraCount--;
                }
            }
        }

        /// <summary>Tamanho de cluster determinístico no range [MinClusterExtra..MaxClusterExtra] (1-3
        /// extras além da semente = 2-4 elementos por cluster), derivado do hash da própria semente.</summary>
        private static int ResolveClusterExtraCount(int seed, Vector2Int seedTile)
        {
            var hash = CaveLayoutStableHash.Compute($"{seed}|cluster_size|{seedTile.x}|{seedTile.y}");
            var range = MaxClusterExtra - MinClusterExtra + 1;
            return MinClusterExtra + Mathf.Abs(hash) % range;
        }

        private static readonly Vector2Int[] OrthogonalDirs =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        /// <summary>Vizinhos ortogonais de <paramref name="seedTile"/> que pertencem ao mesmo conjunto de
        /// células do contexto e ainda não foram usados, em ordem determinística por hash.</summary>
        private static List<Vector2Int> OrderedFreeNeighbors(
            Vector2Int seedTile,
            int seed,
            HashSet<Vector2Int> contextCells,
            HashSet<Vector2Int> used)
        {
            var result = new List<Vector2Int>(4);
            foreach (var dir in OrthogonalDirs)
            {
                var neighbor = seedTile + dir;
                if (contextCells.Contains(neighbor) && !used.Contains(neighbor))
                {
                    result.Add(neighbor);
                }
            }

            result.Sort((a, b) =>
            {
                var hashA = CaveLayoutStableHash.Compute($"{seed}|cluster_nbr|{a.x}|{a.y}");
                var hashB = CaveLayoutStableHash.Compute($"{seed}|cluster_nbr|{b.x}|{b.y}");
                return hashA != hashB ? hashA.CompareTo(hashB) : (a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            });

            return result;
        }

        private static CaveEnvironmentElementProfileSO.ElementEntry? PickEntry(
            IReadOnlyList<CaveEnvironmentElementProfileSO.ElementEntry> entries,
            int seed,
            Vector2Int tile)
        {
            var totalWeight = 0f;
            foreach (var e in entries)
            {
                totalWeight += Mathf.Max(0f, e.Weight);
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            // Roll determinístico em [0, totalWeight) por tile.
            var hash = CaveLayoutStableHash.Compute($"{seed}|pick|{tile.x}|{tile.y}");
            var unit = (hash & 0x7fffffff) / (float)int.MaxValue; // [0,1)
            var target = unit * totalWeight;

            var cursor = 0f;
            foreach (var e in entries)
            {
                cursor += Mathf.Max(0f, e.Weight);
                if (target < cursor)
                {
                    return e;
                }
            }

            return entries[entries.Count - 1];
        }

        private static CaveEnvironmentElementPlacement BuildPlacement(
            int caveLevel,
            Vector2Int tile,
            CaveEnvironmentElementProfileSO.ElementEntry entry,
            CaveDecorPlacementContext context)
        {
            var isMineable = entry.Kind == CaveEnvironmentElementKind.MineableNode;
            var elementId = BuildElementId(caveLevel, tile, entry.Kind);
            return new CaveEnvironmentElementPlacement(
                elementId,
                entry.Kind,
                tile,
                isMineable,
                isMineable ? entry.MineNodeDataId : string.Empty,
                context);
        }

        public static string BuildElementId(int caveLevel, Vector2Int tile, CaveEnvironmentElementKind kind)
        {
            return $"cave_elem_{caveLevel}_{tile.x}_{tile.y}_{kind}";
        }

        private static bool IsEligible(
            Vector2Int tile,
            CaveGeneratedLevel level,
            Vector2Int playerSpawnGrid,
            HashSet<Vector2Int> pathTiles)
        {
            if (tile == level.Entrance || tile == level.Exit)
            {
                return false;
            }

            if (pathTiles.Contains(tile))
            {
                return false;
            }

            if (level.WallTiles.Contains(tile))
            {
                return false;
            }

            if (ManhattanDistance(tile, level.Entrance) < MinDistanceFromEntrance)
            {
                return false;
            }

            if (ManhattanDistance(tile, level.Exit) < MinDistanceFromExit)
            {
                return false;
            }

            if (ManhattanDistance(tile, playerSpawnGrid) < MinDistanceFromEntrance)
            {
                return false;
            }

            return true;
        }

        private static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
