using NUnit.Framework;
using CindarsHope.Cave.Runtime;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// Depth-scaled enemy density: deterministic per seed, never below the floor minimum,
    /// grows with cave level, and respects the hard cap.
    /// </summary>
    [TestFixture]
    public class CaveEnemySpawnDensityTests
    {
        [Test]
        public void TargetCount_IsDeterministic()
        {
            for (int level = 1; level <= 101; level++)
            {
                int seed = CaveEnemySpawnPlanner.StableHash($"seed|{level}");
                int first = CaveEnemySpawnPlanner.ResolveTargetEnemyCount(seed, 32, level);
                int second = CaveEnemySpawnPlanner.ResolveTargetEnemyCount(seed, 32, level);
                Assert.AreEqual(first, second);
            }
        }

        [Test]
        public void TargetCount_NeverBelowMinimum()
        {
            for (int level = 1; level <= 101; level++)
            {
                int seed = CaveEnemySpawnPlanner.StableHash($"seed|{level}");
                int count = CaveEnemySpawnPlanner.ResolveTargetEnemyCount(seed, 32, level);
                Assert.GreaterOrEqual(count, 16, $"Level {level} resolved {count} enemies, below floor minimum.");
            }
        }

        [Test]
        public void TargetCount_NeverAboveHardCap()
        {
            for (int level = 1; level <= 300; level++)
            {
                int seed = CaveEnemySpawnPlanner.StableHash($"seed|{level}");
                int count = CaveEnemySpawnPlanner.ResolveTargetEnemyCount(seed, 32, level);
                Assert.LessOrEqual(count, 44, $"Level {level} resolved {count} enemies, above hard cap.");
            }
        }

        [Test]
        public void TargetCount_GrowsWithDepthOnAverage()
        {
            float shallowAverage = AverageCount(1, 10);
            float deepAverage = AverageCount(90, 101);
            Assert.Greater(deepAverage, shallowAverage,
                $"Deep levels ({deepAverage:F1}) should average more enemies than shallow levels ({shallowAverage:F1}).");
        }

        [Test]
        public void TargetCount_TreatsSerializedSceneValueAsBase()
        {
            // Older scenes serialize _maxEnemiesPerLevel = 24; density must still scale.
            int seed = CaveEnemySpawnPlanner.StableHash("seed|legacy");
            int legacyCount = CaveEnemySpawnPlanner.ResolveTargetEnemyCount(seed, 24, 80);
            Assert.GreaterOrEqual(legacyCount, 16);
            Assert.LessOrEqual(legacyCount, 44);
        }

        private static float AverageCount(int fromLevel, int toLevel)
        {
            float total = 0f;
            int samples = 0;
            for (int level = fromLevel; level <= toLevel; level++)
            {
                // Average across multiple seeds to smooth per-seed variation.
                for (int seedIndex = 0; seedIndex < 20; seedIndex++)
                {
                    int seed = CaveEnemySpawnPlanner.StableHash($"seed_{seedIndex}|{level}");
                    total += CaveEnemySpawnPlanner.ResolveTargetEnemyCount(seed, 32, level);
                    samples++;
                }
            }

            return total / samples;
        }
    }
}
