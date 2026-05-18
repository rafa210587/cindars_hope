using CindarsHope.Core;
using CindarsHope.Core.Events;
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
            if (_enemyData == null)
            {
                return;
            }

            if (amount <= 0)
            {
                return;
            }

            if (_currentHp <= 0)
            {
                return;
            }

            _currentHp -= amount;
            _currentHp = Mathf.Max(0, _currentHp);
            Debug.Log($"EnemyHealth: {name} took {amount} damage. HP {_currentHp}/{_enemyData.maxHp}.");

            if (_currentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"EnemyHealth: {name} died.");
            GameEventBus.Publish(new EnemyKilledEvent(
                _enemyData.enemyId,
                _enemyData.dropItemId,
                _enemyData.dropAmount,
                transform.position));

            gameObject.SetActive(false);
        }
    }
}
