using UnityEngine;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Fade-in de aparição para um NPC (ou qualquer SpriteRenderer). O NPC começa INVISÍVEL
    /// (alpha 0) e, quando o renderer entra na câmera pela PRIMEIRA vez, materializa suave
    /// (alpha 0→1 em <see cref="_fadeDuration"/> s) e então se autodestrói (one-shot — depois
    /// disso o alpha fica cheio e o componente sai do caminho).
    ///
    /// Motivo: NPCs spawnados longe da posição inicial do player (ex.: o Zrix batedor no bosque
    /// NO da fazenda, em (-26,8)) "aparecem de repente" quando o player chega perto. O fade suaviza
    /// essa aparição em vez do pop seco.
    ///
    /// Reusável e sem conflito: só mexe em <c>SpriteRenderer.color.a</c>. O
    /// <see cref="NpcWalkAnimator"/> troca <c>sprite</c>/<c>flipX</c> (não a cor), então os dois
    /// compõem — o personagem pode andar animado enquanto materializa.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NpcAppearFade : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 0.4f;

        private SpriteRenderer _spriteRenderer;
        private bool _started;
        private float _elapsed;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            SetAlpha(0f); // invisível até ser visto pela primeira vez
        }

        private void Update()
        {
            if (!_started)
            {
                // Espera o renderer entrar na câmera (isVisible é confiável e não depende das
                // sutilezas de enabled-state das mensagens OnBecameVisible).
                if (_spriteRenderer == null || !_spriteRenderer.isVisible)
                {
                    return;
                }

                _started = true;
            }

            _elapsed += Time.deltaTime;
            float t = _fadeDuration > 0f ? Mathf.Clamp01(_elapsed / _fadeDuration) : 1f;
            SetAlpha(t);

            if (t >= 1f)
            {
                // Alpha cheio: garante o valor final e remove o componente (one-shot).
                Destroy(this);
            }
        }

        private void SetAlpha(float alpha)
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            Color c = _spriteRenderer.color;
            c.a = alpha;
            _spriteRenderer.color = c;
        }
    }
}
