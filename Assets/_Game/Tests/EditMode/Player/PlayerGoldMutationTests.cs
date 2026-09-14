using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.EditMode.Player
{
    public sealed class PlayerGoldMutationTests
    {
        private GameObject _host;
        private PlayerManager _player;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("PlayerGoldMutationTests");
            _player = _host.AddComponent<PlayerManager>();
            _player.SetGold(20);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        [Test]
        public void StagedDebit_PublishesOnlyAfterCommit()
        {
            var observed = 0;
            var observedTotal = -1;
            using (GameEventBus.Subscribe<GoldChangedEvent>(evt =>
            {
                observed++;
                observedTotal = evt.NewTotal;
            }))
            {
                Assert.That(_player.TryStageGoldMutation(-17, out var token), Is.True);
                Assert.That(_player.CurrentGold, Is.EqualTo(3));
                Assert.That(observed, Is.Zero);
                Assert.That(_player.TryCommitGoldMutation(token), Is.True);
                Assert.That(observed, Is.EqualTo(1));
                Assert.That(observedTotal, Is.EqualTo(3));
                Assert.That(_player.TryCommitGoldMutation(token), Is.False);
                Assert.That(observed, Is.EqualTo(1));
            }
        }

        [Test]
        public void Rollback_RestoresGoldWithoutEvent()
        {
            var observed = 0;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ => observed++))
            {
                Assert.That(_player.TryStageGoldMutation(-17, out var token), Is.True);
                Assert.That(_player.TryRollbackGoldMutation(token), Is.True);
                Assert.That(_player.CurrentGold, Is.EqualTo(20));
                Assert.That(observed, Is.Zero);
                Assert.That(_player.TryRollbackGoldMutation(token), Is.False);
            }
        }

        [Test]
        public void CheckedCredit_RejectsOverflowWithoutMutationOrEvent()
        {
            _player.SetGold(int.MaxValue);
            var observed = 0;
            using (GameEventBus.Subscribe<GoldChangedEvent>(_ => observed++))
            {
                Assert.That(_player.CanCreditGoldChecked(1), Is.False);
                Assert.That(_player.TryCreditGoldChecked(1), Is.False);
                Assert.That(_player.TryStageGoldMutation(1, out _), Is.False);
                Assert.That(_player.CurrentGold, Is.EqualTo(int.MaxValue));
                Assert.That(observed, Is.Zero);
            }
        }

        [Test]
        public void RestoreAndInitialize_CannotInvalidatePendingToken()
        {
            Assert.That(_player.TryStageGoldMutation(-5, out var token), Is.True);
            LogAssert.Expect(LogType.Warning, "Cannot restore player state while an atomic gold mutation is pending.");
            _player.RestoreState(100, 50, 99);
            LogAssert.Expect(LogType.Warning, "Cannot initialize player state while an atomic gold mutation is pending.");
            _player.Initialize(null);

            Assert.That(_player.CurrentGold, Is.EqualTo(15));
            Assert.That(_player.IsInitialized, Is.False);
            Assert.That(_player.TryRollbackGoldMutation(token), Is.True);
            Assert.That(_player.CurrentGold, Is.EqualTo(20));
            Assert.That(_player.TryStageGoldMutation(-1, out var later), Is.True);
            Assert.That(_player.TryRollbackGoldMutation(later), Is.True);
        }
    }
}
