using System;
using System.Collections.Generic;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// F13 — seção de save da run da caverna (CAVE_RUN_SAVE_LOAD_DEBT). Espelho 1:1 do
    /// CaveRuntimeState em tipos simples. Apenas o snapshot do NÍVEL CORRENTE é persistido
    /// (snapshot multi-nível: 16_spec futura). Sem refs Unity.
    /// </summary>
    [Serializable]
    public class CaveRunSaveData
    {
        public bool HasActiveRun;
        public string WorldSeed = string.Empty;
        public string RunSeed = string.Empty;
        public int CurrentLevel = 1;
        public int DeepestLevel = 1;
        public List<int> UnlockedCheckpoints = new List<int>();
        public List<string> DepletedNodeIds = new List<string>();
        public List<CaveBossDefeatSaveRecord> BossDefeats = new List<CaveBossDefeatSaveRecord>();
        public VisitedLevelSnapshot CurrentLevelSnapshot;
        public string GenerationVersion = "fable_13"; // diagnóstico, não gate
    }

    [Serializable]
    public class CaveBossDefeatSaveRecord
    {
        public string BossGateId = string.Empty;
        public int CaveLevel;
        public bool IsDefeated;
        public string DefeatedAt = string.Empty;
        public List<string> UniqueRewardsClaimed = new List<string>();
    }

    /// <summary>Mapper puro testável CaveRuntimeState ↔ CaveRunSaveData.</summary>
    public static class CaveRunSaveMapper
    {
        public static CaveRunSaveData ToSaveData(CaveRuntimeState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.CaveRunSeed))
            {
                return new CaveRunSaveData { HasActiveRun = false };
            }

            var data = new CaveRunSaveData
            {
                HasActiveRun = true,
                WorldSeed = state.CaveWorldSeed ?? string.Empty,
                RunSeed = state.CaveRunSeed ?? string.Empty,
                CurrentLevel = Math.Max(1, state.CurrentCaveLevel),
                DeepestLevel = Math.Max(1, state.DeepestLayerReached)
            };

            foreach (var checkpoint in state.UnlockedCheckpoints)
            {
                data.UnlockedCheckpoints.Add(checkpoint);
            }

            data.UnlockedCheckpoints.Sort();

            foreach (var nodeId in state.DepletedNodeIds)
            {
                if (!string.IsNullOrWhiteSpace(nodeId))
                {
                    data.DepletedNodeIds.Add(nodeId);
                }
            }

            data.DepletedNodeIds.Sort(StringComparer.Ordinal);

            foreach (var kvp in state.BossDefeatStates)
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                data.BossDefeats.Add(new CaveBossDefeatSaveRecord
                {
                    BossGateId = kvp.Value.BossGateId ?? kvp.Key,
                    CaveLevel = kvp.Value.CaveLevel,
                    IsDefeated = kvp.Value.IsDefeated,
                    DefeatedAt = kvp.Value.DefeatedAt ?? string.Empty,
                    UniqueRewardsClaimed = new List<string>(kvp.Value.UniqueRewardsClaimed ?? new List<string>())
                });
            }

            data.BossDefeats.Sort((a, b) => string.CompareOrdinal(a.BossGateId, b.BossGateId));

            if (state.VisitedLevelSnapshots.TryGetValue(data.CurrentLevel, out var currentSnapshot)
                && currentSnapshot != null
                && currentSnapshot.IsValid())
            {
                data.CurrentLevelSnapshot = currentSnapshot;
            }

            return data;
        }

        public static CaveRuntimeState FromSaveData(CaveRunSaveData data)
        {
            if (data == null || !data.HasActiveRun || string.IsNullOrWhiteSpace(data.RunSeed))
            {
                return null;
            }

            var state = new CaveRuntimeState
            {
                CaveWorldSeed = data.WorldSeed ?? string.Empty,
                CaveRunSeed = data.RunSeed,
                CurrentCaveLevel = Math.Max(1, data.CurrentLevel),
                DeepestLayerReached = Math.Max(1, data.DeepestLevel)
            };

            foreach (var checkpoint in data.UnlockedCheckpoints ?? new List<int>())
            {
                state.UnlockedCheckpoints.Add(checkpoint);
            }

            foreach (var nodeId in data.DepletedNodeIds ?? new List<string>())
            {
                if (!string.IsNullOrWhiteSpace(nodeId))
                {
                    state.DepletedNodeIds.Add(nodeId);
                }
            }

            foreach (var record in data.BossDefeats ?? new List<CaveBossDefeatSaveRecord>())
            {
                if (record == null || string.IsNullOrWhiteSpace(record.BossGateId))
                {
                    continue;
                }

                state.BossDefeatStates[record.BossGateId] = new CaveBossDefeatState
                {
                    BossGateId = record.BossGateId,
                    CaveLevel = record.CaveLevel,
                    IsDefeated = record.IsDefeated,
                    DefeatedAt = record.DefeatedAt ?? string.Empty,
                    UniqueRewardsClaimed = new List<string>(record.UniqueRewardsClaimed ?? new List<string>())
                };
            }

            if (data.CurrentLevelSnapshot != null && data.CurrentLevelSnapshot.IsValid())
            {
                state.VisitedLevelSnapshots[data.CurrentLevelSnapshot.CaveLevel] = data.CurrentLevelSnapshot;
            }

            return state;
        }
    }
}
