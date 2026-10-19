using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [Serializable]
    public sealed class CaveSaveData
    {
        public int CurrentCaveLevel = 1;
        public int DeepestLayerReached = 1;
        public string CaveWorldSeed = string.Empty;
        public string CaveRunSeed = string.Empty;
        public List<int> UnlockedCheckpoints = new List<int>();
        public List<string> DepletedNodeIds = new List<string>();
        public List<SerializedVisitedLevelSnapshot> VisitedLevelSnapshots = new List<SerializedVisitedLevelSnapshot>();
        public List<CaveBossDefeatState> BossDefeatStates = new List<CaveBossDefeatState>();

        public void PopulateSnapshots(Dictionary<int, VisitedLevelSnapshot> snapshots)
        {
            VisitedLevelSnapshots.Clear();
            foreach (var kvp in snapshots)
            {
                VisitedLevelSnapshots.Add(SerializedVisitedLevelSnapshot.FromSnapshot(kvp.Value));
            }
        }

        public Dictionary<int, VisitedLevelSnapshot> RestoreSnapshots()
        {
            var result = new Dictionary<int, VisitedLevelSnapshot>();
            foreach (var serialized in VisitedLevelSnapshots)
            {
                var snapshot = serialized.ToSnapshot();
                if (snapshot != null && snapshot.IsValid())
                {
                    result[snapshot.CaveLevel] = snapshot;
                }
            }
            return result;
        }

        public void PopulateBossDefeatStates(Dictionary<string, CaveBossDefeatState> bossStates)
        {
            BossDefeatStates.Clear();
            foreach (var kvp in bossStates)
            {
                BossDefeatStates.Add(kvp.Value);
            }
        }

        public Dictionary<string, CaveBossDefeatState> RestoreBossDefeatStates()
        {
            var result = new Dictionary<string, CaveBossDefeatState>();
            foreach (var state in BossDefeatStates)
            {
                if (state != null && !string.IsNullOrWhiteSpace(state.BossGateId))
                {
                    result[state.BossGateId] = state;
                }
            }
            return result;
        }
    }

    [Serializable]
    public sealed class SerializedVisitedLevelSnapshot
    {
        public int CaveLevel;
        public string SnapshotId;
        public string CaveWorldSeed;
        public string CaveRunSeed;
        public string BiomeId;
        public string LayoutHash;
        public int Width;
        public int Height;
        public Vector2 EntrancePosition;
        public Vector2 ExitPosition;
        public List<Vector2Int> WalkableTilesList = new List<Vector2Int>();
        public List<Vector2Int> WallTilesList = new List<Vector2Int>();
        public List<SerializedCaveGenerationPoint> EnemySpawnPointsList = new List<SerializedCaveGenerationPoint>();
        public List<SerializedCaveGenerationPoint> ResourceSpawnPointsList = new List<SerializedCaveGenerationPoint>();
        public List<SerializedEnemySpawn> EnemySpawns = new List<SerializedEnemySpawn>();
        public List<SerializedResourceNode> ResourceNodes = new List<SerializedResourceNode>();
        public List<CaveResourceNodeSnapshotEntry> ResourceNodeStates = new List<CaveResourceNodeSnapshotEntry>();
        public List<string> DepletedResourceNodeIds = new List<string>();
        public CaveFishingSpotSnapshotEntry FishingSpotState = new CaveFishingSpotSnapshotEntry();
        public CaveEnemySpawnPlan EnemySpawnPlan = new CaveEnemySpawnPlan();
        public List<EnemySpawnPlanEntry> LegacyEnemySpawnPlanEntries = new List<EnemySpawnPlanEntry>();
        public List<string> Warnings = new List<string>();
        // fable_78 (16.4): campos ADITIVOS — elementos ambientais, presença de água, estado de conflito.
        // Tipos simples + IDs apenas (sem refs Unity). Save antigo sem estes campos carrega com defaults
        // seguros (listas vazias, HasWater=false, conflito inativo) → regeneração determinística.
        public List<SerializedEnvironmentElement> EnvironmentElements = new List<SerializedEnvironmentElement>();
        public bool HasWater;
        public CaveConflictSnapshot ConflictState = new CaveConflictSnapshot();

        public static SerializedVisitedLevelSnapshot FromSnapshot(VisitedLevelSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }

            return new SerializedVisitedLevelSnapshot
            {
                CaveLevel = snapshot.CaveLevel,
                SnapshotId = snapshot.SnapshotId,
                CaveWorldSeed = snapshot.CaveWorldSeed,
                CaveRunSeed = snapshot.CaveRunSeed,
                BiomeId = snapshot.BiomeId,
                LayoutHash = snapshot.LayoutHash,
                Width = snapshot.Width,
                Height = snapshot.Height,
                EntrancePosition = snapshot.EntrancePosition,
                ExitPosition = snapshot.ExitPosition,
                WalkableTilesList = new List<Vector2Int>(snapshot.WalkableTilesList),
                WallTilesList = new List<Vector2Int>(snapshot.WallTilesList),
                EnemySpawnPointsList = new List<SerializedCaveGenerationPoint>(snapshot.EnemySpawnPointsList),
                ResourceSpawnPointsList = new List<SerializedCaveGenerationPoint>(snapshot.ResourceSpawnPointsList),
                EnemySpawns = new List<SerializedEnemySpawn>(snapshot.EnemySpawns),
                ResourceNodes = new List<SerializedResourceNode>(snapshot.ResourceNodes),
                ResourceNodeStates = new List<CaveResourceNodeSnapshotEntry>(snapshot.ResourceNodeStates),
                DepletedResourceNodeIds = new List<string>(snapshot.DepletedResourceNodeIds),
                FishingSpotState = snapshot.FishingSpotState,
                EnemySpawnPlan = snapshot.EnemySpawnPlan,
                LegacyEnemySpawnPlanEntries = new List<EnemySpawnPlanEntry>(snapshot.LegacyEnemySpawnPlanEntries),
                Warnings = new List<string>(snapshot.Warnings),
                EnvironmentElements = CloneEnvironmentElements(snapshot.EnvironmentElements),
                HasWater = snapshot.HasWater,
                ConflictState = CloneConflictState(snapshot.ConflictState)
            };
        }

        // fable_78: deep-copy dos elementos ambientais (lista de DTOs simples — sem refs Unity).
        private static List<SerializedEnvironmentElement> CloneEnvironmentElements(
            List<SerializedEnvironmentElement> source)
        {
            var result = new List<SerializedEnvironmentElement>();
            if (source == null)
            {
                return result;
            }

            foreach (var element in source)
            {
                if (element == null || string.IsNullOrWhiteSpace(element.ElementId))
                {
                    continue;
                }

                result.Add(new SerializedEnvironmentElement
                {
                    ElementId = element.ElementId,
                    Kind = element.Kind,
                    GridX = element.GridX,
                    GridY = element.GridY,
                    IsMineable = element.IsMineable,
                    MineNodeDataId = element.MineNodeDataId ?? string.Empty,
                    IsDepleted = element.IsDepleted
                });
            }

            return result;
        }

        // fable_78: deep-copy do estado de conflito (default seguro quando ausente — back-compat).
        private static CaveConflictSnapshot CloneConflictState(CaveConflictSnapshot source)
        {
            if (source == null)
            {
                return new CaveConflictSnapshot();
            }

            return new CaveConflictSnapshot
            {
                ConflictActive = source.ConflictActive,
                FactionAId = source.FactionAId ?? string.Empty,
                FactionBId = source.FactionBId ?? string.Empty,
                HasHadConflict = source.HasHadConflict,
                EntryCount = source.EntryCount
            };
        }

        public VisitedLevelSnapshot ToSnapshot()
        {
            if (CaveLevel <= 0 || string.IsNullOrWhiteSpace(SnapshotId))
            {
                return null;
            }

            var snapshot = new VisitedLevelSnapshot(CaveLevel, BiomeId, LayoutHash)
            {
                SnapshotId = SnapshotId,
                CaveWorldSeed = CaveWorldSeed,
                CaveRunSeed = CaveRunSeed,
                Width = Width,
                Height = Height,
                EntrancePosition = EntrancePosition,
                ExitPosition = ExitPosition,
                WalkableTilesList = new List<Vector2Int>(WalkableTilesList ?? new List<Vector2Int>()),
                WallTilesList = new List<Vector2Int>(WallTilesList ?? new List<Vector2Int>()),
                EnemySpawnPointsList = new List<SerializedCaveGenerationPoint>(EnemySpawnPointsList ?? new List<SerializedCaveGenerationPoint>()),
                ResourceSpawnPointsList = new List<SerializedCaveGenerationPoint>(ResourceSpawnPointsList ?? new List<SerializedCaveGenerationPoint>()),
                EnemySpawns = new List<SerializedEnemySpawn>(EnemySpawns),
                ResourceNodes = new List<SerializedResourceNode>(ResourceNodes),
                ResourceNodeStates = new List<CaveResourceNodeSnapshotEntry>(ResourceNodeStates ?? new List<CaveResourceNodeSnapshotEntry>()),
                DepletedResourceNodeIds = new List<string>(DepletedResourceNodeIds),
                FishingSpotState = FishingSpotState ?? new CaveFishingSpotSnapshotEntry(),
                EnemySpawnPlan = EnemySpawnPlan ?? new CaveEnemySpawnPlan(),
                LegacyEnemySpawnPlanEntries = new List<EnemySpawnPlanEntry>(LegacyEnemySpawnPlanEntries ?? new List<EnemySpawnPlanEntry>()),
                Warnings = new List<string>(Warnings ?? new List<string>()),
                // fable_78: campos aditivos — ausência (save antigo) cai em defaults seguros.
                EnvironmentElements = CloneEnvironmentElements(EnvironmentElements),
                HasWater = HasWater,
                ConflictState = CloneConflictState(ConflictState)
            };

            if ((snapshot.LegacyEnemySpawnPlanEntries == null || snapshot.LegacyEnemySpawnPlanEntries.Count == 0)
                && EnemySpawnPlan == null
                && LegacyEnemySpawnPlanEntries == null)
            {
                snapshot.LegacyEnemySpawnPlanEntries = new List<EnemySpawnPlanEntry>();
            }

            return snapshot;
        }
    }
}
