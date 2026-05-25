using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
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
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private int _baseDamage = 10;
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private float _attackRange = 1.5f;

        private float _leftHandCooldownEnd;
        private float _rightHandCooldownEnd;
        private UnarmedAttackDataSO _unarmedData;

        public int BaseDamage => _baseDamage;
        public float AttackRange => _attackRange;

        private void Start()
        {
            if (_unarmedData == null)
            {
                var unarmedPath = "Combat/Weapons/unarmed_default";
                _unarmedData = Resources.Load<UnarmedAttackDataSO>(unarmedPath);
                if (_unarmedData == null)
                {
                    Debug.LogWarning($"PlayerCombatController: Could not load unarmed data from {unarmedPath}", this);
                }
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerHitEvent>(OnPlayerHit);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerHitEvent>(OnPlayerHit);
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
        }

        private void TryAttackLeftHand()
        {
            if (Time.time < _leftHandCooldownEnd)
                return;

            var leftHandItemId = _equipmentManager?.GetEquippedItem(EquipmentSlot.LeftHand);
            ExecuteHandAttack(leftHandItemId, true);
        }

        private void TryAttackRightHand()
        {
            if (Time.time < _rightHandCooldownEnd)
                return;

            var rightHandItemId = _equipmentManager?.GetEquippedItem(EquipmentSlot.RightHand);
            ExecuteHandAttack(rightHandItemId, false);
        }

        private void ExecuteHandAttack(string itemInstanceId, bool isLeftHand)
        {
            if (string.IsNullOrEmpty(itemInstanceId) && _itemDatabase != null)
            {
                ExecuteUnarmedAttack(isLeftHand);
                return;
            }

            if (string.IsNullOrEmpty(itemInstanceId))
            {
                ExecuteUnarmedAttack(isLeftHand);
                return;
            }

            if (_itemDatabase != null && _itemDatabase.TryGetById(itemInstanceId, out var itemData))
            {
                if (!string.IsNullOrEmpty(itemData.WeaponId))
                {
                    var weaponPath = $"Combat/Weapons/{itemData.WeaponId}";
                    var weaponData = Resources.Load<WeaponDataSO>(weaponPath);
                    if (weaponData != null)
                    {
                        ExecuteWeaponAttack(weaponData, isLeftHand);
                        return;
                    }
                }
            }

            ExecuteUnarmedAttack(isLeftHand);
        }

        private void ExecuteWeaponAttack(WeaponDataSO weapon, bool isLeftHand)
        {
            int staminaCost = Mathf.RoundToInt(weapon.StaminaCost);
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                return;
            }

            int baseDamage = weapon.BaseDamage;
            var damageResult = DamageCalculator.CalculateDirectDamage(baseDamage);

            GameEventBus.Publish(new PlayerAttackedEvent(baseDamage, Vector2.right, damageResult));

            if (_equipmentManager != null)
            {
                _equipmentManager.RegisterEquipmentUsage();
            }

            float cooldown = weapon.BaseCooldownSeconds;
            if (isLeftHand)
            {
                _leftHandCooldownEnd = Time.time + cooldown;
            }
            else
            {
                _rightHandCooldownEnd = Time.time + cooldown;
            }
        }

        private void ExecuteUnarmedAttack(bool isLeftHand)
        {
            if (_unarmedData == null)
            {
                Debug.LogWarning("PlayerCombatController: No unarmed attack data available", this);
                return;
            }

            int staminaCost = Mathf.RoundToInt(_unarmedData.StaminaCost);
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                return;
            }

            int baseDamage = _unarmedData.BaseDamage;
            var damageResult = DamageCalculator.CalculateDirectDamage(baseDamage);

            GameEventBus.Publish(new PlayerAttackedEvent(baseDamage, Vector2.right, damageResult));

            float cooldown = _unarmedData.BaseCooldownSeconds;
            if (isLeftHand)
            {
                _leftHandCooldownEnd = Time.time + cooldown;
            }
            else
            {
                _rightHandCooldownEnd = Time.time + cooldown;
            }
        }

        private void TryDodge()
        {
            int staminaCost = 15;
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                return;
            }

            GameEventBus.Publish(new PlayerDodgeStartedEvent());
            Debug.Log("PlayerCombatController: Dodge executed", this);
        }

        public bool TryAttack(Vector2 direction, int? staminaCost = null)
        {
            TryAttackRightHand();
            return true;
        }

        public void ResetCooldown()
        {
            _leftHandCooldownEnd = 0f;
            _rightHandCooldownEnd = 0f;
        }

        private void OnPlayerHit(PlayerHitEvent evt)
        {
            if (_playerManager != null)
            {
                _playerManager.DamageHP(evt.DamageAmount);
            }
        }
    }
}
