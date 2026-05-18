using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class EnemyContactDamage : MonoBehaviour
    {
        [SerializeField] private EnemyDataSO _enemyData;
        [SerializeField] private Collider2D _collider;

        private PlayerManager _playerManager;
        private float _lastDamageTime;

        private void Start()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (_enemyData == null)
            {
                return;
            }

            if (_playerManager == null && collision.CompareTag("Player"))
            {
                _playerManager = collision.GetComponent<PlayerManager>();
            }

            if (_playerManager != null && collision.CompareTag("Player"))
            {
                if (Time.time >= _lastDamageTime + _enemyData.contactDamageCooldownSeconds)
                {
                    _playerManager.DamageHP(_enemyData.contactDamage);
                    _lastDamageTime = Time.time;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                _playerManager = null;
            }
        }
    }
}
