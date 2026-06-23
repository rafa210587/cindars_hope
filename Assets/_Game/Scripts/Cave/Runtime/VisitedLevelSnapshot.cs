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
        // fable_38: células reveladas do fog-of-war do minimapa NESTE nível/run (aditivo; estado
        // mutável — FORA do LayoutHash, mesmo padrão de OpenedChestIds/TrapStates). O fog vive
        // EXCLUSIVAMENTE aqui (estado de nível do stable-run, ADR-0005): revisitar o nível na MESMA
        // run mantém o revelado; nova run (troca de CaveRunSeed) gera snapshot novo = fog zerado.
        // NÃO é save global. Snapshot legado sem o campo = caverna nasce escura (lista vazia).
        [SerializeField] public List<Vector2Int> RevealedCells = new();
        // fable_78 (14.8/16.4): elementos ambientais materializados NESTE nível/run (aditivo; FORA do
        // LayoutHash). Posições/tipos são DETERMINÍSTICOS (re-deriváveis pelo CaveEnvironmentElementPlanner),
        // mas o estado depletado de mineráveis é mutável — por isso persistimos a lista junto do snapshot,
        // mesmo padrão de ResourceNodeStates. Snapshot legado sem o campo = lista vazia → regeneração
        // determinística do plano de elementos na próxima materialização.
        [SerializeField] public List<SerializedEnvironmentElement> EnvironmentElements = new();
        // fable_78 (14.3/14.8): este nível tem tile(s) de água (lago)? Habilita criaturas aquáticas.
        // Aditivo; snapshot legado sem o campo = false (re-derivado pelo perfil do bioma na materialização).
        [SerializeField] public bool HasWater;
        // fable_78 (14.5/14.8/16.4): estado de conflito inter-monstro do nível. Conflito é COMPORTAMENTO
        // por visita (ADR-0018), NÃO composição: ConflictActive/FactionAId/FactionBId refletem a visita
        // atual e podem mudar entre entradas; HasHadConflict e EntryCount são o estado persistido que
        // governa a queda 5%->0,5% por entrada. Aditivo; snapshot legado sem o campo = conflito inativo,
        // EntryCount=0, HasHadConflict=false (defaults seguros).
        [SerializeField] public CaveConflictSnapshot ConflictState = new();

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

        // fable_38: revela um lote de células do fog (idempotente). Retorna quantas células NOVAS
        // foram adicionadas. Estado mutável FORA do LayoutHash; persiste intra-run no snapshot
        // (revisita mantém revelado). Reset ocorre naturalmente: nova run = snapshot novo.
        public int RevealCells(IEnumerable<Vector2Int> cells)
        {
            if (cells == null)
            {
                return 0;
            }

            var existing = new HashSet<Vector2Int>(RevealedCells);
            int added = 0;
            foreach (var cell in cells)
            {
                if (existing.Add(cell))
                {
                    RevealedCells.Add(cell);
                    added++;
                }
            }

            return added;
        }

        // fable_38: consulta se uma célula já foi revelada nesta run/nível (fog-of-war).
        public bool IsCellRevealed(Vector2Int cell)
        {
            return RevealedCells.Contains(cell);
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

        // fable_78: grava o conjunto de elementos ambientais do nível (idempotente por ElementId).
        // Sobrescreve a lista corrente — o plano é determinístico; só o estado depletado varia.
        public void SetEnvironmentElements(IEnumerable<SerializedEnvironmentElement> elements)
        {
            EnvironmentElements.Clear();
            if (elements == null)
            {
                return;
            }

            foreach (var element in elements)
            {
                if (element != null && !string.IsNullOrWhiteSpace(element.ElementId))
                {
                    EnvironmentElements.Add(element);
                }
            }
        }

        // fable_78: marca um elemento minerável como depletado (idempotente). Estado mutável fora do
        // LayoutHash — mesmo padrão de MarkResourceNodeDepleted. A depleção real do ResourceNode é
        // idempotente via CaveLootSnapshotService; este flag espelha o estado para a revisita.
        public void MarkEnvironmentElementDepleted(string elementId)
        {
            if (string.IsNullOrWhiteSpace(elementId))
            {
                return;
            }

            foreach (var element in EnvironmentElements)
            {
                if (element != null && element.ElementId == elementId)
                {
                    element.IsDepleted = true;
                }
            }
        }

        // fable_78: garante uma instância não-nula de ConflictState (back-compat de save antigo).
        public CaveConflictSnapshot GetOrCreateConflictState()
        {
            return ConflictState ??= new CaveConflictSnapshot();
        }

        // fable_78: registra mais uma entrada no nível e (opcionalmente) o conflito desta visita.
        // EntryCount cresce a cada entrada; HasHadConflict trava em true na primeira vez que um
        // conflito é rolado (gatilho da queda 5%->0,5%). ConflictActive/Faction* refletem a visita atual.
        public void RecordConflictEntry(bool conflictActive, string factionAId, string factionBId)
        {
            var state = GetOrCreateConflictState();
            state.EntryCount += 1;
            state.ConflictActive = conflictActive;
            state.FactionAId = factionAId ?? string.Empty;
            state.FactionBId = factionBId ?? string.Empty;
            if (conflictActive)
            {
                state.HasHadConflict = true;
            }
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

    // fable_78: elemento ambiental serializável (tipos simples + IDs apenas — sem refs Unity).
    // Kind espelha CindarsHope.Cave.Ecosystem.CaveEnvironmentElementKind (int):
    // 0=DecorNonBlocking, 1=DecorBlocking, 2=WaterTile, 3=MineableNode. Posição em grid (GridX/GridY)
    // para não depender de Vector2Int no JsonUtility de seções legadas. IsDepleted só vale p/ mineráveis.
    [Serializable]
    public sealed class SerializedEnvironmentElement
    {
        public string ElementId = string.Empty;
        public int Kind;
        public int GridX;
        public int GridY;
        public bool IsMineable;
        public string MineNodeDataId = string.Empty;
        public bool IsDepleted;
    }

    // fable_78: estado serializável do conflito inter-monstro do nível (tipos simples — sem refs Unity).
    // EntryCount/HasHadConflict são o estado persistido que governa a chance por entrada (14.5);
    // ConflictActive/FactionAId/FactionBId refletem a VISITA atual (comportamento, não composição).
    [Serializable]
    public sealed class CaveConflictSnapshot
    {
        public bool ConflictActive;
        public string FactionAId = string.Empty;
        public string FactionBId = string.Empty;
        public bool HasHadConflict;
        public int EntryCount;
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
