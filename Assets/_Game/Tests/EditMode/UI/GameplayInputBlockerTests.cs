using CindarsHope.Gameplay.Input;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.UI
{
    public sealed class GameplayInputBlockerTests
    {
        [SetUp]
        [TearDown]
        public void Reset() => GameplayInputBlocker.Reset();

        [Test]
        public void Acquire_BlocksUntilLeaseIsDisposed()
        {
            var lease = GameplayInputBlocker.Acquire(GameplayInputBlockReason.FarmActionMenu);

            Assert.That(GameplayInputBlocker.IsBlocked, Is.True);
            Assert.That(GameplayInputBlocker.IsBlockedBy(GameplayInputBlockReason.FarmActionMenu), Is.True);

            lease.Dispose();
            Assert.That(GameplayInputBlocker.IsBlocked, Is.False);
        }

        [Test]
        public void NestedLeases_ReleaseOnlyTheirOwnClaim()
        {
            var first = GameplayInputBlocker.Acquire(GameplayInputBlockReason.FarmActionMenu);
            var second = GameplayInputBlocker.Acquire(GameplayInputBlockReason.FarmActionMenu);

            first.Dispose();
            first.Dispose();
            Assert.That(GameplayInputBlocker.IsBlocked, Is.True);

            second.Dispose();
            Assert.That(GameplayInputBlocker.IsBlocked, Is.False);
        }

        [Test]
        public void Reset_ClearsOutstandingClaimsAndLateDisposeIsSafe()
        {
            var lease = GameplayInputBlocker.Acquire(GameplayInputBlockReason.FarmActionMenu);

            GameplayInputBlocker.Reset();
            lease.Dispose();

            Assert.That(GameplayInputBlocker.IsBlocked, Is.False);
        }
    }
}
