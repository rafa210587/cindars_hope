# WAVE_INTEGRATION_23 — Human Play Mode Checklist

**Date:** 2026-06-10
**Purpose:** Validate GameplayHudCanvas runtime behavior in Unity Editor Play Mode

---

## Pre-conditions

- [ ] Unity Editor open, project compiles (0 errors)
- [ ] TownScene loaded and Play Mode started

---

## Part 1: Bootstrap Verification

- [ ] 1.1 Enter Play Mode — expected: `GameplayHudCanvas` GameObject appears in DontDestroyOnLoad
- [ ] 1.2 Components present: GameplayHudCanvasController, GameplayHudRuntimeBinder, GameplayFeedbackService, HudVisibilityController, StatusBarsHudView, QuestTrackerHudView, ActiveSkillSlotsHudView, InteractionPromptHudView, FeedbackToastHudView, ModalBlockerHudView
- [ ] 1.3 Console: `[GameplayHudBootstrap] GameplayHudCanvas created via RuntimeInitializeOnLoadMethod.`

---

## Part 2: Status Bar Binding

- [ ] 2.1 Take damage — expected: GameplayHudViewModel.Hp changes in Inspector
- [ ] 2.2 Use Dodge — expected: GameplayHudViewModel.Stamina changes
- [ ] 2.3 Walk near interactable — expected: ContextPrompt.IsVisible = true, DescriptionKey populated
- [ ] 2.4 Walk away — expected: ContextPrompt.IsVisible = false

---

## Part 3: Quest Tracker Binding

- [ ] 3.1 Accept quest — expected: QuestPrompt = "Quest ativa: {questId}"
- [ ] 3.2 Progress objective — expected: QuestPrompt = "{questId}: {curr}/{req}"
- [ ] 3.3 Complete quest — expected: QuestPrompt = ""

---

## Part 4: Feedback Toast

- [ ] 4.1 Save (F5) — Console: `[FeedbackToastHudView] Toast: 'Jogo salvo.' (3s)`
- [ ] 4.2 Kill enemy — Console: `[FeedbackToastHudView] Toast: 'Inimigo derrotado.'` or Loot toast
- [ ] 4.3 Accept quest — Console: Important toast (4s)

---

## Part 5: Modal Visibility Guard

- [ ] 5.1 Open Inventory (I) — expected: StatusBarsHudView, QuestTrackerHudView, ActiveSkillSlotsHudView, InteractionPromptHudView all activeSelf=false; ModalBlockerHudView.IsBlocking=true
- [ ] 5.2 Close Inventory (Esc) — expected: all views activeSelf=true; ModalBlockerHudView.IsBlocking=false
- [ ] 5.3 Repeat with Pause, Dialogue, Shop

---

## Part 6: Active Skill Slots

- [ ] 6.1 Assign skills to R/T/Y/G — expected: ActiveSkillSlots has 4 entries
- [ ] 6.2 FinalHudGuardValidator: no guard violations in Console

---

## Part 7: Scene Transition

- [ ] 7.1 Transition to cave — expected: GameplayHudCanvas persists in DontDestroyOnLoad
- [ ] 7.2 Return to TownScene — expected: single GameplayHudCanvas instance

---

## Acceptance Summary

| Check | Result |
|---|---|
| GameplayHudCanvas created in DontDestroyOnLoad | [ ] PASS / [ ] FAIL |
| Status bars bind to events | [ ] PASS / [ ] FAIL |
| Interaction prompt binds | [ ] PASS / [ ] FAIL |
| Quest tracker binds | [ ] PASS / [ ] FAIL |
| Feedback toasts logged | [ ] PASS / [ ] FAIL |
| HUD hides when modal open | [ ] PASS / [ ] FAIL |
| HUD shows when modal closed | [ ] PASS / [ ] FAIL |
| Skill slots bind | [ ] PASS / [ ] FAIL |
| Persists across scene transitions | [ ] PASS / [ ] FAIL |

---

*Created: 2026-06-10 (WAVE23 — MVP+ 01)*
