using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Player.Progression;
using CindarsHope.Save;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.Core.Bootstrap
{
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
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
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private PlayerProgressionManager _progressionManager;
        [SerializeField] private StatusEffectManager _statusEffectManager;
        [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private WeaponDatabaseSO _weaponDatabase;
        [SerializeField] private SpellDatabaseSO _spellDatabase;
        [SerializeField] private SkillActionDatabaseSO _skillActionDatabase;

        private CaveRuntimeState _cachedCaveRunState;

        public static GameBootstrap Instance => _instance;

        public PlayerManager PlayerManager => _playerManager;
        public InventoryManager InventoryManager => _inventoryManager;
        public TimeManager TimeManager => _timeManager;
        public GameTimeManager GameTimeManager => _gameTimeManager;
        public ModalManager ModalManager => _modalManager;
        public SaveManager SaveManager => _saveManager;
        public HungerManager HungerManager => _hungerManager;
        public StaminaManager StaminaManager => _staminaManager;
        public CraftingManager CraftingManager => _craftingManager;
        public EconomyManager EconomyManager => _economyManager;
        public EquipmentManager EquipmentManager => _equipmentManager;
        public PlayerProgressionManager PlayerProgressionManager => _progressionManager;
        public StatusEffectManager StatusEffectManager => _statusEffectManager;
        public ItemDatabaseSO ItemDatabase => _itemDatabase;
        public WeaponDatabaseSO WeaponDatabase => _weaponDatabase;
        public SpellDatabaseSO SpellDatabase => _spellDatabase;
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
                _saveManager.Initialize();
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

            if (_craftingManager != null)
            {
                _craftingManager.Initialize();
            }

            if (_economyManager != null)
            {
                _economyManager.Initialize();
            }

            if (_statusEffectManager != null)
            {
                _statusEffectManager.Initialize();
            }

            if (_saveManager != null)
            {
                _saveManager.RebindOptionalRuntimeManagers(_equipmentManager, _progressionManager, _gameTimeManager, _staminaManager, _statusEffectManager);
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
        }
    }
}
