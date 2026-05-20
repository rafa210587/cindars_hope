using CindarsHope.Core.Bootstrap;
using CindarsHope.Interaction;
using CindarsHope.UI;
using UnityEngine;

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public sealed class CaveSceneRuntimeReferenceInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;

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

            var interactionSystem = _playerTransform != null ? _playerTransform.GetComponent<InteractionSystem>() : null;
            DebugHud.RebindExisting(playerManager, inventoryManager, hungerManager, interactionSystem, timeManager, saveManager);

            Debug.Log("CaveSceneRuntimeReferenceInstaller rebound runtime refs.", this);
        }
    }
}
