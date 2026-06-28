using UnityEngine;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Faz o NPC ter corpo SÓLIDO (colide com paredes/portas) sem bloquear o jogador. O collider do
    /// NPC é sólido contra o cenário, mas a colisão NPC↔jogador é desligada em runtime via
    /// <see cref="Physics2D.IgnoreCollision(Collider2D, Collider2D, bool)"/> — assim o jogador
    /// atravessa NPCs (padrão RPG top-down) enquanto os NPCs respeitam as barreiras das casas.
    ///
    /// Não faz busca global de cena: o gerador injeta o collider do jogador via <see cref="Configure"/>.
    /// (Alternativa de camada de física + matriz de colisão exigiria editar ProjectSettings.)
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcPhysicsBody : MonoBehaviour
    {
        [SerializeField] private Collider2D _solidCollider;
        [SerializeField] private Collider2D _playerCollider;

        private bool _applied;

        public void Configure(Collider2D solidCollider, Collider2D playerCollider)
        {
            _solidCollider = solidCollider;
            _playerCollider = playerCollider;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Apply()
        {
            if (_applied || _solidCollider == null || _playerCollider == null)
            {
                return;
            }

            Physics2D.IgnoreCollision(_solidCollider, _playerCollider, true);
            _applied = true;
        }
    }
}
