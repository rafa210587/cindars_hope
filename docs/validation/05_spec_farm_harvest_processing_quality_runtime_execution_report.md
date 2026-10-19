# Execution Report — 05_spec_farm_harvest_processing_quality_runtime

## Status
RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES

## Spec
`05_spec_farm_harvest_processing_quality_runtime`  
Wave: WAVE 05 | Priority: P0 | Type: Runtime / Farm / Harvest / Processing / Quality

## Implementation Decision
**CREATE_MINIMAL** — FarmPlot.TryHarvest() handles harvest internally. This spec creates the formal contract layer for external callers.

## Acceptance Criteria

| Criterion | Implementation | Status |
|-----------|---------------|--------|
| Ready crop produces correct item/yield | `HarvestResult.Ok(itemId, qty, quality, regrow)` | OK |
| Crop cannot be harvested twice | `AlreadyHarvested` failure reason + FarmPlot.State guard | OK |
| Inventory full not silent | `HarvestFailureReason.InventoryFull` + `InventoryOverflowed` flag | OK |
| Quality preserved/deferred | `CropQualityTier` on `HarvestedItemEntry` | OK |
| Regrow crop enters regrow state | `RegrowStarted = true` in result | OK |
| Dead crop cannot be harvested | `HarvestFailureReason.CropDead` | OK |
| Processing job has clear states | `ProcessingJobState` enum + `FarmProcessingJob` | OK |
| No infinite multiplier | `AllowsInfiniteReprocessing = false` default | OK |
| Tests | 10 EditMode tests | OK |

## Spec Compliance Matrix

| Spec Requirement | Status | Notes |
|-----------------|--------|-------|
| HarvestCommand | OK | Created |
| HarvestResult | OK | Created with failure reasons |
| YieldResolver | OK | CropYieldResolver with quality bonus |
| QualityTransferPolicy | OK | QualityTransferPolicy enum on ProcessableItem |
| ProcessableItem contract | OK | Created |
| ProcessingJob | OK | FarmProcessingJob with idempotent Collect |
| Inventory overflow contract | OK | InventoryFull reason + InventoryOverflowed flag |
| Infinite multiplier guard | OK | AllowsInfiniteReprocessing defaults false |
| FarmOrder trigger | NOT_APPLICABLE | Out of scope |
| Crafting screen | NOT_APPLICABLE | Out of scope |
| Economy final balance | NOT_APPLICABLE | Out of scope |

## Files Changed

```text
Assets/_Game/Scripts/Farm/Harvest/HarvestCommand.cs       (new)
Assets/_Game/Scripts/Farm/Harvest/HarvestResult.cs        (new)
Assets/_Game/Scripts/Farm/Harvest/CropYieldResolver.cs    (new)
Assets/_Game/Scripts/Farm/Processing/ProcessableItem.cs   (new)
Assets/_Game/Scripts/Farm/Processing/FarmProcessingJob.cs (new)
Assets/_Game/Tests/EditMode/Farm/FarmHarvestTests.cs      (new — 10 tests)
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
Changed runtime code: YES (new contracts)
Changed deterministic logic: YES (yield resolver, processing job idempotency)
Automated tests: YES (10 EditMode tests)
Manual Play Mode scenario: DEFERRED (harvest UI/interaction)
Residual risk: FarmHarvestService not wired to FarmPlot; formal command caller deferred
```

## Honest Status Rationale

All formal contract types implemented and tested. ProcessingJob idempotency enforced. Infinite reprocessing guardrail defaulted to false. FarmPlot.TryHarvest() remains the runtime path; this spec adds the contract layer. Contract-only wiring deferred.

## Known Legacy Gates

- Assembly-CSharp-Editor: pre-existing IProjectValidator mismatch
- Pester: check_spec_quality.ps1 harness issue

---
*Report: 05_spec_farm_harvest_processing_quality_runtime*  
*Date: 2026-06-08*  
*Status: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES*
