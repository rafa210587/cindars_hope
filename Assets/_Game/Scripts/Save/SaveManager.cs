using System;
using System.IO;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Farm;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.Save
{
    [DisallowMultipleComponent]
    public class SaveManager : MonoBehaviour
    {
        private const int CurrentSchemaVersion = 1;
        private const int Slot = 1;
        private const string SaveDirectoryName = "saves";
        private const string SaveFileName = "slot_1.json";

        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private FarmPlotRegistry _farmPlotRegistry;
        [SerializeField] private TreeRegistry _treeRegistry;
        [SerializeField] private Transform _playerTransform;

        public bool IsInitialized { get; private set; }
        public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveDirectoryName, SaveFileName);

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public bool SaveGame()
        {
            try
            {
                var farmSaveData = new FarmSaveData();
                if (_farmPlotRegistry != null)
                {
                    farmSaveData = _farmPlotRegistry.CaptureSaveData();
                }
                else
                {
                    Debug.LogWarning("SaveManager saved without FarmPlotRegistry. Farm plots were omitted.", this);
                }

                if (_treeRegistry != null)
                {
                    farmSaveData.Trees = _treeRegistry.CaptureSaveData();
                }
                else
                {
                    Debug.LogWarning("SaveManager saved without TreeRegistry. Trees were omitted.", this);
                }

                var saveData = new GameSaveData
                {
                    SchemaVersion = CurrentSchemaVersion,
                    CurrentDay = CaptureCurrentDay(),
                    Player = CapturePlayerSaveData(),
                    Inventory = CaptureInventorySaveData(),
                    Farm = farmSaveData
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

                if (_farmPlotRegistry != null)
                {
                    _farmPlotRegistry.RestoreFromSaveData(saveData.Farm);
                }
                else
                {
                    Debug.LogWarning("SaveManager skipped farm plot restore because FarmPlotRegistry is missing.", this);
                }

                if (_treeRegistry != null)
                {
                    _treeRegistry.RestoreFromSaveData(saveData.Farm);
                }
                else
                {
                    Debug.LogWarning("SaveManager skipped tree restore because TreeRegistry is missing.", this);
                }

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

        private void PublishSaveResult(bool wasSuccessful, string message)
        {
            GameEventBus.Publish(new GameSavedEvent(Slot, SaveFilePath, wasSuccessful, message));
        }
    }
}
