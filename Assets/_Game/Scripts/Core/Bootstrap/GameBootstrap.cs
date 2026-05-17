using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Save;
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
        [SerializeField] private SaveManager _saveManager;
        [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private ItemDatabaseSO _itemDatabase;

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

            if (_saveManager != null)
            {
                _saveManager.Initialize();
            }
            else
            {
                Debug.LogWarning("GameBootstrap is missing a SaveManager reference.", this);
            }
        }

        private void ShutdownManagers()
        {
            if (_saveManager != null)
            {
                _saveManager.Shutdown();
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
        }
    }
}
