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
        [SerializeField] public int Width;
        [SerializeField] public int Height;
        [SerializeField] public List<Vector2Int> WalkableTilesList = new();
        [SerializeField] public List<Vector2Int> WallTilesList = new();
        [SerializeField] public List<SerializedCaveGenerationPoint> EnemySpawnPointsList = new();
        [SerializeField] public List<SerializedCaveGenerationPoint> ResourceSpawnPointsList = new();
        [SerializeField] public List<SerializedEnemySpawn> EnemySpawns = new();
        [SerializeField] public List<SerializedResourceNode> ResourceNodes = new();
        [SerializeField] public List<string> DepletedResourceNodeIds = new();
        [SerializeField] public List<EnemySpawnPlanEntry> EnemySpawnPlan = new();
        [SerializeField] public SerializedEnemyRedistributionState RedistributionState = new();
        [SerializeField] public SerializedEnemyRespawnState RespawnState = new();

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
            return CaveLevel > 0
                && !string.IsNullOrWhiteSpace(SnapshotId)
                && !string.IsNullOrWhiteSpace(BiomeId)
                && Width > 0
                && Height > 0
                && WalkableTilesList.Count > 0;
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

        public void SetEnemySpawnPlan(CaveLevelEnemyPlan plan)
        {
            EnemySpawnPlan.Clear();
            if (plan != null && plan.EnemyPlans.Count > 0)
            {
                EnemySpawnPlan.AddRange(plan.EnemyPlans);
            }

            if (plan?.RedistributionState != null)
            {
                RedistributionState = new SerializedEnemyRedistributionState
                {
                    RedistributionCount = plan.RedistributionState.RedistributionCount,
                    LastRedistributionReason = plan.RedistributionState.LastRedistributionReason,
                    RedistributionSeedOffset = plan.RedistributionState.RedistributionSeedOffset
                };
            }

            if (plan?.RespawnState != null)
            {
                RespawnState = new SerializedEnemyRespawnState
                {
                    RespawnDelayGameDays = plan.RespawnState.RespawnDelayGameDays,
                    LastRespawnEvaluationDay = plan.RespawnState.LastRespawnEvaluationDay
                };
            }
        }

        public CaveLevelEnemyPlan RestoreEnemySpawnPlan()
        {
            var plan = new CaveLevelEnemyPlan();
            plan.EnemyPlans.AddRange(EnemySpawnPlan);

            if (RedistributionState != null)
            {
                plan.RedistributionState = new EnemyRedistributionState
                {
                    RedistributionCount = RedistributionState.RedistributionCount,
                    LastRedistributionReason = RedistributionState.LastRedistributionReason,
                    RedistributionSeedOffset = RedistributionState.RedistributionSeedOffset
                };
            }

            if (RespawnState != null)
            {
                plan.RespawnState = new EnemyRespawnState
                {
                    RespawnDelayGameDays = RespawnState.RespawnDelayGameDays,
                    LastRespawnEvaluationDay = RespawnState.LastRespawnEvaluationDay
                };
            }

            return plan;
        }

        public void SetLayoutDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void AddWalkableTile(Vector2Int position)
        {
            if (!WalkableTilesList.Contains(position))
            {
                WalkableTilesList.Add(position);
            }
        }

        public void AddWallTile(Vector2Int position)
        {
            if (!WallTilesList.Contains(position))
            {
                WallTilesList.Add(position);
            }
        }

        public void AddEnemySpawnPoint(int pointTypeValue, Vector2Int position)
        {
            EnemySpawnPointsList.Add(new SerializedCaveGenerationPoint
            {
                PointTypeValue = pointTypeValue,
                Position = position
            });
        }

        public void AddResourceSpawnPoint(int pointTypeValue, Vector2Int position)
        {
            ResourceSpawnPointsList.Add(new SerializedCaveGenerationPoint
            {
                PointTypeValue = pointTypeValue,
                Position = position
            });
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

    [Serializable]
    public sealed class SerializedCaveGenerationPoint
    {
        public int PointTypeValue;
        public Vector2Int Position;
    }

    [Serializable]
    public sealed class SerializedEnemyRedistributionState
    {
        public int RedistributionCount;
        public string LastRedistributionReason;
        public int RedistributionSeedOffset;
    }

    [Serializable]
    public sealed class SerializedEnemyRespawnState
    {
        public int RespawnDelayGameDays = 2;
        public int LastRespawnEvaluationDay = -1;
    }
}
