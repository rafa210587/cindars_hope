# Phase 08 — Crafting and economy passives

**Spec:** `spec_skills_18_crafting_economy_passives_v1`  
**Validation date:** 2026-09-10  
**Result:** PASS (automated scope)

## Delivered behavior

- Mãos Ágeis and Cuidado no Reparo scale per rank with 60% and 30% aggregate caps.
- Olho de Material uses a persisted seeded attempt ledger. A committed harvest consumes its
  attempt even when the bonus cannot fit, while a cancelled harvest does not consume it.
- Common-material eligibility is an explicit catalog allowlist. Quality variants, magical
  crops, rare metals, progression resources and monster drops are excluded.
- Foco de Bancada affects only explicitly common ingredients at the matching station and uses
  `max(1, ceil(amount × (1 - reduction)))`.
- Salvage accepts only identified, unequipped, individually tracked crafted equipment with a
  canonical unit-output recipe. It returns 40% of common ingredients and has a capped seeded
  chance for one extra material; inventory and RNG commit atomically.
- Every `MaxStack=1` inventory entry receives a stable instance id. Crafted equipment retains
  its id and durability metadata through save, drop and pickup.
- Acabamento Durável snapshots +8% maximum durability per rank, capped at 24%, per crafted
  instance.
- Senso de Mercado applies one exclusive buy or sell direction, capped at 15%. Respec and save
  preserve the pricing contract, and the SellPoint remains neutral.
- Mochila Ordenada is preserved in legacy saves but cannot be newly purchased or ranked until
  its inventory UI/runtime exists; no point or success event is consumed.
- Player-facing descriptions in the generated runtime skill assets match the implemented
  formulas and limits.

## Evidence

- Unity compile: PASS — `Logs/skills-phase18-compile-final2.log`.
- Skills EditMode: PASS, 177/177 — `Logs/skills-phase18-editmode-final-results.xml`.
- Item catalog policy: PASS, 20/20 —
  `Logs/skills-phase18-item-catalog-closeout-results.xml`.
- Item drop identity EditMode: PASS, 3/3 —
  `Logs/skills-phase18-drop-editmode-results.xml`.
- Crafting passives PlayMode: PASS, 2/2 —
  `Logs/skills-phase18-crafting-playmode-closeout-results.xml`.
- Material Eye PlayMode: PASS, 1/1 —
  `Logs/skills-phase18-material-eye-playmode-closeout-results.xml`.
- Salvage PlayMode: PASS, 1/1 —
  `Logs/skills-phase18-salvage-playmode-closeout-results.xml`.
- Drop/pickup identity PlayMode: PASS, 1/1 —
  `Logs/skills-phase18-drop-playmode-results.xml`.
- Canonical item generation: PASS and idempotent on the second run —
  `Logs/skills-phase18-item-generation-closeout.log`,
  `Logs/skills-phase18-item-generation-idempotent-closeout.log`.
- Canonical skill generation: PASS, 66 nodes / 5 trees / 31 actions —
  `Logs/skills-phase18-skill-generation-closeout.log`.
- Scoped `git diff --check`: PASS.

## Residual validation

Manual feel, tutorial clarity and final icon readability belong to the integrated in-game
acceptance after the skill-tree, loadout and animation phases. The deterministic economy,
identity, save and runtime contracts in this phase are covered by EditMode and PlayMode tests.
