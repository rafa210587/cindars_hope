using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int _maxHp = 10;
        private int _currentHp;
        public bool IsDead => _currentHp <= 0;

        private void Start()
        {
            _currentHp = _maxHp;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead)
                return;

            _currentHp = Mathf.Max(0, _currentHp - amount);
            GameEventBus.Publish(new EnemyDamagedEvent(gameObject.name, amount));

            if (IsDead)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
        }

        private void Die()
        {
            GetComponent<EnemyBrain>()?.SetState(EnemyBrainState.Dead);
            GameEventBus.Publish(new EnemyKilledEvent(gameObject.name, transform.position));
            Destroy(gameObject);
        }

        public int CurrentHp => _currentHp;
        public float HealthPercentage => _currentHp / (float)_maxHp;
    }
}
