# WAVE 05 Dependency Resolution Plan

> **Date:** 2026-06-08  
> **Mode:** DEPENDENCY_RESOLUTION_MODE  
> **Original Target:** `05_spec_companion_farm_job_board_automation_runtime`

---

## Original Target

`05_spec_companion_farm_job_board_automation_runtime` — Companion Farm Job Board

**Why blocked:** Depends on farm_animals, which has a 6-level dependency chain below it.

---

## Dependency Chain (Lowest → Highest)

| Order | Spec | Dependencies | Status |
|---|---|---|---|
| 1 (ROOT) | `05_spec_farm_scale_tilemap_player_footbox_runtime` | None | **EXECUTE FIRST** |
| 2 | `05_spec_farm_level1_layout_fixed_anchors_runtime` | ← spec 1 | Blocked by 1 |
| 3 | `05_spec_farm_building_footprints_placement_grid_runtime` | ← spec 2 | Blocked by 2 |
| 4 | `05_spec_farm_buildings_construction_workshops_storage_runtime` | ← spec 3 | Blocked by 3 |
| 5 | `05_spec_farm_animals_housing_feeding_care_runtime` | ← spec 3, 4 | Blocked by 3-4 |
| 6 (TARGET) | `05_spec_companion_farm_job_board_automation_runtime` | ← spec 5 | Blocked by 5 |

---

## Execution Mode

**DEPENDENCY_RESOLUTION_MODE**: Execute lower-level dependencies first, then return to original target.

**Rule:** Do NOT pivot to random specs. DO resolve the full chain in order.

---

## Stop Conditions

Pause chain if any spec:
- Is future/mapped
- Is pet/HOLD/BLOCKED_SCOPE
- Requires WAVE 06+
- Requires scene/prefab/asset edits
- Requires Packages/ProjectSettings changes
- Build/docs/quality validation fails
- Declares non-resolvable blocker

---

## Batch Accounting

- **Batch capacity:** 10 specs max
- **Already executed:** 1 spec (companion eligibility, BUILD_VALIDATED)
- **Chain capacity:** Up to 6 specs (farm_scale → farm_job_board)
- **Remaining buffer:** 3 specs for independent WAVE 05 specs after chain

---

## Dependency Reports to Update

The following reports created earlier should be reclassified:

| File | Old Status | New Status |
|---|---|---|
| `05_spec_companion_farm_job_board_automation_runtime_execution_report.md` | BLOCKED | BLOCKED_BY_DEPENDENCY_PENDING |
| `05_spec_farm_animals_housing_feeding_care_runtime_execution_report.md` | BLOCKED | BLOCKED_BY_DEPENDENCY_PENDING |
| `05_spec_farm_building_footprints_placement_grid_runtime_execution_report.md` | BLOCKED | BLOCKED_BY_DEPENDENCY_PENDING |
| `05_spec_farm_level1_layout_fixed_anchors_runtime_execution_report.md` | BLOCKED | BLOCKED_BY_DEPENDENCY_PENDING |

---

## Next Step

Execute `05_spec_farm_scale_tilemap_player_footbox_runtime` (root spec).

Then proceed up the chain in order.

---

*Plan created: 2026-06-08 / Dependency Resolution Mode active*

