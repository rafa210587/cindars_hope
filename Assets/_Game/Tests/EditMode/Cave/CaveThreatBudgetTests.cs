using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_78 (SLICE 2) — threat budget + entrada segura + distribuição por sala + gating aquático
    /// (critérios de aceite 14.3 e 14.4).
    ///
    /// Cobre: budget cresce com a profundidade; piso e teto da banda respeitados; hard cap não
    /// estourado; raio de entrada limpo (nenhum spawn dentro do SafeEntryRadius); distribuição por
    /// sala soma o total; gating aquático (com água → aquático elegível; sem água → não elegível).
    /// Determinístico — sem UnityEngine.Random.
    /// </summary>
    [TestFixture]
    public class CaveThreatBudgetTests
    {
        private CaveEcosystemBalanceSO _balance;

        // Nível representativo de cada banda 1..7 (stone..void).
        private static readonly int[] BandRepresentativeLevels = { 1, 11, 26, 41, 56, 71, 86 };

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

        [Test]
        public void ResolveThreatBudget_GrowsWithDepth_AcrossBands()
        {
            var previous = int.MinValue;
            for (var band = 1; band <= CaveEcosystemBalanceSO.BandCount; band++)
            {
                var level = BandRepresentativeLevels[band - 1];
                var budget = CaveEnemySpawnPlanner.ResolveThreatBudget(band, level, _balance);
                Assert.GreaterOrEqual(budget, previous,
                    $"Banda {band} (nível {level}): budget {budget} deve ser >= banda anterior ({previous}).");
                previous = budget;
            }
        }

        [Test]
        public void ResolveThreatBudget_StaysBetweenFloorAndCeiling()
        {
            for (var band = 1; band <= CaveEcosystemBalanceSO.BandCount; band++)
            {
                var floor = _balance.GetThreatBudgetMin(band - 1);
                var ceiling = _balance.GetThreatBudgetMax(band - 1);
                // Varre todos os níveis da banda (representativo + alguns acima).
                for (var level = BandRepresentativeLevels[band - 1]; level < BandRepresentativeLevels[band - 1] + 15; level++)
                {
                    var budget = CaveEnemySpawnPlanner.ResolveThreatBudget(band, level, _balance);
                    Assert.GreaterOrEqual(budget, floor,
                        $"Banda {band} nível {level}: budget {budget} abaixo do piso {floor}.");
                    Assert.LessOrEqual(budget, ceiling,
                        $"Banda {band} nível {level}: budget {budget} acima do teto {ceiling}.");
                }
            }
        }

        [Test]
        public void ResolveThreatBudget_NeverExceedsHardCap()
        {
            for (var band = 1; band <= CaveEcosystemBalanceSO.BandCount; band++)
            {
                var level = BandRepresentativeLevels[band - 1] + 10;
                var budget = CaveEnemySpawnPlanner.ResolveThreatBudget(band, level, _balance);
                Assert.LessOrEqual(budget, _balance.EnemyDensityHardCap,
                    $"Banda {band}: budget {budget} estourou o hard cap {_balance.EnemyDensityHardCap}.");
            }
        }

        [Test]
        public void ResolveThreatBudget_IsDeterministic()
        {
            for (var band = 1; band <= CaveEcosystemBalanceSO.BandCount; band++)
            {
                var level = BandRepresentativeLevels[band - 1] + 3;
                var a = CaveEnemySpawnPlanner.ResolveThreatBudget(band, level, _balance);
                var b = CaveEnemySpawnPlanner.ResolveThreatBudget(band, level, _balance);
                Assert.AreEqual(a, b, $"Banda {band}: budget não-determinístico.");
            }
        }

        [Test]
        public void ResolveThreatBudget_DeepLevelWithinBand_IsNotBelowFloor()
        {
            // O piso garante desafio mínimo — nenhum nível regular sai abaixo do threat mínimo.
            var band = 3; // Ice
            var floor = _balance.GetThreatBudgetMin(band - 1);
            for (var level = 26; level <= 40; level++)
            {
                var budget = CaveEnemySpawnPlanner.ResolveThreatBudget(band, level, _balance);
                Assert.GreaterOrEqual(budget, floor, $"Nível {level} abaixo do piso da banda.");
            }
        }

        [Test]
        public void ApplySafeEntryRadius_RemovesPointsWithinRadius()
        {
            var entrance = new Vector2Int(10, 10);
            var radius = 4f;
            var points = new List<Vector2Int>
            {
                new Vector2Int(10, 10), // distância 0 → removido
                new Vector2Int(12, 10), // distância 2 → removido
                new Vector2Int(13, 13), // distância ~4.24 → mantido
                new Vector2Int(20, 20), // longe → mantido
            };

            var result = CaveEnemySpawnPlanner.ApplySafeEntryRadius(points, entrance, radius);

            Assert.IsFalse(result.Contains(new Vector2Int(10, 10)), "Ponto na entrada deveria ser removido.");
            Assert.IsFalse(result.Contains(new Vector2Int(12, 10)), "Ponto dentro do raio deveria ser removido.");
            Assert.IsTrue(result.Contains(new Vector2Int(13, 13)), "Ponto fora do raio deveria permanecer.");
            Assert.IsTrue(result.Contains(new Vector2Int(20, 20)), "Ponto distante deveria permanecer.");

            // Invariante: nenhum ponto restante dentro do raio.
            foreach (var p in result)
            {
                var dx = p.x - entrance.x;
                var dy = p.y - entrance.y;
                Assert.GreaterOrEqual(dx * dx + dy * dy, radius * radius,
                    $"Ponto {p} permaneceu dentro do SafeEntryRadius.");
            }
        }

        [Test]
        public void DistributeEnemiesPerRoom_SumsToTotal()
        {
            var rooms = new List<CaveRoom>
            {
                new CaveRoom(0, 0, 10, 10),   // área 100
                new CaveRoom(20, 0, 6, 6),    // área 36
                new CaveRoom(0, 20, 14, 8),   // área 112
            };

            var counts = CaveEnemySpawnPlanner.DistributeEnemiesPerRoom(rooms, 20);

            Assert.AreEqual(rooms.Count, counts.Count, "Deve haver uma contagem por sala.");
            Assert.AreEqual(20, counts.Sum(), "A soma das contagens por sala deve bater o total.");
            // A maior sala deve receber pelo menos tanto quanto a menor.
            Assert.GreaterOrEqual(counts[2], counts[1], "Sala maior deveria receber >= sala menor.");
        }

        [Test]
        public void DistributeEnemiesPerRoom_IsDeterministic()
        {
            var rooms = new List<CaveRoom>
            {
                new CaveRoom(0, 0, 12, 9),
                new CaveRoom(20, 0, 8, 8),
                new CaveRoom(0, 20, 10, 10),
                new CaveRoom(20, 20, 5, 5),
            };

            var a = CaveEnemySpawnPlanner.DistributeEnemiesPerRoom(rooms, 17);
            var b = CaveEnemySpawnPlanner.DistributeEnemiesPerRoom(rooms, 17);
            CollectionAssert.AreEqual(a, b, "Distribuição por sala deve ser determinística.");
        }

        [Test]
        public void DistributeEnemiesPerRoom_NoRooms_ReturnsEmpty()
        {
            var empty = CaveEnemySpawnPlanner.DistributeEnemiesPerRoom(new List<CaveRoom>(), 10);
            Assert.IsEmpty(empty);
            var nullRooms = CaveEnemySpawnPlanner.DistributeEnemiesPerRoom(null, 10);
            Assert.IsEmpty(nullRooms);
        }

        [Test]
        public void FilterAquaticEligibility_WithWater_KeepsAquatics()
        {
            var candidates = new List<CaveEnemySpawnPlanner.AquaticCandidate>
            {
                new CaveEnemySpawnPlanner.AquaticCandidate("enemy_lake_lurker", true),
                new CaveEnemySpawnPlanner.AquaticCandidate("enemy_cave_bat", false),
            };

            var result = CaveEnemySpawnPlanner.FilterAquaticEligibility(candidates, hasWater: true);

            Assert.AreEqual(2, result.Count, "Com água, todos (aquáticos + terrestres) são elegíveis.");
            Assert.IsTrue(result.Any(c => c.EnemyId == "enemy_lake_lurker"), "Aquático deveria ser elegível com água.");
        }

        [Test]
        public void FilterAquaticEligibility_WithoutWater_RemovesAquatics()
        {
            var candidates = new List<CaveEnemySpawnPlanner.AquaticCandidate>
            {
                new CaveEnemySpawnPlanner.AquaticCandidate("enemy_lake_lurker", true),
                new CaveEnemySpawnPlanner.AquaticCandidate("enemy_cave_bat", false),
            };

            var result = CaveEnemySpawnPlanner.FilterAquaticEligibility(candidates, hasWater: false);

            Assert.AreEqual(1, result.Count, "Sem água, aquáticos não entram.");
            Assert.IsFalse(result.Any(c => c.EnemyId == "enemy_lake_lurker"), "Aquático não deveria entrar sem água.");
            Assert.IsTrue(result.Any(c => c.EnemyId == "enemy_cave_bat"), "Terrestre permanece sem água.");
        }
    }
}
