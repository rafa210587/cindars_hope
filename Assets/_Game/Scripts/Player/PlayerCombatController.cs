using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerCombatController : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private int _baseDamage = 10;
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private float _attackRange = 1.5f;

        private float _lastAttackTime;
        private bool _canAttack = true;

        public int BaseDamage => _baseDamage;
        public float AttackRange => _attackRange;
        public bool CanAttack => _canAttack && _lastAttackTime + _attackCooldown <= Time.time;

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerHitEvent>(OnPlayerHit);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerHitEvent>(OnPlayerHit);
        }

        public bool TryAttack(Vector2 direction, int? staminaCost = null)
        {
            if (!CanAttack)
                return false;

            int cost = staminaCost ?? Mathf.Max(1, Mathf.RoundToInt(_baseDamage * 0.2f));

            if (_staminaManager != null && !_staminaManager.TrySpendStamina(cost))
                return false;

            ExecuteAttack(direction);
            _lastAttackTime = Time.time;
            return true;
        }

        private void ExecuteAttack(Vector2 direction)
        {
            int weaponBonus = 0;
            int attributeBonus = _playerManager != null ? _playerManager.Strength : 0;
            float typeMultiplier = 1f;

            var damageResult = DamageCalculator.CalculateDirectDamage(_baseDamage, attributeBonus, typeMultiplier);

            GameEventBus.Publish(new PlayerAttackedEvent(_baseDamage + attributeBonus, direction, damageResult));

            if (_equipmentManager != null)
            {
                _equipmentManager.RegisterEquipmentUsage();
            }
        }

        public void ResetCooldown()
        {
            _lastAttackTime = Time.time - _attackCooldown;
        }

        private void OnPlayerHit(PlayerHitEvent evt)
        {
            // Handle player taking damage
            if (_playerManager != null)
            {
                _playerManager.DamageHP(evt.DamageAmount);
            }
        }
    }
}
