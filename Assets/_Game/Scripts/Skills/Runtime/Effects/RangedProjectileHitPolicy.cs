using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public enum RangedProjectilePolicyKind
    {
        Single,
        LinePiercer,
        TripleFan
    }

    /// <summary>Shared per cast so multiple colliders/projectiles obey one target policy.</summary>
    public sealed class RangedProjectileHitPolicy : CindarsHope.Foundation.IProjectileHitPolicy
    {
        private static readonly float[] LineMultipliers = { 1f, .8f, .64f, .51f, .41f };
        private readonly RangedProjectilePolicyKind _kind;
        private readonly HashSet<int> _lineTargets = new HashSet<int>();
        private readonly Dictionary<int, int> _fanHitsByTarget = new Dictionary<int, int>();

        public RangedProjectileHitPolicy(RangedProjectilePolicyKind kind)
        {
            _kind = kind;
        }

        public bool TryResolveHit(int targetRuntimeId, out float damageMultiplier)
        {
            damageMultiplier = 1f;
            if (_kind == RangedProjectilePolicyKind.LinePiercer)
            {
                if (_lineTargets.Count >= LineMultipliers.Length || !_lineTargets.Add(targetRuntimeId))
                    return false;
                damageMultiplier = LineMultipliers[_lineTargets.Count - 1];
                return true;
            }

            if (_kind == RangedProjectilePolicyKind.TripleFan)
            {
                _fanHitsByTarget.TryGetValue(targetRuntimeId, out int hitCount);
                if (hitCount >= 2) return false;
                _fanHitsByTarget[targetRuntimeId] = hitCount + 1;
                damageMultiplier = hitCount == 0 ? 1f : .5f;
            }

            return true;
        }
    }
}
