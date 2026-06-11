# WAVE_INTEGRATION_23 — Feedback Coverage Matrix

**Date:** 2026-06-10

---

## Feedback Events: DebugHud vs GameplayFeedbackService

This matrix shows which events were previously only in DebugHud and are now also covered in player-facing feedback via GameplayFeedbackService.

| Event | DebugHud (IMGUI) | GameplayFeedbackService | Coverage |
|---|---|---|---|
| `PlayerActionFeedbackEvent` | ✓ `SetFeedback(evt.Message)` | ✓ Enqueue(Normal) | COMPLETE |
| `GameSavedEvent` | ✓ `SetFeedback(...)` | ✓ Enqueue(Normal) | COMPLETE |
| `GameLoadedEvent` | ✓ `SetFeedback(...)` | ✓ Enqueue(Normal) | COMPLETE |
| `QuestAcceptedEvent` | ✓ `SetFeedback("Quest aceita: ...")` | ✓ Enqueue(Important) | COMPLETE |
| `QuestObjectiveProgressedEvent` | ✓ `SetFeedback(...)` | ✗ Not in feedback (only ViewModel updated) | PARTIAL — objective progress shown in QuestTracker, not toast |
| `QuestReadyToCompleteEvent` | — | ✗ Not in feedback | PARTIAL — shown in QuestTracker |
| `QuestCompletedEvent` | ✓ `SetFeedback(...)` | ✓ Enqueue(Important) | COMPLETE |
| `QuestRewardClaimedEvent` | ✓ `SetFeedback(...)` | ✓ Enqueue(Normal) | COMPLETE |
| `EnemyKilledEvent` | ✓ `SetFeedback(...)` | ✓ Enqueue(Low) | COMPLETE |
| `CaveLevelEnteredEvent` | ✓ `SetFeedback(...)` | ✓ Enqueue(Normal) | COMPLETE |
| `CaveExitedEvent` | ✓ `SetFeedback(...)` | ✓ Enqueue(Normal) | COMPLETE |
| `NotificationToastRequestedEvent` | ✗ Not in DebugHud | ✓ Enqueue(Normal) | NEW — not in DebugHud |
| `EconomyTransactionCompletedEvent` | ✓ (economy panel) | ✗ Not in WAVE23 feedback | DEFERRED — economy feedback via shop modal |
| `PlayerXpChangedEvent` / `PlayerLevelChangedEvent` | ✓ (progression panel) | ✗ Not in WAVE23 feedback | DEFERRED — progression shown in DebugHud |

---

## Toast Priority Queue Behavior

| Scenario | Expected Result |
|---|---|
| Two Low-priority toasts queued | First shown for duration, then second |
| Important toast while Normal active | Important interrupts immediately |
| Queue overflows (5+ items) | Queue FIFO; old items may expire before showing |
| Empty queue | No toast shown |

---

## Known Feedback Gaps (Post-WAVE23)

| Gap | ID | Priority |
|---|---|---|
| Quest objective progress not toasted (only tracker updated) | DEBT-UX-023-01 | Low |
| QuestReadyToComplete not toasted | DEBT-UX-023-02 | Low |
| Economy transactions not toasted | DEBT-UX-023-03 | Low |
| XP/Level changes not toasted | DEBT-UX-023-04 | Low |

These gaps are acceptable for MVP+. The DebugHud covers all events during development.

---

*Created: 2026-06-10 (WAVE23)*
