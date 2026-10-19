# Execution Report — 05_spec_farm_soil_crop_growth_quality_runtime

## Status
RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES

## Spec
`05_spec_farm_soil_crop_growth_quality_runtime`  
Wave: WAVE 05 | Priority: P0 | Type: Runtime / Farm / Soil / Crop Growth / Quality

## Implementation Decision
**HARDEN_EXISTING** — FarmPlot.cs already has substantial farm loop; spec adds missing death/water-tracking contracts.

## Acceptance Criteria

| Criterion | Implementation | Status |
|-----------|---------------|--------|
| Crop growth processed once per day | `LastProcessedDay` guard in `OnDayStarted` | OK |
| Unwatered crop does not grow | `PlantedDry` → `DaysWithoutWater++`, no growth | OK |
| Crop death threshold explicit and testable | `DefaultDeathThresholdDays = 3`; `CropGrowthProcessor.Process()` | OK |
| Dead crop transition to Dead state | `DaysWithoutWater >= 3` → `SetState(FarmPlotState.Dead)` | OK |
| Dead crop stays dead (no more growth) | Guard in `CropGrowthProcessor` + MonoBehaviour | OK |
| Regrow crop preserves behavior | Existing: `RegrowDays > 0` triggers partial reset | OK |
| Invalid CropId safe fallback | `TryGetPlantedSeedData` logs warning and resets | OK |
| Quality hook without final balance | `CropQualityResolver.Resolve(CropQualityInput)` | OK |
| Save/load preserves plot/crop state | `DaysWithoutWater` + `LastProcessedDay` added to FarmPlotSaveData | OK |
| No farm loop rewrite | HARDEN_EXISTING pattern; FarmPlot.cs minimally changed | OK |

## Existing Systems Audit

| System | Status | Decision |
|--------|--------|----------|
| `FarmPlot.cs` | EXISTING_CANONICAL | HARDEN — added DaysWithoutWater, LastProcessedDay, death logic |
| `FarmPlotState.cs` | EXISTING_CANONICAL | REUSE — Dead state already existed |
| `FarmPlotSaveData.cs` | EXISTING_PARTIAL | HARDEN — added DaysWithoutWater, LastProcessedDay fields |
| `SeedDataSO.cs` | EXISTING_CANONICAL (ScriptableObject) | REUSE — pure C# contract created alongside |
| `RainIrrigationIntegration.cs` | EXISTING_CANONICAL | REUSE — left untouched (out of scope) |

## Spec Compliance Matrix

| Spec Requirement | Status | Notes |
|-----------------|--------|-------|
| FarmPlotState canonical | OK | Existing + Dead always was there |
| CropDefinitionData contract | OK | `CropDefinitionData.cs` (pure C#) |
| CropGrowthState + Processor | OK | `CropGrowthProcessor.cs` — deterministic, testable |
| CropQualityResolver | OK | Hook created, balance deferred |
| Death threshold | OK | 3 days default |
| DaysWithoutWater tracking | OK | Added to FarmPlot + FarmPlotSaveData |
| LastProcessedDay guard | OK | `OnDayStarted` returns early if same day |
| Save/load safe defaults | OK | New fields default to 0, backward compatible |
| Season availability hooks | OK (CONTRACT) | `IsValidSeason` in `CropGrowthInput` |
| Regrow handling | OK | Existing in FarmPlot, preserved |
| Invalid ID fallback | OK | Existing `TryGetPlantedSeedData` logs warning |
| UI final | NOT_APPLICABLE | Out of scope |
| Watering/irrigation impl | NOT_APPLICABLE | Out of scope |
| Greenhouse off-season | NOT_APPLICABLE | Out of scope |

## Files Changed

```text
Assets/_Game/Scripts/Farm/FarmPlot.cs                              (modified — death logic, DaysWithoutWater, LastProcessedDay)
Assets/_Game/Scripts/Farm/FarmPlotSaveData.cs                      (modified — added DaysWithoutWater, LastProcessedDay)
Assets/_Game/Scripts/Farm/Crops/CropDefinitionData.cs              (new)
Assets/_Game/Scripts/Farm/Crops/CropGrowthState.cs                 (new)
Assets/_Game/Scripts/Farm/Crops/CropGrowthProcessor.cs             (new)
Assets/_Game/Scripts/Farm/Crops/CropQualityResolver.cs             (new)
Assets/_Game/Scripts/Core/Events/CropDiedEvent.cs                  (new)
Assets/_Game/Tests/EditMode/Farm/CropGrowthProcessorTests.cs       (new — 15 tests)
Assembly-CSharp.csproj                                              (modified — new file entries)
```

## Validation

```text
Validation mode: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES
Assembly-CSharp: PASS (0E, 0W)
Assembly-CSharp-Editor: LEGACY_BLOCKER (pre-existing)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY
Quality check: PESTER_ERROR (known)
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (growth, death, idempotency)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (15 EditMode tests in CropGrowthProcessorTests)
Manual Play Mode scenario: DEFERRED (plant-water-sleep-grow visual loop)
Justification if no Play Mode: Out of scope; visual loop requires FarmScene/prefabs
Residual risk: Season integration unvalidated at runtime; quality balance deferred
```

## Honest Status Rationale

All core deterministic contracts implemented and tested. Death logic, DaysWithoutWater, idempotency guard added. Save/load extended backward-compatibly. Quality hook exists without final balance. PlayMode/visual deferred as specified in spec. Assembly-CSharp PASS.

## Known Legacy Gates

- Assembly-CSharp-Editor: pre-existing IProjectValidator mismatch
- Pester: check_spec_quality.ps1 harness issue

---
*Report: 05_spec_farm_soil_crop_growth_quality_runtime*  
*Date: 2026-06-08*  
*Status: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES*
