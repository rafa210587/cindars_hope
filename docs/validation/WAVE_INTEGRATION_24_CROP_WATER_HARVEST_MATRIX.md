# WAVE_INTEGRATION_24 — Crop / Water / Harvest Matrix

**Date:** 2026-06-11

## Behavior: Plant

| Case | Expected | Actual (Code) | Works? |
|------|----------|---------------|--------|
| Plant seed from inventory | Seed consumed, plot → PlantedDry or PlantedWet | FarmPlot.TryPlantSeed removes from inventory, sets state | YES |
| Plot not tillable | Feedback "Plot is not plantable." | State check in TryPlantSeed | YES |
| Seed not in inventory | Feedback "Seed not in inventory." | inventoryManager.HasItem check | YES |
| Stamina insufficient | Feedback "Not enough stamina to plant." | ValidateStamina(4) | YES |
| SeedDatabaseSO missing | Feedback "Seed data unavailable." | null guard | YES |
| Publish SeedPlantedEvent | YES | GameEventBus.Publish(new SeedPlantedEvent) | YES |

## Behavior: Water

| Case | Expected | Actual (Code) | Works? |
|------|----------|---------------|--------|
| Water TilledDry | → TilledWet, feedback "Soil watered." | FarmPlot.TryWater | YES |
| Water PlantedDry | → PlantedWet, feedback "Crop watered." | FarmPlot.TryWater | YES |
| Water already wet | Feedback "Cannot water this plot." | State guard | YES |
| WateringCan missing (no temp mode) | Feedback "Watering Can required." | HasRequiredTool check | YES |
| Stamina insufficient | Feedback "Not enough stamina to water." | ValidateStamina(8) | YES |
| Skill-triggered water | TryWaterViaSkill() — bypasses tool/stamina | WAVE_INTEGRATION_11 | YES |

## Behavior: Grow

| Case | Expected | Actual (Code) | Works? |
|------|----------|---------------|--------|
| DayStarted: PlantedWet | DaysGrown++; if >= GrowthDays → ReadyToHarvest | FarmPlot.OnDayStarted + AdvanceGrowth | YES |
| DayStarted: PlantedDry | DaysWithoutWater++; if >= 3 → Dead | FarmPlot.OnDayStarted | YES |
| DayStarted: TilledWet | → TilledDry | FarmPlot.OnDayStarted | YES |
| Idempotency (same day) | Skip if LastProcessedDay == CurrentDay | Guard in OnDayStarted | YES |
| CropReadyEvent published | YES (when DaysGrown >= GrowthDays) | GameEventBus.Publish(CropReadyEvent) | YES |
| CropDiedEvent published | YES (when DaysWithoutWater >= 3) | GameEventBus.Publish(CropDiedEvent) | YES |

## Behavior: Harvest

| Case | Expected | Actual (Code) | Works? |
|------|----------|---------------|--------|
| Harvest ReadyToHarvest | Items added to inventory, CropHarvestedEvent | FarmPlot.TryHarvest | YES |
| Harvest non-ready | Feedback "Crop is not ready." | State guard | YES |
| InventoryManager missing | LogWarning, return false | null guard | YES |
| Stamina insufficient | Feedback "Not enough stamina to harvest." | ValidateStamina(4) | YES |
| HarvestItems empty | LogWarning, return false | pairCount == 0 guard | YES |
| Regrow crops | DaysGrown reduced, state → PlantedDry | RegrowDays > 0 branch | YES |
| Non-regrow crops | Plot reset to TilledDry | ResetPlot() | YES |
| CropHarvestedEvent published | YES — per harvest item | GameEventBus.Publish(CropHarvestedEvent) | YES |
| DailyGoalProgressedEvent | YES — FarmDailyGoalService.OnCropHarvested | NEW WAVE24 | YES |

## Save/Load: Crop State

| Case | Expected | Actual | Works? |
|------|----------|--------|--------|
| CaptureSaveData | All plot fields serialized | FarmPlot.CaptureSaveData() | YES |
| RestoreFromSaveData | State, seed, days, water restored | FarmPlot.RestoreFromSaveData() | YES |
| Invalid state in save | Reset to Raw, LogWarning | Enum.TryParse guard | YES |
| Invalid seed in save | Reset to TilledDry, LogWarning | TryGetPlantedSeedData guard | YES |
| Idempotency | Load restores exact state | State string comparison | YES |
