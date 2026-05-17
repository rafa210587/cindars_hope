using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o jogador retorna a uma posicao segura apos derrota/exaustao.
    /// </summary>
    public readonly struct PlayerRespawnedEvent
    {
        public Vector2 Position { get; }
        public int CurrentHP { get; }
        public int GoldLost { get; }

        public PlayerRespawnedEvent(Vector2 position, int currentHP, int goldLost)
        {
            Position = position;
            CurrentHP = currentHP;
            GoldLost = goldLost;
        }
    }
}
