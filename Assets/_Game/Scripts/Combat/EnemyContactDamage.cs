using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
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

        public void Configure(EnemyDataSO enemyData, Collider2D collider)
        {
            _enemyData = enemyData;
            _collider = collider;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (_enemyData == null)
            {
                return;
            }

            var playerController = collision.GetComponentInParent<PlayerController>();
            if (playerController == null)
            {
                playerController = collision.GetComponent<PlayerController>();
            }

            if (playerController == null)
            {
                return;
            }

            if (_playerManager == null)
            {
                if (GameBootstrap.Instance != null && GameBootstrap.Instance.PlayerManager != null)
                {
                    _playerManager = GameBootstrap.Instance.PlayerManager;
                }
                else
                {
                    _playerManager = collision.GetComponentInParent<PlayerManager>();
                    if (_playerManager == null)
                    {
                        _playerManager = collision.GetComponent<PlayerManager>();
                    }
                }
            }

            if (_playerManager != null)
            {
                if (Time.time >= _lastDamageTime + _enemyData.contactDamageCooldownSeconds)
                {
                    _playerManager.DamageHP(_enemyData.contactDamage);
                    Debug.Log($"CombatLog: EnemyContactDamage. SourceName={_enemyData.DisplayName}, SourceEnemyId={_enemyData.enemyId}, Target=Player, Damage={_enemyData.contactDamage}.");
                    GameEventBus.Publish(new PlayerDamagedEvent(_enemyData.contactDamage, collision.transform.position, _enemyData.enemyId, _enemyData.DisplayName));
                    // SPEC 14A-FIX10: popup at the actual player object, not the trigger position.
                    FloatingDamageNumberDisplayer.ShowAtTarget(playerController.gameObject, _enemyData.contactDamage, DamageType.Physical, false, true);

                    var playerHitFlash = collision.GetComponentInParent<HitFlashController>();
                    if (playerHitFlash == null)
                    {
                        playerHitFlash = collision.GetComponent<HitFlashController>();
                    }
                    if (playerHitFlash != null)
                    {
                        playerHitFlash.Flash();
                    }

                    if (_enemyData.contactKnockbackForce > 0f)
                    {
                        var playerKnockback = collision.GetComponentInParent<KnockbackController>();
                        if (playerKnockback == null)
                        {
                            playerKnockback = collision.GetComponent<KnockbackController>();
                        }
                        if (playerKnockback != null)
                        {
                            Vector2 direction = (collision.transform.position - transform.position).normalized;
                            playerKnockback.ApplyKnockback(direction, _enemyData.contactKnockbackForce);
                        }
                    }

                    _lastDamageTime = Time.time;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var playerController = collision.GetComponentInParent<PlayerController>();
            if (playerController == null)
            {
                playerController = collision.GetComponent<PlayerController>();
            }

            if (playerController != null)
            {
                _playerManager = null;
            }
        }
    }
}
