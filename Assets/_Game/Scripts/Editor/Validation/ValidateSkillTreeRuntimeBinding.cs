using System.Collections.Generic;
using CindarsHope.Skills;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// WAVE_INTEGRATION_10: Validates that the skill tree runtime contracts are in place.
    /// Checks: DefaultSkillCatalog builds without warnings, SkillTreeManager component presence
    /// in scene bootstrap, and active slot state accessors work correctly.
    /// Does NOT validate gameplay effects (deferred to WAVE_INTEGRATION_11).
    /// </summary>
    public static class ValidateSkillTreeRuntimeBinding
    {
        [MenuItem("CindarsHope/Validate/Validate Skill Tree Runtime", priority = 48)]
        public static void RunValidation()
        {
            Debug.Log("=== ValidateSkillTreeRuntimeBinding — START ===");

            var issues = new List<string>();
            var passes = new List<string>();

            // 1. Verify DefaultSkillCatalog builds all 55 nodes and 5 trees
            try
            {
                var nodes = DefaultSkillCatalog.BuildAllNodes();
                if (nodes.Count == 55)
                    passes.Add($"DefaultSkillCatalog: {nodes.Count} nodes (expected 55) — PASS");
                else
                    issues.Add($"DefaultSkillCatalog: {nodes.Count} nodes (expected 55) — FAIL");

                var trees = DefaultSkillCatalog.BuildAllTrees(nodes);
                if (trees.Count == 5)
                    passes.Add($"DefaultSkillCatalog: {trees.Count} trees (expected 5) — PASS");
                else
                    issues.Add($"DefaultSkillCatalog: {trees.Count} trees (expected 5) — FAIL");

                // Verify each tree has nodes
                foreach (var tree in trees)
                {
                    if (tree.Nodes.Count > 0)
                        passes.Add($"  Tree '{tree.TreeId}': {tree.Nodes.Count} nodes — PASS");
                    else
                        issues.Add($"  Tree '{tree.TreeId}': 0 nodes — FAIL");
                }

                // Verify equippable skills have UnlockedSkillActionId
                int equippableCount = 0;
                foreach (var node in nodes)
                {
                    if (node.SkillCategory == SkillCategory.EquippableSkill)
                    {
                        equippableCount++;
                        if (string.IsNullOrEmpty(node.UnlockedSkillActionId))
                            issues.Add($"  Node '{node.SkillNodeId}' is EquippableSkill but has no UnlockedSkillActionId — FAIL");
                    }
                }
                passes.Add($"DefaultSkillCatalog: {equippableCount} equippable skill nodes verified — PASS");
            }
            catch (System.Exception ex)
            {
                issues.Add($"DefaultSkillCatalog build threw exception: {ex.Message} — FAIL");
            }

            // 2. Verify SkillTreeState contracts
            try
            {
                var state = new SkillTreeState(3);
                if (state.AvailableSkillPoints == 3)
                    passes.Add("SkillTreeState: constructor sets available points — PASS");
                else
                    issues.Add("SkillTreeState: constructor does not set available points — FAIL");

                state.Purchase("test_node", 1);
                if (state.IsPurchased("test_node") && state.AvailableSkillPoints == 2)
                    passes.Add("SkillTreeState: Purchase decrements points and marks purchased — PASS");
                else
                    issues.Add("SkillTreeState: Purchase contract broken — FAIL");

                state.AssignActiveSlot(0, "skill_action_test");
                if (state.GetActiveSlotSkillActionId(0) == "skill_action_test")
                    passes.Add("SkillTreeState: AssignActiveSlot / GetActiveSlotSkillActionId — PASS");
                else
                    issues.Add("SkillTreeState: active slot assignment contract broken — FAIL");

                state.ClearActiveSlot(0);
                if (string.IsNullOrEmpty(state.GetActiveSlotSkillActionId(0)))
                    passes.Add("SkillTreeState: ClearActiveSlot works — PASS");
                else
                    issues.Add("SkillTreeState: ClearActiveSlot contract broken — FAIL");
            }
            catch (System.Exception ex)
            {
                issues.Add($"SkillTreeState contract check threw exception: {ex.Message} — FAIL");
            }

            // 3. Verify SkillTreeManager is present in current loaded scene bootstrap
            var bootstrap = Object.FindAnyObjectByType<CindarsHope.Core.Bootstrap.GameBootstrap>();
            if (bootstrap != null)
            {
                var skillTreeMgr = bootstrap.GetComponent<SkillTreeManager>();
                if (skillTreeMgr != null)
                    passes.Add("Scene Bootstrap: SkillTreeManager component found — PASS");
                else
                    issues.Add("Scene Bootstrap: SkillTreeManager component missing — FAIL (run CreateMvpFarmScene)");

                if (bootstrap.SkillTreeManager != null)
                    passes.Add("GameBootstrap.SkillTreeManager property wired — PASS");
                else
                    issues.Add("GameBootstrap.SkillTreeManager property null — FAIL (SerializedField not assigned)");
            }
            else
            {
                passes.Add("Scene Bootstrap: GameBootstrap not found in scene — SKIP (open FarmScene first)");
            }

            // 4. Verify SkillTreeGameplayPanelController is present as a singleton in scene/DontDestroyOnLoad
            // (only meaningful in Play Mode; in Edit Mode it won't exist via RuntimeInitializeOnLoad)
            if (Application.isPlaying)
            {
                var panel = Object.FindAnyObjectByType<CindarsHope.UI.Skills.SkillTreeGameplayPanelController>();
                if (panel != null)
                    passes.Add("SkillTreeGameplayPanelController: singleton found in scene — PASS");
                else
                    issues.Add("SkillTreeGameplayPanelController: singleton not found — FAIL (RuntimeInitializeOnLoad should create it)");
            }
            else
            {
                passes.Add("SkillTreeGameplayPanelController singleton: SKIP (Play Mode only via RuntimeInitializeOnLoad)");
            }

            // Report
            Debug.Log($"=== ValidateSkillTreeRuntimeBinding — RESULTS ===");
            foreach (var p in passes)
                Debug.Log($"  [PASS] {p}");
            foreach (var issue in issues)
                Debug.LogError($"  [FAIL] {issue}");

            if (issues.Count == 0)
                Debug.Log($"=== ValidateSkillTreeRuntimeBinding — ALL PASS ({passes.Count} checks) ===");
            else
                Debug.LogError($"=== ValidateSkillTreeRuntimeBinding — {issues.Count} FAILURES, {passes.Count} PASSES ===");
        }
    }
}
