# Skills SDD v1 — Phase 05 Survival Actions

**Spec:** `spec_skills_15_survival_actions_v1`  
**Status:** PLAYMODE_VALIDATED  
**Date:** 2026-09-10

## Acceptance criteria extracted

The phase makes all six Survival actions operational: Last Breath, Retreat Signal,
Improvised Lure, Emergency Kit, Survival Instinct and Safe Camp. The authoritative
contracts include action-owned cooldowns, atomic costs, deterministic target ordering,
run/encounter identity, save/restore and stable cave revisits.

## Existing systems audit

The implementation reuses the active-skill timeline, action cooldown tracker,
`GameEventBus`, cave runtime state/snapshots, inventory transactions, enemy runtime,
natural player resource ticks and materialized cave objects. It adds narrow Foundation
ports for directional mobility, natural-rate modifiers, skill placement and temporary
reveal. No procedural cave generation rule or visited-level snapshot contract was changed.

## Spec compliance matrix

| AC | Evidence | Result |
|---|---|---|
| AC01 | encounter authority, threshold window, once-per-encounter state and restore tests | PASS |
| AC02 | directional modifier, nearest-threat ordering, stamina cost and pre-effect offensive cancel | PASS |
| AC03 | walkable/line-of-effect placement, deterministic five-target cap, elite/miniboss/boss rules and player-hit cancel | PASS |
| AC04 | 0.8 s timeline, 0.2 s movement tolerance, damage priority, commit-time item transaction and percent heal | PASS |
| AC05 | materialized-target registry, inclusive radius and temporary overlay | PASS |
| AC06 | run-scoped idempotency, fixed zone and natural hunger/fatigue/STA/MP modifiers | PASS |
| AC07 | opaque runId, simple DTOs, restore order and unchanged cave snapshot replay | PASS |

## Validation

- Canonical skill generator: PASS — 66 nodes, 5 trees and 31 actions.
- Canonical item generator: PASS — 216 base rows / 248 expanded rows; three Survival consumables generated.
- Unity compile after generated assets and review fixes: PASS (`Logs/skills-phase15-compile3.log`).
- Skills EditMode: PASS — 102/102 (`Logs/skills-phase15-editmode-pass-results.xml`).
- Survival PlayMode: PASS — 5/5 (`Logs/skills-phase15-survival-playmode-results.xml`).
- Independent non-regression review: seven findings found; all P1 findings were corrected before the final runs.
- Shared cooldown by `skillActionId`: covered by `SkillCooldownTrackerTests` inside the EditMode run.

An integrated Composition run initially completed 40/42 with both failures inside the new
Survival fixture. After correcting shared state and test isolation, the focal Survival run
passed 5/5. A later broad rerun was terminated when an older ranged projectile test remained
waiting for physics; it produced no final XML and is not claimed as PASS. The previously
accepted Phase 13 ranged evidence remains unchanged.

## Honest status rationale

The applicable automated gates for this phase pass and the six actions are wired to live
runtime consumers. Human evaluation of feel, readability and the complete Farm → Town →
Cave → Save/Load loop remains grouped under `FINAL_HUMAN_VALIDATION_BY_WAVE`; it is not
claimed here. Promotion to ACCEPTED is therefore deferred while the mechanics wave continues.
