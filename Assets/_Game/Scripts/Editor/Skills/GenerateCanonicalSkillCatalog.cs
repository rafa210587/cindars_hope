#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using CindarsHope.Foundation;
using CindarsHope.Skills;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Skills
{
    // fable_29 — data-driven generator for the canonical skill catalog (69 SkillNodeDataSO
    // assets + 5 SkillTreeDataSO assets), sourced from DefaultSkillCatalog (single ordered
    // table). Also runs a per-tree count validation against the WI-11 canonical composition.
    //
    // Assets are written via AssetDatabase (no manual YAML). Run:
    //   CindarsHope/Skills/Generate Canonical Skill Catalog
    //   CindarsHope/Skills/Validate Skill Catalog Counts
    public static class GenerateCanonicalSkillCatalog
    {
        private const string NodesDir = "Assets/_Game/Data/Skills/Nodes";
        private const string TreesDir = "Assets/_Game/Data/Skills/Trees";

        // Canonical per-tree counts (fable_70 saneamento). Sum = 66.
        private static readonly Dictionary<string, int> ExpectedPerTree = new Dictionary<string, int>
        {
            { "melee", 13 }, { "ranged", 11 }, { "magic", 13 }, { "survival", 15 }, { "crafting", 14 },
        };

        public static void Generate()
        {
            Debug.Log("=== GenerateCanonicalSkillCatalog — START ===");

            if (!ValidateCounts(out var report))
            {
                Debug.LogError("GenerateCanonicalSkillCatalog: count validation FAILED — aborting generation.\n" + report);
                return;
            }

            EnsureFolder(NodesDir);
            EnsureFolder(TreesDir);

            var nodes = DefaultSkillCatalog.BuildAllNodes();
            var nodeAssetById = new Dictionary<string, SkillNodeDataSO>();
            int created = 0, updated = 0;

            foreach (var node in nodes)
            {
                var path = $"{NodesDir}/SkillNode_{node.SkillNodeId.Replace('.', '_')}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<SkillNodeDataSO>(path);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(node, path);
                    nodeAssetById[node.SkillNodeId] = node;
                    created++;
                }
                else
                {
                    CopyNode(node, existing);
                    EditorUtility.SetDirty(existing);
                    nodeAssetById[node.SkillNodeId] = existing;
                    updated++;
                }
            }

            var trees = DefaultSkillCatalog.BuildAllTrees(nodes);
            int treesCreated = 0, treesUpdated = 0;
            foreach (var tree in trees)
            {
                var path = $"{TreesDir}/SkillTree_{tree.TreeId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<SkillTreeDataSO>(path);
                if (existing == null)
                {
                    // Re-point the tree's node list at the persisted node assets.
                    RepointTreeNodes(tree, nodeAssetById);
                    AssetDatabase.CreateAsset(tree, path);
                    treesCreated++;
                }
                else
                {
                    existing.TreeId = tree.TreeId;
                    existing.DisplayName = tree.DisplayName;
                    existing.Description = tree.Description;
                    existing.CapstoneNodeId = tree.CapstoneNodeId;
                    existing.Nodes.Clear();
                    foreach (var n in tree.Nodes)
                        if (n != null && nodeAssetById.TryGetValue(n.SkillNodeId, out var asset))
                            existing.Nodes.Add(asset);
                    EditorUtility.SetDirty(existing);
                    treesUpdated++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"=== GenerateCanonicalSkillCatalog — DONE === nodes: {created} created, {updated} updated "
                + $"(total {nodes.Count}); trees: {treesCreated} created, {treesUpdated} updated (total {trees.Count}).\n" + report);
        }

        public static void ValidateCountsMenu()
        {
            bool ok = ValidateCounts(out var report);
            if (ok) Debug.Log("=== Validate Skill Catalog Counts — PASS ===\n" + report);
            else Debug.LogError("=== Validate Skill Catalog Counts — FAIL ===\n" + report);
        }

        public static bool ValidateCounts(out string report)
        {
            var nodes = DefaultSkillCatalog.BuildAllNodes();
            var perTree = new Dictionary<string, int>();
            foreach (var n in nodes)
            {
                perTree.TryGetValue(n.TreeId, out var c);
                perTree[n.TreeId] = c + 1;
            }

            var sb = new StringBuilder();
            bool ok = nodes.Count == DefaultSkillCatalog.CanonicalNodeCount;
            sb.AppendLine($"Total nodes: {nodes.Count} (expected {DefaultSkillCatalog.CanonicalNodeCount}) — {(ok ? "PASS" : "FAIL")}");

            foreach (var kvp in ExpectedPerTree)
            {
                perTree.TryGetValue(kvp.Key, out var actual);
                bool treeOk = actual == kvp.Value;
                if (!treeOk) ok = false;
                sb.AppendLine($"  Tree '{kvp.Key}': {actual} (expected {kvp.Value}) — {(treeOk ? "PASS" : "FAIL")}");
            }

            // Tier sanity: every node tier in 1..5; capstones tier 5.
            foreach (var n in nodes)
            {
                if (n.Tier < SkillTierRules.MinTier || n.Tier > SkillTierRules.MaxTier)
                {
                    ok = false;
                    sb.AppendLine($"  Node '{n.SkillNodeId}': invalid Tier {n.Tier} — FAIL");
                }
                if (n.IsCapstone && n.Tier != 5)
                {
                    ok = false;
                    sb.AppendLine($"  Capstone '{n.SkillNodeId}': Tier {n.Tier} (expected 5) — FAIL");
                }
            }

            report = sb.ToString();
            return ok;
        }

        private static void RepointTreeNodes(SkillTreeDataSO tree, Dictionary<string, SkillNodeDataSO> assets)
        {
            for (int i = 0; i < tree.Nodes.Count; i++)
            {
                var n = tree.Nodes[i];
                if (n != null && assets.TryGetValue(n.SkillNodeId, out var asset))
                    tree.Nodes[i] = asset;
            }
        }

        private static void CopyNode(SkillNodeDataSO src, SkillNodeDataSO dst)
        {
            dst.SkillNodeId = src.SkillNodeId;
            dst.TreeId = src.TreeId;
            dst.DisplayName = src.DisplayName;
            dst.Description = src.Description;
            dst.NodeType = src.NodeType;
            dst.SkillCategory = src.SkillCategory;
            dst.IsCapstone = src.IsCapstone;
            dst.SkillPointCost = src.SkillPointCost;
            dst.MinimumPlayerLevel = src.MinimumPlayerLevel;
            dst.PrerequisiteNodeIds = new List<string>(src.PrerequisiteNodeIds);
            dst.RequiredPurchasedNodesInTree = src.RequiredPurchasedNodesInTree;
            dst.Tier = src.Tier;
            dst.EffectRoute = src.EffectRoute;
            dst.RoutePayloadPerRank = src.RoutePayloadPerRank;
            dst.EffectPending = src.EffectPending;
            dst.EffectPendingTooltip = src.EffectPendingTooltip;
            dst.NotYetExecutable = src.NotYetExecutable;
            dst.CapstoneVariants = new List<string>(src.CapstoneVariants);
            dst.UnlockedSkillActionId = src.UnlockedSkillActionId;
            dst.LinkedSpellId = src.LinkedSpellId;
            dst.PassiveModifiers = new List<SkillPassiveModifier>(src.PassiveModifiers);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
