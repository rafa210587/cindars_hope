using CindarsHope.Cave.Runtime;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Progression;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private EnemyDataSO _enemyData;

        private int _currentHp;
        private StatusEffectManager _statusEffects = new StatusEffectManager();

        public int CurrentHp => _currentHp;
        public int MaxHp => _enemyData != null ? _enemyData.maxHp : 0;
        public string EnemyId => _enemyData != null ? _enemyData.enemyId : string.Empty;
        public string DisplayName => _enemyData != null && !string.IsNullOrWhiteSpace(_enemyData.DisplayName) ? _enemyData.DisplayName : name;
        public StatusEffectManager StatusEffects => _statusEffects;

        public void Configure(EnemyDataSO enemyData)
        {
            _enemyData = enemyData;
            if (_enemyData != null)
            {
                _currentHp = _enemyData.maxHp;
                Debug.Log($"CombatLog: Enemy configured. {BuildEnemyLogPrefix()}, HP={_currentHp}/{MaxHp}, Level={_enemyData.enemyLevel}, Difficulty={_enemyData.baseDifficulty}.", this);
            }
        }

        private void Start()
        {
            if (_enemyData == null)
            {
                Debug.LogWarning($"EnemyHealth on '{name}' has no EnemyDataSO assigned.", this);
                return;
            }

            _currentHp = _enemyData.maxHp;
            Debug.Log($"CombatLog: Enemy spawned. {BuildEnemyLogPrefix()}, HP={_currentHp}/{MaxHp}, Level={_enemyData.enemyLevel}, Difficulty={_enemyData.baseDifficulty}.", this);
        }

        public void ApplyStatusEffect(StatusEffectSO statusEffect)
        {
            if (statusEffect != null)
            {
                _statusEffects.ApplyStatusEffect(statusEffect);
                Debug.Log($"CombatLog: Applied status effect '{statusEffect.DisplayName}' to {DisplayName}.", this);
            }
        }

        public void TakeDamage(int amount)
        {
            TakeDamage(new DamageRequest(amount, transform.position, 0f));
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

            var damageResult = DamageCalculator.CalculateDirectDamage(request.Amount);
            if (damageResult.FinalDamage <= 0)
            {
                return;
            }

            var hpBefore = _currentHp;
            _currentHp -= damageResult.FinalDamage;
            _currentHp = Mathf.Max(0, _currentHp);
            Debug.Log($"CombatLog: Hit enemy. {BuildEnemyLogPrefix()}, Damage={damageResult.FinalDamage}, HP={hpBefore}->{_currentHp}/{MaxHp}.", this);

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
                    Vector2 currentPosition = transform.position;
                    Vector2 direction = (currentPosition - request.SourcePosition).normalized;
                    float finalForce = request.KnockbackForce * _enemyData.receivedKnockbackMultiplier;
                    knockback.ApplyKnockback(direction, finalForce);
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