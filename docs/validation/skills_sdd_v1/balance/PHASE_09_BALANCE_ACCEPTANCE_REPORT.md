# Phase 09 — comparative skill balance acceptance

**Spec:** `spec_skills_21_balance_acceptance_v1`  
**Date:** 2026-09-10  
**Status:** IN PROGRESS — first matrix candidate rejected; runtime observations missing

## Current result

The catalog is not yet accepted as globally balanced. Phase 20 proved each mechanic's local
contracts, while this phase must compare actual builds at ranks 1/3 and levels 20/50 against real
Common, Elite, Miniboss and Boss targets.

The first matrix candidate was removed after independent review. It could return `Accepted` from a
caller-provided `IsAssetReal` flag, did not reject TTK outside canonical bands, aggregated medians
across different builds, and treated control as a risk cost. Keeping it would create false confidence
and violate the requirement that real enemies/builds remain the source of acceptance.

## Evidence

- Rejected candidate did compile and execute 12 tests, but that evidence is diagnostic only and is
  not accepted as a balance gate: `TestResults/skills-phase21-foundation-integrated-editmode.xml`.
- Independent review: FAIL, with P0 false acceptance and missing TTK enforcement. The candidate
  runtime/test files were removed rather than retained as dead or misleading infrastructure.

## Acceptance matrix status

| Requirement | Status | Missing evidence |
|---|---|---|
| AC01 build grid R1/R3 L20/L50 | PARTIAL | Grid contract exists; real assets/equipment runs absent |
| AC02 TTK and 25% comparison | PARTIAL | Evaluator and comparison pass synthetic contracts; no runtime medians |
| AC03 resources, refusals, whiffs, hits and control | NOT RUN | Event capture and real windows absent |
| AC04 survival sustain | NOT RUN | Short/long real runs and provision accounting absent |
| AC05 economy `<20%` | NOT RUN | No canonical median-income fixture is authored yet |
| AC06 regenerate after tuning | NOT APPLICABLE YET | No Phase 21 tuning applied |
| AC07 graphical Farm/Town/Cave | NOT RUN | No graphical acceptance session yet |

## Next executable slices

1. Connect action commits, failed skill feedback, hits and status windows to combat telemetry with
   additive report fields.
2. Derive the stage-income fixture from existing canonical farm/cave/shop activities; fail closed if
   a stage lacks enough comparable activities.
3. Feed real build and enemy observations into the matrix, then execute the graphical session.
4. Apply data tuning only where measured rows fail, regenerate and repeat affected rows.

Skill-tree Canvas, loadout UI and animation phases remain blocked until this report reaches PASS.
