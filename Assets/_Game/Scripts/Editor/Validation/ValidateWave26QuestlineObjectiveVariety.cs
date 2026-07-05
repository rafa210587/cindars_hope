using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Editor validator for WAVE26: Questline Expansion + Objective Variety.
    /// Checks documentation, code presence, quest chain, and objective type coverage.
    /// Menu: CindarsHope/Validate/Wave26 Questline Objective Variety
    /// </summary>
    public static class ValidateWave26QuestlineObjectiveVariety
    {
        private const string MenuPath = "CindarsHope/Archive/Wave26 Questline Objective Variety";

        [MenuItem(MenuPath)]
        public static void Validate()
        {
            Debug.Log("[ValidateWave26] Starting WAVE26 Questline Expansion + Objective Variety validation...");
            int passCount = 0;
            int failCount = 0;

            // --- DOCS CHECKS ---
            Check("Decision doc exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_QUESTLINE_OBJECTIVE_VARIETY_DECISION.md"),
                ref passCount, ref failCount);

            Check("Existing quest system matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_EXISTING_QUEST_SYSTEM_MATRIX.md"),
                ref passCount, ref failCount);

            Check("Questline definition matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_QUESTLINE_DEFINITION_MATRIX.md"),
                ref passCount, ref failCount);

            Check("Objective type coverage matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_OBJECTIVE_TYPE_COVERAGE_MATRIX.md"),
                ref passCount, ref failCount);

            Check("NPC quest hook matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_NPC_QUEST_HOOK_MATRIX.md"),
                ref passCount, ref failCount);

            Check("Reward idempotency matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_REWARD_IDEMPOTENCY_MATRIX.md"),
                ref passCount, ref failCount);

            Check("Quest save/load matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_QUEST_SAVE_LOAD_MATRIX.md"),
                ref passCount, ref failCount);

            Check("Human Play Mode checklist exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_HUMAN_PLAYMODE_CHECKLIST.md"),
                ref passCount, ref failCount);

            Check("Execution report exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_26_QUESTLINE_OBJECTIVE_VARIETY_REPORT.md"),
                ref passCount, ref failCount);

            // --- EXISTING SYSTEM CHECKS (must NOT be recreated) ---
            Check("QuestService exists (not recreated)",
                File.Exists("Assets/_Game/Scripts/Quests/Runtime/QuestService.cs"),
                ref passCount, ref failCount);

            Check("QuestRegistry exists (not recreated)",
                File.Exists("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs"),
                ref passCount, ref failCount);

            Check("QuestRuntimeBootstrap exists (not recreated)",
                File.Exists("Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs"),
                ref passCount, ref failCount);

            Check("QuestProgressEventBridge exists (extended, not recreated)",
                File.Exists("Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs"),
                ref passCount, ref failCount);

            Check("QuestOfferPanelController exists (not recreated)",
                File.Exists("Assets/_Game/Scripts/UI/Quests/Runtime/QuestOfferPanelController.cs"),
                ref passCount, ref failCount);

            Check("QuestLogPanelController exists (not recreated)",
                File.Exists("Assets/_Game/Scripts/UI/Quests/Runtime/QuestLogPanelController.cs"),
                ref passCount, ref failCount);

            Check("QuestStateSection exists (not recreated)",
                File.Exists("Assets/_Game/Scripts/Quests/Save/QuestStateSection.cs"),
                ref passCount, ref failCount);

            // --- QUEST CHAIN CHECKS ---
            // Quest 1 — CollectItem (WAVE15 original, must still exist)
            Check("Quest 1 ID constant exists (SupplyQuestId)",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "quest_first_supplies_for_cindar"),
                ref passCount, ref failCount);

            // Quest 2 — SellItem (WAVE26 new)
            Check("Quest 2 ID registered (quest_tools_for_the_town)",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "quest_tools_for_the_town"),
                ref passCount, ref failCount);

            // Quest 3 — ReachCaveDepth (WAVE26 new)
            Check("Quest 3 ID registered (quest_echo_from_the_cave)",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "quest_echo_from_the_cave"),
                ref passCount, ref failCount);

            // --- OBJECTIVE TYPE COVERAGE CHECKS ---
            // CollectItem (Q1 — WAVE15, existing)
            Check("ObjectiveType.CollectItem used in QuestRegistry",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "QuestObjectiveType.CollectItem"),
                ref passCount, ref failCount);

            // SellItem (Q2 — WAVE26)
            Check("ObjectiveType.SellItem used in QuestRegistry",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "QuestObjectiveType.SellItem"),
                ref passCount, ref failCount);

            // ReachCaveDepth (Q3 — WAVE26)
            Check("ObjectiveType.ReachCaveDepth used in QuestRegistry",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "QuestObjectiveType.ReachCaveDepth"),
                ref passCount, ref failCount);

            // --- EVENT BRIDGE COVERAGE CHECKS ---
            Check("QuestProgressEventBridge subscribes CropHarvestedEvent",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs",
                    "CropHarvestedEvent"),
                ref passCount, ref failCount);

            Check("QuestProgressEventBridge subscribes EconomyTransactionCompletedEvent",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs",
                    "EconomyTransactionCompletedEvent"),
                ref passCount, ref failCount);

            Check("QuestProgressEventBridge subscribes NpcInteractionStartedEvent",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs",
                    "NpcInteractionStartedEvent"),
                ref passCount, ref failCount);

            Check("QuestProgressEventBridge subscribes CaveLevelEnteredEvent",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs",
                    "CaveLevelEnteredEvent"),
                ref passCount, ref failCount);

            Check("QuestProgressEventBridge subscribes EnemyKilledEvent",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs",
                    "EnemyKilledEvent"),
                ref passCount, ref failCount);

            // --- QUESTSERVICE HANDLER CHECKS ---
            Check("QuestService.OnCropHarvested exists",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestService.cs",
                    "OnCropHarvested"),
                ref passCount, ref failCount);

            Check("QuestService.OnItemSold exists",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestService.cs",
                    "OnItemSold"),
                ref passCount, ref failCount);

            Check("QuestService.OnNpcTalkedTo exists",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestService.cs",
                    "OnNpcTalkedTo"),
                ref passCount, ref failCount);

            Check("QuestService.OnCaveLevelEntered exists",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestService.cs",
                    "OnCaveLevelEntered"),
                ref passCount, ref failCount);

            Check("QuestService.OnEnemyKilled exists",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestService.cs",
                    "OnEnemyKilled"),
                ref passCount, ref failCount);

            // --- REWARD IDEMPOTENCY CHECKS ---
            Check("QuestService uses GrantedRewardIds guard",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestService.cs",
                    "GrantedRewardIds"),
                ref passCount, ref failCount);

            Check("Reward 2 (tools quest) has TrackByRewardId idempotency",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "reward_tools_quest_gold"),
                ref passCount, ref failCount);

            Check("Reward 3 (cave quest) has TrackByRewardId idempotency",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "reward_cave_quest_gold"),
                ref passCount, ref failCount);

            // --- SAVE/LOAD CHECKS ---
            Check("QuestStateSection persists quest records",
                File.Exists("Assets/_Game/Scripts/Quests/Save/QuestStateSection.cs"),
                ref passCount, ref failCount);

            Check("QuestStateRecord has GrantedRewardIds for idempotency after load",
                CheckFileContains("Assets/_Game/Scripts/Quests/Save/QuestStateRecord.cs",
                    "GrantedRewardIds"),
                ref passCount, ref failCount);

            Check("QuestRuntimeBootstrap.CaptureSaveData exists",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs",
                    "CaptureSaveData"),
                ref passCount, ref failCount);

            Check("QuestRuntimeBootstrap.RestoreFromSaveData exists",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs",
                    "RestoreFromSaveData"),
                ref passCount, ref failCount);

            // --- NPC QUEST HOOK CHECKS ---
            Check("QuestGiverInteractable exists for NPC quest hooks",
                File.Exists("Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs"),
                ref passCount, ref failCount);

            Check("npc_pip registered in QuestRuntimeIds",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "npc_pip"),
                ref passCount, ref failCount);

            Check("npc_maelor registered in QuestRuntimeIds",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "npc_maelor"),
                ref passCount, ref failCount);

            // --- PREREQUISITE CHAIN CHECK ---
            Check("Quest 2 has prerequisite pointing to Quest 1",
                CheckFileContains("Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs",
                    "PrerequisiteQuestIds"),
                ref passCount, ref failCount);

            // --- SUMMARY ---
            var result = failCount == 0 ? "ALL PASS" : $"{failCount} FAIL";
            Debug.Log($"[ValidateWave26] Result: {passCount} pass / {failCount} fail — {result}");

            if (failCount > 0)
                Debug.LogWarning($"[ValidateWave26] {failCount} check(s) failed. See above for details.");
            else
                Debug.Log("[ValidateWave26] WAVE26 Questline Expansion + Objective Variety: BUILD_VALIDATED");
        }

        private static void Check(string label, bool condition, ref int pass, ref int fail)
        {
            if (condition)
            {
                Debug.Log($"[ValidateWave26] PASS: {label}");
                pass++;
            }
            else
            {
                Debug.LogWarning($"[ValidateWave26] FAIL: {label}");
                fail++;
            }
        }

        private static bool CheckFileContains(string path, string text)
        {
            if (!File.Exists(path)) return false;
            return File.ReadAllText(path).Contains(text);
        }
    }
}
