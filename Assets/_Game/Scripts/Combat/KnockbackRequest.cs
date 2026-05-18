using UnityEngine;

namespace CindarsHope.Combat
{
    public readonly struct KnockbackRequest
    {
        public readonly Vector2 Direction;
        public readonly float Force;

        public KnockbackRequest(Vector2 direction, float force)
        {
            Direction = direction.normalized;
            Force = force;
        }
    }
}
