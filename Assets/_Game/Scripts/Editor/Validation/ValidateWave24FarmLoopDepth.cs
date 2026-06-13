using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Validator WAVE24: Farm Loop Depth + Daily Goals.
    /// Verifica que artefatos críticos da wave existem e que não há duplicações de sistemas core.
    /// </summary>
    public static class ValidateWave24FarmLoopDepth
    {
        private const string MenuPath = "CindarsHope/Validate/Wave 24 - Farm Loop Depth";

        [MenuItem(MenuPath)]
        public static void Run()
        {
            var results = new List<string>();
            var passed = true;

            // Check: docs WAVE24 existem
            CheckDoc(results, ref passed, "docs/validation/WAVE_INTEGRATION_24_FARM_LOOP_DEPTH_REPORT.md",
                "WAVE24 final report");
            CheckDoc(results, ref passed, "docs/validation/WAVE_INTEGRATION_24_FARM_LOOP_DEPTH_DECISION.md",
                "WAVE24 decision doc");
            CheckDoc(results, ref passed, "docs/validation/WAVE_INTEGRATION_24_EXISTING_FARM_FUNCTIONALITY_MATRIX.md",
                "WAVE24 farm functionality matrix");
            CheckDoc(results, ref passed, "docs/validation/WAVE_INTEGRATION_24_CROP_WATER_HARVEST_MATRIX.md",
                "WAVE24 crop/water/harvest matrix");
            CheckDoc(results, ref passed, "docs/validation/WAVE_INTEGRATION_24_DAILY_GOAL_MATRIX.md",
                "WAVE24 daily goal matrix");
            CheckDoc(results, ref passed, "docs/validation/WAVE_INTEGRATION_24_RESOURCE_SHIPPING_ECONOMY_MATRIX.md",
                "WAVE24 resource/shipping/economy matrix");
            CheckDoc(results, ref passed, "docs/validation/WAVE_INTEGRATION_24_SAVE_LOAD_FARM_DAILY_GOAL_MATRIX.md",
                "WAVE24 save/load matrix");
            CheckDoc(results, ref passed, "docs/validation/WAVE_INTEGRATION_24_HUMAN_PLAYMODE_CHECKLIST.md",
                "WAVE24 human Play Mode checklist");

            // Check: FarmDailyGoalService existe
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalService.cs",
                "FarmDailyGoalService");
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalDefinition.cs",
                "FarmDailyGoalDefinition");
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalState.cs",
                "FarmDailyGoalState");
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalsSaveData.cs",
                "FarmDailyGoalsSaveData");
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalRuntimeBootstrap.cs",
                "FarmDailyGoalRuntimeBootstrap");
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/FarmLoopFeedbackBridge.cs",
                "FarmLoopFeedbackBridge");
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/ShippingSummaryService.cs",
                "ShippingSummaryService");

            // Check: eventos novos existem
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Core/Events/DailyGoalProgressedEvent.cs",
                "DailyGoalProgressedEvent");
            CheckScript(results, ref passed,
                "Assets/_Game/Scripts/Core/Events/DailyGoalCompletedEvent.cs",
                "DailyGoalCompletedEvent");
            // CropWateredEvent check removed 2026-06-13: the event was retired
            // (never published nor subscribed anywhere; dead contract from WAVE24).

            // Check: sistemas core NÃO foram duplicados
            CheckNoDuplicate(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/InventoryManager.cs",
                "InventoryManager não deve ser duplicado em Farm/Runtime");
            CheckNoDuplicate(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/SaveManager.cs",
                "SaveManager não deve ser duplicado em Farm/Runtime");
            CheckNoDuplicate(results, ref passed,
                "Assets/_Game/Scripts/Farm/Runtime/EconomyManager.cs",
                "EconomyManager não deve ser duplicado em Farm/Runtime");

            // Gate WAVE20 check
            CheckDoc(results, ref passed,
                "docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md",
                "WAVE20 gate satisfeito");

            // Check: CURRENT_STATE atualizado
            CheckDoc(results, ref passed, "docs/project/CURRENT_STATE.md", "CURRENT_STATE existe");

            // Summary
            var prefix = passed ? "[WAVE24 PASS]" : "[WAVE24 FAIL]";
            foreach (var r in results)
            {
                if (r.StartsWith("FAIL"))
                    Debug.LogError($"{prefix} {r}");
                else
                    Debug.Log($"{prefix} {r}");
            }

            if (passed)
                Debug.Log("[ValidateWave24FarmLoopDepth] Todos os checks PASSARAM.");
            else
                Debug.LogError("[ValidateWave24FarmLoopDepth] Um ou mais checks FALHARAM. Ver log acima.");
        }

        private static void CheckDoc(List<string> results, ref bool passed, string path, string label)
        {
            if (File.Exists(path))
            {
                results.Add($"PASS: {label} — {path}");
            }
            else
            {
                results.Add($"FAIL: {label} ausente — {path}");
                passed = false;
            }
        }

        private static void CheckScript(List<string> results, ref bool passed, string path, string label)
        {
            if (File.Exists(path))
            {
                results.Add($"PASS: {label} existe — {path}");
            }
            else
            {
                results.Add($"FAIL: {label} ausente — {path}");
                passed = false;
            }
        }

        private static void CheckNoDuplicate(List<string> results, ref bool passed, string path, string label)
        {
            if (!File.Exists(path))
            {
                results.Add($"PASS: sem duplicata — {label}");
            }
            else
            {
                results.Add($"FAIL: duplicata encontrada — {label} em {path}");
                passed = false;
            }
        }
    }
}
