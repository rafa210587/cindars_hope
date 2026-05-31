using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
using CindarsHope.Inventory.Data;
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

        // SPEC 14A-FIX8: Resolution chain - itemInstanceId -> ItemDataSO -> ItemDataSO.WeaponId -> WeaponDataSO.
        // Either wire _weaponDatabase, or list the known weapons in _knownWeapons (or both).
        // Equipment IDs that ARE weapon IDs (e.g. "weapon_sword_iron") also resolve directly.
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private WeaponDatabaseSO _weaponDatabase;
        [SerializeField] private WeaponDataSO[] _knownWeapons = new WeaponDataSO[0];

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

            // SPEC 14A-FIX9: create a runtime unarmed fallback if the inspector field is null.
            // Otherwise pressing attack with empty slot would hard-error with NoUnarmedFallback.
            if (_unarmedFallback == null)
            {
                _unarmedFallback = ScriptableObject.CreateInstance<UnarmedAttackDataSO>();
                _unarmedFallback.Id = "unarmed_default_runtime";
                _unarmedFallback.DisplayName = "Punch";
                _unarmedFallback.BaseDamage = 3;
                _unarmedFallback.BaseCooldownSeconds = 0.4f;
                _unarmedFallback.StaminaCost = 5f;
                _unarmedFallback.Range = 0.6f;
                _unarmedFallback.ArcDegrees = 120f;
                _unarmedFallback.DamageType = DamageType.Physical;
                Debug.Log("PlayerAttackController: created runtime UnarmedAttackDataSO fallback (no asset wired).", this);
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EquipmentSlotChangedEvent>(OnEquipmentSlotChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EquipmentSlotChangedEvent>(OnEquipmentSlotChanged);
        }

        // SPEC 14A-FIX9: log every equip/unequip so we can see what the UI flow really stored.
        private void OnEquipmentSlotChanged(EquipmentSlotChangedEvent evt)
        {
            Debug.Log($"CombatLog: EquipmentSlotChanged. Slot={evt.Slot}, ItemInstanceId='{evt.ItemInstanceId ?? "<null>"}'", this);
        }

        public void RebindStaminaManager(StaminaManager staminaManager)
        {
            _staminaManager = staminaManager;
        }

        // SPEC 14A-FIX10: explicit rebind so the installer can supply combat databases at runtime
        // without depending on serialized inspector references that get wiped on scene re-save.
        public void RebindCombatData(ItemDatabaseSO itemDatabase, WeaponDatabaseSO weaponDatabase)
        {
            if (itemDatabase != null) _itemDatabase = itemDatabase;
            if (weaponDatabase != null) _weaponDatabase = weaponDatabase;
            string itemDbName = _itemDatabase != null ? _itemDatabase.name : "null";
            string weaponDbName = _weaponDatabase != null ? _weaponDatabase.name : "null";
            Debug.Log($"PlayerAttackController.RebindCombatData. ItemDb={itemDbName}, WeaponDb={weaponDbName}.", this);
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
            var equippedItemId = _equipmentManager != null ? _equipmentManager.GetEquippedItem(EquipmentSlot.LeftHand) : null;
            AttackWithSlot(EquipmentSlot.LeftHand, ref _lastLeftHandAttackTime, equippedItemId);
        }

        private void TryAttackRightHand()
        {
            var equippedItemId = _equipmentManager != null ? _equipmentManager.GetEquippedItem(EquipmentSlot.RightHand) : null;
            AttackWithSlot(EquipmentSlot.RightHand, ref _lastRightHandAttackTime, equippedItemId);
        }

        // SPEC 14A-FIX8: full resolution chain itemInstanceId -> ItemDataSO -> WeaponDataSO
        // with explicit logging at each step. Returns null when nothing is equipped (caller will use
        // unarmed fallback). Returns null + sets error when SOMETHING is equipped but doesn't resolve
        // (caller must NOT silently fall back to unarmed in that case).
        private WeaponDataSO ResolveEquippedWeapon(EquipmentSlot slot, string equippedItemId, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(equippedItemId))
            {
                Debug.Log($"CombatLog: PlayerAttackResolveSlot. Slot={slot}, EquippedInstanceId=<empty>", this);
                return null;
            }

            Debug.Log($"CombatLog: PlayerAttackResolveSlot. Slot={slot}, EquippedInstanceId={equippedItemId}", this);

            ItemDataSO itemData = null;
            string weaponLookupId = equippedItemId;
            bool wentThroughItemDatabase = false;

            if (_itemDatabase != null && _itemDatabase.TryGetById(equippedItemId, out itemData) && itemData != null)
            {
                wentThroughItemDatabase = true;
                Debug.Log($"CombatLog: PlayerAttackResolveItemData. ItemInstanceId={equippedItemId}, ItemDataId={itemData.Id}, ItemType={itemData.Category}, WeaponId='{itemData.WeaponId}'", this);
                if (!string.IsNullOrEmpty(itemData.WeaponId))
                {
                    weaponLookupId = itemData.WeaponId;
                }
                else
                {
                    error = $"Item '{equippedItemId}' (Category={itemData.Category}) is not a weapon — WeaponId is empty.";
                    Debug.Log($"CombatLog: PlayerAttackResolveWeapon. WeaponId=<none>, WeaponResolved=False, Reason=ItemNotWeapon", this);
                    return null;
                }
            }
            else
            {
                Debug.Log($"CombatLog: PlayerAttackResolveItemData. ItemInstanceId={equippedItemId}, ItemDataId=<not_in_itemdb>, FallingBackToDirectWeaponLookup=True", this);
            }

            var weapon = LookupWeapon(weaponLookupId);
            Debug.Log($"CombatLog: PlayerAttackResolveWeapon. WeaponId={weaponLookupId}, WeaponResolved={weapon != null}, ViaItemDb={wentThroughItemDatabase}", this);

            if (weapon == null)
            {
                error = $"Could not resolve WeaponDataSO for WeaponId='{weaponLookupId}' (from EquippedInstanceId='{equippedItemId}'). " +
                        $"_itemDatabase assigned={_itemDatabase != null}, _weaponDatabase assigned={_weaponDatabase != null}, _knownWeapons count={(_knownWeapons?.Length ?? 0)}.";
            }
            return weapon;
        }

        private WeaponDataSO LookupWeapon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return null;
            if (_weaponDatabase != null && _weaponDatabase.TryGetById(weaponId, out var fromDb) && fromDb != null) return fromDb;
            if (_knownWeapons != null)
            {
                foreach (var w in _knownWeapons)
                {
                    if (w != null && w.Id == weaponId) return w;
                }
            }
            return null;
        }

        private void AttackWithSlot(EquipmentSlot slot, ref float lastAttackTime, string equippedItemId)
        {
            if (_isDodging)
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Dodging, Slot={slot}", this);
                return;
            }

            WeaponDataSO weapon = ResolveEquippedWeapon(slot, equippedItemId, out string resolveError);

            // CASE A: Slot is empty (nothing equipped) -> use unarmed fallback.
            // CASE B: Something IS equipped but didn't resolve -> ERROR + abort (do not silently fall to unarmed).
            // CASE C: Resolved weapon -> attack with it.
            if (weapon == null && string.IsNullOrEmpty(equippedItemId))
            {
                if (_unarmedFallback == null)
                {
                    Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=NoWeaponEquippedAndNoUnarmedFallback, Slot={slot}", this);
                    return;
                }
                weapon = ConvertUnarmedToWeapon(_unarmedFallback);
            }
            else if (weapon == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=WeaponEquippedButNotResolved, Slot={slot}, EquippedInstanceId={equippedItemId}, ResolveError={resolveError}", this);
                return;
            }

            float cooldown = weapon.BaseCooldownSeconds;
            float attackSpeed = weapon.AttackSpeedMultiplier;
            cooldown = cooldown / Mathf.Max(0.1f, attackSpeed);

            if (Time.time < lastAttackTime + cooldown)
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Cooldown, Slot={slot}, RemainingSeconds={(lastAttackTime + cooldown - Time.time):F2}", this);
                return;
            }

            int staminaCost = Mathf.RoundToInt(weapon.StaminaCost);
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=InsufficientStamina, Slot={slot}, StaminaCost={staminaCost}", this);
                return;
            }

            Debug.Log($"CombatLog: PlayerAttackStarted. Slot={slot}, Weapon={weapon.DisplayName}, BaseDamage={weapon.BaseDamage}, Range={weapon.Range:F2}, Type={weapon.Type}", this);
            ExecuteWeaponAttack(weapon);
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

        // SPEC 14A-FIX8: removed GetWeaponAsset (Resources.Load fallback); resolution now goes
        // through ItemDatabase + WeaponDatabase + _knownWeapons (LookupWeapon).

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
