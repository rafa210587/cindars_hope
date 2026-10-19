using NUnit.Framework;
using CindarsHope.Combat;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// Pure tests for the shared enemy scale math (boss scale resolution + collider radius per size class).
    /// Locks the two behaviours this wiring change introduced: bosses honour their baked per-boss VisualScale
    /// (a Gargantuan boss is NOT flattened to the global ceiling), and the legacy spawner sizes colliders by
    /// natural size class instead of a flat 0.4.
    /// </summary>
    [TestFixture]
    public class EnemyScaleResolverTests
    {
        private const float Delta = 0.0001f;

        [Test]
        public void ResolveBossScale_LargeOverride_IsNotClampedToConfigMax()
        {
            // Gargantuan boss baked at 7.5; config ceiling is 3.0 — the override must survive.
            var scale = EnemyScaleResolver.ResolveBossScale(7.5f, 2.5f, 2f, 3f);
            Assert.AreEqual(7.5f, scale, Delta);
        }

        [Test]
        public void ResolveBossScale_MediumBossOverride_IsHonoured()
        {
            var scale = EnemyScaleResolver.ResolveBossScale(2.5f, 2.5f, 2f, 3f);
            Assert.AreEqual(2.5f, scale, Delta);
        }

        [Test]
        public void ResolveBossScale_DefaultData_FallsBackToClampedConfig()
        {
            // Data still at the EnemyDataSO default (1.0) -> use config, clamped into [min,max].
            var scale = EnemyScaleResolver.ResolveBossScale(1f, 5f, 2f, 3f);
            Assert.AreEqual(3f, scale, Delta);
        }

        [Test]
        public void ResolveBossScale_OverrideBelowFloor_IsFlooredToMin()
        {
            // A real override (>1.01) but below the boss floor is raised to bossMin.
            var scale = EnemyScaleResolver.ResolveBossScale(1.5f, 2.5f, 2f, 3f);
            Assert.AreEqual(2f, scale, Delta);
        }

        [TestCase(BestiarySizeClass.Tiny, 0.25f)]
        [TestCase(BestiarySizeClass.Small, 0.35f)]
        [TestCase(BestiarySizeClass.Medium, 0.45f)]
        [TestCase(BestiarySizeClass.Large, 0.65f)]
        [TestCase(BestiarySizeClass.Huge, 0.95f)]
        [TestCase(BestiarySizeClass.Gargantuan, 1.2f)]
        public void ColliderRadiusFor_MapsEachSizeClass(BestiarySizeClass size, float expected)
        {
            Assert.AreEqual(expected, EnemyScaleResolver.ColliderRadiusFor(size), Delta);
        }
    }
}
