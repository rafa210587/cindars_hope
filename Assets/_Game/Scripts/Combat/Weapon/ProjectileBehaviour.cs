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

        private Vector2 _spawnPosition;
        private bool _hasHit;

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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hasHit)
                return;

            var enemyHealth = collision.GetComponentInParent<EnemyHealth>() ?? collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                _hasHit = true;
                HitEnemy(enemyHealth);
                Destroy(gameObject);
            }
        }

        private void HitEnemy(EnemyHealth enemyHealth)
        {
            var damageRequest = new DamageRequest(enemyHealth.EnemyId, _baseDamage)
            {
                DamageType = _damageType,
                SourcePosition = transform.position,
                KnockbackForce = _knockbackForce
            };

            var result = DamageCalculator.Calculate(damageRequest);
            enemyHealth.TakeDamage(damageRequest);

            if (!enemyHealth.IsDead && _statusEffect != null && _statusApplyChance > 0f && Random.value <= _statusApplyChance)
            {
                enemyHealth.ApplyStatusEffect(_statusEffect);
            }
        }

        public void Initialize(Vector2 direction, float speed, float range, int baseDamage, DamageType damageType, float knockbackForce)
        {
            _speed = speed;
            _range = range;
            _baseDamage = baseDamage;
            _damageType = damageType;
            _knockbackForce = knockbackForce;
            _spawnPosition = transform.position;

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = direction.normalized * _speed;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public void InitializeWithStatus(Vector2 direction, float speed, float range, int baseDamage, DamageType damageType, float knockbackForce, CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect, float statusApplyChance)
        {
            _statusEffect = statusEffect;
            _statusApplyChance = statusApplyChance;
            Initialize(direction, speed, range, baseDamage, damageType, knockbackForce);
        }
    }
}
