using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado pelo PlayerAttackController quando um ataque MELEE resolve (apos passar
    /// pelos checks de cooldown e stamina). Dirige a animacao de ataque do player
    /// (PlayerWalkAnimator): direcao do golpe + duracao do swing atrelada ao cooldown
    /// efetivo do ataque (= attack speed, que escala com progressao e ASPD da arma).
    ///
    /// Carrega apenas primitivos + enum Core (sem WeaponType) para nao acoplar Core a Combat;
    /// o publisher computa o arquetipo via WeaponAttackArchetypeMapper (Combat).
    /// </summary>
    public sealed class PlayerMeleeSwingEvent
    {
        /// <summary>Direcao do golpe (facing do player no momento do ataque).</summary>
        public Vector2 Direction { get; }

        /// <summary>Duracao do swing em segundos (= cooldown efetivo do ataque).</summary>
        public float Duration { get; }

        /// <summary>Arquetipo de animacao de ataque derivado do tipo de arma equipada.</summary>
        public PlayerAttackAnimArchetype Archetype { get; }

        public PlayerMeleeSwingEvent(Vector2 direction, float duration,
                                     PlayerAttackAnimArchetype archetype)
        {
            Direction = direction;
            Duration  = duration;
            Archetype = archetype;
        }
    }
}
