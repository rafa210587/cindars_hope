using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Craft;
using CindarsHope.Economy;
using CindarsHope.Farm;
using CindarsHope.Interaction;
using CindarsHope.Save;
using CindarsHope.UI;
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

        // Campo legado mantido com o nome serializado atual da FarmScene/gerador.
        // Apesar do nome, este slot referencia o SellPoint antigo da FarmScene, não o SellAllPoint event-driven da TownScene.
        [SerializeField] private SellPoint _sellAllPoint;

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
            var staminaManager = bootstrap.StaminaManager;
            var timeManager = bootstrap.TimeManager;
            // arch: quebra do par mutuo Core|Save (2026-07-15) — bootstrap.SaveManager agora retorna
            // a porta ISaveRuntime; este installer (SceneManagement, fora do par cortado) resolve o
            // tipo concreto por cast local para os Rebind* cross-modulo (nao portaveis).
            var saveManager = bootstrap.SaveManager as SaveManager;
            // arch: Core|Craft (spec_arch_core_craft_cycle_reduction_v32) — CraftingManager
            // self-registra via static Instance; GameBootstrap nao segura mais essa ref.
            var craftingManager = CraftingManager.Instance;

            if (inventoryManager != null && _farmPlots != null)
            {
                foreach (var plot in _farmPlots)
                {
                    if (plot != null)
                    {
                        plot.RebindInventoryManager(inventoryManager);
                        plot.RebindStaminaManager(staminaManager);
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
                        tree.RebindStaminaManager(staminaManager);
                    }
                }
            }

            if (inventoryManager != null && _fishingSpot != null)
            {
                _fishingSpot.RebindInventoryManager(inventoryManager);
                _fishingSpot.RebindStaminaManager(staminaManager);
            }

            if (inventoryManager != null && playerManager != null && _seedShopPoint != null)
            {
                _seedShopPoint.RebindRuntimeManagers(inventoryManager, playerManager);
            }

            if (inventoryManager != null && playerManager != null && _sellAllPoint != null)
            {
                _sellAllPoint.RebindRuntimeManagers(inventoryManager, playerManager);
            }

            if (craftingManager != null && _craftingPoint != null)
            {
                _craftingPoint.RebindCraftingManager(craftingManager);
            }

            var craftingRuntime = bootstrap.GetComponent<CraftingRuntime>();
            if (craftingRuntime != null)
            {
                craftingRuntime.RebindStaminaManager(staminaManager);
            }

            if (saveManager != null)
            {
                saveManager.RebindSceneReferences(_farmPlotRegistry, _treeRegistry, _itemPickupRegistry, _playerTransform);
                if (inventoryManager != null && playerManager != null && hungerManager != null && timeManager != null)
                {
                    saveManager.RebindRuntimeManagers(playerManager, inventoryManager, hungerManager, timeManager);
                }

                // arch: Core|Economy (spec_arch_core_economy_cycle_reduction_v33) — ShopManager
                // self-registra via static Instance; GameBootstrap nao segura mais essa ref.
                // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — idem para
                // EquipmentManager, via EquipmentManager.Instance.
                saveManager.RebindOptionalRuntimeManagers(CindarsHope.Equipment.EquipmentManager.Instance, bootstrap.PlayerProgressionManager, bootstrap.GameTimeManager, staminaManager, bootstrap.StatusEffectManager, CindarsHope.Skills.SkillTreeManager.Instance, ShopManager.Instance);
            }

            var interactionSystem = _playerTransform != null ? _playerTransform.GetComponent<InteractionSystem>() : null;
            DebugHud.RebindExisting(playerManager, inventoryManager, hungerManager, staminaManager, bootstrap.StatusEffectManager, interactionSystem, timeManager, saveManager);

            var restored = FarmSceneRuntimeStateCache.TryRestore(_farmPlotRegistry, _treeRegistry, _itemPickupRegistry);
            var plotCount = _farmPlotRegistry?.Plots?.Count ?? 0;
            var treeCount = _treeRegistry?.Trees?.Count ?? 0;
            Debug.Log($"FarmSceneRuntimeReferenceInstaller: rebound {plotCount} plots, {treeCount} trees. State cache restored: {restored}.");
        }
    }
}
