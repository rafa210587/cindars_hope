# Phase 07 — Combat and survival passives

**Spec:** `spec_skills_17_combat_survival_passives_v1`  
**Validation date:** 2026-09-10  
**Result:** PASS (automated scope)

## Delivered behavior

- Passive modifiers preserve node/tree identity and are aggregated idempotently.
- Typed derived channels isolate melee, magic, Arcane Bolt, bow recovery, two-handed,
  guarded defense, mana-base regeneration, terrain recovery and kiting.
- Equipment gates reject broken or incompatible main/off-hand equipment.
- Eagle Focus and Elemental Confluence retain their ranged/magic rewards.
- Kiting uses a deterministic nearest-threat tie-break, a 20-tile search radius and a
  two-second window; melee, timeout, lost threat, scene transition and respec clear it.
- Dodge Training composes with Retreat multiplicatively and respects the 50% floor in
  both dodge controllers.
- Hunger fractional drain survives save/load. Legacy saves default the remainder to zero.
- Status duration follows `clamp(base × resistance × recovery, 1, 30)` and recovery only
  applies to Poison, Burn and Slow.
- Resistance snapshots publish both values and effective duration reductions.
- All legacy weapon assets are included in handedness generation; the wooden spear is
  explicitly two-handed.

## Evidence

- Unity compile: PASS — `Logs/skills-phase17-compile-final.log`.
- Skills EditMode: PASS, 138/138 — `Logs/skills-phase17-editmode-closed-results.xml`.
- Phase 17 PlayMode composition: PASS, 4/4 —
  `Logs/skills-phase17-playmode-closed-results.xml`.
- Survival action regression, isolated fixture: PASS, 5/5 —
  `Logs/skills-phase17-survival-regression-results.xml`.
- Canonical skill generation: PASS, 66 nodes / 5 trees / 31 actions —
  `Logs/skills-phase17-skill-generate-hardening.log`.
- Weapon handedness generation: PASS —
  `Logs/skills-phase17-weapon-generate-hardening.log`.
- Scoped `git diff --check`: PASS.
- Independent non-regression re-audit: APPROVED WITH WARNING; no Phase 17 blocker.

## Triage note

A combined run of 44 skill PlayMode tests reported 42 passes and two failures in
`SkillSurvivalActionsPlayModeTests`. The shared run inherited live cave enemies from earlier
fixtures. Re-running that class in isolation passed 5/5, so the failures are classified as
test-order contamination rather than a gameplay regression. The combined result is retained
at `Logs/skills-phase17-playmode-regression-results.xml` and is not presented as passing evidence.

## Residual validation

Manual feel and visual readability belong to the final in-game acceptance phase after the skill
tree, loadout UI and animation feedback are integrated. Numeric and runtime contracts in this
phase are covered by deterministic and PlayMode tests.
