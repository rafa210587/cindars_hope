using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
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
        [SerializeField] private InteractionSystem _interactionSystem;
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

        private void Start()
        {
            if (_interactionSystem == null)
            {
                _interactionSystem = GetComponent<InteractionSystem>();
            }
        }

        public void RebindStaminaManager(StaminaManager staminaManager)
        {
            _staminaManager = staminaManager;
        }

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
            {
                UpdateDodgeState();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryAttackLeftHand();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_interactionSystem != null && _interactionSystem.HasCandidate)
                {
                    return;
                }
                TryAttackRightHand();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryDodge();
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

            if (weapon.Type == WeaponType.Bow && weapon.ProjectilePrefab != null)
            {
                ExecuteRangedAttack(weapon, direction);
            }
            else
            {
                ExecuteMeleeAttack(weapon, direction);
            }

            if (_equipmentManager != null)
                _equipmentManager.RegisterEquipmentUsage();
        }

        private void ExecuteMeleeAttack(WeaponDataSO weapon, Vector2 direction)
        {
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
            }
        }

        private void ExecuteRangedAttack(WeaponDataSO weapon, Vector2 direction)
        {
            Vector2 spawnPos = (Vector2)transform.position + direction.normalized * 0.5f;
            var projectile = Instantiate(weapon.ProjectilePrefab, spawnPos, Quaternion.identity);

            var projectileBehaviour = projectile.GetComponent<ProjectileBehaviour>();
            if (projectileBehaviour != null)
            {
                projectileBehaviour.Initialize(
                    direction,
                    weapon.ProjectileSpeed,
                    weapon.Range,
                    weapon.BaseDamage,
                    weapon.DamageType,
                    _knockbackForce
                );
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

            GameEventBus.Publish(new PlayerDodgeStartedEvent());
        }

        private void UpdateDodgeState()
        {
            if (_isDodging && Time.time >= _dodgeEndTime)
            {
                _isDodging = false;
                GameEventBus.Publish(new PlayerDodgeEndedEvent());
            }
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
