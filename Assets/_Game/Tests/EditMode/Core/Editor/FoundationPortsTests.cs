using CindarsHope.Core;
using CindarsHope.Core.Random;
using CindarsHope.Foundation.Time;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Core
{
    public class FoundationPortsTests
    {
        [Test]
        public void SeededGameplayAndWorldSources_AreDeterministicAndIndependent()
        {
            var gameplayA = new SeededGameplayRandomSource(1234);
            var gameplayB = new SeededGameplayRandomSource(1234);
            var world = new SeededWorldRandomSource(1234);

            Assert.That(gameplayA.NextFloat(), Is.EqualTo(gameplayB.NextFloat()));
            Assert.That(gameplayA.NextInt(0, 100), Is.EqualTo(gameplayB.NextInt(0, 100)));
            Assert.That(world.NextInt(0, 100), Is.InRange(0, 99));
        }

        [Test]
        public void GameTimeManager_ImplementsClockPortWithoutChangingDefaults()
        {
            var gameObject = new GameObject("ClockPortTest");
            var manager = gameObject.AddComponent<GameTimeManager>();
            IGameClock clock = manager;

            Assert.That(clock.CurrentDay, Is.EqualTo(1));
            Assert.That(clock.CurrentHourOfDay, Is.EqualTo(6));
            Assert.That(clock.IsDaytime, Is.True);

            Object.DestroyImmediate(gameObject);
        }
    }
}
