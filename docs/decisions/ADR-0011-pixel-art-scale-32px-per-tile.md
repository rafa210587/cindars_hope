---
doc_type: adr
status: accepted
adr_id: ADR-0011
title: Pixel Art Scale — 32 px per Tile
date: 2026-06-13
source_documents:
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
supersedes: []
superseded_by: []
applies_to:
  - art-pipeline
  - sprite-authoring
  - world-scale
  - creature-size-classes
---

# ADR-0011 — Pixel Art Scale: 32 px per Tile

## Status

**accepted** (canonical art-scale decision; binds the art pipeline before any final sprite)

## Context

Cindar's Hope is a 2D pixel-art RPG + farm sim. Until now the art scale was implicit: tiles,
creatures, props, and the player had no single canonical pixels-per-unit / pixels-per-tile
figure. Without a fixed scale, sprites produced in different sessions (or by different artists)
drift in resolution, world tiles do not align, and creature size classes (Small / Medium / Large
/ Huge, used by the bestiary and cave layout) cannot be sized consistently against the tile grid.

The owner answered decision **4.3** of `FABLE_DECISOES_RESPOSTAS_v3.0.md` (VINCULANTE,
2026-06-13) by fixing the art scale and requesting an ADR **before any final sprite is produced**.

## Decision

**The canonical art scale is 32 pixels per tile.**

- 1 world tile = **32 × 32 px**.
- A **Medium** creature/object occupying **1×1 tile** = **32 × 32 px**.
- A **Huge** creature/object occupying **3×3 tiles** = **96 × 96 px**.
- Other size classes scale linearly off the same 32 px/tile base (e.g. a 2×2 footprint = 64 × 64 px).

This figure is the single source of truth for the import scale (pixels-per-unit), the tilemap cell
size, and every creature/prop sprite's authored resolution. Any size class declared in the
bestiary or cave layout maps to a tile footprint, and the tile footprint maps to pixels via the
32 px/tile rule.

### Why 32 px/tile

- It is a power-of-two-friendly base that keeps tile alignment exact and avoids sub-pixel seams.
- It is large enough to carry recognizable pixel detail for a farm-sim/RPG, yet small enough to
  keep memory and authoring cost reasonable for a single-developer art pass.
- It gives a clean integer mapping from the existing tile-based footprints (Small/Medium/Large/Huge)
  to pixel dimensions.

## Scope

- **In scope:** the authoring/import resolution contract for tiles, creatures, props, the player,
  and UI art that must align to the world grid; the tile-footprint → pixel mapping for size classes.
- **Out of scope:** the visual style, palette, or theme of the art (separate art-direction work);
  HUD/canvas layout, which uses percentage anchors via CanvasScaler (see F14 amendments), not the
  world tile grid; final sprite production scheduling (the art phase).

## Implementation

- Any spec, generator, or asset that authors final sprites must size them to the 32 px/tile rule.
- Tilemap cell size and sprite import "pixels per unit" must be configured to 32 so that 1 tile =
  1 world unit (or the chosen unit convention) with no fractional scaling.
- Creature/prop size classes referenced by the bestiary and cave layout resolve to tile footprints,
  then to pixels at 32 px/tile (Medium 1×1 = 32px, Huge 3×3 = 96px).
- This ADR is the gate: do not commit final sprites authored at an ad-hoc resolution.

## Consequences

- All future sprite work shares one resolution; no per-asset scale negotiation.
- Size classes have an unambiguous pixel footprint, so bestiary/cave content is visually consistent.
- Placeholder/early art produced before this ADR that does not match 32 px/tile is technical debt
  to be re-exported during the art phase; this is acceptable because no final sprites existed yet.
- Changing the scale later would invalidate all authored sprites; that would require a superseding ADR.

## Applies To

- The art pipeline and all sprite-authoring specs
- Tilemap configuration and world-scale decisions
- Creature/object size-class definitions in the bestiary and cave layout

## Source Documents

- [FABLE_DECISOES_RESPOSTAS_v3.0.md](../design/FABLE_DECISOES_RESPOSTAS_v3.0.md) — decision 4.3 (VINCULANTE)

---

*Created: 2026-06-13*
*Status: accepted*
*Source decision: 4.3 (FABLE Decisões v3.0)*
