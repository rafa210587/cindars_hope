using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    [DisallowMultipleComponent]
    public class ProjectileBehaviour : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _range = 6f;
        [SerializeField] private int _baseDamage = 5;
        [SerializeField] private DamageType _damageType = DamageType.Physical;
        [SerializeField] private float _knockbackForce = 2f;
        [SerializeField] private Rigidbody2D _rigidbody;
        // Keep this serialized field as Collider2D so projectile prefabs can use either
        // CircleCollider2D or BoxCollider2D without Unity YAML type mismatch errors.
        [SerializeField] private Collider2D _collider;
        [SerializeField] private CindarsHope.Combat.StatusEffect.StatusEffectSO _statusEffect;
        [SerializeField] private float _statusApplyChance = 0f;

        [SerializeField] private int _maxHits = 1;

        // fable_48 (aditivo): tags de material/elemento da munição (ex.: "Silver"/"Fire"). Anexadas
        // ao DamageRequest no impacto para o matching de vulnerabilidade F06. Null => sem tags.
        private string[] _appliedTags;

        private Vector2 _spawnPosition;
        private int _hitCount;

        // Game-feel de desaceleracao: a flecha comeca rapida e perde velocidade conforme avanca,
        // chegando a (_speedDecayToFraction * velocidade inicial) no alcance maximo. 1 = constante.
        private Vector2 _direction = Vector2.right;
        private float _initialSpeed;
        private float _speedDecayToFraction = 1f;

        private void Start()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null)
                _collider = GetComponent<Collider2D>();

            _spawnPosition = transform.position;
        }

        private void Update()
        {
            float distanceTraveled = Vector2.Distance(transform.position, _spawnPosition);
            if (distanceTraveled > _range)
            {
                Destroy(gameObject);
            }
        }

        // Desaceleracao por distancia (so quando _speedDecayToFraction < 1). Recalcula a velocidade
        // a cada passo de fisica: rapida no inicio, decaindo linearmente ate a fracao no alcance maximo.
        private void FixedUpdate()
        {
            if (_rigidbody == null || _initialSpeed <= 0f || _speedDecayToFraction >= 1f)
            {
                return;
            }

            float traveled = Vector2.Distance(_rigidbody.position, _spawnPosition);
            float t = _range > 0f ? Mathf.Clamp01(traveled / _range) : 0f;
            float currentSpeed = Mathf.Lerp(_initialSpeed, _initialSpeed * _speedDecayToFraction, t);
            _rigidbody.linearVelocity = _direction * currentSpeed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hitCount >= _maxHits)
                return;

            var enemyHealth = collision.GetComponentInParent<EnemyHealth>() ?? collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                _hitCount++;
                HitEnemy(enemyHealth);
                if (_hitCount >= _maxHits)
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>Allows piercing projectiles (line piercer skill). 1 = despawn on first hit.</summary>
        public void SetMaxHits(int maxHits)
        {
            _maxHits = Mathf.Max(1, maxHits);
        }

        /// <summary>
        /// fable_48 — anexa tags de material/elemento da munição (ex.: flecha "Silver"/"Fire"),
        /// propagadas ao DamageRequest no impacto para o matching de vulnerabilidade F06. Aditivo;
        /// chamado pelo ProjectileSpawnService. Null/vazio mantém o comportamento sem tags.
        /// </summary>
        public void SetAppliedTags(string[] appliedTags)
        {
            _appliedTags = appliedTags;
        }

        private void HitEnemy(EnemyHealth enemyHealth)
        {
            var damageRequest = new DamageRequest(enemyHealth.EnemyId, _baseDamage)
            {
                DamageType = _damageType,
                SourcePosition = transform.position,
                KnockbackForce = _knockbackForce,
                // fable_48: tags da flecha viajam até o matching F06 (bônus só com vulnerabilidade
                // declarada). Null/vazio para magias/projéteis sem tags (comportamento inalterado).
                WeaponMaterialTags = _appliedTags
            };

            var result = DamageCalculator.Calculate(damageRequest);
            enemyHealth.TakeDamage(damageRequest);

            if (!enemyHealth.IsDead && _statusEffect != null && _statusApplyChance > 0f && Random.value <= _statusApplyChance)
            {
                enemyHealth.ApplyStatusEffect(_statusEffect);
            }
        }

        public void Initialize(Vector2 direction, float speed, float range, int baseDamage, DamageType damageType, float knockbackForce, float speedDecayToFraction = 1f)
        {
            // Start() só roda no próximo frame; caminhos procedurais chamam Initialize no mesmo frame
            // do AddComponent — cacheia aqui para garantir que velocity e collider estejam disponíveis.
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null)
                _collider = GetComponent<Collider2D>();

            _speed = speed;
            _range = range;
            _baseDamage = baseDamage;
            _damageType = damageType;
            _knockbackForce = knockbackForce;
            _spawnPosition = transform.position;

            _direction = direction.normalized;
            _initialSpeed = speed;
            _speedDecayToFraction = speedDecayToFraction <= 0f ? 1f : Mathf.Clamp01(speedDecayToFraction);

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = _direction * _speed;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public void InitializeWithStatus(Vector2 direction, float speed, float range, int baseDamage, DamageType damageType, float knockbackForce, CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect, float statusApplyChance, float speedDecayToFraction = 1f)
        {
            _statusEffect = statusEffect;
            _statusApplyChance = statusApplyChance;
            Initialize(direction, speed, range, baseDamage, damageType, knockbackForce, speedDecayToFraction);
        }
    }
}
