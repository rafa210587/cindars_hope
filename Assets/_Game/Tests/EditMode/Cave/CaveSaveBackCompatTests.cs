using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Save;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_78 (14.8 / 16.4) — persistência ADITIVA do ecossistema da caverna e back-compat de save.
    ///
    /// Cobre, sem refs Unity nos DTOs:
    /// - round-trip dos novos campos (EnvironmentElements / HasWater / ConflictState) via
    ///   SerializedVisitedLevelSnapshot (mirror de CaveSaveData) E via CaveRunSaveData/Mapper
    ///   (caminho de save ativo que serializa VisitedLevelSnapshot diretamente);
    /// - save antigo SEM os campos → defaults seguros (listas vazias, HasWater=false, conflito inativo,
    ///   EntryCount=0) sem erro;
    /// - estado depletado de minerável persiste;
    /// - EntryCount/HasHadConflict acumulam corretamente.
    /// </summary>
    public class CaveSaveBackCompatTests
    {
        // ------------------------------------------------------------------ helpers

        private static VisitedLevelSnapshot BuildSnapshot(int level, int tileCount = 4)
        {
            var snapshot = new CaveLevelSnapshot(level, "biome_cave_ice", "hash_" + level, "world_abc", "run_xyz");
            snapshot.SetLayoutDimensions(Mathf.Max(2, tileCount), Mathf.Max(2, tileCount));
            for (var i = 0; i < tileCount; i++)
            {
                snapshot.AddWalkableTile(new Vector2Int(i, level));
            }

            return snapshot;
        }

        private static SerializedEnvironmentElement Decor(string id, int x, int y)
        {
            return new SerializedEnvironmentElement
            {
                ElementId = id,
                Kind = 0, // DecorNonBlocking
                GridX = x,
                GridY = y,
                IsMineable = false,
                MineNodeDataId = string.Empty,
                IsDepleted = false
            };
        }

        private static SerializedEnvironmentElement Ore(string id, int x, int y, string nodeDataId, bool depleted)
        {
            return new SerializedEnvironmentElement
            {
                ElementId = id,
                Kind = 3, // MineableNode
                GridX = x,
                GridY = y,
                IsMineable = true,
                MineNodeDataId = nodeDataId,
                IsDepleted = depleted
            };
        }

        // ------------------------------------------------------------------ CA: round-trip CaveSaveData mirror

        [Test]
        public void CaveSaveDataMirror_RoundTrip_PreservesNewFields()
        {
            var snapshot = BuildSnapshot(3);
            snapshot.SetEnvironmentElements(new[]
            {
                Decor("cave_elem_3_1_3_DecorNonBlocking", 1, 3),
                Ore("cave_elem_3_2_3_MineableNode", 2, 3, "resnode_ore_iron_3", depleted: true)
            });
            snapshot.HasWater = true;
            snapshot.RecordConflictEntry(true, "enemy_a", "enemy_b");

            var serialized = SerializedVisitedLevelSnapshot.FromSnapshot(snapshot);
            var restored = serialized.ToSnapshot();

            Assert.IsNotNull(restored, "Snapshot reconstruido do mirror.");
            Assert.AreEqual(2, restored.EnvironmentElements.Count, "Elementos ambientais preservados.");
            Assert.IsTrue(restored.HasWater, "HasWater preservado.");

            var ore = restored.EnvironmentElements.Find(e => e.IsMineable);
            Assert.IsNotNull(ore, "Minerável preservado.");
            Assert.IsTrue(ore.IsDepleted, "Estado depletado do minerável persiste.");
            Assert.AreEqual("resnode_ore_iron_3", ore.MineNodeDataId, "MineNodeDataId preservado.");

            Assert.IsNotNull(restored.ConflictState, "ConflictState preservado.");
            Assert.IsTrue(restored.ConflictState.ConflictActive, "ConflictActive preservado.");
            Assert.IsTrue(restored.ConflictState.HasHadConflict, "HasHadConflict travado em true.");
            Assert.AreEqual(1, restored.ConflictState.EntryCount, "EntryCount preservado.");
            Assert.AreEqual("enemy_a", restored.ConflictState.FactionAId);
            Assert.AreEqual("enemy_b", restored.ConflictState.FactionBId);
        }

        [Test]
        public void CaveSaveDataMirror_DeepCopies_DoNotShareReferences()
        {
            var snapshot = BuildSnapshot(2);
            snapshot.SetEnvironmentElements(new[] { Decor("cave_elem_2_0_2_DecorNonBlocking", 0, 2) });

            var serialized = SerializedVisitedLevelSnapshot.FromSnapshot(snapshot);
            // Muta a fonte após a serialização — o DTO não pode refletir a mudança (cópia profunda).
            snapshot.EnvironmentElements[0].IsDepleted = true;

            Assert.IsFalse(serialized.EnvironmentElements[0].IsDepleted,
                "FromSnapshot faz deep-copy (sem alias da lista de elementos).");
        }

        // ------------------------------------------------------------------ CA: round-trip CaveRunSaveData (caminho ativo)

        [Test]
        public void CaveRunSaveData_RoundTrip_PreservesNewFields_ViaJsonUtility()
        {
            var state = new CaveRuntimeState
            {
                CaveWorldSeed = "world_abc",
                CaveRunSeed = "run_xyz",
                CurrentCaveLevel = 1,
                DeepestLayerReached = 1
            };
            var snapshot = BuildSnapshot(1);
            snapshot.SetEnvironmentElements(new[]
            {
                Ore("cave_elem_1_2_1_MineableNode", 2, 1, "resnode_ore_copper_1", depleted: false)
            });
            snapshot.HasWater = true;
            snapshot.RecordConflictEntry(false, string.Empty, string.Empty); // entrada sem conflito
            state.VisitedLevelSnapshots[1] = snapshot;

            var data = CaveRunSaveMapper.ToSaveData(state);
            // Serializa/desserializa com o MESMO JsonUtility do SaveManager.
            var json = JsonUtility.ToJson(data, true);
            var reloaded = JsonUtility.FromJson<CaveRunSaveData>(json);
            var restored = CaveRunSaveMapper.FromSaveData(reloaded);

            Assert.IsNotNull(restored, "Run restaurada do JSON.");
            var restoredSnapshot = restored.VisitedLevelSnapshots[1];
            Assert.AreEqual(1, restoredSnapshot.EnvironmentElements.Count, "Elemento ambiental sobrevive ao JSON round-trip.");
            Assert.IsTrue(restoredSnapshot.HasWater, "HasWater sobrevive ao JSON round-trip.");
            Assert.IsNotNull(restoredSnapshot.ConflictState, "ConflictState presente apos JSON round-trip.");
            Assert.AreEqual(1, restoredSnapshot.ConflictState.EntryCount, "EntryCount preservado no JSON round-trip.");
            Assert.IsFalse(restoredSnapshot.ConflictState.HasHadConflict, "Entrada sem conflito nao trava HasHadConflict.");
        }

        // ------------------------------------------------------------------ CA: back-compat (save antigo sem os campos)

        [Test]
        public void LegacySnapshot_WithoutNewFields_LoadsWithSafeDefaults()
        {
            // Simula JSON de um save anterior a fable_78: sem EnvironmentElements/HasWater/ConflictState.
            const string legacyJson =
                "{\"CaveLevel\":5,\"SnapshotId\":\"snapshot_run_xyz_5\",\"CaveWorldSeed\":\"world_abc\"," +
                "\"CaveRunSeed\":\"run_xyz\",\"BiomeId\":\"biome_cave_ice\",\"LayoutHash\":\"hash_5\"," +
                "\"Width\":4,\"Height\":4," +
                "\"WalkableTilesList\":[{\"x\":0,\"y\":5},{\"x\":1,\"y\":5}]}";

            VisitedLevelSnapshot restored = null;
            Assert.DoesNotThrow(() => restored = JsonUtility.FromJson<VisitedLevelSnapshot>(legacyJson),
                "Save antigo sem os campos novos carrega sem erro.");

            Assert.IsNotNull(restored);
            Assert.IsNotNull(restored.EnvironmentElements, "Lista de elementos nunca nula (default seguro).");
            Assert.AreEqual(0, restored.EnvironmentElements.Count, "Save antigo => lista de elementos vazia.");
            Assert.IsFalse(restored.HasWater, "Save antigo => HasWater=false.");
            Assert.IsNotNull(restored.ConflictState, "ConflictState nunca nulo (default seguro).");
            Assert.IsFalse(restored.ConflictState.ConflictActive, "Save antigo => conflito inativo.");
            Assert.IsFalse(restored.ConflictState.HasHadConflict, "Save antigo => sem conflito previo.");
            Assert.AreEqual(0, restored.ConflictState.EntryCount, "Save antigo => EntryCount=0.");
        }

        [Test]
        public void LegacyMirror_NullNewFields_ToSnapshot_DoesNotThrowAndDefaults()
        {
            // Mirror DTO com os campos novos nulos (defensivo, como save legado deserializado).
            var legacy = new SerializedVisitedLevelSnapshot
            {
                CaveLevel = 7,
                SnapshotId = "snapshot_run_xyz_7",
                BiomeId = "biome_cave_ice",
                LayoutHash = "hash_7",
                Width = 4,
                Height = 4,
                WalkableTilesList = new List<Vector2Int> { new Vector2Int(0, 7) },
                EnvironmentElements = null,
                ConflictState = null
            };

            VisitedLevelSnapshot restored = null;
            Assert.DoesNotThrow(() => restored = legacy.ToSnapshot());
            Assert.IsNotNull(restored);
            Assert.IsNotNull(restored.EnvironmentElements);
            Assert.AreEqual(0, restored.EnvironmentElements.Count);
            Assert.IsNotNull(restored.ConflictState);
            Assert.AreEqual(0, restored.ConflictState.EntryCount);
        }

        // ------------------------------------------------------------------ CA: acumulo de conflito por entrada

        [Test]
        public void RecordConflictEntry_AccumulatesEntryCount_AndLatchesHasHadConflict()
        {
            var snapshot = BuildSnapshot(4);

            snapshot.RecordConflictEntry(false, string.Empty, string.Empty);
            Assert.AreEqual(1, snapshot.ConflictState.EntryCount);
            Assert.IsFalse(snapshot.ConflictState.HasHadConflict, "Sem conflito ainda => nao trava.");

            snapshot.RecordConflictEntry(true, "enemy_a", "enemy_b");
            Assert.AreEqual(2, snapshot.ConflictState.EntryCount);
            Assert.IsTrue(snapshot.ConflictState.HasHadConflict, "Primeiro conflito trava HasHadConflict.");

            // Entrada posterior sem conflito NAO destrava HasHadConflict (governa a queda 5%->0,5%).
            snapshot.RecordConflictEntry(false, string.Empty, string.Empty);
            Assert.AreEqual(3, snapshot.ConflictState.EntryCount);
            Assert.IsTrue(snapshot.ConflictState.HasHadConflict, "HasHadConflict permanece travado.");
            Assert.IsFalse(snapshot.ConflictState.ConflictActive, "ConflictActive reflete a visita atual (sem conflito).");
        }

        [Test]
        public void MarkEnvironmentElementDepleted_IsIdempotentById()
        {
            var snapshot = BuildSnapshot(6);
            snapshot.SetEnvironmentElements(new[]
            {
                Ore("cave_elem_6_1_6_MineableNode", 1, 6, "resnode_ore_silver_6", depleted: false)
            });

            snapshot.MarkEnvironmentElementDepleted("cave_elem_6_1_6_MineableNode");
            snapshot.MarkEnvironmentElementDepleted("cave_elem_6_1_6_MineableNode"); // idempotente

            Assert.IsTrue(snapshot.EnvironmentElements[0].IsDepleted, "Minerável marcado depletado.");
        }
    }
}
