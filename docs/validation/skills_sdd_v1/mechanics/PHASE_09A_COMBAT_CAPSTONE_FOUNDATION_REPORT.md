# Phase 09A — Combat capstone foundation

**Spec:** `spec_skills_19_combat_capstones_v1` — T19A  
**Validation date:** 2026-09-10  
**Result:** PASS

## Delivered behavior

- Damage requests and results now carry source/target identity, action token, critical state,
  primary/secondary classification and explicit gates for capstones, status and reactions.
- Enemy damage resolution fixes the target identity before calculation and publication.
- Perfect block and player-caused posture break can share one monotonic `ResolutionId`, allowing
  later capstones to deduplicate a single combat resolution.
- Player stun duration and knockback expose synchronous resistance providers. Existing enemies
  keep neutral behavior and existing player scenes opt in through the player composition.
- Projectiles preserve action and target context until impact. Bow critical chance is resolved
  once against the actual impacted target, while the player attack contribution remains intact.
- Spells and active magic skills carry an explicitly authored discipline: `None`, `Spiritual`
  or `Offensive`.
- Spell casting exposes prepare, reserve, commit and cancel transaction seams for the later
  Confluence implementation without granting any capstone reward in this foundation phase.

## Evidence

- Unity compile: PASS — `Logs/skills-phase19a-compile-after-tests.log`.
- Combat capstone foundation EditMode: PASS, 10/10.
- Spell discipline catalog EditMode: PASS, 3/3.
- Ranged identity regression EditMode: PASS, 7/7.
- Combat capstone foundation PlayMode: PASS, 2/2.
- Ranged identity regression PlayMode: PASS, 5/5.
- Spell and active-skill generators: PASS and idempotent —
  `Logs/skills-phase19a-spell-generation-final.log`,
  `Logs/skills-phase19a-skill-generation-final.log`.
- Independent non-regression audit: PASS, with no T19A findings.

## Deferred by the approved phase boundary

The real `SpellCastService` and active-skill routing through prepare/reserve/commit/cancel is
part of T19D. Kanthor, Kaand and lunar rewards are intentionally absent until T19B and T19C.
