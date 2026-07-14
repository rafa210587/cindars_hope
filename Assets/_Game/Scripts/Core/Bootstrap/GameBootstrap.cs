using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Bootstrap.Installers;
using CindarsHope.Core.Data;
using CindarsHope.Core.Respawn;
using CindarsHope.Core.Time;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Core.Bootstrap
{
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        private const string CombatRuntimeDatabasesRegistryResourcePath = "CombatRuntimeDatabasesRegistry";

        private static GameBootstrap _instance;

        // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) — PlayerManager
        // nao eh mais passado por aqui via campo serializado; resolvido via DomainManagerRegistry (nao
        // static Instance/Active, proibido pela regra de ratchet GlobalGoldAccess para este tipo).
        // PlayerProgressionManager/StatusEffectManager idem, self-registram via static Instance (molde
        // Craft/Economy/Skills/Equipment). StaminaManager/ManaManager/HungerManager/PlayerDataSO
        // permanecem campos serializados, com o tipo totalmente qualificado (sem using
        // CindarsHope.Player*) para nao reintroduzir a aresta Core->Player.
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private GameTimeManager _gameTimeManager;
        // arch: quebra do ciclo Core|UI (spec_arch_core_ui_cycle_reduction_v38) — tipo totalmente
        // qualificado (sem using CindarsHope.UI.Modal) para nao reintroduzir a aresta Core->UI; o
        // campo/property permanecem para os ~40 consumidores existentes de GameBootstrap.Instance.ModalManager.
        [SerializeField] private CindarsHope.UI.Modal.ModalManager _modalManager;
        // arch: quebra do ciclo Core|Save (spec_arch_core_save_cycle_reduction_v39) — tipo totalmente
        // qualificado (sem using CindarsHope.Save) para nao reintroduzir a aresta Core->Save; o
        // campo/property permanecem como shim para os consumidores existentes de
        // GameBootstrap.Instance.SaveManager (SceneManagement, UI, Editor validators).
        [SerializeField] private CindarsHope.Save.SaveManager _saveManager;
        [SerializeField] private CindarsHope.Player.HungerManager _hungerManager;
        [SerializeField] private CindarsHope.Player.StaminaManager _staminaManager;
        [SerializeField] private CindarsHope.Player.Data.PlayerDataSO _playerData;
        // arch: quebra do ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) —
        // ItemDatabaseSO agora vive em CindarsHope.Inventory.Data; referenciado por nome totalmente
        // qualificado (sem using CindarsHope.Inventory) para nao reintroduzir a aresta Core->Inventory.
        [SerializeField] private CindarsHope.Inventory.Data.ItemDatabaseSO _itemDatabase;
        [SerializeField] private WeaponDatabaseSO _weaponDatabase;
        [SerializeField] private SpellDatabaseSO _spellDatabase;
        [SerializeField] private StatusEffectDatabaseSO _statusEffectDatabase;
        [SerializeField] private CindarsHope.Player.ManaManager _manaManager;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private AnyaFountain _anyaFountain;

        private CaveRuntimeState _cachedCaveRunState;
        private CindarsHope.Player.Death.CorpseRecoveryManager _corpseRecoveryManager;

        public static GameBootstrap Instance => _instance;

        public CindarsHope.Player.PlayerManager PlayerManager =>
            CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();

        // arch: quebra do ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) —
        // InventoryManager nao eh mais passado por aqui via campo serializado; resolvido via
        // DomainManagerRegistry (nao static Instance/Active, proibido pela regra de ratchet
        // GlobalInventoryAccess para este tipo). Nome totalmente qualificado, sem using
        // CindarsHope.Inventory, para nao reintroduzir a aresta Core->Inventory.
        public CindarsHope.Inventory.InventoryManager InventoryManager =>
            CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();

        public TimeManager TimeManager => _timeManager;
        public GameTimeManager GameTimeManager => _gameTimeManager;
        public CindarsHope.UI.Modal.ModalManager ModalManager => _modalManager;
        public CindarsHope.Save.SaveManager SaveManager => _saveManager;
        public CindarsHope.Player.HungerManager HungerManager => _hungerManager;
        public CindarsHope.Player.StaminaManager StaminaManager => _staminaManager;
        public CindarsHope.Player.ManaManager ManaManager => _manaManager;

        // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) —
        // PlayerProgressionManager/StatusEffectManager nao sao mais passados por aqui; self-registram
        // via static Instance (molde Craft/Economy/Skills/Equipment).
        public CindarsHope.Player.Progression.PlayerProgressionManager PlayerProgressionManager =>
            CindarsHope.Player.Progression.PlayerProgressionManager.Instance;

        public CindarsHope.Player.StatusEffectManager StatusEffectManager =>
            CindarsHope.Player.StatusEffectManager.Instance;

        public CaveRunManager CaveRunManager => _caveRunManager;
        public CindarsHope.Player.Death.CorpseRecoveryManager CorpseRecoveryManager => _corpseRecoveryManager;
        public AnyaFountain AnyaFountain => _anyaFountain;
        public CindarsHope.Inventory.Data.ItemDatabaseSO ItemDatabase => _itemDatabase;
        public WeaponDatabaseSO WeaponDatabase => _weaponDatabase;
        public SpellDatabaseSO SpellDatabase => _spellDatabase;
        public StatusEffectDatabaseSO StatusEffectDatabase => _statusEffectDatabase;
        public CaveRuntimeState CachedCaveRunState => _cachedCaveRunState;

        public void SetCachedCaveRunState(CaveRuntimeState state)
        {
            _cachedCaveRunState = state;
            if (state != null)
            {
                Debug.Log($"GameBootstrap: cached CaveRunState. RunSeed={state.CaveRunSeed}, Level={state.CurrentCaveLevel}", this);
            }
        }

        public CaveRuntimeState TakeCachedCaveRunState()
        {
            var state = _cachedCaveRunState;
            _cachedCaveRunState = null;
            if (state != null)
            {
                Debug.Log($"GameBootstrap: restored CaveRunState from cache. RunSeed={state.CaveRunSeed}, Level={state.CurrentCaveLevel}", this);
            }
            return state;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }

        private void OnDestroy()
        {
            if (_instance != this)
            {
                return;
            }

            ShutdownManagers();
            _instance = null;
        }

        private void InitializeManagers()
        {
            EnsureCombatRuntimeReferences();

            // arch: quebra do ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) —
            // resolvido via DomainManagerRegistry (nao static Instance/Active, proibido para este tipo
            // pela regra de ratchet GlobalInventoryAccess) em vez do campo serializado removido.
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();

            // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) — resolvido
            // via DomainManagerRegistry (nao static Instance/Active, proibido pela regra de ratchet
            // GlobalGoldAccess) em vez do campo serializado removido.
            var playerManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();

            if (playerManager != null)
            {
                if (_playerData != null)
                {
                    playerManager.Initialize(_playerData);
                }
                else
                {
                    Debug.LogWarning("GameBootstrap is missing a PlayerDataSO reference. PlayerManager will initialize without starting state.", this);
                    playerManager.Initialize();
                }
            }
            else
            {
                Debug.LogWarning("GameBootstrap: PlayerManager.Awake ainda nao registrou no DomainManagerRegistry (referencia ausente na cena).", this);
            }

            if (inventoryManager != null)
            {
                if (_playerData != null && _itemDatabase != null)
                {
                    inventoryManager.InitializeFromStartingItems(_playerData, _itemDatabase);
                }
                else
                {
                    Debug.LogWarning("GameBootstrap is missing PlayerDataSO or ItemDatabaseSO. InventoryManager will initialize without starting items.", this);
                    inventoryManager.Initialize();
                }
            }
            else
            {
                Debug.LogWarning("GameBootstrap: InventoryManager.Awake ainda nao registrou no DomainManagerRegistry (referencia ausente na cena).", this);
            }

            // Loadout inicial de combate: deixa arco + flecha JA EQUIPADOS num jogo novo (arco numa mao,
            // flecha na outra) para o arco/flecha ser testavel sem abrir o painel de equipamento. So
            // preenche maos VAZIAS; ao carregar um save, EquipmentManager.RestoreFromSaveData faz
            // _slots.Clear() e reconstroi do save, entao o save sempre vence (sem vazar este default).
            EquipStarterCombatLoadout();

            if (_timeManager != null)
            {
                _timeManager.Initialize();
            }
            else
            {
                Debug.LogWarning("GameBootstrap is missing a TimeManager reference.", this);
            }

            if (_gameTimeManager != null)
            {
                if (_modalManager != null)
                {
                    _gameTimeManager.Initialize();
                }
                else
                {
                    Debug.LogWarning("GameBootstrap: ModalManager ausente para GameTimeManager. GameTimeManager não será inicializado.", this);
                }
            }
            else
            {
                Debug.LogWarning("GameBootstrap is missing a GameTimeManager reference.", this);
            }

            if (_saveManager != null)
            {
                _saveManager.RebindStarterInventoryData(_playerData, _itemDatabase);
                _saveManager.Initialize();

                // SPEC 14A-FIX14: drop hotbar bindings that don't have a matching item in the inventory.
                // Prevents the "hotbar shows item_seed_wheat but Inventory is empty" inconsistency.
                if (inventoryManager != null && _saveManager.HotbarState != null)
                {
                    var hotbar = _saveManager.HotbarState;
                    inventoryManager.ClearHotbarBindingsForMissingItems(
                        hotbar.GetSlotItemId,
                        (slot, id) => hotbar.SetSlot(slot, id),
                        CindarsHope.Foundation.HotbarState.SlotCount);
                }
            }
            else
            {
                Debug.LogWarning("GameBootstrap is missing a SaveManager reference.", this);
            }

            if (_hungerManager != null)
            {
                if (_playerData != null)
                {
                    _hungerManager.Initialize(_playerData);
                }
                else
                {
                    Debug.LogWarning("GameBootstrap: PlayerDataSO ausente. HungerManager não será inicializado pelo Bootstrap.", this);
                }
            }

            if (_staminaManager != null)
            {
                _staminaManager.Initialize();
            }
            else
            {
                Debug.LogWarning("GameBootstrap is missing a StaminaManager reference.", this);
            }

            if (_manaManager != null)
            {
                _manaManager.Initialize();
            }

            var craftingManager = DomainManagerRegistry.Get<ICraftingRuntimeManager>();
            if (craftingManager != null)
            {
                craftingManager.Initialize();
            }

            InitializeBootstrapRuntimeServices();

            // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) —
            // PlayerProgressionManager/StatusEffectManager nao sao mais passados por aqui; self-registram
            // via static Instance (molde Craft/Economy/Skills/Equipment).
            var progressionManager = CindarsHope.Player.Progression.PlayerProgressionManager.Instance;
            var statusEffectManager = CindarsHope.Player.StatusEffectManager.Instance;

            if (statusEffectManager != null)
            {
                statusEffectManager.Initialize();
            }

            // arch: Core|Skills (spec_arch_core_skills_cycle_reduction_v34_followup) — SkillTreeManager
            // nao eh mais referenciado pelo tipo concreto aqui; resolvido via DomainManagerRegistry
            // (molde IEquipmentRuntime/ICraftingRuntimeManager) contra a porta ISkillTreeRuntime em
            // Foundation. O campo _skillTreeManager do SaveManager continua sendo re-setado pelos
            // *SceneRuntimeReferenceInstaller (Town/Farm/Cave) com o tipo concreto logo em seguida.
            var skillTreeManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.ISkillTreeRuntime>();
            if (skillTreeManager == null)
            {
                Debug.LogError($"GameBootstrap skill tree wiring missing in scene '{gameObject.scene.name}' on GameObject '{gameObject.name}': ISkillTreeRuntime not registered.", this);
            }
            else
            {
                skillTreeManager.RebindProgressionManager();
            }

            if (_saveManager != null)
            {
                // arch: Core|Enemy (spec_arch_core_enemy_cycle_reduction_v31) — BestiaryManager nao eh
                // mais passado por aqui; SaveManager resolve via BestiaryManager.Instance (self-registro).
                _saveManager.RebindOptionalRuntimeManagers(null, progressionManager, _gameTimeManager, _staminaManager, statusEffectManager);
            }

            CombatRuntimeInstaller.Install(BuildCombatInstallContext(), this);

            InitializeDeathSystem();
        }

        // Ids do loadout inicial de combate (mesmos do StartingItems/hotbar). Arco de madeira tem o
        // WeaponDataSO (Type=Bow) e o ProjectilePrefab; flecha basica e a municao equipavel canonica.
        private const string StarterBowItemId = "item_weapon_bow_wood";
        private const string StarterArrowItemId = "item_ammo_arrow_basic";

        // Deixa arco + flecha equipados num jogo novo. So preenche maos VAZIAS (num load, o
        // EquipmentManager limpa e reconstroi do save depois — o save sempre vence).
        private void EquipStarterCombatLoadout()
        {
            var equipmentRuntime = DomainManagerRegistry.Get<IEquipmentRuntime>();
            // arch: Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) — resolvido via
            // DomainManagerRegistry em vez do campo serializado removido.
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();
            if (equipmentRuntime == null || inventoryManager == null)
            {
                return;
            }

            bool rightEmpty = string.IsNullOrEmpty(equipmentRuntime.GetEquippedItem(EquipmentSlot.RightHand));
            bool leftEmpty = string.IsNullOrEmpty(equipmentRuntime.GetEquippedItem(EquipmentSlot.LeftHand));
            if (!rightEmpty || !leftEmpty)
            {
                return;
            }

            // Arco numa mao, flecha na outra: BowArrowAttackService exige o arco na mao OPOSTA a municao.
            if (inventoryManager.HasItem(StarterBowItemId) && inventoryManager.HasItem(StarterArrowItemId))
            {
                equipmentRuntime.EquipItem(EquipmentSlot.RightHand, StarterBowItemId);
                equipmentRuntime.EquipItem(EquipmentSlot.LeftHand, StarterArrowItemId);
                Debug.Log($"GameBootstrap: loadout inicial equipado (arco '{StarterBowItemId}' RightHand, flecha '{StarterArrowItemId}' LeftHand).", this);
            }
            else
            {
                Debug.Log("GameBootstrap: loadout inicial de arco/flecha pulado — itens ausentes no inventario inicial.", this);
            }
        }

        private CombatRuntimeInstallContext BuildCombatInstallContext()
        {
            return new CombatRuntimeInstallContext
            {
                ItemDatabase = _itemDatabase,
                WeaponDatabase = _weaponDatabase,
                SpellDatabase = _spellDatabase,
                StatusEffectDatabase = _statusEffectDatabase,
                StaminaManager = _staminaManager,
                ManaManager = _manaManager
            };
        }

        private void EnsureCombatRuntimeReferences()
        {
            if (_weaponDatabase == null || _spellDatabase == null)
            {
                var combatRegistry = Resources.Load<CombatRuntimeDatabasesRegistrySO>(CombatRuntimeDatabasesRegistryResourcePath);
                if (combatRegistry == null)
                {
                    Debug.LogError($"GameBootstrap: CombatRuntimeDatabasesRegistry not found at Resources/{CombatRuntimeDatabasesRegistryResourcePath}. Combat database fallback cannot run.", this);
                }
                else
                {
                    if (_weaponDatabase == null)
                    {
                        _weaponDatabase = combatRegistry.WeaponDatabase;
                    }

                    if (_spellDatabase == null)
                    {
                        _spellDatabase = combatRegistry.SpellDatabase;
                    }
                }
            }

            if (_manaManager != null)
            {
                return;
            }

            _manaManager = GetComponent<CindarsHope.Player.ManaManager>();
            if (_manaManager != null)
            {
                return;
            }

            _manaManager = gameObject.AddComponent<CindarsHope.Player.ManaManager>();
        }

        private void InitializeDeathSystem()
        {
            var equipmentRuntime = DomainManagerRegistry.Get<IEquipmentRuntime>();
            // arch: Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) — resolvido via
            // DomainManagerRegistry em vez do campo serializado removido.
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();
            // arch: Core|Player (spec_arch_core_player_cycle_reduction_v37) — resolvido via
            // DomainManagerRegistry em vez do campo serializado removido.
            var playerManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();
            if (playerManager != null && inventoryManager != null && equipmentRuntime != null)
            {
                _corpseRecoveryManager = new CindarsHope.Player.Death.CorpseRecoveryManager(playerManager, inventoryManager, equipmentRuntime);
            }
            else
            {
                Debug.LogWarning("GameBootstrap: Missing required managers for death system initialization.", this);
            }

        }

        private void ShutdownManagers()
        {
            if (_saveManager != null)
            {
                _saveManager.Shutdown();
            }

            // arch: Core|Player (spec_arch_core_player_cycle_reduction_v37) — StatusEffectManager
            // resolvido via static Instance (molde Craft/Economy/Skills/Equipment) em vez do campo
            // serializado removido.
            var statusEffectManager = CindarsHope.Player.StatusEffectManager.Instance;
            if (statusEffectManager != null)
            {
                statusEffectManager.Shutdown();
            }

            if (_staminaManager != null)
            {
                _staminaManager.Shutdown();
            }

            if (_gameTimeManager != null)
            {
                _gameTimeManager.Shutdown();
            }

            if (_timeManager != null)
            {
                _timeManager.Shutdown();
            }

            // arch: Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) — resolvido via
            // DomainManagerRegistry em vez do campo serializado removido.
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>();
            if (inventoryManager != null)
            {
                inventoryManager.Shutdown();
            }

            // arch: Core|Player (spec_arch_core_player_cycle_reduction_v37) — resolvido via
            // DomainManagerRegistry em vez do campo serializado removido.
            var playerManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();
            if (playerManager != null)
            {
                playerManager.Shutdown();
            }

            var craftingManager = DomainManagerRegistry.Get<ICraftingRuntimeManager>();
            if (craftingManager != null && craftingManager.IsInitialized)
            {
                craftingManager.Shutdown();
            }

            ShutdownBootstrapRuntimeServices();
        }

        private void InitializeBootstrapRuntimeServices()
        {
            var context = new GameBootstrapRuntimeContext(_itemDatabase);
            var foundShopService = false;

            foreach (var behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is not IGameBootstrapRuntimeService service)
                {
                    continue;
                }

                service.InitializeFromBootstrap(context);
                foundShopService |= service.BootstrapServiceId == "ShopManager";
            }

            if (!foundShopService)
            {
                Debug.LogError($"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(GameBootstrap)}' could not find a ShopManager bootstrap service to initialize.", this);
            }
        }

        private void ShutdownBootstrapRuntimeServices()
        {
            foreach (var behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is IGameBootstrapRuntimeService service && service.IsInitialized)
                {
                    service.ShutdownFromBootstrap();
                }
            }
        }
    }
}
