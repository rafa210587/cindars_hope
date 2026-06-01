using CindarsHope.Combat;
using CindarsHope.Cave;
using CindarsHope.Cave.Resources;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Economy;
using CindarsHope.Farm;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.NPC;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using CindarsHope.UI;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Crafting;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Shop;
using CindarsHope.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CaveResourceNode = CindarsHope.Cave.Resources.ResourceNode;

namespace CindarsHope.Editor.Validation
{
    public static class MvpSceneValidator
    {
        [MenuItem("CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP")]
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

        [MenuItem("CindarsHope/Advanced/Legacy/Validate/Validate Cave MVP")]
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

        [MenuItem("CindarsHope/Advanced/Legacy/Validate/Validate All MVP Scenes")]
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

        public static void ValidateSpec06Scenes()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/TownScene.unity");
            if (!ValidateTownScene())
            {
                throw new System.InvalidOperationException("SPEC 06 validation failed for TownScene.");
            }

            EditorSceneManager.OpenScene("Assets/_Game/Scenes/FarmScene.unity");
            if (!ValidateFarmScene())
            {
                throw new System.InvalidOperationException("SPEC 06 validation failed for FarmScene.");
            }

            Debug.Log("SPEC 06 scene validation passed for TownScene and FarmScene.");
        }

        public static void ValidateSpec07Scene()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/FarmScene.unity");
            if (!ValidateFarmScene())
            {
                throw new System.InvalidOperationException("SPEC 07 validation failed for FarmScene.");
            }

            Debug.Log("SPEC 07 scene validation passed for FarmScene.");
        }

        public static void ValidateSpec08Scene()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/TownScene.unity");
            if (!ValidateTownScene() || !ValidateSpec08Assets())
            {
                throw new System.InvalidOperationException("SPEC 08 validation failed for TownScene or NPC dialogue assets.");
            }

            Debug.Log("SPEC 08 scene and NPC dialogue validation passed for TownScene.");
        }

        public static void ValidateSpec09Scenes()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/FarmScene.unity");
            if (!ValidateFarmScene())
            {
                throw new System.InvalidOperationException("SPEC 09 validation failed for FarmScene.");
            }

            EditorSceneManager.OpenScene("Assets/_Game/Scenes/TownScene.unity");
            if (!ValidateTownScene())
            {
                throw new System.InvalidOperationException("SPEC 09 validation failed for TownScene.");
            }

            EditorSceneManager.OpenScene("Assets/_Game/Scenes/CaveScene.unity");
            if (!ValidateCaveScene())
            {
                throw new System.InvalidOperationException("SPEC 09 validation failed for CaveScene.");
            }

            if (AssetDatabase.LoadAssetAtPath<PlayerNeedsBalanceSO>("Assets/_Game/Data/Config/PlayerNeedsBalance.asset") == null
                || AssetDatabase.LoadAssetAtPath<GameTimeBalanceSO>("Assets/_Game/Data/Config/GameTimeBalance.asset") == null)
            {
                throw new System.InvalidOperationException("SPEC 09 balance assets are missing.");
            }

            Debug.Log("SPEC 09 scene validation passed for hunger, stamina, status and game time wiring.");
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
                passed &= ValidateSpec09Bootstrap(bootstrap);
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

            var farmPlots = Object.FindObjectsByType<FarmPlot>();
            if (farmPlots.Length == 0)
                { Debug.LogError("MvpSceneValidator: No FarmPlot found in FarmScene."); passed = false; }

            var treeNodes = Object.FindObjectsByType<TreeNode>();
            if (treeNodes.Length == 0)
                { Debug.LogError("MvpSceneValidator: No TreeNode found in FarmScene."); passed = false; }

            if (FindComponent<FishingSpot>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: FishingSpot not found in FarmScene."); passed = false; }

            if (FindComponent<SeedShopPoint>(rootObjects) != null || FindComponent<SellPoint>(rootObjects) != null)
                { Debug.LogError("MvpSceneValidator: Legacy farm purchase/sale points must be disabled after SPEC 06."); passed = false; }

            if (FindComponent<CraftingRuntime>(rootObjects) == null
                || FindComponent<ModalManager>(rootObjects) == null
                || FindComponent<CraftingModal>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: SPEC 07 runtime/modal wiring is missing in FarmScene."); passed = false; }

            var craftingStations = Object.FindObjectsByType<CraftingPoint>();
            if (craftingStations.Length != 3
                || !HasStation(craftingStations, "farm_workbench_01", WorkshopType.Workbench)
                || !HasStation(craftingStations, "farm_forge_01", WorkshopType.Forge)
                || !HasStation(craftingStations, "farm_cooking_01", WorkshopType.CookingStation))
                { Debug.LogError("MvpSceneValidator: SPEC 07 requires Workbench, Forge and CookingStation with stable farm IDs."); passed = false; }

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
                passed &= ValidateSpec09Bootstrap(bootstrap);
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

            if (Object.FindObjectsByType<NpcShopController>().Length != 2)
                { Debug.LogError("MvpSceneValidator: TownScene requires exactly two shopkeeper NPC controllers."); passed = false; }
            if (FindComponent<PipReceptionController>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: Pip reception controller not found in TownScene."); passed = false; }
            var dialogueNpcs = Object.FindObjectsByType<NpcController>();
            if (!HasDialogueNpc(dialogueNpcs, "npc_pip_miudinho")
                || !HasDialogueNpc(dialogueNpcs, "npc_vaalara_wanderer_01")
                || FindComponent<NpcWanderer>(rootObjects) == null
                || FindComponent<NpcManager>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: SPEC 08 requires Pip dialogue, bounded wanderer and NpcManager wiring."); passed = false; }
            if (FindComponent<ShopManager>(rootObjects) == null || FindComponent<ModalManager>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: ShopManager or ModalManager not found in TownScene."); passed = false; }
            if (FindComponent<DialogueModal>(rootObjects) == null
                || FindComponent<ShopMenuModal>(rootObjects) == null
                || FindComponent<BuyPanel>(rootObjects) == null
                || FindComponent<SellPanel>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: TownScene is missing one or more SPEC 06 shop UI modals."); passed = false; }
            if (Object.FindObjectsByType<BuyItemPoint>().Length != 0 || FindComponent<SellAllPoint>(rootObjects) != null)
                { Debug.LogError("MvpSceneValidator: Legacy Town commerce points must not compete with NPC shops."); passed = false; }

            return passed;
        }

        private static bool ValidateSpec08Assets()
        {
            var pip = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Pip_Miudinho.asset");
            var weapons = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Shop_Weapons_Armor.asset");
            var seeds = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Shop_Seeds_Tools.asset");
            var wanderer = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Vaalara_Wanderer_01.asset");
            if (pip == null || pip.DialogueTree == null
                || weapons == null || weapons.ShopId != "shop_weapons_armor"
                || seeds == null || seeds.ShopId != "shop_seeds_tools"
                || wanderer == null || wanderer.DialogueTree == null
                || wanderer.MovementMode != NpcMovementMode.RandomWander)
            {
                Debug.LogError("MvpSceneValidator: SPEC 08 NPC data assets are missing or invalid.");
                return false;
            }

            var pipStart = pip.DialogueTree.GetNodeById(pip.DialogueTree.StartNodeId);
            var wandererStart = wanderer.DialogueTree.GetNodeById(wanderer.DialogueTree.StartNodeId);
            if (pipStart == null || pipStart.Choices == null || pipStart.Choices.Count < 3
                || wandererStart == null || wandererStart.RandomLinePool == null || wandererStart.RandomLinePool.Count < 3)
            {
                Debug.LogError("MvpSceneValidator: Pip orientation choices or wanderer lore lines do not meet SPEC 08.");
                return false;
            }

            foreach (var choice in pipStart.Choices)
            {
                if (choice.ActionType == DialogueActionType.OpenShop)
                {
                    Debug.LogError("MvpSceneValidator: Pip must not open a shop.");
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateCaveScene()
        {
            var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            var passed = true;

            var bootstrap = FindComponent<GameBootstrap>(rootObjects);
            if (bootstrap == null)
                { Debug.LogError("MvpSceneValidator: GameBootstrap not found in CaveScene."); passed = false; }
            else
            {
                passed &= ValidateSpec09Bootstrap(bootstrap);
                passed &= ValidateSpec11CombatDatabases(bootstrap);
            }

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

            if (FindComponent<EnemyDropSpawner>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: EnemyDropSpawner not found in CaveScene."); passed = false; }

            if (FindComponent<CaveRunManager>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: CaveRunManager not found in CaveScene."); passed = false; }

            if (FindComponent<CaveLevelRuntimeController>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: CaveLevelRuntimeController not found in CaveScene."); passed = false; }

            if (FindComponent<CaveRuntimeMaterializer>(rootObjects) == null
                || FindComponent<CaveEnemySpawner>(rootObjects) == null)
                { Debug.LogError("MvpSceneValidator: Cave procedural materializer or enemy spawner not found in CaveScene."); passed = false; }

            return passed;
        }

        private static bool ValidateSpec11CombatDatabases(GameBootstrap bootstrap)
        {
            var passed = true;
            if (bootstrap.ItemDatabase == null)
                { Debug.LogError("MvpSceneValidator: SPEC_11 requires ItemDatabase wired on GameBootstrap."); passed = false; }
            if (bootstrap.WeaponDatabase == null)
                { Debug.LogError("MvpSceneValidator: SPEC_11 requires WeaponDatabase wired on GameBootstrap."); passed = false; }
            if (bootstrap.SpellDatabase == null)
                { Debug.LogError("MvpSceneValidator: SPEC_11 requires SpellDatabase wired on GameBootstrap."); passed = false; }
            return passed;
        }

        private static bool ValidateSpec09Bootstrap(GameBootstrap bootstrap)
        {
            var passed = true;
            if (bootstrap.StaminaManager == null || bootstrap.GameTimeManager == null || bootstrap.StatusEffectManager == null)
            {
                Debug.LogError("MvpSceneValidator: SPEC 09 requires StaminaManager, GameTimeManager and StatusEffectManager on GameBootstrap.");
                return false;
            }

            var stamina = new SerializedObject(bootstrap.StaminaManager);
            if (stamina.FindProperty("_playerNeedsBalance").objectReferenceValue == null
                || stamina.FindProperty("_hungerManager").objectReferenceValue == null)
            {
                Debug.LogError("MvpSceneValidator: SPEC 09 StaminaManager dependencies are not wired.");
                passed = false;
            }

            var gameTime = new SerializedObject(bootstrap.GameTimeManager);
            if (gameTime.FindProperty("_timeBalance").objectReferenceValue == null
                || gameTime.FindProperty("_timeManager").objectReferenceValue == null
                || gameTime.FindProperty("_modalManager").objectReferenceValue == null)
            {
                Debug.LogError("MvpSceneValidator: SPEC 09 GameTimeManager dependencies are not wired.");
                passed = false;
            }

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

        private static bool HasStation(CraftingPoint[] stations, string id, WorkshopType type)
        {
            foreach (var station in stations)
            {
                if (station.StationInstanceId == id && station.StationType == type)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDialogueNpc(NpcController[] npcs, string npcId)
        {
            foreach (var npc in npcs)
            {
                if (npc.NpcData != null && npc.NpcData.NpcId == npcId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
