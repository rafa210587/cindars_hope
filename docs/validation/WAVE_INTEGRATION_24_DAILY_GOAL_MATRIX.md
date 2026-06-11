# WAVE_INTEGRATION_24 — Daily Goal Matrix

**Date:** 2026-06-11

## Goals Implemented

| Goal ID | Display Name | Required Progress | Trigger Event | Completion |
|---------|-------------|------------------|---------------|-----------|
| daily_goal_first_harvest | Primeira colheita do dia | 1 | CropHarvestedEvent | DailyGoalCompletedEvent published |
| daily_goal_sell_first_crop | Vender primeiro item colhido | 1 | EconomyTransactionCompletedEvent (WasSuccessful=true, GoldDelta > 0) | DailyGoalCompletedEvent published |

## Goal Lifecycle

| Phase | Behavior |
|-------|----------|
| Day start | Goals reset via DayStartedEvent.OnDayStarted → ResetDailyGoals() |
| Progress | AddProgress(goalId, 1) via relevant event |
| Completion | Completed=true, DailyGoalCompletedEvent published |
| Idempotency | After Completed=true, AddProgress is a no-op |
| After load | RestoreFromSaveData restores exact progress/completed/day |

## Events Produced

| Event | When |
|-------|------|
| DailyGoalProgressedEvent(goalId, current, required) | Each time progress advances (current < required) |
| DailyGoalCompletedEvent(goalId) | When current >= required |

## Feedback Chain

```
CropHarvestedEvent
  → FarmDailyGoalService.OnCropHarvested → AddProgress("daily_goal_first_harvest")
  → DailyGoalProgressedEvent or DailyGoalCompletedEvent
  → FarmLoopFeedbackBridge → PlayerActionFeedbackEvent("Meta diária concluída!")
  → GameplayFeedbackService → HudFeedbackUpdatedEvent → FeedbackToastHudView
```

## Save/Load Goal State

| Case | Expected | Actual |
|------|----------|--------|
| CaptureSaveData | All goal states serialized | FarmDailyGoalService.CaptureSaveData() |
| RestoreFromSaveData | Progress/completed/claimed restored | RestoreFromSaveData() — idempotent |
| New goal (not in save) | Default state (0 progress) | InitializeGoalStates covers all definitions |
| Unknown goalId in save | Skipped (not in _states) | _states.TryGetValue guard |
| Completed goal after load | Stays completed, AddProgress no-op | Completed=true guard |

## Known Debt

| Debt | Priority | Notes |
|------|----------|-------|
| SAVE_LOAD_DAILY_GOAL_DEBT | P2 | SaveManager does not yet call FarmDailyGoalService.CaptureSaveData/RestoreFromSaveData; state survives scene transitions (DontDestroyOnLoad) but NOT game restarts. |
| DAILY_GOAL_REWARD_DEFERRED | P3 | Gold/item reward on completion deferred to economy balance phase. |
| DAILY_GOAL_HUD_DISPLAY_DEFERRED | P3 | No dedicated HUD panel for goal display; feedback toast only. |
