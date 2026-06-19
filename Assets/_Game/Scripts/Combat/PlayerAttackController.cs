using CindarsHope.Combat.Magic;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
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
        [SerializeField] private SpellDatabaseSO _spellDatabase;
        private StatusEffectDatabaseSO _statusEffectDatabase;
        [SerializeField] private InventoryManager _inventoryManager;
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

        // SPEC_05: Service extraction
        private EquippedItemResolver _itemResolver;
        private CombatActionContext _currentActionContext;

        // SPEC_07: Attack services
        private BowArrowAttackService _bowArrowService;
        private SpellCastService _spellCastService;

        // fable_08: gerencia janela de cast time + interrupt (criado em Start no player).
        private CindarsHope.Combat.Magic.SpellCastRoutine _spellCastRoutine;

        // F02: carga por mão (tap=light, hold=heavy, hold longo=charged) + stats derivados.
        private readonly AttackChargeTracker _leftCharge = new AttackChargeTracker();
        private readonly AttackChargeTracker _rightCharge = new AttackChargeTracker();
        private PlayerCombatStatsProvider _statsProvider;
        private SpriteRenderer _chargeTelegraphRenderer;
        private Color _chargeTelegraphBaseColor = Color.white;

        private void Start()
        {
            if (_interactionSystem == null)
            {
                _interactionSystem = GetComponent<InteractionSystem>();
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null)
            {
                if (_equipmentManager == null) _equipmentManager = bootstrap.EquipmentManager;
                if (_staminaManager == null) _staminaManager = bootstrap.StaminaManager;
                if (_manaManager == null) _manaManager = bootstrap.ManaManager;
                if (_inventoryManager == null) _inventoryManager = bootstrap.InventoryManager;
                if (_itemDatabase == null) _itemDatabase = bootstrap.ItemDatabase;
                if (_weaponDatabase == null) _weaponDatabase = bootstrap.WeaponDatabase;
                if (_spellDatabase == null) _spellDatabase = bootstrap.SpellDatabase;
                if (_statusEffectDatabase == null) _statusEffectDatabase = bootstrap.StatusEffectDatabase;
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

            // SPEC_05: Initialize item resolver with current databases
            RefreshItemResolver();
            _currentActionContext = new CombatActionContext();

            // F02: provider de stats derivados (DerivedStatsCalculator WAVE 05, antes órfão).
            // Base de Attack = Força do player; equipment dict entra quando F03 criar o registry.
            var progression = bootstrap != null ? bootstrap.PlayerProgressionManager : null;
            var skillTree = bootstrap != null ? bootstrap.SkillTreeManager : null;
            _statsProvider = new PlayerCombatStatsProvider(
                () => progression != null ? progression.Strength : 0,
                () => skillTree != null ? skillTree.GetAllActivePassiveModifiers() : null);

            // F03: scaling por atributo da arma.
            _statsProvider.AttributeSource = attributeType =>
            {
                if (progression == null) return 0;
                switch (attributeType)
                {
                    case Player.Progression.PlayerAttributeType.Strength: return progression.Strength;
                    case Player.Progression.PlayerAttributeType.Dexterity: return progression.Dexterity;
                    case Player.Progression.PlayerAttributeType.Intelligence: return progression.Intelligence;
                    case Player.Progression.PlayerAttributeType.Willpower: return progression.Willpower;
                    case Player.Progression.PlayerAttributeType.Constitution: return progression.Constitution;
                    case Player.Progression.PlayerAttributeType.Breath: return progression.Breath;
                    default: return 0;
                }
            };

            if (_playerController != null)
            {
                _chargeTelegraphRenderer = _playerController.GetComponent<SpriteRenderer>();
                if (_chargeTelegraphRenderer != null)
                {
                    _chargeTelegraphBaseColor = _chargeTelegraphRenderer.color;
                }
            }

            // SPEC_07: Initialize attack services
            RefreshServices();

            // fable_08: rotina de cast time/interrupt fica no GameObject do player (mesmo telegraph
            // renderer da carga). Anexa ao PlayerController quando houver; senão neste GameObject.
            var routineHost = _playerController != null ? _playerController.gameObject : gameObject;
            _spellCastRoutine = routineHost.GetComponent<CindarsHope.Combat.Magic.SpellCastRoutine>();
            if (_spellCastRoutine == null)
            {
                _spellCastRoutine = routineHost.AddComponent<CindarsHope.Combat.Magic.SpellCastRoutine>();
            }
            _spellCastRoutine.Configure(_chargeTelegraphRenderer);
        }

        private void OnDestroy()
        {
            _statsProvider?.Dispose();
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
            // SPEC_07B: Refresh services to pick up updated stamina manager
            RefreshServices();
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

            // SPEC_05B: Refresh resolver with updated databases
            RefreshItemResolver();
            // SPEC_07: Refresh services after resolver update
            RefreshServices();
        }

        public void RebindCombatData(ItemDatabaseSO itemDatabase, WeaponDatabaseSO weaponDatabase, SpellDatabaseSO spellDatabase)
        {
            RebindCombatData(itemDatabase, weaponDatabase);
            if (spellDatabase != null) _spellDatabase = spellDatabase;
            string spellDbName = _spellDatabase != null ? _spellDatabase.name : "null";
            Debug.Log($"PlayerAttackController.RebindCombatData (with spell). SpellDb={spellDbName}.", this);

            // SPEC_05B: Refresh resolver with updated spell database
            RefreshItemResolver();
            // SPEC_07: Refresh services after resolver update
            RefreshServices();
        }

        // SPEC_05B: Recreate EquippedItemResolver with current database references.
        // Called on Start and after any RebindCombatData to ensure resolver uses latest databases.
        private void RefreshItemResolver()
        {
            _itemResolver = new EquippedItemResolver(_itemDatabase, _weaponDatabase, _spellDatabase, _knownWeapons);
        }

        // SPEC_07: Recreate attack services after resolver or database rebind.
        private void RefreshServices()
        {
            _bowArrowService = new BowArrowAttackService(_equipmentManager, _inventoryManager, _staminaManager, _itemDatabase, _itemResolver, _knockbackForce, _statusEffectDatabase);
            _spellCastService = new SpellCastService(_manaManager, _equipmentManager, _itemResolver, _knockbackForce, _statusEffectDatabase);
            // F02: serviços consomem o mesmo provider (dano derivado em projéteis).
            if (_bowArrowService != null) _bowArrowService.StatsProvider = _statsProvider;
            if (_spellCastService != null)
            {
                _spellCastService.StatsProvider = _statsProvider;
                // fable_08: alvos de SelfRestore + spellbook (fable_07).
                _spellCastService.PlayerManager = GameBootstrap.Instance?.PlayerManager;
                _spellCastService.StaminaManager = _staminaManager;
                _spellCastService.Spellbook = CindarsHope.Magic.PlayerSpellbook.Instance;
                // fable_08 EMENDA 6.6-A: auto-target via QUERY de Physics2D (inimigos no raio),
                // NÃO FindObjectsByType (rule unity-architecture). Devolve posições de EnemyHealth.
                _spellCastService.EnemyPositionQuery = QueryEnemyPositions;
            }
        }

        // fable_08: posições de inimigos vivos dentro do raio, para auto-target/área. Usa
        // OverlapCircleAll (mesma query do melee) — busca de física, não de objeto global.
        private System.Collections.Generic.IReadOnlyList<UnityEngine.Vector2> QueryEnemyPositions(UnityEngine.Vector2 center, float radius)
        {
            var positions = new System.Collections.Generic.List<UnityEngine.Vector2>();
            var hits = Physics2D.OverlapCircleAll(center, Mathf.Max(0.1f, radius));
            var seen = new System.Collections.Generic.HashSet<EnemyHealth>();
            foreach (var col in hits)
            {
                if (col == null) continue;
                var enemy = col.GetComponentInParent<EnemyHealth>() ?? col.GetComponent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || !seen.Add(enemy)) continue;
                positions.Add(enemy.transform.position);
            }
            return positions;
        }

        private void Update()
        {
            bool modalOpen = GameBootstrap.Instance?.ModalManager?.HasActiveModal == true;

            // SPEC 14A-FIX13: J removido como ataque. Q = mao esquerda (tool), E = mao direita
            // (weapon, ou interagir se ha InteractionCandidate).
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
            {
                string key = Input.GetKeyDown(KeyCode.Q) ? "Q" : "E";
                Debug.Log($"CombatLog: PlayerAttackInputReceived. Key={key}, ModalOpen={modalOpen}", this);
            }

            if (modalOpen)
            {
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
                    Debug.Log("CombatLog: PlayerAttackBlocked. Reason=ModalActive", this);
                _leftCharge.Cancel();
                _rightCharge.Cancel();
                UpdateChargeTelegraph();
                UpdateDodgeState();
                return;
            }

            // F01: Stun no player bloqueia ataques.
            if (PlayerStatusReceiver.Instance != null && PlayerStatusReceiver.Instance.IsActionBlocked)
            {
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
                    Debug.Log("CombatLog: PlayerAttackBlocked. Reason=PlayerStunned", this);
                _leftCharge.Cancel();
                _rightCharge.Cancel();
                UpdateChargeTelegraph();
                UpdateDodgeState();
                return;
            }

            // F02: tap=light, hold=heavy, hold longo=charged (release no KeyUp).
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _leftCharge.Begin(Time.time);
            }

            if (Input.GetKeyUp(KeyCode.Q) && _leftCharge.IsCharging)
            {
                var weight = _leftCharge.Release(Time.time);
                TryAttackLeftHand(weight);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_interactionSystem != null && _interactionSystem.HasCandidate)
                {
                    // Regressão protegida: E com candidato continua interagindo (charge não inicia).
                    Debug.Log("CombatLog: PlayerAttackBlocked. Reason=InteractionCandidatePresent (E used for interact)", this);
                }
                else
                {
                    _rightCharge.Begin(Time.time);
                }
            }

            if (Input.GetKeyUp(KeyCode.E) && _rightCharge.IsCharging)
            {
                var weight = _rightCharge.Release(Time.time);
                TryAttackRightHand(weight);
            }

            UpdateChargeTelegraph();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_playerController != null && _playerController.MoveInput.sqrMagnitude > 0.1f)
                {
                    UpdateDodgeState();
                    return;
                }

                TryDodge();
            }

            UpdateDodgeState();
        }

        private void TryAttackLeftHand(AttackWeight weight = AttackWeight.Light)
        {
            var equippedItemId = _equipmentManager != null ? _equipmentManager.GetEquippedItem(EquipmentSlot.LeftHand) : null;
            AttackWithSlot(EquipmentSlot.LeftHand, ref _lastLeftHandAttackTime, equippedItemId, weight);
        }

        private void TryAttackRightHand(AttackWeight weight = AttackWeight.Light)
        {
            var equippedItemId = _equipmentManager != null ? _equipmentManager.GetEquippedItem(EquipmentSlot.RightHand) : null;
            AttackWithSlot(EquipmentSlot.RightHand, ref _lastRightHandAttackTime, equippedItemId, weight);
        }

        // F02: telegraph simples de carga — tinta o sprite conforme o peso acumulado.
        private void UpdateChargeTelegraph()
        {
            if (_chargeTelegraphRenderer == null)
            {
                return;
            }

            var hold = Mathf.Max(_leftCharge.HoldSeconds(Time.time), _rightCharge.HoldSeconds(Time.time));
            if (hold < AttackChargeRules.HeavyThresholdSeconds)
            {
                _chargeTelegraphRenderer.color = _chargeTelegraphBaseColor;
                return;
            }

            var weight = AttackChargeRules.ResolveWeight(hold);
            var tint = weight >= AttackWeight.ChargedShort
                ? new Color(1f, 0.6f, 0.2f)
                : new Color(1f, 0.85f, 0.5f);
            _chargeTelegraphRenderer.color = Color.Lerp(_chargeTelegraphBaseColor, tint, 0.6f);
        }

        // SPEC_05: Delegated to EquippedItemResolver; kept here as wrapper for external callers
        // Full resolution chain itemInstanceId -> ItemDataSO -> WeaponDataSO with explicit logging.
        // Returns null when nothing is equipped (caller will use unarmed fallback).
        // Returns null + sets error when SOMETHING is equipped but doesn't resolve.
        // SPEC_05B: Added null guard for resolver safety.
        private WeaponDataSO ResolveEquippedWeapon(EquipmentSlot slot, string equippedItemId, out string error)
        {
            error = null;
            if (_itemResolver == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ItemResolverNull, Slot={slot}", this);
                return null;
            }
            return _itemResolver.ResolveEquippedWeapon(slot, equippedItemId, out error);
        }

        private WeaponDataSO LookupWeapon(string weaponId)
        {
            if (_itemResolver == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ItemResolverNull", this);
                return null;
            }
            return _itemResolver.LookupWeapon(weaponId);
        }

        private void AttackWithSlot(EquipmentSlot slot, ref float lastAttackTime, string equippedItemId, AttackWeight weight = AttackWeight.Light)
        {
            if (_isDodging)
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Dodging, Slot={slot}", this);
                return;
            }

            // Resolve ItemDataSO first to categorize the equipped item.
            ItemDataSO itemData = null;
            if (!string.IsNullOrEmpty(equippedItemId) && _itemDatabase != null)
                _itemDatabase.TryGetById(equippedItemId, out itemData);

            // Ammo (arrow) slot dispatch: fire bow+arrow combo.
            if (itemData != null && itemData.Category == ItemCategory.Ammo)
            {
                TryExecuteArrowAttack(slot, itemData, ref lastAttackTime);
                return;
            }

            // Magic slot dispatch: fire spell.
            if (itemData != null && itemData.Category == ItemCategory.Magic)
            {
                TryExecuteSpellAttack(slot, itemData, ref lastAttackTime);
                return;
            }

            // Weapon (Bow) dispatch: block — bow fires only from the arrow-hand side.
            if (itemData != null && itemData.Category == ItemCategory.Weapon && !string.IsNullOrEmpty(itemData.WeaponId))
            {
                var bowCheck = LookupWeapon(itemData.WeaponId);
                if (bowCheck != null && bowCheck.Type == WeaponType.Bow)
                {
                    Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=BowHandPressed_UseArrowHand, Slot={slot}", this);
                    return;
                }
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

            // SPEC_07B: Block bow from normal weapon path — must use bow+arrow path instead
            if (weapon.Type == WeaponType.Bow)
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=BowHandPressed_UseArrowHand, Slot={slot}", this);
                return;
            }

            // F02/F03: cooldown final via AttackSpeed derivado × ASPD da arma.
            float cooldown = CooldownHelper.CalculateWeaponCooldown(weapon);
            if (_statsProvider != null)
            {
                cooldown = _statsProvider.FinalCooldown(cooldown, weapon);
            }

            if (!CooldownHelper.IsCooldownExpired(lastAttackTime, cooldown))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Cooldown, Slot={slot}, RemainingSeconds={CooldownHelper.GetRemainingCooldown(lastAttackTime, cooldown):F2}", this);
                return;
            }

            // F03: custos canônicos POR ARMA quando autorados; senão razões F02.
            int staminaCost = PlayerCombatStatsProvider.WeaponStaminaCost(weapon, weight);
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=InsufficientStamina, Slot={slot}, StaminaCost={staminaCost}", this);
                return;
            }

            Debug.Log($"CombatLog: PlayerAttackStarted. Slot={slot}, Weapon={weapon.DisplayName}, BaseDamage={weapon.BaseDamage}, Weight={weight}, Range={weapon.Range:F2}, Type={weapon.Type}", this);
            GameEventBus.Publish(new PlayerChargedAttackEvent((int)weight));
            ExecuteWeaponAttack(weapon, weight);
            lastAttackTime = Time.time;
        }

        // SPEC_07: Delegated to BowArrowAttackService
        private void TryExecuteArrowAttack(EquipmentSlot ammoSlot, ItemDataSO ammoItemData, ref float lastAttackTime)
        {
            if (_bowArrowService == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=BowArrowServiceNull, Slot={ammoSlot}", this);
                return;
            }
            Vector2 direction = _playerController?.LastFacingDirection ?? Vector2.right;
            var result = _bowArrowService.TryFire(ammoSlot, ammoItemData, lastAttackTime, direction, transform.position);
            if (result.Success)
                lastAttackTime = Time.time;
        }

        // SPEC_07: Delegated to SpellCastService
        private void TryExecuteSpellAttack(EquipmentSlot slot, ItemDataSO itemData, ref float lastAttackTime)
        {
            if (_spellCastService == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=SpellCastServiceNull, Slot={slot}", this);
                return;
            }
            Vector2 direction = _playerController?.LastFacingDirection ?? Vector2.right;

            // fable_08: valida/reserva (cooldown, mana, conhecimento). Em sucesso a rotina cuida da
            // janela de cast time + interrupt; cast 0s resolve imediatamente dentro do BeginCast.
            var begin = _spellCastService.TryBeginCast(slot, itemData, lastAttackTime, direction, transform.position, out var plan);
            if (!begin.Success || plan == null)
            {
                return;
            }

            if (_spellCastRoutine != null)
            {
                if (!_spellCastRoutine.BeginCast(_spellCastService, plan))
                {
                    // Já conjurando outra magia: reembolsa a mana reservada deste plano.
                    _spellCastService.RefundCast(plan);
                    return;
                }
            }
            else
            {
                // Fallback sem rotina (não deveria ocorrer em cena): resolve direto.
                _spellCastService.ResolveCast(plan);
            }

            lastAttackTime = Time.time;
        }

        // SPEC_05: Delegated to EquippedItemResolver
        // SPEC_05B: Added null guard for resolver safety.
        private SpellDataSO ResolveEquippedSpell(ItemDataSO itemData)
        {
            if (_itemResolver == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ItemResolverNull", this);
                return null;
            }
            return _itemResolver.ResolveEquippedSpell(itemData);
        }

        private static EquipmentSlot GetOppositeHand(EquipmentSlot slot)
        {
            return slot == EquipmentSlot.LeftHand ? EquipmentSlot.RightHand : EquipmentSlot.LeftHand;
        }

        private void ExecuteWeaponAttack(WeaponDataSO weapon, AttackWeight weight = AttackWeight.Light)
        {
            Vector2 direction = _playerController?.LastFacingDirection ?? Vector2.right;

            if (weapon.Type == WeaponType.Bow && weapon.ProjectilePrefab != null)
            {
                ExecuteRangedAttack(weapon, direction);
            }
            else
            {
                ExecuteMeleeAttack(weapon, direction, weight);
            }

            if (_equipmentManager != null)
                _equipmentManager.RegisterEquipmentUsage();
        }

        private void ExecuteMeleeAttack(WeaponDataSO weapon, Vector2 direction, AttackWeight weight = AttackWeight.Light)
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

                // F02: dano final = (base + Attack derivado) × peso × crítico canônico.
                // Janela de vulnerabilidade aberta (CoreExposed) GARANTE crítico (emenda).
                var vulnerability = enemyHealth.GetComponent<CindarsHope.Enemy.EnemyVulnerabilityState>();
                var guaranteedCrit = vulnerability != null && vulnerability.IsVulnerable;
                var finalDamage = weapon.BaseDamage;
                var isCrit = false;
                if (_statsProvider != null)
                {
                    // F03: inclui scaling por atributo da arma.
                    finalDamage = _statsProvider.FinalDamage(weapon, weight, guaranteedCrit, out isCrit);
                }

                var damageRequest = new DamageRequest(enemyHealth.EnemyId, finalDamage)
                {
                    DamageType = weapon.DamageType,
                    SourcePosition = transform.position,
                    KnockbackForce = _knockbackForce,
                    // fable_06: tags de material da arma (ex.: prata) para matching de vulnerabilidade.
                    WeaponMaterialTags = weapon.MaterialTagsApplied
                };

                int hpBefore = enemyHealth.CurrentHp;
                enemyHealth.TakeDamage(damageRequest);
                hitEnemies++;

                // F02: dano de posture por peso (quebra → stagger + CoreExposed).
                var posture = enemyHealth.GetComponent<EnemyPostureState>();
                if (posture != null)
                {
                    posture.ApplyPostureDamage(weapon.BaseDamage * AttackChargeRules.PostureMultiplier(weight));
                }

                Debug.Log($"CombatLog: PlayerAttackDamageApplied. EnemyId={enemyHealth.EnemyId}, BaseDamage={weapon.BaseDamage}, FinalDamage={finalDamage}, Weight={weight}, Crit={isCrit}, HP={hpBefore}->{enemyHealth.CurrentHp}", this);
            }

            if (hitEnemies == 0)
            {
                Debug.Log($"CombatLog: PlayerAttackMissed. Reason={(candidatesTotal == 0 ? "NoCollidersInRange" : "NoEnemyHealthInColliders")}, AttackCenter={attackCenter}, Range={weapon.Range:F2}, CollidersSeen={candidatesTotal}, Direction={direction}", this);
            }
        }

        private void ExecuteRangedAttack(WeaponDataSO weapon, Vector2 direction)
        {
            // SPEC_06: Use ProjectileSpawnService to centralize spawn logic
            var spawnRequest = new ProjectileSpawnRequest(
                weapon.ProjectilePrefab,
                (Vector2)transform.position,
                direction,
                weapon.ProjectileSpeed,
                weapon.Range,
                weapon.BaseDamage,
                weapon.DamageType,
                _knockbackForce,
                spawnOffset: 0.5f
            );

            var spawnResult = ProjectileSpawnService.SpawnProjectile(spawnRequest);
            if (!spawnResult.Success)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}", this);
            }
        }

        private void ExecuteSpellAttack(SpellDataSO spellData, Vector2 direction)
        {
            if (spellData.ProjectilePrefab == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=SpellHasNoProjectilePrefab, SpellId={spellData.Id}", this);
                return;
            }

            // SPEC_06: Use ProjectileSpawnService to centralize spawn logic
            CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect = null;
            if (!string.IsNullOrEmpty(spellData.StatusEffectId))
                statusEffect = Resources.Load<CindarsHope.Combat.StatusEffect.StatusEffectSO>(spellData.StatusEffectId);

            var spawnRequest = new ProjectileSpawnRequest(
                spellData.ProjectilePrefab,
                (Vector2)transform.position,
                direction,
                spellData.ProjectileSpeed,
                spellData.Range,
                spellData.BaseDamage,
                spellData.DamageType,
                _knockbackForce,
                spawnOffset: 0.5f,
                statusEffect: statusEffect,
                statusApplyChance: spellData.StatusApplyChance
            );

            var spawnResult = ProjectileSpawnService.SpawnProjectile(spawnRequest);
            if (!spawnResult.Success)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}", this);
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
