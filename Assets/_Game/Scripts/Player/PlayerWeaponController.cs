using CindarsHope.Core;
using CindarsHope.Equipment;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerWeaponController : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private float _attackCooldown = 0.5f;

        private float _lastAttackTime = float.MinValue;

        public bool CanAttack => Time.time - _lastAttackTime >= _attackCooldown;

        public bool TryAttackWithWeapon(Vector2 direction)
        {
            if (!CanAttack)
                return false;

            ExecuteWeaponAttack(direction);
            _lastAttackTime = Time.time;
            return true;
        }

        private void ExecuteWeaponAttack(Vector2 direction)
        {
            int damage = 10;
            if (_playerManager != null)
            {
                damage += _playerManager.Strength;
            }

            GameEventBus.Publish(new CombatEventData(damage, direction));
        }
    }

    public readonly struct CombatEventData
    {
        public readonly int Damage;
        public readonly Vector2 Direction;

        public CombatEventData(int damage, Vector2 direction)
        {
            Damage = damage;
            Direction = direction;
        }
    }
}
