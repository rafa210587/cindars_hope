using UnityEngine;

namespace CindarsHope.Enemy
{
    // Pure-logic helper for blink-strike destination calculation.
    // No MonoBehaviour — deterministic, EditMode-testable.
    // Flank side is passed in from EnemyBrain._blinkFlankSide (alternating, not Random).
    public static class EnemyBlinkExecutor
    {
        public readonly struct BlinkResult
        {
            public readonly bool Success;
            public readonly Vector2 Destination;
            public readonly string FailReason;

            public BlinkResult(bool success, Vector2 destination, string failReason = null)
            {
                Success = success;
                Destination = destination;
                FailReason = failReason;
            }

            public static BlinkResult Fail(string reason) => new BlinkResult(false, Vector2.zero, reason);
            public static BlinkResult Ok(Vector2 dest) => new BlinkResult(true, dest);
        }

        // Calculates the blink destination: lands at blinkRange units from the player,
        // offset slightly to one flank. FlankSide must be +1 or -1 (set once per brain
        // at spawn to avoid every enemy flanking the same direction).
        public static BlinkResult CalculateDestination(
            Vector2 enemyPos, Vector2 playerPos, float blinkRange, float flankSide)
        {
            if (blinkRange <= 0f)
                return BlinkResult.Fail("BlinkRange must be > 0");

            Vector2 toPlayer = playerPos - enemyPos;
            float dist = toPlayer.magnitude;
            if (dist < 0.01f)
                return BlinkResult.Fail("Enemy and player overlap");

            Vector2 dir = toPlayer / dist;
            // Land at blinkRange from the player (approaching from current angle)
            Vector2 base_dest = playerPos - dir * blinkRange;
            // Slight flank offset to avoid standing on top of the player
            Vector2 flank = new Vector2(-dir.y, dir.x) * (flankSide * 0.3f);
            return BlinkResult.Ok(base_dest + flank);
        }
    }
}
