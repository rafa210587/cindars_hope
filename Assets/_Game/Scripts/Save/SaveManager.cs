using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Player.Progression;
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
        private const int CurrentSchemaVersion = 1;
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

        private readonly HotbarState _hotbarState = new HotbarState();

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
                    World = worldSaveData
                };

                var savePath = SaveFilePath;
                var directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(savePath, json);

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
                var json = File.ReadAllText(savePath);
                var saveData = JsonUtility.FromJson<GameSaveData>(json);
                if (saveData == null)
                {
                    Debug.LogWarning($"Save file at {savePath} could not be parsed.", this);
                    return false;
                }

                if (saveData.SchemaVersion != CurrentSchemaVersion)
                {
                    Debug.LogWarning($"Unsupported save schema version {saveData.SchemaVersion}. Expected {CurrentSchemaVersion}.", this);
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
                var json = File.ReadAllText(savePath);
                var saveData = JsonUtility.FromJson<GameSaveData>(json);
                if (saveData == null)
                {
                    Debug.LogWarning($"Existing save file could not be parsed.", this);
                    return null;
                }

                if (saveData.SchemaVersion != CurrentSchemaVersion)
                {
                    Debug.LogWarning($"Existing save has unsupported schema version {saveData.SchemaVersion}.", this);
                    return null;
                }

                return saveData;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Error reading existing save: {exception.Message}", this);
                return null;
            }
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
        }

        private void PublishSaveResult(bool wasSuccessful, string message)
        {
            GameEventBus.Publish(new GameSavedEvent(Slot, SaveFilePath, wasSuccessful, message));
        }
    }
}
