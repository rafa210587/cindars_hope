using System;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class CraftBombProjectile : MonoBehaviour
    {
        private Vector2 _origin;
        private Vector2 _destination;
        private float _duration;
        private float _elapsed;
        private int _damage;
        private DamageType _damageType;
        private string _sourceId;

        public static bool TryLaunch(Vector2 origin, Vector2 direction, float range, float speed,
            int damage, DamageType damageType, string sourceId)
        {
            var projectile = RuntimeProjectileFactory.Create(ProjectileVisualStyle.MagicBolt, damageType);
            if (projectile == null) return false;
            var standard = projectile.GetComponent<ProjectileBehaviour>();
            if (standard != null) standard.enabled = false;
            var collider = projectile.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            var body = projectile.GetComponent<Rigidbody2D>();
            if (body != null) body.simulated = false;
            projectile.transform.position = origin;
            projectile.AddComponent<CraftBombProjectile>().Configure(origin,
                origin + direction.normalized * range, Math.Max(.2f, range / speed),
                damage, damageType, sourceId);
            return true;
        }

        private void Configure(Vector2 origin, Vector2 destination, float duration,
            int damage, DamageType damageType, string sourceId)
        {
            _origin = origin;
            _destination = destination;
            _duration = duration;
            _damage = damage;
            _damageType = damageType;
            _sourceId = sourceId ?? string.Empty;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _duration > 0f ? Mathf.Clamp01(_elapsed / _duration) : 1f;
            Vector2 position = Vector2.Lerp(_origin, _destination, t);
            position.y += 1.1f * 4f * t * (1f - t);
            transform.position = position;
            if (t >= 1f) Explode();
        }

        private void Explode()
        {
            var center = _destination;
            var targets = new List<(EnemyHealth enemy, float distance, string id)>();
            foreach (var enemy in EnemyHealth.ActiveInstances)
            {
                if (enemy == null || enemy.IsDead) continue;
                float distance = ((Vector2)enemy.transform.position - center).sqrMagnitude;
                if (distance <= CraftBombRules.ExplosionRadius * CraftBombRules.ExplosionRadius)
                    targets.Add((enemy, distance, enemy.GetEntityId().ToString()));
            }
            targets.Sort((a, b) =>
            {
                int distance = a.distance.CompareTo(b.distance);
                return distance != 0 ? distance : string.CompareOrdinal(a.id, b.id);
            });
            var hitIds = new HashSet<string>(StringComparer.Ordinal);
            int count = Math.Min(CraftBombRules.MaximumTargets, targets.Count);
            for (int i = 0; i < count; i++)
            {
                var enemy = targets[i].enemy;
                string hitId = string.IsNullOrWhiteSpace(enemy.EnemyInstanceId)
                    ? enemy.GetEntityId().ToString()
                    : enemy.EnemyInstanceId;
                if (!hitIds.Add(hitId)) continue;
                enemy.TakeDamage(new DamageRequest(enemy.EnemyId, _damage, _damageType, _sourceId)
                {
                    SourcePosition = center,
                    KnockbackForce = 2f
                });
            }
            Destroy(gameObject);
        }
    }
}
