using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Combat.StatusEffect
{
    // Ticks active status effects on the host enemy every 1 real second.
    // DurationTurns is treated as tick count (1 tick = 1 second) in the MVP runtime.
    // F01: semÃ¢ntica genÃ©rica por tipo via StatusEffectSemantics (substitui o caso especial
    // de burn hardcoded) â€” DoT, slow/chill, root, fear, confusÃ£o e corrupÃ§Ã£o.
    [DisallowMultipleComponent]
    public class EnemyStatusRuntimeTicker : MonoBehaviour
    {
        private const float TickWindowSeconds = 1.1f; // cobre o intervalo atÃ© o prÃ³ximo tick

        private EnemyHealth _enemyHealth;
        private EnemyBrain _enemyBrain;
        private readonly HashSet<string> _announcedEffects = new HashSet<string>();

        private void Start()
        {
            _enemyHealth = GetComponent<EnemyHealth>();
            _enemyBrain = GetComponent<EnemyBrain>();
            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        private void Tick()
        {
            if (_enemyHealth == null || _enemyHealth.IsDead)
            {
                CancelInvoke(nameof(Tick));
                return;
            }

            var database = CindarsHope.Core.Bootstrap.GameBootstrap.Instance?.StatusEffectDatabase;
            var statusEffects = _enemyHealth.StatusEffects;
            var activeEffects = statusEffects.GetActiveEffects();

            var speedFactor = 1f;
            var invertMovement = false;
            var fearSeconds = 0f;

            foreach (var active in activeEffects)
            {
                if (active == null || string.IsNullOrWhiteSpace(active.StatusEffectId))
                {
                    continue;
                }

                StatusEffectSO effect = null;
                if (database == null || !database.TryGetById(active.StatusEffectId, out effect) || effect == null)
                {
                    continue;
                }

                if (_announcedEffects.Add(active.StatusEffectId))
                {
                    GameEventBus.Publish(new StatusEffectAppliedEvent(_enemyHealth.EnemyId, active.StatusEffectId));
                }

                if (StatusEffectSemantics.IsDamageOverTime(effect.Type) && effect.DamagePerTurn > 0)
                {
                    var dotRequest = new DamageRequest(_enemyHealth.EnemyId, effect.DamagePerTurn)
                    {
                        DamageType = StatusEffectSemantics.GetDamageType(effect.Type),
                        IsDamageOverTimeTick = true,
                        SourcePosition = transform.position,
                        KnockbackForce = 0f
                    };
                    _enemyHealth.TakeDamage(dotRequest);
                }

                speedFactor = Mathf.Min(speedFactor, StatusEffectSemantics.GetMoveSpeedFactor(effect));

                if (StatusEffectSemantics.InvertsMovement(effect.Type))
                {
                    invertMovement = true;
                }

                if (StatusEffectSemantics.ForcesRetreat(effect.Type))
                {
                    fearSeconds = Mathf.Max(fearSeconds, effect.BehaviorOverrideSeconds > 0f ? effect.BehaviorOverrideSeconds : TickWindowSeconds);
                }
            }

            if (_enemyBrain != null && (speedFactor < 1f || invertMovement || fearSeconds > 0f))
            {
                _enemyBrain.ApplyExternalBehaviorOverride(
                    speedFactor,
                    TickWindowSeconds,
                    invertMovement,
                    TickWindowSeconds,
                    fearSeconds > 0f,
                    fearSeconds);
            }

            statusEffects.TickStatusEffects();

            // Limpa anÃºncios de efeitos que expiraram (permite re-anunciar reaplicaÃ§Ãµes).
            _announcedEffects.RemoveWhere(id => !statusEffects.HasStatusEffect(id));
        }
    }
}
