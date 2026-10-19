using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_44 — persistência multi-nível da run da caverna + cap LRU determinístico + orçamento de
    /// tamanho + load legado (só CurrentLevelSnapshot) + política de save em boss fight (flag/gate puro).
    /// Estende a cobertura da F13 (SaveDebtClosureTests) sem deletá-la.
    /// </summary>
    public class CaveMultiLevelSaveTests
    {
        // ------------------------------------------------------------------ helpers

        private static VisitedLevelSnapshot BuildSnapshot(int level, int tileCount = 4)
        {
            var snapshot = new CaveLevelSnapshot(level, "biome_fungal", "hash_" + level, "world_abc", "run_xyz");
            snapshot.SetLayoutDimensions(Mathf.Max(2, tileCount), Mathf.Max(2, tileCount));
            for (int i = 0; i < tileCount; i++)
            {
                snapshot.AddWalkableTile(new Vector2Int(i, level));
            }

            return snapshot;
        }

        private static CaveRuntimeState BuildRunWithLevels(int currentLevel, params int[] levels)
        {
            var state = new CaveRuntimeState
            {
                CaveWorldSeed = "world_abc",
                CaveRunSeed = "run_xyz",
                CurrentCaveLevel = currentLevel,
                DeepestLayerReached = currentLevel
            };

            foreach (var level in levels)
            {
                state.DeepestLayerReached = Mathf.Max(state.DeepestLayerReached, level);
                state.VisitedLevelSnapshots[level] = BuildSnapshot(level);
            }

            return state;
        }

        // ------------------------------------------------------------------ CA-1 multi-nível

        [Test]
        public void Mapper_RoundTrip_PersistsAllVisitedLevels()
        {
            var state = BuildRunWithLevels(3, 1, 2, 3);

            // Estado mutável distinto por nível: inimigo morto no 1, nó depletado no 2, baú aberto no 3.
            state.VisitedLevelSnapshots[1].SetEnemyHpRecords(new[]
            {
                new EnemyHpRecord { EnemyInstanceId = "enemy_1_a", CurrentHp = 0 }
            });
            state.VisitedLevelSnapshots[2].MarkResourceNodeDepleted("node_2_a");
            state.VisitedLevelSnapshots[2].DepletedResourceNodeIds.Add("node_2_a");
            state.VisitedLevelSnapshots[3].MarkChestOpened("chest_3_a");
            state.VisitedLevelSnapshots[3].SetTrapState("trap_3_a", "spike", new Vector2Int(2, 3), 2);

            var data = CaveRunSaveMapper.ToSaveData(state);
            Assert.AreEqual(3, data.VisitedLevelSnapshots.Count, "Todos os 3 níveis visitados persistidos.");

            var restored = CaveRunSaveMapper.FromSaveData(data);
            Assert.AreEqual(3, restored.VisitedLevelSnapshots.Count, "Dicionário completo restaurado.");

            // Mortos/depletados/baús/armadilhas preservados por nível após o round-trip.
            Assert.AreEqual(0, restored.VisitedLevelSnapshots[1].EnemyHpRecords[0].CurrentHp, "Inimigo morto no nível 1 segue morto.");
            Assert.IsTrue(restored.VisitedLevelSnapshots[2].DepletedResourceNodeIds.Contains("node_2_a"), "Nó depletado no nível 2 segue vazio.");
            Assert.IsTrue(restored.VisitedLevelSnapshots[3].IsChestOpened("chest_3_a"), "Baú aberto no nível 3 segue aberto.");
            Assert.AreEqual(2, restored.VisitedLevelSnapshots[3].GetTrapState("trap_3_a"), "Armadilha disparada no nível 3 não rearma.");
        }

        [Test]
        public void Mapper_OutputOrdering_IsStableByCaveLevelAscending()
        {
            // Insere fora de ordem; a lista persistida deve sair ordenada por CaveLevel asc (determinismo).
            var state = BuildRunWithLevels(2, 3, 1, 2);

            var data = CaveRunSaveMapper.ToSaveData(state);
            Assert.AreEqual(3, data.VisitedLevelSnapshots.Count);
            Assert.AreEqual(1, data.VisitedLevelSnapshots[0].CaveLevel);
            Assert.AreEqual(2, data.VisitedLevelSnapshots[1].CaveLevel);
            Assert.AreEqual(3, data.VisitedLevelSnapshots[2].CaveLevel);
        }

        [Test]
        public void Mapper_CurrentLevelSnapshot_WrittenInParallelForLegacyCompat()
        {
            var state = BuildRunWithLevels(3, 1, 2, 3);

            var data = CaveRunSaveMapper.ToSaveData(state);
            Assert.IsNotNull(data.CurrentLevelSnapshot, "CurrentLevelSnapshot mantido (compat F13).");
            Assert.AreEqual(3, data.CurrentLevelSnapshot.CaveLevel, "CurrentLevelSnapshot é o nível corrente.");
        }

        // ------------------------------------------------------------------ CA-2 cap LRU

        [Test]
        public void Mapper_Cap_KeepsOnlyMostRecentLevelsWithinBudget()
        {
            // 12 níveis visitados, corrente = 12. Cap = MaxPersistedLevelSnapshots (8). Mais "quentes"
            // (proximidade ao corrente) preservados; os mais distantes voltam a replay puro.
            var levels = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var state = BuildRunWithLevels(12, levels);

            var data = CaveRunSaveMapper.ToSaveData(state);
            Assert.AreEqual(CaveRunSaveData.MaxPersistedLevelSnapshots, data.VisitedLevelSnapshots.Count,
                "Persistidos exatamente N (cap) níveis.");

            var persistedLevels = new HashSet<int>();
            foreach (var s in data.VisitedLevelSnapshots)
            {
                persistedLevels.Add(s.CaveLevel);
            }

            // Corrente sempre incluído; os 8 mais próximos do corrente (12) são 5..12.
            Assert.IsTrue(persistedLevels.Contains(12), "Nível corrente sempre persistido.");
            Assert.IsTrue(persistedLevels.Contains(5), "8 níveis mais próximos do corrente preservados (5..12).");
            Assert.IsFalse(persistedLevels.Contains(4), "Nível fora do cap descartado (volta a replay de seed).");
            Assert.IsFalse(persistedLevels.Contains(1), "Nível mais antigo fora do cap descartado.");
        }

        [Test]
        public void Mapper_Cap_IsDeterministic_AcrossRepeatedSerialization()
        {
            var levels = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var state = BuildRunWithLevels(10, levels);

            var first = CaveRunSaveMapper.ToSaveData(state);
            var second = CaveRunSaveMapper.ToSaveData(state);

            Assert.AreEqual(first.VisitedLevelSnapshots.Count, second.VisitedLevelSnapshots.Count);
            for (int i = 0; i < first.VisitedLevelSnapshots.Count; i++)
            {
                Assert.AreEqual(first.VisitedLevelSnapshots[i].CaveLevel, second.VisitedLevelSnapshots[i].CaveLevel,
                    "Seleção e ordenação do cap são determinísticas entre serializações.");
            }
        }

        [Test]
        public void SaveSizeBudget_SyntheticCappedRun_IsWithinDocumentedBudget()
        {
            // Run sintética no cap: K = MaxPersistedLevelSnapshots níveis grandes (55×55 ≈ 3025 tiles).
            // Serializa com o MESMO JsonUtility indentado do SaveManager e assert < orçamento documentado.
            const int largeTilesPerSide = 55;
            var state = new CaveRuntimeState
            {
                CaveWorldSeed = "world_budget",
                CaveRunSeed = "run_budget",
                CurrentCaveLevel = CaveRunSaveData.MaxPersistedLevelSnapshots,
                DeepestLayerReached = CaveRunSaveData.MaxPersistedLevelSnapshots
            };

            for (int level = 1; level <= CaveRunSaveData.MaxPersistedLevelSnapshots; level++)
            {
                var snapshot = new CaveLevelSnapshot(level, "biome_fungal", "hash_" + level, "world_budget", "run_budget");
                snapshot.SetLayoutDimensions(largeTilesPerSide, largeTilesPerSide);
                // ~metade walkable, ~metade wall — espelha um nível real grande.
                for (int x = 0; x < largeTilesPerSide; x++)
                {
                    for (int y = 0; y < largeTilesPerSide; y++)
                    {
                        if (((x + y) & 1) == 0)
                        {
                            snapshot.AddWalkableTile(new Vector2Int(x, y));
                        }
                        else
                        {
                            snapshot.AddWallTile(new Vector2Int(x, y));
                        }
                    }
                }

                state.VisitedLevelSnapshots[level] = snapshot;
            }

            var data = CaveRunSaveMapper.ToSaveData(state);
            Assert.AreEqual(CaveRunSaveData.MaxPersistedLevelSnapshots, data.VisitedLevelSnapshots.Count);

            var json = JsonUtility.ToJson(data, true);
            var bytes = System.Text.Encoding.UTF8.GetByteCount(json);

            Assert.Less(bytes, CaveRunSaveData.SaveSizeBudgetBytes,
                $"Seção da caverna no cap ({bytes} bytes) deve caber no orçamento documentado ({CaveRunSaveData.SaveSizeBudgetBytes} bytes).");
        }

        // ------------------------------------------------------------------ CA-4 load legado

        [Test]
        public void LegacyLoad_OnlyCurrentLevelSnapshot_FallsBackToSingleLevel()
        {
            // Simula save F13: lista multi-nível ausente/vazia, só CurrentLevelSnapshot preenchido.
            var currentSnapshot = BuildSnapshot(8);
            var legacy = new CaveRunSaveData
            {
                HasActiveRun = true,
                WorldSeed = "world_abc",
                RunSeed = "run_xyz",
                CurrentLevel = 8,
                DeepestLevel = 8,
                CurrentLevelSnapshot = currentSnapshot,
                VisitedLevelSnapshots = new List<VisitedLevelSnapshot>() // vazia = save legado
            };

            var restored = CaveRunSaveMapper.FromSaveData(legacy);
            Assert.IsNotNull(restored, "Save legado carrega sem erro.");
            Assert.AreEqual(1, restored.VisitedLevelSnapshots.Count, "Fallback F13: só o nível corrente.");
            Assert.IsTrue(restored.VisitedLevelSnapshots.ContainsKey(8));
        }

        [Test]
        public void LegacyLoad_NullMultiLevelList_DoesNotThrow()
        {
            var currentSnapshot = BuildSnapshot(4);
            var legacy = new CaveRunSaveData
            {
                HasActiveRun = true,
                WorldSeed = "world_abc",
                RunSeed = "run_xyz",
                CurrentLevel = 4,
                DeepestLevel = 4,
                CurrentLevelSnapshot = currentSnapshot,
                VisitedLevelSnapshots = null // JsonUtility raramente, mas defensivo
            };

            CaveRuntimeState restored = null;
            Assert.DoesNotThrow(() => restored = CaveRunSaveMapper.FromSaveData(legacy));
            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.VisitedLevelSnapshots.Count);
        }

        [Test]
        public void EmptyRun_HasNoActiveRunAndNoSnapshots()
        {
            Assert.IsFalse(CaveRunSaveMapper.ToSaveData(null).HasActiveRun);
            var data = CaveRunSaveMapper.ToSaveData(new CaveRuntimeState());
            Assert.IsFalse(data.HasActiveRun);
            Assert.AreEqual(0, data.VisitedLevelSnapshots.Count);
        }

        // ------------------------------------------------------------------ CA-3 boss fight gate

        [Test]
        public void BossGate_DefaultsToSaveAllowed()
        {
            var gate = new CaveBossFightSaveGate();
            Assert.IsFalse(gate.IsBossFightActive);
            Assert.IsTrue(gate.CanSaveNow(), "Sem boss fight, save liberado.");
        }

        [Test]
        public void BossGate_BeginBlocksSave_EndAllowsSave()
        {
            var gate = new CaveBossFightSaveGate();
            gate.BeginBossFight(5);
            Assert.IsTrue(gate.IsBossFightActive);
            Assert.IsFalse(gate.CanSaveNow(), "Durante boss fight, save bloqueado.");
            Assert.AreEqual(5, gate.ActiveBossLevel);

            gate.EndBossFight();
            Assert.IsFalse(gate.IsBossFightActive);
            Assert.IsTrue(gate.CanSaveNow(), "Após o boss, save volta a funcionar.");
            Assert.AreEqual(-1, gate.ActiveBossLevel);
        }

        [Test]
        public void BossGate_Begin_IsIdempotent()
        {
            var gate = new CaveBossFightSaveGate();
            gate.BeginBossFight(7);
            gate.BeginBossFight(7);
            Assert.IsTrue(gate.IsBossFightActive);
            Assert.IsFalse(gate.CanSaveNow());
        }

        [Test]
        public void BossGate_End_WhenInactive_IsNoOpSafe()
        {
            var gate = new CaveBossFightSaveGate();
            Assert.DoesNotThrow(() => gate.EndBossFight());
            Assert.IsFalse(gate.IsBossFightActive);
            Assert.IsTrue(gate.CanSaveNow());
        }

        [Test]
        public void BossGate_EveryEndPath_ClearsFlag()
        {
            // Cada caminho de fim (derrota, morte, saída de nível, saída de caverna) chama EndBossFight.
            // Aqui validamos a semântica do gate: independente de quantas vezes inicia, qualquer End limpa.
            var gate = new CaveBossFightSaveGate();

            gate.BeginBossFight(3);
            gate.EndBossFight(); // derrota do boss
            Assert.IsTrue(gate.CanSaveNow());

            gate.BeginBossFight(3);
            gate.EndBossFight(); // morte do player
            Assert.IsTrue(gate.CanSaveNow());

            gate.BeginBossFight(3);
            gate.EndBossFight(); // saída do nível / da caverna
            Assert.IsTrue(gate.CanSaveNow());
        }

        [Test]
        public void BossGate_SaveBlockedReason_IsAsciiSafeMessage()
        {
            // Mensagem de recusa sem acentos (canal de feedback existente; sem UI nova).
            Assert.IsFalse(string.IsNullOrWhiteSpace(CaveBossFightSaveGate.SaveBlockedReason));
            foreach (var ch in CaveBossFightSaveGate.SaveBlockedReason)
            {
                Assert.Less((int)ch, 128, "Razão de recusa deve ser ASCII-safe.");
            }
        }
    }
}
