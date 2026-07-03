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
    public partial class PlayerAttackController : MonoBehaviour
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

        private readonly PlayerAttackCore _attackCore = new PlayerAttackCore();

        // SPEC_05: Service extraction
        private EquippedItemResolver _itemResolver;
        private CombatActionContext _currentActionContext;

        // SPEC_07: Attack services
        private BowArrowAttackService _bowArrowService;
        private SpellCastService _spellCastService;

        // fable_08: gerencia janela de cast time + interrupt (criado em Start no player).
        private CindarsHope.Combat.Magic.SpellCastRoutine _spellCastRoutine;

        // F02: carga por mÃ£o (tap=light, hold=heavy, hold longo=charged) + stats derivados.
        private readonly AttackChargeTracker _leftCharge = new AttackChargeTracker();
        private readonly AttackChargeTracker _rightCharge = new AttackChargeTracker();
        private PlayerCombatStatsProvider _statsProvider;
        private SpriteRenderer _chargeTelegraphRenderer;
        private Color _chargeTelegraphBaseColor = Color.white;

        // spec_codex_13: buffer reutilizavel para as queries de combate deste controller
        // (ContactFilter2D + NonAlloc), evitando alocacao por ataque. Tamanho fixo — convencao
        // compartilhada com SpellCastService (mesmo tamanho de buffer entre os pontos de query).
        internal const int CombatQueryBufferSize = 32;
        private readonly Collider2D[] _combatQueryBuffer = new Collider2D[CombatQueryBufferSize];
        private readonly System.Collections.Generic.HashSet<EnemyHealth> _combatQuerySeenBuffer =
            new System.Collections.Generic.HashSet<EnemyHealth>();
        private ContactFilter2D _enemyContactFilter;
        private bool _enemyContactFilterInitialized;

        // fable_08 / spec_codex_13: ContactFilter2D configurado por LayerMask("Enemy"), com
        // fallback para NoFilter (sem mask) quando o layer ainda nao existe — nao quebra o
        // comportamento atual antes do humano rodar CindarsHope/Inicializar Projeto.
        private ContactFilter2D EnemyContactFilter
        {
            get
            {
                if (!_enemyContactFilterInitialized)
                {
                    var mask = CindarsHope.Core.Physics.GameplayLayerNames.GetMaskSafe(
                        CindarsHope.Core.Physics.GameplayLayerNames.Enemy);
                    _enemyContactFilter = mask.value != 0 ? new ContactFilter2D() : ContactFilter2D.noFilter;
                    if (mask.value != 0)
                    {
                        _enemyContactFilter.SetLayerMask(mask);
                    }
                    _enemyContactFilter.useTriggers = true;
                    _enemyContactFilterInitialized = true;
                }
                return _enemyContactFilter;
            }
        }

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

            // F02: provider de stats derivados (DerivedStatsCalculator WAVE 05, antes Ã³rfÃ£o).
            // Base de Attack = ForÃ§a do player; equipment dict entra quando F03 criar o registry.
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
            // renderer da carga). Anexa ao PlayerController quando houver; senÃ£o neste GameObject.
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
            CombatLog.Log($"CombatLog: EquipmentSlotChanged. Slot={evt.Slot}, ItemInstanceId='{evt.ItemInstanceId ?? "<null>"}'", this);
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
            // F02: serviÃ§os consomem o mesmo provider (dano derivado em projÃ©teis).
            if (_bowArrowService != null) _bowArrowService.StatsProvider = _statsProvider;
            if (_spellCastService != null)
            {
                _spellCastService.StatsProvider = _statsProvider;
                // fable_08: alvos de SelfRestore + spellbook (fable_07).
                _spellCastService.PlayerManager = GameBootstrap.Instance?.PlayerManager;
                _spellCastService.StaminaManager = _staminaManager;
                _spellCastService.Spellbook = CindarsHope.Magic.PlayerSpellbook.Instance;
                // fable_08 EMENDA 6.6-A: auto-target via QUERY de Physics2D (inimigos no raio),
                // NÃƒO FindObjectsByType (rule unity-architecture). Devolve posiÃ§Ãµes de EnemyHealth.
                _spellCastService.EnemyPositionQuery = QueryEnemyPositions;
            }
        }

        // fable_08: posicoes de inimigos vivos dentro do raio, para auto-target/area.
        // spec_codex_13: ContactFilter2D (mask "Enemy" com fallback NoFilter) + buffer
        // pre-alocado reutilizavel (OverlapCircle NonAlloc) — sem List/HashSet novos por chamada.
        private readonly System.Collections.Generic.List<UnityEngine.Vector2> _enemyPositionQueryResult =
            new System.Collections.Generic.List<UnityEngine.Vector2>();

        private System.Collections.Generic.IReadOnlyList<UnityEngine.Vector2> QueryEnemyPositions(UnityEngine.Vector2 center, float radius)
        {
            _enemyPositionQueryResult.Clear();
            _combatQuerySeenBuffer.Clear();
            int count = Physics2D.OverlapCircle(center, Mathf.Max(0.1f, radius), EnemyContactFilter, _combatQueryBuffer);
            for (int i = 0; i < count; i++)
            {
                var col = _combatQueryBuffer[i];
                if (col == null) continue;
                var enemy = col.GetComponentInParent<EnemyHealth>() ?? col.GetComponent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || !_combatQuerySeenBuffer.Add(enemy)) continue;
                _enemyPositionQueryResult.Add(enemy.transform.position);
            }
            return _enemyPositionQueryResult;
        }

        private void Update()
        {
            bool modalOpen = GameBootstrap.Instance?.ModalManager?.HasActiveModal == true;

            // SPEC 14A-FIX13: J removido como ataque. Q = mao esquerda (tool), E = mao direita
            // (weapon, ou interagir se ha InteractionCandidate).
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
            {
                string key = Input.GetKeyDown(KeyCode.Q) ? "Q" : "E";
                CombatLog.Log($"CombatLog: PlayerAttackInputReceived. Key={key}, ModalOpen={modalOpen}", this);
            }

            if (modalOpen)
            {
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
                    CombatLog.Log("CombatLog: PlayerAttackBlocked. Reason=ModalActive", this);
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
                    CombatLog.Log("CombatLog: PlayerAttackBlocked. Reason=PlayerStunned", this);
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
                // fable_82: sinaliza windup ao EnemyBrain para evasao reativa.
                GameEventBus.Publish(new CindarsHope.Core.Events.PlayerAttackWindupEvent((UnityEngine.Vector2)transform.position));
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
                    // RegressÃ£o protegida: E com candidato continua interagindo (charge nÃ£o inicia).
                    CombatLog.Log("CombatLog: PlayerAttackBlocked. Reason=InteractionCandidatePresent (E used for interact)", this);
                }
                else
                {
                    _rightCharge.Begin(Time.time);
                    // fable_82: sinaliza windup ao EnemyBrain para evasao reativa.
                    GameEventBus.Publish(new CindarsHope.Core.Events.PlayerAttackWindupEvent((UnityEngine.Vector2)transform.position));
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
            AttackWithSlot(EquipmentSlot.LeftHand, equippedItemId, weight);
        }

        private void TryAttackRightHand(AttackWeight weight = AttackWeight.Light)
        {
            var equippedItemId = _equipmentManager != null ? _equipmentManager.GetEquippedItem(EquipmentSlot.RightHand) : null;
            AttackWithSlot(EquipmentSlot.RightHand, equippedItemId, weight);
        }

        // F02: telegraph simples de carga â€” tinta o sprite conforme o peso acumulado.
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

        private void TryDodge()
        {
            // Pass hasEnoughStamina=true here; actual stamina check (spend) happens below.
            // If stamina spend fails, BeginDodge is not called so dodge state is never set.
            if (!_attackCore.CanDodge(Time.time, _dodgeCooldownSeconds, hasEnoughStamina: true))
                return;

            if (_staminaManager != null && !_staminaManager.TrySpendStamina((int)_dodgeStaminaCost))
                return;

            _attackCore.BeginDodge(Time.time, _dodgeDurationSeconds);

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
            if (_attackCore.UpdateDodge(Time.time))
            {
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