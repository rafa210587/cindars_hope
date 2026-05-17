using System;
using System.IO;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Farm;
using CindarsHope.Inventory;
using CindarsHope.Player;
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
            if (!ValidateReferences())
            {
                PublishSaveResult(false, "Save failed: missing required references.");
                return false;
            }

            try
            {
                var saveData = new GameSaveData
                {
                    SchemaVersion = CurrentSchemaVersion,
                    CurrentDay = _timeManager.CurrentDay,
                    Player = _playerManager.CaptureSaveData(_hungerManager.CurrentHunger, _hungerManager.MaxHunger, _playerTransform.position),
                    Inventory = _inventoryManager.CaptureSaveData(),
                    Farm = _farmPlotRegistry.CaptureSaveData()
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

            if (!ValidateReferences())
            {
                Debug.LogWarning("SaveManager cannot load because required references are missing.", this);
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

                _timeManager.SetCurrentDay(saveData.CurrentDay);
                _playerManager.RestoreFromSaveData(saveData.Player);

                if (saveData.Player != null)
                {
                    _hungerManager.RestoreFromSaveData(saveData.Player.CurrentHunger, saveData.Player.MaxHunger);
                    _playerTransform.position = saveData.Player.PlayerPosition;
                }

                _inventoryManager.RestoreFromSaveData(saveData.Inventory);
                _farmPlotRegistry.RestoreFromSaveData(saveData.Farm);

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

        private bool ValidateReferences()
        {
            var valid = true;

            if (_playerManager == null)
            {
                Debug.LogWarning("SaveManager missing PlayerManager reference.", this);
                valid = false;
            }

            if (_inventoryManager == null)
            {
                Debug.LogWarning("SaveManager missing InventoryManager reference.", this);
                valid = false;
            }

            if (_hungerManager == null)
            {
                Debug.LogWarning("SaveManager missing HungerManager reference.", this);
                valid = false;
            }

            if (_timeManager == null)
            {
                Debug.LogWarning("SaveManager missing TimeManager reference.", this);
                valid = false;
            }

            if (_farmPlotRegistry == null)
            {
                Debug.LogWarning("SaveManager missing FarmPlotRegistry reference.", this);
                valid = false;
            }

            if (_playerTransform == null)
            {
                Debug.LogWarning("SaveManager missing Player Transform reference.", this);
                valid = false;
            }

            return valid;
        }

        private void PublishSaveResult(bool wasSuccessful, string message)
        {
            GameEventBus.Publish(new GameSavedEvent(Slot, SaveFilePath, wasSuccessful, message));
        }
    }
}
