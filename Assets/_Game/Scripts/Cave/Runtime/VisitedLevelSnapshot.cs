using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [Serializable]
    public sealed class VisitedLevelSnapshot : IVisitedLevelSnapshot
    {
        [SerializeField] public int CaveLevel;
        [SerializeField] public string SnapshotId;
        [SerializeField] public string BiomeId;
        [SerializeField] public string LayoutHash;
        [SerializeField] public Vector2 EntrancePosition;
        [SerializeField] public Vector2 ExitPosition;
        [SerializeField] public List<SerializedEnemySpawn> EnemySpawns = new();
        [SerializeField] public List<SerializedResourceNode> ResourceNodes = new();
        [SerializeField] public List<string> DepletedResourceNodeIds = new();

        int IVisitedLevelSnapshot.CaveLevel => CaveLevel;
        string IVisitedLevelSnapshot.SnapshotId => SnapshotId;

        public VisitedLevelSnapshot()
        {
        }

        public VisitedLevelSnapshot(int caveLevel, string biomeId, string layoutHash)
        {
            CaveLevel = caveLevel;
            BiomeId = biomeId;
            LayoutHash = layoutHash;
            SnapshotId = $"snapshot_{caveLevel}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}";
        }

        public bool IsValid()
        {
            return CaveLevel > 0 && !string.IsNullOrWhiteSpace(SnapshotId) && !string.IsNullOrWhiteSpace(BiomeId);
        }

        public void SetEntranceAndExit(Vector2 entrance, Vector2 exit)
        {
            EntrancePosition = entrance;
            ExitPosition = exit;
        }

        public void AddEnemySpawn(string enemyId, Vector2 position, int level)
        {
            EnemySpawns.Add(new SerializedEnemySpawn
            {
                EnemyId = enemyId,
                Position = position,
                Level = level
            });
        }

        public void AddResourceNode(string nodeId, Vector2 position, string resourceDataId)
        {
            ResourceNodes.Add(new SerializedResourceNode
            {
                NodeInstanceId = nodeId,
                Position = position,
                ResourceDataId = resourceDataId
            });
        }

        public void MarkResourceNodeDepleted(string nodeInstanceId)
        {
            if (!string.IsNullOrWhiteSpace(nodeInstanceId) && !DepletedResourceNodeIds.Contains(nodeInstanceId))
            {
                DepletedResourceNodeIds.Add(nodeInstanceId);
            }
        }
    }

    [Serializable]
    public sealed class SerializedEnemySpawn
    {
        public string EnemyId;
        public Vector2 Position;
        public int Level;
    }

    [Serializable]
    public sealed class SerializedResourceNode
    {
        public string NodeInstanceId;
        public Vector2 Position;
        public string ResourceDataId;
    }
}
