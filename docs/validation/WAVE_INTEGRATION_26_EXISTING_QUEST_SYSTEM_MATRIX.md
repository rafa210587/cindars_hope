# WAVE_INTEGRATION_26 — Existing Quest System Matrix

Date: 2026-06-11

---

## Core Runtime Components

| Component | File | Namespace | Status | WAVE Introduced |
|-----------|------|-----------|--------|-----------------|
| QuestService | Runtime/QuestService.cs | CindarsHope.Quests.Runtime | EXISTS | WAVE15 |
| QuestRegistry | Runtime/QuestRegistry.cs | CindarsHope.Quests.Runtime | EXISTS | WAVE15 |
| QuestRuntimeBootstrap | Runtime/QuestRuntimeBootstrap.cs | CindarsHope.Quests.Runtime | EXISTS | WAVE15 |
| QuestProgressEventBridge | Runtime/QuestProgressEventBridge.cs | CindarsHope.Quests.Runtime | EXISTS | WAVE15 |
| QuestGiverInteractable | Runtime/QuestGiverInteractable.cs | CindarsHope.Quests.Runtime | EXISTS | WAVE15 |
| QuestBoardInteractable | Runtime/QuestBoardInteractable.cs | CindarsHope.Quests.Runtime | EXISTS | WAVE15 |

## Data Models

| Component | File | Key Fields | Status |
|-----------|------|------------|--------|
| QuestDefinition | QuestDefinition.cs | QuestId, Category, Trackable, PrerequisiteQuestIds | EXISTS |
| QuestObjective | QuestStepDefinition.cs | ObjectiveId, ObjectiveType, TargetId, RequiredAmount | EXISTS |
| QuestObjectiveType | QuestCategoryType.cs | CollectItem(3), SellItem(12), HarvestCrop(8), TalkToNpc(0), ReachCaveDepth(30), DefeatEnemy(20) | EXISTS |
| QuestRewardDefinition | Rewards/QuestRewardDefinition.cs | RewardId, RewardType, Quantity, IdempotencyPolicy | EXISTS |
| QuestStateRecord | Save/QuestStateRecord.cs | QuestId, State, ObjectiveStates, GrantedRewardIds, GrantedFlagIds | EXISTS |
| QuestStateSection | Save/QuestStateSection.cs | QuestStates list | EXISTS |

## UI Components

| Component | File | Status | Key Behavior |
|-----------|------|--------|--------------|
| QuestOfferPanelController | UI/Quests/Runtime/QuestOfferPanelController.cs | EXISTS | IMGUI headless; listens QuestGiverInteractedEvent; Accept button |
| QuestLogPanelController | UI/Quests/Runtime/QuestLogPanelController.cs | EXISTS | IMGUI; J key; shows active/completed |
| QuestLogRuntimeBinder | UI/Quests/Runtime/QuestLogRuntimeBinder.cs | EXISTS | ModalType.QuestLog integration |
| QuestTrackerHudView | UI/HUD/Views/QuestTrackerHudView.cs | EXISTS | HUD tracker (headless, WAVE23) |

## Events (all in CindarsHope.Core.Events)

| Event | File | Fields | Producer | Consumer |
|-------|------|--------|----------|----------|
| QuestAcceptedEvent | QuestRuntimeEvents.cs | QuestId | QuestService | QuestLogPanelController |
| QuestObjectiveProgressedEvent | QuestRuntimeEvents.cs | QuestId, ObjectiveId, Current, Required | QuestService | QuestLogPanelController |
| QuestReadyToCompleteEvent | QuestRuntimeEvents.cs | QuestId | QuestService | QuestOfferPanelController |
| QuestCompletedEvent | QuestRuntimeEvents.cs | QuestId | QuestService | QuestLogPanelController |
| QuestRewardClaimedEvent | QuestRuntimeEvents.cs | QuestId, GoldGiven, ItemsGiven | QuestService | DebugHud |
| QuestGiverInteractedEvent | QuestRuntimeEvents.cs | NpcId, QuestId, Mode | QuestGiverInteractable | QuestOfferPanelController |

## Save/Load Integration

| Feature | Status | File | Notes |
|---------|--------|------|-------|
| QuestStateSectionSaveData DTO | EXISTS | Save/QuestStateSection.cs | [Serializable] simple types only |
| SaveManager.CaptureQuestSaveData | EXISTS | SaveManager.cs | WAVE18 |
| SaveManager.RestoreQuestSaveData | EXISTS | SaveManager.cs | WAVE18 |
| QuestRuntimeBootstrap.CaptureSaveData | EXISTS | QuestRuntimeBootstrap.cs | WAVE18 |
| QuestRuntimeBootstrap.RestoreFromSaveData | EXISTS | QuestRuntimeBootstrap.cs | WAVE18 |
| QuestService.RestoreFromSaveData | EXISTS | QuestService.cs | WAVE18 — preserves GrantedRewardIds |

## Reward Idempotency

| Mechanism | Status | Notes |
|-----------|--------|-------|
| GrantedRewardIds list in QuestStateRecord | EXISTS | Tracks which reward IDs were applied |
| GrantedFlagIds list in QuestStateRecord | EXISTS | Tracks which flag rewards were applied |
| QuestRewardApplicator.Apply idempotency check | EXISTS | SkippedAlreadyGranted result when duplicate |
| RewardIdempotencyPolicy.TrackByRewardId | EXISTS | Default policy for gold/item rewards |
| RewardIdempotencyPolicy.TrackByFlagId | EXISTS | Default policy for flag rewards |

## WAVE26 Delta Summary

| What | Action | Why |
|------|--------|-----|
| QuestRegistry | EXTENDED — +2 quests | Add quest chain Q2 + Q3 |
| QuestProgressEventBridge | EXTENDED — +5 event subscriptions | Enable HarvestCrop/SellItem/TalkToNpc/CaveDepth/DefeatEnemy objective types |
| QuestService | EXTENDED — +6 new handler methods | Route new event types to objective completion |
| ValidateWave26 | NEW — editor validator | Validate WAVE26 docs + code presence |
| All else | UNCHANGED | No recreation of existing systems |
