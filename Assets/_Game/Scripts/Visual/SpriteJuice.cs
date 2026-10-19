using UnityEngine;

namespace CindarsHope.Visual
{
    /// <summary>
    /// Juice programatico para um sprite estatico (1 frame): idle bob com squash,
    /// flash de hit e lunge de attack. Auto-contido no proprio GameObject (usa o
    /// SpriteRenderer do mesmo object); so apresentacao, sem gameplay logic.
    /// Combat/AI chamam <see cref="Flash"/> e <see cref="Lunge"/> para o feedback.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteJuice : MonoBehaviour
    {
        [Header("Idle")]
        [SerializeField] private float idleBobAmplitude = 0.06f;
        [SerializeField] private float idleBobSpeed = 2.2f;
        [SerializeField] private float idleSquash = 0.04f;

        [Header("Hit flash")]
        [SerializeField] private float hitFlashDuration = 0.12f;
        [SerializeField] private Color hitFlashColor = Color.white;

        [Header("Attack lunge")]
        [SerializeField] private float lungeDistance = 0.22f;
        [SerializeField] private float lungeDuration = 0.18f;

        private SpriteRenderer _renderer;
        private Vector3 _baseLocalPos;
        private Vector3 _baseScale;
        private Color _baseColor;
        private float _phase;
        private float _flashTimer;
        private float _lungeTimer;
        private Vector2 _lungeDir = Vector2.right;
        // false em objetos movidos por IA (o bob de posicao brigaria com o movimento);
        // squash e flash continuam ativos pois nao mexem na posicao.
        private bool _bobPosition = true;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseLocalPos = transform.localPosition;
            _baseScale = transform.localScale;
            _baseColor = _renderer != null ? _renderer.color : Color.white;
            _phase = Random.value * Mathf.PI * 2f; // dessincroniza inimigos vizinhos
        }

        private void OnDisable()
        {
            if (_bobPosition) transform.localPosition = _baseLocalPos;
            transform.localScale = _baseScale;
            if (_renderer != null) _renderer.color = _baseColor;
        }

        /// <summary>
        /// Liga/desliga o bob de POSICAO. Deixe false em objetos movidos por IA
        /// (inimigos): squash e flash continuam, mas a posicao fica com o movimento.
        /// </summary>
        public void SetBobPosition(bool enabled) => _bobPosition = enabled;

        private void Update()
        {
            float t = Time.time * idleBobSpeed + _phase;
            float bob = Mathf.Sin(t) * idleBobAmplitude;
            float squash = Mathf.Cos(t * 2f) * idleSquash;

            // posicao (bob + lunge) so quando o objeto NAO e movido por IA externa
            if (_bobPosition)
            {
                Vector3 pos = _baseLocalPos + Vector3.up * bob;
                if (_lungeTimer > 0f && lungeDuration > 0f)
                {
                    _lungeTimer -= Time.deltaTime;
                    float k = Mathf.Sin(Mathf.Clamp01(1f - _lungeTimer / lungeDuration) * Mathf.PI); // ida e volta
                    pos += (Vector3)(_lungeDir * (lungeDistance * k));
                }
                transform.localPosition = pos;
            }

            // squash (escala) e sempre seguro — a movimentacao da IA nao mexe na escala
            transform.localScale = new Vector3(
                _baseScale.x * (1f + squash),
                _baseScale.y * (1f - squash),
                _baseScale.z);

            if (_flashTimer > 0f && _renderer != null)
            {
                _flashTimer -= Time.deltaTime;
                float k = hitFlashDuration > 0f ? Mathf.Clamp01(_flashTimer / hitFlashDuration) : 0f;
                _renderer.color = Color.Lerp(_baseColor, hitFlashColor, k);
            }
        }

        /// <summary>Redefine a escala base do squash. Chame ao redimensionar o transform em runtime
        /// depois deste componente ja ter cacheado a escala no Awake (ex.: o EnemyAnimator ajusta a
        /// escala para a folha animada). Sem isto, o squash reverteria para a escala antiga todo frame.</summary>
        public void SetBaseScale(Vector3 scale) => _baseScale = scale;

        /// <summary>Define a cor base (chame ao trocar o tint do sprite em runtime).</summary>
        public void SetBaseColor(Color color)
        {
            _baseColor = color;
            if (_renderer != null && _flashTimer <= 0f) _renderer.color = color;
        }

        /// <summary>Pisca rapido (feedback de dano recebido).</summary>
        public void Flash() => _flashTimer = hitFlashDuration;

        /// <summary>Lunge curto numa direcao (feedback de ataque).</summary>
        public void Lunge(Vector2 dir)
        {
            _lungeDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            _lungeTimer = lungeDuration;
        }
    }
}
