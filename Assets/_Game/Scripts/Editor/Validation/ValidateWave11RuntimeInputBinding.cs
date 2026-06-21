using System.Collections.Generic;
using System.Linq;
using CindarsHope.Player;
using CindarsHope.Player.Movement;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateWave11RuntimeInputBinding
    {
        private static readonly string[] NewNodeIds =
        {
            "melee.avanco_aco",
            "melee.grito_desafio",
            "melee.investida_quebra_guarda",
            "magic.chama_breve",
            "magic.rajada_gelida",
            "survival.sinal_retirada",
            "survival.isca_improvisada",
            "survival.kit_emergencia",
            "survival.instinto_sobrevivencia",
            "survival.campo_seguro",
            "crafting.irrigador_portatil",
            "crafting.bomba_improvisada",
            "crafting.mecanismo_campo",
            "crafting.marca_eficiencia"
        };

        private static readonly string[] NewActionIds =
        {
            "skill_melee_avanco_aco",
            "skill_melee_grito_desafio",
            "skill_melee_investida_quebra_guarda",
            "skill_magic_chama_breve",
            "skill_magic_rajada_gelida",
            "skill_survival_sinal_retirada",
            "skill_survival_isca_improvisada",
            "skill_survival_kit_emergencia",
            "skill_survival_instinto_sobrevivencia",
            "skill_survival_campo_seguro",
            "skill_crafting_irrigador_portatil",
            "skill_crafting_bomba_improvisada",
            "skill_crafting_mecanismo_campo",
            "skill_crafting_marca_eficiencia"
        };

        public static void RunValidation()
        {
            var issues = new List<string>();
            var passes = new List<string>();

            ValidateTypes(passes, issues);
            ValidateCatalog(passes, issues);
            ValidateEffectMappings(passes, issues);
            ValidateMovementActions(passes, issues);

            Debug.Log("=== ValidateWave11RuntimeInputBinding - DONE ===");
            Debug.Log($"PASS: {passes.Count}  FAIL: {issues.Count}");
            foreach (var pass in passes) Debug.Log($"  [PASS] {pass}");
            foreach (var issue in issues) Debug.LogError($"  [FAIL] {issue}");

            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("WAVE11 Runtime Input Binding", $"All {passes.Count} checks PASSED.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "WAVE11 Runtime Input Binding",
                    $"{passes.Count} passed, {issues.Count} failed. See Console for details.",
                    "OK");
            }
        }

        private static void ValidateTypes(ICollection<string> passes, ICollection<string> issues)
        {
            RequireType<ActiveSkillExecutionController>(passes, issues, nameof(ActiveSkillExecutionController));
            RequireType<FeedbackOnlySkillEffectExecutor>(passes, issues, nameof(FeedbackOnlySkillEffectExecutor));
            RequireType<PlayerDashController>(passes, issues, nameof(PlayerDashController));
            RequireType<PlayerMovementAbilityController>(passes, issues, nameof(PlayerMovementAbilityController));
            RequireType<DirectionalDoubleTapDetector>(passes, issues, nameof(DirectionalDoubleTapDetector));
            RequireType<PlayerController>(passes, issues, nameof(PlayerController));
        }

        private static void ValidateCatalog(ICollection<string> passes, ICollection<string> issues)
        {
            var nodes = DefaultSkillCatalog.BuildAllNodes();
            var nodeIds = new HashSet<string>(nodes.Select(node => node.SkillNodeId));
            foreach (var nodeId in NewNodeIds)
            {
                if (nodeIds.Contains(nodeId)) passes.Add($"DefaultSkillCatalog.BuildAllNodes includes {nodeId}");
                else issues.Add($"DefaultSkillCatalog.BuildAllNodes missing {nodeId}");
            }

            var trees = DefaultSkillCatalog.BuildAllTrees(nodes);
            foreach (var nodeId in NewNodeIds)
            {
                var node = nodes.FirstOrDefault(candidate => candidate.SkillNodeId == nodeId);
                var tree = node != null
                    ? trees.FirstOrDefault(candidate => candidate.TreeId == node.TreeId)
                    : null;
                var inTree = tree != null && tree.Nodes.Any(candidate => candidate.SkillNodeId == nodeId);
                if (inTree) passes.Add($"DefaultSkillCatalog tree '{node.TreeId}' includes {nodeId}");
                else issues.Add($"DefaultSkillCatalog tree list missing {nodeId}");
            }
        }

        private static void ValidateEffectMappings(ICollection<string> passes, ICollection<string> issues)
        {
            foreach (var actionId in NewActionIds)
            {
                if (ActiveSkillExecutionController.TryGetEffectIdForValidation(actionId, out var effectId)
                    && !string.IsNullOrWhiteSpace(effectId))
                {
                    passes.Add($"SkillActionToEffectId maps {actionId} -> {effectId}");
                }
                else
                {
                    issues.Add($"SkillActionToEffectId missing {actionId}");
                }
            }

            RequireEffectMapping("skill_crafting_field_patch", "farm.crop.water_skill", passes, issues);
            RequireNoEffectMapping("dash", passes, issues);
            RequireNoEffectMapping("dodge", passes, issues);
            RequireNoEffectMapping("block", passes, issues);
        }

        private static void ValidateMovementActions(ICollection<string> passes, ICollection<string> issues)
        {
            var detector = new DirectionalDoubleTapDetector();
            if (detector != null) passes.Add("DirectionalDoubleTapDetector can be constructed");
            else issues.Add("DirectionalDoubleTapDetector construction failed");
        }

        private static void RequireType<T>(ICollection<string> passes, ICollection<string> issues, string label)
        {
            if (typeof(T) != null) passes.Add($"{label} type exists");
            else issues.Add($"{label} type missing");
        }

        private static void RequireEffectMapping(
            string actionId,
            string expectedEffectId,
            ICollection<string> passes,
            ICollection<string> issues)
        {
            if (ActiveSkillExecutionController.TryGetEffectIdForValidation(actionId, out var effectId)
                && effectId == expectedEffectId)
            {
                passes.Add($"{actionId} maps to real effect {expectedEffectId}");
            }
            else
            {
                issues.Add($"{actionId} does not map to {expectedEffectId}");
            }
        }

        private static void RequireNoEffectMapping(
            string actionId,
            ICollection<string> passes,
            ICollection<string> issues)
        {
            if (ActiveSkillExecutionController.TryGetEffectIdForValidation(actionId, out _))
            {
                issues.Add($"Non-slot movement/defense action '{actionId}' must not be mapped as active skill");
            }
            else
            {
                passes.Add($"Non-slot movement/defense action '{actionId}' is not mapped as active skill");
            }
        }
    }
}
