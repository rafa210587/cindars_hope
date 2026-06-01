using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Combat.StatusEffect
{
    // Ticks active status effects on the host enemy every 1 real second.
    // DurationTurns is treated as tick count (1 tick = 1 second) in the MVP runtime.
    [DisallowMultipleComponent]
    public class EnemyStatusRuntimeTicker : MonoBehaviour
    {
        private EnemyHealth _enemyHealth;
        private StatusEffectSO _burnSO;

        private void Start()
        {
            _enemyHealth = GetComponent<EnemyHealth>();
            _burnSO = Resources.Load<StatusEffectSO>("status_burn_test");

            if (_burnSO == null)
                Debug.LogWarning("EnemyStatusRuntimeTicker: status_burn_test not found in Resources.", this);

            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        private void Tick()
        {
            if (_enemyHealth == null || _enemyHealth.IsDead)
            {
                CancelInvoke(nameof(Tick));
                return;
            }

            var statusEffects = _enemyHealth.StatusEffects;

            if (_burnSO != null)
            {
                int burnDamage = statusEffects.GetStatusDamageThisTurn(_burnSO);
                if (burnDamage > 0)
                {
                    var dotRequest = new DamageRequest(_enemyHealth.EnemyId, burnDamage)
                    {
                        DamageType = DamageType.Fire,
                        IsDamageOverTimeTick = true,
                        SourcePosition = transform.position,
                        KnockbackForce = 0f
                    };
                    _enemyHealth.TakeDamage(dotRequest);
                }
            }

            statusEffects.TickStatusEffects();
        }
    }
}
