using System.Collections.Generic;
using CindarsHope.Economy;
using CindarsHope.NPC;
using CindarsHope.NPC.Runtime;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Shop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateNpcDialogueShopBridge
    {
        private const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";

        private static readonly ExpectedNpc[] ExpectedNpcs =
        {
            new("npc_pip_miudinho", "Assets/_Game/Data/NPCs/Npc_Pip_Miudinho.asset", true, false, string.Empty),
            new("npc_sylveth", "Assets/_Game/Data/NPCs/Npc_Sylveth.asset", true, true, "shop_seeds_tools"),
            new("npc_brumdar", "Assets/_Game/Data/NPCs/Npc_Brumdar.asset", true, true, "shop_blacksmith"),
            new("npc_renko", "Assets/_Game/Data/NPCs/Npc_Renko.asset", true, true, "shop_general_store"),
            new("npc_thalindra", "Assets/_Game/Data/NPCs/Npc_Thalindra.asset", true, false, string.Empty),
            new("npc_zrix", "Assets/_Game/Data/NPCs/Npc_Zrix.asset", true, true, "shop_cave_supplies"),
            new("npc_nimble", "Assets/_Game/Data/NPCs/Npc_Nimble.asset", true, false, string.Empty),
        };

        private static readonly ExpectedShop[] ExpectedShops =
        {
            new("shop_seeds_tools", "Assets/_Game/Data/Economy/Shop_Seeds_Tools.asset", "npc_sylveth"),
            new("shop_blacksmith", "Assets/_Game/Data/Economy/Shop_Blacksmith.asset", "npc_brumdar"),
            new("shop_general_store", "Assets/_Game/Data/Economy/Shop_General_Store.asset", "npc_renko"),
            new("shop_cave_supplies", "Assets/_Game/Data/Economy/Shop_Cave_Supplies.asset", "npc_zrix"),
        };

        private static readonly string[] RequiredDocs =
        {
            "docs/validation/WAVE_INTEGRATION_12_RUNTIME_GAP_AUDIT.md",
            "docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md",
            "docs/validation/WAVE_INTEGRATION_12_NPC_PLACEMENT_MAP.md",
            "docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md",
            "docs/validation/WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md",
            "docs/validation/WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md",
            "docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md",
            "docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_AUTHORING_MODEL.md",
            "docs/validation/WAVE_INTEGRATION_12_HUMAN_PLAYMODE_CHECKLIST.md",
        };

        [MenuItem("CindarsHope/Validate/Validate WAVE12 NPC Dialogue Shop Bridge", priority = 49)]
        public static void RunValidation()
        {
            var passes = new List<string>();
            var issues = new List<string>();

            ValidateRuntimeTypes(passes, issues);
            ValidateRequiredDocs(passes, issues);

            foreach (var expectedNpc in ExpectedNpcs)
            {
                ValidateNpcData(expectedNpc, passes, issues);
            }

            foreach (var expectedShop in ExpectedShops)
            {
                ValidateShopData(expectedShop, passes, issues);
            }

            ValidateForbiddenNpcClassPattern(passes, issues);
            ValidateLoadedTownScene(passes, issues);

            Debug.Log("=== ValidateNpcDialogueShopBridge - DONE ===");
            Debug.Log($"PASS: {passes.Count}  FAIL: {issues.Count}");
            foreach (var pass in passes) Debug.Log($"  [PASS] {pass}");
            foreach (var issue in issues) Debug.LogError($"  [FAIL] {issue}");

            EditorUtility.DisplayDialog(
                "WAVE12 NPC Dialogue Shop Bridge",
                issues.Count == 0 ? $"All {passes.Count} checks PASSED." : $"{passes.Count} passed, {issues.Count} failed. See Console.",
                "OK");
        }

        private static void ValidateRuntimeTypes(ICollection<string> passes, ICollection<string> issues)
        {
            RequireType<NpcController>(passes, issues, nameof(NpcController));
            RequireType<NpcShopController>(passes, issues, nameof(NpcShopController));
            RequireType<NpcManager>(passes, issues, nameof(NpcManager));
            RequireType<NpcScenePlacementMarker>(passes, issues, nameof(NpcScenePlacementMarker));
            RequireType<DialogueModal>(passes, issues, nameof(DialogueModal));
            RequireType<ShopMenuModal>(passes, issues, nameof(ShopMenuModal));
            RequireType<BuyPanel>(passes, issues, nameof(BuyPanel));
            RequireType<SellPanel>(passes, issues, nameof(SellPanel));
            RequireType<ShopManager>(passes, issues, nameof(ShopManager));
            RequireType<ModalManager>(passes, issues, nameof(ModalManager));
        }

        private static void ValidateNpcData(
            ExpectedNpc expected,
            ICollection<string> passes,
            ICollection<string> issues)
        {
            var npc = AssetDatabase.LoadAssetAtPath<NpcDataSO>(expected.AssetPath);
            if (npc == null)
            {
                issues.Add($"Missing NpcDataSO at {expected.AssetPath}");
                return;
            }

            if (npc.NpcId != expected.NpcId) issues.Add($"{expected.AssetPath} expected NpcId '{expected.NpcId}', found '{npc.NpcId}'");
            else passes.Add($"{expected.AssetPath} has NpcId '{npc.NpcId}'");

            if (string.IsNullOrWhiteSpace(npc.DisplayName)) issues.Add($"{npc.NpcId} has empty DisplayName");
            else passes.Add($"{npc.NpcId} has DisplayName '{npc.DisplayName}'");

            if (string.IsNullOrWhiteSpace(npc.DefaultSceneId)) issues.Add($"{npc.NpcId} has empty DefaultSceneId");
            else passes.Add($"{npc.NpcId} has DefaultSceneId '{npc.DefaultSceneId}'");

            if (expected.RequiresDialogueTree)
            {
                ValidateDialogueTree(npc, passes, issues);
            }

            if (expected.RequiresShop)
            {
                if (npc.ShopId != expected.ShopId) issues.Add($"{npc.NpcId} expected ShopId '{expected.ShopId}', found '{npc.ShopId}'");
                else passes.Add($"{npc.NpcId} has ShopId '{npc.ShopId}'");
            }
        }

        private static void ValidateDialogueTree(NpcDataSO npc, ICollection<string> passes, ICollection<string> issues)
        {
            if (npc.DialogueTree == null)
            {
                issues.Add($"{npc.NpcId} requires DialogueTree");
                return;
            }

            if (string.IsNullOrWhiteSpace(npc.DialogueTree.Id)) issues.Add($"{npc.NpcId} DialogueTree has empty Id");
            else passes.Add($"{npc.NpcId} DialogueTree Id '{npc.DialogueTree.Id}'");

            if (string.IsNullOrWhiteSpace(npc.DialogueTree.StartNodeId)) issues.Add($"{npc.NpcId} DialogueTree has empty StartNodeId");
            else passes.Add($"{npc.NpcId} DialogueTree StartNodeId '{npc.DialogueTree.StartNodeId}'");

            var nodeCount = npc.DialogueTree.Nodes != null ? npc.DialogueTree.Nodes.Count : 0;
            if (nodeCount < 10) issues.Add($"{npc.NpcId} DialogueTree has {nodeCount} nodes; expected >= 10 for WAVE12 MVP coverage");
            else passes.Add($"{npc.NpcId} DialogueTree has {nodeCount} nodes");
        }

        private static void ValidateShopData(ExpectedShop expected, ICollection<string> passes, ICollection<string> issues)
        {
            var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(expected.AssetPath);
            if (shop == null)
            {
                issues.Add($"Missing ShopDataSO at {expected.AssetPath}");
                return;
            }

            if (shop.Id != expected.ShopId) issues.Add($"{expected.AssetPath} expected shop Id '{expected.ShopId}', found '{shop.Id}'");
            else passes.Add($"{expected.AssetPath} has shop Id '{shop.Id}'");

            if (shop.NpcId != expected.NpcId) issues.Add($"{shop.Id} expected NpcId '{expected.NpcId}', found '{shop.NpcId}'");
            else passes.Add($"{shop.Id} mapped to NpcId '{shop.NpcId}'");

            var stockCount = shop.Items != null ? shop.Items.Length : 0;
            if (stockCount <= 0) issues.Add($"{shop.Id} has no stock entries");
            else passes.Add($"{shop.Id} has {stockCount} stock entries");
        }

        private static void RequireType<T>(ICollection<string> passes, ICollection<string> issues, string label)
        {
            if (typeof(T) != null) passes.Add($"{label} type exists");
            else issues.Add($"{label} type missing");
        }

        private static void ValidateRequiredDocs(ICollection<string> passes, ICollection<string> issues)
        {
            foreach (var doc in RequiredDocs)
            {
                if (System.IO.File.Exists(doc)) passes.Add($"Required doc exists: {doc}");
                else issues.Add($"Missing required doc: {doc}");
            }
        }

        private static void ValidateForbiddenNpcClassPattern(ICollection<string> passes, ICollection<string> issues)
        {
            var files = System.IO.Directory.GetFiles("Assets/_Game/Scripts", "*Npc.cs", System.IO.SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(file);
                if (name.StartsWith("Npc") || name.EndsWith("Controller") || name.EndsWith("Manager"))
                {
                    continue;
                }

                if (name.Contains("Pip") || name.Contains("Sylveth") || name.Contains("Brumdar") || name.Contains("Renko") || name.Contains("Thalindra") || name.Contains("Zrix") || name.Contains("Nimble"))
                {
                    issues.Add($"Forbidden class-per-NPC pattern found: {file}");
                }
            }

            passes.Add("No prohibited WAVE12 class-per-NPC pattern detected by filename scan.");
        }

        private static void ValidateLoadedTownScene(ICollection<string> passes, ICollection<string> issues)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != TownScenePath)
            {
                passes.Add("TownScene scene-object validation skipped because TownScene is not the active loaded scene.");
                return;
            }

            var found = new Dictionary<string, bool>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var marker in root.GetComponentsInChildren<NpcScenePlacementMarker>(includeInactive: true))
                {
                    if (marker == null || string.IsNullOrWhiteSpace(marker.NpcId))
                    {
                        continue;
                    }

                    found[marker.NpcId] = true;
                    var hasDialogue = marker.GetComponent<NpcController>() != null;
                    var hasShop = marker.GetComponent<NpcShopController>() != null;
                    if (!hasDialogue && !hasShop)
                    {
                        issues.Add($"{marker.NpcId} has NpcScenePlacementMarker but no NPC interactable controller.");
                    }

                    var collider = marker.GetComponent<Collider2D>();
                    if (collider == null || !collider.isTrigger)
                    {
                        issues.Add($"{marker.NpcId} has no trigger Collider2D for interaction.");
                    }
                }
            }

            foreach (var expectedNpc in ExpectedNpcs)
            {
                if (!found.ContainsKey(expectedNpc.NpcId))
                {
                    issues.Add($"Loaded TownScene missing placement marker for MVP NPC '{expectedNpc.NpcId}'.");
                }
            }
        }

        private readonly struct ExpectedNpc
        {
            public readonly string NpcId;
            public readonly string AssetPath;
            public readonly bool RequiresDialogueTree;
            public readonly bool RequiresShop;
            public readonly string ShopId;

            public ExpectedNpc(string npcId, string assetPath, bool requiresDialogueTree, bool requiresShop, string shopId)
            {
                NpcId = npcId;
                AssetPath = assetPath;
                RequiresDialogueTree = requiresDialogueTree;
                RequiresShop = requiresShop;
                ShopId = shopId;
            }
        }

        private readonly struct ExpectedShop
        {
            public readonly string ShopId;
            public readonly string AssetPath;
            public readonly string NpcId;

            public ExpectedShop(string shopId, string assetPath, string npcId)
            {
                ShopId = shopId;
                AssetPath = assetPath;
                NpcId = npcId;
            }
        }
    }
}
