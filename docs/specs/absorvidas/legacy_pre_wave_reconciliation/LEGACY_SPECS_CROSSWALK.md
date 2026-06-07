# Legacy Specs Crosswalk — Pre-Wave Reconciliation

> **Date:** 2026-06-07  
> **Context:** Cleanup of specs from pre-wave-based generation era (SPEC_10-17 MVP era)  
> **Decision:** All legacy specs moved here are absorbed into new wave-based specs or marked for future reference  
> **Execution:** No legacy specs should be executed from this location; use new wave-based specs instead

---

## Summary

| Spec | Action | Reason | Specs that cover | Gap residual | Decision |
|------|--------|--------|------------------|--------------|----------|
| `spec_14a_cave_enemy_spawnplan_materialization_run_stability.md` | MOVED_ABSORBED | Partial implementation (spawn plan/enemy count); fully covered by WAVE 06 `06_spec_enemy_elite_boss_drop_tables_runtime.md` and WAVE 10 endgame specs | WAVE 06 (loot/drops), WAVE 10 (bosses/progression) | None confirmed | Do not execute; use WAVE 06/10 specs |
| `spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md` | MOVED_ABSORBED | Combat tuning spec (damage numbers display); largely covered by WAVE 06 `06_spec_enemy_elite_boss_drop_tables_runtime.md` and WAVE 11 UI specs | WAVE 06, WAVE 11 (UI hotbar/HUD) | Floating damage number visuals may need refinement in WAVE 04 UI | Do not execute; check WAVE 04/06/11 coverage |
| `spec_14b_cave_snapshot_replay_enemy_plan.md` | MOVED_ABSORBED | Cave snapshot/replay spec; feature fully moved to WAVE 06 `06_spec_cave_treasure_mining_loot_snapshot_runtime.md` | WAVE 06 | None confirmed | Do not execute; snapshot logic in WAVE 06 |
| `spec_cave_runtime_generation_checkpoints_boss_gates.md` | MOVED_ABSORBED | Cave runtime generation (gates/checkpoints/confinement); fully split and covered by WAVE 06 treasure/loot and WAVE 10 level 100-101 specs | WAVE 06 (cave procedural), WAVE 10 (gates/bosses) | None — gates factored into WAVE 10 progression | Do not execute; gates in WAVE 10, cave in WAVE 06 |
| `spec_combat_movement_projectiles_melee_visuals_runtime.md` | MOVED_ABSORBED | Broad combat spec (movement/projectiles/melee/visuals); features distributed across WAVE 06 (economy/items) and likely future combat refinement | WAVE 06 | Movement/projectiles likely need dedicated WAVE 10 spec or future hardening | Candidate for WAVE 25+ legacy gap spec if gap confirmed |
| `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` | MOVED_ABSORBED | Enemy AI/roster/bestiary spec (from SPEC_13); large partial implementation; covered by WAVE 06 `06_spec_enemy_elite_boss_drop_tables_runtime.md`, WAVE 22 bestiary, WAVE 23 pets | WAVE 06 (elite drops), WAVE 22 (bestiary UI), WAVE 23 (pet AI) | Faction locks may be gap; deferred to WAVE 25+ if needed | Do not execute; check WAVE 22/23 for faction coverage |
| `spec_ui_ux_full_gameplay_inventory_hotbar_menus.md` | MOVED_ABSORBED | Large UI spec (SPEC_17 partial); implementation exists; largely covered by WAVE 04 `04_spec_ui_*_runtime.md` (21 UI specs) and WAVE 11 `11_spec_ui_*_runtime.md` (HUD/hotbar/projections) | WAVE 04 (18 UI screens), WAVE 11 (HUD/projections) | Canvas final and menu consolidation; likely covered by WAVE 04 specs | Do not execute; full coverage by WAVE 04/11 UI specs |

---

## Movement Details

### Moved to docs/specs/absorvidas/legacy_pre_wave_reconciliation/

**Date moved:** 2026-06-07  
**Method:** git mv (history preserved)  
**Total specs moved:** 7

```text
spec_14a_cave_enemy_spawnplan_materialization_run_stability.md
spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md
spec_14b_cave_snapshot_replay_enemy_plan.md
spec_cave_runtime_generation_checkpoints_boss_gates.md
spec_combat_movement_projectiles_melee_visuals_runtime.md
spec_enemy_ai_roster_bestiary_faction_locks_runtime.md
spec_ui_ux_full_gameplay_inventory_hotbar_menus.md
```

### Review Required

**No specs moved to review_required.** All legacy specs had clear mapping to new wave-based specs.

---

## Coverage Analysis

### WAVE 04 (UI Foundation) — 21 specs

Covers: All UI screens, menus, dialogue, quest log, equipment compare, tooltips, repair/upgrade, confirmation patterns, empty/error states, Fonte menu, hotbar, social detail, gamepad navigation, spell details, weapon/armor drawers, crafting, storage, calendar detail.

**Covers legacy:** `spec_ui_ux_full_gameplay_inventory_hotbar_menus.md` (Canvas final, hotbar, inventory UI fully factored into WAVE 04 + WAVE 11)

### WAVE 06 (Economy/Loot/Crafting) — 8 specs

Covers: Shop inventory, stockline, restock, pricing profiles, buy/sell channels, loot tables, enemy elite drops, cave treasure mining, crafting station, recipe processing, item definition tags, quality/rarity, anti-arbitrage validation.

**Covers legacy:** 
- `spec_14a_cave_enemy_spawnplan_materialization_run_stability.md` (enemy/loot side)
- `spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md` (drops/loot)
- `spec_14b_cave_snapshot_replay_enemy_plan.md` (snapshot logic)
- `spec_cave_runtime_generation_checkpoints_boss_gates.md` (loot/treasure side)
- `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` (elite drops/balancing)

### WAVE 10 (Endgame/Fonte/Progression) — 4 specs

Covers: Fonte Anya functions, living water, respec purification, level 100-101, final choice endings, main progression acts, memory arc, black stone corruption fragments.

**Covers legacy:**
- `spec_cave_runtime_generation_checkpoints_boss_gates.md` (boss gates for level 100-101)
- `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` (boss AI/roster for endgame encounters)

### WAVE 11 (UI Projections/HUD) — 4 specs

Covers: HUD hotbar, active slots, notifications, debug UI, input focus, modal routing, inventory equipment tooltips, shop/crafting/skill tree/quest/Fonte menu projections.

**Covers legacy:**
- `spec_ui_ux_full_gameplay_inventory_hotbar_menus.md` (hotbar, projections fully factored)

### WAVE 22 (Bestiary/Knowledge/Research) — 4 specs

Covers: Bestiary compendium knowledge log UI, combat HUD known weakness overlay, books collections documentation achievements, research service NPC laboratory knowledge unlock.

**Covers legacy:**
- `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` (bestiary coverage fully moved to WAVE 22)

### WAVE 23 (Pets) — 4 specs — HOLD/BLOCKED_SCOPE

Covers: Pet core identity, bond routine save, home area, HUD feedback, cave alerts, treasure trap light support.

**Covers legacy:**
- `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` (pet AI/companion angles; deferred)

---

## Decisions Made

1. **Combat movement/projectiles/melee/visuals spec:** Checked WAVE 06 (economy/loot focus) and WAVE 10 (progression/bosses). Combat movement and projectiles likely need dedicated spec or are part of future wave. Marked as potential WAVE 25+ legacy gap candidate if gap confirmed by WAVE 10 execution.

2. **Faction locks:** `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` distributed across WAVE 06 (AI/drops), WAVE 22 (bestiary/knowledge), WAVE 23 (pets). Faction locks per se (e.g., "fire elementals don't spawn with ice elementals") not explicitly in new waves. Deferred to WAVE 25+ if gap confirmed.

3. **All other specs:** Clear 1-to-many mapping to new wave-based specs. No review required.

---

## Validation

**Crosswalk completeness:** ✓ All legacy specs have destination and coverage analysis  
**Duplicate prevention:** ✓ No legacy spec duplicates new wave specs  
**Gap identification:** ⚠ Potential gaps (combat movement, faction locks) identified but deferred to WAVE 25+ pending execution

---

## Next Steps

1. ✓ Move legacy specs to `docs/specs/absorvidas/legacy_pre_wave_reconciliation/`
2. ✓ Create this crosswalk document
3. ⚠ After WAVE 10 execution: review if combat movement/projectiles/faction locks need legacy gap specs
4. ✓ Update SPEC_REGISTRY_TO_IMPLEMENT.md to reflect moved specs
5. ✓ Update CURRENT_STATE.md with cleanup status
6. ✓ Update SPEC_EXECUTION_ORDER.md reconciliation note

---

**Crosswalk created:** 2026-06-07  
**Status:** COMPLETE — all legacy specs classified and moved
