# Skills SDD v1 — Phase 06 Crafting and Farm Actions

**Spec:** `spec_skills_16_crafting_actions_v1`  
**Status:** PLAYMODE_VALIDATED  
**Date:** 2026-09-10

## Acceptance criteria extracted

The phase makes Field Patch, Quick Repair, Irrigator, Craft Bomb and Efficiency Mark
operational. The authoritative contracts include action-owned cooldowns, atomic item and
stamina costs, deterministic equipment and payload selection, recipe gating and a
fractional stamina ledger that never grants a benefit without a completed work action.

## Existing systems audit

The implementation reuses the active-skill timeline, action cooldown tracker, inventory
capture/restore, equipment durability, FarmPlot watering, CraftingRuntime and the existing
recipe unlock authority. Narrow executors connect those systems to skill actions. Repair
kits remain bench resources outside the two field skills, and elemental bomb recipes use
stable unlock IDs rather than transient inventory metadata.

## Spec compliance matrix

| AC | Evidence | Result |
|---|---|---|
| AC01 | exact durability transaction, deterministic lowest-durability target, 60% cap and basic-kit consumption; preview carried to UI dependency | PASS (runtime) |
| AC02 | rank timing, outside-combat channel, 85% cap and standard-kit consumption | PASS |
| AC03 | equipped watering-can gate, 18 STA, charge transaction and deterministic 3/4/5 dry-plot line | PASS |
| AC04 | stable payload order, generated elemental recipe gates, arcing projectile, radius/damage/cap contract | PASS |
| AC05 | materialized station anchor, inclusive radius, 12/16/20 s window and persistent fractional ledger | PASS |
| AC06 | rollback on inventory/output failure, cooldown only after commit and stamina refund paths | PASS |

## Validation

- Canonical item generator: PASS — 222 base rows / 254 expanded rows; six items and six recipes for the phase (`Logs/skills-phase16-items-generate3.log`).
- Generated recipe gates: PASS — Physical is default; Fire/Frost/Shock/Toxic persist stable `RequiredRecipeUnlockId` values.
- Unity compile after final independent-review fix: PASS (`Logs/skills-phase16-compile-closed.log`).
- Skills EditMode: PASS — 117/117 (`Logs/skills-phase16-editmode-final-results.xml`).
- Runtime composition PlayMode: PASS — 2/2 (`Logs/skills-phase16-composition3-results.xml`).
- Skill execution foundation PlayMode: PASS — 14/14 (`Logs/skills-phase16-foundation2-results.xml`).
- Shared cooldown by `skillActionId` remains covered inside the EditMode suite.

## Review corrections

The independent review found early-commit paths in crafting, a station-lifetime leak in
Efficiency Mark, portable kit bypass, recipe-gating gaps and unchecked rollback paths.
The final implementation commits the Mark ledger only after successful work, refunds STA
on failed crafting, clears a Mark whose station or scene disappears, restricts direct kit
use to a bench context, gates elemental recipes, and restores captured inventory state on
failed downstream operations. Its second pass approved the atomic workbench transaction
and the materialized `CraftingPoint` authority with no remaining blocker. Readiness also
rejects equipment already at maximum durability.

## Honest status rationale

The applicable automated gates pass and the five actions are connected to live runtime
consumers. Human evaluation of feel, targeting readability and the complete Farm → Town →
Cave → Save/Load loop remains grouped under `FINAL_HUMAN_VALIDATION_BY_WAVE`; it is not
claimed here. Promotion to ACCEPTED is deferred while the mechanics wave continues.
The loadout/tree UI phase must display the deterministic repair target and resolved bomb
payload before commit; this visual dependency is tracked there and is not claimed by this
runtime phase.
