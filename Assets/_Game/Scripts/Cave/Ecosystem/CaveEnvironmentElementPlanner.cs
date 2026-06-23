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
    ///
    /// Pure C# (sem MonoBehaviour, sem cena) → 100% testável em EditMode. Consome um
    /// <see cref="CaveGeneratedLevel"/> já gerado/restaurado + o perfil do bioma + os seeds da run.
    /// Tudo derivado de <see cref="CaveLayoutStableHash"/> (FNV-1a) — revisitar o mesmo
    /// (worldSeed, runSeed, caveLevel) reproduz EXATAMENTE o mesmo plano. Sem GUID/timestamp/Random.
    ///
    /// Invariantes:
    /// - elementos só caem em walkable tiles;
    /// - decor bloqueante e nós mineráveis NUNCA caem no caminho entrance↔exit (BFS 4-conn, mesmo
    ///   espírito do <see cref="CaveHazardPlanner"/>) → sem softlock;
    /// - pedra e minério estão presentes em TODAS as bandas (garantia de presença);
    /// - densidade conforme EnvironmentElementDensityByBand do balance SO (ou default por banda).
    /// </summary>
    public static class CaveEnvironmentElementPlanner
    {
        private const int MinDistanceFromEntrance = 3;
        private const int MinDistanceFromExit = 2;

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

            // Garantia de presença: pedra (decor) + minério (mineável) em TODAS as bandas.
            EnsureGuaranteedPresence(level.CaveLevel, profile, seed, candidates, used, placements);

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

                var entry = PickEntry(entries, seed, tile);
                if (!entry.HasValue)
                {
                    continue;
                }

                used.Add(tile);
                placements.Add(BuildPlacement(level.CaveLevel, tile, entry.Value));
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

        private static void EnsureGuaranteedPresence(
            int caveLevel,
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
            PlaceGuaranteed(caveLevel, candidates, used, placements, stoneEntry);

            // Minério (mineável) garantido — usa o primeiro mineNodeDataId declarado no perfil, se houver.
            var oreEntry = new CaveEnvironmentElementProfileSO.ElementEntry
            {
                Kind = CaveEnvironmentElementKind.MineableNode,
                Weight = 1f,
                MineNodeDataId = FindFirstMineNodeId(profile)
            };
            PlaceGuaranteed(caveLevel, candidates, used, placements, oreEntry);
        }

        private static void PlaceGuaranteed(
            int caveLevel,
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
                placements.Add(BuildPlacement(caveLevel, tile, entry));
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
            CaveEnvironmentElementProfileSO.ElementEntry entry)
        {
            var isMineable = entry.Kind == CaveEnvironmentElementKind.MineableNode;
            var elementId = BuildElementId(caveLevel, tile, entry.Kind);
            return new CaveEnvironmentElementPlacement(
                elementId,
                entry.Kind,
                tile,
                isMineable,
                isMineable ? entry.MineNodeDataId : string.Empty);
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
