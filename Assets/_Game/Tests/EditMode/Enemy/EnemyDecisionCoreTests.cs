using NUnit.Framework;
using CindarsHope.Combat;
using CindarsHope.Enemy;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Enemy
{
    public class EnemyDecisionCoreTests
    {
        private static EnemyDecisionInput DefaultInput(EnemyBrainState state = EnemyBrainState.Idle) => new EnemyDecisionInput(
            distanceToTarget: 5f,
            detectionRange: 10f,
            leashRange: 30f,
            currentTime: 1f,
            burrowEmergeDistance: 1.4f,
            lowHealthRetreatThreshold: 0.25f,
            retreatEndTime: 0f,
            forcedRetreatUntil: 0f,
            stunUntil: 0f,
            currentHpFraction: 0.8f,
            currentState: state,
            movementType: EnemyMovementType.GroundChase,
            primaryRole: EnemyRole.Chaser,
            hasActiveThreat: false,
            targetIsValid: true,
            healthIsValid: true
        );

        [Test]
        public void Idle_PlayerInRange_TransitionsToAlert()
        {
            var output = EnemyDecisionCore.Evaluate(DefaultInput(EnemyBrainState.Idle));
            Assert.AreEqual(EnemyBrainState.Alert, output.NextState);
        }

        [Test]
        public void Idle_PlayerOutOfRange_TransitionsToPatrol()
        {
            var input = new EnemyDecisionInput(
                distanceToTarget: 15f, detectionRange: 10f, leashRange: 30f,
                currentTime: 1f, burrowEmergeDistance: 1.4f, lowHealthRetreatThreshold: 0.25f,
                retreatEndTime: 0f, forcedRetreatUntil: 0f, stunUntil: 0f,
                currentHpFraction: 1f, currentState: EnemyBrainState.Idle,
                movementType: EnemyMovementType.GroundChase, primaryRole: EnemyRole.Chaser,
                hasActiveThreat: false, targetIsValid: true, healthIsValid: true
            );
            var output = EnemyDecisionCore.Evaluate(input);
            Assert.AreEqual(EnemyBrainState.Patrol, output.NextState);
        }

        [Test]
        public void LowHealth_SkittishRole_ShouldRetreat()
        {
            bool should = EnemyDecisionCore.ShouldRetreatAtLowHealth(0.2f, true, EnemyRole.Ranged, 0.25f);
            Assert.IsTrue(should);
        }

        [Test]
        public void LowHealth_MeleeRole_ShouldNotRetreat()
        {
            bool should = EnemyDecisionCore.ShouldRetreatAtLowHealth(0.1f, true, EnemyRole.Chaser, 0.25f);
            Assert.IsFalse(should);
        }

        [Test]
        public void ForcedRetreat_WhenFearActive_SetRetreatState()
        {
            var input = new EnemyDecisionInput(
                distanceToTarget: 5f, detectionRange: 10f, leashRange: 30f,
                currentTime: 1f, burrowEmergeDistance: 1.4f, lowHealthRetreatThreshold: 0.25f,
                retreatEndTime: 0f, forcedRetreatUntil: 2f, stunUntil: 0f,
                currentHpFraction: 0.8f, currentState: EnemyBrainState.Chase,
                movementType: EnemyMovementType.GroundChase, primaryRole: EnemyRole.Chaser,
                hasActiveThreat: false, targetIsValid: true, healthIsValid: true
            );
            var output = EnemyDecisionCore.Evaluate(input);
            Assert.AreEqual(EnemyBrainState.Retreat, output.NextState);
            Assert.IsTrue(output.ShouldSetRetreat);
        }

        [Test]
        public void Chase_PlayerBeyondLeash_NoThreat_TransitionsToPatrol()
        {
            var input = new EnemyDecisionInput(
                distanceToTarget: 35f, detectionRange: 10f, leashRange: 30f,
                currentTime: 1f, burrowEmergeDistance: 1.4f, lowHealthRetreatThreshold: 0.25f,
                retreatEndTime: 0f, forcedRetreatUntil: 0f, stunUntil: 0f,
                currentHpFraction: 0.8f, currentState: EnemyBrainState.Chase,
                movementType: EnemyMovementType.GroundChase, primaryRole: EnemyRole.Chaser,
                hasActiveThreat: false, targetIsValid: true, healthIsValid: true
            );
            var output = EnemyDecisionCore.Evaluate(input);
            Assert.AreEqual(EnemyBrainState.Patrol, output.NextState);
        }

        [Test]
        public void ResolveEngageState_BurrowCapable_FarFromPlayer_ReturnsBurrow()
        {
            var state = EnemyDecisionCore.ResolveEngageState(EnemyMovementType.BurrowAmbush, 10f, 1.4f);
            Assert.AreEqual(EnemyBrainState.Burrow, state);
        }

        [Test]
        public void ResolveEngageState_GroundChase_ReturnsChase()
        {
            var state = EnemyDecisionCore.ResolveEngageState(EnemyMovementType.GroundChase, 5f, 1.4f);
            Assert.AreEqual(EnemyBrainState.Chase, state);
        }
    }
}
