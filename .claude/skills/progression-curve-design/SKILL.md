---
name: progression-curve-design
description: Design XP curves, stat scaling, unlock gating and upgrade tier costs. Use for fable_42 (cap 100 XP curve), fable_02/18 (derived stats), fable_03 (equipment baselines), skill tree costs, tool tiers, or any spec defining progression numbers.
---

# Skill: Progression Curve Design

## Curve selection guide

| Need | Curve | Formula sketch | Use for |
|---|---|---|---|
| Steady early levels, slowing late | Polynomial | `xpToNext = base * level^k` (k ≈ 1.5–2.2) | Player level to cap 100 |
| Each tier a deliberate investment | Geometric | `cost = base * ratio^tier` (ratio ≈ 1.6–2.5) | Tool upgrades, skill tree nodes, building tiers |
| Linear utility, capped | Linear with cap | `value = base + perLevel * level` (clamp) | Derived stats per attribute point |
| Early power spike, diminishing | Diminishing returns | `value = max * level / (level + half)` | Resistances, percent-based stats |

## Project-specific rules

1. **Cap is 100 (+ level 101 endgame).** Any curve must produce sane values at level 1, 50 AND 100 — compute and record all three in the spec/report. Beware percent stats crossing 100% or damage formulas inverting at high values.
2. **Derived stats live in data, not code constants.** Curve parameters belong in balance SOs (`PlayerNeedsBalanceSO`, `GameTimeBalanceSO` precedent — create the equivalent for combat curves), so tuning never requires recompiling formulas.
3. **Gating consistency:** an unlock gated by cave depth must also be reachable by the player level expected at that depth. Cross-check: time-to-level (from XP curve + XP sources) vs. time-to-depth. Mismatch = wall or trivialization.
4. **Respec exists** (Fonte/Anya respec). Curves must not assume permanent choices; costs must stay meaningful after respec (no free re-optimization loop each day).
5. **Each tier needs a felt difference.** If tier N+1 is <15% better than N for the listed cost, merge tiers or raise the delta — invisible upgrades are dead content.
6. **XP sources must sum:** when defining the curve, list every XP source per stage (combat per enemy tier, quests, farm actions if any) and verify expected time per level stays within the pacing target (state the target, e.g., level 10 by end of week 1).

## Mandatory deliverable

Curve table in the spec/report — never only the formula:

```text
| Level/Tier | Cost/XP to next | Cumulative | Expected real-time | Key unlocks at this point |
```

With sample rows at minimum: 1, 5, 10, 25, 50, 75, 100. Plus EditMode tests asserting curve invariants (monotonic, no overflow at cap, valid at boundaries) — skill: editmode-test-authoring.
