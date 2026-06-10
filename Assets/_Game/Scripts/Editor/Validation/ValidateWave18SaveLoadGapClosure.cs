using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Editor-only validator for WAVE_INTEGRATION_18 Save/Load Gap Closure.
    /// Menu: [CindarsHope]/Validate Wave 18 Save Load Gap Closure
    /// </summary>
    public static class ValidateWave18SaveLoadGapClosure
    {
        [MenuItem("CindarsHope/Validate Wave 18 Save Load Gap Closure")]
        public static void ValidateAll()
        {
            var issues = 0;
            var checks = 0;

            // ─── 1. GameSaveData has Quests field ────────────────────────────────
            checks++;
            var gameSaveDataType = FindType("CindarsHope.Save.GameSaveData");
            if (gameSaveDataType == null)
            {
                LogFail("GameSaveData type not found");
                issues++;
            }
            else
            {
                var questsField = gameSaveDataType.GetField("Quests", BindingFlags.Public | BindingFlags.Instance);
                if (questsField == null)
                {
                    LogFail("GameSaveData.Quests field missing — quest save gap still open");
                    issues++;
                }
                else
                {
                    LogPass("GameSaveData.Quests field present");
                }
            }

            // ─── 2. QuestStateSectionSaveData exists and is serializable ─────────
            checks++;
            var questDtoType = FindType("CindarsHope.Save.QuestStateSectionSaveData");
            if (questDtoType == null)
            {
                LogFail("QuestStateSectionSaveData type not found");
                issues++;
            }
            else if (!questDtoType.IsSerializable)
            {
                LogFail("QuestStateSectionSaveData is not [Serializable]");
                issues++;
            }
            else
            {
                LogPass("QuestStateSectionSaveData found and [Serializable]");
            }

            // ─── 3. QuestRuntimeBootstrap has CaptureSaveData ───────────────────
            checks++;
            var bootstrapType = FindType("CindarsHope.Quests.Runtime.QuestRuntimeBootstrap");
            if (bootstrapType == null)
            {
                LogFail("QuestRuntimeBootstrap type not found");
                issues++;
            }
            else
            {
                var captureMethod = bootstrapType.GetMethod("CaptureSaveData", BindingFlags.Public | BindingFlags.Static);
                var restoreMethod = bootstrapType.GetMethod("RestoreFromSaveData", BindingFlags.Public | BindingFlags.Static);
                if (captureMethod == null || restoreMethod == null)
                {
                    LogFail($"QuestRuntimeBootstrap missing Capture/Restore static APIs. Capture={captureMethod != null} Restore={restoreMethod != null}");
                    issues++;
                }
                else
                {
                    LogPass("QuestRuntimeBootstrap has CaptureSaveData + RestoreFromSaveData");
                }
            }

            // ─── 4. QuestService has RestoreFromSaveData ───────────────────────
            checks++;
            var questServiceType = FindType("CindarsHope.Quests.Runtime.QuestService");
            if (questServiceType == null)
            {
                LogFail("QuestService type not found");
                issues++;
            }
            else
            {
                var restoreMethod = questServiceType.GetMethod("RestoreFromSaveData", BindingFlags.Public | BindingFlags.Instance);
                if (restoreMethod == null)
                {
                    LogFail("QuestService.RestoreFromSaveData not found — restore API missing");
                    issues++;
                }
                else
                {
                    LogPass("QuestService.RestoreFromSaveData present");
                }
            }

            // ─── 5. NpcManager has CaptureSaveData ──────────────────────────────
            checks++;
            var npcManagerType = FindType("CindarsHope.NPC.NpcManager");
            if (npcManagerType == null)
            {
                LogFail("NpcManager type not found");
                issues++;
            }
            else
            {
                var captureMethod = npcManagerType.GetMethod("CaptureSaveData", BindingFlags.Public | BindingFlags.Instance);
                var restoreMethod = npcManagerType.GetMethod("RestoreFromSaveData", BindingFlags.Public | BindingFlags.Instance);
                if (captureMethod == null || restoreMethod == null)
                {
                    LogFail($"NpcManager missing Capture/Restore. Capture={captureMethod != null} Restore={restoreMethod != null}");
                    issues++;
                }
                else
                {
                    LogPass("NpcManager has CaptureSaveData + RestoreFromSaveData");
                }
            }

            // ─── 6. CaveRunManager (cave state) ────────────────────────────────
            checks++;
            var caveRunType = FindType("CindarsHope.Cave.Runtime.CaveRunManager");
            if (caveRunType == null)
            {
                LogFail("CaveRunManager type not found");
                issues++;
            }
            else
            {
                var captureMethod = caveRunType.GetMethod("CaptureSaveData", BindingFlags.Public | BindingFlags.Instance);
                var restoreMethod = caveRunType.GetMethod("RestoreFromSaveData", BindingFlags.Public | BindingFlags.Instance);
                if (captureMethod == null || restoreMethod == null)
                {
                    LogFail($"CaveRunManager missing Capture/Restore. Capture={captureMethod != null} Restore={restoreMethod != null}");
                    issues++;
                }
                else
                {
                    LogPass("CaveRunManager has CaptureSaveData + RestoreFromSaveData");
                }
            }

            // ─── 7. No Unity refs in quest DTOs ────────────────────────────────
            checks++;
            var unityObjectType = typeof(UnityEngine.Object);
            var hasBadRef = false;
            foreach (var dtoTypeName in new[] { "CindarsHope.Save.QuestStateSaveData", "CindarsHope.Save.QuestObjectiveStateSaveData", "CindarsHope.Save.QuestStateSectionSaveData" })
            {
                var t = FindType(dtoTypeName);
                if (t == null) continue;
                foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (unityObjectType.IsAssignableFrom(field.FieldType))
                    {
                        LogFail($"{dtoTypeName}.{field.Name} is a Unity Object reference — violates save-dto-simple-types-only rule");
                        hasBadRef = true;
                        issues++;
                    }
                }
            }
            if (!hasBadRef)
            {
                LogPass("Quest DTOs contain no Unity Object references");
            }

            // ─── 8. Required docs present ──────────────────────────────────────
            var requiredDocs = new[]
            {
                "docs/validation/WAVE_INTEGRATION_18_SAVE_LOAD_GAP_DECISION.md",
                "docs/validation/WAVE_INTEGRATION_18_SAVE_LOAD_GAP_REPORT.md",
                "docs/validation/WAVE_INTEGRATION_18_SAVE_DTO_AUDIT.md",
                "docs/validation/WAVE_INTEGRATION_18_EXISTING_FUNCTIONALITY_MATRIX.md",
                "docs/validation/WAVE_INTEGRATION_18_QUEST_SAVE_MATRIX.md",
                "docs/validation/WAVE_INTEGRATION_18_NPC_SAVE_MATRIX.md",
                "docs/validation/WAVE_INTEGRATION_18_CAVE_ENEMY_LOOT_SAVE_MATRIX.md",
                "docs/validation/WAVE_INTEGRATION_18_SCENE_BOUND_PRESERVATION_MATRIX.md",
                "docs/validation/WAVE_INTEGRATION_18_NEGATIVE_SAVE_LOAD_TESTS.md",
                "docs/validation/WAVE_INTEGRATION_18_HUMAN_PLAYMODE_CHECKLIST.md"
            };

            foreach (var doc in requiredDocs)
            {
                checks++;
                var fullPath = Path.Combine(Application.dataPath, "..", doc);
                if (!File.Exists(fullPath))
                {
                    LogFail($"Required doc missing: {doc}");
                    issues++;
                }
                else
                {
                    LogPass($"Doc present: {doc}");
                }
            }

            // ─── Summary ──────────────────────────────────────────────────────
            if (issues == 0)
            {
                Debug.Log($"[ValidateWave18] ALL {checks} checks PASS — WAVE18 Save/Load gap closure validated.");
            }
            else
            {
                Debug.LogWarning($"[ValidateWave18] {issues}/{checks} checks FAILED. Review issues above.");
            }
        }

        private static System.Type FindType(string fullTypeName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = assembly.GetType(fullTypeName);
                if (t != null) return t;
            }
            return null;
        }

        private static void LogPass(string message) => Debug.Log($"[ValidateWave18] PASS: {message}");
        private static void LogFail(string message) => Debug.LogError($"[ValidateWave18] FAIL: {message}");
    }
}
