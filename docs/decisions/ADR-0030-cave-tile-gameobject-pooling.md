---
doc_type: adr
status: proposed
adr_id: ADR-0030
title: GameObject Pooling and Tilemap for Cave Materialization
date: 2026-07-03
source_documents:
  - .claude/skills/object-pooling-pattern/SKILL.md
  - .claude/rules/cave-stable-run.md
supersedes: []
superseded_by: []
applies_to:
  - cave-runtime
  - performance
  - object-pooling
  - tilemap-world-rendering
---

# ADR-0030 — GameObject Pooling and Tilemap for Cave Materialization

## Status

**proposed** (draft — requires human decision before any implementation)

## Relationship to existing patterns

The project already has an `object-pooling-pattern` skill for high-churn spawns (projectiles,
floating damage text, drops) and a `tilemap-world-rendering` skill for ground/rule-tile rendering.
This ADR does not invent a new pattern — it proposes applying both existing patterns to a system
that currently uses neither: the cave's procedural tile materialization.

## Context

Reconfirmed 2026-07-03 by direct code inspection: **no `GameObject` pool exists in the project**
(`Select-String -Pattern "class\s+\w*Pool\b"` returns 3 matches project-wide, and none of them are
in the `Cave/Runtime/**` materializer files). `CaveTileMaterializer.cs` instantiates one
`GameObject` per floor tile (line 39: `Object.Instantiate(floorPrefab, worldPos,
Quaternion.identity, floorParent.transform)`) and one per wall tile (line 83, same pattern) with no
pooling — every cave level materialization is a fresh batch of `Instantiate` calls, and (by the
same pattern, confirmed present in sibling files)
`CaveEnvironmentElementMaterializer.cs`/`CaveResourceNodeMaterializer.cs`/
`CaveHazardMaterializer.cs`/`CaveTrapMaterializer.cs`/`CaveExitMaterializer.cs` all instantiate
per-element GameObjects the same way for decor, resource nodes, hazards, traps, and exit portals.

This is architecturally consistent with the `cave-stable-run` contract today (a revisited
`CaveLevel` must not reroll layout/enemies/resources — the current code satisfies that by
regenerating the *same* deterministic content and re-instantiating fresh GameObjects for it each
time the level is entered), but it means **every cave-level entry/re-entry pays the full
Instantiate/Destroy cost** for potentially hundreds of tiles, with no reuse of already-created
GameObjects across level transitions within the same run.

## Decision (proposed, not accepted)

Two complementary changes, evaluated separately:

1. **GameObject pooling for cave tile/decor/hazard/resource-node materializers**, using the
   existing `object-pooling-pattern` skill's conventions: pre-warm a pool per prefab type, acquire
   on materialization, release (not `Destroy`) on level exit/despawn, reuse on next materialization
   instead of a fresh `Instantiate`. This must preserve the `cave-stable-run` invariant exactly —
   pooling changes *object lifecycle management*, not *what gets spawned where*, so the
   deterministic seed → content mapping must remain untouched.
2. **Tilemap for cave floor/wall tiles specifically** (not decor/hazards/resource nodes, which are
   discrete interactable objects better suited to pooled GameObjects than a Tilemap cell): floor
   and wall tiles are the highest-count, most uniform content in `CaveTileMaterializer.cs` and are
   the best candidate for the same Tilemap-based rendering the `tilemap-world-rendering` skill
   already establishes for ground rendering elsewhere, following the same "Tilemap for
   repeated/uniform grid content, GameObject for discrete interactables" split ADR-0023 proposes
   for the TownScene forest.

### Alternatives considered

- **Do nothing (status quo):** correct today, but every cave level transition pays full
  Instantiate/Destroy cost proportional to tile/decor/hazard/resource-node count; this cost grows
  with any future increase in cave level size or decor density (mirroring the TownScene forest
  growth trend noted in ADR-0023).
- **Pooling only, no Tilemap:** lower implementation cost, keeps the current
  one-GameObject-per-tile floor/wall representation but reuses objects across level transitions.
  Solves the churn cost but not the per-tile GameObject/Transform/SpriteRenderer overhead itself.
- **Tilemap only, no pooling:** solves the floor/wall overhead (many tiles become tilemap cells,
  not GameObjects) but does nothing for decor/hazard/resource-node/exit-portal churn, which remain
  discrete GameObjects regardless.
- **Both (this proposal):** Tilemap for the uniform floor/wall grid (biggest single count
  reduction) plus pooling for the remaining discrete per-instance objects (decor, hazards,
  resource nodes, exits) that a Tilemap cannot represent as interactable, individually-stateful
  objects.

## Cost

- Tilemap conversion of floor/wall tiles: medium — must preserve exact tile positions/types
  produced by the existing deterministic generation (the Tilemap becomes a rendering target fed by
  the same seeded layout data, not a new source of layout truth) and must not break existing
  collision behavior (walls likely rely on collider presence per tile today).
- Pooling for decor/hazard/resource-node/exit materializers: medium — each materializer needs a
  pool-acquire/release lifecycle instead of Instantiate/Destroy, and release timing must align
  exactly with level-exit/re-entry to avoid either premature reuse (an object still "in play" on
  the current level) or pool growth without bound (never releasing).

## Risk

- Any change to `CaveTileMaterializer` or the other cave materializers **must be scoped and
  reviewed under the `cave-stable-run` rule and the `cave-stable-run-guard` skill** — this is
  explicitly the highest-risk category of cave change per that rule ("Qualquer mudança
  procedural/runtime da cave precisa preservar o stable-run contract"). Pooling/Tilemap conversion
  must be provably content-neutral (same seed → same visible layout/enemies/resources/depleted
  state) or it violates the FASE9F contract outright.
- Resource-node depleted-state persistence (already a documented concern elsewhere in the cave
  system) must be re-verified against pooled objects — a pooled GameObject reused for a different
  logical node must not carry over stale depleted/state flags from its previous use.

## Requer decisão humana

**SIM** — motivo: this touches `Cave/Runtime/**` materializers, which are explicitly gated by the
`cave-stable-run` rule as requiring careful review before any procedural/runtime change; any
implementation must be validated against the FASE9F stable-run contract (same seed → same layout,
enemies, resources, depleted state) before being considered safe, and that validation — plus the
decision to invest in this now versus leaving it as a known performance debt — should be a human
call, not an incidental side effect of unrelated cave work.

## Source Documents

- [object-pooling-pattern SKILL.md](../../.claude/skills/object-pooling-pattern/SKILL.md)
- [cave-stable-run rule](../../.claude/rules/cave-stable-run.md)
- `Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs` (confirmed per-tile `Instantiate`,
  no pool)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
