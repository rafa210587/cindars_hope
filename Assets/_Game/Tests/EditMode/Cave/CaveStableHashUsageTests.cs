using NUnit.Framework;
using CindarsHope.Cave.Generation;
using CindarsHope.Enemy;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// spec_codex_09 — golden-value tests garantindo que o hash usado para semear RNG de conteudo
    /// estavel da cave (CaveEnemySpawner, CaveResourceNodeMaterializer, EnemyActionExecution) e
    /// determinístico entre execucoes (FNV-1a via CaveLayoutStableHash.Compute), nunca
    /// string.GetHashCode(). Valores golden fixos — nao recomputados dinamicamente — para provar
    /// que o mesmo input sempre produz o mesmo hash, cross-run (cave-stable-run / ADR-0005).
    /// </summary>
    public class CaveStableHashUsageTests
    {
        // ── Golden values fixos (FNV-1a 32-bit, offset 2166136261, prime 16777619) ──────────────

        [Test]
        public void Compute_EnemySeedString_MatchesGoldenValue()
        {
            // Formato usado por CaveEnemySpawner.SpawnEnemiesForLevel:
            // "{world}_{run}_{level}_enemies"
            int result = CaveLayoutStableHash.Compute("world1_run1_1_enemies");
            Assert.AreEqual(1016885348, result);
        }

        [Test]
        public void Compute_ResourceSpawnSeedString_MatchesGoldenValue()
        {
            // Formato usado por CaveResourceNodeMaterializer.MaterializeResourceNodes:
            // "{world}_{run}_{level}_resource_spawn"
            int result = CaveLayoutStableHash.Compute("world1_run1_1_resource_spawn");
            Assert.AreEqual(1774218272, result);
        }

        [Test]
        public void Compute_ResourceSelectionSeedString_MatchesGoldenValue()
        {
            // Formato usado por CaveResourceNodeMaterializer.SelectResourceNodeData:
            // "{world}_{run}_{level}_resources_{spawnIndex}_{x}_{y}"
            int result = CaveLayoutStableHash.Compute("world1_run1_1_resources_0_5_5");
            Assert.AreEqual(-730841378, result);
        }

        [Test]
        public void Compute_EmptyString_MatchesGoldenValue()
        {
            int result = CaveLayoutStableHash.Compute(string.Empty);
            Assert.AreEqual(-2128831035, result);
        }

        // ── Determinismo cross-call (mesmo input -> mesmo hash, execuções repetidas) ────────────

        [Test]
        public void Compute_SameInput_ProducesSameHash_AcrossRepeatedCalls()
        {
            const string seedString = "world1_run1_1_enemies";

            int first = CaveLayoutStableHash.Compute(seedString);
            int second = CaveLayoutStableHash.Compute(seedString);
            int third = CaveLayoutStableHash.Compute(seedString);

            Assert.AreEqual(first, second);
            Assert.AreEqual(second, third);
            Assert.AreEqual(1016885348, first);
        }

        [Test]
        public void Compute_DifferentLevelInSeedString_ProducesDifferentHash()
        {
            // Confirma que o hash reage à composição da seed string (nível diferente = hash diferente),
            // preservando a propriedade de que a stable-run depende do CaveLevel na string.
            int level1 = CaveLayoutStableHash.Compute("world1_run1_1_enemies");
            int level2 = CaveLayoutStableHash.Compute("world1_run1_2_enemies");

            Assert.AreNotEqual(level1, level2);
        }

        // ── EnemyActionExecution.DeriveSummonSeed (combinação h*31+x preservada) ────────────────

        [Test]
        public void DeriveSummonSeed_SameInputs_ProducesSameSeed_AcrossRepeatedCalls()
        {
            int first = EnemyActionExecution.DeriveSummonSeed("run1", 3, "enemy_goblin_1");
            int second = EnemyActionExecution.DeriveSummonSeed("run1", 3, "enemy_goblin_1");
            int third = EnemyActionExecution.DeriveSummonSeed("run1", 3, "enemy_goblin_1");

            Assert.AreEqual(first, second);
            Assert.AreEqual(second, third);
        }

        [Test]
        public void DeriveSummonSeed_MatchesGoldenValue_ComputedFromFnv1aCombination()
        {
            // h = 17
            // h = h*31 + Compute("run1")           = 17*31 + 1546868001
            // h = h*31 + 3
            // h = h*31 + Compute("enemy_goblin_1") = ... + 94611823
            unchecked
            {
                int h = 17;
                h = h * 31 + 1546868001; // CaveLayoutStableHash.Compute("run1")
                h = h * 31 + 3;
                h = h * 31 + 94611823; // CaveLayoutStableHash.Compute("enemy_goblin_1")

                int result = EnemyActionExecution.DeriveSummonSeed("run1", 3, "enemy_goblin_1");
                Assert.AreEqual(h, result);
            }
        }

        [Test]
        public void DeriveSummonSeed_DifferentSummonerId_ProducesDifferentSeed()
        {
            int seedA = EnemyActionExecution.DeriveSummonSeed("run1", 3, "enemy_goblin_1");
            int seedB = EnemyActionExecution.DeriveSummonSeed("run1", 3, "enemy_goblin_2");

            Assert.AreNotEqual(seedA, seedB);
        }

        [Test]
        public void DeriveSummonSeed_NullCaveRunSeedOrSummonerId_DoesNotThrow()
        {
            // caveRunSeed/summonerId nulos caem em string.Empty (comportamento preservado da versão
            // anterior baseada em string.GetHashCode()).
            Assert.DoesNotThrow(() => EnemyActionExecution.DeriveSummonSeed(null, 1, null));
        }
    }
}
