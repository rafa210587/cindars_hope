# Phase 09C — Ranged lunar capstones

**Spec:** `spec_skills_19_combat_capstones_v1` — T19C  
**Validation date:** 2026-09-10  
**Result:** PASS

## Delivered behavior

- The generalized encounter owns one stable first target across reinforcements, death, expiry,
  respec and save/load. Cave scopes use run and level identity; other scenes use scene identity.
- Alihana modifies only arrows launched while Lunar Focus is active and the first target is alive:
  range +8/12/16%, with +15% projectile speed at rank three.
- Nyx checks isolation at impact with the inclusive 2.5-tile boundary, adds +8/12/15 percentage
  points of critical chance and rank-three critical damage, using one critical roll per arrow.
- Senya uses the damage type from the last committed Offensive player magic cast. Its 8/12/16%
  secondary hit cannot crit, apply status or posture, trigger reactions, capstones or recursion.
- Lunar save data contains simple values, restores silently and validates variant, rank and timer.

## Evidence

- Unity compile: PASS — `Logs/skills-phase19-integrated-compile.log`.
- Integrated Skills EditMode: PASS, 219/219 —
  `TestResults/skills-phase19-integrated-editmode-r3.xml`.
- Lunar PlayMode: PASS, 4/4 —
  `TestResults/skills-phase19c-lunar-playmode-r2.xml`.
- Encounter foundation: PASS, 9/9 EditMode and 3/3 PlayMode —
  `TestResults/skills-phase19c-encounter-editmode-results.xml` and
  `TestResults/skills-phase19c-encounter-playmode-results.xml`.
- Canonical ranged asset regenerated with `alihana`, `senya`, `nyx` and no obsolete flat
  modifiers — `Logs/skills-phase19-generate-r2.log`.

## Review

The independent review found that elemental history was initially authored at impact. T19D moved
it to the common successful magic-commit event, so a confirmed Offensive cast establishes history
even if its projectile later misses. The review also confirmed target ownership, impact timing,
isolation, save/respec and anti-recursion gates.
