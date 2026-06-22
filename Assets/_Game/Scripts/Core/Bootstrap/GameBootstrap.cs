using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Bootstrap.Installers;
using CindarsHope.Skills;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Enemy;
using CindarsHope.Inventory;
using CindarsHope.Locations;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using CindarsHope.Save;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.Core.Bootstrap
{
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        private const string CombatRuntimeDatabasesRegistryResourcePath = "CombatRuntimeDatabasesRegistry";

        private static GameBootstrap _instance;

        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private GameTimeManager _gameTimeManager;
        [SerializeField] private ModalManager _modalManager;
        [SerializeField] private SaveManager _saveManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private CraftingManager _craftingManager;
        [SerializeField] private EconomyManager _economyManager;
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private PlayerProgressionManager _progressionManager;
        [SerializeField] private StatusEffectManager _statusEffectManager;
        [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private WeaponDatabaseSO _weaponDatabase;
        [SerializeField] private SpellDatabaseSO _spellDatabase;
        [SerializeField] private StatusEffectDatabaseSO _statusEffectDatabase;
        [SerializeField] private SkillActionDatabaseSO _skillActionDatabase;
        [SerializeField] private ManaManager _manaManager;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private AnyaFountain _anyaFountain;
        [SerializeField] private Skills.SkillTreeManager _skillTreeManager;
        [SerializeField] private BestiaryManager _bestiaryManager;

        private CaveRuntimeState _cachedCaveRunState;
        private CorpseRecoveryManager _corpseRecoveryManager;

        public static GameBootstrap Instance => _instance;

        public PlayerManager PlayerManager => _playerManager;
        public InventoryManager InventoryManager => _inventoryManager;
        public TimeManager TimeManager => _timeManager;
        public GameTimeManager GameTimeManager => _gameTimeManager;
        public ModalManager ModalManager => _modalManager;
        public SaveManager SaveManager => _saveManager;
        public HungerManager HungerManager => _hungerManager;
        public StaminaManager StaminaManager => _staminaManager;
        public ManaManager ManaManager => _manaManager;
        public CraftingManager CraftingManager => _craftingManager;
        public EconomyManager EconomyManager => _economyManager;
        public ShopManager ShopManager => _shopManager;
        public EquipmentManager EquipmentManager => _equipmentManager;
        public PlayerProgressionManager PlayerProgressionManager => _progressionManager;
        public StatusEffectManager StatusEffectManager => _statusEffectManager;
        public CaveRunManager CaveRunManager => _caveRunManager;
        public CorpseRecoveryManager CorpseRecoveryManager => _corpseRecoveryManager;
        public AnyaFountain AnyaFountain => _anyaFountain;
        public Skills.SkillTreeManager SkillTreeManager => _skillTreeManager;
        public BestiaryManager BestiaryManager => _bestiaryManager;
        public ItemDatabaseSO ItemDatabase => _itemDatabase;
        public WeaponDatabaseSO WeaponDatabase => _weaponDatabase;
        public SpellDatabaseSO SpellDatabase => _spellDatabase;
        public StatusEffectDatabaseSO StatusEffectDatabase => _statusEffectDatabase;
        public SkillActionDatabaseSO SkillActionDatabase => _skillActionDatabase;
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
            EnsurePersistentShopManager();
            EnsurePersistentBestiaryManager();
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

            if (_playerManager != null)
            {
                if (_playerData != null)
                {
                    _playerManager.Initialize(_playerData);
                }
                else
                {
                    Debug.LogWarning("GameBootstrap is missing a PlayerDataSO reference. PlayerManager will initialize without starting state.", this);
                    _playerManager.Initialize();
                }
            }
            else
            {
                Debug.LogWarning("GameBootstrap is missing a PlayerManager reference.", this);
            }

            if (_inventoryManager != null)
            {
                if (_playerData != null && _itemDatabase != null)
                {
                    _inventoryManager.InitializeFromStartingItems(_playerData, _itemDatabase);
                }
                else
                {
                    Debug.LogWarning("GameBootstrap is missing PlayerDataSO or ItemDatabaseSO. InventoryManager will initialize without starting items.", this);
                    _inventoryManager.Initialize();
                }
            }
            else
            {
                Debug.LogWarning("GameBootstrap is missing an InventoryManager reference.", this);
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
                if (_inventoryManager != null && _saveManager.HotbarState != null)
                {
                    var hotbar = _saveManager.HotbarState;
                    _inventoryManager.ClearHotbarBindingsForMissingItems(
                        hotbar.GetSlotItemId,
                        (slot, id) => hotbar.SetSlot(slot, id),
                        CindarsHope.UI.Hotbar.HotbarState.SlotCount);
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

            if (_craftingManager != null)
            {
                _craftingManager.Initialize();
            }

            if (_economyManager != null)
            {
                _economyManager.Initialize();
            }

            if (_shopManager != null)
            {
                _shopManager.Configure(_itemDatabase);
            }
            else
            {
                Debug.LogError($"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(GameBootstrap)}' field '_shopManager' could not be initialized.", this);
            }

            if (_statusEffectManager != null)
            {
                _statusEffectManager.Initialize();
            }

            if (_skillTreeManager == null)
            {
                Debug.LogError($"GameBootstrap skill tree wiring missing in scene '{gameObject.scene.name}' on GameObject '{gameObject.name}': _skillTreeManager.", this);
            }
            else
            {
                _skillTreeManager.RebindProgressionManager(_progressionManager);
            }

            if (_saveManager != null)
            {
                _saveManager.RebindOptionalRuntimeManagers(_equipmentManager, _progressionManager, _gameTimeManager, _staminaManager, _statusEffectManager, _skillTreeManager, _shopManager, _bestiaryManager);
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
            if (_equipmentManager == null || _inventoryManager == null)
            {
                return;
            }

            bool rightEmpty = string.IsNullOrEmpty(_equipmentManager.GetEquippedItem(EquipmentSlot.RightHand));
            bool leftEmpty = string.IsNullOrEmpty(_equipmentManager.GetEquippedItem(EquipmentSlot.LeftHand));
            if (!rightEmpty || !leftEmpty)
            {
                return;
            }

            // Arco numa mao, flecha na outra: BowArrowAttackService exige o arco na mao OPOSTA a municao.
            if (_inventoryManager.HasItem(StarterBowItemId) && _inventoryManager.HasItem(StarterArrowItemId))
            {
                _equipmentManager.EquipItem(EquipmentSlot.RightHand, StarterBowItemId);
                _equipmentManager.EquipItem(EquipmentSlot.LeftHand, StarterArrowItemId);
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
                EquipmentManager = _equipmentManager,
                InventoryManager = _inventoryManager,
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

            _manaManager = GetComponent<ManaManager>();
            if (_manaManager != null)
            {
                return;
            }

            _manaManager = gameObject.AddComponent<ManaManager>();
        }

        private void EnsurePersistentBestiaryManager()
        {
            if (_bestiaryManager != null)
            {
                return;
            }

            _bestiaryManager = GetComponent<BestiaryManager>();
            if (_bestiaryManager != null)
            {
                return;
            }

            _bestiaryManager = gameObject.AddComponent<BestiaryManager>();
            Debug.LogWarning($"GameBootstrap created missing BestiaryManager on '{gameObject.name}'. Scene should serialize this reference on next scene generation.", this);
        }

        private void InitializeDeathSystem()
        {
            if (_playerManager != null && _inventoryManager != null && _equipmentManager != null)
            {
                _corpseRecoveryManager = new CorpseRecoveryManager(_playerManager, _inventoryManager, _equipmentManager);
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

            if (_statusEffectManager != null)
            {
                _statusEffectManager.Shutdown();
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

            if (_inventoryManager != null)
            {
                _inventoryManager.Shutdown();
            }

            if (_playerManager != null)
            {
                _playerManager.Shutdown();
            }

            if (_craftingManager != null && _craftingManager.IsInitialized)
            {
                _craftingManager.Shutdown();
            }

            if (_economyManager != null && _economyManager.IsInitialized)
            {
                _economyManager.Shutdown();
            }

            if (_shopManager != null && _shopManager.IsInitialized)
            {
                _shopManager.Shutdown();
            }
        }

        private void EnsurePersistentShopManager()
        {
            if (_shopManager != null)
            {
                return;
            }

            _shopManager = GetComponent<ShopManager>();
            if (_shopManager != null)
            {
                Debug.Log($"GameBootstrap adopted serialized ShopManager on persistent bootstrap in scene '{gameObject.scene.path}'.", this);
                return;
            }

            _shopManager = gameObject.AddComponent<ShopManager>();
            Debug.Log($"GameBootstrap created persistent ShopManager because scene '{gameObject.scene.path}' did not serialize one.", this);
        }
    }
}
