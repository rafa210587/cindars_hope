using System;
using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    public enum TemporaryRevealKind
    {
        Resource = 0,
        Hazard = 1,
        Interactable = 2
    }

    /// <summary>
    /// Engine-free view of a materialized world target. Implementations own their visual state
    /// and must unregister on disable/destruction.
    /// </summary>
    public interface ITemporaryRevealTarget
    {
        string RevealTargetId { get; }
        TemporaryRevealKind RevealKind { get; }
        float WorldX { get; }
        float WorldY { get; }
        bool IsEnabled { get; }
        bool IsSecret { get; }
        bool IsExhausted { get; }
        bool IsActive { get; }
        void ApplyTemporaryReveal(string sourceId, float expiresAt);
    }

    /// <summary>
    /// Registry of targets that already exist in the loaded world. It never materializes content,
    /// changes fog/minimap state or exposes Unity object types through its contract.
    /// </summary>
    public sealed class TemporaryRevealRegistry
    {
        private readonly Dictionary<string, ITemporaryRevealTarget> _targets =
            new Dictionary<string, ITemporaryRevealTarget>(StringComparer.Ordinal);

        public int Count => _targets.Count;

        public bool Register(ITemporaryRevealTarget target)
        {
            if (target == null || string.IsNullOrWhiteSpace(target.RevealTargetId)
                || _targets.ContainsKey(target.RevealTargetId))
            {
                return false;
            }

            _targets.Add(target.RevealTargetId, target);
            return true;
        }

        public bool Unregister(ITemporaryRevealTarget target)
        {
            if (target == null || string.IsNullOrWhiteSpace(target.RevealTargetId)
                || !_targets.TryGetValue(target.RevealTargetId, out var registered)
                || !ReferenceEquals(registered, target))
            {
                return false;
            }

            return _targets.Remove(target.RevealTargetId);
        }

        /// <summary>
        /// Applies a reveal to eligible targets inside the inclusive radius and returns its count.
        /// Sorting by stable target id makes callback order deterministic across registrations.
        /// </summary>
        public int RevealEligible(
            float originX,
            float originY,
            float inclusiveRadius,
            string sourceId,
            float expiresAt)
        {
            if (inclusiveRadius < 0f || float.IsNaN(inclusiveRadius)
                || float.IsInfinity(inclusiveRadius) || string.IsNullOrWhiteSpace(sourceId)
                || float.IsNaN(expiresAt) || float.IsInfinity(expiresAt))
            {
                return 0;
            }

            var radiusSquared = inclusiveRadius * inclusiveRadius;
            var eligible = new List<ITemporaryRevealTarget>();
            foreach (var target in _targets.Values)
            {
                if (!IsEligible(target))
                {
                    continue;
                }

                var dx = target.WorldX - originX;
                var dy = target.WorldY - originY;
                if ((dx * dx) + (dy * dy) <= radiusSquared)
                {
                    eligible.Add(target);
                }
            }

            eligible.Sort((left, right) =>
                string.CompareOrdinal(left.RevealTargetId, right.RevealTargetId));

            foreach (var target in eligible)
            {
                target.ApplyTemporaryReveal(sourceId, expiresAt);
            }

            return eligible.Count;
        }

        public static bool IsEligible(ITemporaryRevealTarget target)
        {
            if (target == null || !target.IsEnabled)
            {
                return false;
            }

            switch (target.RevealKind)
            {
                case TemporaryRevealKind.Resource:
                    return !target.IsExhausted;
                case TemporaryRevealKind.Hazard:
                    return target.IsActive;
                case TemporaryRevealKind.Interactable:
                    return !target.IsSecret;
                default:
                    return false;
            }
        }
    }

    public static class TemporaryRevealRegistryProvider
    {
        public static TemporaryRevealRegistry Registry { get; set; } = new TemporaryRevealRegistry();

        public static void Reset()
        {
            Registry = new TemporaryRevealRegistry();
        }
    }
}
