using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Economy;
using CindarsHope.Enemy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.NPC;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using CindarsHope.Save.Migrations;
using CindarsHope.Save.Providers;
using CindarsHope.Skills;
using CindarsHope.World;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace CindarsHope.Save
{
    /// <summary>
    /// Coordenador central de save/load. Delega capture e restore de cada domÃ­nio a um
    /// <see cref="ISaveSectionProvider"/> registrado, mantendo-se como orquestrador fino
    /// sem lÃ³gica de domÃ­nio interna. O formato serializado do <see cref="GameSaveData"/> Ã©
    /// preservado byte-a-byte â€” a refatoraÃ§Ã£o apenas realoca o cÃ³digo de capture/restore para
    /// os providers sem alterar DTOs, nomes de campo ou ordem de serializaÃ§Ã£o.
    /// </summary>
    [DisallowMultipleComponent]
    public partial class SaveManager : MonoBehaviour
    {
        private static readonly ProfilerMarker SaveMarker =
            new ProfilerMarker("CindarsHope.Save.CaptureSerializeWrite");
        private static readonly ProfilerMarker RestoreMarker =
            new ProfilerMarker("CindarsHope.Save.Restore");

        private const int CurrentSchemaVersion = 5;
        private const int Slot = 1;
        private const string SaveDirectoryName = "saves";
        private const string SaveFileName = "slot_1.json";

        // â”€â”€ SerializeField refs mantidas para compatibilidade com cenas existentes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Nota: os [SerializeField] abaixo sÃ£o necessÃ¡rios porque as cenas referenciam estes campos
        // por GUID. NÃ£o remova nem renomeie sem atualizar as cenas correspondentes (permission-gated).
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private ManaManager _manaManager;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private Core.GameTimeManager _gameTimeManager;
        [SerializeField] private Player.StatusEffectManager _statusEffectManager;
        [SerializeField] private FarmPlotRegistry _farmPlotRegistry;
        [SerializeField] private TreeRegistry _treeRegistry;
        [SerializeField] private ItemPickupRegistry _itemPickupRegistry;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private PlayerProgressionManager _progressionManager;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private CraftingRuntime _craftingRuntime;
        [SerializeField] private NpcManager _npcManager;
        [SerializeField] private Skills.ActiveSkillSlots _activeSkillSlots;
        [SerializeField] private Skills.SkillTreeManager _skillTreeManager;
        [SerializeField] private BestiaryManager _bestiaryManager;
        [SerializeField] private CorpseRecoveryManager _corpseRecoveryManager;
        [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private ItemDatabaseSO _itemDatabase;

        // â”€â”€ Estado interno â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private readonly HotbarState _hotbarState = new HotbarState();
        private readonly SaveMigrationRegistry _migrationRegistry = new SaveMigrationRegistry(new ISaveMigration[]
        {
            new InventorySlotsV1ToV2Migration(),
            new SaveV2ToV3Migration(),
            new SaveV3ToV4Migration(),
            new SaveV4ToV5Migration()
        });

        // â”€â”€ Providers registrados (inicializados em Initialize) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private ISaveSectionProvider _playerProvider;
        private ISaveSectionProvider _inventoryProvider;
        private ISaveSectionProvider _equipmentProvider;
        private ISaveSectionProvider _progressionProvider;
        private ISaveSectionProvider _staminaProvider;
        private ISaveSectionProvider _gameTimeProvider;
        private ISaveSectionProvider _playerStatusEffectsProvider;
        private ISaveSectionProvider _equipmentDurabilityProvider;
        private ISaveSectionProvider _bestiaryProvider;
        private ISaveSectionProvider _activeSkillSlotsProvider;
        private ISaveSectionProvider _skillTreeProvider;
        private ISaveSectionProvider _craftingProvider;
        private ISaveSectionProvider _economyProvider;
        private ISaveSectionProvider _npcsProvider;
        private ISaveSectionProvider _caveProvider;
        private ISaveSectionProvider _caveRunProvider;
        private ISaveSectionProvider _deathProvider;
        private ISaveSectionProvider _farmProvider;
        private ISaveSectionProvider _worldProvider;
        private ISaveSectionProvider _fonteProvider;
        private ISaveSectionProvider _mainProgressionProvider;
        private ISaveSectionProvider _dailyGoalsProvider;
        private ISaveSectionProvider _friendshipProvider;
        private ISaveSectionProvider _farmAnimalsProvider;
        private ISaveSectionProvider _npcServicesProvider;
        private ISaveSectionProvider _farmLotsProvider;
        private ISaveSectionProvider _questProvider;
        // Providers jÃ¡ existentes (SPEC_10 / fable_07 / fable_62)
        private ISaveSectionProvider _hotbarProvider;
        private ISaveSectionProvider _spellbookProvider;
        private ISaveSectionProvider _onboardingHintsProvider;
        // spec_farm_till_anywhere_tilemap: tiles araveis por coordenada (campo aditivo FarmTiles).
        private ISaveSectionProvider _farmTilesProvider;
        // Grid de tiles araveis — criado aqui e compartilhado com FarmTilledSoilService.
        private readonly Farm.FarmTileGrid _farmTileGrid = new Farm.FarmTileGrid(new Farm.FarmNonArableZones());
        private readonly SaveProviderRegistry _providerRegistry = new SaveProviderRegistry();

        /// <summary>Caminho completo do arquivo de save em disco.</summary>
        public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveDirectoryName, SaveFileName);

        /// <summary>Estado do hotbar compartilhado com a HUD; persistido via HotbarSectionProvider.</summary>
        public HotbarState HotbarState => _hotbarState;

        /// <summary>
        /// Grid de tiles araveis da fazenda. Compartilhado com FarmTilledSoilService.
        /// O FarmTilesRuntimeBootstrap deve registrar as zonas nao-araveis aqui apos carregar a cena.
        /// </summary>
        public Farm.FarmTileGrid FarmTileGrid => _farmTileGrid;

        /// <summary>Indica se o SaveManager foi inicializado e estÃ¡ pronto para salvar/carregar.</summary>
        public bool IsInitialized { get; private set; }
        public int RegisteredProviderCount => _providerRegistry.Count;

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Ciclo de vida
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Inicializa todos os providers de seÃ§Ã£o com as dependÃªncias injetadas via [SerializeField].
        /// Deve ser chamado pelo GameBootstrap antes do primeiro SaveGame/LoadGame.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;

            // Providers jÃ¡ existentes (SPEC_10 / fable_07 / fable_62)
            _hotbarProvider = new HotbarSectionProvider(_hotbarState);
            _spellbookProvider = new SpellbookSectionProvider();
            // arch: Save|UI (spec_arch_save_ui_cycle_reduction_v26) — nome totalmente qualificado
            // (provider mora em CindarsHope.UI.Onboarding.Save), sem novo using de topo.
            _onboardingHintsProvider = new CindarsHope.UI.Onboarding.Save.OnboardingHintsSectionProvider();

            // Hotbar: inicializa defaults se novo jogo
            if (string.IsNullOrWhiteSpace(_hotbarState.GetSlotItemId(0)))
            {
                _hotbarState.SetSlot(0, "item_seed_wheat");
                _hotbarState.SetSlot(1, "item_seed_carrot");
                _hotbarState.SetSlot(2, "item_tool_fishing_rod_basic");
                _hotbarState.SetSlot(3, "item_weapon_bow_basic");
                _hotbarState.SetSlot(4, "item_ammo_arrow_basic");
                _hotbarState.SetSlot(5, "item_spell_fireball_test");
            }

            // â”€â”€ Lote 1: singleton puro â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _fonteProvider = new FonteSectionProvider();
            _mainProgressionProvider = new MainProgressionSectionProvider();
            _dailyGoalsProvider = new DailyGoalsSectionProvider();
            _friendshipProvider = new FriendshipSectionProvider();
            _farmAnimalsProvider = new FarmAnimalsSectionProvider();
            _npcServicesProvider = new NpcServicesSectionProvider();
            _farmLotsProvider = new FarmLotsSectionProvider();
            // arch: quebra do ciclo Quests|Save (spec_arch_quests_save_cycle_reduction_v24) — nome
            // totalmente qualificado para não poluir o topo do arquivo com um `using
            // CindarsHope.Quests` referenciado uma única vez (mesmo precedente de
            // spec_arch_npc_save_cycle_reduction_v23 / spec_arch_economy_save_cycle_reduction_v22).
            _questProvider = new CindarsHope.Quests.Save.QuestSectionProvider();
            _caveRunProvider = new CaveRunSectionProvider();

            // â”€â”€ Lote 2: manager injetado â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _playerProvider = new PlayerSectionProvider(
                _playerManager,
                _hungerManager,
                _manaManager,
                () => _playerTransform != null ? (Vector2)_playerTransform.position : Vector2.zero);

            _inventoryProvider = new InventorySectionProvider(
                _inventoryManager,
                _playerData,
                _itemDatabase,
                _hotbarState);

            _equipmentProvider = new EquipmentSectionProvider(_equipmentManager);
            _progressionProvider = new ProgressionSectionProvider(_progressionManager);
            _staminaProvider = new StaminaSectionProvider(_staminaManager);
            _gameTimeProvider = new GameTimeSectionProvider(_gameTimeManager, _timeManager);
            _playerStatusEffectsProvider = new PlayerStatusEffectsSectionProvider(_statusEffectManager);
            _equipmentDurabilityProvider = new EquipmentDurabilitySectionProvider(_equipmentManager);
            _bestiaryProvider = new BestiarySectionProvider(_bestiaryManager);
            _activeSkillSlotsProvider = new ActiveSkillSlotsSectionProvider(_activeSkillSlots);

            _skillTreeProvider = new SkillTreeSectionProvider(
                _skillTreeManager,
                () =>
                {
                    // NÃ­vel vem da seÃ§Ã£o de progressÃ£o jÃ¡ restaurada (ProgressionSectionProvider).
                    return _progressionManager != null
                        ? (_progressionManager.CaptureSaveData()?.Level ?? 1)
                        : 1;
                });

            _craftingProvider = new CraftingSectionProvider(_craftingRuntime);
            _economyProvider = new EconomySectionProvider(_shopManager);
            _npcsProvider = new NpcsSectionProvider(_npcManager);
            _caveProvider = new CaveSectionProvider(_caveRunManager);
            _deathProvider = new DeathSectionProvider(_corpseRecoveryManager);

            // â”€â”€ Lote 3: lÃ³gica condicional de cena â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _farmProvider = new FarmSectionProvider(_farmPlotRegistry);
            _worldProvider = new WorldSectionProvider(_itemPickupRegistry, _treeRegistry);

            // spec_farm_till_anywhere_tilemap: tiles araveis por coordenada (secao aditiva FarmTiles).
            _farmTilesProvider = new Providers.FarmTilesSectionProvider(_farmTileGrid);
            RegisterProviderDescriptors();
        }

        /// <summary>Desliga o SaveManager. Chamado pelo GameBootstrap no shutdown.</summary>
        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Save / Load
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Serializa o estado atual do jogo para o slot 1. Recusa o save durante boss fight ativa
        /// na caverna (fable_44). Retorna true se o save foi escrito com sucesso.
        /// </summary>
        public bool SaveGame()
        {
            using var profilerScope = SaveMarker.Auto();

            // fable_44: polÃ­tica de save em boss fight (CA-3).
            if (TryGetActiveCaveBossFightGuard(out var blockedReason))
            {
                Debug.Log($"SaveManager: manual save blocked â€” {blockedReason}", this);
                PublishSaveResult(false, blockedReason);
                return false;
            }

            try
            {
                var existingSaveData = TryReadExistingValidSave();
                var activeScene = SceneManager.GetActiveScene();

                // Captura de cada seÃ§Ã£o via provider
                var playerData = _playerProvider?.Capture(existingSaveData) as PlayerSaveData;

                var saveData = new GameSaveData
                {
                    SchemaVersion = CurrentSchemaVersion,
                    CurrentDay = CaptureCurrentDay(),
                    CurrentSceneName = activeScene.name,
                    CurrentScenePath = activeScene.path,
                    Player = playerData,
                    Inventory = _inventoryProvider?.Capture(existingSaveData) as InventorySaveData,
                    Equipment = _equipmentProvider?.Capture(existingSaveData) as EquipmentSaveData,
                    Hotbar = _providerRegistry.Capture<HotbarSaveData>("hotbar", existingSaveData),
                    Progression = _progressionProvider?.Capture(existingSaveData) as PlayerProgressionSaveData,
                    Farm = _farmProvider?.Capture(existingSaveData) as FarmSaveData,
                    World = _worldProvider?.Capture(existingSaveData) as WorldSaveData,
                    Cave = _caveProvider?.Capture(existingSaveData) as CaveSaveData,
                    Death = _deathProvider?.Capture(existingSaveData) as DeathSaveData,
                    Economy = _economyProvider?.Capture(existingSaveData) as EconomySaveData,
                    Crafting = _craftingProvider?.Capture(existingSaveData) as CraftingRuntimeSaveData,
                    Stamina = _staminaProvider?.Capture(existingSaveData) as StaminaSaveData,
                    GameTime = _gameTimeProvider?.Capture(existingSaveData) as GameTimeSaveData,
                    PlayerStatusEffects = _playerStatusEffectsProvider?.Capture(existingSaveData) as PlayerStatusEffectsSaveData,
                    EquipmentDurability = _equipmentDurabilityProvider?.Capture(existingSaveData) as EquipmentDurabilitySaveData,
                    Npcs = _npcsProvider?.Capture(existingSaveData) as NpcManagerSaveData,
                    ActiveSkillSlots = _activeSkillSlotsProvider?.Capture(existingSaveData) as ActiveSkillSlotsSaveData,
                    SkillTree = _skillTreeProvider?.Capture(existingSaveData) as Skills.SkillTreeSaveData,
                    Bestiary = _bestiaryProvider?.Capture(existingSaveData) as BestiarySaveData,
                    Quests = _questProvider?.Capture(existingSaveData) as QuestStateSectionSaveData,
                    Fonte = _fonteProvider?.Capture(existingSaveData) as FonteSaveData,
                    CaveRun = _caveRunProvider?.Capture(existingSaveData) as CaveRunSaveData,
                    DailyGoals = _dailyGoalsProvider?.Capture(existingSaveData) as Farm.Runtime.FarmDailyGoalsSaveData,
                    Spellbook = _spellbookProvider?.Capture(existingSaveData) as Magic.SpellbookSaveData,
                    FarmLots = _farmLotsProvider?.Capture(existingSaveData) as Farm.Lots.FarmLotsSaveData,
                    FarmAnimals = _farmAnimalsProvider?.Capture(existingSaveData) as Farm.Animals.FarmAnimalsSaveData,
                    Friendship = _friendshipProvider?.Capture(existingSaveData) as NPC.Friendship.FriendshipSaveData,
                    NpcServices = _npcServicesProvider?.Capture(existingSaveData) as NPC.Services.NpcServicesSaveData,
                    MainProgression = _mainProgressionProvider?.Capture(existingSaveData) as MainProgressionSaveData,
                    OnboardingHints = _onboardingHintsProvider?.Capture(existingSaveData) as OnboardingHintsSaveData,
                    FarmTiles = _farmTilesProvider?.Capture(existingSaveData) as FarmTilesSaveData
                };

                var savePath = SaveFilePath;
                var directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonUtility.ToJson(saveData, true);
                WriteTextSafely(savePath, json);

                Debug.Log($"Game saved to {savePath}.", this);
                PublishSaveResult(true, "Save complete.");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                PublishSaveResult(false, exception.Message);
                return false;
            }
        }

        /// <summary>
        /// Carrega o save do slot 1 e aplica o estado ao jogo. Retorna false se o arquivo nÃ£o
        /// existir ou estiver corrompido.
        /// </summary>
        public bool LoadGame()
        {
            var savePath = SaveFilePath;
            TryRecoverFromBackupIfNeeded(savePath);

            if (!File.Exists(savePath))
            {
                Debug.Log($"Save file not found at {savePath}.", this);
                return false;
            }

            try
            {
                if (!TryReadSaveWithMigration(savePath, true, out var saveData, out var migrationResult))
                {
                    Debug.LogWarning($"Save file at {savePath} could not be loaded. {migrationResult.ErrorMessage}", this);
                    return false;
                }

                var activeScene = SceneManager.GetActiveScene();
                if (!string.IsNullOrEmpty(saveData.CurrentSceneName) && saveData.CurrentSceneName != activeScene.name)
                {
                    StartCoroutine(LoadSceneAndApplySaveData(saveData));
                    return true;
                }

                ApplySaveData(saveData);
                Debug.Log($"Game loaded from {savePath}.", this);
                GameEventBus.Publish(new GameLoadedEvent(Slot, savePath, true, "Jogo carregado."));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                GameEventBus.Publish(new GameLoadedEvent(Slot, SaveFilePath, false, "Falha ao carregar save."));
                return false;
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Rebind pÃºblico (chamado por RuntimeBootstraps de cena)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Rebinda refs de cena para registries de plots, Ã¡rvores, pickups e transform do player.
        /// Chamado por sceneRuntimeBootstraps apÃ³s o load de cena.
        /// </summary>
        public void RebindSceneReferences(FarmPlotRegistry farmPlotRegistry, TreeRegistry treeRegistry, ItemPickupRegistry itemPickupRegistry, Transform playerTransform)
        {
            if (farmPlotRegistry != null)
            {
                _farmPlotRegistry = farmPlotRegistry;
            }

            if (treeRegistry != null)
            {
                _treeRegistry = treeRegistry;
            }

            if (itemPickupRegistry != null)
            {
                _itemPickupRegistry = itemPickupRegistry;
            }

            if (playerTransform != null)
            {
                _playerTransform = playerTransform;
            }

            // Rebinda providers que dependem dessas refs de cena
            _farmProvider = new FarmSectionProvider(_farmPlotRegistry);
            _worldProvider = new WorldSectionProvider(_itemPickupRegistry, _treeRegistry);
        }

        /// <summary>Rebinda managers obrigatÃ³rios de runtime (player, inventory, hunger, time).</summary>
        public void RebindRuntimeManagers(PlayerManager playerManager, InventoryManager inventoryManager, HungerManager hungerManager, TimeManager timeManager)
        {
            if (playerManager != null)
            {
                _playerManager = playerManager;
            }
            else
            {
                Debug.LogWarning("SaveManager.RebindRuntimeManagers received null PlayerManager.", this);
            }

            if (inventoryManager != null)
            {
                _inventoryManager = inventoryManager;
            }
            else
            {
                Debug.LogWarning("SaveManager.RebindRuntimeManagers received null InventoryManager.", this);
            }

            if (hungerManager != null)
            {
                _hungerManager = hungerManager;
            }
            else
            {
                Debug.LogWarning("SaveManager.RebindRuntimeManagers received null HungerManager.", this);
            }

            if (timeManager != null)
            {
                _timeManager = timeManager;
            }
            else
            {
                Debug.LogWarning("SaveManager.RebindRuntimeManagers received null TimeManager.", this);
            }

            // Rebinda providers afetados
            _playerProvider = new PlayerSectionProvider(
                _playerManager,
                _hungerManager,
                _manaManager,
                () => _playerTransform != null ? (Vector2)_playerTransform.position : Vector2.zero);
            _inventoryProvider = new InventorySectionProvider(_inventoryManager, _playerData, _itemDatabase, _hotbarState);
            _gameTimeProvider = new GameTimeSectionProvider(_gameTimeManager, _timeManager);
        }

        /// <summary>Rebinda managers opcionais de runtime.</summary>
        public void RebindOptionalRuntimeManagers(
            EquipmentManager equipmentManager,
            PlayerProgressionManager progressionManager,
            Core.GameTimeManager gameTimeManager = null,
            StaminaManager staminaManager = null,
            Player.StatusEffectManager statusEffectManager = null,
            Skills.SkillTreeManager skillTreeManager = null,
            ShopManager shopManager = null,
            BestiaryManager bestiaryManager = null)
        {
            if (equipmentManager != null)
            {
                _equipmentManager = equipmentManager;
            }

            if (progressionManager != null)
            {
                _progressionManager = progressionManager;
            }

            if (gameTimeManager != null)
            {
                _gameTimeManager = gameTimeManager;
            }

            if (staminaManager != null)
            {
                _staminaManager = staminaManager;
            }

            if (statusEffectManager != null)
            {
                _statusEffectManager = statusEffectManager;
            }

            if (skillTreeManager != null)
            {
                _skillTreeManager = skillTreeManager;
            }

            if (shopManager != null)
            {
                _shopManager = shopManager;
            }

            // arch: Core|Enemy (spec_arch_core_enemy_cycle_reduction_v31) — GameBootstrap nao segura
            // mais essa ref; resolve via BestiaryManager.Instance (self-registro) quando o chamador
            // (ex.: geradores de cena legados) nao passar uma explicita.
            _bestiaryManager = bestiaryManager != null ? bestiaryManager : BestiaryManager.Instance;

            // Rebinda providers afetados
            _equipmentProvider = new EquipmentSectionProvider(_equipmentManager);
            _equipmentDurabilityProvider = new EquipmentDurabilitySectionProvider(_equipmentManager);
            _progressionProvider = new ProgressionSectionProvider(_progressionManager);
            _staminaProvider = new StaminaSectionProvider(_staminaManager);
            _gameTimeProvider = new GameTimeSectionProvider(_gameTimeManager, _timeManager);
            _playerStatusEffectsProvider = new PlayerStatusEffectsSectionProvider(_statusEffectManager);
            _skillTreeProvider = new SkillTreeSectionProvider(
                _skillTreeManager,
                () => _progressionManager != null ? (_progressionManager.CaptureSaveData()?.Level ?? 1) : 1);
            _economyProvider = new EconomySectionProvider(_shopManager);
            _bestiaryProvider = new BestiarySectionProvider(_bestiaryManager);
        }

        /// <summary>Rebinda os ScriptableObjects de dados para reparo de inventÃ¡rio starter.</summary>
        public void RebindStarterInventoryData(PlayerDataSO playerData, ItemDatabaseSO itemDatabase)
        {
            if (playerData != null)
            {
                _playerData = playerData;
            }

            if (itemDatabase != null)
            {
                _itemDatabase = itemDatabase;
            }

            _inventoryProvider = new InventorySectionProvider(_inventoryManager, _playerData, _itemDatabase, _hotbarState);
        }

        /// <summary>Rebinda o CaveRunManager apÃ³s entrar na cena de caverna.</summary>
        public void RebindCaveRuntime(CaveRunManager caveRunManager)
        {
            if (caveRunManager != null)
            {
                _caveRunManager = caveRunManager;
            }

            _caveProvider = new CaveSectionProvider(_caveRunManager);
        }

        /// <summary>Rebinda o Transform do player (chamado apÃ³s respawn ou mudanÃ§a de cena).</summary>
        public void RebindPlayerTransform(Transform playerTransform)
        {
            if (playerTransform != null)
            {
                _playerTransform = playerTransform;
            }
            else
            {
                Debug.LogWarning("SaveManager.RebindPlayerTransform received null Transform.", this);
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Internos
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private int CaptureCurrentDay()
        {
            if (_timeManager != null)
            {
                return _timeManager.CurrentDay;
            }

            Debug.LogWarning("SaveManager saved without TimeManager. CurrentDay fallback is 1.", this);
            return 1;
        }

        private void RegisterProviderDescriptors()
        {
            int order = 0;
            _providerRegistry.Register<PlayerSaveData>(_playerProvider, order++);
            _providerRegistry.Register<InventorySaveData>(_inventoryProvider, order++);
            _providerRegistry.Register<HotbarSaveData>(_hotbarProvider, order++);
            _providerRegistry.Register<CindarsHope.Magic.SpellbookSaveData>(_spellbookProvider, order++);
            _providerRegistry.Register<EquipmentSaveData>(_equipmentProvider, order++);
            _providerRegistry.Register<EquipmentDurabilitySaveData>(_equipmentDurabilityProvider, order++);
            _providerRegistry.Register<PlayerProgressionSaveData>(_progressionProvider, order++);
            _providerRegistry.Register<ActiveSkillSlotsSaveData>(_activeSkillSlotsProvider, order++);
            _providerRegistry.Register<SkillTreeSaveData>(_skillTreeProvider, order++);
            _providerRegistry.Register<StaminaSaveData>(_staminaProvider, order++);
            _providerRegistry.Register<GameTimeSaveData>(_gameTimeProvider, order++);
            _providerRegistry.Register<PlayerStatusEffectsSaveData>(_playerStatusEffectsProvider, order++);
            _providerRegistry.Register<CaveSaveData>(_caveProvider, order++);
            _providerRegistry.Register<CindarsHope.Cave.Runtime.CaveRunSaveData>(_caveRunProvider, order++);
            _providerRegistry.Register<WorldSaveData>(_worldProvider, order++);
            _providerRegistry.Register<FarmSaveData>(_farmProvider, order++);
            _providerRegistry.Register<EconomySaveData>(_economyProvider, order++);
            _providerRegistry.Register<NpcManagerSaveData>(_npcsProvider, order++);
            _providerRegistry.Register<CraftingRuntimeSaveData>(_craftingProvider, order++);
            _providerRegistry.Register<BestiarySaveData>(_bestiaryProvider, order++);
            _providerRegistry.Register<FonteSaveData>(_fonteProvider, order++);
            _providerRegistry.Register<MainProgressionSaveData>(_mainProgressionProvider, order++);
            _providerRegistry.Register<Farm.Runtime.FarmDailyGoalsSaveData>(_dailyGoalsProvider, order++);
            _providerRegistry.Register<Farm.Lots.FarmLotsSaveData>(_farmLotsProvider, order++);
            _providerRegistry.Register<Farm.Animals.FarmAnimalsSaveData>(_farmAnimalsProvider, order++);
            _providerRegistry.Register<NPC.Friendship.FriendshipSaveData>(_friendshipProvider, order++);
            _providerRegistry.Register<NPC.Services.NpcServicesSaveData>(_npcServicesProvider, order++);
            _providerRegistry.Register<DeathSaveData>(_deathProvider, order++);
            _providerRegistry.Register<QuestStateSectionSaveData>(_questProvider, order++);
            _providerRegistry.Register<OnboardingHintsSaveData>(_onboardingHintsProvider, order++);
            _providerRegistry.Register<FarmTilesSaveData>(_farmTilesProvider, order);
        }

        // fable_44: lÃª o flag de boss fight do CaveLevelRuntimeController pelo canal do bootstrap.
        private bool TryGetActiveCaveBossFightGuard(out string reason)
        {
            reason = CaveBossFightSaveGate.SaveBlockedReason;

            var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
            var runManager = bootstrap != null ? bootstrap.CaveRunManager : null;
            if (runManager == null)
            {
                return false;
            }

            var levelController = runManager.GetComponent<Cave.CaveLevelRuntimeController>();
            return levelController != null && levelController.IsBossFightActive;
        }

        private IEnumerator LoadSceneAndApplySaveData(GameSaveData saveData)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(saveData.CurrentScenePath))
            {
                EditorSceneManager.LoadSceneInPlayMode(saveData.CurrentScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            }
            else
            {
                SceneManager.LoadScene(saveData.CurrentSceneName);
            }
#else
            SceneManager.LoadScene(saveData.CurrentSceneName);
#endif
            yield return null;
            yield return null;

            ApplySaveData(saveData);
            GameEventBus.Publish(new GameLoadedEvent(Slot, SaveFilePath, true, "Jogo carregado."));
        }

        /// <summary>
        /// Aplica os dados do save ao estado do jogo, delegando cada seÃ§Ã£o ao provider
        /// correspondente. A ordem respeita as dependÃªncias entre seÃ§Ãµes (ex: progressÃ£o
        /// antes de skill tree; Fonte antes de MainProgression).
        /// </summary>
        private void ApplySaveData(GameSaveData saveData)
        {
            using var profilerScope = RestoreMarker.Auto();

            // â”€â”€ Dia / tempo â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (_timeManager != null)
            {
                _timeManager.SetCurrentDay(saveData.CurrentDay);
            }
            else
            {
                Debug.LogWarning("SaveManager skipped day restore because TimeManager is missing.", this);
            }

            // â”€â”€ Jogador (HP, fome, mana, fadiga) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _playerProvider?.Restore(saveData.Player);

            // PosiÃ§Ã£o do player: mantida no SaveManager porque _playerTransform Ã© uma ref serializada
            // de cena que os providers nÃ£o devem conhecer diretamente.
            if (saveData.Player != null && _playerTransform != null)
            {
                _playerTransform.position = saveData.Player.PlayerPosition;
            }
            else if (_playerTransform == null)
            {
                Debug.LogWarning("SaveManager skipped player position restore because player Transform is missing.", this);
            }

            // â”€â”€ InventÃ¡rio (inclui reparo de starter items e limpeza de hotbar bindings) â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _inventoryProvider?.Restore(saveData.Inventory);

            // â”€â”€ Hotbar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _providerRegistry.Restore("hotbar", saveData.Hotbar);

            // â”€â”€ GrimÃ³rio (apÃ³s inventÃ¡rio â€” contrato de ordem da spec fable_07) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _spellbookProvider?.Restore(saveData.Spellbook);

            // â”€â”€ Equipamento â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _equipmentProvider?.Restore(saveData.Equipment);
            _equipmentDurabilityProvider?.Restore(saveData.EquipmentDurability);

            // â”€â”€ ProgressÃ£o (antes de skill tree que depende do nÃ­vel) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _progressionProvider?.Restore(saveData.Progression);

            // â”€â”€ Skills â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _activeSkillSlotsProvider?.Restore(saveData.ActiveSkillSlots);
            _skillTreeProvider?.Restore(saveData.SkillTree);

            // â”€â”€ Stamina / tempo de jogo / status effects â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _staminaProvider?.Restore(saveData.Stamina);
            _gameTimeProvider?.Restore(saveData.GameTime);
            _playerStatusEffectsProvider?.Restore(saveData.PlayerStatusEffects);

            // â”€â”€ Mana (via PlayerSaveData â€” o PlayerSectionProvider jÃ¡ restaura; nenhum passo extra)

            // â”€â”€ Caverna â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _caveProvider?.Restore(saveData.Cave);
            _caveRunProvider?.Restore(saveData.CaveRun);

            // â”€â”€ Mundo / Farm â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _worldProvider?.Restore(saveData.World);
            _farmProvider?.Restore(saveData.Farm);

            // â”€â”€ Economia / NPCs / Crafting â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (saveData.Economy != null)
            {
                _economyProvider?.Restore(saveData.Economy);
            }

            _npcsProvider?.Restore(saveData.Npcs);
            _craftingProvider?.Restore(saveData.Crafting);

            // â”€â”€ BestiÃ¡rio â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _bestiaryProvider?.Restore(saveData.Bestiary);

            // â”€â”€ Fonte (antes de MainProgression que depende da seÃ§Ã£o recriada) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (saveData.Fonte != null)
            {
                _fonteProvider?.Restore(saveData.Fonte);

                if (saveData.MainProgression != null)
                {
                    _mainProgressionProvider?.Restore(saveData.MainProgression);
                }
            }

            // â”€â”€ DomÃ­nios globais aditivos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _dailyGoalsProvider?.Restore(saveData.DailyGoals);
            _farmLotsProvider?.Restore(saveData.FarmLots);
            _farmAnimalsProvider?.Restore(saveData.FarmAnimals);
            _friendshipProvider?.Restore(saveData.Friendship);
            _npcServicesProvider?.Restore(saveData.NpcServices);

            // â”€â”€ Morte / cadÃ¡ver â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _deathProvider?.Restore(saveData.Death);

            // â”€â”€ Quests â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _questProvider?.Restore(saveData.Quests);

            // â”€â”€ Onboarding hints â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _onboardingHintsProvider?.Restore(saveData.OnboardingHints);

            // â”€â”€ Tiles araveis (farm tile system — spec_farm_till_anywhere_tilemap) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            _farmTilesProvider?.Restore(saveData.FarmTiles);
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Save infra (leitura, migraÃ§Ã£o, escrita segura)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private void PublishSaveResult(bool wasSuccessful, string message)
        {
            GameEventBus.Publish(new GameSavedEvent(Slot, SaveFilePath, wasSuccessful, message));
        }
    }
}
