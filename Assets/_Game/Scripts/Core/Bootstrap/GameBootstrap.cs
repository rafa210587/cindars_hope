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

        // arch: quebra do par mutuo Core|Player (spec_arch_core_player_cycle_reduction_v37,
        // finalizado 2026-07-15) — PlayerManager nao eh mais passado por aqui via campo serializado;
        // resolvido via DomainManagerRegistry sob a porta IPlayerRuntime (Foundation), nao Instance/
        // Active estatico (proibido pela regra de ratchet GlobalGoldAccess para este tipo) e sem
        // nomear CindarsHope.Player. PlayerProgressionManager/StatusEffectManager idem, self-registram
        // via Instance estatico (molde Craft/Economy/Skills/Equipment) E via DomainManagerRegistry sob
        // as portas IPlayerProgressionRuntime/IStatusEffectRuntime para Core resolver sem nomear o
        // tipo concreto. StaminaManager/ManaManager/HungerManager/PlayerDataSO viram campos
        // serializados tipados como MonoBehaviour/ScriptableObject (molde ModalManager/SaveManager do
        // corte Core|UI/Core|Save); as portas IStaminaRuntime/IManaRuntime/IHungerRuntime (Foundation)
        // sao resolvidas via cast local so para as chamadas de lifecycle.
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
        [SerializeField] private MonoBehaviour _hungerManager;
        [SerializeField] private MonoBehaviour _staminaManager;
        [SerializeField] private ScriptableObject _playerData;
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
        [SerializeField] private MonoBehaviour _manaManager;
        [SerializeField] private AnyaFountain _anyaFountain;

        // arch: quebra do par mutuo Core|Player (2026-07-15) — CorpseRecoveryManager (Player.Death)
        // e uma classe C# pura (nao MonoBehaviour), construida via IPlayerRuntime.CreateCorpseRecoveryManager
        // (fabrica dentro do modulo Player); guardado como object pois Core nao pode nomear o tipo.
        private object _corpseRecoveryManager;

        public static GameBootstrap Instance => _instance;

        public MonoBehaviour PlayerManager =>
            CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IPlayerRuntime>() as MonoBehaviour;

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
        public MonoBehaviour HungerManager => _hungerManager;
        public MonoBehaviour StaminaManager => _staminaManager;
        public MonoBehaviour ManaManager => _manaManager;

        // arch: quebra do par mutuo Core|Player (spec_arch_core_player_cycle_reduction_v37,
        // finalizado 2026-07-15) — PlayerProgressionManager/StatusEffectManager nao sao mais
        // resolvidos pelo tipo concreto; resolvidos via DomainManagerRegistry sob as portas
        // IPlayerProgressionRuntime/IStatusEffectRuntime (Foundation), que os managers registram em
        // Awake alem do static Instance (molde Craft/Economy/Skills/Equipment).
        public MonoBehaviour PlayerProgressionManager =>
            CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IPlayerProgressionRuntime>() as MonoBehaviour;

        public MonoBehaviour StatusEffectManager =>
            CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IStatusEffectRuntime>() as MonoBehaviour;

        public object CorpseRecoveryManager => _corpseRecoveryManager;
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

            // arch: quebra do par mutuo Core|Player (spec_arch_core_player_cycle_reduction_v37,
            // finalizado 2026-07-15) — resolvido via DomainManagerRegistry sob a porta IPlayerRuntime
            // (nao static Instance/Active, proibido pela regra de ratchet GlobalGoldAccess) e sem
            // nomear CindarsHope.Player.
            var playerRuntime = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IPlayerRuntime>();

            if (playerRuntime != null)
            {
                if (_playerData != null)
                {
                    playerRuntime.Initialize(_playerData);
                }
                else
                {
                    Debug.LogWarning("GameBootstrap is missing a PlayerDataSO reference. PlayerManager will initialize without starting state.", this);
                    playerRuntime.Initialize();
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

            // arch: quebra do par mutuo Core|Player (2026-07-15) — _hungerManager/_staminaManager/
            // _manaManager sao MonoBehaviour (Core nao pode nomear os tipos concretos); as chamadas de
            // lifecycle resolvem as portas IHungerRuntime/IStaminaRuntime/IManaRuntime (Foundation)
            // por cast local.
            if (_hungerManager != null)
            {
                if (_playerData != null)
                {
                    (_hungerManager as CindarsHope.Foundation.IHungerRuntime)?.Initialize(_playerData);
                }
                else
                {
                    Debug.LogWarning("GameBootstrap: PlayerDataSO ausente. HungerManager não será inicializado pelo Bootstrap.", this);
                }
            }

            if (_staminaManager != null)
            {
                (_staminaManager as CindarsHope.Foundation.IStaminaRuntime)?.Initialize();
            }
            else
            {
                Debug.LogWarning("GameBootstrap is missing a StaminaManager reference.", this);
            }

            if (_manaManager != null)
            {
                (_manaManager as CindarsHope.Foundation.IManaRuntime)?.Initialize();
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
            ConfigureEquipmentDurabilityResolver();
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

            // arch: quebra do par mutuo Core|Player (spec_arch_core_player_cycle_reduction_v37,
            // finalizado 2026-07-15) — StatusEffectManager nao e mais resolvido pelo tipo concreto
            // (.Instance); resolvido via DomainManagerRegistry sob a porta IStatusEffectRuntime.
            var statusEffectRuntime = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IStatusEffectRuntime>();

            if (statusEffectRuntime != null)
            {
                statusEffectRuntime.Initialize();
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

        private void ConfigureEquipmentDurabilityResolver()
        {
            var equipmentRuntime = DomainManagerRegistry.Get<IEquipmentRuntime>();
            equipmentRuntime?.ConfigureWeaponDurabilityResolver(itemInstanceId =>
            {
                if (string.IsNullOrWhiteSpace(itemInstanceId) || _weaponDatabase == null
                    || !(_itemDatabase is IWeaponItemCatalog itemCatalog))
                    return null;
                if (!itemCatalog.TryGetWeaponId(itemInstanceId, out var weaponId))
                {
                    var separator = itemInstanceId.IndexOf('#');
                    var itemId = separator > 0 ? itemInstanceId.Substring(0, separator) : itemInstanceId;
                    if (itemId == itemInstanceId || !itemCatalog.TryGetWeaponId(itemId, out weaponId))
                        return null;
                }
                return _weaponDatabase.TryGetById(weaponId, out var weapon) && weapon != null
                    ? weapon.DurabilityMax
                    : (int?)null;
            });
        }

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

            // arch: quebra do par mutuo Core|Player (2026-07-15) — antes deste corte, o fallback
            // fazia GetComponent<CindarsHope.Player.ManaManager>()/AddComponent<...>() no proprio
            // GameObject, o que exigiria Core nomear o tipo concreto (proibido). Agora procura por
            // qualquer MonoBehaviour no GameObject que implemente a porta IManaRuntime; se nao achar,
            // loga wiring error em vez de auto-criar o componente (auto-criar exigiria conhecer o tipo
            // concreto, que Core nao pode mais nomear).
            // Residual risk: perde o auto-heal por AddComponent; confirmado por grep que as 3 cenas
            // canonicas (Town/Farm/Cave) ja tem _manaManager serializado, entao este fallback e
            // defensivo e nao e exercitado em producao.
            foreach (var behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is CindarsHope.Foundation.IManaRuntime)
                {
                    _manaManager = behaviour;
                    return;
                }
            }

            Debug.LogError("GameBootstrap: ManaManager ausente no GameObject e Core nao pode mais auto-criar o componente (corte do par Core|Player, 2026-07-15). Adicione o componente ManaManager manualmente na cena.", this);
        }

        private void InitializeDeathSystem()
        {
            var equipmentRuntime = DomainManagerRegistry.Get<IEquipmentRuntime>();
            // arch: Core|Inventory (2026-07-15) — resolvido via DomainManagerRegistry sob a porta
            // IInventoryRuntime, sem nomear CindarsHope.Inventory. CorpseRecoveryManager agora recebe
            // IInventoryRuntime no construtor (nao mais o tipo concreto InventoryManager).
            var inventoryManager = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IInventoryRuntime>();
            // arch: quebra do par mutuo Core|Player (spec_arch_core_player_cycle_reduction_v37,
            // finalizado 2026-07-15) — resolvido via DomainManagerRegistry sob a porta IPlayerRuntime.
            // CorpseRecoveryManager (Player.Death) e construido pela fabrica
            // IPlayerRuntime.CreateCorpseRecoveryManager, dentro do modulo Player (mesmo modulo de
            // CorpseRecoveryManager), em vez de 'new CindarsHope.Player.Death.CorpseRecoveryManager(...)'
            // aqui — o que exigiria Core nomear o tipo concreto.
            var playerRuntime = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IPlayerRuntime>();
            if (playerRuntime != null && inventoryManager != null && equipmentRuntime != null)
            {
                _corpseRecoveryManager = playerRuntime.CreateCorpseRecoveryManager(inventoryManager, equipmentRuntime);
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

            // arch: quebra do par mutuo Core|Player (spec_arch_core_player_cycle_reduction_v37,
            // finalizado 2026-07-15) — StatusEffectManager resolvido via DomainManagerRegistry sob a
            // porta IStatusEffectRuntime, sem nomear CindarsHope.Player.
            var statusEffectRuntime = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IStatusEffectRuntime>();
            if (statusEffectRuntime != null)
            {
                statusEffectRuntime.Shutdown();
            }

            if (_staminaManager != null)
            {
                (_staminaManager as CindarsHope.Foundation.IStaminaRuntime)?.Shutdown();
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

            // arch: quebra do par mutuo Core|Player (spec_arch_core_player_cycle_reduction_v37,
            // finalizado 2026-07-15) — resolvido via DomainManagerRegistry sob a porta IPlayerRuntime.
            var playerRuntimeForShutdown = CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IPlayerRuntime>();
            if (playerRuntimeForShutdown != null)
            {
                playerRuntimeForShutdown.Shutdown();
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
