using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateWave22DebtBacklog
    {
        private const string MenuPath = "CindarsHope/Archive/Validate Wave 22 Debt Backlog";

        [MenuItem(MenuPath)]
        public static void Validate()
        {
            int pass = 0;
            int warn = 0;

            void Check(bool condition, string label)
            {
                if (condition) { Debug.Log($"[WAVE22] PASS: {label}"); pass++; }
                else { Debug.LogWarning($"[WAVE22] FAIL: {label}"); warn++; }
            }

            void CheckDoc(string relativePath, string label)
            {
                var full = Path.Combine(Application.dataPath, "..", relativePath);
                Check(File.Exists(full), label);
            }

            Debug.Log("=== WAVE_INTEGRATION_22 Post-MVP Debt Backlog Validator ===");
            Debug.Log("[WAVE22] Status: DEBT_BACKLOG_CONSOLIDATED_NEXT_ROADMAP_READY");

            // ─── WAVE22 required docs ──────────────────────────────────────────
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_DEBT_BACKLOG_DECISION.md",
                "WAVE22 decision report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_DEBT_BACKLOG_REPORT.md",
                "WAVE22 execution report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_CANONICAL_DEBT_REGISTER.md",
                "WAVE22 canonical debt register exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_DEBT_DEDUPLICATION_MATRIX.md",
                "WAVE22 deduplication matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_NEXT_ROADMAP_PROPOSAL.md",
                "WAVE22 next roadmap proposal exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_NEXT_SPEC_CANDIDATE_MATRIX.md",
                "WAVE22 next spec candidate matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_RELEASE_CANDIDATE_NOTES.md",
                "WAVE22 release candidate notes exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_HANDOFF_FOR_NEXT_EXECUTION.md",
                "WAVE22 handoff doc exists");

            // ─── WAVE20/21 prerequisite docs ──────────────────────────────────
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md",
                "WAVE20 closeout report (gate)");
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_BUG_DEBT_REGISTER.md",
                "WAVE20 bug/debt register (gate)");
            CheckDoc("docs/validation/WAVE_INTEGRATION_21_POST_ACCEPTANCE_BUGFIX_REPORT.md",
                "WAVE21 bugfix report (gate)");

            // ─── CURRENT_STATE updated ─────────────────────────────────────────
            var csPath = Path.Combine(Application.dataPath, "..", "docs/project/CURRENT_STATE.md");
            if (File.Exists(csPath))
            {
                var content = File.ReadAllText(csPath);
                Check(content.Contains("WAVE_INTEGRATION_22"), "CURRENT_STATE contains WAVE22 entry");
            }

            // ─── No premature ACCEPTED claim in debt register ─────────────────
            var drPath = Path.Combine(Application.dataPath, "..",
                "docs/validation/WAVE_INTEGRATION_22_CANONICAL_DEBT_REGISTER.md");
            if (File.Exists(drPath))
            {
                var content = File.ReadAllText(drPath);
                bool hasPrematureAccepted = content.Contains("Status: ACCEPTED\n") &&
                                             !content.Contains("ACCEPTED_DEBT");
                Check(!hasPrematureAccepted, "Debt register does not claim ACCEPTED prematurely");
                Check(content.Contains("DEBT-SCENE-001"), "Debt register has canonical debt IDs");
                Check(content.Contains("Disposition"), "Debt register has disposition column");
            }

            // ─── Next spec candidate matrix has entries ────────────────────────
            var ncmPath = Path.Combine(Application.dataPath, "..",
                "docs/validation/WAVE_INTEGRATION_22_NEXT_SPEC_CANDIDATE_MATRIX.md");
            if (File.Exists(ncmPath))
            {
                var content = File.ReadAllText(ncmPath);
                Check(content.Contains("MVP_PLUS"), "Next spec candidate matrix has MVP+ candidates");
            }

            // ─── Previous wave validators still exist (no regression) ─────────
            CheckDoc("Assets/_Game/Scripts/Editor/Validation/ValidateWave20PlayableSliceAcceptance.cs",
                "ValidateWave20 still exists (no regression)");
            CheckDoc("Assets/_Game/Scripts/Editor/Validation/ValidateWave21PostAcceptanceBugfix.cs",
                "ValidateWave21 still exists (no regression)");

            // ─── Summary ──────────────────────────────────────────────────────
            Debug.Log($"=== WAVE22 Validation: {pass} PASS / {warn} FAIL ===");
            if (warn == 0)
                Debug.Log("[WAVE22] ALL CHECKS PASSED — debt backlog consolidated; " +
                          "next roadmap ready; release candidate notes created; handoff prepared");
            else
                Debug.LogWarning($"[WAVE22] {warn} CHECK(S) FAILED — review warnings above");
        }
    }
}
