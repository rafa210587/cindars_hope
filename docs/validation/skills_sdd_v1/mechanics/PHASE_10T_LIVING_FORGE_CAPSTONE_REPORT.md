# Phase 10T — Living Forge / Thoren capstone

**Spec:** `spec_skills_20_survival_crafting_capstones_v1` — slice 20T  
**Validation date:** 2026-09-10  
**Result:** PASS — automated mechanics and transaction gates complete

## Delivered behavior

- The playable crafting modal lets the player keep the daily charge or explicitly choose Quality
  or Save Common Material before crafting.
- Rank one produces functional Q1. Rank two produces Q2 or saves exactly one eligible common
  material while keeping at least one consumed. Rank three preserves that choice and adds +8% Max
  Durability to durable equipment or +8% potency to an eligible consumable batch of at most five.
- Independent economy review rejected the initial sale-value uplift and the duration path: the
  former could add up to 465.50g in one daily shipping action, while no authored item in the current
  catalog has positive buff duration. The approved amendment keeps variant BaseValue equal to the
  base item and scales only payload/durability that the runtime actually consumes.
- A shared daily state reserves the charge before requirements are committed. Output insertion is
  the commit point. Failure and cancellation release it; a full inventory keeps the finished job
  and reservation for retry.
- Job and skill state save only IDs, numbers and enum values. Loading or respec does not rearm an
  already consumed day; a later day is available.
- Equipment durability is applied once in this order: authored/crafted baseline, Q1/Q2 quality,
  then the rank-three +8% bonus. The deterministic expectations for base 100 are 105, 110, 119 and
  108 for Q1, Q2, R3 Quality and R3 Save respectively.
- The obsolete capstone craft-time and repair modifiers were removed from the source catalog.

## Static review

- `git diff --check`: PASS.
- Forbidden API scan: PASS.
- Independent audit: PASS after two correction rounds. The review caught and closed a missing
  playable call site, non-cumulative rank three, unreachable Q2, forced daily-charge spending and
  missing quality durability multipliers.

## 20V evidence

- Canonical skill asset regenerated with the functional-potency description and no obsolete passive
  modifiers: `Logs/skills-phase20-amendment-canonical-generate-r2.log`.
- Living Forge variants regenerated and stable on repeat: 34 unchanged, `noChanges=True`:
  `Logs/skills-phase20-amendment-livingforge-generate-r3.log`.
- Focused EditMode PASS, 29/29: `TestResults/skills-phase20-livingforge-final-editmode.xml`.
- Integrated Skills EditMode PASS, 257/257:
  `TestResults/skills-phase20-integrated-editmode-r3.xml`.
- PlayMode through crafting runtime/modal PASS, 5/5, covering opt-out, cancel, inventory-full retry,
  save/load, day rollover and rank-three potency:
  `TestResults/skills-phase20-livingforge-final-playmode.xml`.
- Fresh Caveborn/Telisandra PlayMode PASS, 4/4:
  `TestResults/skills-phase20-caveborn-playmode-fresh.xml`.
- Economic channel parity is tested through EconomyPricingService/SellPoint,
  ShopManager.CalculateSellPrice, ShippingPriceResolver and ItemPriceResolver. The stage-income
  `<20%` comparison remains assigned to the approved Phase 21 balance matrix and is not claimed here.
