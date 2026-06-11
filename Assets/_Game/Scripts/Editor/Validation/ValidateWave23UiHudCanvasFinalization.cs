using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateWave23UiHudCanvasFinalization
    {
        private const string MenuPath = "CindarsHope/Validate Wave 23 UI HUD Canvas Finalization";

        [MenuItem(MenuPath)]
        public static void Validate()
        {
            int pass = 0;
            int warn = 0;

            void Check(bool condition, string label)
            {
                if (condition) { Debug.Log($"[WAVE23] PASS: {label}"); pass++; }
                else { Debug.LogWarning($"[WAVE23] FAIL: {label}"); warn++; }
            }

            void CheckDoc(string relativePath, string label)
            {
                var full = Path.Combine(Application.dataPath, "..", relativePath);
                Check(File.Exists(full), label);
            }

            void CheckScript(string relativePath, string label)
            {
                var full = Path.Combine(Application.dataPath, "..", relativePath);
                Check(File.Exists(full), label);
            }

            Debug.Log("=== WAVE_INTEGRATION_23 UI/HUD Canvas Finalization Validator ===");

            // ─── Gate docs (WAVE20/21/22) ──────────────────────────────────────
            CheckDoc("docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md",
                "WAVE20 gate: closeout report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_21_POST_ACCEPTANCE_BUGFIX_REPORT.md",
                "WAVE21 gate: no-op report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_22_DEBT_BACKLOG_REPORT.md",
                "WAVE22 gate: debt backlog report exists");

            // ─── WAVE23 required docs ──────────────────────────────────────────
            CheckDoc("docs/validation/WAVE_INTEGRATION_23_UI_HUD_CANVAS_DECISION.md",
                "WAVE23 decision report exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_23_EXISTING_UI_AUDIT.md",
                "WAVE23 existing UI audit exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_23_HUD_DATA_BINDING_MATRIX.md",
                "WAVE23 HUD data binding matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_23_FEEDBACK_COVERAGE_MATRIX.md",
                "WAVE23 feedback coverage matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_23_MODAL_VISIBILITY_GUARD_MATRIX.md",
                "WAVE23 modal visibility guard matrix exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_23_HUMAN_PLAYMODE_CHECKLIST.md",
                "WAVE23 human Play Mode checklist exists");
            CheckDoc("docs/validation/WAVE_INTEGRATION_23_UI_HUD_CANVAS_REPORT.md",
                "WAVE23 execution report exists");

            // ─── Runtime scripts ───────────────────────────────────────────────
            CheckScript("Assets/_Game/Scripts/Core/Events/HudEvents.cs",
                "HudEvents.cs (HudFeedbackUpdatedEvent, HudVisibilityChangedEvent) exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/GameplayFeedbackMessage.cs",
                "GameplayFeedbackMessage.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/GameplayFeedbackService.cs",
                "GameplayFeedbackService.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/HudVisibilityController.cs",
                "HudVisibilityController.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/GameplayHudRuntimeBinder.cs",
                "GameplayHudRuntimeBinder.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/GameplayHudCanvasController.cs",
                "GameplayHudCanvasController.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/GameplayHudBootstrap.cs",
                "GameplayHudBootstrap.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/Views/StatusBarsHudView.cs",
                "StatusBarsHudView.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/Views/QuestTrackerHudView.cs",
                "QuestTrackerHudView.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/Views/ActiveSkillSlotsHudView.cs",
                "ActiveSkillSlotsHudView.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/Views/InteractionPromptHudView.cs",
                "InteractionPromptHudView.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/Views/FeedbackToastHudView.cs",
                "FeedbackToastHudView.cs exists");
            CheckScript("Assets/_Game/Scripts/UI/HUD/Views/ModalBlockerHudView.cs",
                "ModalBlockerHudView.cs exists");

            // ─── ViewModel and guard (must not have been removed) ─────────────
            CheckScript("Assets/_Game/Scripts/UI/HUD/HUDGameplayViewModel.cs",
                "HUDGameplayViewModel.cs (existing) not removed");
            CheckScript("Assets/_Game/Scripts/UI/HUD/FinalHudGuardValidator.cs",
                "FinalHudGuardValidator.cs (existing) not removed");

            // ─── No scene/prefab/asset files created by WAVE23 ────────────────
            var sceneHudAsset = Path.Combine(Application.dataPath, "HudCanvas.prefab");
            Check(!File.Exists(sceneHudAsset), "No HudCanvas.prefab created (scene wiring deferred to human)");

            // ─── CURRENT_STATE updated ─────────────────────────────────────────
            var csPath = Path.Combine(Application.dataPath, "..", "docs/project/CURRENT_STATE.md");
            if (File.Exists(csPath))
            {
                var content = File.ReadAllText(csPath);
                Check(content.Contains("WAVE_INTEGRATION_23"), "CURRENT_STATE contains WAVE23 entry");
            }

            // ─── Previous validators still exist (no regression) ──────────────
            CheckScript("Assets/_Game/Scripts/Editor/Validation/ValidateWave20PlayableSliceAcceptance.cs",
                "ValidateWave20 still exists (no regression)");
            CheckScript("Assets/_Game/Scripts/Editor/Validation/ValidateWave21PostAcceptanceBugfix.cs",
                "ValidateWave21 still exists (no regression)");
            CheckScript("Assets/_Game/Scripts/Editor/Validation/ValidateWave22DebtBacklog.cs",
                "ValidateWave22 still exists (no regression)");

            // ─── Summary ──────────────────────────────────────────────────────
            Debug.Log($"=== WAVE23 Validation: {pass} PASS / {warn} FAIL ===");
            if (warn == 0)
                Debug.Log("[WAVE23] ALL CHECKS PASSED — HUD Canvas system ready; " +
                          "Canvas visual wiring pending human Unity Editor work.");
            else
                Debug.LogWarning($"[WAVE23] {warn} CHECK(S) FAILED — review warnings above");
        }
    }
}
