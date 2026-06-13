# WAVE 08 Closeout Report — City/NPC/Dialogue/Services

**Wave:** 08  
**Date:** 2026-06-08  
**Executor:** Claude Sonnet 4.6 (auto-loop)  
**Status:** COMPLETED_WITH_KNOWN_LEGACY_GATES

---

## Summary

All 4 WAVE 08 specs executed and BUILD_VALIDATED.
WAVE 07 does not exist (no `07_spec_*.md` files) — WAVE 08 followed WAVE 06 directly.

---

## Spec Results

| Spec | Status | Commit | Tests |
|------|--------|--------|-------|
| 08_spec_city_layout_schedule_doors_beds_runtime | BUILD_VALIDATED | 2b15c9d | 18 |
| 08_spec_npc_definition_classes_relationships_runtime | BUILD_VALIDATED | 72483d2 | 14 |
| 08_spec_dialogue_conditions_rumors_farm_visits_runtime | BUILD_VALIDATED | 690d022 | 15 |
| 08_spec_city_services_contracts_licenses_shops_runtime | BUILD_VALIDATED | 650d825 | 14 |

**Total tests added:** ~61 EditMode tests

---

## Validation Baseline

```
Runtime validation mode: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES
Assembly-CSharp: PASS (exit code 0) — all 4 specs
Assembly-CSharp-Editor: legacy blocker / not blocking
Quality check: known Pester issue / not blocking
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (pre-existing)
```

---

## Canon Guards Implemented

| Guard | Implementation |
|-------|---------------|
| Kanthor = public temple, no city HQ blocked | CityBuildingDefinition.IsKanthorTemple reference flag |
| Anya = no active city temple/altar | Blocked in resolver + validator (ServiceId contains check) |
| No D&D class names for NPCs | ForbiddenDnDClassNames HashSet in NpcDefinitionValidator |
| No Folego/Breath/BR stat for NPCs | Reflection test verifies NpcStats has no such field |
| NPC age: no marriage for Child/YoungAdult | NPC_TOO_YOUNG_MARRIAGE validator |
| FarmVisit: no pet visits | IsPetRelated always deferred gate |
| Anya: HasPrivateAnyaSympathy private/narrative only | No public temple integration |

---

## Key Systems Delivered

1. **City Layout** — ZoneDefinition, BuildingDefinition, DoorTrigger, BedDefinition with full gate contract
2. **NPC Schedule** — SchedulePeriod, NpcScheduleResolver with Festival > Lunar > Rain > Quest priority
3. **NPC Definition** — Pure C# NpcDefinition coexisting with NpcDataSO (not modified); FunctionalGameplayClass (17 non-D&D classes)
4. **Dialogue System** — DialogueCondition with 15+ context gates, SpoilerLevel, RumorPool, FarmVisitEligibilityResolver
5. **City Services** — CityServiceAvailabilityResolver with all gates, LicenseDefinition, ContractDefinition (WAVE 09 bridge)

---

## Forbidden Files (Not Modified)

- Packages/ — NOT modified
- ProjectSettings/ — NOT modified
- *.unity scenes — NOT modified
- *.prefab files — NOT modified
- *.asset ScriptableObjects — NOT modified
- NpcDataSO — NOT modified (coexistence pattern)
- DialogueTreeSO — NOT modified (coexistence pattern)

---

## Next Wave

WAVE 09: Quest System (8 specs)
- `09_spec_quest_*.md` files in `.specs/a_implementar/`
