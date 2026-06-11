# WAVE_INTEGRATION_24 — Save/Load: Farm + Daily Goal Matrix

**Date:** 2026-06-11

## Farm Plot Save/Load

| Case | Expected | Actual | Works? |
|------|----------|--------|--------|
| Save: plot state | State/seed/days/water serialized | FarmPlot.CaptureSaveData + FarmPlotRegistry.CaptureSaveData | YES |
| Save: only in FarmScene | Farm data captured only when active scene is FarmScene | SaveManager.CaptureFarmSaveData — scene guard | YES |
| Save: from TownScene | Preserves existing farm save section | existingSaveData?.Farm fallback | YES |
| Load: plot state | State/seed/days/water restored | FarmPlot.RestoreFromSaveData | YES |
| Load: invalid state | Reset to Raw, LogWarning | Enum.TryParse guard | YES |
| Load: invalid seed | Reset to TilledDry, LogWarning | TryGetPlantedSeedData guard | YES |
| Load: idempotency | Same state after multiple load | State string comparison | YES |

## Daily Goal Save/Load

| Case | Expected | Actual | Works? |
|------|----------|--------|--------|
| Save: goal state | GoalId/Day/Progress/Completed/Claimed serialized | FarmDailyGoalService.CaptureSaveData | YES (code) |
| Save integration | SaveManager calls CaptureSaveData | NOT YET — SAVE_LOAD_DAILY_GOAL_DEBT | NO (debt) |
| Load: goal state | Progress/completed/day restored | FarmDailyGoalService.RestoreFromSaveData | YES (code) |
| Load integration | SaveManager calls RestoreFromSaveData | NOT YET — SAVE_LOAD_DAILY_GOAL_DEBT | NO (debt) |
| Scene survival | DontDestroyOnLoad preserves state across scenes | FarmDailyGoalRuntimeBootstrap sets DontDestroyOnLoad | YES |
| Restart survival | Game restart loses goal state | SAVE_LOAD_DAILY_GOAL_DEBT — NOT persisted across restarts | DEBT |
| Completed goal idempotency | After load, completed goal not re-completable | Completed=true guard in AddProgress | YES |
| Day mismatch | Load from previous day — goals are for "today" | Day field tracked; reset on DayStartedEvent | YES |

## Known Save/Load Debts

| Debt ID | Description | Priority | Workaround |
|---------|-------------|----------|-----------|
| SAVE_LOAD_DAILY_GOAL_DEBT | SaveManager does not call FarmDailyGoalService save/load methods | P2 | State survives scene transitions (DontDestroyOnLoad) but not game restarts |
| DAILY_GOAL_REWARD_DEFERRED | Goal rewards not persisted (no reward to save) | P3 | N/A — reward is deferred entirely |

## Risk Assessment

| Risk | Severity | Notes |
|------|----------|-------|
| Goal progress lost on game restart | LOW for MVP+ | Daily goals reset each day anyway; losing progress mid-day is acceptable short-term |
| Goal duplicates on load | NONE | Completed=true guard prevents duplicate completion |
| Plot state corruption | LOW | Multiple guards (enum parse, seed resolve, null check) |
| Schema mismatch on load | LOW | GameSaveData.DailyGoals will be null for old saves → graceful null check in RestoreFromSaveData |
