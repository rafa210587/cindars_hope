using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o jogador percorre distancia suficiente para contar um passo.
    /// </summary>
    public readonly struct PlayerStepEvent
    {
        public Vector2 Position { get; }
        public float DistanceSinceLastStep { get; }

        public PlayerStepEvent(Vector2 position, float distanceSinceLastStep)
        {
            Position = position;
            DistanceSinceLastStep = distanceSinceLastStep;
        }
    }
}
