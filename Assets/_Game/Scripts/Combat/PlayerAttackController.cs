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
            bool modalOpen = GameBootstrap.Instance?.ModalManager?.HasActiveModal == true;

            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.E))
            {
                string key = Input.GetKeyDown(KeyCode.Q) ? "Q" : Input.GetKeyDown(KeyCode.J) ? "J" : "E";
                Debug.Log($"CombatLog: PlayerAttackInputReceived. Key={key}, ModalOpen={modalOpen}", this);
            }

            if (modalOpen)
            {
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.E))
                    Debug.Log("CombatLog: PlayerAttackBlocked. Reason=ModalActive", this);
                UpdateDodgeState();
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
                if (_interactionSystem != null && _interactionSystem.HasCandidate)
                {
                    Debug.Log("CombatLog: PlayerAttackBlocked. Reason=InteractionCandidatePresent (E used for interact)", this);
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
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Dodging, Slot={slot}", this);
                return;
            }

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
            {
                Debug.LogWarning($"CombatLog: PlayerAttackBlocked. Reason=NoWeaponNoUnarmedFallback, Slot={slot}, EquippedItemId='{equippedItemId}'", this);
                return;
            }

            float cooldown = weapon?.BaseCooldownSeconds ?? _unarmedFallback.BaseCooldownSeconds;
            if (weapon != null)
            {
                float attackSpeed = weapon.AttackSpeedMultiplier;
                cooldown = cooldown / Mathf.Max(0.1f, attackSpeed);
            }

            if (Time.time < lastAttackTime + cooldown)
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Cooldown, Slot={slot}, RemainingSeconds={(lastAttackTime + cooldown - Time.time):F2}", this);
                return;
            }

            float staminaCost = weapon?.StaminaCost ?? _unarmedFallback.StaminaCost;
            if (_staminaManager != null && !_staminaManager.TrySpendStamina((int)staminaCost))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=InsufficientStamina, Slot={slot}, StaminaCost={(int)staminaCost}", this);
                return;
            }

            var resolvedWeapon = weapon ?? ConvertUnarmedToWeapon(_unarmedFallback);
            Debug.Log($"CombatLog: PlayerAttackStarted. Slot={slot}, Weapon={resolvedWeapon.DisplayName}, BaseDamage={resolvedWeapon.BaseDamage}, Range={resolvedWeapon.Range:F2}, Type={resolvedWeapon.Type}", this);
            ExecuteWeaponAttack(resolvedWeapon);
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
            Vector2 attackCenter = (Vector2)transform.position + direction * 0.5f;
            var hitColliders = Physics2D.OverlapCircleAll(attackCenter, weapon.Range);

            int candidatesTotal = hitColliders.Length;
            int hitEnemies = 0;
            foreach (var collider in hitColliders)
            {
                if (collider.gameObject == gameObject)
                    continue;

                var enemyHealth = collider.GetComponentInParent<EnemyHealth>() ?? collider.GetComponent<EnemyHealth>();
                if (enemyHealth == null)
                    continue;

                Debug.Log($"CombatLog: PlayerAttackHitCandidate. EnemyId={enemyHealth.EnemyId}, EnemyHP={enemyHealth.CurrentHp}/{enemyHealth.MaxHp}, Distance={Vector2.Distance(attackCenter, collider.transform.position):F2}", this);

                var damageRequest = new DamageRequest(enemyHealth.EnemyId, weapon.BaseDamage)
                {
                    DamageType = weapon.DamageType,
                    SourcePosition = transform.position,
                    KnockbackForce = _knockbackForce
                };

                int hpBefore = enemyHealth.CurrentHp;
                enemyHealth.TakeDamage(damageRequest);
                hitEnemies++;
                Debug.Log($"CombatLog: PlayerAttackDamageApplied. EnemyId={enemyHealth.EnemyId}, BaseDamage={weapon.BaseDamage}, HP={hpBefore}->{enemyHealth.CurrentHp}", this);
            }

            if (hitEnemies == 0)
            {
                Debug.Log($"CombatLog: PlayerAttackMissed. Reason={(candidatesTotal == 0 ? "NoCollidersInRange" : "NoEnemyHealthInColliders")}, AttackCenter={attackCenter}, Range={weapon.Range:F2}, CollidersSeen={candidatesTotal}, Direction={direction}", this);
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
