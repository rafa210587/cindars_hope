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
        // arch: quebra do par mutuo Core|UI (2026-07-15) — campo agora tipado como MonoBehaviour
        // (nao mais CindarsHope.UI.Modal.ModalManager) para que Core pare de nomear CindarsHope.UI;
        // Unity mantem a referencia de cena serializada normalmente (molde ManaManager/
        // ICraftingStationModal). A property expoe a porta IModalRuntime (Foundation) para os
        // consumidores existentes de GameBootstrap.Instance.ModalManager.
        [SerializeField] private MonoBehaviour _modalManager;
        // arch: quebra do par mutuo Core|Save (2026-07-15) — campo agora tipado como MonoBehaviour
        // (nao mais CindarsHope.Save.SaveManager) para que Core pare de nomear CindarsHope.Save;
        // Unity mantem a referencia de cena serializada normalmente (molde ModalManager/
        // ICraftingStationModal). A property expoe a porta ISaveRuntime (Foundation) para os
        // consumidores existentes de GameBootstrap.Instance.SaveManager. Rebind/Initialize/Shutdown
        // (assinaturas cross-modulo) agora sao acionados via IGameBootstrapRuntimeService, que
        // SaveManager implementa (ver InitializeBootstrapRuntimeServices/ShutdownBootstrapRuntimeServices).
        [SerializeField] private MonoBehaviour _saveManager;
        [SerializeField] private CindarsHope.Player.HungerManager _hungerManager;
        [SerializeField] private CindarsHope.Player.StaminaManager _staminaManager;
        [SerializeField] private CindarsHope.Player.Data.PlayerDataSO _playerData;
        // arch: quebra do par mutuo Core|Inventory (2026-07-15) — campo agora tipado como
        // ScriptableObject (nao mais CindarsHope.Inventory.Data.ItemDatabaseSO) para que Core pare de
        // nomear CindarsHope.Inventory; Unity mantem a referencia de cena serializada normalmente
        // (molde ModalManager/SaveManager). Consumidores fora de Core castam para o tipo concreto
        // localmente. InventoryManager recebe este valor via GameBootstrapRuntimeContext
        // (IGameBootstrapRuntimeService), nao mais por chamada direta.
        [SerializeField] private ScriptableObject _itemDatabase;
        [SerializeField] private WeaponDatabaseSO _weaponDatabase;
        [SerializeField] private SpellDatabaseSO _spellDatabase;
        [SerializeField] private StatusEffectDatabaseSO _statusEffectDatabase;
        [SerializeField] private CindarsHope.Player.ManaManager _manaManager;
        [SerializeField] private AnyaFountain _anyaFountain;

        private CindarsHope.Player.Death.CorpseRecoveryManager _corpseRecoveryManager;

        public static GameBootstrap Instance => _instance;

        public CindarsHope.Player.PlayerManager PlayerManager =>
            CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>();

        // arch: quebra do par mutuo Core|Inventory (2026-07-15) — InventoryManager nao eh mais passado
        // por aqui via campo serializado; resolvido via DomainManagerRegistry sob a porta
        // IInventoryRuntime (Foundation), nao static Instance/Active (proibido pela regra de ratchet
        // GlobalInventoryAccess) e sem nomear CindarsHope.Inventory.
        public CindarsHope.Foundation.IInventoryRuntime InventoryManager =>
            CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IInventoryRuntime>();

        public TimeManager TimeManager => _timeManager;
        public GameTimeManager GameTimeManager => _gameTimeManager;
        public CindarsHope.Foundation.IModalRuntime ModalManager => _modalManager as CindarsHope.Foundation.IModalRuntime;
        public CindarsHope.Foundation.ISaveRuntime SaveManager => _saveManager as CindarsHope.Foundation.ISaveRuntime;
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

        public CindarsHope.Player.Death.CorpseRecoveryManager CorpseRecoveryManager => _corpseRecoveryManager;
        public AnyaFountain AnyaFountain => _anyaFountain;
        public ScriptableObject ItemDatabase => _itemDatabase;
        public WeaponDatabaseSO WeaponDatabase => _weaponDatabase;
        public SpellDatabaseSO SpellDatabase => _spellDatabase;
        public StatusEffectDatabaseSO StatusEffectDatabase => _statusEffectDatabase;

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

            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — resolvido via
            // DomainManagerRegistry sob a porta IInventoryRuntime (nao static Instance/Active,
            // proibido para este tipo pela regra de ratchet GlobalInventoryAccess), sem nomear
            // CindarsHope.Inventory.
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IInventoryRuntime>();

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

            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — a populacao de starting items
            // (antigo inventoryManager.InitializeFromStartingItems(_playerData, _itemDatabase)) nao e
            // mais chamada diretamente aqui: exigiria nomear CindarsHope.Inventory.InventoryManager e
            // CindarsHope.Inventory.Data.ItemDatabaseSO. InventoryManager agora implementa
            // IGameBootstrapRuntimeService e recebe PlayerData/ItemDatabase via
            // GameBootstrapRuntimeContext dentro de InitializeBootstrapRuntimeServices() (mesmo molde
            // ja usado por SaveManager/ShopManager).
            if (inventoryManager == null)
            {
                Debug.LogWarning("GameBootstrap: InventoryManager.Awake ainda nao registrou no DomainManagerRegistry (referencia ausente na cena).", this);
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

            if (_saveManager == null)
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

            // arch: quebra do par mutuo Core|Save (2026-07-15) — SaveManager.RebindStarterInventoryData
            // + Initialize() nao sao mais chamados diretamente aqui; SaveManager implementa
            // IGameBootstrapRuntimeService e recebe PlayerDataSO/ItemDatabaseSO via
            // GameBootstrapRuntimeContext dentro do loop abaixo (mesmo molde ja usado por ShopManager).
            InitializeBootstrapRuntimeServices();

            // Loadout inicial de combate: deixa arco + flecha JA EQUIPADOS num jogo novo (arco numa mao,
            // flecha na outra) para o arco/flecha ser testavel sem abrir o painel de equipamento. So
            // preenche maos VAZIAS; ao carregar um save, EquipmentManager.RestoreFromSaveData faz
            // _slots.Clear() e reconstroi do save, entao o save sempre vence (sem vazar este default).
            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — movido para depois de
            // InitializeBootstrapRuntimeServices() porque e ali que InventoryManager.InitializeFromBootstrap
            // agora popula os starting items (antes rodava mais cedo, via chamada direta removida acima).
            EquipStarterCombatLoadout();

            // SPEC 14A-FIX14: drop hotbar bindings that don't have a matching item in the inventory.
            // Prevents the "hotbar shows item_seed_wheat but Inventory is empty" inconsistency. Movido
            // para depois de InitializeBootstrapRuntimeServices() porque e ali que SaveManager.Initialize()
            // agora roda (via IGameBootstrapRuntimeService) e popula os defaults do hotbar.
            var saveRuntime = _saveManager as CindarsHope.Foundation.ISaveRuntime;
            if (inventoryManager != null && saveRuntime != null && saveRuntime.HotbarState != null)
            {
                var hotbar = saveRuntime.HotbarState;
                inventoryManager.ClearHotbarBindingsForMissingItems(
                    hotbar.GetSlotItemId,
                    (slot, id) => hotbar.SetSlot(slot, id),
                    CindarsHope.Foundation.HotbarState.SlotCount);
            }

            // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) —
            // PlayerProgressionManager/StatusEffectManager nao sao mais passados por aqui; self-registram
            // via static Instance (molde Craft/Economy/Skills/Equipment).
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

            // arch: quebra do par mutuo Core|Save (2026-07-15) — a chamada
            // _saveManager.RebindOptionalRuntimeManagers(...) que existia aqui foi removida: os tres
            // *SceneRuntimeReferenceInstaller (Town/Farm/Cave) ja chamam
            // saveManager.RebindOptionalRuntimeManagers(...) em Start() com o conjunto completo
            // (incl. EquipmentManager/SkillTreeManager/ShopManager/BestiaryManager), sempre antes de
            // qualquer Save/Load disparado pelo jogador. Confirmado via cena gerada: os campos
            // _progressionManager/_gameTimeManager/_staminaManager/_statusEffectManager do SaveManager
            // ja vem wireados diretamente (nao fileID: 0) pelo gerador de cena, entao
            // SaveManager.Initialize() (Awake) ja os enxerga sem depender desta chamada precoce.
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
            // arch: Core|Inventory (2026-07-15) — resolvido via DomainManagerRegistry sob a porta
            // IInventoryRuntime, sem nomear CindarsHope.Inventory.
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IInventoryRuntime>();
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
            // arch: Core|Inventory (2026-07-15) — resolvido via DomainManagerRegistry sob a porta
            // IInventoryRuntime, sem nomear CindarsHope.Inventory. CorpseRecoveryManager agora recebe
            // IInventoryRuntime no construtor (nao mais o tipo concreto InventoryManager).
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IInventoryRuntime>();
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
            // arch: quebra do par mutuo Core|Save (2026-07-15) — _saveManager.Shutdown() nao e mais
            // chamado diretamente aqui; SaveManager implementa IGameBootstrapRuntimeService e e
            // desligado por ShutdownBootstrapRuntimeServices() no fim deste metodo (mesmo molde do
            // ShopManager). SaveManager.Shutdown() so alterna a flag IsInitialized (sem efeito
            // colateral observavel), entao a mudanca de ordem relativa e segura.

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

            // arch: Core|Inventory (2026-07-15) — resolvido via DomainManagerRegistry sob a porta
            // IInventoryRuntime, sem nomear CindarsHope.Inventory.
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IInventoryRuntime>();
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
            // arch: quebra do par mutuo Core|Save (2026-07-15) — PlayerData incluido no context para
            // que SaveManager.InitializeFromBootstrap chame RebindStarterInventoryData internamente.
            var context = new GameBootstrapRuntimeContext(_itemDatabase, _playerData);
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
