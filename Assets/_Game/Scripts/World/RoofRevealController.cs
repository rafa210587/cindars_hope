using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// Casa fisica "de verdade": o interior fica NO MESMO lugar do exterior (sem teleporte, sem cena
    /// separada). O telhado cobre o footprint e ESCONDE o interior enquanto o jogador esta fora; quando
    /// o jogador entra pela porta (vao na parede de baixo) e pisa no trigger desta casa, o telhado some
    /// (alpha -> <see cref="_revealedAlpha"/>) revelando o comodo, e volta a aparecer quando ele sai.
    ///
    /// Segura apenas referencias serializadas (os SpriteRenderers do telhado desta casa) — nenhuma busca
    /// global de cena. O gerador de cena pluga as refs via <see cref="Configure"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoofRevealController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] _roofRenderers;
        [Tooltip("Alpha do telhado quando o jogador esta dentro (0 = totalmente revelado).")]
        [SerializeField] private float _revealedAlpha = 0f;
        [Tooltip("Alpha do telhado quando o jogador esta fora (1 = telhado opaco, esconde o interior).")]
        [SerializeField] private float _hiddenAlpha = 1f;
        [SerializeField] private string _playerTag = "Player";

        // Conta quantos colliders do jogador estao dentro (root + filhos com trigger): so volta a cobrir
        // quando o ultimo sai. Evita piscar o telhado quando dois colliders do jogador cruzam a borda.
        private int _playerColliders;

        private void OnEnable()
        {
            // Estado inicial coerente caso o componente seja (re)ativado fora da cena gerada.
            ApplyAlpha(_playerColliders > 0 ? _revealedAlpha : _hiddenAlpha);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            _playerColliders++;
            if (_playerColliders == 1)
            {
                ApplyAlpha(_revealedAlpha);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            _playerColliders = Mathf.Max(0, _playerColliders - 1);
            if (_playerColliders == 0)
            {
                ApplyAlpha(_hiddenAlpha);
            }
        }

        private bool IsPlayer(Collider2D other)
        {
            if (other == null)
            {
                return false;
            }

            // O collider solido do jogador esta no root marcado com a tag; um filho-trigger
            // (InteractionTrigger) compartilha o mesmo rigidbody/root e tambem deve contar como "jogador".
            if (other.CompareTag(_playerTag))
            {
                return true;
            }

            var body = other.attachedRigidbody;
            return body != null && body.CompareTag(_playerTag);
        }

        private void ApplyAlpha(float alpha)
        {
            if (_roofRenderers == null)
            {
                return;
            }

            foreach (var renderer in _roofRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                var color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
        }

        /// <summary>Configuracao pelo gerador de cena: os SpriteRenderers do telhado desta casa.</summary>
        public void Configure(SpriteRenderer[] roofRenderers, float hiddenAlpha = 1f, float revealedAlpha = 0f)
        {
            _roofRenderers = roofRenderers;
            _hiddenAlpha = hiddenAlpha;
            _revealedAlpha = revealedAlpha;
            ApplyAlpha(_hiddenAlpha);
        }
    }
}
