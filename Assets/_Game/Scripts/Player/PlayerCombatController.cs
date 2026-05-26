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
        [SerializeField] private ManaManager _manaManager;
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private WeaponDatabaseSO _weaponDatabase;

        private float _leftHandCooldownEnd;
        private float _rightHandCooldownEnd;
        private UnarmedAttackDataSO _unarmedData;

        public int BaseDamage => 10;
        public float AttackRange => 1.5f;

        private void Start()
        {
            if (_unarmedData == null)
            {
                CreateDefaultUnarmedData();
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
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryAttackLeftHand();
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                TryAttackRightHand();
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
            WeaponDataSO weaponData = null;

            if (!string.IsNullOrEmpty(itemInstanceId) && _itemDatabase != null && _itemDatabase.TryGetById(itemInstanceId, out var itemData))
            {
                if (!string.IsNullOrEmpty(itemData.WeaponId) && _weaponDatabase != null)
                {
                    _weaponDatabase.TryGetById(itemData.WeaponId, out weaponData);
                }
            }

            if (weaponData != null)
            {
                ExecuteWeaponAttack(weaponData, isLeftHand);
            }
            else
            {
                ExecuteUnarmedAttack(isLeftHand);
            }
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

            Debug.Log($"PlayerCombatController: Weapon attack {weapon.DisplayName}, damage={baseDamage}", this);
        }

        private void ExecuteUnarmedAttack(bool isLeftHand)
        {
            if (_unarmedData == null)
            {
                CreateDefaultUnarmedData();
            }

            if (_unarmedData == null)
            {
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

            Debug.Log($"PlayerCombatController: Unarmed attack, damage={baseDamage}", this);
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

        private void CreateDefaultUnarmedData()
        {
            _unarmedData = ScriptableObject.CreateInstance<UnarmedAttackDataSO>();
            _unarmedData.Id = "unarmed_default";
            _unarmedData.DisplayName = "Punch";
            _unarmedData.BaseDamage = 3;
            _unarmedData.BaseCooldownSeconds = 0.4f;
            _unarmedData.StaminaCost = 10f;
            _unarmedData.Range = 0.5f;
            _unarmedData.ArcDegrees = 120f;
            _unarmedData.DamageType = DamageType.Physical;
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
