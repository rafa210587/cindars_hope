using UnityEngine;

namespace CindarsHope.Combat
{
    public readonly struct DamageRequest
    {
        public readonly int Amount;
        public readonly Vector2 SourcePosition;
        public readonly float KnockbackForce;

        public DamageRequest(int amount, Vector2 sourcePosition, float knockbackForce = 0f)
        {
            Amount = amount;
            SourcePosition = sourcePosition;
            KnockbackForce = knockbackForce;
        }
    }
}
