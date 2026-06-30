using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado pelo BowArrowAttackService quando uma flecha e disparada com sucesso.
    /// Dirige a animacao de tiro de arco do player (PlayerWalkAnimator): direcao do tiro +
    /// duracao do saque/release atrelada ao cooldown efetivo do arco (= attack speed, que
    /// escala com progressao e ASPD da arma). Espelha PlayerMeleeSwingEvent, para o arco.
    /// </summary>
    public sealed class PlayerBowShootEvent
    {
        /// <summary>Direcao do tiro (facing do player no momento do disparo).</summary>
        public Vector2 Direction { get; }

        /// <summary>Duracao da animacao em segundos (= cooldown efetivo do arco).</summary>
        public float Duration { get; }

        public PlayerBowShootEvent(Vector2 direction, float duration)
        {
            Direction = direction;
            Duration = duration;
        }
    }
}
