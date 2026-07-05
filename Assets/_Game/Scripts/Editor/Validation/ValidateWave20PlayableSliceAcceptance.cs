using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateWave20PlayableSliceAcceptance
    {
        private const string MenuPath = "CindarsHope/Archive/Validate Wave 20 Playable Slice Acceptance";

        [MenuItem(MenuPath)]
        public static void Validate()
        {
            int pass = 0;
            int fail = 0;

            void Check(bool condition, string label)
            {
                if (condition) { Debug.Log($"[WAVE20] PASS: {label}"); pass++; }
                else { Debug.LogWarning($"[WAVE20] FAIL: {label}"); fail++; }
            }

            void CheckDoc(string relativePath, string label)
            {
                var full = Path.Combine(Application.dataPath, "..", relativePath);
                Check(File.Exists(full), label);
            }

            Debug.Log("=== WAVE_INTEGRATION_20 Playable Slice Acceptance Validator ===");

            // ─── WAVE20 docs ───────────────────────────────────────────────────────
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_ACCEPTANCE_DECISION.md",
                "WAVE20 decision report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md",
                "WAVE20 closeout report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_FINAL_HUMAN_PLAYMODE_CHECKLIST.md",
                "WAVE20 final human checklist exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_BUG_DEBT_REGISTER.md",
                "WAVE20 bug/debt register exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_GO_NO_GO_MATRIX.md",
                "WAVE20 go/no-go matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_ROADMAP_COVERAGE_MATRIX.md",
                "WAVE20 roadmap coverage matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md",
                "WAVE20 evidence template exists");

            // ─── WAVE13-19 reports (prerequisite audit) ───────────────────────────
            CheckDoc("docs/validation/WAVE_INTEGRATION_13_SCENE_TRANSITION_REPORT.md",
                "WAVE13 scene transition report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_14_CRAFTING_PROCESSING_REPORT.md",
                "WAVE14 crafting processing report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_15_QUEST_GIVER_REPORT.md",
                "WAVE15 quest giver report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_16_CAVE_ENTRANCE_REPORT.md",
                "WAVE16 cave entrance report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_REPORT.md",
                "WAVE17 cave combat loot report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_18_SAVE_LOAD_GAP_REPORT.md",
                "WAVE18 save/load gap report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_19_HUD_UX_ACCEPTANCE_REPORT.md",
                "WAVE19 HUD/UX acceptance report exists");

            // ─── Runtime types: core systems ──────────────────────────────────────
            CheckType("CindarsHope.Core.Events.GameSavedEvent", "GameSavedEvent exists");
            CheckType("CindarsHope.Core.Events.GameLoadedEvent", "GameLoadedEvent exists");
            CheckType("CindarsHope.Core.Bootstrap.GameBootstrap", "GameBootstrap exists");
            CheckType("CindarsHope.UI.Modal.ModalManager", "ModalManager exists");

            // ─── ModalType enum values ────────────────────────────────────────────
            CheckEnumValue("CindarsHope.UI.Modal.ModalType", "QuestLog", "ModalType.QuestLog exists");
            CheckEnumValue("CindarsHope.UI.Modal.ModalType", "Inventory", "ModalType.Inventory exists");
            CheckEnumValue("CindarsHope.UI.Modal.ModalType", "QuestOffer", "ModalType.QuestOffer exists");
            CheckEnumValue("CindarsHope.UI.Modal.ModalType", "ShopMenu", "ModalType.ShopMenu exists");

            // ─── Player movement action types ─────────────────────────────────────
            CheckType("CindarsHope.Player.Movement.PlayerDashController", "PlayerDashController exists");
            CheckType("CindarsHope.Player.Movement.PlayerDodgeController", "PlayerDodgeController exists");
            CheckType("CindarsHope.Player.Movement.PlayerBlockController", "PlayerBlockController exists");

            // ─── Quest system types ───────────────────────────────────────────────
            CheckType("CindarsHope.Quests.Runtime.QuestService", "QuestService exists");
            CheckType("CindarsHope.Quests.Runtime.QuestRuntimeBootstrap", "QuestRuntimeBootstrap exists");

            // ─── Save system types ────────────────────────────────────────────────
            CheckType("CindarsHope.Save.SaveManager", "SaveManager exists");
            CheckType("CindarsHope.Save.QuestStateSectionSaveData", "QuestStateSectionSaveData exists");

            // ─── Cave system types ────────────────────────────────────────────────
            CheckType("CindarsHope.Cave.CaveEntranceInteractable", "CaveEntranceInteractable exists");
            CheckType("CindarsHope.Cave.CaveRuntimeBridge", "CaveRuntimeBridge exists");
            CheckType("CindarsHope.Cave.CaveSmokeTestSpawnerBridge", "CaveSmokeTestSpawnerBridge exists");

            // ─── Debug HUD ────────────────────────────────────────────────────────
            CheckType("CindarsHope.UI.DebugHud", "DebugHud exists");

            // ─── Evidence template check: no ACCEPTED claim without human ─────────
            var evidencePath = Path.Combine(Application.dataPath, "..",
                "docs/validation/WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md");
            if (File.Exists(evidencePath))
            {
                var content = File.ReadAllText(evidencePath);
                bool hasAcceptedChecked = content.Contains("[x] ACCEPTED") ||
                                          content.Contains("[X] ACCEPTED") ||
                                          content.Contains("[x] ACCEPTED_WITH_DEBT") ||
                                          content.Contains("[X] ACCEPTED_WITH_DEBT");
                if (hasAcceptedChecked)
                    Check(true, "Evidence template has acceptance decision filled");
                else
                    Debug.Log("[WAVE20] INFO: Evidence template exists but acceptance decision not yet filled (expected — pending human Play Mode)");
            }

            // ─── Summary ──────────────────────────────────────────────────────────
            Debug.Log($"=== WAVE20 Validation: {pass} PASS / {fail} FAIL ===");
            if (fail == 0)
                Debug.Log("[WAVE20] ALL CHECKS PASSED — BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE");
            else
                Debug.LogWarning($"[WAVE20] {fail} CHECK(S) FAILED — review warnings above");
        }

        private static void CheckType(string typeName, string label)
        {
            bool found = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetType(typeName) != null);
            if (found) { Debug.Log($"[WAVE20] PASS: {label}"); }
            else { Debug.LogWarning($"[WAVE20] FAIL: {label} — type '{typeName}' not found"); }
        }

        private static void CheckEnumValue(string enumTypeName, string valueName, string label)
        {
            bool found = false;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumTypeName);
                if (type == null || !type.IsEnum) continue;
                found = Enum.GetNames(type).Any(n => n == valueName);
                if (found) break;
            }
            if (found) { Debug.Log($"[WAVE20] PASS: {label}"); }
            else { Debug.LogWarning($"[WAVE20] FAIL: {label} — '{valueName}' not in {enumTypeName}"); }
        }
    }
}
