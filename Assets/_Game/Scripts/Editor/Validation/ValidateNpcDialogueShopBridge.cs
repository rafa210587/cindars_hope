using System.Collections.Generic;
using CindarsHope.Economy;
using CindarsHope.NPC;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Shop;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateNpcDialogueShopBridge
    {
        private const string PipNpcPath = "Assets/_Game/Data/NPCs/Npc_Pip_Miudinho.asset";
        private const string WandererNpcPath = "Assets/_Game/Data/NPCs/Npc_Vaalara_Wanderer_01.asset";
        private const string SeedsNpcPath = "Assets/_Game/Data/NPCs/Npc_Shop_Seeds_Tools.asset";
        private const string WeaponsNpcPath = "Assets/_Game/Data/NPCs/Npc_Shop_Weapons_Armor.asset";
        private const string SeedsShopPath = "Assets/_Game/Data/Economy/Shop_Seeds_Tools.asset";
        private const string WeaponsShopPath = "Assets/_Game/Data/Economy/Shop_Weapons_Armor.asset";

        [MenuItem("CindarsHope/Validate/Validate WAVE12 NPC Dialogue Shop Bridge", priority = 49)]
        public static void RunValidation()
        {
            var passes = new List<string>();
            var issues = new List<string>();

            ValidateRuntimeTypes(passes, issues);
            ValidateNpcData(PipNpcPath, requiresDialogueTree: true, requiresShop: false, passes, issues);
            ValidateNpcData(WandererNpcPath, requiresDialogueTree: true, requiresShop: false, passes, issues);
            ValidateNpcData(SeedsNpcPath, requiresDialogueTree: false, requiresShop: true, passes, issues);
            ValidateNpcData(WeaponsNpcPath, requiresDialogueTree: false, requiresShop: true, passes, issues);
            ValidateShopData(SeedsShopPath, "npc_shop_seeds_tools", passes, issues);
            ValidateShopData(WeaponsShopPath, "npc_shop_weapons_armor", passes, issues);

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
            RequireType<DialogueModal>(passes, issues, nameof(DialogueModal));
            RequireType<ShopMenuModal>(passes, issues, nameof(ShopMenuModal));
            RequireType<BuyPanel>(passes, issues, nameof(BuyPanel));
            RequireType<SellPanel>(passes, issues, nameof(SellPanel));
            RequireType<ShopManager>(passes, issues, nameof(ShopManager));
            RequireType<ModalManager>(passes, issues, nameof(ModalManager));
        }

        private static void ValidateNpcData(
            string path,
            bool requiresDialogueTree,
            bool requiresShop,
            ICollection<string> passes,
            ICollection<string> issues)
        {
            var npc = AssetDatabase.LoadAssetAtPath<NpcDataSO>(path);
            if (npc == null)
            {
                issues.Add($"Missing NpcDataSO at {path}");
                return;
            }

            if (string.IsNullOrWhiteSpace(npc.NpcId)) issues.Add($"{path} has empty NpcId");
            else passes.Add($"{path} has NpcId '{npc.NpcId}'");

            if (string.IsNullOrWhiteSpace(npc.DisplayName)) issues.Add($"{npc.NpcId} has empty DisplayName");
            else passes.Add($"{npc.NpcId} has DisplayName '{npc.DisplayName}'");

            if (string.IsNullOrWhiteSpace(npc.DefaultSceneId)) issues.Add($"{npc.NpcId} has empty DefaultSceneId");
            else passes.Add($"{npc.NpcId} has DefaultSceneId '{npc.DefaultSceneId}'");

            if (requiresDialogueTree)
            {
                ValidateDialogueTree(npc, passes, issues);
            }

            if (requiresShop)
            {
                if (string.IsNullOrWhiteSpace(npc.ShopId)) issues.Add($"{npc.NpcId} requires ShopId");
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

        private static void ValidateShopData(string path, string expectedNpcId, ICollection<string> passes, ICollection<string> issues)
        {
            var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
            if (shop == null)
            {
                issues.Add($"Missing ShopDataSO at {path}");
                return;
            }

            if (string.IsNullOrWhiteSpace(shop.Id)) issues.Add($"{path} has empty shop Id");
            else passes.Add($"{path} has shop Id '{shop.Id}'");

            if (shop.NpcId != expectedNpcId) issues.Add($"{shop.Id} expected NpcId '{expectedNpcId}', found '{shop.NpcId}'");
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
    }
}
