using System;
using System.Collections.Generic;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// F13 — seção de save da run da caverna (CAVE_RUN_SAVE_LOAD_DEBT). Espelho 1:1 do
    /// CaveRuntimeState em tipos simples. Sem refs Unity.
    ///
    /// fable_44 — persistência MULTI-NÍVEL: <see cref="VisitedLevelSnapshots"/> guarda TODOS os
    /// níveis visitados da run (dentro de um orçamento — ver <see cref="MaxPersistedLevelSnapshots"/>),
    /// não só o corrente. <see cref="CurrentLevelSnapshot"/> é MANTIDO e escrito em paralelo para
    /// compatibilidade de load legado (save F13): builds/saves antigos sem a lista caem no
    /// comportamento de um nível sem erro.
    /// </summary>
    [Serializable]
    public class CaveRunSaveData
    {
        // fable_44 — orçamento de tamanho da seção da caverna (CA-2). A política adotada é CAP LRU
        // (sem compressão de tiles): apenas os N níveis mais recentes são persistidos; níveis fora do
        // cap voltam a replay puro de seed (layout idêntico via ADR-0005; só o estado mutável —
        // HP/depleção/baús/armadilhas — desses níveis antigos é descartado). A medição da Fase 0
        // (ver execution report) mostrou ~148 KB por nível 65×65; N=8 limita o pior caso realista.
        // Decisão de NÃO implementar codec de compressão registrada no report (cap basta; codec
        // arriscaria o replay hash — ver Riscos técnicos da spec).
        public const int MaxPersistedLevelSnapshots = 8;

        // Orçamento explícito da seção da caverna sob o cap (bytes do JSON JsonUtility indentado).
        // Pior caso analítico sob o cap (8 × 65×65) ≈ 1.18 MB; o limite documentado deixa folga.
        // O SaveSizeBudgetTest serializa uma run sintética no cap e assert < este valor.
        public const long SaveSizeBudgetBytes = 1_572_864L; // 1.5 MB

        public bool HasActiveRun;
        public string WorldSeed = string.Empty;
        public string RunSeed = string.Empty;
        public int CurrentLevel = 1;
        public int DeepestLevel = 1;
        public List<int> UnlockedCheckpoints = new List<int>();
        public List<string> DepletedNodeIds = new List<string>();
        public List<CaveBossDefeatSaveRecord> BossDefeats = new List<CaveBossDefeatSaveRecord>();
        // F13 (legado): snapshot APENAS do nível corrente. fable_44 mantém este campo escrito em
        // paralelo para que saves desta versão continuem carregando em builds F13.
        public VisitedLevelSnapshot CurrentLevelSnapshot;
        // fable_44: lista ADITIVA de TODOS os níveis visitados persistidos (respeitando o cap LRU),
        // ordenada de forma estável por CaveLevel ascendente. Load legado: lista vazia/null → cai no
        // comportamento F13 (só CurrentLevelSnapshot). Sem refs Unity (VisitedLevelSnapshot é simples).
        public List<VisitedLevelSnapshot> VisitedLevelSnapshots = new List<VisitedLevelSnapshot>();
        public string GenerationVersion = "fable_44"; // diagnóstico, não gate
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

            // fable_44: persiste TODOS os níveis visitados válidos, aplicando o cap LRU determinístico,
            // ordenados de forma estável por CaveLevel ascendente. CurrentLevelSnapshot continua escrito
            // em paralelo (compat F13).
            var persisted = SelectSnapshotsWithinCap(state, data.CurrentLevel);
            data.VisitedLevelSnapshots = persisted;

            foreach (var snapshot in persisted)
            {
                if (snapshot.CaveLevel == data.CurrentLevel)
                {
                    data.CurrentLevelSnapshot = snapshot;
                    break;
                }
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

            // fable_44: restaura o dicionário completo a partir da lista multi-nível.
            // Compat F13: se a lista estiver vazia/null (save antigo), cai no CurrentLevelSnapshot.
            var restoredAny = false;
            if (data.VisitedLevelSnapshots != null)
            {
                foreach (var snapshot in data.VisitedLevelSnapshots)
                {
                    if (snapshot != null && snapshot.IsValid())
                    {
                        state.VisitedLevelSnapshots[snapshot.CaveLevel] = snapshot;
                        restoredAny = true;
                    }
                }
            }

            if (!restoredAny && data.CurrentLevelSnapshot != null && data.CurrentLevelSnapshot.IsValid())
            {
                state.VisitedLevelSnapshots[data.CurrentLevelSnapshot.CaveLevel] = data.CurrentLevelSnapshot;
            }

            return state;
        }

        /// <summary>
        /// fable_44 — seleção determinística dos níveis a persistir sob o cap LRU.
        /// Recência: como o runtime não rastreia ordem de visita, a proximidade ao nível CORRENTE é
        /// o proxy de recência (descida normal = níveis mais fundos são mais recentes; ida e volta
        /// mantém os adjacentes ao corrente como os mais "quentes"). O nível corrente é SEMPRE incluído
        /// se válido. Saída ordenada de forma estável por CaveLevel ascendente (determinismo de save).
        /// Níveis fora do cap voltam a replay puro de seed (layout idêntico — ADR-0005).
        /// </summary>
        private static List<VisitedLevelSnapshot> SelectSnapshotsWithinCap(CaveRuntimeState state, int currentLevel)
        {
            var valid = new List<VisitedLevelSnapshot>();
            foreach (var kvp in state.VisitedLevelSnapshots)
            {
                if (kvp.Value != null && kvp.Value.IsValid())
                {
                    valid.Add(kvp.Value);
                }
            }

            // Ordena por recência (proxy): distância ao nível corrente crescente; empate → mais fundo
            // primeiro; empate final → CaveLevel para estabilidade total.
            valid.Sort((a, b) =>
            {
                var da = Math.Abs(a.CaveLevel - currentLevel);
                var db = Math.Abs(b.CaveLevel - currentLevel);
                if (da != db)
                {
                    return da.CompareTo(db);
                }

                if (a.CaveLevel != b.CaveLevel)
                {
                    return b.CaveLevel.CompareTo(a.CaveLevel); // mais fundo primeiro no empate de distância
                }

                return 0;
            });

            var cap = Math.Max(1, CaveRunSaveData.MaxPersistedLevelSnapshots);
            if (valid.Count > cap)
            {
                valid.RemoveRange(cap, valid.Count - cap);
            }

            // Ordem de saída estável por CaveLevel ascendente (independe da ordem de iteração do Dictionary).
            valid.Sort((a, b) => a.CaveLevel.CompareTo(b.CaveLevel));
            return valid;
        }
    }
}
