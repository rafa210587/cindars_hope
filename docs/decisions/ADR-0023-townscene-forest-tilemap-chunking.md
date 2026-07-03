---
doc_type: adr
status: proposed
adr_id: ADR-0023
title: TownScene External Forest — Migration to Tilemap/Chunked Rendering
date: 2026-07-03
source_documents:
  - .specs/a_implementar/spec_codex_08_convergence_decisions.md
  - .claude/rules/id-stability.md
supersedes: []
superseded_by: []
applies_to:
  - townscene
  - world-rendering
  - performance
---

# ADR-0023 — TownScene External Forest: Migration to Tilemap/Chunked Rendering

## Status

**proposed** (draft — requires human decision before any implementation)

## Context

Reconfirmed 2026-07-03:

- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` is **3331 lines** (confirmed
  via `Get-Content ... | Measure-Object -Line`), matching the prior audit's figure exactly.
- The file creates individual trees as separate GameObjects via
  `WorldSpriteLibrary.Tree(townTreeSpecies[speciesIndex])` (one sprite lookup + one GameObject per
  tree instance), which is the same per-instance materialization pattern used throughout the file
  for other scene decoration.
- The prior audit prompt cited ~2826 total GameObjects in the materialized `TownScene`, of which
  ~1153 are individual trees. This session did not re-open Unity to re-count materialized
  GameObjects directly (no batchmode session was run for this doc-only spec); the 1153 figure is
  **not independently re-verified in this session** and should be re-confirmed by opening the
  generated scene (or running a scene-object-count editor script) before this ADR is acted on.
- A separate historical effort, `spec_city_preservation_first_coherent_relayout`, already
  documented a before/after tree manifest citing **497 trees**. That number and the audit's ~1153
  do not obviously reconcile from documentation alone — they may refer to different scene passes
  (before vs. after a later densification; note the recent commit `feat(city): densificar floresta
  e ajustar telhados` in this repo's history, which is consistent with the count having grown
  after the 497 baseline was recorded). **Whoever implements this ADR must reconcile the two
  numbers by re-counting the current materialized scene directly**, not by picking whichever
  number is convenient.

Regardless of the exact current count, the qualitative problem is real and uncontested: hundreds
to low thousands of individual tree GameObjects, each with its own `Transform` + `SpriteRenderer`
+ (per the `sortingOrder` audit) likely a manually-set sorting order, is expensive to instantiate,
serialize into the scene file, and render, compared to a `Tilemap`-based or chunked-batch approach
purpose-built for exactly this kind of repeated foliage.

## Decision (proposed, not accepted)

Migrate the TownScene's external forest from **per-instance GameObjects** to a **Tilemap-based or
chunked-rendering** representation (e.g. a dedicated foliage `Tilemap` layer, or batched
`GraphicsBuffer`/combined-mesh chunks), while preserving:

1. **Tree identity/position stability** — per the `id-stability` rule, any tree that carries a
   stable ID today (if any do — e.g. for a quest/interaction hook) must keep that ID and world
   position through the migration; this ADR does not authorize silently renumbering or repositioning
   trees that other systems reference by ID.
2. **Visual parity** — species variety, Y-sort/occlusion behavior with the player and buildings,
   and collision (if trees currently block player movement) must be preserved or explicitly
   re-approved if changed.

### Alternatives considered

- **Do nothing (status quo):** simplest, but per-instance forest GameObject count keeps growing
  as the world is densified further (already trending up per the recent "densificar floresta"
  commit), and it is the single largest concrete GameObject-count contributor identified in the
  convergence audit.
- **Tilemap-only foliage layer:** matches the project's existing `tilemap-world-rendering` skill
  pattern (ground/rule-tiles already use Tilemap); reuses `com.unity.2d.tilemap`, already a
  project dependency (confirmed in `Packages/manifest.json`). Simpler mental model, but a
  strict grid-cell Tilemap may constrain organic-looking forest placement compared to today's
  free-position instantiation.
- **Chunked batching (combined mesh / GPU instancing) without Tilemap:** preserves free-position
  placement exactly, but is a custom rendering system with no existing precedent in this codebase
  — higher implementation cost, no reuse of `com.unity.2d.tilemap`.

## Cost

- Medium-high: requires (a) re-counting and exporting the current tree manifest (position +
  species + any stable ID) as the source of truth before any code change, per `id-stability`; (b)
  building the new Tilemap/chunk representation; (c) visual QA to confirm parity (Y-sort,
  collision, species distribution) with the current scene; (d) updating
  `CreateMvpTownScene.cs`'s forest-creation section (a meaningful excision from a 3331-line file
  that must not regress unrelated sections).

## Risk

- Losing tree position/ID stability would violate `id-stability` and could break any
  quest/interaction/save-state system that references a specific tree by position or ID (must
  audit for this before migrating, not assume there are none).
- Visual regression (Y-sort ordering, collision behavior, or species density looking different)
  is a real risk given the manual `sortingOrder` pattern already in wide use in this file (25
  files project-wide set `sortingOrder` manually, confirmed 2026-07-03; `CreateMvpTownScene.cs`
  is one of them).
- The 497 vs. ~1153 discrepancy must be resolved with a fresh count before implementation, or the
  "before/after" comparison this ADR's implementer produces will not be trustworthy evidence.

## Requer decisão humana

**SIM** — motivo: this touches a materialized scene with ~1153 (unverified this session) to
possibly more GameObjects, requires re-confirming a contested count (497 vs. ~1153) before any
code change, and risks silent regression to tree position/ID stability and to the Y-sort/collision
behavior of the busiest single scene-creation file in the codebase (3331 lines). It is also purely
a performance/architecture trade — not required for any current spec's functional acceptance —
so the human should decide priority and timing, not have it happen as a side effect of an
unrelated task.

## Source Documents

- [spec_codex_08_convergence_decisions.md](../../.specs/a_implementar/spec_codex_08_convergence_decisions.md)
- [id-stability rule](../../.claude/rules/id-stability.md)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
