using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Cave.Runtime;
using CindarsHope.Enemy;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Inventory;
using CindarsHope.NPC;
using CindarsHope.Player;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using CindarsHope.Player.Data;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using CindarsHope.Save.Migrations;
using CindarsHope.Save.Providers;
using CindarsHope.Skills;
using CindarsHope.UI.Hotbar;
using CindarsHope.World;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace CindarsHope.Save
{
    [DisallowMultipleComponent]
    public class SaveManager : MonoBehaviour
    {
        private const int CurrentSchemaVersion = 5;
        private const int Slot = 1;
        private const string SaveDirectoryName = "saves";
        private const string SaveFileName = "slot_1.json";
        private const string FarmSceneName = "FarmScene";
        private const string TownSceneName = "TownScene";

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
        [SerializeField] private Player.Death.CorpseRecoveryManager _corpseRecoveryManager;
        [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private ItemDatabaseSO _itemDatabase;

        private readonly HotbarState _hotbarState = new HotbarState();
        private ISaveSectionProvider _hotbarProvider;
        // fable_07: provider do grimório (padrão HotbarSectionProvider; fonte = PlayerSpellbook.Instance).
        private ISaveSectionProvider _spellbookProvider;
        // fable_62: provider dos hints de onboarding (fonte = OnboardingHintService.Instance).
        private ISaveSectionProvider _onboardingHintsProvider;
        private readonly SaveMigrationRegistry _migrationRegistry = new SaveMigrationRegistry(new ISaveMigration[]
        {
            new InventorySlotsV1ToV2Migration(),
            new SaveV2ToV3Migration(),
            new SaveV3ToV4Migration(),
            new SaveV4ToV5Migration()
        });

        public bool IsInitialized { get; private set; }
        public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveDirectoryName, SaveFileName);
        public HotbarState HotbarState => _hotbarState;

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;

            // SPEC_10: Initialize save providers
            _hotbarProvider = new HotbarSectionProvider(_hotbarState);
            // fable_07: provider do grimório (resolve PlayerSpellbook.Instance no momento de capture/restore).
            _spellbookProvider = new SpellbookSectionProvider();
            // fable_62: provider dos hints de onboarding (resolve OnboardingHintService.Instance ao capturar/restaurar).
            _onboardingHintsProvider = new OnboardingHintsSectionProvider();

            if (string.IsNullOrWhiteSpace(_hotbarState.GetSlotItemId(0)))
            {
                _hotbarState.SetSlot(0, "item_seed_wheat");
                _hotbarState.SetSlot(1, "item_seed_carrot");
                _hotbarState.SetSlot(2, "item_tool_fishing_rod_basic");
                _hotbarState.SetSlot(3, "item_weapon_bow_basic");
                _hotbarState.SetSlot(4, "item_ammo_arrow_basic");
                _hotbarState.SetSlot(5, "item_spell_fireball_test");
            }
        }

        public bool SaveGame()
        {
            // fable_44: política de save em boss fight (CA-3). Ponto ÚNICO de save manual — recusa o save
            // enquanto uma boss fight da caverna está ativa, com feedback no canal existente (GameSavedEvent).
            // Sem UI nova. Fora de boss fight, o save nunca é bloqueado por aqui.
            if (TryGetActiveCaveBossFightGuard(out var blockedReason))
            {
                Debug.Log($"SaveManager: manual save blocked — {blockedReason}", this);
                PublishSaveResult(false, blockedReason);
                return false;
            }

            try
            {
                var existingSaveData = TryReadExistingValidSave();
                var activeScene = SceneManager.GetActiveScene();

                // Capture farm and world only if in FarmScene; otherwise preserve existing data to avoid loss when saving from TownScene.
                var farmSaveData = CaptureFarmSaveData(existingSaveData);
                var worldSaveData = CaptureWorldSaveData(existingSaveData);
                var caveSaveData = CaptureCaveSaveData(existingSaveData);
                var deathSaveData = CaptureDeathSaveData(existingSaveData);
                var economySaveData = CaptureEconomySaveData(existingSaveData);
                var craftingSaveData = CaptureCraftingSaveData();
                var staminaSaveData = CaptureStaminaSaveData();
                var gameTimeSaveData = CaptureGameTimeSaveData();
                var statusEffectsSaveData = CapturePlayerStatusEffectsSaveData();
                var equipmentDurabilitySaveData = CaptureEquipmentDurabilitySaveData();
                var npcSaveData = CaptureNpcSaveData(existingSaveData);
                var activeSkillSlotsSaveData = CaptureActiveSkillSlotsSaveData();
                var skillTreeSaveData = CaptureSkillTreeSaveData();
                var bestiarySaveData = CaptureBestiarySaveData();
                var questSaveData = CaptureQuestSaveData(existingSaveData);

                var playerData = CapturePlayerSaveData();
                if (playerData != null && _manaManager != null)
                {
                    playerData.CurrentMana = _manaManager.CurrentMana;
                    playerData.MaxMana = _manaManager.MaxMana;
                }

                // SPEC_10: Use hotbar provider if available, otherwise fallback to direct _hotbarState
                var hotbarSaveData = _hotbarProvider != null
                    ? (_hotbarProvider.Capture(existingSaveData) as HotbarSaveData)
                    : _hotbarState.CaptureSaveData();

                // fable_07: captura do grimório via provider (fonte PlayerSpellbook.Instance; fallback ao save).
                var spellbookSaveData = _spellbookProvider != null
                    ? (_spellbookProvider.Capture(existingSaveData) as CindarsHope.Magic.SpellbookSaveData)
                    : (existingSaveData?.Spellbook ?? new CindarsHope.Magic.SpellbookSaveData());

                // fable_62: captura dos hints de onboarding vistos via provider (fonte OnboardingHintService.Instance).
                var onboardingHintsSaveData = _onboardingHintsProvider != null
                    ? (_onboardingHintsProvider.Capture(existingSaveData) as OnboardingHintsSaveData)
                    : (existingSaveData?.OnboardingHints ?? new OnboardingHintsSaveData());

                var saveData = new GameSaveData
                {
                    SchemaVersion = CurrentSchemaVersion,
                    CurrentDay = CaptureCurrentDay(),
                    CurrentSceneName = activeScene.name,
                    CurrentScenePath = activeScene.path,
                    Player = playerData,
                    Inventory = CaptureInventorySaveData(),
                    Equipment = CaptureEquipmentSaveData(),
                    Hotbar = hotbarSaveData,
                    Progression = CaptureProgressionSaveData(),
                    Farm = farmSaveData,
                    World = worldSaveData,
                    Cave = caveSaveData,
                    Death = deathSaveData,
                    Economy = economySaveData,
                    Crafting = craftingSaveData,
                    Stamina = staminaSaveData,
                    GameTime = gameTimeSaveData,
                    PlayerStatusEffects = statusEffectsSaveData,
                    EquipmentDurability = equipmentDurabilitySaveData,
                    Npcs = npcSaveData,
                    ActiveSkillSlots = activeSkillSlotsSaveData,
                    SkillTree = skillTreeSaveData,
                    Bestiary = bestiarySaveData,
                    Quests = questSaveData,
                    Fonte = CaptureFonteSaveData(),
                    CaveRun = CaptureCaveRunSaveData(),
                    DailyGoals = CaptureFarmDailyGoalsSaveData(),
                    Spellbook = spellbookSaveData,
                    FarmLots = CaptureFarmLotsSaveData(),
                    FarmAnimals = CaptureFarmAnimalsSaveData(),
                    Friendship = CaptureFriendshipSaveData(),
                    NpcServices = CaptureNpcServicesSaveData(),
                    MainProgression = CaptureMainProgressionSaveData(),
                    OnboardingHints = onboardingHintsSaveData
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

        public bool LoadGame()
        {
            var savePath = SaveFilePath;
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

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
        }

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
        }

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
        }

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

            if (bestiaryManager != null)
            {
                _bestiaryManager = bestiaryManager;
            }
        }

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
        }

        public void RebindCaveRuntime(CaveRunManager caveRunManager)
        {
            if (caveRunManager != null)
            {
                _caveRunManager = caveRunManager;
            }
        }

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

        private int CaptureCurrentDay()
        {
            if (_timeManager != null)
            {
                return _timeManager.CurrentDay;
            }

            Debug.LogWarning("SaveManager saved without TimeManager. CurrentDay fallback is 1.", this);
            return 1;
        }

        private PlayerSaveData CapturePlayerSaveData()
        {
            if (_playerManager == null)
            {
                Debug.LogWarning("SaveManager saved without PlayerManager. Player section was omitted.", this);
                return null;
            }

            var currentHunger = _hungerManager != null ? _hungerManager.CurrentHunger : 0;
            var maxHunger = _hungerManager != null ? _hungerManager.MaxHunger : 1;
            if (_hungerManager == null)
            {
                Debug.LogWarning("SaveManager saved without HungerManager. Hunger values used safe fallbacks.", this);
            }

            var playerPosition = _playerTransform != null ? (Vector2)_playerTransform.position : Vector2.zero;
            if (_playerTransform == null)
            {
                Debug.LogWarning("SaveManager saved without Player Transform. PlayerPosition fallback is zero.", this);
            }

            var playerData = _playerManager.CaptureSaveData(currentHunger, maxHunger, playerPosition);

            // F16: fadiga persistida via serviço runtime (campo aditivo; default 0 em saves legados).
            var conditionService = Player.Conditions.PlayerConditionService.Instance;
            if (playerData != null && conditionService != null)
            {
                playerData.Fatigue = conditionService.CurrentFatigue;
            }

            return playerData;
        }

        // F17: seção da Fonte de Anya via serviço runtime (null-safe; default = Fonte dormante).
        private FonteSaveData CaptureFonteSaveData()
        {
            var fonte = Fonte.FonteRuntimeService.Instance;
            if (fonte == null)
            {
                return new FonteSaveData();
            }

            var data = new FonteSaveData
            {
                FonteState = (int)fonte.Section.FonteState,
                LivingWaterUnlocked = fonte.Section.LivingWater.Unlocked,
                LivingWaterCharges = fonte.Section.LivingWater.CurrentCharges,
                LastGrantDay = fonte.LastGrantDay
            };

            foreach (var fn in fonte.Section.UnlockedFunctions)
            {
                data.UnlockedFunctions.Add((int)fn);
            }

            foreach (var fragment in fonte.Progression.FragmentStates)
            {
                if (fragment.IsIntegrated())
                {
                    data.IntegratedFragments.Add((int)fragment.FragmentType);
                }
            }

            return data;
        }

        // fable_43: estado do endgame (Ato 5) da MainProgressionSection viva (host = FonteRuntimeService).
        // Null-safe; sem o host => endgame nao iniciado (defaults). So tipos simples (enums por valor,
        // ending por id) — sem refs Unity (ADR-0006). Os fragmentos integrados continuam em FonteSaveData.
        private MainProgressionSaveData CaptureMainProgressionSaveData()
        {
            var fonte = Fonte.FonteRuntimeService.Instance;
            if (fonte == null || fonte.Progression == null)
            {
                return new MainProgressionSaveData();
            }

            var prog = fonte.Progression;
            return new MainProgressionSaveData
            {
                CurrentAct = (int)prog.CurrentAct,
                Level100GateState = (int)prog.Level100GateState,
                Level101AccessState = (int)prog.Level101AccessState,
                FinalChoiceState = (int)prog.FinalChoiceState,
                PostGameWorldState = prog.PostGameWorldState
            };
        }

        // fable_44: lê o flag de boss fight do CaveLevelRuntimeController pelo MESMO canal do bootstrap
        // usado em CaptureCaveRunSaveData (sem GameObject.Find). Retorna true (com a razão) se o save
        // manual deve ser recusado. Fora da caverna / sem boss fight ativa → false.
        private bool TryGetActiveCaveBossFightGuard(out string reason)
        {
            reason = Cave.Runtime.CaveBossFightSaveGate.SaveBlockedReason;

            var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
            var runManager = bootstrap != null ? bootstrap.CaveRunManager : null;
            if (runManager == null)
            {
                return false;
            }

            var levelController = runManager.GetComponent<Cave.CaveLevelRuntimeController>();
            return levelController != null && levelController.IsBossFightActive;
        }

        // F13: run da caverna sobrevive a fechar o jogo (CAVE_RUN_SAVE_LOAD_DEBT).
        // Fonte do estado: CaveRunManager vivo (cena de caverna) ou cache do bootstrap.
        private Cave.Runtime.CaveRunSaveData CaptureCaveRunSaveData()
        {
            var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return new Cave.Runtime.CaveRunSaveData { HasActiveRun = false };
            }

            var runManager = bootstrap.CaveRunManager;
            if (runManager != null)
            {
                // Regrava HP dos inimigos do nível corrente antes de capturar.
                var levelController = runManager.GetComponent<Cave.CaveLevelRuntimeController>();
                if (levelController != null)
                {
                    levelController.RefreshCurrentSnapshotEnemyHp();
                    levelController.RefreshCurrentSnapshotOpenedChests(); // fable_09
                    levelController.RefreshCurrentSnapshotTrapStates();   // fable_60
                }

                return Cave.Runtime.CaveRunSaveMapper.ToSaveData(runManager.State);
            }

            return Cave.Runtime.CaveRunSaveMapper.ToSaveData(bootstrap.CachedCaveRunState);
        }

        // F13: wiring do DTO que a WAVE 24 criou sem ligação (SAVE_LOAD_DAILY_GOAL_DEBT).
        private Farm.Runtime.FarmDailyGoalsSaveData CaptureFarmDailyGoalsSaveData()
        {
            var service = Farm.Runtime.FarmDailyGoalService.Instance;
            return service != null ? service.CaptureSaveData() : new Farm.Runtime.FarmDailyGoalsSaveData();
        }

        // fable_26: amizade por NPC (seção aditiva, domínio NPC via serviço singleton).
        private NPC.Friendship.FriendshipSaveData CaptureFriendshipSaveData()
        {
            var service = NPC.Friendship.FriendshipService.Instance;
            return service != null ? service.CaptureSaveData() : new NPC.Friendship.FriendshipSaveData();
        }

        // fable_12: animais de fazenda (seção aditiva, domínio global via registry singleton).
        private Farm.Animals.FarmAnimalsSaveData CaptureFarmAnimalsSaveData()
        {
            var registry = Farm.Animals.FarmAnimalRegistry.Instance;
            return registry != null ? registry.CaptureSaveData() : new Farm.Animals.FarmAnimalsSaveData();
        }

        // fable_25: pendências dos serviços de NPC (seção aditiva, domínio global via bridge singleton).
        private NPC.Services.NpcServicesSaveData CaptureNpcServicesSaveData()
        {
            var runtime = NPC.Services.NpcServiceRuntime.Instance;
            return runtime != null ? runtime.CaptureSaveData() : new NPC.Services.NpcServicesSaveData();
        }

        // fable_41: posse dos lotes de expansão (campo aditivo na seção farm). Independe da cena
        // ativa (lotes são domínio global), então captura incondicional pelo serviço singleton.
        private Farm.Lots.FarmLotsSaveData CaptureFarmLotsSaveData()
        {
            var service = Farm.Lots.FarmLotService.Instance;
            return service != null ? service.CaptureSaveData() : new Farm.Lots.FarmLotsSaveData();
        }

        private InventorySaveData CaptureInventorySaveData()
        {
            if (_inventoryManager != null)
            {
                return _inventoryManager.CaptureSaveData();
            }

            Debug.LogWarning("SaveManager saved without InventoryManager. Inventory section is empty.", this);
            return new InventorySaveData();
        }

        private EquipmentSaveData CaptureEquipmentSaveData()
        {
            return _equipmentManager != null ? _equipmentManager.CaptureSaveData() : new EquipmentSaveData();
        }

        private PlayerProgressionSaveData CaptureProgressionSaveData()
        {
            return _progressionManager != null ? _progressionManager.CaptureSaveData() : new PlayerProgressionSaveData();
        }

        private CaveSaveData CaptureCaveSaveData(GameSaveData existingSaveData)
        {
            if (_caveRunManager != null)
            {
                return _caveRunManager.CaptureSaveData();
            }

            return existingSaveData?.Cave ?? new CaveSaveData();
        }

        private WorldSaveData CaptureWorldSaveData()
        {
            var worldSaveData = new WorldSaveData();

            if (_itemPickupRegistry != null)
            {
                worldSaveData.Pickups = _itemPickupRegistry.CaptureSaveData();
            }
            else
            {
                Debug.LogWarning("SaveManager saved without ItemPickupRegistry. Pickups were omitted.", this);
            }

            if (_treeRegistry != null)
            {
                worldSaveData.Trees = _treeRegistry.CaptureSaveData();
            }
            else
            {
                Debug.LogWarning("SaveManager saved without TreeRegistry. Trees were omitted.", this);
            }

            return worldSaveData;
        }

        private static List<TreeSaveData> GetSavedTrees(GameSaveData saveData)
        {
            if (saveData.World != null && saveData.World.Trees != null)
            {
                return saveData.World.Trees;
            }

            return saveData.Farm != null && saveData.Farm.Trees != null
                ? saveData.Farm.Trees
                : new List<TreeSaveData>();
        }

        private GameSaveData TryReadExistingValidSave()
        {
            var savePath = SaveFilePath;
            if (!File.Exists(savePath))
            {
                return null;
            }

            try
            {
                return TryReadSaveWithMigration(savePath, true, out var saveData, out _)
                    ? saveData
                    : null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Error reading existing save: {exception.Message}", this);
                return null;
            }
        }

        private bool TryReadSaveWithMigration(string savePath, bool allowWriteBack, out GameSaveData saveData, out SaveMigrationResult result)
        {
            saveData = null;
            result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, "Unknown save migration failure.");

            if (string.IsNullOrWhiteSpace(savePath) || !File.Exists(savePath))
            {
                result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, $"Save file not found at {savePath}.");
                return false;
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(savePath);
            }
            catch (Exception exception)
            {
                result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, $"Could not read save file: {exception.Message}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, "Save file is empty.");
                return false;
            }

            if (!TryDeserializeSave(rawJson, out var parsedSaveData, out var parseError))
            {
                result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, parseError);
                return false;
            }

            var sourceVersion = DetectSourceSchemaVersion(parsedSaveData);
            if (sourceVersion > CurrentSchemaVersion)
            {
                result = SaveMigrationResult.Failed(
                    sourceVersion,
                    CurrentSchemaVersion,
                    $"Save schema version {sourceVersion} is newer than supported version {CurrentSchemaVersion}.");
                return false;
            }

            if (sourceVersion == CurrentSchemaVersion)
            {
                parsedSaveData.SchemaVersion = CurrentSchemaVersion;
                if (!ValidateAndNormalizeSave(parsedSaveData, out var validationError))
                {
                    result = SaveMigrationResult.Failed(sourceVersion, CurrentSchemaVersion, validationError);
                    return false;
                }

                saveData = parsedSaveData;
                result = SaveMigrationResult.NotRequired(CurrentSchemaVersion);
                return true;
            }

            if (!_migrationRegistry.CanMigrate(sourceVersion, CurrentSchemaVersion))
            {
                result = SaveMigrationResult.Failed(
                    sourceVersion,
                    CurrentSchemaVersion,
                    $"No complete migration path from v{sourceVersion} to v{CurrentSchemaVersion}.");
                return false;
            }

            var backupFilePath = string.Empty;
            if (allowWriteBack && !SaveBackupService.TryCreateBackup(savePath, out backupFilePath, out var backupError))
            {
                result = SaveMigrationResult.Failed(sourceVersion, CurrentSchemaVersion, $"Could not create save backup: {backupError}");
                return false;
            }

            var context = new SaveMigrationContext(savePath, backupFilePath, sourceVersion, CurrentSchemaVersion, rawJson);
            if (!_migrationRegistry.TryMigrate(context, out result))
            {
                Debug.LogWarning(result.ErrorMessage, this);
                return false;
            }

            if (!TryDeserializeSave(result.MigratedJson, out var migratedSaveData, out var migratedParseError))
            {
                result.Success = false;
                result.ErrorMessage = migratedParseError;
                return false;
            }

            migratedSaveData.SchemaVersion = CurrentSchemaVersion;
            if (!ValidateAndNormalizeSave(migratedSaveData, out var migratedValidationError))
            {
                result.Success = false;
                result.ErrorMessage = migratedValidationError;
                return false;
            }

            if (allowWriteBack)
            {
                try
                {
                    WriteTextSafely(savePath, JsonUtility.ToJson(migratedSaveData, true));
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Could not write migrated save: {exception.Message}";
                    return false;
                }
            }

            saveData = migratedSaveData;
            Debug.Log($"Save migrated from schema v{sourceVersion} to v{CurrentSchemaVersion}. Backup: {backupFilePath}", this);
            return true;
        }

        private static bool TryDeserializeSave(string json, out GameSaveData saveData, out string errorMessage)
        {
            saveData = null;
            errorMessage = string.Empty;

            try
            {
                saveData = JsonUtility.FromJson<GameSaveData>(json);
                if (saveData == null)
                {
                    errorMessage = "Save file could not be parsed.";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Save file could not be parsed: {exception.Message}";
                return false;
            }
        }

        private static int DetectSourceSchemaVersion(GameSaveData saveData)
        {
            if (saveData == null)
            {
                return 0;
            }

            if (saveData.SchemaVersion > 0)
            {
                return saveData.SchemaVersion;
            }

            return IsLegacyV1Candidate(saveData) ? 1 : 0;
        }

        private static bool IsLegacyV1Candidate(GameSaveData saveData)
        {
            if (saveData == null)
            {
                return false;
            }

            return saveData.Player != null
                || saveData.Inventory != null
                || saveData.Farm != null
                || saveData.World != null
                || saveData.Cave != null
                || saveData.CurrentDay > 0;
        }

        private static bool ValidateAndNormalizeSave(GameSaveData saveData, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (saveData == null)
            {
                errorMessage = "Save data is null.";
                return false;
            }

            if (saveData.SchemaVersion != CurrentSchemaVersion)
            {
                errorMessage = $"Unsupported save schema version {saveData.SchemaVersion}. Expected {CurrentSchemaVersion}.";
                return false;
            }

            if (saveData.Player == null)
            {
                errorMessage = "Save data is missing Player section.";
                return false;
            }

            if (saveData.CurrentDay <= 0)
            {
                saveData.CurrentDay = 1;
            }

            saveData.Inventory ??= new InventorySaveData();
            saveData.Equipment ??= new EquipmentSaveData();
            saveData.Hotbar ??= new HotbarSaveData();
            saveData.Progression ??= new PlayerProgressionSaveData();
            saveData.Farm ??= new FarmSaveData();
            saveData.World ??= new WorldSaveData();
            saveData.Cave ??= new CaveSaveData();
            saveData.Npcs ??= new NpcManagerSaveData();
            saveData.Bestiary ??= new BestiarySaveData();

            saveData.Inventory.Items ??= new List<InventoryItemSaveData>();
            saveData.Inventory.Slots ??= new List<InventorySlotSaveData>();
            if (saveData.Inventory.Capacity <= 0)
            {
                saveData.Inventory.Capacity = InventoryManager.DefaultCapacity;
            }

            saveData.Farm.Plots ??= new List<FarmPlotSaveData>();
            saveData.Farm.Trees ??= new List<TreeSaveData>();
            // fable_55: save legado sem o campo aditivo carrega com seção de processamento vazia.
            saveData.Farm.Processing ??= new Farm.Processing.FarmProcessingSaveData();
            saveData.Farm.Processing.Jobs ??= new List<Farm.Processing.FarmProcessingJobSaveData>();
            saveData.World.Pickups ??= new List<ItemPickupSaveData>();
            saveData.World.Trees ??= new List<TreeSaveData>();
            saveData.Npcs.Npcs ??= new List<NpcSaveData>();
            saveData.Bestiary.Entries ??= new List<BestiaryEntrySaveData>();

            saveData.Death ??= new DeathSaveData();
            saveData.Death.DeathStats ??= new DeathStatsSaveData();
            if (saveData.Death.ActiveCorpse != null)
            {
                saveData.Death.ActiveCorpse.LostInventoryItems ??= new List<InventorySlotSaveData>();
                saveData.Death.ActiveCorpse.LostEquipmentItems ??= new List<InventorySlotSaveData>();
            }

            saveData.Quests ??= new QuestStateSectionSaveData();
            saveData.Quests.QuestStates ??= new List<QuestStateSaveData>();
            saveData.Quests.GlobalKnownHints ??= new List<string>();
            foreach (var qr in saveData.Quests.QuestStates)
            {
                if (qr == null) continue;
                qr.CompletedStepIds ??= new List<string>();
                qr.FailedStepIds ??= new List<string>();
                qr.ObjectiveStates ??= new List<QuestObjectiveStateSaveData>();
                qr.KnownObjectiveIds ??= new List<string>();
                qr.KnownHints ??= new List<string>();
                qr.GrantedRewardIds ??= new List<string>();
                qr.GrantedFlagIds ??= new List<string>();
            }

            return true;
        }

        private static void WriteTextSafely(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = $"{path}.tmp";
            File.WriteAllText(tempPath, contents);

            if (!File.Exists(tempPath))
            {
                throw new IOException($"Temporary save file was not written: {tempPath}");
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(tempPath, path);
        }

        private FarmSaveData CaptureFarmSaveData(GameSaveData existingSaveData)
        {
            var activeScene = SceneManager.GetActiveScene();
            FarmSaveData farmSaveData;
            if (activeScene.name == FarmSceneName && _farmPlotRegistry != null)
            {
                farmSaveData = _farmPlotRegistry.CaptureSaveData();
            }
            else if (existingSaveData?.Farm != null)
            {
                farmSaveData = existingSaveData.Farm;
            }
            else
            {
                farmSaveData = new FarmSaveData();
            }

            // fable_55: jobs de processamento são donos do FarmProcessingStationService (DontDestroyOnLoad,
            // independente de cena). Sempre sobrescreve o campo aditivo com o estado vivo do serviço;
            // serviço ausente = preserva o que já existia (ou vazio).
            var processingService = Farm.Processing.FarmProcessingStationService.Instance;
            if (processingService != null)
            {
                farmSaveData.Processing = processingService.CaptureSaveData();
            }
            else if (farmSaveData.Processing == null)
            {
                farmSaveData.Processing = existingSaveData?.Farm?.Processing ?? new Farm.Processing.FarmProcessingSaveData();
            }

            // fable_54: batch de envio pendente (dono ShippingBinRuntimeService, DontDestroyOnLoad,
            // independente de cena). Sempre sobrescreve com o estado vivo; serviço ausente = preserva.
            var shippingBin = Farm.Shipping.ShippingBinRuntimeService.Instance;
            if (shippingBin != null)
            {
                farmSaveData.PendingShipping = shippingBin.CaptureSaveData();
            }
            else if (farmSaveData.PendingShipping == null)
            {
                farmSaveData.PendingShipping = existingSaveData?.Farm?.PendingShipping ?? new Farm.Shipping.PendingShippingSaveData();
            }

            // fable_54: estado dos pontos de forrageio do dia (dono FarmForageRuntimeService,
            // DontDestroyOnLoad). Sempre sobrescreve com o estado vivo; serviço ausente = preserva.
            var forageService = Farm.Forage.FarmForageRuntimeService.Instance;
            if (forageService != null)
            {
                farmSaveData.ForageSpawns = forageService.CaptureSaveData();
            }
            else if (farmSaveData.ForageSpawns == null)
            {
                farmSaveData.ForageSpawns = existingSaveData?.Farm?.ForageSpawns ?? new Farm.Forage.ForageSpawnsSaveData();
            }

            return farmSaveData;
        }

        private WorldSaveData CaptureWorldSaveData(GameSaveData existingSaveData)
        {
            var worldSaveData = new WorldSaveData();
            var activeScene = SceneManager.GetActiveScene();

            if (activeScene.name == FarmSceneName)
            {
                if (_itemPickupRegistry != null)
                {
                    worldSaveData.Pickups = _itemPickupRegistry.CaptureSaveData();
                }
                else if (existingSaveData?.World?.Pickups != null)
                {
                    worldSaveData.Pickups = existingSaveData.World.Pickups;
                }

                if (_treeRegistry != null)
                {
                    worldSaveData.Trees = _treeRegistry.CaptureSaveData();
                }
                else if (existingSaveData?.World?.Trees != null)
                {
                    worldSaveData.Trees = existingSaveData.World.Trees;
                }
            }
            else
            {
                if (existingSaveData?.World != null)
                {
                    worldSaveData.Pickups = existingSaveData.World.Pickups;
                    worldSaveData.Trees = existingSaveData.World.Trees;
                }
            }

            return worldSaveData;
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

        private void ApplySaveData(GameSaveData saveData)
        {
            if (_timeManager != null)
            {
                _timeManager.SetCurrentDay(saveData.CurrentDay);
            }
            else
            {
                Debug.LogWarning("SaveManager skipped day restore because TimeManager is missing.", this);
            }

            if (_playerManager != null)
            {
                _playerManager.RestoreFromSaveData(saveData.Player);
            }
            else
            {
                Debug.LogWarning("SaveManager skipped player restore because PlayerManager is missing.", this);
            }

            if (saveData.Player != null && _hungerManager != null)
            {
                _hungerManager.RestoreFromSaveData(saveData.Player.CurrentHunger, saveData.Player.MaxHunger);
            }

            // F16: restaura fadiga (saves legados carregam com 0).
            if (saveData.Player != null && Player.Conditions.PlayerConditionService.Instance != null)
            {
                Player.Conditions.PlayerConditionService.Instance.SetFatigue(saveData.Player.Fatigue);
            }

            // F17: restaura a Fonte (saves legados = seção nula → Fonte dormante padrão).
            if (saveData.Fonte != null && Fonte.FonteRuntimeService.Instance != null)
            {
                Fonte.FonteRuntimeService.Instance.RestoreFromSave(
                    saveData.Fonte.FonteState,
                    saveData.Fonte.UnlockedFunctions,
                    saveData.Fonte.LivingWaterUnlocked,
                    saveData.Fonte.LivingWaterCharges,
                    saveData.Fonte.LastGrantDay,
                    saveData.Fonte.IntegratedFragments);

                // fable_43: restaura o estado do endgame (Ato 5) APOS os fragmentos serem reconstruidos
                // (RestoreFromSave recria a secao). Save legado sem a secao endgame => defaults (nao
                // iniciado). So tipos simples; nenhuma ref Unity.
                if (saveData.MainProgression != null)
                {
                    Fonte.FonteRuntimeService.Instance.RestoreEndgameState(
                        saveData.MainProgression.CurrentAct,
                        saveData.MainProgression.Level100GateState,
                        saveData.MainProgression.Level101AccessState,
                        saveData.MainProgression.FinalChoiceState,
                        saveData.MainProgression.PostGameWorldState);
                }
            }

            // F13: restaura a run da caverna via cache do bootstrap (CaveRunManager consome ao entrar).
            if (saveData.CaveRun != null && saveData.CaveRun.HasActiveRun)
            {
                var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
                var restoredRun = Cave.Runtime.CaveRunSaveMapper.FromSaveData(saveData.CaveRun);
                if (bootstrap != null && restoredRun != null)
                {
                    bootstrap.SetCachedCaveRunState(restoredRun);
                    Debug.Log($"SaveManager: cave run restaurada (nível {restoredRun.CurrentCaveLevel}, seed {restoredRun.CaveRunSeed}).", this);
                }
            }

            // F13: restaura metas diárias (DTO existia desde WAVE 24 sem wiring).
            if (saveData.DailyGoals != null && Farm.Runtime.FarmDailyGoalService.Instance != null)
            {
                Farm.Runtime.FarmDailyGoalService.Instance.RestoreFromSaveData(saveData.DailyGoals);
            }
            else
            {
                Debug.LogWarning("SaveManager skipped hunger restore because save data or HungerManager is missing.", this);
            }

            // fable_41: restaura a posse dos lotes. saveData.FarmLots ausente (legado) ⇒ o serviço
            // recebe null e mantém TODOS os lotes Locked (CA-4), sem erro.
            if (Farm.Lots.FarmLotService.Instance != null)
            {
                Farm.Lots.FarmLotService.Instance.RestoreFromSaveData(saveData.FarmLots);
            }

            // fable_12: restaura os animais de fazenda. saveData.FarmAnimals ausente (legado) ⇒ o
            // registry recebe null e mantém zero animais (CA-3), sem erro. Os FarmAnimalRuntime são
            // re-hidratados pelos AnimalReleaseHandler de cena após o load.
            if (Farm.Animals.FarmAnimalRegistry.Instance != null)
            {
                Farm.Animals.FarmAnimalRegistry.Instance.RestoreFromSaveData(saveData.FarmAnimals);
            }

            // fable_26: restaura a amizade por NPC. saveData.Friendship ausente (legado) ⇒ o serviço
            // recebe null e mantém estado limpo = todos os NPCs nível 0 (Unknown) (CA-3), sem erro.
            if (NPC.Friendship.FriendshipService.Instance != null)
            {
                NPC.Friendship.FriendshipService.Instance.RestoreFromSaveData(saveData.Friendship);
            }

            // fable_25: restaura as pendências dos serviços de NPC. saveData.NpcServices ausente (legado)
            // ⇒ Restore(null) = sem encomendas/contrato/prato/pasto, sem erro (CA-5).
            if (NPC.Services.NpcServiceRuntime.Instance != null)
            {
                NPC.Services.NpcServiceRuntime.Instance.RestoreFromSaveData(saveData.NpcServices);
            }

            // fable_55: restaura os jobs de processamento (queijaria/barril). Campo aditivo na seção
            // farm; ausente em save legado ⇒ Restore(null) = nenhuma estação em producao (CA-5).
            if (Farm.Processing.FarmProcessingStationService.Instance != null)
            {
                Farm.Processing.FarmProcessingStationService.Instance.RestoreFromSaveData(saveData.Farm?.Processing);
            }

            // fable_54: restaura o batch de envio pendente. Campo aditivo na seção farm; ausente em
            // save legado ⇒ Restore(null) = lista vazia = nenhum batch pendente (CA-4). Pagamento
            // acontece no próximo DayStarted da manhã correta.
            if (Farm.Shipping.ShippingBinRuntimeService.Instance != null)
            {
                Farm.Shipping.ShippingBinRuntimeService.Instance.RestoreFromSaveData(saveData.Farm?.PendingShipping);
            }

            // fable_54: restaura o estado dos pontos de forrageio do dia. Campo aditivo na seção farm;
            // ausente em save legado ⇒ Restore(null) = sem spawns (regeneram no próximo DayStarted).
            if (Farm.Forage.FarmForageRuntimeService.Instance != null)
            {
                Farm.Forage.FarmForageRuntimeService.Instance.RestoreFromSaveData(saveData.Farm?.ForageSpawns);
            }

            if (saveData.Player != null && _playerTransform != null)
            {
                _playerTransform.position = saveData.Player.PlayerPosition;
            }
            else
            {
                Debug.LogWarning("SaveManager skipped player position restore because save data or player Transform is missing.", this);
            }

            if (_inventoryManager != null)
            {
                _inventoryManager.RestoreFromSaveData(saveData.Inventory);
                _inventoryManager.EnsureStarterItemsPresent(_playerData, _itemDatabase, "RepairMissingItems");
            }
            else
            {
                Debug.LogWarning("SaveManager skipped inventory restore because InventoryManager is missing.", this);
            }

            if (_equipmentManager != null)
            {
                _equipmentManager.RestoreFromSaveData(saveData.Equipment);
            }

            // SPEC_10: Use hotbar provider if available, otherwise fallback to direct _hotbarState
            if (_hotbarProvider != null)
            {
                _hotbarProvider.Restore(saveData.Hotbar);
            }
            else
            {
                _hotbarState.RestoreFromSaveData(saveData.Hotbar);
            }

            if (_inventoryManager != null)
            {
                _inventoryManager.ClearHotbarBindingsForMissingItems(
                    _hotbarState.GetSlotItemId,
                    (slot, id) => _hotbarState.SetSlot(slot, id),
                    HotbarState.SlotCount);
            }

            // fable_07: restaura o grimório APÓS o inventário (contrato de ordem da spec).
            // Seção nula (saves legados) = grimório vazio. Fonte: PlayerSpellbook.Instance.
            _spellbookProvider?.Restore(saveData.Spellbook);

            // fable_62: restaura os hints de onboarding vistos. Seção nula (save legado) = lista
            // vazia = todos os hints elegíveis de novo. Fonte: OnboardingHintService.Instance.
            _onboardingHintsProvider?.Restore(saveData.OnboardingHints);

            if (_progressionManager != null)
            {
                _progressionManager.RestoreFromSaveData(saveData.Progression);
            }

            if (_caveRunManager != null)
            {
                _caveRunManager.RestoreFromSaveData(saveData.Cave);
            }

            if (_itemPickupRegistry != null && saveData.World != null)
            {
                _itemPickupRegistry.RestoreFromSaveData(saveData.World.Pickups);
            }

            if (_farmPlotRegistry != null && saveData.Farm != null)
            {
                _farmPlotRegistry.RestoreFromSaveData(saveData.Farm);
            }

            if (_treeRegistry != null && saveData.World != null)
            {
                var treeFarmSaveData = new FarmSaveData
                {
                    Trees = saveData.World.Trees ?? new List<TreeSaveData>()
                };
                _treeRegistry.RestoreFromSaveData(treeFarmSaveData);
            }

            if (_shopManager != null && saveData.Economy != null)
            {
                RestoreEconomySaveData(saveData.Economy);
            }

            if (_npcManager != null && saveData.Npcs != null)
            {
                _npcManager.RestoreFromSaveData(saveData.Npcs);
            }

            if (_craftingRuntime != null && saveData.Crafting != null)
            {
                _craftingRuntime.LoadFromSaveData(saveData.Crafting);
            }

            if (_staminaManager != null && saveData.Stamina != null)
            {
                _staminaManager.Initialize(saveData.Stamina.MaxStamina, saveData.Stamina.CurrentStamina);
            }

            if (_gameTimeManager != null && saveData.GameTime != null)
            {
                _gameTimeManager.RestoreFromSaveData(saveData.GameTime);
            }

            if (_statusEffectManager != null && saveData.PlayerStatusEffects != null)
            {
                _statusEffectManager.Shutdown();
                _statusEffectManager.Initialize();
                foreach (var effectEntry in saveData.PlayerStatusEffects.ActiveEffects)
                {
                    if (!string.IsNullOrWhiteSpace(effectEntry.EffectId) && effectEntry.RemainingSeconds > 0)
                    {
                        _statusEffectManager.TryAddEffect(effectEntry.EffectId, effectEntry.RemainingSeconds);
                    }
                }
            }

            if (saveData.EquipmentDurability != null && _equipmentManager != null && _equipmentManager.DurabilityTracker != null)
            {
                _equipmentManager.DurabilityTracker.LoadFromSaveData(saveData.EquipmentDurability);
            }

            if (_manaManager != null && saveData.Player != null)
            {
                _manaManager.RestoreFromSaveData(new ManaManagerSaveData { CurrentMana = saveData.Player.CurrentMana, MaxMana = saveData.Player.MaxMana });
            }

            if (_activeSkillSlots != null && saveData.ActiveSkillSlots != null)
            {
                RestoreActiveSkillSlots(saveData.ActiveSkillSlots);
            }

            if (_skillTreeManager != null && saveData.SkillTree != null)
            {
                int level = saveData.Progression?.Level ?? 1;
                _skillTreeManager.RestoreFromSaveData(saveData.SkillTree, level);
            }

            if (_bestiaryManager != null && saveData.Bestiary != null)
            {
                _bestiaryManager.RestoreFromSaveData(saveData.Bestiary);
            }

            RestoreDeathSaveData(saveData.Death);
            RestoreQuestSaveData(saveData.Quests);
        }

        private static bool ShouldRepairStarterInventoryAfterRestore(InventorySaveData inventorySaveData)
        {
            if (inventorySaveData == null)
            {
                return true;
            }

            var hasSlots = inventorySaveData.Slots != null
                && inventorySaveData.Slots.Exists(slot => slot != null && !string.IsNullOrWhiteSpace(slot.ItemId) && slot.Amount > 0);
            var hasLegacyItems = inventorySaveData.Items != null
                && inventorySaveData.Items.Exists(item => item != null && !string.IsNullOrWhiteSpace(item.ItemId) && item.Amount > 0);

            return !hasSlots && !hasLegacyItems;
        }

        private EconomySaveData CaptureEconomySaveData(GameSaveData existingSaveData)
        {
            var economyData = new EconomySaveData();

            if (_shopManager != null)
            {
                economyData.Shops = _shopManager.CaptureAllShopStock();
            }
            else if (existingSaveData?.Economy != null)
            {
                economyData.Shops = existingSaveData.Economy.Shops;
            }

            return economyData;
        }

        private void RestoreEconomySaveData(EconomySaveData economyData)
        {
            if (_shopManager == null || economyData == null)
            {
                return;
            }

            if (economyData.Shops == null)
            {
                return;
            }

            foreach (var shopStockData in economyData.Shops)
            {
                _shopManager.LoadShopStock(shopStockData);
            }
        }

        private CraftingRuntimeSaveData CaptureCraftingSaveData()
        {
            if (_craftingRuntime == null)
            {
                return new CraftingRuntimeSaveData();
            }

            return _craftingRuntime.CaptureSaveData();
        }

        private StaminaSaveData CaptureStaminaSaveData()
        {
            if (_staminaManager == null)
            {
                return new StaminaSaveData { CurrentStamina = 100, MaxStamina = 100 };
            }

            return new StaminaSaveData
            {
                CurrentStamina = _staminaManager.CurrentStamina,
                MaxStamina = _staminaManager.MaxStamina
            };
        }

        private GameTimeSaveData CaptureGameTimeSaveData()
        {
            if (_gameTimeManager == null)
            {
                return new GameTimeSaveData
                {
                    CurrentDay = CaptureCurrentDay(),
                    CurrentPhase = 0,
                    PhaseElapsedSeconds = 0f
                };
            }

            return new GameTimeSaveData
            {
                CurrentDay = _timeManager != null ? _timeManager.CurrentDay : 1,
                CurrentPhase = (int)_gameTimeManager.CurrentPhase,
                PhaseElapsedSeconds = _gameTimeManager.PhaseTimer
            };
        }

        private PlayerStatusEffectsSaveData CapturePlayerStatusEffectsSaveData()
        {
            var data = new PlayerStatusEffectsSaveData();

            if (_statusEffectManager == null || !_statusEffectManager.IsInitialized)
            {
                return data;
            }

            foreach (var kvp in _statusEffectManager.ActiveEffects)
            {
                if (kvp.Value != null && kvp.Value.IsActive)
                {
                    data.ActiveEffects.Add(new StatusEffectEntryData
                    {
                        EffectId = kvp.Key,
                        RemainingSeconds = kvp.Value.RemainingSeconds
                    });
                }
            }

            return data;
        }

        private EquipmentDurabilitySaveData CaptureEquipmentDurabilitySaveData()
        {
            var data = new EquipmentDurabilitySaveData();

            if (_equipmentManager != null && _equipmentManager.DurabilityTracker != null)
            {
                return _equipmentManager.DurabilityTracker.CaptureSaveData();
            }

            return data;
        }

        private NpcManagerSaveData CaptureNpcSaveData(GameSaveData existingSaveData)
        {
            if (SceneManager.GetActiveScene().name == TownSceneName && _npcManager != null)
            {
                return _npcManager.CaptureSaveData();
            }

            return existingSaveData?.Npcs ?? new NpcManagerSaveData();
        }

        private ActiveSkillSlotsSaveData CaptureActiveSkillSlotsSaveData()
        {
            var data = new ActiveSkillSlotsSaveData();
            if (_activeSkillSlots != null)
            {
                var slot0 = _activeSkillSlots.GetSlot(0);
                var slot1 = _activeSkillSlots.GetSlot(1);
                var slot2 = _activeSkillSlots.GetSlot(2);
                var slot3 = _activeSkillSlots.GetSlot(3);

                data.SlotRSkillActionId = slot0?.SkillActionId ?? string.Empty;
                data.SlotTSkillActionId = slot1?.SkillActionId ?? string.Empty;
                data.SlotYSkillActionId = slot2?.SkillActionId ?? string.Empty;
                data.SlotGSkillActionId = slot3?.SkillActionId ?? string.Empty;
            }
            return data;
        }

        private void RestoreActiveSkillSlots(ActiveSkillSlotsSaveData data)
        {
            if (_activeSkillSlots == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(data.SlotRSkillActionId))
            {
                _activeSkillSlots.SetSkillInSlot(0, data.SlotRSkillActionId, 0f);
            }

            if (!string.IsNullOrEmpty(data.SlotTSkillActionId))
            {
                _activeSkillSlots.SetSkillInSlot(1, data.SlotTSkillActionId, 0f);
            }

            if (!string.IsNullOrEmpty(data.SlotYSkillActionId))
            {
                _activeSkillSlots.SetSkillInSlot(2, data.SlotYSkillActionId, 0f);
            }

            if (!string.IsNullOrEmpty(data.SlotGSkillActionId))
            {
                _activeSkillSlots.SetSkillInSlot(3, data.SlotGSkillActionId, 0f);
            }
        }

        private Skills.SkillTreeSaveData CaptureSkillTreeSaveData()
        {
            if (_skillTreeManager != null)
                return _skillTreeManager.CaptureSaveData();
            return new Skills.SkillTreeSaveData();
        }

        private BestiarySaveData CaptureBestiarySaveData()
        {
            return _bestiaryManager != null ? _bestiaryManager.CaptureSaveData() : new BestiarySaveData();
        }

        private DeathSaveData CaptureDeathSaveData(GameSaveData existingSaveData)
        {
            var deathData = new DeathSaveData();
            if (existingSaveData?.Death != null)
            {
                deathData.DeathStats = existingSaveData.Death.DeathStats ?? new DeathStatsSaveData();
                deathData.ActiveCorpse = existingSaveData.Death.ActiveCorpse;
            }
            else
            {
                deathData.DeathStats = new DeathStatsSaveData();
            }
            return deathData;
        }

        private void RestoreDeathSaveData(DeathSaveData deathData)
        {
            if (deathData == null)
            {
                return;
            }

            // SPEC_25: Restore active corpse to CorpseRecoveryManager if available
            if (_corpseRecoveryManager != null && deathData.ActiveCorpse != null)
            {
                var corpseData = deathData.ActiveCorpse;
                var corpse = new Player.Death.Corpse(corpseData.CorpseId)
                {
                    Status = (Player.Death.CorpseStatus)corpseData.CorpseStatusValue,
                    RunId = corpseData.RunId,
                    CaveSeed = corpseData.CaveSeed,
                    CaveLevel = corpseData.CaveLevel,
                    SceneName = corpseData.SceneName,
                    Position = corpseData.Position,
                    GoldAmount = corpseData.GoldAmount,
                    CreatedAtGameDay = corpseData.CreatedAtGameDay,
                    CreatedAtGameTime = corpseData.CreatedAtGameTime,
                    RecoveredAtGameDay = corpseData.RecoveredAtGameDay,
                    ReplacedByCorpseId = corpseData.ReplacedByCorpseId
                };

                // Restore lost inventory items
                if (corpseData.LostInventoryItems != null)
                {
                    foreach (var slotData in corpseData.LostInventoryItems)
                    {
                        if (slotData == null || string.IsNullOrWhiteSpace(slotData.ItemId))
                            continue;

                        var corpseItem = new Player.Death.CorpseItem(slotData.ItemId, slotData.Amount)
                        {
                            SourceSlotIndex = slotData.SlotIndex
                        };
                        corpse.InventoryItems.Add(corpseItem);
                    }
                }

                // Restore lost equipment items
                if (corpseData.LostEquipmentItems != null)
                {
                    foreach (var slotData in corpseData.LostEquipmentItems)
                    {
                        if (slotData == null || string.IsNullOrWhiteSpace(slotData.ItemId))
                            continue;

                        var corpseItem = new Player.Death.CorpseItem(slotData.ItemId, slotData.Amount)
                        {
                            SourceSlotIndex = slotData.SlotIndex
                        };
                        corpse.EquipmentItems.Add(corpseItem);
                    }
                }

                _corpseRecoveryManager.SetActiveCorpse(corpse);
                Debug.Log($"[SaveManager] Restored active corpse {corpse.CorpseId} from save data with {corpse.InventoryItems.Count} inventory items and {corpse.EquipmentItems.Count} equipment items", this);
            }
            else if (_corpseRecoveryManager == null && deathData.ActiveCorpse != null)
            {
                Debug.LogWarning("[SaveManager] CorpseRecoveryManager not injected; active corpse from save data will not be restored", this);
            }
        }

        private static QuestStateSectionSaveData CaptureQuestSaveData(GameSaveData existingSaveData)
        {
            var liveSection = QuestRuntimeBootstrap.CaptureSaveData();
            if (liveSection != null)
            {
                return liveSection;
            }

            // QuestService not yet initialized — preserve existing save section
            return existingSaveData?.Quests ?? new QuestStateSectionSaveData();
        }

        private static void RestoreQuestSaveData(QuestStateSectionSaveData questData)
        {
            if (questData == null)
            {
                return;
            }

            QuestRuntimeBootstrap.RestoreFromSaveData(questData);
        }

        private void PublishSaveResult(bool wasSuccessful, string message)
        {
            GameEventBus.Publish(new GameSavedEvent(Slot, SaveFilePath, wasSuccessful, message));
        }
    }
}
