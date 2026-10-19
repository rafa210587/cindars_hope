# WAVE_INTEGRATION_24 — Existing Farm Functionality Matrix

**Date:** 2026-06-11

| Feature | Exists? | Path/Asset | Works? | Reuse? | Action |
|---------|---------|-----------|--------|--------|--------|
| FarmScene | YES | Assets/_Game/Scenes/FarmScene.unity | YES (scene exists) | YES | DO NOT edit |
| FarmPlot (till/plant/water/harvest) | YES | Farm/FarmPlot.cs | YES | YES | Reuse as-is |
| FarmPlotState (state machine) | YES | Farm/FarmPlotState.cs | YES | YES | Reuse as-is |
| FarmPlotRegistry (save/load plots) | YES | Farm/FarmPlotRegistry.cs | YES | YES | Reuse as-is |
| FarmPlotSaveData (DTO) | YES | Farm/FarmPlotSaveData.cs | YES | YES | Reuse as-is |
| Planting (seed consume + state) | YES | FarmPlot.TryPlantSeed | YES | YES | Reuse |
| Watering (state TilledDry→TilledWet, PlantedDry→PlantedWet) | YES | FarmPlot.TryWater | YES | YES | Reuse |
| Growth (DayStartedEvent + AdvanceGrowth) | YES | FarmPlot.OnDayStarted | YES | YES | Reuse |
| Harvest (AddItem to inventory) | YES | FarmPlot.TryHarvest | YES | YES | Reuse |
| Seeds (SeedDataSO with HarvestItems) | YES | Farm/Data/SeedDataSO.cs | YES | YES | Reuse |
| Crop items (in InventoryManager) | YES | InventoryManager.AddItem | YES | YES | Reuse |
| Shipping/sell | YES | Economy/SellPoint.cs + FarmShippingService.cs | YES | YES | Reuse |
| Resource nodes (trees, rocks, forage) | YES | Farm/Resources/, Farm/Mining/, Farm/Trees/, Farm/Forage/ | YES | YES | Reuse |
| Save/load (plots) | YES | FarmPlotRegistry.CaptureSaveData/RestoreFromSaveData | YES | YES | Reuse |
| Save/load (full game) | YES | Save/SaveManager.cs | YES | YES | Reuse |
| HUD/feedback | YES | UI/HUD/GameplayFeedbackService.cs (WAVE23) | YES | YES | Reuse |
| Daily goal system | NO | — | N/A | CREATE | FarmDailyGoalService (new) |
| Daily goal save/load | NO | — | N/A | CREATE | FarmDailyGoalsSaveData (new) |
| Daily goal feedback | NO | — | N/A | CREATE | FarmLoopFeedbackBridge (new) |
| Shipping summary feedback | PARTIAL | SellPoint publishes EconomyTransactionCompletedEvent | YES | EXTEND | ShippingSummaryService (new) |
| Day reset for goals | EXISTING | DayStartedEvent | YES | YES | FarmDailyGoalService subscribes |
| CropWateredEvent | NO | — | N/A | CREATE | New event struct (thin; future use) |
| DailyGoalProgressedEvent | NO | — | N/A | CREATE | New event struct |
| DailyGoalCompletedEvent | NO | — | N/A | CREATE | New event struct |
