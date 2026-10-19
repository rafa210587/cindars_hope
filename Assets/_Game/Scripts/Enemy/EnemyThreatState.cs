using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_04 — threat/aggro memory for a single enemy brain.
    ///
    /// Pure C# (no MonoBehaviour, no UnityEngine.Time): the owner passes <c>now</c> explicitly
    /// so the rule is deterministic and EditMode-testable. The brain notices the target while it
    /// is in detection range; after the target leaves range the threat persists for
    /// <see cref="ThreatMemorySeconds"/> so the enemy keeps pursuing the last known position
    /// instead of forgetting at the leash edge (the "pull one by one / kite forever" problem).
    /// </summary>
    public sealed class EnemyThreatState
    {
        // Per-movement-type memory windows (seconds). Swarm forgets fast; guards hold a grudge.
        public const float DefaultMemorySeconds = 4f;
        public const float SwarmMemorySeconds = 2f;
        public const float GuardMemorySeconds = 6f;

        private float _threatMemorySeconds = DefaultMemorySeconds;
        private float _lastSeenTime = float.NegativeInfinity;
        private bool _hasEverSeen;
        private Vector2 _lastKnownPosition;

        public float ThreatMemorySeconds => _threatMemorySeconds;
        public Vector2 LastKnownPosition => _lastKnownPosition;
        public bool HasEverSeenTarget => _hasEverSeen;

        public EnemyThreatState(float threatMemorySeconds = DefaultMemorySeconds)
        {
            SetMemorySeconds(threatMemorySeconds);
        }

        public void SetMemorySeconds(float seconds)
        {
            _threatMemorySeconds = Mathf.Max(0f, seconds);
        }

        /// <summary>The target is currently in range: refresh last-seen time and position.</summary>
        public void NoticeTarget(Vector2 targetPosition, float now)
        {
            _lastKnownPosition = targetPosition;
            _lastSeenTime = now;
            _hasEverSeen = true;
        }

        /// <summary>
        /// True while the memory window is still open. Returns false once the target has been
        /// out of range for longer than <see cref="ThreatMemorySeconds"/>, or if never seen.
        /// </summary>
        public bool HasThreat(float now)
        {
            if (!_hasEverSeen)
            {
                return false;
            }

            return now - _lastSeenTime <= _threatMemorySeconds;
        }

        /// <summary>Seconds remaining before the threat expires (0 once expired).</summary>
        public float RemainingThreatSeconds(float now)
        {
            if (!_hasEverSeen)
            {
                return 0f;
            }

            return Mathf.Max(0f, _threatMemorySeconds - (now - _lastSeenTime));
        }

        /// <summary>Forget the target entirely (used on collective leash reset).</summary>
        public void Clear()
        {
            _hasEverSeen = false;
            _lastSeenTime = float.NegativeInfinity;
        }

        /// <summary>Per-type default memory window. Swarm/Guard deviate from the 4s baseline.</summary>
        public static float ResolveMemorySeconds(EnemyMovementType movementType)
        {
            switch (movementType)
            {
                case EnemyMovementType.SwarmErratic:
                    return SwarmMemorySeconds;
                case EnemyMovementType.GuardStationary:
                    return GuardMemorySeconds;
                default:
                    return DefaultMemorySeconds;
            }
        }
    }
}
