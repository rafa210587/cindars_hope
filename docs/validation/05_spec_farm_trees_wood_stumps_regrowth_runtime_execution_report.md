# Execution Report — 05_spec_farm_trees_wood_stumps_regrowth_runtime

## Status
RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES

## Implementation Decision
CREATE_MINIMAL — no existing tree system.

## Acceptance Criteria

| Criterion | Implementation | Status |
|-----------|---------------|--------|
| TreeDefinition/Instance contracts | TreeDefinition + TreeInstanceState | OK |
| Chop hits reduce count | RemainingHits-- per chop | OK |
| Final hit fells tree | RemainingHits <= 0 → Felled + drops | OK |
| ManaTree loreprotected | Kind=ManaReserved → IsLoreProtected → chop blocked | OK |
| Stump persists until removed | Stage=Stump, RemoveStump required | OK |
| Regrowth policy tracked | NextRegrowthEligibleDay set on fell | OK |
| Tests | 9 EditMode tests | OK |

## Validation
Assembly-CSharp: PASS (0E, 0W) | Mode: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES

---
*Date: 2026-06-08 | Status: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES*
