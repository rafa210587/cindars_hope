using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Player;
using CindarsHope.Skills;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class PlayerAttackController : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private ManaManager _manaManager;
        [SerializeField] private UnarmedAttackDataSO _unarmedFallback;
        [SerializeField] private float _knockbackForce = 2.5f;
        [SerializeField] private float _dodgeCooldownSeconds = 0.5f;
        [SerializeField] private float _dodgeStaminaCost = 20f;
        [SerializeField] private float _dodgeDistance = 2f;
        [SerializeField] private float _dodgeDurationSeconds = 0.2f;

        private float _lastLeftHandAttackTime;
        private float _lastRightHandAttackTime;
        private float _lastDodgeTime;
        private float _dodgeEndTime;
        private bool _isDodging;

        public void RebindStaminaManager(StaminaManager staminaManager)
        {
            _staminaManager = staminaManager;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryAttackLeftHand();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                TryAttackRightHand();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryDodge();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TryActivateSkillSlot(0);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                TryActivateSkillSlot(1);
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                TryActivateSkillSlot(2);
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                TryActivateSkillSlot(3);
            }

            UpdateDodgeState();
        }

        private void TryAttackLeftHand()
        {
            var equippedItemId = _equipmentManager?.GetEquippedItem(EquipmentSlot.LeftHand);
            AttackWithSlot(EquipmentSlot.LeftHand, ref _lastLeftHandAttackTime, equippedItemId);
        }

        private void TryAttackRightHand()
        {
            var equippedItemId = _equipmentManager?.GetEquippedItem(EquipmentSlot.RightHand);
            AttackWithSlot(EquipmentSlot.RightHand, ref _lastRightHandAttackTime, equippedItemId);
        }

        private void AttackWithSlot(EquipmentSlot slot, ref float lastAttackTime, string equippedItemId)
        {
            if (_isDodging)
                return;

            WeaponDataSO weapon = null;
            if (!string.IsNullOrEmpty(equippedItemId))
            {
                weapon = Resources.Load<WeaponDataSO>($"Weapons/{equippedItemId}");
            }

            if (weapon == null)
            {
                weapon = GetWeaponAsset(_unarmedFallback?.Id ?? "unarmed_default");
            }

            if (weapon == null && _unarmedFallback == null)
                return;

            float cooldown = weapon?.BaseCooldownSeconds ?? _unarmedFallback.BaseCooldownSeconds;
            if (weapon != null)
            {
                float attackSpeed = weapon.AttackSpeedMultiplier;
                cooldown = cooldown / Mathf.Max(0.1f, attackSpeed);
            }

            if (Time.time < lastAttackTime + cooldown)
                return;

            float staminaCost = weapon?.StaminaCost ?? _unarmedFallback.StaminaCost;
            if (_staminaManager != null && !_staminaManager.TrySpendStamina((int)staminaCost))
                return;

            ExecuteWeaponAttack(weapon ?? ConvertUnarmedToWeapon(_unarmedFallback));
            lastAttackTime = Time.time;
        }

        private void ExecuteWeaponAttack(WeaponDataSO weapon)
        {
            Vector2 direction = _playerController?.LastFacingDirection ?? Vector2.right;
            var hitColliders = Physics2D.OverlapCircleAll((Vector2)transform.position + direction * 0.5f, weapon.Range);

            foreach (var collider in hitColliders)
            {
                if (collider.gameObject == gameObject)
                    continue;

                var enemyHealth = collider.GetComponentInParent<EnemyHealth>() ?? collider.GetComponent<EnemyHealth>();
                if (enemyHealth == null)
                    continue;

                var damageRequest = new DamageRequest(enemyHealth.EnemyId, weapon.BaseDamage)
                {
                    DamageType = weapon.DamageType,
                    SourcePosition = transform.position,
                    KnockbackForce = _knockbackForce
                };

                var result = DamageCalculator.Calculate(damageRequest);
                enemyHealth.TakeDamage(damageRequest);

                if (_equipmentManager != null)
                    _equipmentManager.RegisterEquipmentUsage();
            }
        }

        private void TryDodge()
        {
            if (Time.time < _lastDodgeTime + _dodgeCooldownSeconds)
                return;

            if (_staminaManager != null && !_staminaManager.TrySpendStamina((int)_dodgeStaminaCost))
                return;

            _isDodging = true;
            _dodgeEndTime = Time.time + _dodgeDurationSeconds;
            _lastDodgeTime = Time.time;

            Vector2 direction = _playerController?.MoveInput ?? Vector2.right;
            if (direction.sqrMagnitude < 0.01f)
                direction = _playerController?.LastFacingDirection ?? Vector2.right;

            var nextPosition = (Vector2)transform.position + direction.normalized * _dodgeDistance;
            if (_playerController != null)
            {
                _playerController.GetComponent<Rigidbody2D>().MovePosition(nextPosition);
            }
        }

        private void UpdateDodgeState()
        {
            if (_isDodging && Time.time >= _dodgeEndTime)
            {
                _isDodging = false;
            }
        }

        private void TryActivateSkillSlot(int slotIndex)
        {
            // Placeholder for skill activation - will be enhanced in future specs
        }

        private WeaponDataSO GetWeaponAsset(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
                return null;

            return Resources.Load<WeaponDataSO>($"Weapons/{weaponId}");
        }

        private WeaponDataSO ConvertUnarmedToWeapon(UnarmedAttackDataSO unarmed)
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
            weapon.Id = unarmed.Id;
            weapon.DisplayName = unarmed.DisplayName;
            weapon.BaseDamage = unarmed.BaseDamage;
            weapon.BaseCooldownSeconds = unarmed.BaseCooldownSeconds;
            weapon.StaminaCost = unarmed.StaminaCost;
            weapon.Range = unarmed.Range;
            weapon.ArcDegrees = unarmed.ArcDegrees;
            weapon.DamageType = unarmed.DamageType;
            return weapon;
        }
    }
}
