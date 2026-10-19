using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_82 — publicado pelo PlayerAttackController no inicio do charge de ataque (KeyDown).
    /// Permite que o EnemyBrain detecte o windup do jogador para evasao reativa.
    /// Nao substitui PlayerChargedAttackEvent (que sai no KeyUp/resolve).
    /// </summary>
    public sealed class PlayerAttackWindupEvent
    {
        /// <summary>Posicao do player no inicio do charge.</summary>
        public Vector2 PlayerPosition { get; }

        public PlayerAttackWindupEvent(Vector2 playerPosition)
        {
            PlayerPosition = playerPosition;
        }
    }
}
