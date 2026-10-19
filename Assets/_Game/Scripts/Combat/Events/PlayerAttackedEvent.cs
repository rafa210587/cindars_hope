using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o jogador executa um ataque.
    /// </summary>
    public readonly struct PlayerAttackedEvent
    {
        public int Damage { get; }
        public Vector2 Direction { get; }
        public DamageResult DamageResult { get; }

        public PlayerAttackedEvent(int damage, Vector2 direction, DamageResult damageResult)
        {
            Damage = damage;
            Direction = direction;
            DamageResult = damageResult;
        }
    }
}
