using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_24 — pure, deterministic decision rules for the new enemy moves, extracted out of the
    /// <see cref="EnemyBrain"/> MonoBehaviour so they get EditMode coverage (spec CA-2/CA-4):
    /// pack engagement, mimic ambush activation, charge-line telegraph sequencing, and
    /// anchor/arena leash. No UnityEngine.Time, no scene access — callers pass everything in.
    /// </summary>
    public static class EnemyMoveLogic
    {
        // ── Tuning constants (canonical faixas; tuning lives here, not as scattered magic numbers) ──

        /// <summary>Mimic (TreasureIdleAmbush) stays disguised until the player is within this many tiles.</summary>
        public const float MimicActivationTiles = 2f;

        /// <summary>A flanker that loses its leader waits this long before falling back to GroundChase.</summary>
        public const float FlankerNoLeaderFallbackSeconds = 1.5f;

        /// <summary>Default anchor leash radius (tiles) for ProtectAnchor when the profile gives none.</summary>
        public const float DefaultAnchorLeashTiles = 4f;

        // Windup/recovery by role (DECISOES §7-8): common 0.5/0.5, elite 0.7/0.6, boss 0.9-1.2/0.8.
        public const float CommonWindupSeconds = 0.5f;
        public const float CommonRecoverSeconds = 0.5f;
        public const float EliteWindupSeconds = 0.7f;
        public const float EliteRecoverSeconds = 0.6f;
        public const float BossWindupSeconds = 0.9f;
        public const float BossRecoverSeconds = 0.8f;

        // ── Move classification ─────────────────────────────────────────────────────────────────

        /// <summary>True for the two floating moves — they ignore ground/floor obstacles.</summary>
        public static bool IsFloating(EnemyMovementType move)
        {
            return move == EnemyMovementType.FloatingSlow || move == EnemyMovementType.FloatingOrbit;
        }

        /// <summary>True for the two boss primitives (arena leash + phase shift).</summary>
        public static bool IsBossPrimitive(EnemyMovementType move)
        {
            return move == EnemyMovementType.BossArenaControl || move == EnemyMovementType.BossPhaseShift;
        }

        /// <summary>True for moves that orbit/strafe the player at firing distance.</summary>
        public static bool IsOrbitingMove(EnemyMovementType move)
        {
            return move == EnemyMovementType.CircleStrafe || move == EnemyMovementType.FloatingOrbit;
        }

        // ── Pack coordination (CA-2) ──────────────────────────────────────────────────────────────

        /// <summary>
        /// A <see cref="EnemyMovementType.PackFlanker"/> only commits to engaging while a living
        /// <see cref="EnemyMovementType.PackLeader"/> is within <paramref name="leaderAwarenessRadius"/>.
        /// </summary>
        public static bool ShouldFlankerEngage(bool leaderAlive, float distanceToLeader, float leaderAwarenessRadius)
        {
            return leaderAlive && distanceToLeader <= leaderAwarenessRadius;
        }

        /// <summary>
        /// Fallback rule (risk mitigation: no pack deadlock). When no leader is in range, a flanker
        /// must NOT stand still forever — after <see cref="FlankerNoLeaderFallbackSeconds"/> without a
        /// leader it acts as a plain GroundChase. Returns true once the timeout has elapsed.
        /// </summary>
        public static bool ShouldFlankerFallbackToChase(float secondsWithoutLeader)
        {
            return secondsWithoutLeader >= FlankerNoLeaderFallbackSeconds;
        }

        /// <summary>
        /// When a <see cref="EnemyMovementType.PackLeader"/> dies, its flankers switch to
        /// RetreatAndCall (flee + call for help). Returns the move a flanker should adopt.
        /// </summary>
        public static EnemyMovementType ResolveFlankerMoveOnLeaderState(bool leaderAlive, EnemyMovementType currentMove)
        {
            if (currentMove != EnemyMovementType.PackFlanker)
            {
                return currentMove;
            }

            return leaderAlive ? EnemyMovementType.PackFlanker : EnemyMovementType.RetreatAndCall;
        }

        // ── Mimic ambush (CA-4) ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Mimic (<see cref="EnemyMovementType.TreasureIdleAmbush"/>) activates only when the player is
        /// strictly within <see cref="MimicActivationTiles"/>. Until then it stays disguised/immobile.
        /// </summary>
        public static bool ShouldMimicActivate(float distanceToPlayerTiles)
        {
            return distanceToPlayerTiles >= 0f && distanceToPlayerTiles < MimicActivationTiles;
        }

        // ── Charge line (CA-4) ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// ChargeLine sequence: a charge is only allowed to fire once a telegraph has been shown.
        /// Returns true when the enemy may begin the straight investida (telegraph done + off cooldown).
        /// </summary>
        public static bool ShouldChargeLineFire(bool telegraphShown, float telegraphElapsedSeconds, float telegraphDurationSeconds, bool offCooldown)
        {
            if (!telegraphShown || !offCooldown)
            {
                return false;
            }

            return telegraphElapsedSeconds >= telegraphDurationSeconds;
        }

        /// <summary>
        /// Straight-line charge velocity toward a locked aim direction. The investida does NOT
        /// home — direction is captured at telegraph time so the player can sidestep it.
        /// </summary>
        public static Vector2 ResolveChargeVelocity(Vector2 lockedDirection, float chargeSpeed)
        {
            if (lockedDirection.sqrMagnitude <= 0.0001f)
            {
                return Vector2.zero;
            }

            return lockedDirection.normalized * Mathf.Max(0f, chargeSpeed);
        }

        // ── Anchor / arena leash ────────────────────────────────────────────────────────────────

        /// <summary>
        /// True when a <see cref="EnemyMovementType.ProtectAnchor"/> (or BossArenaControl) enemy is
        /// beyond its leash radius from the anchor and must be pulled back.
        /// </summary>
        public static bool IsBeyondAnchorLeash(Vector2 position, Vector2 anchor, float leashRadiusTiles)
        {
            float r = Mathf.Max(0f, leashRadiusTiles);
            return (position - anchor).sqrMagnitude > r * r;
        }

        /// <summary>
        /// Clamp a desired destination to within <paramref name="leashRadiusTiles"/> of the anchor.
        /// Used so ProtectAnchor/arena enemies chase the player but never abandon their post/arena.
        /// </summary>
        public static Vector2 ClampToAnchor(Vector2 desired, Vector2 anchor, float leashRadiusTiles)
        {
            float r = Mathf.Max(0f, leashRadiusTiles);
            Vector2 fromAnchor = desired - anchor;
            if (fromAnchor.sqrMagnitude <= r * r)
            {
                return desired;
            }

            return anchor + fromAnchor.normalized * r;
        }

        // ── Windup/recovery by role ─────────────────────────────────────────────────────────────

        /// <summary>Canonical windup seconds for the enemy tier (common/elite/boss). DECISOES §7-8.</summary>
        public static float ResolveWindupSeconds(bool isBoss, bool isElite)
        {
            if (isBoss) return BossWindupSeconds;
            return isElite ? EliteWindupSeconds : CommonWindupSeconds;
        }

        /// <summary>Canonical recovery seconds for the enemy tier (common/elite/boss). DECISOES §7-8.</summary>
        public static float ResolveRecoverSeconds(bool isBoss, bool isElite)
        {
            if (isBoss) return BossRecoverSeconds;
            return isElite ? EliteRecoverSeconds : CommonRecoverSeconds;
        }
    }
}
