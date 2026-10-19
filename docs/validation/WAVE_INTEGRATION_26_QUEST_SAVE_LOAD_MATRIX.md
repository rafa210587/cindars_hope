# WAVE_INTEGRATION_26 — Quest Save/Load Matrix

Date: 2026-06-11

---

## Save/Load Infrastructure (WAVE18)

| Component | File | Status |
|-----------|------|--------|
| QuestStateSectionSaveData | Save/QuestStateSection.cs | EXISTS — [Serializable] simple types |
| QuestStateSaveData | Save/QuestStateSection.cs | EXISTS |
| QuestObjectiveStateSaveData | Save/QuestStateSection.cs | EXISTS |
| SaveManager.CaptureQuestSaveData | SaveManager.cs | EXISTS (WAVE18) |
| SaveManager.RestoreQuestSaveData | SaveManager.cs | EXISTS (WAVE18) |
| QuestRuntimeBootstrap.CaptureSaveData | QuestRuntimeBootstrap.cs | EXISTS (WAVE18) |
| QuestRuntimeBootstrap.RestoreFromSaveData | QuestRuntimeBootstrap.cs | EXISTS (WAVE18) |
| QuestService.RestoreFromSaveData | QuestService.cs | EXISTS (WAVE18) |

---

## Save/Load Scenarios

| Scenario | Saved | Loaded | Result |
|----------|-------|--------|--------|
| Quest 1 accepted (Active) | State = Active; ObjectiveStates = [] or partial | QuestStateRecord restored | Active state continues |
| Q1 obj_collect_wood_x2 progress = 1/2 | ObjectiveStates[0].CurrentProgress = 1 | Progress restored | 1/2 shown |
| Q1 ReadyToComplete | State = ReadyToComplete | State restored | TurnIn available |
| Q1 Completed | State = Completed; GrantedRewardIds populated | State + rewards restored | Can't TurnIn again |
| Q2 accepted (Active) | State = Active | QuestStateRecord restored | Q2 continues |
| Q2 SellItem progress = 0/1 | ObjectiveStates[0].CurrentProgress = 0 | Progress restored | Next sell completes |
| Q3 accepted (Active) | State = Active | QuestStateRecord restored | Q3 continues |
| Q3 cave entry pending | State = Active | Restored | Next cave entry completes |
| All 3 quests completed | 3 QuestStateRecords with Completed | All 3 restored | No chain unlock needed |
| Reward granted then load | GrantedRewardIds = [reward_...] | GrantedRewardIds restored | No duplicate reward |

---

## DTO Fields Persisted

```
QuestStateSaveData fields:
  QuestId (string)
  State (int — QuestStateStatus enum value)
  ObjectiveStates: List<QuestObjectiveStateSaveData>
    ObjectiveId (string)
    CurrentProgress (int)
    RequiredProgress (int)
    IsCompleted (bool)
    IsKnown (bool)
  GrantedRewardIds: List<string>
  GrantedFlagIds: List<string>
  Tracked (bool)
  Discovered (bool)
  StartedAtDay (int)
  CompletedAtDay (int)
```

No Unity refs in DTO — compliant with save-dto-simple-types-only rule.

---

## Known Debts

| Debt | Description | Impact |
|------|-------------|--------|
| PREREQUISITE_UI_DEBT | QuestGiverInteractable does not enforce PrerequisiteQuestIds at offer time | Player could accept Q2 before completing Q1 (no gameplay crash, just sequence issue) |
| SCENE_WIRING_DEBT | npc_pip and npc_maelor need QuestGiverInteractable in TownScene | Q2 and Q3 not offerable until human wires scene |
| DAILY_GOAL_QUEST_INTEGRATION_DEBT | FarmDailyGoalService (WAVE24) not connected to quest progress | No cross-system objective tracking yet |
