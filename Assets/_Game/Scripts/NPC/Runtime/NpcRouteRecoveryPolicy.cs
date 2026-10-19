using UnityEngine;

namespace CindarsHope.NPC.Runtime
{
    /// <summary>Small deterministic policy shared by schedule recovery and N1 tests.</summary>
    public static class NpcRouteRecoveryPolicy
    {
        public const float ProgressWindowSeconds = 2f;
        public const float MinimumProgress = 0.10f;
        public const int MaxReplans = 2;

        public static bool HasInsufficientProgress(float previousDistance, float currentDistance, float elapsed)
            => elapsed >= ProgressWindowSeconds && previousDistance - currentDistance < MinimumProgress;

        public static bool CanReplan(int replanCount) => replanCount < MaxReplans;

        public static float ProgressTowardTarget(Vector2 windowStart, Vector2 current, Vector2 target)
            => Vector2.Distance(windowStart, target) - Vector2.Distance(current, target);

        public static bool CanSnap(bool offscreen) => offscreen;

        public static bool IsSafePoint(Vector2 point, float radius, Collider2D self = null)
        {
            var owner = self != null ? self.GetComponentInParent<NpcWanderer>() : null;
            var hits = Physics2D.OverlapCircleAll(point, radius);
            for (var i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null || hits[i] == self) continue;
                if (owner != null && hits[i].GetComponentInParent<NpcWanderer>() == owner) continue;
                return false;
            }
            return true;
        }
    }
}
