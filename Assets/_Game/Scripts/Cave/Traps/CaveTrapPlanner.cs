using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using UnityEngine;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — planejador DETERMINÍSTICO de armadilhas por bioma/tier (cave-stable-run / ADR-0005).
    ///
    /// Pure C# (sem MonoBehaviour, sem cena) → 100% testável em EditMode. Consome um
    /// <see cref="CaveGeneratedLevel"/> já gerado (ou restaurado de snapshot, idêntico) + os seeds
    /// da run. Tudo derivado de StableHash(worldSeed|runSeed|level|"traps"|salt):
    ///
    /// - quantidade pela tabela §24 (tamanho do nível) ∩ §25 (tier/banda), clampada ao subset e à
    ///   disponibilidade do pool do bioma;
    /// - tipos do POOL do bioma §26 (<see cref="TrapDefinition.PoolForBiome"/>), na mesma régua de
    ///   bandas do <see cref="CaveBiomeLayoutProfile"/>;
    /// - posições em células válidas FORA do caminho crítico entrance↔exit (reusa o BFS já validado
    ///   do <see cref="CaveHazardPlanner.ComputeEntranceExitPathWithBuffer"/>) e longe do spawn do
    ///   player e das âncoras de entrada/saída;
    /// - trapInstanceIds ESTÁVEIS (hash posicional de runSeed/level/cell/trapKey — sem GUID/timestamp).
    ///
    /// Revisitar o mesmo nível/run reproduz EXATAMENTE o mesmo plano. Não cria um segundo caminho de
    /// hazard: é a camada de "armadilha tipada por bioma/tier" sobre a MESMA superfície determinística
    /// do fable_09 (BFS de caminho crítico, StableHash, perfil de banda).
    /// </summary>
    public static class CaveTrapPlanner
    {
        public const int MinDistanceFromEntrance = 4;
        public const int MinDistanceFromExit = 3;
        public const int MinDistanceFromPlayerSpawn = 3;
        public const int MinDistanceBetweenTraps = 3;

        // §24 — faixa de quantidade por TAMANHO do nível (área). Limiares na régua do
        // CaveBiomeLayoutProfile (Small 42×42, Base 55×55, Large 65×65). Clampado ao subset v1.
        public const int SmallAreaThreshold = 42 * 42;  // 1764
        public const int LargeAreaThreshold = 60 * 60;   // 3600

        /// <summary>
        /// Plano determinístico de armadilhas para o nível. <paramref name="playerSpawnGrid"/> mantém
        /// armadilhas longe do ponto de chegada do player. Nunca retorna null.
        /// </summary>
        public static CaveTrapPlan BuildPlan(
            CaveGeneratedLevel level,
            string caveWorldSeed,
            string caveRunSeed,
            Vector2Int playerSpawnGrid)
        {
            var plan = new CaveTrapPlan();
            if (level == null || level.WalkableTiles == null || level.WalkableTiles.Count == 0)
            {
                return plan;
            }

            var profile = CaveBiomeLayoutProfile.ForLevel(level.CaveLevel);
            var band = ResolveBand(level.CaveLevel);
            var pool = TrapDefinition.PoolForBiome(profile.BandId);
            if (pool.Count == 0)
            {
                return plan; // bioma sem tipos no subset v1 → sem armadilhas (rollback inofensivo)
            }

            var seedSource = $"{caveWorldSeed}|{caveRunSeed}|{level.CaveLevel}|traps";
            var seed = CaveLayoutStableHash.Compute(seedSource);

            var desiredCount = ResolveTrapCount(level, band, seed);
            if (desiredCount <= 0)
            {
                return plan; // alguns níveis ficam sem armadilha (faixa inclui 0)
            }

            // Caminho crítico entrance↔exit (+buffer) — reusa o BFS já validado do fable_09.
            var pathTiles = CaveHazardPlanner.ComputeEntranceExitPathWithBuffer(level);

            var candidates = level.WalkableTiles
                .Where(tile => IsTrapEligible(tile, level, playerSpawnGrid, pathTiles))
                .OrderBy(tile => CaveLayoutStableHash.Compute($"{seed}|cell|{tile.x}|{tile.y}"))
                .ThenBy(tile => tile.x)
                .ThenBy(tile => tile.y)
                .ToList();

            foreach (var tile in candidates)
            {
                if (plan.Traps.Count >= desiredCount)
                {
                    break;
                }

                var farEnough = plan.Traps.All(existing =>
                    ManhattanDistance(existing.Cell, tile) >= MinDistanceBetweenTraps);
                if (!farEnough)
                {
                    continue;
                }

                var typeIndex = Mathf.Abs(CaveLayoutStableHash.Compute($"{seed}|type|{tile.x}|{tile.y}")) % pool.Count;
                var def = pool[typeIndex];

                plan.Traps.Add(new CaveTrapPlacement
                {
                    TrapInstanceId = BuildTrapInstanceId(caveRunSeed, level.CaveLevel, tile, def.Id),
                    TrapId = def.Id,
                    Cell = tile,
                    Band = band
                });
            }

            return plan;
        }

        /// <summary>
        /// Banda 1..7 do nível na régua canônica (stone 1-10 → 1, fungal 11-25 → 2, ice 26-40 → 3,
        /// fire 41-55 → 4, ruins 56-70 → 5, deep 71-85 → 6, void 86+ → 7). Mesmo agrupamento do
        /// <see cref="CaveBiomeLayoutProfile"/>. Tier §25 = banda.
        /// </summary>
        public static int ResolveBand(int caveLevel)
        {
            var level = Mathf.Max(1, caveLevel);
            if (level <= 10) return 1;
            if (level <= 25) return 2;
            if (level <= 40) return 3;
            if (level <= 55) return 4;
            if (level <= 70) return 5;
            if (level <= 85) return 6;
            return 7;
        }

        /// <summary>
        /// Quantidade determinística de armadilhas: mínimo entre a faixa de §24 (tamanho do nível) e a
        /// faixa de §25 (tier/banda), clampada ao subset (teto v1) e ao tamanho do pool do bioma. A
        /// faixa inclui 0 (níveis sem armadilha). Determinístico por <paramref name="seed"/>.
        /// </summary>
        public static int ResolveTrapCount(CaveGeneratedLevel level, int band, int seed)
        {
            var area = Mathf.Max(1, level.Width * level.Height);

            ResolveSizeRange(area, out var sizeMin, out var sizeMax);
            ResolveTierRange(band, out var tierMin, out var tierMax);

            // Interseção das duas tabelas (§24 ∩ §25): faixa final clampada e nunca invertida.
            var min = Mathf.Max(sizeMin, tierMin);
            var max = Mathf.Min(sizeMax, tierMax);
            if (max < min)
            {
                max = min;
            }

            // Teto do subset v1 (catálogo de 10 tipos; mantém o nível legível). Conservador.
            max = Mathf.Min(max, 5);
            min = Mathf.Min(min, max);

            var span = max - min + 1;
            var roll = Mathf.Abs(CaveLayoutStableHash.Compute($"{seed}|count")) % span;
            return min + roll;
        }

        /// <summary>§24 — faixa por tamanho do nível (Small/Medium/Large por área).</summary>
        public static void ResolveSizeRange(int area, out int min, out int max)
        {
            if (area <= SmallAreaThreshold)
            {
                min = 0; max = 2;   // Small
            }
            else if (area >= LargeAreaThreshold)
            {
                min = 2; max = 5;   // Large (clampado ao subset; direction vai até Huge 5-10)
            }
            else
            {
                min = 1; max = 4;   // Medium
            }
        }

        /// <summary>§25 — faixa por tier/banda (T1 baixo … T7 alto), clampada ao subset v1.</summary>
        public static void ResolveTierRange(int band, out int min, out int max)
        {
            var clamped = Mathf.Clamp(band, 1, 7);
            switch (clamped)
            {
                case 1: min = 0; max = 2; break; // stone
                case 2: min = 0; max = 3; break; // fungal
                case 3: min = 1; max = 3; break; // ice
                case 4: min = 1; max = 4; break; // fire
                case 5: min = 1; max = 4; break; // ruins
                case 6: min = 2; max = 5; break; // deep
                default: min = 2; max = 5; break; // void
            }
        }

        private static bool IsTrapEligible(
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
                return false; // §22 — nunca bloqueia o caminho crítico entrada→saída
            }

            if (ManhattanDistance(tile, level.Entrance) < MinDistanceFromEntrance)
            {
                return false;
            }

            if (ManhattanDistance(tile, level.Exit) < MinDistanceFromExit)
            {
                return false;
            }

            if (ManhattanDistance(tile, playerSpawnGrid) < MinDistanceFromPlayerSpawn)
            {
                return false;
            }

            return !level.WallTiles.Contains(tile);
        }

        /// <summary>
        /// Id estável de instância (hash posicional). Mesmo (runSeed, level, cell, trap) → mesmo id
        /// na revisita (cave-stable-run). Sem GUID/timestamp. Inclui um sufixo legível para debug.
        /// </summary>
        public static string BuildTrapInstanceId(string caveRunSeed, int caveLevel, Vector2Int cell, TrapId trapId)
        {
            var def = TrapDefinition.Get(trapId);
            var trapKey = def != null ? def.TrapKey : trapId.ToString().ToLowerInvariant();
            var hash = CaveLayoutStableHash.Compute($"{caveRunSeed}|{caveLevel}|{cell.x}|{cell.y}|{trapKey}");
            var unsigned = unchecked((uint)hash);
            return $"trap_{caveLevel}_{cell.x}_{cell.y}_{trapKey}_{unsigned:x8}";
        }

        private static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }
}
