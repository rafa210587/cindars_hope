using CindarsHope.Cave.Runtime;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;
using CindarsHope.Player.Progression;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private EnemyDataSO _enemyData;

        private int _currentHp;
        private bool _hpRestoredFromSnapshot;
        private CindarsHope.Combat.StatusEffect.StatusEffectManager _statusEffects = new CindarsHope.Combat.StatusEffect.StatusEffectManager();

        public int CurrentHp => _currentHp;
        public int MaxHp => _enemyData != null ? _enemyData.maxHp : 0;
        public string EnemyId => _enemyData != null ? _enemyData.enemyId : string.Empty;
        public string DisplayName => _enemyData != null && !string.IsNullOrWhiteSpace(_enemyData.DisplayName) ? _enemyData.DisplayName : name;
        public CindarsHope.Combat.StatusEffect.StatusEffectManager StatusEffects => _statusEffects;
        // SPEC 14A-FIX10: expose IsDead so EnemyBrain/external controllers can check death state.
        // Previously a duplicate CindarsHope.Enemy.EnemyHealth in /Enemy/ provided this; removed
        // in FIX10 because it was a parallel/legacy class that never got Configure'd at runtime.
        public bool IsDead => _enemyData != null && _currentHp <= 0;

        public void Configure(EnemyDataSO enemyData)
        {
            _enemyData = enemyData;
            if (_enemyData != null)
            {
                _currentHp = _enemyData.maxHp;
                Debug.Log($"CombatLog: Enemy configured. {BuildEnemyLogPrefix()}, HP={_currentHp}/{MaxHp}, Level={_enemyData.enemyLevel}, Difficulty={_enemyData.baseDifficulty}.", this);
            }

            if (GetComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>() == null)
                gameObject.AddComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>();
        }

        // F13: restaura HP salvo do snapshot da run (chamado APÓS Configure, antes do Start).
        // O guard impede o Start de resetar o valor restaurado para o máximo.
        public void RestoreHp(int savedHp)
        {
            if (_enemyData == null)
            {
                return;
            }

            _currentHp = Mathf.Clamp(savedHp, 0, _enemyData.maxHp);
            _hpRestoredFromSnapshot = true;
        }

        private void Start()
        {
            if (_enemyData == null)
            {
                Debug.LogWarning($"EnemyHealth on '{name}' has no EnemyDataSO assigned.", this);
                return;
            }

            if (_hpRestoredFromSnapshot)
            {
                return;
            }

            _currentHp = _enemyData.maxHp;
            Debug.Log($"CombatLog: Enemy spawned. {BuildEnemyLogPrefix()}, HP={_currentHp}/{MaxHp}, Level={_enemyData.enemyLevel}, Difficulty={_enemyData.baseDifficulty}.", this);

            if (GetComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>() == null)
                gameObject.AddComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>();
        }

        public void ApplyStatusEffect(CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect)
        {
            if (statusEffect != null)
            {
                _statusEffects.ApplyStatusEffect(statusEffect);
                Debug.Log($"CombatLog: Applied status effect '{statusEffect.DisplayName}' to {DisplayName}.", this);
            }
        }

        public void TakeDamage(int amount)
        {
            var request = new DamageRequest(EnemyId, amount);
            request.SourcePosition = transform.position;
            request.KnockbackForce = 0f;
            TakeDamage(request);
        }

        public void TakeDamage(DamageRequest request)
        {
            if (_enemyData == null)
            {
                return;
            }

            if (request.Amount <= 0)
            {
                return;
            }

            if (_currentHp <= 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.TargetId))
            {
                request.TargetId = EnemyId;
            }

            var vulnerabilityState = GetComponent<EnemyVulnerabilityState>();
            float vulnerabilityMultiplier = vulnerabilityState != null && vulnerabilityState.IsVulnerable
                ? vulnerabilityState.Multiplier
                : 1f;

            var damageResult = DamageCalculator.Calculate(request, _enemyData.defense, null, vulnerabilityMultiplier);
            if (damageResult.FinalDamage <= 0)
            {
                return;
            }

            var hpBefore = _currentHp;
            _currentHp -= damageResult.FinalDamage;
            _currentHp = Mathf.Max(0, _currentHp);
            Debug.Log($"CombatLog: Hit enemy. {BuildEnemyLogPrefix()}, Damage={damageResult.FinalDamage}, HP={hpBefore}->{_currentHp}/{MaxHp}.", this);

            GameEventBus.Publish(new DamageAppliedEvent(damageResult, transform.position));
            // SPEC 14A-FIX10: explicit show-at-target so popup lands above this enemy's collider
            // top, not at the OverlapPoint guess (which is unreliable for fast-moving enemies).
            FloatingDamageNumberDisplayer.ShowAtTarget(gameObject, damageResult.FinalDamage, damageResult.DamageType, damageResult.WasImmune, false);

            var hitFlash = GetComponentInChildren<HitFlashController>();
            if (hitFlash != null)
            {
                hitFlash.Flash();
            }

            if (request.KnockbackForce > 0f)
            {
                var knockback = GetComponent<KnockbackController>();
                if (knockback != null)
                {
                    Vector3 currentPosition = transform.position;
                    Vector3 direction = (currentPosition - request.SourcePosition).normalized;
                    float finalForce = request.KnockbackForce * _enemyData.receivedKnockbackMultiplier;
                    knockback.ApplyKnockback((Vector2)direction, finalForce);
                    Debug.Log($"CombatLog: Knockback enemy. {BuildEnemyLogPrefix()}, Force={finalForce}, HP={_currentHp}/{MaxHp}.", this);
                }
            }

            if (_currentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            var xpReward = PlayerProgressionRules.CalculateEnemyXpReward(
                _enemyData.enemyLevel,
                _enemyData.baseDifficulty,
                _enemyData.xpRewardOverride);

            Debug.Log($"CombatLog: Enemy defeated. {BuildEnemyLogPrefix()}, HP=0/{MaxHp}, Drop={_enemyData.dropItemId} x{_enemyData.dropAmount}, XP={xpReward}.", this);

            var bossReporter = GetComponent<CaveBossDeathReporter>();
            if (bossReporter != null)
            {
                bossReporter.ReportDefeatedFromOwner(transform.position);
            }

            GameEventBus.Publish(new EnemyKilledEvent(
                _enemyData.enemyId,
                _enemyData.dropItemId,
                _enemyData.dropAmount,
                transform.position,
                xpReward));

            gameObject.SetActive(false);
        }

        private string BuildEnemyLogPrefix()
        {
            var bossReporter = GetComponent<CaveBossDeathReporter>();
            if (bossReporter == null)
            {
                return $"Name={DisplayName}, EnemyId={EnemyId}, IsBoss=false";
            }

            return $"Name={DisplayName}, EnemyId={EnemyId}, IsBoss=true, BossGateId={bossReporter.BossGateId}, CaveLevel={bossReporter.CaveLevel}, CheckpointUnlock={bossReporter.CheckpointUnlockedOnDefeat}";
        }
    }
}
