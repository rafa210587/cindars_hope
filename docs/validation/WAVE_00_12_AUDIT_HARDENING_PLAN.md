# WAVE 00–12 Audit Hardening Plan

**Status:** IN_PROGRESS
**Date:** 2026-06-08
**Branch:** dev
**Goal:** Repair 4 documentation findings (FIND-01 through FIND-04) and remove tech debt (FIND-03)

---

## Task Breakdown

| Task | Finding | Description | Status |
|------|---------|-------------|--------|
| 1 | FIND-01 | Expand 7 WAVE 05 stub reports with code/test evidence | PENDING |
| 2 | FIND-02 | Create 4 retrospective WAVE 09 execution reports | PENDING |
| 3 | FIND-04 | Create WAVE 03 → WAVE 09 traceability redirect | PENDING |
| 4 | FIND-03 | Remove dead `*Status2` enums from EndgameContracts.cs | PENDING |
| 5 | — | Final validation (dotnet build, docs validation) | PENDING |
| 6 | — | Create closeout report | PENDING |
| 7 | — | Commit all changes | PENDING |

---

## FIND-01 Details — 7 WAVE 05 Stub Reports

Specs to expand:
1. `05_spec_farm_rocks_stone_light_mining_runtime`
2. `05_spec_farm_resource_node_refresh_runtime`
3. `05_spec_farm_layout_expansion_zones_free_build_runtime`
4. `05_spec_farm_forage_fishing_lake_runtime`
5. `05_spec_farm_shipping_sellpoint_runtime`
6. `05_spec_player_condition_fatigue_sleep_hunger_stamina_runtime`
7. `05_spec_farm_tool_upgrade_repair_tier_runtime`

Method: Extract acceptance criteria from spec files; find code evidence in `Assets/_Game/Scripts/`; find tests in `Assets/_Game/Tests/EditMode/`.

---

## FIND-02 Details — 4 Missing WAVE 09 Reports

Missing individual reports (closeout lists but no file):
1. `09_spec_quest_flag_system_runtime`
2. `09_spec_quest_conditions_triggers_runtime`
3. `09_spec_quest_definition_state_runtime`
4. `09_spec_quest_reward_applicator_runtime`

(Note: May also include `09_spec_quest_save_load_normalizer_runtime` — verify from closeout)

Method: Read WAVE_09_CLOSEOUT_REPORT.md; for each missing spec, find commit; extract code and test evidence from quest/ folder.

---

## FIND-04 Details — WAVE 03 → WAVE 09 Redirect

Status: WAVE 03 quest specs (8 total, 4 future) were re-executed in WAVE 09 as new specs.
Action: Create redirect document explaining why WAVE 03 has no execution reports.
Target: `docs/validation/WAVE_03_TO_WAVE_09_TRACEABILITY_REDIRECT.md`

---

## FIND-03 Details — Dead Enum Removal

Enums to remove (if unused confirmed):
- `Level100GateStatus2`
- `Level101AccessStatus2`
- `FinalChoiceStatus2`

File: `Assets/_Game/Scripts/MainProgression/EndgameContracts.cs`

Prerequisite: Grep all `.cs` files to confirm zero usage.

---

*Plan created: 2026-06-08*
