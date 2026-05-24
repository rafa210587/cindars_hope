using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Cave.Runtime;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Player.Progression;
using CindarsHope.Save.Migrations;
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
        private const int CurrentSchemaVersion = 2;
        private const int Slot = 1;
        private const string SaveDirectoryName = "saves";
        private const string SaveFileName = "slot_1.json";
        private const string FarmSceneName = "FarmScene";

        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private FarmPlotRegistry _farmPlotRegistry;
        [SerializeField] private TreeRegistry _treeRegistry;
        [SerializeField] private ItemPickupRegistry _itemPickupRegistry;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private PlayerProgressionManager _progressionManager;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private ShopManager _shopManager;

        private readonly HotbarState _hotbarState = new HotbarState();
        private readonly SaveMigrationRegistry _migrationRegistry = new SaveMigrationRegistry(new ISaveMigration[]
        {
            new InventorySlotsV1ToV2Migration()
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

            if (string.IsNullOrWhiteSpace(_hotbarState.GetSlotItemId(0)))
            {
                _hotbarState.SetSlot(0, "seed_wheat");
                _hotbarState.SetSlot(1, "seed_carrot");
                _hotbarState.SetSlot(2, "item_tool_fishing_rod_basic");
            }
        }

        public bool SaveGame()
        {
            try
            {
                var existingSaveData = TryReadExistingValidSave();
                var activeScene = SceneManager.GetActiveScene();

                // Capture farm and world only if in FarmScene; otherwise preserve existing data to avoid loss when saving from TownScene.
                var farmSaveData = CaptureFarmSaveData(existingSaveData);
                var worldSaveData = CaptureWorldSaveData(existingSaveData);
                var caveSaveData = CaptureCaveSaveData(existingSaveData);
                var economySaveData = CaptureEconomySaveData(existingSaveData);
                var craftingSaveData = CaptureCraftingSaveData(existingSaveData);

                var saveData = new GameSaveData
                {
                    SchemaVersion = CurrentSchemaVersion,
                    CurrentDay = CaptureCurrentDay(),
                    CurrentSceneName = activeScene.name,
                    CurrentScenePath = activeScene.path,
                    Player = CapturePlayerSaveData(),
                    Inventory = CaptureInventorySaveData(),
                    Equipment = CaptureEquipmentSaveData(),
                    Hotbar = _hotbarState.CaptureSaveData(),
                    Progression = CaptureProgressionSaveData(),
                    Farm = farmSaveData,
                    World = worldSaveData,
                    Cave = caveSaveData,
                    Economy = economySaveData,
                    Crafting = craftingSaveData
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
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
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

        public void RebindOptionalRuntimeManagers(EquipmentManager equipmentManager, PlayerProgressionManager progressionManager)
        {
            if (equipmentManager != null)
            {
                _equipmentManager = equipmentManager;
            }

            if (progressionManager != null)
            {
                _progressionManager = progressionManager;
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

            return _playerManager.CaptureSaveData(currentHunger, maxHunger, playerPosition);
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

            // TODO: SaveBackupService backup feature (future)
            var backupFilePath = string.Empty;
            // if (allowWriteBack && !SaveBackupService.TryCreateBackup(savePath, out backupFilePath, out var backupError))
            // {
            //     result = SaveMigrationResult.Failed(sourceVersion, CurrentSchemaVersion, $"Could not create save backup: {backupError}");
            //     return false;
            // }

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

            saveData.Inventory.Items ??= new List<InventoryItemSaveData>();
            saveData.Inventory.Slots ??= new List<InventorySlotSaveData>();
            if (saveData.Inventory.Capacity <= 0)
            {
                saveData.Inventory.Capacity = InventoryManager.DefaultCapacity;
            }

            saveData.Farm.Plots ??= new List<FarmPlotSaveData>();
            saveData.Farm.Trees ??= new List<TreeSaveData>();
            saveData.World.Pickups ??= new List<ItemPickupSaveData>();
            saveData.World.Trees ??= new List<TreeSaveData>();

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
            if (activeScene.name == FarmSceneName && _farmPlotRegistry != null)
            {
                return _farmPlotRegistry.CaptureSaveData();
            }

            if (existingSaveData?.Farm != null)
            {
                return existingSaveData.Farm;
            }

            return new FarmSaveData();
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
            else
            {
                Debug.LogWarning("SaveManager skipped hunger restore because save data or HungerManager is missing.", this);
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
            }
            else
            {
                Debug.LogWarning("SaveManager skipped inventory restore because InventoryManager is missing.", this);
            }

            if (_equipmentManager != null)
            {
                _equipmentManager.RestoreFromSaveData(saveData.Equipment);
            }

            _hotbarState.RestoreFromSaveData(saveData.Hotbar);

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

            if (saveData.Crafting != null)
            {
                RestoreCraftingSaveData(saveData.Crafting);
            }
        }

        private EconomySaveData CaptureEconomySaveData(GameSaveData existingSaveData)
        {
            var economyData = new EconomySaveData();

            if (_shopManager != null)
            {
                // Capture shop stock from all active shops
                // Note: This will require ShopManager to track all registered shops
                // For now, shops register themselves during initialization
                var existingEconomy = existingSaveData?.Economy;
                if (existingEconomy?.Shops != null)
                {
                    foreach (var shopStock in existingEconomy.Shops)
                    {
                        var capturedStock = _shopManager.CaptureShopStock(shopStock.ShopId);
                        if (capturedStock != null)
                        {
                            economyData.Shops.Add(capturedStock);
                        }
                    }
                }
            }

            return economyData.Shops.Count > 0 ? economyData : (existingSaveData?.Economy ?? new EconomySaveData());
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

        private CraftingSaveData CaptureCraftingSaveData(GameSaveData existingSaveData)
        {
            var craftingData = new CraftingSaveData();

            // TODO: Capture crafting station data from CraftingManager when integrated
            // For now, preserve existing crafting data
            return existingSaveData?.Crafting ?? new CraftingSaveData();
        }

        private void RestoreCraftingSaveData(CraftingSaveData craftingData)
        {
            if (craftingData == null || craftingData.Stations == null)
            {
                return;
            }

            // TODO: Restore crafting station data to CraftingManager when integrated
            foreach (var stationData in craftingData.Stations)
            {
                // Will be restored by CraftingManager/CraftingStation
            }
        }

        private void PublishSaveResult(bool wasSuccessful, string message)
        {
            GameEventBus.Publish(new GameSavedEvent(Slot, SaveFilePath, wasSuccessful, message));
        }
    }
}
