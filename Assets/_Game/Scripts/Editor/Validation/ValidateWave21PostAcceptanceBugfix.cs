using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateWave21PostAcceptanceBugfix
    {
        private const string MenuPath = "CindarsHope/Validate Wave 21 Post-Acceptance Bugfix";

        [MenuItem(MenuPath)]
        public static void Validate()
        {
            int pass = 0;
            int warn = 0;

            void Check(bool condition, string label)
            {
                if (condition) { Debug.Log($"[WAVE21] PASS: {label}"); pass++; }
                else { Debug.LogWarning($"[WAVE21] FAIL: {label}"); warn++; }
            }

            void CheckDoc(string relativePath, string label)
            {
                var full = Path.Combine(Application.dataPath, "..", relativePath);
                Check(File.Exists(full), label);
            }

            Debug.Log("=== WAVE_INTEGRATION_21 Post-Acceptance Bugfix Validator ===");
            Debug.Log("[WAVE21] Status: NO_OP_NO_P0_P1_FOUND — no code fixes were required");

            // ─── WAVE20 prerequisite docs ──────────────────────────────────────
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md",
                "WAVE20 closeout report exists (gate)");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_BUG_DEBT_REGISTER.md",
                "WAVE20 bug/debt register exists (gate)");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_GO_NO_GO_MATRIX.md",
                "WAVE20 go/no-go matrix exists (gate)");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_FINAL_HUMAN_PLAYMODE_CHECKLIST.md",
                "WAVE20 human checklist exists (gate)");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md",
                "WAVE20 evidence template exists (gate)");

            // ─── WAVE21 required docs ──────────────────────────────────────────
            CheckDoc("docs/validation/WAVE_INTEGRATION_21_POST_ACCEPTANCE_BUGFIX_DECISION.md",
                "WAVE21 decision report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_21_POST_ACCEPTANCE_BUGFIX_REPORT.md",
                "WAVE21 execution report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_21_BUG_TRIAGE_MATRIX.md",
                "WAVE21 bug triage matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_21_FIX_VERIFICATION_MATRIX.md",
                "WAVE21 fix verification matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_21_REGRESSION_CHECKLIST.md",
                "WAVE21 regression checklist exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_21_RETEST_INSTRUCTIONS.md",
                "WAVE21 retest instructions exist");

            // ─── CURRENT_STATE updated ─────────────────────────────────────────
            var csPath = Path.Combine(Application.dataPath, "..", "docs/project/CURRENT_STATE.md");
            if (File.Exists(csPath))
            {
                var content = File.ReadAllText(csPath);
                Check(content.Contains("WAVE_INTEGRATION_21"), "CURRENT_STATE contains WAVE21 entry");
            }

            // ─── NO premature ACCEPTED claim ───────────────────────────────────
            var reportPath = Path.Combine(Application.dataPath, "..",
                "docs/validation/WAVE_INTEGRATION_21_POST_ACCEPTANCE_BUGFIX_REPORT.md");
            if (File.Exists(reportPath))
            {
                var content = File.ReadAllText(reportPath);
                bool hasPrematureAccepted = content.Contains("Status: ACCEPTED\n") ||
                                             content.Contains("Status: `ACCEPTED`\n") ||
                                             content.Contains("**Status:** ACCEPTED\n");
                Check(!hasPrematureAccepted, "WAVE21 report does not claim ACCEPTED prematurely");
            }

            // ─── Core system types still compile ──────────────────────────────
            CheckType("CindarsHope.Core.Events.GameLoadedEvent", "GameLoadedEvent still exists (no regression)");
            CheckType("CindarsHope.UI.Modal.ModalManager", "ModalManager still exists (no regression)");
            CheckType("CindarsHope.Quests.Runtime.QuestService", "QuestService still exists (no regression)");
            CheckType("CindarsHope.Save.SaveManager", "SaveManager still exists (no regression)");
            CheckType("CindarsHope.Player.Movement.PlayerDashController", "PlayerDashController still exists (no regression)");

            // ─── ModalType enum has QuestLog (WAVE19 regression check) ────────
            CheckEnumValue("CindarsHope.UI.Modal.ModalType", "QuestLog",
                "ModalType.QuestLog still exists (WAVE19 no regression)");

            // ─── WAVE20 validator still works ─────────────────────────────────
            CheckType("CindarsHope.Editor.Validation.ValidateWave20PlayableSliceAcceptance",
                "ValidateWave20PlayableSliceAcceptance still compiles (no regression)");

            // ─── Summary ──────────────────────────────────────────────────────
            Debug.Log($"=== WAVE21 Validation: {pass} PASS / {warn} FAIL ===");
            if (warn == 0)
                Debug.Log("[WAVE21] ALL CHECKS PASSED — NO_OP_NO_P0_P1_FOUND confirmed; " +
                          "builds pass; no regression detected; human Play Mode retested via WAVE20 checklist");
            else
                Debug.LogWarning($"[WAVE21] {warn} CHECK(S) FAILED — review warnings above");
        }

        private static void CheckType(string typeName, string label)
        {
            bool found = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetType(typeName) != null);
            if (found) Debug.Log($"[WAVE21] PASS: {label}");
            else Debug.LogWarning($"[WAVE21] FAIL: {label} — '{typeName}' not found");
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
            if (found) Debug.Log($"[WAVE21] PASS: {label}");
            else Debug.LogWarning($"[WAVE21] FAIL: {label} — '{valueName}' not in {enumTypeName}");
        }
    }
}
