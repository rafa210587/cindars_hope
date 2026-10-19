using CindarsHope.Combat;
using CindarsHope.Enemy;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Enemy
{
    public sealed class EnemyMovementStrategyRegistryTests
    {
        [Test]
        public void TryMove_RoutesRegisteredMovementAndSpeed()
        {
            var registry = new EnemyMovementStrategyRegistry();
            var context = new FakeMovementContext();
            float receivedSpeed = 0f;
            registry.Register(EnemyMovementType.CircleStrafe,
                new DelegateEnemyMovementStrategy((_, speed) => receivedSpeed = speed));

            Assert.That(registry.TryMove(context, EnemyMovementType.CircleStrafe, 3.25f), Is.True);
            Assert.That(receivedSpeed, Is.EqualTo(3.25f));
        }

        [Test]
        public void TryMove_LeavesUnregisteredLegacyMovementForSharedPipeline()
        {
            var registry = new EnemyMovementStrategyRegistry();
            var context = new FakeMovementContext();

            Assert.That(registry.TryMove(context, EnemyMovementType.GroundChase, 2f), Is.False);
        }

        [Test]
        public void Register_CanMapMultipleTypesToSameStrategy()
        {
            var registry = new EnemyMovementStrategyRegistry();
            var context = new FakeMovementContext();
            int calls = 0;
            var strategy = new DelegateEnemyMovementStrategy((_, __) => calls++);
            registry.Register(EnemyMovementType.CircleStrafe, strategy);
            registry.Register(EnemyMovementType.FloatingOrbit, strategy);

            registry.TryMove(context, EnemyMovementType.CircleStrafe, 1f);
            registry.TryMove(context, EnemyMovementType.FloatingOrbit, 1f);

            Assert.That(calls, Is.EqualTo(2));
        }

        private sealed class FakeMovementContext : IEnemyMovementStrategyContext
        {
            public void MoveOrbit(float speed) { }
            public void MoveFloatingSlow(float speed) { }
            public void MoveChargeLine(float speed) { }
            public void MoveRetreatAndCall(float speed) { }
            public void MoveRetreat(float speed) { }
            public void MoveMimicAmbush(float speed) { }
            public void MoveAnchoredChase(float speed) { }
            public void MovePackFlanker(float speed) { }
        }
    }
}
