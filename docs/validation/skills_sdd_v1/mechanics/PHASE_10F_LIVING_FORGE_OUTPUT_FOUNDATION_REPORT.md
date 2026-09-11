# Phase 10F — Living Forge output foundation

**Spec:** `spec_skills_20_survival_crafting_capstones_v1` — slice 20F  
**Validation date:** 2026-09-10  
**Result:** PASS — zero-sale-delta potency amendment validated

## Why this slice exists

The inventory persists item identity and quantity, without per-stack quality or duration metadata.
Existing consumables also cannot receive a useful duration bonus when their authored duration is
zero. Applying Thoren directly would therefore produce rewards that exist only in text.

## Delivered contracts

- Stable item IDs represent Q1, Q2 and potency variants, so existing stack, save, drop, pickup and
  consumption paths preserve the enhancement without a transversal inventory schema migration.
- Q1 and Q2 apply 1.15 and 1.35 only to a numeric consumable payload used by runtime. Equipment
  exposes 1.05/1.10 Max Durability multipliers for the Thoren transaction.
- The rank-three potency variant preserves BaseValue and authored buff duration, applies exactly
  +8% to a scalable payload, accepts batches from one through five and rejects inert outputs.
- Every variant preserves the base item's sale value. Materials and outputs without a measurable
  payload are excluded from quality generation.
- Variants preserve use kind, slots, status effects, weapon/spell links and other serialized payload.
- A missing canonical `item_processed_wood` recipe output was added through the item generator.

## Evidence

- Living Forge focused EditMode: PASS, 29/29 —
  `TestResults/skills-phase20-livingforge-amendment-editmode-r2.xml`.
- Canonical item generator materialized the missing output: one created, 254 unchanged —
  `Logs/skills-phase20f-items-generate.log`.
- Amendment generator removed two obsolete variants, created two potency variants and stabilized
  the 34 active outputs. Final idempotence: PASS, 0 created, 0 updated, 34 unchanged, 0 removed,
  `noChanges=True` — `Logs/skills-phase20-amendment-livingforge-generate-r3.log`.

## Phase boundary

This slice provides functional enhanced outputs but does not grant them. Slice 20T must reserve the
player's explicit choice and common material before crafting, commit the daily charge only after a
real output exists, and preserve the decision across job save/load.

## Independent audit correction

The first audit rejected this slice because the generator included inactive recipe assets and
special consumers recognized only base bomb/irrigator IDs. The corrected generator now uses the
runtime `RecipeDatabase`, removes obsolete assets only from its owned directory and synchronizes
the `ItemDatabase`. Bomb and irrigation actions select and consume the exact variant ID. Corrected
Unity tests pass. Independent review confirmed scope, registry integrity, exact-ID consumption and
idempotence with no blocking findings.
