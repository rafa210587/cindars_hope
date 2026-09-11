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
        private const string ActionsDir = "Assets/_Game/Data/Skills/Actions";
        private const string ActionDatabasePath = "Assets/_Game/Data/Skills/SkillActionDatabase.asset";
        private const string NodeDatabasePath = "Assets/_Game/Data/Skills/SkillNodeDatabase.asset";
        private const string TreeRegistryPath = "Assets/_Game/Data/Skills/SkillTreeRegistry.asset";
        private const string RuntimeRegistryDir = "Assets/_Game/Resources/Skills";
        private const string RuntimeRegistryPath = RuntimeRegistryDir + "/SkillRuntimeCatalogRegistry.asset";

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
            EnsureFolder(ActionsDir);
            EnsureFolder(RuntimeRegistryDir);

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

            var actions = DefaultSkillActionCatalog.BuildAll();
            var actionAssets = new List<SkillActionSO>();
            int actionsCreated = 0, actionsUpdated = 0;
            foreach (var action in actions)
            {
                var path = $"{ActionsDir}/SkillAction_{action.SkillActionId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<SkillActionSO>(path);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(action, path);
                    actionAssets.Add(action);
                    actionsCreated++;
                }
                else
                {
                    CopyAction(action, existing);
                    EditorUtility.SetDirty(existing);
                    actionAssets.Add(existing);
                    actionsUpdated++;
                }
            }

            var actionDatabase = AssetDatabase.LoadAssetAtPath<SkillActionDatabaseSO>(ActionDatabasePath);
            if (actionDatabase == null)
            {
                actionDatabase = ScriptableObject.CreateInstance<SkillActionDatabaseSO>();
                AssetDatabase.CreateAsset(actionDatabase, ActionDatabasePath);
            }
            var serializedDatabase = new SerializedObject(actionDatabase);
            var items = serializedDatabase.FindProperty("_items");
            items.arraySize = actionAssets.Count;
            for (int i = 0; i < actionAssets.Count; i++)
                items.GetArrayElementAtIndex(i).objectReferenceValue = actionAssets[i];
            serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(actionDatabase);

            var nodeDatabase = UpsertRegistry<SkillNodeDatabaseSO, SkillNodeDataSO>(
                NodeDatabasePath, new List<SkillNodeDataSO>(nodeAssetById.Values));
            var treeAssets = new List<SkillTreeDataSO>();
            foreach (var tree in trees)
            {
                var asset = AssetDatabase.LoadAssetAtPath<SkillTreeDataSO>($"{TreesDir}/SkillTree_{tree.TreeId}.asset");
                if (asset != null) treeAssets.Add(asset);
            }
            var treeRegistry = UpsertRegistry<SkillTreeRegistrySO, SkillTreeDataSO>(TreeRegistryPath, treeAssets);
            var runtimeRegistry = AssetDatabase.LoadAssetAtPath<SkillRuntimeCatalogRegistrySO>(RuntimeRegistryPath);
            if (runtimeRegistry == null)
            {
                runtimeRegistry = ScriptableObject.CreateInstance<SkillRuntimeCatalogRegistrySO>();
                AssetDatabase.CreateAsset(runtimeRegistry, RuntimeRegistryPath);
            }
            runtimeRegistry.Configure(treeRegistry, nodeDatabase, actionDatabase);
            EditorUtility.SetDirty(runtimeRegistry);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"=== GenerateCanonicalSkillCatalog — DONE === nodes: {created} created, {updated} updated "
                + $"(total {nodes.Count}); trees: {treesCreated} created, {treesUpdated} updated (total {trees.Count}); "
                + $"actions: {actionsCreated} created, {actionsUpdated} updated (total {actions.Count}).\n" + report);
        }

        private static TRegistry UpsertRegistry<TRegistry, TItem>(string path, List<TItem> values)
            where TRegistry : ScriptableObject
            where TItem : UnityEngine.Object
        {
            var registry = AssetDatabase.LoadAssetAtPath<TRegistry>(path);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<TRegistry>();
                AssetDatabase.CreateAsset(registry, path);
            }
            var serialized = new SerializedObject(registry);
            var items = serialized.FindProperty("_items");
            items.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                items.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
            return registry;
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
            int actionCount = DefaultSkillActionCatalog.BuildAll().Count;
            bool actionsOk = actionCount == DefaultSkillActionCatalog.CanonicalActionCount;
            if (!actionsOk) ok = false;
            sb.AppendLine($"Total actions: {actionCount} (expected {DefaultSkillActionCatalog.CanonicalActionCount}) — {(actionsOk ? "PASS" : "FAIL")}");

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
            dst.AuthoredMaxRank = src.AuthoredMaxRank;
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

        private static void CopyAction(SkillActionSO src, SkillActionSO dst)
        {
            dst.SkillActionId = src.SkillActionId;
            dst.DisplayName = src.DisplayName;
            dst.Description = src.Description;
            dst.SkillActionType = src.SkillActionType;
            dst.TimingProfileId = src.TimingProfileId;
            dst.WindupSeconds = src.WindupSeconds;
            dst.ActiveSeconds = src.ActiveSeconds;
            dst.RecoverySeconds = src.RecoverySeconds;
            dst.CooldownSeconds = src.CooldownSeconds;
            dst.StaminaCost = src.StaminaCost;
            dst.ManaCost = src.ManaCost;
            dst.LinkedSpellId = src.LinkedSpellId;
            dst.SpellDiscipline = src.SpellDiscipline;
            dst.BaseDamage = src.BaseDamage;
            dst.DamagePerRank = src.DamagePerRank;
            dst.DamageType = src.DamageType;
            dst.Range = src.Range;
            dst.ProjectileCount = src.ProjectileCount;
            dst.ProjectileSpreadDegrees = src.ProjectileSpreadDegrees;
            dst.LinePierceCount = src.LinePierceCount;
            dst.ChargeMinimumHoldSeconds = src.ChargeMinimumHoldSeconds;
            dst.ChargeTimeSeconds = src.ChargeTimeSeconds;
            dst.ChargeMaximumDamage = src.ChargeMaximumDamage;
            dst.ChargeMaximumRange = src.ChargeMaximumRange;
            dst.ChargeMaximumPostureDamageMultiplier = src.ChargeMaximumPostureDamageMultiplier;
            dst.ChargeMaximumStaminaCost = src.ChargeMaximumStaminaCost;
            dst.ProjectileSpeed = src.ProjectileSpeed;
            dst.EffectDurationSeconds = src.EffectDurationSeconds;
            dst.EffectDurationPerRank = src.EffectDurationPerRank;
            dst.RangedDamageBonusFraction = src.RangedDamageBonusFraction;
            dst.RangedDamageBonusPerRank = src.RangedDamageBonusPerRank;
            dst.EffectMagnitudeByRank = src.EffectMagnitudeByRank != null
                ? (float[])src.EffectMagnitudeByRank.Clone() : new float[0];
            dst.SecondaryMagnitudeByRank = src.SecondaryMagnitudeByRank != null
                ? (float[])src.SecondaryMagnitudeByRank.Clone() : new float[0];
            dst.SecondaryDurationByRank = src.SecondaryDurationByRank != null
                ? (float[])src.SecondaryDurationByRank.Clone() : new float[0];
            dst.EffectRadius = src.EffectRadius;
            dst.PulseCount = src.PulseCount;
            dst.ChainJumpRange = src.ChainJumpRange;
            dst.TargetingRange = src.TargetingRange;
            dst.TargetDamageMultipliers = src.TargetDamageMultipliers != null
                ? (float[])src.TargetDamageMultipliers.Clone() : new float[0];
            dst.ControlStrengthByRank = src.ControlStrengthByRank != null
                ? (float[])src.ControlStrengthByRank.Clone() : new float[0];
            dst.EffectHitCharges = src.EffectHitCharges;
            dst.StrongSlowFraction = src.StrongSlowFraction;
            dst.StrongSlowDurationSeconds = src.StrongSlowDurationSeconds;
            dst.ArcDegrees = src.ArcDegrees;
            dst.MaxTargets = src.MaxTargets;
            dst.FullDamageTargetCount = src.FullDamageTargetCount;
            dst.AdditionalTargetDamageMultiplier = src.AdditionalTargetDamageMultiplier;
            dst.KnockbackForce = src.KnockbackForce;
            dst.PostureDamageMultiplier = src.PostureDamageMultiplier;
            dst.StatusEffectId = src.StatusEffectId;
            dst.StatusApplyChance = src.StatusApplyChance;
            dst.NotYetExecutable = src.NotYetExecutable;
            dst.BlockDurationSeconds = src.BlockDurationSeconds;
            dst.DashDistance = src.DashDistance;
            dst.LeapDistance = src.LeapDistance;
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
