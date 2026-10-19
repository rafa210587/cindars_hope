using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// Pure state machine logic extracted from EnemyBrain. No MonoBehaviour, no Time.time,
    /// no scene references — all inputs arrive via EnemyDecisionInput; outputs are plain flags.
    /// </summary>
    public static class EnemyDecisionCore
    {
        public static EnemyDecisionOutput Evaluate(in EnemyDecisionInput input)
        {
            float dist = input.DistanceToTarget;
            bool inDetect = dist <= input.DetectionRange;
            bool inLeash = dist <= input.LeashRange;

            // F01: Fear externo força Retreat (fora de windup/recover — guard acima já retornou).
            if (input.CurrentTime < input.ForcedRetreatUntil && input.CurrentState != EnemyBrainState.Retreat)
            {
                float retreatEnd = Mathf.Max(input.RetreatEndTime, input.ForcedRetreatUntil);
                return new EnemyDecisionOutput(
                    nextState: EnemyBrainState.Retreat,
                    shouldTryAction: false,
                    shouldSetSubmergedVisual: false,
                    clearSubmergedVisual: false,
                    shouldSetRetreat: true,
                    retreatEndTimeOverride: retreatEnd
                );
            }

            // Skittish roles break off and flee when badly hurt, regardless of current state.
            if (input.CurrentState != EnemyBrainState.Retreat
                && ShouldRetreatAtLowHealth(input.CurrentHpFraction, input.HealthIsValid, input.PrimaryRole, input.LowHealthRetreatThreshold)
                && inLeash)
            {
                float retreatEnd = input.CurrentTime + input.RetreatDurationSeconds;
                return new EnemyDecisionOutput(
                    nextState: EnemyBrainState.Retreat,
                    shouldTryAction: false,
                    shouldSetSubmergedVisual: false,
                    clearSubmergedVisual: false,
                    shouldSetRetreat: true,
                    retreatEndTimeOverride: retreatEnd
                );
            }

            EnemyBrainState next = input.CurrentState;
            bool shouldTryAction = false;
            bool shouldSetSubmergedVisual = false;
            bool clearSubmergedVisual = false;
            bool shouldSetRetreat = false;
            float retreatEndTimeOverride = input.RetreatEndTime;

            switch (input.CurrentState)
            {
                case EnemyBrainState.Idle:
                    next = inDetect ? EnemyBrainState.Alert : EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Patrol:
                    if (inDetect)
                    {
                        var (engageState, setBurrow) = ResolveEngageStateInternal(input.MovementType, dist, input.BurrowEmergeDistance, input.CanBurrow);
                        next = engageState;
                        if (setBurrow) shouldSetSubmergedVisual = true;
                    }
                    break;

                case EnemyBrainState.Alert:
                    if (inDetect)
                    {
                        var (engageState, setBurrow) = ResolveEngageStateInternal(input.MovementType, dist, input.BurrowEmergeDistance, input.CanBurrow);
                        next = engageState;
                        if (setBurrow) shouldSetSubmergedVisual = true;
                    }
                    else if (input.HasActiveThreat)
                        next = EnemyBrainState.Chase;
                    else
                        next = EnemyBrainState.Patrol;
                    break;

                case EnemyBrainState.Retreat:
                    if (input.CurrentTime >= input.RetreatEndTime)
                    {
                        if (inDetect)
                        {
                            var (engageState, setBurrow) = ResolveEngageStateInternal(input.MovementType, dist, input.BurrowEmergeDistance, input.CanBurrow);
                            next = engageState;
                            if (setBurrow) shouldSetSubmergedVisual = true;
                        }
                        else
                        {
                            next = EnemyBrainState.Patrol;
                        }
                    }
                    break;

                case EnemyBrainState.Burrow:
                    if (!inLeash)
                    {
                        clearSubmergedVisual = true;
                        next = EnemyBrainState.Patrol;
                        break;
                    }

                    if (dist <= input.BurrowEmergeDistance)
                    {
                        clearSubmergedVisual = true;
                        next = EnemyBrainState.Chase;
                        shouldTryAction = true;
                    }
                    break;

                case EnemyBrainState.Chase:
                case EnemyBrainState.Kite:
                case EnemyBrainState.GuardHold:
                case EnemyBrainState.CastPrepare:
                    if (!inLeash)
                    {
                        if (input.HasActiveThreat)
                        {
                            // remain engaged — no state change
                            break;
                        }

                        // LogThreatExpiredOnce and TryCollectivePackLeashReset are side effects
                        // that stay in EnemyBrain; here we just transition to Patrol.
                        next = EnemyBrainState.Patrol;
                        break;
                    }
                    shouldTryAction = true;
                    break;
            }

            return new EnemyDecisionOutput(
                nextState: next,
                shouldTryAction: shouldTryAction,
                shouldSetSubmergedVisual: shouldSetSubmergedVisual,
                clearSubmergedVisual: clearSubmergedVisual,
                shouldSetRetreat: shouldSetRetreat,
                retreatEndTimeOverride: retreatEndTimeOverride
            );
        }

        /// <summary>
        /// Skittish roles (Swarm/Ranged/Caster) retreat when HP drops below the threshold.
        /// </summary>
        public static bool ShouldRetreatAtLowHealth(float hpFraction, bool healthIsValid, EnemyRole role, float threshold)
        {
            if (!healthIsValid)
                return false;

            if (role != EnemyRole.Swarm && role != EnemyRole.Ranged && role != EnemyRole.Caster)
                return false;

            return hpFraction <= threshold;
        }

        /// <summary>
        /// Burrowers approach hidden underground; everyone else goes straight to Chase.
        /// canBurrow cobre tanto MovementType==BurrowAmbush quanto EnemyMovementProfileSO.CanBurrow
        /// (a mesma condição do EnemyBrain original).
        /// </summary>
        public static EnemyBrainState ResolveEngageState(EnemyMovementType moveType, float dist, float burrowEmergeDistance, bool canBurrow = false)
        {
            bool isBurrower = moveType == EnemyMovementType.BurrowAmbush || canBurrow;
            if (isBurrower && dist > burrowEmergeDistance * 2f)
                return EnemyBrainState.Burrow;

            return EnemyBrainState.Chase;
        }

        // Internal version que passa CanBurrow via EnemyDecisionInput.
        private static (EnemyBrainState state, bool setSubmergedVisual) ResolveEngageStateInternal(
            EnemyMovementType moveType, float dist, float burrowEmergeDistance, bool canBurrow)
        {
            bool isBurrower = moveType == EnemyMovementType.BurrowAmbush || canBurrow;
            if (isBurrower && dist > burrowEmergeDistance * 2f)
                return (EnemyBrainState.Burrow, true);

            return (EnemyBrainState.Chase, false);
        }
    }

    public readonly struct EnemyDecisionInput
    {
        public readonly float DistanceToTarget;
        public readonly float DetectionRange;
        public readonly float LeashRange;
        public readonly float CurrentTime;
        public readonly float BurrowEmergeDistance;
        public readonly float LowHealthRetreatThreshold;
        public readonly float RetreatEndTime;
        public readonly float RetreatDurationSeconds;
        public readonly float ForcedRetreatUntil;
        public readonly float StunUntil;
        public readonly float CurrentHpFraction;
        public readonly EnemyBrainState CurrentState;
        public readonly EnemyMovementType MovementType;
        public readonly EnemyRole PrimaryRole;
        public readonly bool HasActiveThreat;
        public readonly bool TargetIsValid;
        public readonly bool HealthIsValid;
        /// <summary>True quando o EnemyMovementProfileSO.CanBurrow está ligado (além de BurrowAmbush MovementType).</summary>
        public readonly bool CanBurrow;

        public EnemyDecisionInput(
            float distanceToTarget,
            float detectionRange,
            float leashRange,
            float currentTime,
            float burrowEmergeDistance,
            float lowHealthRetreatThreshold,
            float retreatEndTime,
            float forcedRetreatUntil,
            float stunUntil,
            float currentHpFraction,
            EnemyBrainState currentState,
            EnemyMovementType movementType,
            EnemyRole primaryRole,
            bool hasActiveThreat,
            bool targetIsValid,
            bool healthIsValid,
            bool canBurrow = false,
            float retreatDurationSeconds = 2.5f)
        {
            DistanceToTarget = distanceToTarget;
            DetectionRange = detectionRange;
            LeashRange = leashRange;
            CurrentTime = currentTime;
            BurrowEmergeDistance = burrowEmergeDistance;
            LowHealthRetreatThreshold = lowHealthRetreatThreshold;
            RetreatEndTime = retreatEndTime;
            ForcedRetreatUntil = forcedRetreatUntil;
            StunUntil = stunUntil;
            CurrentHpFraction = currentHpFraction;
            CurrentState = currentState;
            MovementType = movementType;
            PrimaryRole = primaryRole;
            HasActiveThreat = hasActiveThreat;
            TargetIsValid = targetIsValid;
            HealthIsValid = healthIsValid;
            CanBurrow = canBurrow;
            RetreatDurationSeconds = retreatDurationSeconds;
        }
    }

    public readonly struct EnemyDecisionOutput
    {
        public readonly EnemyBrainState NextState;
        public readonly bool ShouldTryAction;
        public readonly bool ShouldSetSubmergedVisual;
        public readonly bool ClearSubmergedVisual;
        public readonly bool ShouldSetRetreat;
        public readonly float RetreatEndTimeOverride;

        public EnemyDecisionOutput(
            EnemyBrainState nextState,
            bool shouldTryAction,
            bool shouldSetSubmergedVisual,
            bool clearSubmergedVisual,
            bool shouldSetRetreat,
            float retreatEndTimeOverride)
        {
            NextState = nextState;
            ShouldTryAction = shouldTryAction;
            ShouldSetSubmergedVisual = shouldSetSubmergedVisual;
            ClearSubmergedVisual = clearSubmergedVisual;
            ShouldSetRetreat = shouldSetRetreat;
            RetreatEndTimeOverride = retreatEndTimeOverride;
        }
    }
}
