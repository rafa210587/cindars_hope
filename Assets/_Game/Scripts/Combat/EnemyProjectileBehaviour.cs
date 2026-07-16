using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Combat
{
    /// <summary>
    /// Projectile fired by enemies (EnemyBrain RangedProjectile/CastProjectile actions).
    /// Travels in a straight line, damages the player on contact via the same
    /// PlayerManager.DamageHP + PlayerDamagedEvent path used by EnemyContactDamage.
    /// Spawned via SpawnTowards with procedural visuals (RuntimeProjectileFactory style).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyProjectileBehaviour : MonoBehaviour
    {
        private int _damage;
        private float _range;
        private float _knockbackForce;
        private string _sourceEnemyId;
        private string _sourceEnemyName;
        private Vector2 _spawnPosition;
        private bool _hasHit;
        private DamageType _damageType = DamageType.Physical;

        public static EnemyProjectileBehaviour SpawnTowards(
            Vector2 origin,
            Vector2 direction,
            float speed,
            float range,
            int damage,
            DamageType damageType,
            float knockbackForce,
            string sourceEnemyId,
            string sourceEnemyName)
        {
            if (direction.sqrMagnitude < 0.001f)
            {
                return null;
            }

            var projectileObject = RuntimeProjectileFactory.Create(ProjectileVisualStyle.MagicBolt, damageType);
            // Player-targeting projectile must not reuse the player ProjectileBehaviour added
            // by the factory; replace it with the enemy-side behaviour.
            var playerSideBehaviour = projectileObject.GetComponent<ProjectileBehaviour>();
            if (playerSideBehaviour != null)
            {
                Destroy(playerSideBehaviour);
            }

            projectileObject.name = $"EnemyProjectile_{sourceEnemyId}";
            projectileObject.transform.position = origin + direction.normalized * 0.45f;

            var behaviour = projectileObject.AddComponent<EnemyProjectileBehaviour>();
            behaviour._damage = Mathf.Max(1, damage);
            behaviour._range = Mathf.Max(1f, range);
            behaviour._knockbackForce = knockbackForce;
            behaviour._sourceEnemyId = sourceEnemyId ?? string.Empty;
            behaviour._sourceEnemyName = sourceEnemyName ?? string.Empty;
            behaviour._spawnPosition = projectileObject.transform.position;
            behaviour._damageType = damageType;

            var body = projectileObject.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = direction.normalized * Mathf.Max(1f, speed);
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectileObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            return behaviour;
        }

        private void Update()
        {
            if (Vector2.Distance(transform.position, _spawnPosition) > _range)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hasHit)
            {
                return;
            }

            var playerController = collision.GetComponentInParent<PlayerController>() ?? collision.GetComponent<PlayerController>();
            if (playerController == null)
            {
                return;
            }

            _hasHit = true;
            var playerManager = GameBootstrap.Instance?.PlayerManager as CindarsHope.Player.PlayerManager;
            if (playerManager != null)
            {
                // F03/F18: reduÃ§Ã£o central por Defense + resistÃªncia do tipo de dano.
                var finalDamage = PlayerDamageReceiver.ApplyDamage(playerManager, _damage, _sourceEnemyId, _damageType);
                GameEventBus.Publish(new PlayerDamagedEvent(finalDamage, transform.position, _sourceEnemyId, _sourceEnemyName));
                FloatingDamageNumberDisplayer.ShowAtTarget(playerController.gameObject, finalDamage, _damageType, false, true);

                if (_knockbackForce > 0f)
                {
                    var knockback = collision.GetComponentInParent<KnockbackController>() ?? collision.GetComponent<KnockbackController>();
                    if (knockback != null)
                    {
                        Vector2 direction = (collision.transform.position - transform.position).normalized;
                        knockback.ApplyKnockback(direction, _knockbackForce);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
