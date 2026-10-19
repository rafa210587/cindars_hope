# Phase 09D — Magic Confluence capstone

**Spec:** `spec_skills_19_combat_capstones_v1` — T19D  
**Validation date:** 2026-09-10  
**Result:** PASS after corrective audit

## Delivered behavior

- Successful committed magic spends enter one six-second sliding ledger. Its first commit snapshots
  MaxMP and arms at `ceil(MaxMP × 35%)`; cancelled, refunded and failed resolutions contribute zero.
- The crossing cast arms without consuming. Anya lasts ten seconds, Senya eight; a matching cast
  consumes the seed and starts a 25-second lockout. Respec clears arm and ledger while preserving
  an active lockout.
- Anya applies the authored 30/40/50% MP discount and 20/28/35% spiritual output bonus without
  creating MP. Its stamina echo uses the discounted cost and resolves over 1/2/4 seconds.
- Senya atomically reserves its 5/8/10% surcharge and applies +20/28/35% direct damage with
  +8/12/15 percentage points of critical chance. Item spells and active magic share prepare,
  reserve, commit and cancel.
- Persistent zones preserve the prepared Senya payload on their pulses with secondary/DoT and
  anti-recursion gates. Total projectile spawn failure refunds before commit.
- Save data uses simple values and restores arm, ledger, echo and lockout silently.

## Evidence

- Unity compile: PASS — `Logs/skills-phase19-integrated-compile.log`.
- Integrated Skills EditMode: PASS, 219/219 —
  `TestResults/skills-phase19-integrated-editmode-r3.xml`.
- Confluence PlayMode through `SpellCastService` and active magic: PASS, 7/7 —
  `TestResults/skills-phase19d-confluence-playmode-r4.xml`.
- Existing magic-shape PlayMode regression: PASS, 5/5 —
  `TestResults/skills-phase19d-magic-regression-playmode-r4.xml`.
- Canonical catalog generation: PASS, 66 nodes, 31 actions and expected tree counts —
  `Logs/skills-phase19-generate-r2.log`.

## Corrective audit

The first independent audit rejected closeout because Toxic Cloud dropped the prepared payload,
projectile spawn failure committed too early, positive-cost item spells accepted a missing mana
runtime, and balance literals were not centralized. All four findings were corrected before the
final PlayMode runs. Active magic also limits its new critical roll to a positive Confluence bonus,
preserving the existing deterministic behavior outside Senya.

The final regression round directly exercises the three rejected paths: Toxic Cloud payload and
gates, zero-projectile refund before commit, and missing-mana-runtime rejection. Critical resolution
is shared by cone and projectile executors and uses the canonical combat critical multiplier.

Final independent non-regression audit: PASS in file scope, runtime APIs, save DTOs, balance values,
ID stability, EventBus, status claims and testing quality gate; no remaining corrective action.
