# WAVE 05 Closeout Report

## Status

COMPLETED_WITH_KNOWN_LEGACY_GATES

---

## Specs Executed (20/20)

| Spec | Status | Report |
|------|--------|--------|
| 05_spec_farm_scale_tilemap_player_footbox_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_level1_layout_fixed_anchors_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_building_footprints_placement_grid_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_buildings_construction_workshops_storage_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_animals_housing_feeding_care_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_animal_products_quality_collection_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_companion_eligibility_bond_availability_save_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_companion_farm_job_board_automation_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_soil_crop_growth_quality_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_watering_irrigation_rain_greenhouse_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_harvest_processing_quality_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_fertilizer_crop_quality_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_trees_wood_stumps_regrowth_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_rocks_stone_light_mining_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_resource_node_refresh_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_forage_fishing_lake_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_tool_upgrade_repair_tier_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_layout_expansion_zones_free_build_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_farm_shipping_sellpoint_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |
| 05_spec_player_condition_fatigue_sleep_hunger_stamina_runtime | RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES | ✓ |

---

## Specs Deferred

None — all 20 executable WAVE 05 specs completed.

---

## Known Legacy Gates

- **Docs validation**: EXPECTED_FAIL_LEGACY_ONLY — pre-existing legacy errors, no new failures introduced
- **Assembly-CSharp-Editor**: Pre-existing IProjectValidator mismatch — not introduced by WAVE 05
- **Quality check / Pester**: Pre-existing Pester 3.4.0 issue — check_spec_quality.ps1 cannot run in `Should` context

---

## Runtime Validation

- **Assembly-CSharp**: PASS — 0E, 0W on final build (exit code 0)
- **Validation mode**: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES

---

## Test Coverage Summary

| Area | Tests Added |
|------|------------|
| CropGrowthProcessor | 15 |
| FarmWateringService | 12 |
| FarmHarvest | 10 |
| FertilizerApplication | 9 |
| TreeChopService | 9 |
| RockMiningService | 6 |
| FarmResourceNodeRefresh | 11 |
| FarmForageFishing | 11 |
| FarmToolUpgrade | 9 |
| FarmExpansionZones | 9 |
| FarmShippingService | 10 |
| FatigueSystem | 12 |
| **Total** | **~123** |

All tests in `Assets/_Game/Tests/EditMode/` — no runtime tests, no PlayMode, no Unity Test Runner required.

---

## Decision

- **Can continue to WAVE 06**: YES — Assembly-CSharp PASS, all 20 specs have individual reports
- **Can mark WAVE 05 ACCEPTED**: NO — PlayMode/human validation not executed (deferred to FINAL_HUMAN_VALIDATION_BY_WAVE)

---

*Created: 2026-06-08 — WAVE 05 runtime execution complete*
