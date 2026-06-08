# Execution Report - 05_spec_farm_resource_node_refresh_runtime

**Status:** BUILD_VALIDATED_EXPANDED  
**Date expanded:** 2026-06-08  
**Phase:** Phase 1 (Code complete, automated tests present)

---

## Summary

Spec defines resource node refresh/respawn policy (rocks, forage, fishing spots, etc.) without replacing cave loot. Implementation verified:
- ResourceNodeType, ResourceNodeDefinition, ResourceNodeInstanceState contracts
- FarmResourceNodeService with harvest, idempotency, and depletion logic
- FarmResourceRefreshProcessor with zone/cooldown/level gates
- 11 EditMode tests covering refresh, zone guards, and idempotency

---

## Acceptance Criteria

| # | Criterion | Implementation | Test Evidence | Status |
|---|-----------|-----------------|---|---|
| 1 | Resource node can be harvested with valid tool | FarmResourceNodeService.HarvestNode() validates tool tier | FarmResourceNodeRefreshTests.cs | PASS |
| 2 | Drop/harvest generated once | Atomic harvest + drop logic; idempotency enforced | FarmResourceNodeRefreshTests.cs (idempotency test) | PASS |
| 3 | Harvest state survives reload | ResourceNodeInstanceState persisted | RefreshTests.cs (state persistence) | PASS |
| 4 | Refresh respects zones and cooldown | FarmResourceRefreshProcessor validates zone, cooldown, farm level | RefreshTests.cs (zone guard, cooldown tests) | PASS |
| 5 | No refresh under buildings/crops | Zone-aware processor blocks refresh in occupied zones | RefreshTests.cs (placement validation) | PASS |
| 6 | Resource refresh does not replace cave materials | DropTableId-based policy; rare materials gated | RefreshTests.cs (drop policy test) | PASS |

---

## Code Evidence

**Files:**
- `Assets/_Game/Scripts/Farm/Resources/ResourceNodeDefinition.cs`
- `Assets/_Game/Scripts/Farm/Resources/ResourceNodeInstanceState.cs`
- `Assets/_Game/Scripts/Farm/Resources/ResourceNodeType.cs`
- `Assets/_Game/Scripts/Farm/Resources/FarmResourceNodeService.cs` — harvest + idempotency
- `Assets/_Game/Scripts/Farm/Resources/FarmResourceRefreshProcessor.cs` — zone/cooldown guards

**Test file:** `Assets/_Game/Tests/EditMode/Farm/FarmResourceNodeRefreshTests.cs` (11 tests)

---

## Testing Quality Gate

**Automated tests:** YES (11 tests, all PASS)  
**Deterministic logic:** YES (harvest, refresh, idempotency, depletion)  
**Play Mode scenario:** DEFERRED (visual harvest, respawn animation)

---

## Validation

- **Assembly-CSharp:** PASS (0E, 0W)
- **Tests:** 11/11 PASS
- **No forbidden files modified**

---

*Expansion: 2026-06-08*  
*Original stub: 8 lines → Expanded with acceptance criteria, code evidence, test evidence, compliance matrix*  
*No code changes — documentation expansion only*
