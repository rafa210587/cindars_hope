using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Editor validator for WAVE_INTEGRATION_15 Quest Giver + Quest Log Runtime.
    ///
    /// Menu: CindarsHope/Validate/Validate WAVE15 Quest Runtime Binding
    ///
    /// Checks:
    /// - QuestService, QuestRegistry, QuestRuntimeBootstrap exist
    /// - QuestStateRecord has GrantedRewardIds field
    /// - QuestGiverInteractable and QuestBoardInteractable exist
    /// - Quest events exist
    /// - UI controllers exist
    /// - No direct InventoryManager refs in UI/Quests/ files
    /// - No DefeatEnemy objectives (combat not ready)
    /// </summary>
    public static class ValidateQuestGiverQuestLogRuntimeBinding
    {
        [MenuItem("CindarsHope/Validate/Validate WAVE15 Quest Runtime Binding")]
        public static void Validate()
        {
            var errors = new System.Collections.Generic.List<string>();
            var passes = new System.Collections.Generic.List<string>();

            // Check 1: Core runtime scripts
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/QuestService.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/IQuestInventoryAccess.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/QuestInventoryAdapter.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/QuestGoldAdapter.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs", passes, errors);

            // Check 2: QuestStateRecord has GrantedRewardIds (idempotency guard)
            CheckFileContainsPattern(
                "Assets/_Game/Scripts/Quests/Save/QuestStateRecord.cs",
                "GrantedRewardIds",
                "QuestStateRecord.GrantedRewardIds (idempotency guard)",
                passes, errors);

            // Check 3: Interactables
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/Quests/Runtime/QuestBoardInteractable.cs", passes, errors);

            // Check 4: Quest events
            CheckFileExists("Assets/_Game/Scripts/Core/Events/QuestRuntimeEvents.cs", passes, errors);

            // Check 5: UI controllers
            CheckFileExists("Assets/_Game/Scripts/UI/Quests/Runtime/QuestOfferPanelController.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/UI/Quests/Runtime/QuestLogPanelController.cs", passes, errors);
            CheckFileExists("Assets/_Game/Scripts/UI/Quests/Runtime/QuestLogRuntimeBinder.cs", passes, errors);

            // Check 6: Smoke test quest registered
            CheckFileContainsPattern(
                "Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                "quest_first_supplies_for_cindar",
                "Smoke test quest 'quest_first_supplies_for_cindar' registered",
                passes, errors);

            // Check 7: No DefeatEnemy/kill objectives in smoke test (combat not ready)
            CheckFileDoesNotContainPattern(
                "Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                "DefeatEnemy",
                "No DefeatEnemy objective in smoke test quest",
                passes, errors);

            // Check 8: No direct InventoryManager use in UI/Quests/Runtime/
            CheckDirectoryFilesDoNotContainPattern(
                "Assets/_Game/Scripts/UI/Quests/Runtime",
                "InventoryManager",
                "No direct InventoryManager ref in UI/Quests/Runtime/",
                passes, errors);

            // Summary
            Debug.Log($"[ValidateWAVE15] PASS: {passes.Count}, FAIL: {errors.Count}");
            foreach (var p in passes) Debug.Log($"  PASS: {p}");
            foreach (var e in errors) Debug.LogError($"  FAIL: {e}");

            if (errors.Count == 0)
                EditorUtility.DisplayDialog("WAVE15 Validation", $"ALL {passes.Count} CHECKS PASSED", "OK");
            else
                EditorUtility.DisplayDialog("WAVE15 Validation", $"{passes.Count} PASS / {errors.Count} FAIL\nSee Console for details.", "OK");
        }

        private static void CheckFileExists(string path,
            System.Collections.Generic.List<string> passes,
            System.Collections.Generic.List<string> errors)
        {
            if (File.Exists(path))
                passes.Add(path);
            else
                errors.Add($"Missing: {path}");
        }

        private static void CheckFileContainsPattern(string path, string pattern, string label,
            System.Collections.Generic.List<string> passes,
            System.Collections.Generic.List<string> errors)
        {
            if (!File.Exists(path))
            {
                errors.Add($"File not found for pattern check '{label}': {path}");
                return;
            }
            var content = File.ReadAllText(path);
            if (content.Contains(pattern))
                passes.Add(label);
            else
                errors.Add($"Pattern '{pattern}' not found in {path} — expected for: {label}");
        }

        private static void CheckFileDoesNotContainPattern(string path, string pattern, string label,
            System.Collections.Generic.List<string> passes,
            System.Collections.Generic.List<string> errors)
        {
            if (!File.Exists(path))
            {
                passes.Add($"{label} (file absent = no violation)");
                return;
            }
            var content = File.ReadAllText(path);
            if (!content.Contains(pattern))
                passes.Add(label);
            else
                errors.Add($"Forbidden pattern '{pattern}' found in {path} — violation for: {label}");
        }

        private static void CheckDirectoryFilesDoNotContainPattern(string dirPath, string pattern, string label,
            System.Collections.Generic.List<string> passes,
            System.Collections.Generic.List<string> errors)
        {
            if (!Directory.Exists(dirPath))
            {
                passes.Add($"{label} (directory absent = no violation)");
                return;
            }
            var files = Directory.GetFiles(dirPath, "*.cs", SearchOption.AllDirectories);
            bool clean = true;
            foreach (var f in files)
            {
                var content = File.ReadAllText(f);
                if (content.Contains(pattern))
                {
                    errors.Add($"Forbidden '{pattern}' in {f} — violation for: {label}");
                    clean = false;
                }
            }
            if (clean) passes.Add(label);
        }
    }
}
