using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_09 — planejador DETERMINÍSTICO de hazards e sala de tesouro (cave-stable-run / ADR-0005).
    ///
    /// Pure C# (sem MonoBehaviour, sem cena) → 100% testável em EditMode. Consome um
    /// <see cref="CaveGeneratedLevel"/> já gerado (ou restaurado de snapshot, idêntico) + os seeds
    /// da run. Tudo derivado de StableHash(worldSeed|runSeed|level|salt):
    ///
    /// - 0..MaxHazards hazards por nível (banda decide o teto), NUNCA em entrance/exit, no caminho
    ///   entrance↔exit (BFS sobre os walkable, 4-conn), nem dentro do raio do spawn do player;
    /// - ~15% dos níveis ganham 1 sala de tesouro (banda ajusta), com baú no centro de uma sala
    ///   elegível distante da entrada + âncoras para 2 guardiões.
    ///
    /// Revisitar o mesmo nível/run reproduz EXATAMENTE o mesmo plano (mesma entrada → mesma saída).
    /// Sem GUID/timestamp/Random não-semeado.
    /// </summary>
    public static class CaveHazardPlanner
    {
        public const int MinDistanceFromEntrance = 4;
        public const int MinDistanceFromExit = 3;
        public const int MinDistanceFromPlayerSpawn = 3;
        public const int MinDistanceBetweenHazards = 3;
        private const int GuardiansPerTreasureRoom = 2;

        public static CaveHazardPlan BuildPlan(
            CaveGeneratedLevel level,
            string caveWorldSeed,
            string caveRunSeed,
            Vector2Int playerSpawnGrid)
        {
            var plan = new CaveHazardPlan();
            if (level == null || level.WalkableTiles == null || level.WalkableTiles.Count == 0)
            {
                return plan;
            }

            var profile = CaveBiomeLayoutProfile.ForLevel(level.CaveLevel);

            // Conjunto de tiles do caminho obrigatório entrance↔exit (+ buffer) — proibido p/ hazards.
            var pathTiles = ComputeEntranceExitPathWithBuffer(level);

            plan.Hazards.AddRange(BuildHazards(level, profile, caveWorldSeed, caveRunSeed, playerSpawnGrid, pathTiles));
            plan.TreasureRoom = BuildTreasureRoom(level, profile, caveWorldSeed, caveRunSeed);
            return plan;
        }

        // --- Hazards ------------------------------------------------------------------------------

        private static IEnumerable<CaveHazardPlacement> BuildHazards(
            CaveGeneratedLevel level,
            CaveBiomeLayoutProfile profile,
            string worldSeed,
            string runSeed,
            Vector2Int playerSpawnGrid,
            HashSet<Vector2Int> pathTiles)
        {
            var results = new List<CaveHazardPlacement>();
            if (profile.MaxHazards <= 0 || profile.AllowedHazards.Length == 0)
            {
                return results;
            }

            var seedSource = $"{worldSeed}|{runSeed}|{level.CaveLevel}|hazard";
            var seed = CaveLayoutStableHash.Compute(seedSource);

            // Quantidade determinística 0..MaxHazards (inclusivo) — alguns níveis ficam sem hazard.
            var hazardCount = Mathf.Abs(seed % (profile.MaxHazards + 1));
            if (hazardCount <= 0)
            {
                return results;
            }

            var candidates = level.WalkableTiles
                .Where(tile => IsHazardEligible(tile, level, playerSpawnGrid, pathTiles))
                .OrderBy(tile => CaveLayoutStableHash.Compute($"{seed}|{tile.x}|{tile.y}"))
                .ThenBy(tile => tile.x)
                .ThenBy(tile => tile.y)
                .ToList();

            foreach (var tile in candidates)
            {
                if (results.Count >= hazardCount)
                {
                    break;
                }

                var farEnough = results.All(existing =>
                    ManhattanDistance(existing.GridPosition, tile) >= MinDistanceBetweenHazards);
                if (!farEnough)
                {
                    continue;
                }

                var kindIndex = Mathf.Abs(CaveLayoutStableHash.Compute($"{seed}|kind|{tile.x}|{tile.y}"))
                                % profile.AllowedHazards.Length;
                var kind = profile.AllowedHazards[kindIndex];

                results.Add(new CaveHazardPlacement
                {
                    HazardId = BuildHazardId(level.CaveLevel, tile, kind),
                    GridPosition = tile,
                    Kind = kind
                });
            }

            return results;
        }

        private static bool IsHazardEligible(
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

        public static string BuildHazardId(int caveLevel, Vector2Int tile, CaveHazardKind kind)
        {
            return $"hazard_{caveLevel}_{tile.x}_{tile.y}_{kind}";
        }

        // --- Sala de tesouro ----------------------------------------------------------------------

        private static CaveTreasureRoomPlacement BuildTreasureRoom(
            CaveGeneratedLevel level,
            CaveBiomeLayoutProfile profile,
            string worldSeed,
            string runSeed)
        {
            if (level.Rooms == null || level.Rooms.Count < 2)
            {
                return null; // precisa de ao menos a sala da entrada + 1 candidata
            }

            var seed = CaveLayoutStableHash.Compute($"{worldSeed}|{runSeed}|{level.CaveLevel}|treasure_room");
            var roll = Mathf.Abs(seed % 100);
            if (roll >= Mathf.Clamp(profile.TreasureRoomChancePercent, 0, 100))
            {
                return null;
            }

            var chosenRoom = SelectTreasureRoom(level, seed);
            if (!chosenRoom.HasValue)
            {
                return null;
            }

            var room = chosenRoom.Value;
            var chestPos = room.Center;
            if (!level.WalkableTiles.Contains(chestPos))
            {
                chestPos = FindNearestWalkableInRoom(level, room);
            }

            var placement = new CaveTreasureRoomPlacement
            {
                ChestId = BuildChestId(level.CaveLevel, chestPos),
                ChestGridPosition = chestPos,
                LootSeed = CaveLayoutStableHash.Compute($"{worldSeed}|{runSeed}|{level.CaveLevel}|treasure_loot|{chestPos.x}|{chestPos.y}")
            };

            placement.GuardianGridPositions.AddRange(
                ResolveGuardianAnchors(level, room, chestPos, seed));
            return placement;
        }

        private static CaveRoom? SelectTreasureRoom(CaveGeneratedLevel level, int seed)
        {
            // Salas elegíveis: NÃO a sala que contém a entrada (deixar o desvio valer a pena),
            // ordenadas deterministicamente; preferir as mais distantes da entrada.
            var entranceRoomIndex = FindRoomIndexContaining(level, level.Entrance);
            var eligible = new List<CaveRoom>();
            for (var i = 0; i < level.Rooms.Count; i++)
            {
                if (i == entranceRoomIndex)
                {
                    continue;
                }

                eligible.Add(level.Rooms[i]);
            }

            if (eligible.Count == 0)
            {
                return null;
            }

            var ordered = eligible
                .OrderByDescending(r => ManhattanDistance(r.Center, level.Entrance))
                .ThenBy(r => CaveLayoutStableHash.Compute($"{seed}|room|{r.X}|{r.Y}"))
                .ToList();

            // Sorteia entre o terço mais distante (sala de tesouro "vale o desvio"), de forma estável.
            var poolSize = Mathf.Max(1, ordered.Count / 3);
            var pick = Mathf.Abs(CaveLayoutStableHash.Compute($"{seed}|treasure_pick") % poolSize);
            return ordered[pick];
        }

        private static List<Vector2Int> ResolveGuardianAnchors(
            CaveGeneratedLevel level,
            CaveRoom room,
            Vector2Int chestPos,
            int seed)
        {
            var anchors = level.WalkableTiles
                .Where(tile => room.Contains(tile) && tile != chestPos)
                .OrderBy(tile => CaveLayoutStableHash.Compute($"{seed}|guardian|{tile.x}|{tile.y}"))
                .ThenBy(tile => tile.x)
                .ThenBy(tile => tile.y)
                .Take(GuardiansPerTreasureRoom)
                .ToList();

            // Fallback: se a sala for minúscula, usa o próprio centro deslocado.
            if (anchors.Count == 0)
            {
                anchors.Add(chestPos);
            }

            return anchors;
        }

        public static string BuildChestId(int caveLevel, Vector2Int tile)
        {
            return $"chest_{caveLevel}_{tile.x}_{tile.y}";
        }

        private static int FindRoomIndexContaining(CaveGeneratedLevel level, Vector2Int point)
        {
            for (var i = 0; i < level.Rooms.Count; i++)
            {
                if (level.Rooms[i].Contains(point))
                {
                    return i;
                }
            }

            return -1;
        }

        private static Vector2Int FindNearestWalkableInRoom(CaveGeneratedLevel level, CaveRoom room)
        {
            var best = room.Center;
            var bestDistance = int.MaxValue;
            foreach (var tile in level.WalkableTiles)
            {
                if (!room.Contains(tile))
                {
                    continue;
                }

                var distance = ManhattanDistance(tile, room.Center);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = tile;
                }
            }

            return best;
        }

        // --- Caminho entrance↔exit (BFS 4-conn sobre walkable) ------------------------------------

        /// <summary>
        /// Caminho mais curto entrance→exit nos walkable (4-conn) + buffer de 1 tile. Hazards nunca
        /// caem aqui (mitigação do risco "hazard em corredor obrigatório bloquear caminho").
        /// Se não houver caminho (mapa degenerado), retorna ao menos entrance/exit para segurança.
        /// </summary>
        public static HashSet<Vector2Int> ComputeEntranceExitPathWithBuffer(CaveGeneratedLevel level)
        {
            var path = ComputeShortestPath(level, level.Entrance, level.Exit);
            var result = new HashSet<Vector2Int>(path);

            // Buffer de 1 tile ao redor do caminho (8-conn) para não estreitar demais o corredor.
            foreach (var tile in path)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        var neighbor = new Vector2Int(tile.x + dx, tile.y + dy);
                        if (level.WalkableTiles.Contains(neighbor))
                        {
                            result.Add(neighbor);
                        }
                    }
                }
            }

            result.Add(level.Entrance);
            result.Add(level.Exit);
            return result;
        }

        private static List<Vector2Int> ComputeShortestPath(CaveGeneratedLevel level, Vector2Int start, Vector2Int goal)
        {
            var path = new List<Vector2Int>();
            if (!level.WalkableTiles.Contains(start) || !level.WalkableTiles.Contains(goal))
            {
                return path;
            }

            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var visited = new HashSet<Vector2Int> { start };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);

            var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var found = false;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == goal)
                {
                    found = true;
                    break;
                }

                foreach (var dir in directions)
                {
                    var next = current + dir;
                    if (visited.Contains(next) || !level.WalkableTiles.Contains(next))
                    {
                        continue;
                    }

                    visited.Add(next);
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }

            if (!found)
            {
                return path;
            }

            var node = goal;
            path.Add(node);
            while (cameFrom.TryGetValue(node, out var prev))
            {
                node = prev;
                path.Add(node);
            }

            path.Reverse();
            return path;
        }

        private static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }
}
