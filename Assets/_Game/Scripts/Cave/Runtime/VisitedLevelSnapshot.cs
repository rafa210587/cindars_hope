using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [Serializable]
    public class VisitedLevelSnapshot : IVisitedLevelSnapshot
    {
        [SerializeField] public int CaveLevel;
        [SerializeField] public string SnapshotId;
        [SerializeField] public string CaveWorldSeed;
        [SerializeField] public string CaveRunSeed;
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
        [SerializeField] public List<CaveResourceNodeSnapshotEntry> ResourceNodeStates = new();
        [SerializeField] public List<string> DepletedResourceNodeIds = new();
        [SerializeField] public CaveFishingSpotSnapshotEntry FishingSpotState = new();
        [SerializeField] public CaveEnemySpawnPlan EnemySpawnPlan = new();
        [SerializeField] public List<EnemySpawnPlanEntry> LegacyEnemySpawnPlanEntries = new();
        [SerializeField] public List<string> Warnings = new();
        [SerializeField] public SerializedEnemyRedistributionState RedistributionState = new();
        [SerializeField] public SerializedEnemyRespawnState RespawnState = new();
        // F13: HP por instância (aditivo; estado mutável — fica FORA do LayoutHash).
        [SerializeField] public List<EnemyHpRecord> EnemyHpRecords = new();
        // fable_09: baús de tesouro JÁ ABERTOS neste nível/run (aditivo; estado mutável — FORA do
        // LayoutHash, mesmo padrão de DepletedResourceNodeIds/EnemyHpRecords). Revisita mostra aberto.
        [SerializeField] public List<string> OpenedChestIds = new();
        // fable_60: estado das armadilhas geradas neste nível/run (aditivo; estado mutável — FORA do
        // LayoutHash, mesmo padrão de OpenedChestIds). Triggered/Disarmed não rearmam na revisita
        // (cave-stable-run / ADR-0005). Snapshot legado sem o campo = armadilhas re-derivadas Armed
        // (o plano determinístico garante composição/posições/IDs idênticos).
        [SerializeField] public List<CaveTrapSnapshotEntry> TrapStates = new();

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
            SnapshotId = BuildSnapshotId(string.Empty, caveLevel);
        }

        public VisitedLevelSnapshot(int caveLevel, string biomeId, string layoutHash, string caveWorldSeed, string caveRunSeed)
        {
            CaveLevel = caveLevel;
            BiomeId = biomeId;
            LayoutHash = layoutHash;
            CaveWorldSeed = caveWorldSeed ?? string.Empty;
            CaveRunSeed = caveRunSeed ?? string.Empty;
            SnapshotId = BuildSnapshotId(CaveRunSeed, caveLevel);
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

            ResourceNodeStates.Add(new CaveResourceNodeSnapshotEntry
            {
                NodeInstanceId = nodeId ?? string.Empty,
                ResourceNodeId = resourceDataId ?? string.Empty,
                GridPosition = Vector2Int.RoundToInt(position),
                IsDepleted = DepletedResourceNodeIds.Contains(nodeId)
            });
        }

        public void MarkResourceNodeDepleted(string nodeInstanceId)
        {
            if (!string.IsNullOrWhiteSpace(nodeInstanceId) && !DepletedResourceNodeIds.Contains(nodeInstanceId))
            {
                DepletedResourceNodeIds.Add(nodeInstanceId);
            }

            foreach (var resourceNodeState in ResourceNodeStates)
            {
                if (resourceNodeState != null && resourceNodeState.NodeInstanceId == nodeInstanceId)
                {
                    resourceNodeState.IsDepleted = true;
                }
            }
        }

        // fable_09: marca um baú de tesouro como aberto (idempotente). Estado mutável fora do LayoutHash.
        public void MarkChestOpened(string chestId)
        {
            if (!string.IsNullOrWhiteSpace(chestId) && !OpenedChestIds.Contains(chestId))
            {
                OpenedChestIds.Add(chestId);
            }
        }

        // fable_09: consulta se um baú já foi aberto nesta run (cave-stable-run).
        public bool IsChestOpened(string chestId)
        {
            return !string.IsNullOrWhiteSpace(chestId) && OpenedChestIds.Contains(chestId);
        }

        // fable_60: grava/atualiza o estado de UMA armadilha (idempotente por trapInstanceId). Estado
        // mutável FORA do LayoutHash. NÃO regride o estado: uma armadilha Triggered/Disarmed nunca volta
        // a Armed dentro do mesmo CaveRunSeed (cave-stable-run / ADR-0005).
        public void SetTrapState(string trapInstanceId, string trapKey, Vector2Int cell, int state)
        {
            if (string.IsNullOrWhiteSpace(trapInstanceId))
            {
                return;
            }

            foreach (var entry in TrapStates)
            {
                if (entry != null && entry.TrapInstanceId == trapInstanceId)
                {
                    // Só avança o estado (Armed=0 < Telegraphing=1 < Triggered=2 / Disarmed=3).
                    if (state > entry.State)
                    {
                        entry.State = state;
                    }

                    return;
                }
            }

            TrapStates.Add(new CaveTrapSnapshotEntry
            {
                TrapInstanceId = trapInstanceId,
                TrapKey = trapKey ?? string.Empty,
                Cell = cell,
                State = state
            });
        }

        // fable_60: estado persistido de uma armadilha (Armed por padrão se ausente). Usado na revisita.
        public int GetTrapState(string trapInstanceId)
        {
            if (string.IsNullOrWhiteSpace(trapInstanceId))
            {
                return 0;
            }

            foreach (var entry in TrapStates)
            {
                if (entry != null && entry.TrapInstanceId == trapInstanceId)
                {
                    return entry.State;
                }
            }

            return 0; // Armed
        }

        public void SetEnemySpawnPlan(CaveLevelEnemyPlan plan)
        {
            LegacyEnemySpawnPlanEntries.Clear();
            if (plan != null && plan.EnemyPlans.Count > 0)
            {
                LegacyEnemySpawnPlanEntries.AddRange(plan.EnemyPlans);
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

        public void SetEnemySpawnPlan(CaveEnemySpawnPlan plan)
        {
            EnemySpawnPlan = plan ?? new CaveEnemySpawnPlan();
            LegacyEnemySpawnPlanEntries.Clear();

            if (EnemySpawnPlan.Entries == null)
            {
                return;
            }

            foreach (var entry in EnemySpawnPlan.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EnemyId))
                {
                    continue;
                }

                LegacyEnemySpawnPlanEntries.Add(new EnemySpawnPlanEntry(
                    entry.EnemyInstanceId,
                    entry.EnemyId,
                    !string.IsNullOrWhiteSpace(entry.RoomId)
                        ? $"{entry.RoomId}_{entry.GridPosition.x}_{entry.GridPosition.y}"
                        : $"anchor_{entry.SpawnIndex}")
                {
                    IsBoss = entry.SizeClass == "Boss"
                });
            }
        }

        public CaveLevelEnemyPlan RestoreEnemySpawnPlan()
        {
            var plan = new CaveLevelEnemyPlan();
            if (LegacyEnemySpawnPlanEntries.Count > 0)
            {
                plan.EnemyPlans.AddRange(LegacyEnemySpawnPlanEntries);
            }
            else if (EnemySpawnPlan?.Entries != null)
            {
                foreach (var entry in EnemySpawnPlan.Entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.EnemyId))
                    {
                        continue;
                    }

                    plan.EnemyPlans.Add(new EnemySpawnPlanEntry(
                        entry.EnemyInstanceId,
                        entry.EnemyId,
                        !string.IsNullOrWhiteSpace(entry.RoomId)
                            ? $"{entry.RoomId}_{entry.GridPosition.x}_{entry.GridPosition.y}"
                            : $"anchor_{entry.SpawnIndex}")
                    {
                        IsBoss = entry.SizeClass == "Boss"
                    });
                }
            }

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

        public void SetEnemyHpRecords(IEnumerable<EnemyHpRecord> records)
        {
            EnemyHpRecords.Clear();
            if (records == null)
            {
                return;
            }

            foreach (var record in records)
            {
                if (record != null && !string.IsNullOrWhiteSpace(record.EnemyInstanceId))
                {
                    EnemyHpRecords.Add(record);
                }
            }
        }

        public void SetFishingSpot(CaveFishingSpotSnapshotEntry fishingSpot)
        {
            FishingSpotState = fishingSpot ?? new CaveFishingSpotSnapshotEntry();
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

        public static string BuildSnapshotId(string caveRunSeed, int caveLevel)
        {
            var seed = string.IsNullOrWhiteSpace(caveRunSeed) ? "run_unknown" : caveRunSeed;
            var safeSeed = new string(seed.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
            return $"snapshot_{safeSeed}_{Mathf.Max(1, caveLevel)}";
        }
    }

    [Serializable]
    public sealed class CaveLevelSnapshot : VisitedLevelSnapshot
    {
        public CaveLevelSnapshot()
        {
        }

        public CaveLevelSnapshot(int caveLevel, string biomeId, string layoutHash, string caveWorldSeed, string caveRunSeed)
            : base(caveLevel, biomeId, layoutHash, caveWorldSeed, caveRunSeed)
        {
        }
    }

    // F13: registro de HP por instância de inimigo (0 = morto, não rematerializa na run).
    [Serializable]
    public sealed class EnemyHpRecord
    {
        public string EnemyInstanceId = string.Empty;
        public int CurrentHp;
    }

    [Serializable]
    public sealed class CaveResourceNodeSnapshotEntry
    {
        public string NodeInstanceId = string.Empty;
        public string ResourceNodeId = string.Empty;
        public Vector2Int GridPosition;
        public bool IsDepleted;
    }

    // fable_60: estado serializável de UMA armadilha (tipos simples + IDs apenas — sem refs Unity).
    // State: 0=Armed, 1=Telegraphing, 2=Triggered, 3=Disarmed (espelha CindarsHope.Cave.Traps.TrapState).
    [Serializable]
    public sealed class CaveTrapSnapshotEntry
    {
        public string TrapInstanceId = string.Empty;
        public string TrapKey = string.Empty;
        public Vector2Int Cell;
        public int State;
    }

    [Serializable]
    public sealed class CaveFishingSpotSnapshotEntry
    {
        public bool HasFishingSpot;
        public string FishingSpotId = string.Empty;
        public Vector2Int GridPosition;
        public string FishingProfileId = string.Empty;
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
