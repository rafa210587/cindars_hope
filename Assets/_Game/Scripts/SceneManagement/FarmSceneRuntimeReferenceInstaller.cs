using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Craft;
using CindarsHope.Economy;
using CindarsHope.Farm;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public class FarmSceneRuntimeReferenceInstaller : MonoBehaviour
    {
        [SerializeField] private FarmPlot[] _farmPlots;
        [SerializeField] private TreeNode[] _treeNodes;
        [SerializeField] private FishingSpot _fishingSpot;
        [SerializeField] private SeedShopPoint _seedShopPoint;
        [SerializeField] private SellAllPoint _sellAllPoint;
        [SerializeField] private CraftingPoint _craftingPoint;
        [SerializeField] private FarmPlotRegistry _farmPlotRegistry;
        [SerializeField] private TreeRegistry _treeRegistry;
        [SerializeField] private ItemPickupRegistry _itemPickupRegistry;
        [SerializeField] private Transform _playerTransform;

        private void OnEnable()
        {
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
        }

        private void Start()
        {
            RebindAllReferences();
        }

        private void OnSceneTransitionStarted(SceneTransitionStartedEvent evt)
        {
            if (evt.SourceSceneName == "FarmScene")
            {
                FarmSceneRuntimeStateCache.Capture(_farmPlotRegistry, _treeRegistry, _itemPickupRegistry);
            }
        }

        private void RebindAllReferences()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogWarning("FarmSceneRuntimeReferenceInstaller: GameBootstrap.Instance is null, cannot rebind references.", this);
                return;
            }

            var playerManager = bootstrap.PlayerManager;
            var inventoryManager = bootstrap.InventoryManager;
            var hungerManager = bootstrap.HungerManager;
            var timeManager = bootstrap.TimeManager;
            var saveManager = bootstrap.SaveManager;
            var craftingManager = bootstrap.CraftingManager;
            var economyManager = bootstrap.EconomyManager;

            if (inventoryManager != null && _farmPlots != null)
            {
                foreach (var plot in _farmPlots)
                {
                    if (plot != null)
                    {
                        plot.RebindInventoryManager(inventoryManager);
                    }
                }
            }

            if (inventoryManager != null && _treeNodes != null)
            {
                foreach (var tree in _treeNodes)
                {
                    if (tree != null)
                    {
                        tree.RebindInventoryManager(inventoryManager);
                    }
                }
            }

            if (inventoryManager != null && _fishingSpot != null)
            {
                _fishingSpot.RebindInventoryManager(inventoryManager);
            }

            if (inventoryManager != null && playerManager != null && _seedShopPoint != null)
            {
                _seedShopPoint.RebindRuntimeManagers(inventoryManager, playerManager);
            }

            if (craftingManager != null && _craftingPoint != null)
            {
                _craftingPoint.RebindCraftingManager(craftingManager);
            }

            if (saveManager != null)
            {
                saveManager.RebindSceneReferences(_farmPlotRegistry, _treeRegistry, _itemPickupRegistry, _playerTransform);
                if (inventoryManager != null && playerManager != null && hungerManager != null && timeManager != null)
                {
                    saveManager.RebindRuntimeManagers(playerManager, inventoryManager, hungerManager, timeManager);
                }
            }

            bool restored = FarmSceneRuntimeStateCache.TryRestore(_farmPlotRegistry, _treeRegistry, _itemPickupRegistry);
            Debug.Log($"FarmSceneRuntimeReferenceInstaller rebound runtime refs and restored cached farm state: restored={restored}");
        }
    }
}
