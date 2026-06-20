using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveSnapshotService
    {
        public string BuildSnapshotId(string caveRunSeed, int caveLevel)
        {
            return VisitedLevelSnapshot.BuildSnapshotId(caveRunSeed, caveLevel);
        }

        public bool TryGetSnapshot(CaveRuntimeState state, string caveRunSeed, int caveLevel, out VisitedLevelSnapshot snapshot)
        {
            snapshot = null;
            if (state == null || state.VisitedLevelSnapshots == null)
            {
                return false;
            }

            if (!state.VisitedLevelSnapshots.TryGetValue(Mathf.Max(1, caveLevel), out var candidate)
                || candidate == null
                || !candidate.IsValid())
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(candidate.CaveRunSeed)
                && !string.IsNullOrWhiteSpace(caveRunSeed)
                && !string.Equals(candidate.CaveRunSeed, caveRunSeed, StringComparison.Ordinal))
            {
                return false;
            }

            snapshot = candidate;
            return true;
        }

        public VisitedLevelSnapshot CaptureSnapshot(
            CaveGeneratedLevel generatedLevel,
            string caveWorldSeed,
            string caveRunSeed,
            CaveEnemySpawnPlan enemySpawnPlan,
            IReadOnlyList<CaveResourceNodeSnapshotEntry> resourceNodeStates,
            CaveFishingSpotSnapshotEntry fishingSpotState,
            IEnumerable<string> depletedNodeIds,
            IEnumerable<EnemyHpRecord> enemyHpRecords = null,
            IEnumerable<string> openedChestIds = null,
            IEnumerable<CaveTrapSnapshotEntry> trapStates = null)
        {
            if (generatedLevel == null)
            {
                return null;
            }

            var snapshot = new CaveLevelSnapshot(
                generatedLevel.CaveLevel,
                generatedLevel.BiomeId,
                string.Empty,
                caveWorldSeed,
                caveRunSeed);

            snapshot.SetEntranceAndExit(generatedLevel.Entrance, generatedLevel.Exit);
            snapshot.SetLayoutDimensions(generatedLevel.Width, generatedLevel.Height);

            foreach (var walkableTile in generatedLevel.WalkableTiles.OrderBy(p => p.x).ThenBy(p => p.y))
            {
                snapshot.AddWalkableTile(walkableTile);
            }

            foreach (var wallTile in generatedLevel.WallTiles.OrderBy(p => p.x).ThenBy(p => p.y))
            {
                snapshot.AddWallTile(wallTile);
            }

            foreach (var point in generatedLevel.EnemySpawnPoints.OrderBy(p => p.Position.x).ThenBy(p => p.Position.y))
            {
                snapshot.AddEnemySpawnPoint((int)point.PointType, point.Position);
            }

            foreach (var point in generatedLevel.ResourceSpawnPoints.OrderBy(p => p.Position.x).ThenBy(p => p.Position.y))
            {
                snapshot.AddResourceSpawnPoint((int)point.PointType, point.Position);
            }

            snapshot.ResourceNodeStates.Clear();
            snapshot.ResourceNodes.Clear();
            foreach (var resourceState in resourceNodeStates ?? Array.Empty<CaveResourceNodeSnapshotEntry>())
            {
                if (resourceState == null || string.IsNullOrWhiteSpace(resourceState.NodeInstanceId))
                {
                    continue;
                }

                snapshot.ResourceNodeStates.Add(new CaveResourceNodeSnapshotEntry
                {
                    NodeInstanceId = resourceState.NodeInstanceId,
                    ResourceNodeId = resourceState.ResourceNodeId ?? string.Empty,
                    GridPosition = resourceState.GridPosition,
                    IsDepleted = resourceState.IsDepleted
                });
                snapshot.ResourceNodes.Add(new SerializedResourceNode
                {
                    NodeInstanceId = resourceState.NodeInstanceId,
                    ResourceDataId = resourceState.ResourceNodeId ?? string.Empty,
                    Position = resourceState.GridPosition
                });
            }

            foreach (var depletedId in depletedNodeIds ?? Array.Empty<string>())
            {
                snapshot.MarkResourceNodeDepleted(depletedId);
            }

            snapshot.SetFishingSpot(fishingSpotState ?? CreateFishingSpotHook(generatedLevel, caveWorldSeed, caveRunSeed));
            snapshot.SetEnemySpawnPlan(enemySpawnPlan);
            snapshot.SetEnemyHpRecords(enemyHpRecords); // F13: fora do LayoutHash (estado mutável)

            // fable_09: preserva baús abertos (estado mutável FORA do LayoutHash, como EnemyHpRecords).
            foreach (var chestId in openedChestIds ?? Array.Empty<string>())
            {
                snapshot.MarkChestOpened(chestId);
            }

            // fable_60: preserva o estado das armadilhas (Triggered/Disarmed não rearmam na revisita).
            // Estado mutável FORA do LayoutHash (mesmo tratamento de OpenedChestIds).
            foreach (var trap in trapStates ?? Array.Empty<CaveTrapSnapshotEntry>())
            {
                if (trap != null && !string.IsNullOrWhiteSpace(trap.TrapInstanceId))
                {
                    snapshot.SetTrapState(trap.TrapInstanceId, trap.TrapKey, trap.Cell, trap.State);
                }
            }

            snapshot.LayoutHash = CalculateLayoutHash(snapshot);
            return snapshot;
        }

        public CaveGeneratedLevel RestoreGeneratedLevel(VisitedLevelSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid())
            {
                return null;
            }

            var generatedLevel = new CaveGeneratedLevel
            {
                CaveLevel = snapshot.CaveLevel,
                BiomeId = snapshot.BiomeId,
                LayoutHash = snapshot.LayoutHash,
                Width = snapshot.Width,
                Height = snapshot.Height,
                Entrance = Vector2Int.RoundToInt(snapshot.EntrancePosition),
                Exit = Vector2Int.RoundToInt(snapshot.ExitPosition)
            };

            foreach (var walkableTile in snapshot.WalkableTilesList)
            {
                generatedLevel.WalkableTiles.Add(walkableTile);
            }

            foreach (var wallTile in snapshot.WallTilesList)
            {
                generatedLevel.WallTiles.Add(wallTile);
            }

            foreach (var point in snapshot.EnemySpawnPointsList)
            {
                generatedLevel.EnemySpawnPoints.Add(
                    new CaveGenerationPoint((CaveGenerationPointType)point.PointTypeValue, point.Position));
            }

            foreach (var point in snapshot.ResourceSpawnPointsList)
            {
                generatedLevel.ResourceSpawnPoints.Add(
                    new CaveGenerationPoint((CaveGenerationPointType)point.PointTypeValue, point.Position));
            }

            return generatedLevel;
        }

        public CaveFishingSpotSnapshotEntry CreateFishingSpotHook(
            CaveGeneratedLevel generatedLevel,
            string caveWorldSeed,
            string caveRunSeed)
        {
            var result = new CaveFishingSpotSnapshotEntry();
            if (generatedLevel == null || generatedLevel.WalkableTiles.Count == 0)
            {
                return result;
            }

            var seed = StableHash($"{caveWorldSeed}|{caveRunSeed}|{generatedLevel.CaveLevel}|fishing_spot");
            if (Math.Abs(seed % 100) >= 10)
            {
                return result;
            }

            var point = generatedLevel.WalkableTiles
                .OrderBy(p => StableHash($"{seed}|{p.x}|{p.y}"))
                .ThenBy(p => p.x)
                .ThenBy(p => p.y)
                .FirstOrDefault();

            result.HasFishingSpot = true;
            result.FishingSpotId = $"fish_{generatedLevel.CaveLevel}_{point.x}_{point.y}";
            result.GridPosition = point;
            result.FishingProfileId = $"fishing_{NormalizeBiome(generatedLevel.BiomeId)}";
            return result;
        }

        public string CalculateLayoutHash(VisitedLevelSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            builder.Append(snapshot.CaveLevel).Append('|')
                .Append(snapshot.BiomeId).Append('|')
                .Append(snapshot.Width).Append('x').Append(snapshot.Height).Append('|')
                .Append(VectorKey(Vector2Int.RoundToInt(snapshot.EntrancePosition))).Append('|')
                .Append(VectorKey(Vector2Int.RoundToInt(snapshot.ExitPosition))).Append('|');

            AppendVectorList(builder, "walk", snapshot.WalkableTilesList);
            AppendVectorList(builder, "wall", snapshot.WallTilesList);
            AppendResourceNodes(builder, snapshot.ResourceNodeStates);
            AppendFishingSpot(builder, snapshot.FishingSpotState);
            AppendEnemyPlan(builder, snapshot.EnemySpawnPlan);

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            return BitConverter.ToString(bytes, 0, 8).Replace("-", string.Empty).ToLowerInvariant();
        }

        public bool ValidateReplayHash(VisitedLevelSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid())
            {
                return false;
            }

            return string.Equals(snapshot.LayoutHash, CalculateLayoutHash(snapshot), StringComparison.Ordinal);
        }

        private static void AppendVectorList(StringBuilder builder, string label, IEnumerable<Vector2Int> points)
        {
            builder.Append(label).Append(':');
            foreach (var point in (points ?? Array.Empty<Vector2Int>()).OrderBy(p => p.x).ThenBy(p => p.y))
            {
                builder.Append(VectorKey(point)).Append(';');
            }
        }

        private static void AppendResourceNodes(StringBuilder builder, IEnumerable<CaveResourceNodeSnapshotEntry> entries)
        {
            builder.Append("resources:");
            foreach (var entry in (entries ?? Array.Empty<CaveResourceNodeSnapshotEntry>())
                         .Where(e => e != null)
                         .OrderBy(e => e.GridPosition.x)
                         .ThenBy(e => e.GridPosition.y)
                         .ThenBy(e => e.NodeInstanceId))
            {
                builder.Append(entry.NodeInstanceId).Append(',')
                    .Append(entry.ResourceNodeId).Append(',')
                    .Append(VectorKey(entry.GridPosition)).Append(',')
                    .Append(entry.IsDepleted).Append(';');
            }
        }

        private static void AppendFishingSpot(StringBuilder builder, CaveFishingSpotSnapshotEntry entry)
        {
            builder.Append("fishing:");
            if (entry == null || !entry.HasFishingSpot)
            {
                builder.Append("none;");
                return;
            }

            builder.Append(entry.FishingSpotId).Append(',')
                .Append(VectorKey(entry.GridPosition)).Append(',')
                .Append(entry.FishingProfileId).Append(';');
        }

        private static void AppendEnemyPlan(StringBuilder builder, CaveEnemySpawnPlan plan)
        {
            builder.Append("enemies:");
            foreach (var entry in (plan?.Entries ?? new List<CaveEnemySpawnPlanEntry>())
                         .Where(e => e != null)
                         .OrderBy(e => e.SpawnIndex)
                         .ThenBy(e => e.EnemyInstanceId))
            {
                builder.Append(entry.EnemyInstanceId).Append(',')
                    .Append(entry.EnemyId).Append(',')
                    .Append(entry.SpawnProfileId).Append(',')
                    .Append(entry.PackId).Append(',')
                    .Append(VectorKey(entry.GridPosition)).Append(',')
                    .Append(entry.RoomId).Append(',')
                    .Append(entry.SpawnIndex).Append(',')
                    .Append(entry.IsElite).Append(',')
                    .Append(entry.SizeClass).Append(',')
                    .Append(entry.FactionId).Append(';');
            }
        }

        private static string VectorKey(Vector2Int point)
        {
            return $"{point.x},{point.y}";
        }

        private static int StableHash(string value)
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

        private static string NormalizeBiome(string biomeId)
        {
            return string.IsNullOrWhiteSpace(biomeId)
                ? "cave"
                : new string(biomeId.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }
    }
}
