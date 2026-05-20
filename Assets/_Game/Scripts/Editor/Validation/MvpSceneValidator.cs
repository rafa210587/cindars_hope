using CindarsHope.Combat;
using CindarsHope.Cave;
using CindarsHope.Cave.Resources;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Economy;
using CindarsHope.Farm;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.NPC;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using CindarsHope.UI;
using CindarsHope.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class MvpSceneValidator
    {
        [MenuItem("CindarsHope/Validate/Validate Farm Town MVP")]
        public static void ValidateScenes()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var sceneName = scene.name;

            var passed = true;

            if (sceneName == "FarmScene")
            {
                passed = ValidateFarmScene();
            }
            else if (sceneName == "TownScene")
            {
                passed = ValidateTownScene();
            }
            else
            {
                Debug.LogError($"MvpSceneValidator: Active scene '{sceneName}' is not FarmScene or TownScene. Cannot validate.");
                passed = false;
            }

            if (passed)
            {
                Debug.Log("Validate Farm Town MVP passed.");
            }
            else
            {
                Debug.LogError("Validate Farm Town MVP failed. See errors above.");
            }
        }

        [MenuItem("CindarsHope/Validate/Validate Cave MVP")]
        public static void ValidateCaveSceneFromMenu()
        {
            var sceneName = EditorSceneManager.GetActiveScene().name;
            if (sceneName != "CaveScene")
            {
                Debug.LogError($"MvpSceneValidator: Active scene '{sceneName}' is not CaveScene.");
                return;
            }

            if (ValidateCaveScene())
            {
                Debug.Log("Validate Cave MVP passed.");
            }
            else
            {
                Debug.LogError("Validate Cave MVP failed. See errors above.");
            }
        }

        [MenuItem("CindarsHope/Validate/Validate All MVP Scenes")]
        public static void ValidateAllMvpScenes()
        {
            var sceneName = EditorSceneManager.GetActiveScene().name;
            bool passed = sceneName switch
            {
                "FarmScene" => ValidateFarmScene(),
                "TownScene" => ValidateTownScene(),
                "CaveScene" => ValidateCaveScene(),
                _ => false
            };

            if (passed)
            {
                Debug.Log($"Validate All MVP Scenes passed for active scene '{sceneName}'.");
            }
            else
            {
                Debug.LogError($"Validate All MVP Scenes failed for active scene '{sceneName}'.");
            }
        }

        private static bool ValidateFarmScene()
        {
            var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            var passed = true;

            var bootstrap = FindComponent<GameBootstrap>(rootObjects);
            if (bootstrap == null)
            {
                Debug.LogError("MvpSceneValidator: GameBootstrap not found in FarmScene.");
                passed = false;
            }
            else
            {
                if (bootstrap.PlayerManager == null)
                    { Debug.LogError("MvpSceneValidator: GameBootstrap missing PlayerManager."); passed = false; }
                if (bootstrap.InventoryManager == null)
                    { Debug.LogError("MvpSceneValidator: GameBootstrap missing InventoryManager."); passed = false; }
                if (bootstrap.TimeManager == null)
                    { Debug.LogError("MvpSceneValidator: GameBootstrap missing TimeManager."); passed = false; }
                if (bootstrap.SaveManager == null)
                    { Debug.LogError("MvpSceneValidator: GameBootstrap missing SaveManager."); passed = false; }
                if (bootstrap.HungerManager == null)
                    { Debug.LogError("MvpSceneValidator: GameBootstrap missing HungerManager."); passed = false; }
                if (bootstrap.CraftingManager == null)
                    { Debug.LogError("MvpSceneValidator: GameBootstrap missing CraftingManager."); passed = false; }
                if (bootstrap.EconomyManager == null)
                    { Debug.LogError("MvpSceneValidator: GameBootstrap missing EconomyManager."); passed = false; }
            }

            if (FindComponent<DebugHud>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: DebugHud not found in FarmScene."); passed = false; }

            var playerTransform = FindComponent<PlayerController>(rootObjects)?.GetComponent<Transform>();
            if (playerTransform == null)
                { Debug.LogError("MvpSceneValidator: Player GameObject not found in FarmScene."); passed = false; }

            if (FindComponent<FarmSceneRuntimeReferenceInstaller>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: FarmSceneRuntimeReferenceInstaller not found in FarmScene."); passed = false; }

            if (FindComponent<FarmPlotRegistry>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: FarmPlotRegistry not found in FarmScene."); passed = false; }

            if (FindComponent<TreeRegistry>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: TreeRegistry not found in FarmScene."); passed = false; }

            if (FindComponent<ItemPickupRegistry>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: ItemPickupRegistry not found in FarmScene."); passed = false; }

            var farmPlots = Object.FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            if (farmPlots.Length == 0)
                { Debug.LogError("MvpSceneValidator: No FarmPlot found in FarmScene."); passed = false; }

            var treeNodes = Object.FindObjectsByType<TreeNode>(FindObjectsSortMode.None);
            if (treeNodes.Length == 0)
                { Debug.LogError("MvpSceneValidator: No TreeNode found in FarmScene."); passed = false; }

            if (FindComponent<FishingSpot>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: FishingSpot not found in FarmScene."); passed = false; }

            if (FindComponent<SeedShopPoint>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: SeedShopPoint not found in FarmScene."); passed = false; }

            if (FindComponent<SellPoint>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: SellPoint not found in FarmScene."); passed = false; }

            if (FindComponent<CraftingPoint>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: CraftingPoint not found in FarmScene."); passed = false; }

            var portalToTown = FindPortalTo(rootObjects, "TownScene");
            if (portalToTown == null)
                { Debug.LogError("MvpSceneValidator: Portal to TownScene not found in FarmScene."); passed = false; }

            return passed;
        }

        private static bool ValidateTownScene()
        {
            var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            var passed = true;

            var bootstrap = FindComponent<GameBootstrap>(rootObjects);
            if (bootstrap == null)
            {
                Debug.LogError("MvpSceneValidator: GameBootstrap not found in TownScene.");
                passed = false;
            }
            else
            {
                if (bootstrap.EconomyManager == null)
                    { Debug.LogError("MvpSceneValidator: GameBootstrap missing EconomyManager."); passed = false; }
            }

            if (FindComponent<DebugHud>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: DebugHud not found in TownScene."); passed = false; }

            var playerTransform = FindComponent<PlayerController>(rootObjects)?.GetComponent<Transform>();
            if (playerTransform == null)
                { Debug.LogError("MvpSceneValidator: Player GameObject not found in TownScene."); passed = false; }

            if (FindComponent<TownSceneRuntimeReferenceInstaller>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: TownSceneRuntimeReferenceInstaller not found in TownScene."); passed = false; }

            if (FindComponent<SceneSpawnInstaller>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: SceneSpawnInstaller not found in TownScene."); passed = false; }

            var portalToFarm = FindPortalTo(rootObjects, "FarmScene");
            if (portalToFarm == null)
                { Debug.LogError("MvpSceneValidator: Portal to FarmScene not found in TownScene."); passed = false; }

            if (FindComponent<NpcTalkPoint>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: NPC not found in TownScene."); passed = false; }

            var buyPoints = Object.FindObjectsByType<BuyItemPoint>(FindObjectsSortMode.None);
            if (buyPoints.Length < 2)
                { Debug.LogError("MvpSceneValidator: Less than 2 BuyItemPoint found in TownScene (expected at least seed_wheat and seed_carrot)."); passed = false; }

            if (FindComponent<SellAllPoint>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: SellAllPoint not found in TownScene."); passed = false; }

            return passed;
        }

        private static bool ValidateCaveScene()
        {
            var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            var passed = true;

            var bootstrap = FindComponent<GameBootstrap>(rootObjects);
            if (bootstrap == null)
                { Debug.LogError("MvpSceneValidator: GameBootstrap not found in CaveScene."); passed = false; }

            if (FindComponent<PlayerController>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: PlayerController not found in CaveScene."); passed = false; }

            if (FindComponent<PlayerAttackController>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: PlayerAttackController not found in CaveScene."); passed = false; }

            if (FindComponent<InteractionSystem>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: InteractionSystem not found in CaveScene."); passed = false; }

            if (FindComponent<DebugHud>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: DebugHud not found in CaveScene."); passed = false; }

            if (FindComponent<CaveSceneRuntimeReferenceInstaller>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: CaveSceneRuntimeReferenceInstaller not found in CaveScene."); passed = false; }

            if (FindComponent<SaveInput>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: SaveInput not found in CaveScene."); passed = false; }

            if (FindPortalTo(rootObjects, "FarmScene") == null)
                { Debug.LogError("MvpSceneValidator: Portal to FarmScene not found in CaveScene."); passed = false; }

            if (FindComponent<EnemyHealth>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: EnemyHealth not found in CaveScene."); passed = false; }

            if (FindComponent<EnemyChaseController>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: EnemyChaseController not found in CaveScene."); passed = false; }

            if (FindComponent<EnemyContactDamage>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: EnemyContactDamage not found in CaveScene."); passed = false; }

            if (FindComponent<EnemyDropSpawner>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: EnemyDropSpawner not found in CaveScene."); passed = false; }

            if (FindComponent<CaveRunManager>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: CaveRunManager not found in CaveScene."); passed = false; }

            if (FindComponent<CaveLevelRuntimeController>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: CaveLevelRuntimeController not found in CaveScene."); passed = false; }

            var resourceNodes = Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
            if (resourceNodes.Length < 3)
                { Debug.LogError("MvpSceneValidator: Less than 3 ResourceNode debug instances found in CaveScene."); passed = false; }

            return passed;
        }

        private static T FindComponent<T>(GameObject[] rootObjects) where T : Component
        {
            foreach (var root in rootObjects)
            {
                var component = root.GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                    return component;
            }
            return null;
        }

        private static GameObject FindPortalTo(GameObject[] rootObjects, string targetSceneName)
        {
            foreach (var root in rootObjects)
            {
                var portals = root.GetComponentsInChildren<ScenePortal>(includeInactive: true);
                foreach (var portal in portals)
                {
                    if (portal.TargetSceneName == targetSceneName)
                        return portal.gameObject;
                }
            }
            return null;
        }
    }
}
