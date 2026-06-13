using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    /// <summary>
    /// Procedural in-flight animation for projectiles while 2D art is not authored.
    /// Magic bolts pulse scale and flicker brightness; arrows keep a stable shaft with a
    /// subtle wobble. All animation is local-only and never affects physics.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileVisualAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private float _pulseFrequency = 9f;
        [SerializeField] private float _pulseAmplitude = 0.16f;
        [SerializeField] private float _flickerAmplitude = 0.12f;
        [SerializeField] private bool _spin;
        [SerializeField] private float _spinDegreesPerSecond = 540f;

        private Vector3 _baseScale;
        private Color _baseColor;
        private float _phaseOffset;

        public void Configure(SpriteRenderer renderer, bool pulse, bool spin)
        {
            _renderer = renderer;
            _spin = spin;
            if (!pulse)
            {
                _pulseAmplitude = 0f;
                _flickerAmplitude = 0f;
            }
        }

        private void Start()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<SpriteRenderer>();
            }

            _baseScale = transform.localScale;
            _baseColor = _renderer != null ? _renderer.color : Color.white;
            // Desync pulse phases between projectiles (visual-only randomness).
            _phaseOffset = Random.value * 6.28f;
        }

        private void Update()
        {
            float wave = Mathf.Sin((Time.time + _phaseOffset) * _pulseFrequency * Mathf.PI * 2f);

            if (_pulseAmplitude > 0f)
            {
                transform.localScale = _baseScale * (1f + wave * _pulseAmplitude);
            }

            if (_renderer != null && _flickerAmplitude > 0f)
            {
                float brightness = 1f + wave * _flickerAmplitude;
                _renderer.color = new Color(
                    Mathf.Clamp01(_baseColor.r * brightness),
                    Mathf.Clamp01(_baseColor.g * brightness),
                    Mathf.Clamp01(_baseColor.b * brightness),
                    _baseColor.a);
            }

            if (_spin)
            {
                transform.Rotate(0f, 0f, _spinDegreesPerSecond * Time.deltaTime);
            }
        }
    }
}
