using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_78 (SLICE 1) — conflito inter-monstro determinístico (critério de aceite 14.5).
    ///
    /// Cobre: frequência ~5% (sem conflito anterior) e ~0,5% (após o primeiro); queda 5%→0,5%;
    /// espécies sempre distintas; reprodutibilidade por entryIndex fixo; variação entre entryIndex;
    /// fallback &lt;2 espécies → inativo. Determinístico (FNV-1a via CaveLayoutStableHash) — sem
    /// UnityEngine.Random.
    /// </summary>
    [TestFixture]
    public class CaveEcosystemConflictPlannerTests
    {
        private CaveEcosystemBalanceSO _balance;

        private static readonly string[] ThreeSpecies = { "enemy_a", "enemy_b", "enemy_c" };

        [SetUp]
        public void SetUp()
        {
            _balance = ScriptableObject.CreateInstance<CaveEcosystemBalanceSO>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_balance != null)
            {
                Object.DestroyImmediate(_balance);
            }
        }

        private float MeasureFrequency(bool hasHadConflictBefore, int sampleSize)
        {
            var active = 0;
            for (var i = 0; i < sampleSize; i++)
            {
                // Varia worldSeed/runSeed/level para uma amostra grande de seeds independentes.
                var worldSeed = $"world_{i}";
                var runSeed = $"run_{i % 97}";
                var caveLevel = (i % 100) + 1;
                var plan = CaveEcosystemConflictPlanner.Decide(
                    worldSeed, runSeed, caveLevel, entryIndex: 0,
                    hasHadConflictBefore, ThreeSpecies, _balance);
                if (plan.ConflictActive)
                {
                    active++;
                }
            }

            return active / (float)sampleSize;
        }

        [Test]
        public void Decide_BaseChance_IsApproximatelyFivePercent()
        {
            var freq = MeasureFrequency(hasHadConflictBefore: false, sampleSize: 20000);
            // Tolerância generosa para amostra de 20k (default 0.05).
            Assert.That(freq, Is.EqualTo(0.05f).Within(0.012f),
                $"Frequência de conflito base esperada ~5%, obtido {freq:P2}.");
        }

        [Test]
        public void Decide_ReducedChance_IsApproximatelyHalfPercent()
        {
            var freq = MeasureFrequency(hasHadConflictBefore: true, sampleSize: 20000);
            Assert.That(freq, Is.EqualTo(0.005f).Within(0.004f),
                $"Frequência de conflito reduzida esperada ~0,5%, obtido {freq:P2}.");
        }

        [Test]
        public void Decide_ReducedChance_IsLowerThanBaseChance()
        {
            var baseFreq = MeasureFrequency(hasHadConflictBefore: false, sampleSize: 20000);
            var reducedFreq = MeasureFrequency(hasHadConflictBefore: true, sampleSize: 20000);
            Assert.Less(reducedFreq, baseFreq,
                "A chance após o primeiro conflito deve cair (5% → 0,5%).");
        }

        [Test]
        public void Decide_WhenActive_AlwaysPicksTwoDistinctSpecies()
        {
            var checkedActive = 0;
            for (var i = 0; i < 5000 && checkedActive < 200; i++)
            {
                var plan = CaveEcosystemConflictPlanner.Decide(
                    $"world_{i}", $"run_{i}", (i % 100) + 1, entryIndex: i % 7,
                    hasHadConflictBefore: false, ThreeSpecies, _balance);
                if (!plan.ConflictActive)
                {
                    continue;
                }

                checkedActive++;
                Assert.IsNotEmpty(plan.FactionAEnemyId);
                Assert.IsNotEmpty(plan.FactionBEnemyId);
                Assert.AreNotEqual(plan.FactionAEnemyId, plan.FactionBEnemyId,
                    "Lados A e B nunca podem ser a mesma espécie.");
            }

            Assert.Greater(checkedActive, 0, "A amostra deveria conter ao menos um conflito ativo.");
        }

        [Test]
        public void Decide_IsReproducible_ForSameEntryIndex()
        {
            for (var i = 0; i < 200; i++)
            {
                var a = CaveEcosystemConflictPlanner.Decide(
                    "world_repro", "run_repro", caveLevel: 12, entryIndex: i,
                    hasHadConflictBefore: false, ThreeSpecies, _balance);
                var b = CaveEcosystemConflictPlanner.Decide(
                    "world_repro", "run_repro", caveLevel: 12, entryIndex: i,
                    hasHadConflictBefore: false, ThreeSpecies, _balance);

                Assert.AreEqual(a.ConflictActive, b.ConflictActive, $"entryIndex {i}: ConflictActive divergiu.");
                Assert.AreEqual(a.FactionAEnemyId, b.FactionAEnemyId, $"entryIndex {i}: FactionA divergiu.");
                Assert.AreEqual(a.FactionBEnemyId, b.FactionBEnemyId, $"entryIndex {i}: FactionB divergiu.");
            }
        }

        [Test]
        public void Decide_VariesAcrossEntryIndex()
        {
            // Sobre muitas entradas no MESMO nível/run, o resultado de ConflictActive deve variar
            // (o re-roll por entrada é o comportamento pedido).
            var results = new HashSet<bool>();
            for (var entry = 0; entry < 400; entry++)
            {
                var plan = CaveEcosystemConflictPlanner.Decide(
                    "world_vary", "run_vary", caveLevel: 30, entryIndex: entry,
                    hasHadConflictBefore: false, ThreeSpecies, _balance);
                results.Add(plan.ConflictActive);
                if (results.Count == 2)
                {
                    break;
                }
            }

            Assert.AreEqual(2, results.Count,
                "Esperado ao menos um conflito ativo e um inativo ao variar entryIndex no mesmo nível.");
        }

        [Test]
        public void Decide_WithFewerThanTwoDistinctSpecies_IsInactive()
        {
            var oneSpecies = new List<string> { "enemy_a", "enemy_a", "enemy_a" };
            for (var entry = 0; entry < 50; entry++)
            {
                var plan = CaveEcosystemConflictPlanner.Decide(
                    "world_one", "run_one", caveLevel: 5, entryIndex: entry,
                    hasHadConflictBefore: false, oneSpecies, _balance);
                Assert.IsFalse(plan.ConflictActive, "Com <2 espécies distintas, nunca pode haver conflito.");
            }

            var empty = CaveEcosystemConflictPlanner.Decide(
                "world_empty", "run_empty", caveLevel: 5, entryIndex: 0,
                hasHadConflictBefore: false, new List<string>(), _balance);
            Assert.IsFalse(empty.ConflictActive);
        }

        [Test]
        public void Decide_WithNullBalance_IsInactive()
        {
            var plan = CaveEcosystemConflictPlanner.Decide(
                "world", "run", 1, 0, false, ThreeSpecies, null);
            Assert.IsFalse(plan.ConflictActive);
        }
    }
}
