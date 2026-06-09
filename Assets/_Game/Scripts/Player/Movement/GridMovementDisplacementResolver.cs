using UnityEngine;

namespace CindarsHope.Player.Movement
{
    // WAVE_INTEGRATION_11: Resolves a movement displacement respecting collision and scene bounds.
    // Used by PlayerDashController and PlayerDodgeController for safe movement calculation.
    // Performs a raycast sweep to find the furthest safe position in a direction.
    public static class GridMovementDisplacementResolver
    {
        private const int DefaultRaySteps = 8;
        private static readonly LayerMask DefaultObstacleLayer = ~0; // All layers — filter by tag/layer at call site

        // Resolve the furthest safe position from origin in direction, up to maxDistance.
        // Uses raycasting with the player's collider radius to avoid wall clipping.
        // Returns the furthest reachable position (may be less than maxDistance if blocked).
        public static Vector2 Resolve(
            Vector2 origin,
            Vector2 direction,
            float maxDistance,
            float colliderRadius = 0.3f,
            LayerMask? obstacleLayer = null,
            Collider2D movingCollider = null)
        {
            if (direction.sqrMagnitude < 0.001f)
                return origin;

            direction = direction.normalized;
            var layer = obstacleLayer ?? (LayerMask)~0;

            if (movingCollider != null)
            {
                var filter = new ContactFilter2D
                {
                    useTriggers = false,
                    useLayerMask = true,
                    layerMask = layer
                };
                var results = new RaycastHit2D[8];
                var hitCount = movingCollider.Cast(direction, filter, results, maxDistance);
                if (hitCount <= 0)
                {
                    return origin + direction * maxDistance;
                }

                var nearestDistance = maxDistance;
                for (var i = 0; i < hitCount; i++)
                {
                    var colliderHit = results[i];
                    if (colliderHit.collider == null || colliderHit.collider == movingCollider)
                    {
                        continue;
                    }

                    nearestDistance = Mathf.Min(nearestDistance, colliderHit.distance);
                }

                var colliderSafeDistance = Mathf.Max(0f, nearestDistance - colliderRadius * 0.5f);
                return origin + direction * colliderSafeDistance;
            }

            // Cast a circle sweep to find the furthest safe point
            var hit = Physics2D.CircleCast(
                origin,
                colliderRadius,
                direction,
                maxDistance,
                layer);

            if (hit.collider == null)
            {
                // No obstacle: full displacement
                return origin + direction * maxDistance;
            }

            // Blocked: stop just before the hit point
            float safeDistance = Mathf.Max(0f, hit.distance - colliderRadius * 0.5f);
            return origin + direction * safeDistance;
        }
    }
}
