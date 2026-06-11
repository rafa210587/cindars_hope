using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Editor validator for WAVE25: Town NPC Schedules + Dialogue Expansion.
    /// Checks documentation, code presence, and canonical NPC coverage.
    /// Menu: CindarsHope/Validate/Wave25 Town NPC Schedules Dialogue
    /// </summary>
    public static class ValidateWave25TownNpcSchedulesDialogue
    {
        private const string MenuPath = "CindarsHope/Validate/Wave25 Town NPC Schedules Dialogue";

        [MenuItem(MenuPath)]
        public static void Validate()
        {
            Debug.Log("[ValidateWave25] Starting WAVE25 Town NPC Schedules + Dialogue validation...");
            int passCount = 0;
            int failCount = 0;

            // --- DOCS CHECKS ---
            Check("Decision doc exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_TOWN_NPC_SCHEDULES_DIALOGUE_DECISION.md"),
                ref passCount, ref failCount);

            Check("Existing NPC functionality matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_EXISTING_NPC_FUNCTIONALITY_MATRIX.md"),
                ref passCount, ref failCount);

            Check("Canonical NPC roster matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_CANONICAL_NPC_ROSTER_MATRIX.md"),
                ref passCount, ref failCount);

            Check("NPC positioning anchor matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_NPC_POSITIONING_ANCHOR_MATRIX.md"),
                ref passCount, ref failCount);

            Check("NPC dialogue expansion matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_NPC_DIALOGUE_EXPANSION_MATRIX.md"),
                ref passCount, ref failCount);

            Check("NPC service shop quest matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_NPC_SERVICE_SHOP_QUEST_MATRIX.md"),
                ref passCount, ref failCount);

            Check("NPC schedule save/load matrix exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_NPC_SCHEDULE_SAVE_LOAD_MATRIX.md"),
                ref passCount, ref failCount);

            Check("Human Play Mode checklist exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_HUMAN_PLAYMODE_CHECKLIST.md"),
                ref passCount, ref failCount);

            Check("Execution report exists",
                File.Exists("docs/validation/WAVE_INTEGRATION_25_TOWN_NPC_SCHEDULES_DIALOGUE_REPORT.md"),
                ref passCount, ref failCount);

            // --- EXISTING SYSTEM CHECKS ---
            Check("NpcManager.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/NpcManager.cs"),
                ref passCount, ref failCount);

            Check("NpcController.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/NpcController.cs"),
                ref passCount, ref failCount);

            Check("NpcShopController.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/NpcShopController.cs"),
                ref passCount, ref failCount);

            Check("NpcWanderer.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/NpcWanderer.cs"),
                ref passCount, ref failCount);

            Check("NpcScenePlacementMarker.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Runtime/NpcScenePlacementMarker.cs"),
                ref passCount, ref failCount);

            // --- WAVE25 NEW CODE CHECKS ---
            Check("NpcScheduleService.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Schedule/NpcScheduleService.cs"),
                ref passCount, ref failCount);

            Check("NpcScheduleAnchor.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Schedule/NpcScheduleAnchor.cs"),
                ref passCount, ref failCount);

            Check("NpcScheduleProfile.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Schedule/NpcScheduleProfile.cs"),
                ref passCount, ref failCount);

            Check("NpcScheduleBlock.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Schedule/NpcScheduleBlock.cs"),
                ref passCount, ref failCount);

            Check("NpcScheduleRuntimeState.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Schedule/NpcScheduleRuntimeState.cs"),
                ref passCount, ref failCount);

            Check("NpcScheduleRuntimeBootstrap.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/Schedule/NpcScheduleRuntimeBootstrap.cs"),
                ref passCount, ref failCount);

            Check("NpcTownRosterRegistry.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/NpcTownRosterRegistry.cs"),
                ref passCount, ref failCount);

            Check("NpcDialogueSetRegistry.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/NpcDialogueSetRegistry.cs"),
                ref passCount, ref failCount);

            Check("NpcDialogueExpansionBootstrap.cs exists",
                File.Exists("Assets/_Game/Scripts/NPC/NpcDialogueExpansionBootstrap.cs"),
                ref passCount, ref failCount);

            // --- NPC DATA ASSET CHECKS (7 MVP) ---
            CheckAsset("Npc_Pip_Miudinho asset exists", "Assets/_Game/Data", "Npc_Pip_Miudinho.asset", ref passCount, ref failCount);
            CheckAsset("Npc_Sylveth asset exists", "Assets/_Game/Data", "Npc_Sylveth.asset", ref passCount, ref failCount);
            CheckAsset("Npc_Brumdar asset exists", "Assets/_Game/Data", "Npc_Brumdar.asset", ref passCount, ref failCount);
            CheckAsset("Npc_Renko asset exists", "Assets/_Game/Data", "Npc_Renko.asset", ref passCount, ref failCount);
            CheckAsset("Npc_Thalindra asset exists", "Assets/_Game/Data", "Npc_Thalindra.asset", ref passCount, ref failCount);
            CheckAsset("Npc_Zrix asset exists", "Assets/_Game/Data", "Npc_Zrix.asset", ref passCount, ref failCount);

            // --- DIALOGUE TREE CHECKS (5 MVP) ---
            CheckAsset("DialogueTree_Pip asset exists", "Assets/_Game/Data", "DialogueTree_Pip.asset", ref passCount, ref failCount);
            CheckAsset("DialogueTree_Sylveth asset exists", "Assets/_Game/Data", "DialogueTree_Sylveth.asset", ref passCount, ref failCount);
            CheckAsset("DialogueTree_Brumdar asset exists", "Assets/_Game/Data", "DialogueTree_Brumdar.asset", ref passCount, ref failCount);
            CheckAsset("DialogueTree_Renko asset exists", "Assets/_Game/Data", "DialogueTree_Renko.asset", ref passCount, ref failCount);
            CheckAsset("DialogueTree_Thalindra asset exists", "Assets/_Game/Data", "DialogueTree_Thalindra.asset", ref passCount, ref failCount);

            // --- THALINDRA QUEST/SHOP FLOW ---
            // Checked by code presence (NpcShopController has the full flow implemented)
            Check("NpcShopController implements Thalindra quest/shop flow",
                ContainsText("Assets/_Game/Scripts/NPC/NpcShopController.cs", "ShowThalindraQuestShopDialogue"),
                ref passCount, ref failCount);

            Check("NpcShopController implements Thalindra buy/sell/adeus",
                ContainsText("Assets/_Game/Scripts/NPC/NpcShopController.cs", "\"Comprar\"") &&
                ContainsText("Assets/_Game/Scripts/NPC/NpcShopController.cs", "\"Vender\"") &&
                ContainsText("Assets/_Game/Scripts/NPC/NpcShopController.cs", "\"Adeus\""),
                ref passCount, ref failCount);

            // --- CITY/SCHEDULE SYSTEM ---
            Check("City/Schedule NpcScheduleDefinition exists",
                File.Exists("Assets/_Game/Scripts/City/Schedule/NpcScheduleDefinition.cs"),
                ref passCount, ref failCount);

            Check("City/Schedule SchedulePeriod exists",
                File.Exists("Assets/_Game/Scripts/City/Schedule/SchedulePeriod.cs"),
                ref passCount, ref failCount);

            // --- SAVE/LOAD ---
            Check("NpcManagerSaveData in SaveData.cs",
                ContainsText("Assets/_Game/Scripts/Save/SaveData.cs", "NpcManagerSaveData"),
                ref passCount, ref failCount);

            Check("NpcSaveData has HasMet field",
                ContainsText("Assets/_Game/Scripts/Save/SaveData.cs", "HasMet"),
                ref passCount, ref failCount);

            // --- CURRENT_STATE CHECK ---
            Check("CURRENT_STATE.md has WAVE25 entry",
                ContainsText("docs/project/CURRENT_STATE.md", "WAVE_INTEGRATION_25"),
                ref passCount, ref failCount);

            Debug.Log($"[ValidateWave25] Complete. PASS: {passCount} | FAIL: {failCount}");
            if (failCount > 0)
            {
                Debug.LogError($"[ValidateWave25] {failCount} check(s) failed. Review above logs.");
            }
        }

        private static void Check(string label, bool condition, ref int passCount, ref int failCount)
        {
            if (condition)
            {
                Debug.Log($"[ValidateWave25] PASS: {label}");
                passCount++;
            }
            else
            {
                Debug.LogError($"[ValidateWave25] FAIL: {label}");
                failCount++;
            }
        }

        private static void CheckAsset(string label, string basePath, string fileName, ref int passCount, ref int failCount)
        {
            var found = false;
            if (Directory.Exists(basePath))
            {
                var files = Directory.GetFiles(basePath, fileName, SearchOption.AllDirectories);
                found = files.Length > 0;
            }

            Check(label, found, ref passCount, ref failCount);
        }

        private static bool ContainsText(string filePath, string searchText)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                var content = File.ReadAllText(filePath);
                return content.Contains(searchText);
            }
            catch
            {
                return false;
            }
        }
    }
}
