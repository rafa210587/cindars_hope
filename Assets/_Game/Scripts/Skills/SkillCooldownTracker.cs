using System;
using System.Collections.Generic;

namespace CindarsHope.Skills
{
    /// <summary>Cooldown ownership follows action identity, independent of equipment slots.</summary>
    public sealed class SkillCooldownTracker
    {
        private struct Cooldown { public float Deadline; public float Duration; }
        private readonly Dictionary<string, Cooldown> _actions = new Dictionary<string, Cooldown>(StringComparer.Ordinal);

        public void Start(string actionId, float now, float duration)
        {
            if (string.IsNullOrEmpty(actionId) || !Finite(now) || !Finite(duration) || duration <= 0 || !Finite(now + duration)) return;
            _actions[actionId] = new Cooldown { Deadline = now + duration, Duration = duration };
        }

        public float Remaining(string actionId, float now)
            => !string.IsNullOrEmpty(actionId) && Finite(now) && _actions.TryGetValue(actionId, out var value)
                ? Math.Max(0, Math.Min(value.Duration, value.Deadline - now)) : 0;

        public float Total(string actionId)
            => !string.IsNullOrEmpty(actionId) && _actions.TryGetValue(actionId, out var value) ? value.Duration : 0;

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
