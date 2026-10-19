using CindarsHope.Core.Time;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Core
{
    public class GameTimeScaleCoordinatorTests
    {
        [SetUp]
        public void SetUp()
        {
            GameTimeScaleCoordinator.Reset();
            Time.timeScale = 0.75f;
        }

        [TearDown]
        public void TearDown()
        {
            GameTimeScaleCoordinator.Reset();
            Time.timeScale = 1f;
        }

        [Test]
        public void OverlappingTokens_RestoreOnlyAfterLastOwnerReleases()
        {
            var first = GameTimeScaleCoordinator.AcquirePause();
            var second = GameTimeScaleCoordinator.AcquirePause();

            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(GameTimeScaleCoordinator.ActiveTokenCount, Is.EqualTo(2));

            first.Dispose();
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(GameTimeScaleCoordinator.IsPaused, Is.True);

            second.Dispose();
            Assert.That(Time.timeScale, Is.EqualTo(0.75f));
            Assert.That(GameTimeScaleCoordinator.IsPaused, Is.False);
        }

        [Test]
        public void DisposingTokenTwice_IsIdempotent()
        {
            var token = GameTimeScaleCoordinator.AcquirePause();
            token.Dispose();
            token.Dispose();

            Assert.That(GameTimeScaleCoordinator.ActiveTokenCount, Is.Zero);
            Assert.That(Time.timeScale, Is.EqualTo(0.75f));
        }
    }
}
