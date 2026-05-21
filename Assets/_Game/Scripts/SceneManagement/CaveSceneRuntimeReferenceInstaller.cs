using CindarsHope.Core.Bootstrap;
using CindarsHope.Cave;
using CindarsHope.Cave.Runtime;
using CindarsHope.Interaction;
using CindarsHope.UI;
using UnityEngine;

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public sealed class CaveSceneRuntimeReferenceInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private CaveLevelRuntimeController _caveLevelRuntimeController;
        [SerializeField] private CaveDebugLevelSkipController _caveDebugLevelSkipController;

        private void Start()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogWarning("CaveSceneRuntimeReferenceInstaller: GameBootstrap.Instance is null, cannot rebind references.", this);
                return;
            }

            var saveManager = bootstrap.SaveManager;
            if (saveManager == null)
            {
                Debug.LogWarning("CaveSceneRuntimeReferenceInstaller: SaveManager is null, cannot rebind player transform.", this);
                return;
            }

            saveManager.RebindPlayerTransform(_playerTransform);
            saveManager.RebindCaveRuntime(_caveRunManager);

            var playerManager = bootstrap.PlayerManager;
            var inventoryManager = bootstrap.InventoryManager;
            var hungerManager = bootstrap.HungerManager;
            var timeManager = bootstrap.TimeManager;

            if (playerManager != null && inventoryManager != null && hungerManager != null && timeManager != null)
            {
                saveManager.RebindRuntimeManagers(playerManager, inventoryManager, hungerManager, timeManager);
                saveManager.RebindOptionalRuntimeManagers(bootstrap.EquipmentManager, bootstrap.PlayerProgressionManager);
            }
            else
            {
                Debug.LogWarning("CaveSceneRuntimeReferenceInstaller: one or more runtime managers are missing during SaveManager rebind.", this);
            }

            // Try to rebind CaveDebugLevelSkipController locally if null
            if (_caveDebugLevelSkipController == null && _caveLevelRuntimeController != null)
            {
                _caveDebugLevelSkipController = _caveLevelRuntimeController.GetComponent<CaveDebugLevelSkipController>();
            }

            var interactionSystem = _playerTransform != null ? _playerTransform.GetComponent<InteractionSystem>() : null;
            DebugHud.RebindExisting(playerManager, inventoryManager, hungerManager, interactionSystem, timeManager, saveManager);
            DebugHud.RebindExistingCaveRuntime(_caveRunManager, _caveLevelRuntimeController, _caveDebugLevelSkipController);

            Debug.Log("CaveSceneRuntimeReferenceInstaller rebound runtime refs.", this);
        }
    }
}
