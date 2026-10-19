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
        [SerializeField] private SpriteRenderer[] _interiorRenderers;
        [Tooltip("Alpha do telhado quando o jogador esta dentro (0 = totalmente revelado).")]
        [SerializeField] private float _revealedAlpha = 0f;
        [Tooltip("Alpha do telhado quando o jogador esta fora (1 = telhado opaco, esconde o interior).")]
        [SerializeField] private float _hiddenAlpha = 1f;
        [SerializeField] private string _playerTag = "Player";

        // Conta quantos colliders do jogador estao dentro (root + filhos com trigger): so volta a cobrir
        // quando o ultimo sai. Evita piscar o telhado quando dois colliders do jogador cruzam a borda.
        private int _playerColliders;
        [SerializeField] private BoxCollider2D _feetInterior;
        private Collider2D _trackedPlayerBody;

        /// <summary>Opt in to feet crossing a useful interior box; interaction sensors do not reveal it.</summary>
        public void ConfigureFeetOccupancy(BoxCollider2D interiorTrigger)
        {
            if (interiorTrigger == null || !interiorTrigger.isTrigger)
                throw new System.ArgumentException("Feet occupancy requires an interior trigger box.");
            _feetInterior = interiorTrigger;
            ResetOccupancy();
        }

        private void OnDisable() => ResetOccupancy();

        private void ResetOccupancy()
        {
            _trackedPlayerBody = null;
            _playerColliders = 0;
            ApplyAlpha(_hiddenAlpha);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_feetInterior != null && IsPlayer(other)) TrackFeet(other);
        }

        private void LateUpdate()
        {
            // Also clears visibility after a warp, disable or destruction without a trigger exit.
            if (_feetInterior != null && (_trackedPlayerBody != null || _playerColliders > 0)) EvaluateFeet();
        }

        private void TrackFeet(Collider2D other)
        {
            if (other.isTrigger) return;
            _trackedPlayerBody = other;
            EvaluateFeet();
        }

        private void EvaluateFeet()
        {
            var inside = false;
            if (_trackedPlayerBody != null && _trackedPlayerBody.enabled &&
                _trackedPlayerBody.gameObject.activeInHierarchy && _feetInterior.enabled)
            {
                var body = _trackedPlayerBody.attachedRigidbody;
                var feet = body != null ? (Vector3)body.position : _trackedPlayerBody.transform.position;
                var local = (Vector2)_feetInterior.transform.InverseTransformPoint(feet) - _feetInterior.offset;
                inside = Mathf.Abs(local.x) < _feetInterior.size.x * 0.5f &&
                    Mathf.Abs(local.y) < _feetInterior.size.y * 0.5f;
            }
            var count = inside ? 1 : 0;
            if (count == _playerColliders) return;
            _playerColliders = count;
            ApplyAlpha(inside ? _revealedAlpha : _hiddenAlpha);
        }

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

            if (_feetInterior != null)
            {
                TrackFeet(other);
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

            if (_feetInterior != null)
            {
                EvaluateFeet();
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
            if (_interiorRenderers != null)
                foreach (var interior in _interiorRenderers)
                    if (interior != null) interior.enabled = _playerColliders > 0;

            if (_roofRenderers == null) return;
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
        public void Configure(SpriteRenderer[] roofRenderers, float hiddenAlpha = 1f, float revealedAlpha = 0f,
            SpriteRenderer[] interiorRenderers = null)
        {
            _roofRenderers = roofRenderers;
            _interiorRenderers = interiorRenderers;
            _hiddenAlpha = hiddenAlpha;
            _revealedAlpha = revealedAlpha;
            ApplyAlpha(_hiddenAlpha);
        }
    }
}
