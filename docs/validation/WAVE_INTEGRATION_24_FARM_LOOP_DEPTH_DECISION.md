# WAVE_INTEGRATION_24 — Farm Loop Depth + Daily Goals — Decision Document

**Date:** 2026-06-11
**Status:** BUILD_VALIDATED_FARM_LOOP_DEPTH_READY_PENDING_HUMAN_PLAYMODE

---

## Sources Read

| Source | Status | Notes |
|--------|--------|-------|
| CLAUDE.md | READ | Project router |
| docs/project/CURRENT_STATE.md | READ | Active queue, gates |
| WAVE_INTEGRATION_24 spec (prompt) | READ | Full spec text |
| Assets/_Game/Scripts/Farm/*.cs (all) | READ | Full farm audit |
| Assets/_Game/Scripts/Core/Events/*.cs | READ | All events audited |
| Assets/_Game/Scripts/Save/SaveManager.cs | READ | Save flow audited |
| Assets/_Game/Scripts/Save/SaveData.cs | READ | Save DTOs audited |
| Assets/_Game/Scripts/Farm/Shipping/FarmShippingService.cs | READ | Shipping audited |
| Assets/_Game/Scripts/UI/HUD/GameplayFeedbackService.cs | READ | WAVE23 feedback service |
| Assets/_Game/Scripts/Core/GameEventBus.cs | READ | Namespace confirmed |
| docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md | EXISTS | Not read (cited in spec only) |
| docs/design/gameplay/farm/FARM_CROP_GROWTH_WATERING_DIRECTION.md | NOT FOUND | N/A |
| docs/design/gameplay/farm/FARM_DAILY_LOOP_DIRECTION.md | NOT FOUND | N/A |

## Sources NOT Read (per context-reading-policy)

- PROJECT_LOG.md
- ROADMAP.md
- Full GDD
- All refinements
- Archived specs

---

## Gates Validation

| Gate | Status | Evidence |
|------|--------|---------|
| WAVE20 gate | SATISFIED | docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md exists |
| WAVE21 gate | SATISFIED | NO_OP_NO_P0_P1_FOUND confirmed in CURRENT_STATE |
| WAVE22 gate | SATISFIED | DEBT_BACKLOG_CONSOLIDATED_NEXT_ROADMAP_READY confirmed |
| WAVE23 dependency | SATISFIED | GameplayFeedbackService.cs exists and BUILD_VALIDATED |
| P0/P1 open | 0 | Confirmed CURRENT_STATE |

---

## Farm Runtime Audit Summary

### Exists (REUSE — DO NOT RECREATE):

| System | File | Status |
|--------|------|--------|
| FarmPlot (plot management, plant/water/harvest) | Assets/_Game/Scripts/Farm/FarmPlot.cs | FULLY IMPLEMENTED |
| FarmPlotRegistry (save/restore) | Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs | FULLY IMPLEMENTED |
| FarmPlotState (state machine: Raw/TilledDry/TilledWet/PlantedDry/PlantedWet/ReadyToHarvest/Dead) | Assets/_Game/Scripts/Farm/FarmPlotState.cs | FULLY IMPLEMENTED |
| FarmPlotSaveData (DTO) | Assets/_Game/Scripts/Farm/FarmPlotSaveData.cs | FULLY IMPLEMENTED |
| CropGrowthProcessor (deterministic growth) | Assets/_Game/Scripts/Farm/Crops/CropGrowthProcessor.cs | FULLY IMPLEMENTED |
| CropGrowthState | Assets/_Game/Scripts/Farm/Crops/CropGrowthState.cs | EXISTS |
| SeedDataSO (seed definition) | Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs | EXISTS |
| FarmShippingService (deposit/process batch) | Assets/_Game/Scripts/Farm/Shipping/FarmShippingService.cs | FULLY IMPLEMENTED |
| FarmWateringService | Assets/_Game/Scripts/Farm/Watering/FarmWateringService.cs | EXISTS |
| FarmResourceNodeService (resource nodes) | Assets/_Game/Scripts/Farm/Resources/FarmResourceNodeService.cs | EXISTS |
| SaveManager (complete save flow) | Assets/_Game/Scripts/Save/SaveManager.cs | FULLY IMPLEMENTED |
| InventoryManager | Assets/_Game/Scripts/Inventory/InventoryManager.cs | FULLY IMPLEMENTED |
| EconomyManager | Assets/_Game/Scripts/Economy/EconomyManager.cs | EXISTS |
| GameplayFeedbackService (WAVE23) | Assets/_Game/Scripts/UI/HUD/GameplayFeedbackService.cs | FULLY IMPLEMENTED |

### Key Events Already Existing:

| Event | File | Notes |
|-------|------|-------|
| SeedPlantedEvent | Core/Events/SeedPlantedEvent.cs | Published by FarmPlot.TryPlantSeed |
| CropHarvestedEvent | Core/Events/CropHarvestedEvent.cs | Published by FarmPlot.TryHarvest |
| CropReadyEvent | Core/Events/CropReadyEvent.cs | Published when crop ready |
| CropDiedEvent | Core/Events/CropDiedEvent.cs | Published when crop dies |
| DayStartedEvent | Core/Events/DayStartedEvent.cs | Published by day system |
| EconomyTransactionCompletedEvent | Core/Events/EconomyTransactionCompletedEvent.cs | Published by SellPoint |
| GoldChangedEvent | Core/Events/GoldChangedEvent.cs | Published by economy |
| PlayerActionFeedbackEvent | Core/Events/PlayerActionFeedbackEvent.cs | Consumed by GameplayFeedbackService |
| GameSavedEvent | Core/Events/GameSavedEvent.cs | Published by SaveManager |
| GameLoadedEvent | Core/Events/GameLoadedEvent.cs | Published by SaveManager |
| ResourceNodeDepletedEvent | Core/Events/ResourceNodeDepletedEvent.cs | Published by resource nodes |

### Missing Events (CREATED):

| Event | File | Notes |
|-------|------|-------|
| DailyGoalProgressedEvent | Core/Events/DailyGoalProgressedEvent.cs | NEW — daily goal progress |
| DailyGoalCompletedEvent | Core/Events/DailyGoalCompletedEvent.cs | NEW — daily goal completed |
| CropWateredEvent | Core/Events/CropWateredEvent.cs | NEW — future use for watering tracking |

---

## Decisions Made

### Daily Goals Strategy

**Decision:** Create minimal FarmDailyGoalService with 2 goals:
- `daily_goal_first_harvest` — triggered by CropHarvestedEvent (1 harvest/day)
- `daily_goal_sell_first_crop` — triggered by EconomyTransactionCompletedEvent (1 sale/day with GoldDelta > 0)

**Rationale:** Both trigger events already exist. No new coupling needed. Goals reset on DayStartedEvent. Goals save/load via new GameSaveData.DailyGoals field (FarmDailyGoalsSaveData).

### Save/Load Strategy

**Decision:** Add `FarmDailyGoalsSaveData DailyGoals` field to GameSaveData. FarmDailyGoalService.CaptureSaveData() and RestoreFromSaveData() are static-accessible via FarmDailyGoalService.Instance.

**Rationale:** SaveManager already has full save/load flow. Adding a field to GameSaveData is the minimal delta. No SaveManager method change needed for now — FarmDailyGoalService auto-wired via DontDestroyOnLoad and bootstrapped.

**Note:** SaveManager does not yet call FarmDailyGoalService.CaptureSaveData/RestoreFromSaveData automatically — this is SAVE_LOAD_DAILY_GOAL_DEBT to be closed in a future patch. State will survive between scenes via DontDestroyOnLoad but NOT survive game restarts without SaveManager integration.

### Scene Edit Strategy

**Decision:** DO NOT edit FarmScene.unity. FarmDailyGoalRuntimeBootstrap uses RuntimeInitializeOnLoadMethod(AfterSceneLoad) to auto-spawn FarmDailyGoalService on any scene load.

**Rationale:** Spec explicitly forbids mass FarmScene edits. RuntimeInitializeOnLoad is the canonical pattern.

### HUD/Feedback Strategy

**Decision:** FarmLoopFeedbackBridge publishes PlayerActionFeedbackEvent for all farm-loop events. GameplayFeedbackService (WAVE23) already consumes PlayerActionFeedbackEvent.

**Rationale:** No change to GameplayFeedbackService needed. FarmLoopFeedbackBridge is thin bridge.

### Design/Direction Compliance

| Rule | Compliant? | Notes |
|------|-----------|-------|
| No FarmManager recreation | YES | FarmPlot/FarmPlotRegistry reused |
| No InventoryManager recreation | YES | Existing InventoryManager reused |
| No SaveManager recreation | YES | Existing SaveManager reused |
| No EconomyManager recreation | YES | Existing EconomyManager/SellPoint reused |
| No HUD recreation | YES | GameplayFeedbackService reused |
| No FarmScene mass edit | YES | RuntimeInitializeOnLoad used |
| GameEventBus for all communication | YES | No direct calls |
| using CindarsHope.Core for GameEventBus | YES | All new files verified |
| Save DTOs simple types only | YES | FarmDailyGoalsSaveData has only string/int/bool |
| No GameObject.Find in gameplay | YES | FindAnyObjectByType only in bootstrap |
