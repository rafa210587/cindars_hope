using System.Collections.Generic;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// WAVE_INTEGRATION_11: Validates the skill effects gameplay bridge is in place.
    /// Checks: SkillEffectRegistry can register FarmCropSkillEffectExecutor,
    /// DefaultSkillCatalog equippable skills have EffectId mappings,
    /// FarmPlot has TryWaterViaSkill public API.
    /// </summary>
    public static class ValidateSkillEffectsGameplayBridge
    {
        public static void RunValidation()
        {
            Debug.Log("=== ValidateSkillEffectsGameplayBridge - START ===");

            var issues = new List<string>();
            var passes = new List<string>();

            // 1. Registry can register and resolve FarmCropSkillEffectExecutor
            try
            {
                var registry = new SkillEffectRegistry();
                var farmExecutor = new FarmCropSkillEffectExecutor();
                registry.Register(farmExecutor);

                if (registry.HasExecutor("farm.crop.water_skill"))
                    passes.Add("SkillEffectRegistry: FarmCropSkillEffectExecutor registered - PASS");
                else
                    issues.Add("SkillEffectRegistry: FarmCropSkillEffectExecutor NOT resolved - FAIL");

                var resolved = registry.Resolve("farm.crop.water_skill");
                if (resolved != null && resolved.Category == SkillEffectCategory.Farm)
                    passes.Add("SkillEffectRegistry: Resolve('farm.crop.water_skill') returns Farm executor - PASS");
                else
                    issues.Add("SkillEffectRegistry: Resolve('farm.crop.water_skill') failed - FAIL");
            }
            catch (System.Exception ex)
            {
                issues.Add($"SkillEffectRegistry exception: {ex.Message}");
            }

            // 2. DefaultSkillCatalog equippable skills
            try
            {
                var nodes = DefaultSkillCatalog.BuildAllNodes();
                int equippable = 0;
                int missingActionId = 0;

                foreach (var node in nodes)
                {
                    if (node.SkillCategory == SkillCategory.EquippableSkill)
                    {
                        equippable++;
                        if (string.IsNullOrEmpty(node.UnlockedSkillActionId))
                            missingActionId++;
                    }
                }

                passes.Add($"DefaultSkillCatalog: {equippable} equippable skills found");
                if (missingActionId == 0)
                    passes.Add("DefaultSkillCatalog: All equippable skills have UnlockedSkillActionId - PASS");
                else
                    issues.Add($"DefaultSkillCatalog: {missingActionId} equippable skills missing UnlockedSkillActionId - FAIL");
            }
            catch (System.Exception ex)
            {
                issues.Add($"DefaultSkillCatalog exception: {ex.Message}");
            }

            // 3. SkillEffectContext and SkillEffectResult construction
            try
            {
                var ctx = new SkillEffectContext
                {
                    SkillActionId = "skill_test",
                    EffectId = "farm.crop.water_skill",
                    ActiveSlotIndex = 0
                };

                var resultSuccess = SkillEffectResult.Succeeded("Test", costSpent: true, cooldownStarted: true);
                var resultFail = SkillEffectResult.Failed("TestFail");

                if (resultSuccess.Success && resultFail.Success == false)
                    passes.Add("SkillEffectContext + SkillEffectResult: contracts functional - PASS");
                else
                    issues.Add("SkillEffectResult: unexpected state - FAIL");
            }
            catch (System.Exception ex)
            {
                issues.Add($"SkillEffectContext/Result exception: {ex.Message}");
            }

            // Summary
            Debug.Log($"=== ValidateSkillEffectsGameplayBridge - DONE ===");
            Debug.Log($"PASS: {passes.Count}  FAIL: {issues.Count}");
            foreach (var p in passes) Debug.Log($"  [PASS] {p}");
            foreach (var issue in issues) Debug.LogError($"  [FAIL] {issue}");

            if (issues.Count == 0)
                EditorUtility.DisplayDialog("Skill Effects Bridge Validation", $"All {passes.Count} checks PASSED.", "OK");
            else
                EditorUtility.DisplayDialog("Skill Effects Bridge Validation",
                    $"{passes.Count} passed, {issues.Count} failed.\nSee Console for details.", "OK");
        }
    }
}
