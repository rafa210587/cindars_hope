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

        private void Start()
        {
            if (_enemyData == null)
            {
                Debug.LogWarning($"EnemyHealth on '{name}' has no EnemyDataSO assigned.", this);
                return;
            }

            _currentHp = _enemyData.maxHp;
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

            _currentHp -= damageResult.FinalDamage;
            _currentHp = Mathf.Max(0, _currentHp);
            Debug.Log($"EnemyHealth: {name} took {damageResult.FinalDamage} damage. HP {_currentHp}/{_enemyData.maxHp}.");

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
                    Debug.Log($"EnemyHealth: {name} knockback applied. Force: {finalForce}.");
                }
            }

            if (_currentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"EnemyHealth: {name} died.");
            Debug.Log($"EnemyHealth: publishing EnemyKilledEvent enemy={_enemyData.enemyId}, drop={_enemyData.dropItemId} x{_enemyData.dropAmount}.");
            GameEventBus.Publish(new EnemyKilledEvent(
                _enemyData.enemyId,
                _enemyData.dropItemId,
                _enemyData.dropAmount,
                transform.position,
                PlayerProgressionRules.CalculateEnemyXpReward(
                    _enemyData.enemyLevel,
                    _enemyData.baseDifficulty,
                    _enemyData.xpRewardOverride)));

            gameObject.SetActive(false);
        }
    }
}
