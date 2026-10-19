using NUnit.Framework;
using UnityEngine;
using CindarsHope.Enemy;
using CindarsHope.Combat;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_04 CA-1 — threat memory is a pure, deterministic rule: an enemy keeps the target in
    /// memory for ThreatMemorySeconds after it leaves range, so it cannot be kited for free at the
    /// leash edge. Time is passed explicitly (no UnityEngine.Time), so the rule is fully testable.
    /// </summary>
    [TestFixture]
    public class EnemyThreatStateTests
    {
        [Test]
        public void NoThreat_BeforeAnyDetection()
        {
            var state = new EnemyThreatState(4f);
            Assert.IsFalse(state.HasThreat(0f), "Should have no threat before ever seeing the target.");
            Assert.IsFalse(state.HasEverSeenTarget);
        }

        [Test]
        public void HasThreat_WhileWithinMemoryWindow()
        {
            var state = new EnemyThreatState(4f);
            state.NoticeTarget(new Vector2(3f, 5f), now: 10f);

            Assert.IsTrue(state.HasThreat(10f), "Threat active immediately after noticing.");
            Assert.IsTrue(state.HasThreat(13.9f), "Threat still active just before the window closes.");
            Assert.IsTrue(state.HasThreat(14f), "Threat active exactly at the window boundary.");
        }

        [Test]
        public void ThreatExpires_AfterMemoryWindow()
        {
            var state = new EnemyThreatState(4f);
            state.NoticeTarget(new Vector2(1f, 1f), now: 10f);

            Assert.IsFalse(state.HasThreat(14.01f), "Threat must expire once the window elapses.");
            Assert.AreEqual(0f, state.RemainingThreatSeconds(14.01f));
        }

        [Test]
        public void NoticeTarget_RefreshesWindowAndLastKnownPosition()
        {
            var state = new EnemyThreatState(4f);
            state.NoticeTarget(new Vector2(1f, 1f), now: 10f);
            state.NoticeTarget(new Vector2(8f, 2f), now: 13f);

            // Window was refreshed at t=13, so it now stays active until t=17.
            Assert.IsTrue(state.HasThreat(16.9f));
            Assert.AreEqual(new Vector2(8f, 2f), state.LastKnownPosition);
        }

        [Test]
        public void Clear_ForgetsTarget()
        {
            var state = new EnemyThreatState(4f);
            state.NoticeTarget(Vector2.one, now: 5f);
            state.Clear();

            Assert.IsFalse(state.HasThreat(5f), "Clear must drop the threat entirely (collective reset).");
            Assert.IsFalse(state.HasEverSeenTarget);
        }

        [Test]
        public void RemainingThreatSeconds_CountsDown()
        {
            var state = new EnemyThreatState(4f);
            state.NoticeTarget(Vector2.zero, now: 0f);

            Assert.AreEqual(4f, state.RemainingThreatSeconds(0f), 1e-4);
            Assert.AreEqual(1.5f, state.RemainingThreatSeconds(2.5f), 1e-4);
            Assert.AreEqual(0f, state.RemainingThreatSeconds(10f), 1e-4);
        }

        [Test]
        public void ResolveMemorySeconds_PerMovementType()
        {
            Assert.AreEqual(EnemyThreatState.SwarmMemorySeconds,
                EnemyThreatState.ResolveMemorySeconds(EnemyMovementType.SwarmErratic),
                "Swarm forgets fastest (2s).");
            Assert.AreEqual(EnemyThreatState.GuardMemorySeconds,
                EnemyThreatState.ResolveMemorySeconds(EnemyMovementType.GuardStationary),
                "Guards hold the longest grudge (6s).");
            Assert.AreEqual(EnemyThreatState.DefaultMemorySeconds,
                EnemyThreatState.ResolveMemorySeconds(EnemyMovementType.GroundChase),
                "Everything else uses the 4s baseline.");
            Assert.AreEqual(EnemyThreatState.DefaultMemorySeconds,
                EnemyThreatState.ResolveMemorySeconds(EnemyMovementType.KiteRanged));
        }

        [Test]
        public void SwarmWindow_IsShorterThanGuardWindow()
        {
            var swarm = new EnemyThreatState(EnemyThreatState.SwarmMemorySeconds);
            var guard = new EnemyThreatState(EnemyThreatState.GuardMemorySeconds);
            swarm.NoticeTarget(Vector2.zero, now: 0f);
            guard.NoticeTarget(Vector2.zero, now: 0f);

            // At t=3 the swarm has forgotten (2s window) but the guard still remembers (6s window).
            Assert.IsFalse(swarm.HasThreat(3f), "Swarm window (2s) should already be closed at t=3.");
            Assert.IsTrue(guard.HasThreat(3f), "Guard window (6s) should still be open at t=3.");
        }
    }
}
