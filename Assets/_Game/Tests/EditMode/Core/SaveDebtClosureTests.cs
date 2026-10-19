using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Core
{
    /// <summary>
    /// F13 — fechamento dos 3 débitos de save (cave run, enemy HP, daily goals wiring).
    /// </summary>
    public class SaveDebtClosureTests
    {
        private static CaveRuntimeState BuildState()
        {
            var state = new CaveRuntimeState
            {
                CaveWorldSeed = "world_abc",
                CaveRunSeed = "run_xyz",
                CurrentCaveLevel = 8,
                DeepestLayerReached = 9
            };
            state.UnlockedCheckpoints.Add(5);
            state.UnlockedCheckpoints.Add(1);
            state.DepletedNodeIds.Add("node_b");
            state.DepletedNodeIds.Add("node_a");
            state.BossDefeatStates["gate_5"] = new CaveBossDefeatState
            {
                BossGateId = "gate_5",
                CaveLevel = 5,
                IsDefeated = true,
                DefeatedAt = "2026-06-12T00:00:00Z"
            };
            return state;
        }

        // ------------------------------------------------------------------ mapper

        [Test]
        public void Mapper_RoundTrip_PreservesRunIdentity()
        {
            var state = BuildState();
            var data = CaveRunSaveMapper.ToSaveData(state);
            var restored = CaveRunSaveMapper.FromSaveData(data);

            Assert.IsTrue(data.HasActiveRun);
            Assert.AreEqual("world_abc", restored.CaveWorldSeed);
            Assert.AreEqual("run_xyz", restored.CaveRunSeed);
            Assert.AreEqual(8, restored.CurrentCaveLevel);
            Assert.AreEqual(9, restored.DeepestLayerReached);
            Assert.IsTrue(restored.UnlockedCheckpoints.SetEquals(state.UnlockedCheckpoints));
            Assert.IsTrue(restored.DepletedNodeIds.SetEquals(state.DepletedNodeIds));
            Assert.IsTrue(restored.BossDefeatStates.ContainsKey("gate_5"));
            Assert.IsTrue(restored.BossDefeatStates["gate_5"].IsDefeated);
        }

        [Test]
        public void Mapper_NoActiveRun_WhenStateNullOrSeedless()
        {
            Assert.IsFalse(CaveRunSaveMapper.ToSaveData(null).HasActiveRun);
            Assert.IsFalse(CaveRunSaveMapper.ToSaveData(new CaveRuntimeState()).HasActiveRun);
            Assert.IsNull(CaveRunSaveMapper.FromSaveData(new CaveRunSaveData { HasActiveRun = false }));
            Assert.IsNull(CaveRunSaveMapper.FromSaveData(null), "Save legado sem seção → sem run (default).");
        }

        // fable_44 ATUALIZADO: o contrato F13 "só o nível corrente" foi substituído por persistência
        // multi-nível (cap LRU). CurrentLevelSnapshot continua escrito em paralelo (compat de load legado),
        // mas a lista VisitedLevelSnapshots agora carrega TODOS os níveis visitados (dentro do cap).
        // Cobertura preservada: current snapshot correto + multi-nível restaurado. (Antes:
        // Mapper_IncludesOnlyCurrentLevelSnapshot — ver CaveMultiLevelSaveTests para a suíte fable_44.)
        [Test]
        public void Mapper_WritesCurrentLevelSnapshotAndPersistsAllVisitedLevels()
        {
            var state = BuildState();
            var currentSnapshot = new CaveLevelSnapshot(8, "biome_fungal", "hash", "world_abc", "run_xyz");
            currentSnapshot.SetLayoutDimensions(10, 10);
            currentSnapshot.AddWalkableTile(new Vector2Int(1, 1));
            var otherSnapshot = new CaveLevelSnapshot(7, "biome_fungal", "hash", "world_abc", "run_xyz");
            otherSnapshot.SetLayoutDimensions(10, 10);
            otherSnapshot.AddWalkableTile(new Vector2Int(1, 1));
            state.VisitedLevelSnapshots[8] = currentSnapshot;
            state.VisitedLevelSnapshots[7] = otherSnapshot;

            var data = CaveRunSaveMapper.ToSaveData(state);
            Assert.IsNotNull(data.CurrentLevelSnapshot);
            Assert.AreEqual(8, data.CurrentLevelSnapshot.CaveLevel, "CurrentLevelSnapshot escrito em paralelo (compat F13).");
            Assert.AreEqual(2, data.VisitedLevelSnapshots.Count, "fable_44: ambos os níveis visitados persistidos.");

            var restored = CaveRunSaveMapper.FromSaveData(data);
            Assert.AreEqual(2, restored.VisitedLevelSnapshots.Count, "fable_44: dicionário multi-nível restaurado.");
            Assert.IsTrue(restored.VisitedLevelSnapshots.ContainsKey(7));
            Assert.IsTrue(restored.VisitedLevelSnapshots.ContainsKey(8));
        }

        // ------------------------------------------------------------------ enemy hp

        [Test]
        public void Snapshot_SetEnemyHpRecords_FiltersInvalid()
        {
            var snapshot = new CaveLevelSnapshot(3, "biome", "hash", "w", "r");
            snapshot.SetEnemyHpRecords(new List<EnemyHpRecord>
            {
                new EnemyHpRecord { EnemyInstanceId = "enemy_1", CurrentHp = 3 },
                new EnemyHpRecord { EnemyInstanceId = "", CurrentHp = 5 },
                null,
                new EnemyHpRecord { EnemyInstanceId = "enemy_2", CurrentHp = 0 }
            });

            Assert.AreEqual(2, snapshot.EnemyHpRecords.Count);
            Assert.AreEqual(0, snapshot.EnemyHpRecords[1].CurrentHp, "Morto (0 HP) é registrado — não rematerializa.");
        }

        [Test]
        public void Snapshot_EnemyHpRecords_DoNotAffectLayoutHash()
        {
            var service = new CaveSnapshotService();
            var snapshot = new CaveLevelSnapshot(3, "biome", string.Empty, "w", "r");
            snapshot.SetLayoutDimensions(5, 5);
            snapshot.AddWalkableTile(new Vector2Int(2, 2));
            var hashBefore = service.CalculateLayoutHash(snapshot);

            snapshot.SetEnemyHpRecords(new[] { new EnemyHpRecord { EnemyInstanceId = "enemy_1", CurrentHp = 1 } });
            var hashAfter = service.CalculateLayoutHash(snapshot);

            Assert.AreEqual(hashBefore, hashAfter, "HP é estado mutável — fora do replay hash (stable-run).");
        }

        // ------------------------------------------------------------------ legado

        [Test]
        public void LegacySave_WithoutCaveRunSection_LoadsWithDefaults()
        {
            // Simula JsonUtility de save v5: campo novo ausente vira instância default.
            var legacy = new CaveRunSaveData();
            Assert.IsFalse(legacy.HasActiveRun);
            Assert.IsNull(CaveRunSaveMapper.FromSaveData(legacy));
        }
    }
}
