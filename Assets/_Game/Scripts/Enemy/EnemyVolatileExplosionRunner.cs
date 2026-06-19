using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_24 — detached runner for a Volatile elite's death explosion.
    ///
    /// A Volatile elite is deactivated synchronously by <c>EnemyHealth.Die()</c> the instant it dies,
    /// which would kill any coroutine on the enemy itself. So the blast is delegated to this tiny,
    /// independent GameObject (same survival pattern as <c>EnemyProjectileBehaviour</c>): it shows the
    /// telegraph window, then deals the (already capped) damage only if the player is still inside the
    /// blast radius — the player can dodge by leaving the area during the telegraph. Damage is capped
    /// at 25% of player maxHP by the caller via <see cref="EliteAffixRules.ResolveVolatileExplosionDamage"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyVolatileExplosionRunner : MonoBehaviour
    {
        private const float BlastRadius = 2.2f;

        private float _telegraphSeconds;
        private int _damage;
        private string _sourceEnemyId = "enemy";
        private float _timer;
        private bool _detonated;

        public static EnemyVolatileExplosionRunner Spawn(Vector2 position, float telegraphSeconds, int damage, string sourceEnemyId)
        {
            var go = new GameObject($"VolatileExplosion_{sourceEnemyId}");
            go.transform.position = position;
            var runner = go.AddComponent<EnemyVolatileExplosionRunner>();
            runner._telegraphSeconds = Mathf.Max(0f, telegraphSeconds);
            runner._damage = Mathf.Max(0, damage);
            runner._sourceEnemyId = string.IsNullOrWhiteSpace(sourceEnemyId) ? "enemy" : sourceEnemyId;
            return runner;
        }

        private void Update()
        {
            if (_detonated)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < _telegraphSeconds)
            {
                return;
            }

            _detonated = true;
            Detonate();
            GameEventBus.Publish(new EnemyTelegraphEndedEvent(_sourceEnemyId));
            Destroy(gameObject);
        }

        private void Detonate()
        {
            if (_damage <= 0)
            {
                return;
            }

            var playerManager = GameBootstrap.Instance?.PlayerManager;
            if (playerManager == null)
            {
                return;
            }

            var playerObject = playerManager.gameObject;
            float dist = Vector2.Distance(transform.position, playerObject.transform.position);
            if (dist > BlastRadius)
            {
                Debug.Log($"CombatLog: EliteVolatileBlastDodged. EnemyId={_sourceEnemyId}, Distance={dist:F2}, Radius={BlastRadius:F2}.");
                return;
            }

            int applied = PlayerDamageReceiver.ApplyDamage(playerManager, _damage, _sourceEnemyId, DamageType.Physical, gameObject);
            GameEventBus.Publish(new PlayerDamagedEvent(applied, transform.position, _sourceEnemyId, _sourceEnemyId));
            Debug.Log($"CombatLog: EliteVolatileBlastHit. EnemyId={_sourceEnemyId}, Damage={applied}.");
        }
    }
}
