using NUnit.Framework;
using UnityEngine;
using CindarsHope.Combat;
using CindarsHope.Enemy;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_24 — pure decision rules for the 12 new enemy moves (extracted from EnemyBrain so they
    /// are EditMode-testable): pack engagement (CA-2), mimic ambush + charge-line telegraph (CA-4),
    /// anchor/arena leash, and the windup/recovery faixas by role.
    /// </summary>
    [TestFixture]
    public class EnemyMovesTests
    {
        // ── Pack coordination (CA-2) ────────────────────────────────────────────────────────────

        [Test]
        public void Flanker_EngagesOnlyWithLivingLeaderInRange()
        {
            const float awareness = 8f;
            Assert.IsTrue(EnemyMoveLogic.ShouldFlankerEngage(true, 5f, awareness), "Leader alive + in range -> engage.");
            Assert.IsFalse(EnemyMoveLogic.ShouldFlankerEngage(false, 5f, awareness), "Dead leader -> do not engage.");
            Assert.IsFalse(EnemyMoveLogic.ShouldFlankerEngage(true, 12f, awareness), "Leader out of range -> do not engage.");
        }

        [Test]
        public void Flanker_FallsBackToChase_AfterTimeoutWithoutLeader()
        {
            Assert.IsFalse(EnemyMoveLogic.ShouldFlankerFallbackToChase(0.5f), "Still within wait window -> hold.");
            Assert.IsTrue(
                EnemyMoveLogic.ShouldFlankerFallbackToChase(EnemyMoveLogic.FlankerNoLeaderFallbackSeconds),
                "At the timeout the flanker must act as GroundChase (no deadlock).");
            Assert.IsTrue(EnemyMoveLogic.ShouldFlankerFallbackToChase(5f), "Well past the timeout -> chase.");
        }

        [Test]
        public void LeaderDeath_SwitchesFlankerToRetreatAndCall()
        {
            Assert.AreEqual(
                EnemyMovementType.PackFlanker,
                EnemyMoveLogic.ResolveFlankerMoveOnLeaderState(true, EnemyMovementType.PackFlanker),
                "Leader alive -> stay a flanker.");
            Assert.AreEqual(
                EnemyMovementType.RetreatAndCall,
                EnemyMoveLogic.ResolveFlankerMoveOnLeaderState(false, EnemyMovementType.PackFlanker),
                "Leader dead -> the bando retreats and calls for help (CA-2).");
            // Non-flankers are unaffected by leader state.
            Assert.AreEqual(
                EnemyMovementType.GroundChase,
                EnemyMoveLogic.ResolveFlankerMoveOnLeaderState(false, EnemyMovementType.GroundChase));
        }

        // ── Mimic ambush (CA-4) ─────────────────────────────────────────────────────────────────

        [Test]
        public void Mimic_StaysDisguisedUntilPlayerWithinTwoTiles()
        {
            Assert.IsFalse(EnemyMoveLogic.ShouldMimicActivate(5f), "Far player -> stay disguised.");
            Assert.IsFalse(EnemyMoveLogic.ShouldMimicActivate(EnemyMoveLogic.MimicActivationTiles), "Exactly at 2 tiles is the boundary -> still disguised.");
            Assert.IsTrue(EnemyMoveLogic.ShouldMimicActivate(1.9f), "Within 2 tiles -> spring the ambush.");
            Assert.IsTrue(EnemyMoveLogic.ShouldMimicActivate(0f), "On top of the mimic -> activate.");
        }

        // ── Charge line (CA-4) ──────────────────────────────────────────────────────────────────

        [Test]
        public void ChargeLine_OnlyFiresAfterTelegraph()
        {
            const float duration = 0.5f;
            Assert.IsFalse(EnemyMoveLogic.ShouldChargeLineFire(false, 1f, duration, true), "No telegraph -> never charge (CA-4).");
            Assert.IsFalse(EnemyMoveLogic.ShouldChargeLineFire(true, 0.2f, duration, true), "Telegraph not finished -> hold.");
            Assert.IsFalse(EnemyMoveLogic.ShouldChargeLineFire(true, 1f, duration, false), "On cooldown -> do not charge.");
            Assert.IsTrue(EnemyMoveLogic.ShouldChargeLineFire(true, 0.5f, duration, true), "Telegraph done + ready -> charge.");
        }

        [Test]
        public void ChargeVelocity_IsStraightLineTowardLockedDirection()
        {
            Vector2 dir = new Vector2(1f, 0f);
            Vector2 v = EnemyMoveLogic.ResolveChargeVelocity(dir, 6f);
            Assert.AreEqual(6f, v.x, 1e-4, "Charges straight along the locked X direction.");
            Assert.AreEqual(0f, v.y, 1e-4, "No vertical drift (no homing).");

            Assert.AreEqual(Vector2.zero, EnemyMoveLogic.ResolveChargeVelocity(Vector2.zero, 6f), "Zero direction -> no charge.");
        }

        // ── Anchor / arena leash ────────────────────────────────────────────────────────────────

        [Test]
        public void ProtectAnchor_DetectsBeyondLeash()
        {
            Vector2 anchor = Vector2.zero;
            Assert.IsFalse(EnemyMoveLogic.IsBeyondAnchorLeash(new Vector2(3f, 0f), anchor, 4f), "Inside leash.");
            Assert.IsTrue(EnemyMoveLogic.IsBeyondAnchorLeash(new Vector2(5f, 0f), anchor, 4f), "Outside leash radius.");
        }

        [Test]
        public void ClampToAnchor_KeepsDestinationWithinLeash()
        {
            Vector2 anchor = new Vector2(2f, 2f);
            // Desired far outside the leash gets clamped onto the leash circle.
            Vector2 clamped = EnemyMoveLogic.ClampToAnchor(new Vector2(12f, 2f), anchor, 4f);
            Assert.AreEqual(6f, clamped.x, 1e-3, "Clamped to anchor.x + radius.");
            Assert.AreEqual(2f, clamped.y, 1e-3);

            // Desired inside the leash is returned unchanged.
            Vector2 inside = EnemyMoveLogic.ClampToAnchor(new Vector2(3f, 2f), anchor, 4f);
            Assert.AreEqual(new Vector2(3f, 2f), inside);
        }

        // ── Move classification ─────────────────────────────────────────────────────────────────

        [Test]
        public void MoveClassification_FloatingAndBossPrimitives()
        {
            Assert.IsTrue(EnemyMoveLogic.IsFloating(EnemyMovementType.FloatingSlow));
            Assert.IsTrue(EnemyMoveLogic.IsFloating(EnemyMovementType.FloatingOrbit));
            Assert.IsFalse(EnemyMoveLogic.IsFloating(EnemyMovementType.GroundChase));

            Assert.IsTrue(EnemyMoveLogic.IsBossPrimitive(EnemyMovementType.BossArenaControl));
            Assert.IsTrue(EnemyMoveLogic.IsBossPrimitive(EnemyMovementType.BossPhaseShift));
            Assert.IsFalse(EnemyMoveLogic.IsBossPrimitive(EnemyMovementType.ChargeLine));

            Assert.IsTrue(EnemyMoveLogic.IsOrbitingMove(EnemyMovementType.CircleStrafe));
            Assert.IsTrue(EnemyMoveLogic.IsOrbitingMove(EnemyMovementType.FloatingOrbit));
            Assert.IsFalse(EnemyMoveLogic.IsOrbitingMove(EnemyMovementType.TankSlowPush));
        }

        // ── Windup/recovery faixas by role (DECISOES §7-8) ──────────────────────────────────────

        [Test]
        public void WindupRecover_FollowCanonicalFaixasByRole()
        {
            // common 0.5/0.5, elite 0.7/0.6, boss 0.9/0.8
            Assert.AreEqual(0.5f, EnemyMoveLogic.ResolveWindupSeconds(false, false), 1e-4);
            Assert.AreEqual(0.5f, EnemyMoveLogic.ResolveRecoverSeconds(false, false), 1e-4);
            Assert.AreEqual(0.7f, EnemyMoveLogic.ResolveWindupSeconds(false, true), 1e-4);
            Assert.AreEqual(0.6f, EnemyMoveLogic.ResolveRecoverSeconds(false, true), 1e-4);
            Assert.AreEqual(0.9f, EnemyMoveLogic.ResolveWindupSeconds(true, false), 1e-4);
            Assert.AreEqual(0.8f, EnemyMoveLogic.ResolveRecoverSeconds(true, false), 1e-4);
            // Boss takes precedence over elite when both flags are set.
            Assert.AreEqual(0.9f, EnemyMoveLogic.ResolveWindupSeconds(true, true), 1e-4);
        }
    }
}
