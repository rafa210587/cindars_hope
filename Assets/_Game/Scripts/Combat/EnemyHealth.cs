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

            _currentHp -= amount;
            _currentHp = Mathf.Max(0, _currentHp);

            if (_currentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            GameEventBus.Publish(new EnemyKilledEvent(
                _enemyData.enemyId,
                _enemyData.dropItemId,
                _enemyData.dropAmount,
                transform.position));

            gameObject.SetActive(false);
        }
    }
}
