using System;
using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using UnityEngine;

namespace CindarsHope.Save
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
    }

    [Serializable]
    public sealed class SerializedVisitedLevelSnapshot
    {
        public int CaveLevel;
        public string SnapshotId;
        public string BiomeId;
        public string LayoutHash;
        public Vector2 EntrancePosition;
        public Vector2 ExitPosition;
        public List<SerializedEnemySpawn> EnemySpawns = new List<SerializedEnemySpawn>();
        public List<SerializedResourceNode> ResourceNodes = new List<SerializedResourceNode>();
        public List<string> DepletedResourceNodeIds = new List<string>();

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
                BiomeId = snapshot.BiomeId,
                LayoutHash = snapshot.LayoutHash,
                EntrancePosition = snapshot.EntrancePosition,
                ExitPosition = snapshot.ExitPosition,
                EnemySpawns = new List<SerializedEnemySpawn>(snapshot.EnemySpawns),
                ResourceNodes = new List<SerializedResourceNode>(snapshot.ResourceNodes),
                DepletedResourceNodeIds = new List<string>(snapshot.DepletedResourceNodeIds)
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
                EntrancePosition = EntrancePosition,
                ExitPosition = ExitPosition,
                EnemySpawns = new List<SerializedEnemySpawn>(EnemySpawns),
                ResourceNodes = new List<SerializedResourceNode>(ResourceNodes),
                DepletedResourceNodeIds = new List<string>(DepletedResourceNodeIds)
            };

            return snapshot;
        }
    }
}
