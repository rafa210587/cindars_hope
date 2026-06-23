using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_78 (SLICE 2) — tamanho do mapa escala com profundidade (critério de aceite 14.1).
    ///
    /// Cobre: monotonicidade do tamanho-base por banda (Stone → Deep/Void cresce, nunca decresce);
    /// determinismo (mesmo worldSeed/runSeed/caveLevel → mesmo tamanho sempre); e preservação dos
    /// buckets de variação por seed (small/base/large) do fable_09 em torno do base da banda.
    /// Determinístico (FNV-1a via CaveLayoutStableHash) — sem UnityEngine.Random.
    /// </summary>
    [TestFixture]
    public class CaveMapSizeScalingTests
    {
        private const string WorldSeed = "world_seed_fable78_size";
        private const string RunSeed = "run_seed_fable78_size";

        // Nível representativo de cada banda 1..7 (stone..void).
        private static readonly int[] BandRepresentativeLevels = { 1, 11, 26, 41, 56, 71, 86 };

        [Test]
        public void ResolveBandBaseMapSize_GrowsMonotonically_AcrossBands()
        {
            var previous = int.MinValue;
            foreach (var level in BandRepresentativeLevels)
            {
                var baseSize = CaveBiomeLayoutProfile.ResolveBandBaseMapSize(level);
                Assert.GreaterOrEqual(baseSize, previous,
                    $"Tamanho-base da banda no nível {level} ({baseSize}) deve ser >= banda anterior ({previous}).");
                previous = baseSize;
            }
        }

        [Test]
        public void ResolveBandBaseMapSize_StoneIsAroundSixty_DeepVoidIsAroundNinety()
        {
            var stone = CaveBiomeLayoutProfile.ResolveBandBaseMapSize(1);
            var deep = CaveBiomeLayoutProfile.ResolveBandBaseMapSize(71);
            var voidBand = CaveBiomeLayoutProfile.ResolveBandBaseMapSize(101);

            Assert.That(stone, Is.EqualTo(60), "Banda Stone (1) deve ter base ~60.");
            Assert.That(deep, Is.EqualTo(90), "Banda Deep (6) deve ter base ~90.");
            Assert.That(voidBand, Is.EqualTo(90), "Banda Void (7) deve ter base ~90.");
            Assert.Greater(deep, stone, "Profundidade deve ter mapa maior que a superfície.");
        }

        [Test]
        public void ResolveMapSize_IsDeterministic_ForSameSeedAndLevel()
        {
            for (var level = 1; level <= 101; level += 5)
            {
                var first = CaveBiomeLayoutProfile.ResolveMapSize(WorldSeed, RunSeed, level);
                var second = CaveBiomeLayoutProfile.ResolveMapSize(WorldSeed, RunSeed, level);
                Assert.AreEqual(first, second,
                    $"Nível {level}: o tamanho resolvido deve ser idêntico para o mesmo (worldSeed, runSeed, caveLevel).");
            }
        }

        [Test]
        public void ResolveMapSize_PerBandRange_NeverDecreasesAcrossBands_ForAnyBucket()
        {
            // Para cada banda, o MENOR tamanho possível (bucket small) ainda deve ser >= o MAIOR
            // tamanho possível (bucket large) da banda anterior? Não — buckets se sobrepõem por design.
            // O invariante real: o tamanho-BASE da banda nunca decresce (coberto acima) e qualquer
            // tamanho da banda fica dentro de [base-delta, base+delta].
            foreach (var level in BandRepresentativeLevels)
            {
                var baseSize = CaveBiomeLayoutProfile.ResolveBandBaseMapSize(level);
                var observed = new HashSet<int>();
                // Varia o seed para cobrir os três buckets (small/base/large).
                for (var s = 0; s < 300; s++)
                {
                    observed.Add(CaveBiomeLayoutProfile.ResolveMapSize($"w_{s}", $"r_{s}", level));
                }

                foreach (var size in observed)
                {
                    Assert.GreaterOrEqual(size, baseSize - CaveBiomeLayoutProfile.SeedVariationDelta,
                        $"Nível {level}: tamanho {size} abaixo de base-delta.");
                    Assert.LessOrEqual(size, baseSize + CaveBiomeLayoutProfile.SeedVariationDelta,
                        $"Nível {level}: tamanho {size} acima de base+delta.");
                }
            }
        }

        [Test]
        public void ResolveMapSize_PreservesSeedVariationBuckets_WithinBand()
        {
            // Dentro de uma única banda, variar o seed deve produzir os três buckets distintos
            // (small = base-delta, base, large = base+delta) — a variação do fable_09 é preservada.
            const int level = 26; // banda Ice
            var baseSize = CaveBiomeLayoutProfile.ResolveBandBaseMapSize(level);
            var sizes = new HashSet<int>();
            for (var s = 0; s < 500; s++)
            {
                sizes.Add(CaveBiomeLayoutProfile.ResolveMapSize($"world_{s}", $"run_{s}", level));
            }

            Assert.Contains(baseSize, new List<int>(sizes), "Bucket base ausente.");
            Assert.Contains(baseSize - CaveBiomeLayoutProfile.SeedVariationDelta, new List<int>(sizes),
                "Bucket small (base-delta) ausente — variação por seed não preservada.");
            Assert.Contains(baseSize + CaveBiomeLayoutProfile.SeedVariationDelta, new List<int>(sizes),
                "Bucket large (base+delta) ausente — variação por seed não preservada.");
            Assert.AreEqual(3, sizes.Count, "Esperados exatamente 3 buckets de tamanho na banda.");
        }
    }
}
