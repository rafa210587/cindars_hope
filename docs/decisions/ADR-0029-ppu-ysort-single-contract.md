---
doc_type: adr
status: proposed
adr_id: ADR-0029
title: Single PPU / Y-Sort Contract (Amends ADR-0011)
date: 2026-07-03
source_documents:
  - docs/decisions/ADR-0011-pixel-art-scale-32px-per-tile.md
supersedes: []
superseded_by: []
applies_to:
  - art-pipeline
  - sprite-authoring
  - world-scale
  - y-sort
---

# ADR-0029 — Single PPU / Y-Sort Contract (Amends ADR-0011)

## Status

**proposed** (draft — requires human decision before any implementation; proposes amending
[ADR-0011](./ADR-0011-pixel-art-scale-32px-per-tile.md), which remains **accepted** and in force
until superseded)

## Context

[ADR-0011](./ADR-0011-pixel-art-scale-32px-per-tile.md) (accepted 2026-06-13) fixed the canonical
art scale at **32 pixels per tile** as "the single source of truth for the import scale
(pixels-per-unit)... any size class... maps to pixels via the 32 px/tile rule," explicitly gating
"do not commit final sprites authored at an ad-hoc resolution."

Reconfirmed 2026-07-03 by direct code inspection, the actual pixels-per-unit values in use today
do **not** follow 32:

- `Assets/_Game/Scripts/Editor/GeneratedSpriteImporter.cs` line 18:
  `private const float Ppu = 128f;` (default PPU for generated sprites), with a second constant at
  line 23, `private const float HousesModularPpu = 64f;`, applied when the asset path starts with
  the modular-houses root (line 44:
  `s.spritePixelsPerUnit = p.StartsWith(HousesModularRoot) ? HousesModularPpu : Ppu;`). The file's
  own comment at line 21 states: *"Casas modulares... geradas a 64 px/tile, nao 128 — ADR-0011 sera
  emendada 32->64 em tarefa separada."* — i.e. the codebase already anticipated this exact amendment
  and left a pointer to it, but the amendment was never written until now.
- `Assets/_Game/Scripts/Camera/CameraFollow2D.cs` line 19:
  `[SerializeField] private float _pixelsPerUnit = 128f;` (camera pixel-snap reference value).
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` line 3259 and
  `Assets/_Game/Scripts/Editor/NPC/AssignNpcBodySprites.cs` line 126 and
  `Assets/_Game/Scripts/Editor/NPC/GenerateNpcWalkAnimations.cs` line 195:
  `importer.spritePixelsPerUnit = 234f;` (NPC body/walk sprites).
- `Assets/_Game/Scripts/Editor/NPC/AssignNpcPortraitSprites.cs` line 96:
  `importer.spritePixelsPerUnit = 100f;` (NPC portrait sprites — but portraits are UI-facing
  bust art, not world-grid content, so this one is arguably out of ADR-0011's stated scope, which
  explicitly excludes "HUD/canvas layout"; still listed here for completeness).

So the confirmed set of PPU values actually in force is **128 (default/creatures/camera), 64
(modular houses), 234 (NPC body/walk sprites), and 100 (portraits, likely out of scope)** — none of
which is 32. Separately, **25 files** set `sortingOrder` manually (confirmed via
`Select-String -Pattern "sortingOrder\s*="`), with no single documented Y-sort contract governing
when a manual override is appropriate versus relying on the default transparency-sort mode.

## Decision (proposed, not accepted)

Two related but separable amendments:

1. **PPU: formally amend ADR-0011 from 32 → 64 px/tile**, matching what the codebase's own
   `GeneratedSpriteImporter.cs` comment already anticipated, and matching the modular-houses value
   already shipping. The default-128/NPC-234 values would then need their own explicit
   reconciliation: either (a) treat 64 as the tile/world-grid PPU and allow character/creature art
   to use a different, explicitly-documented PPU (since a creature is not a tile and may
   legitimately need higher pixel density for detail), or (b) require every world-grid-aligned
   asset to converge on 64 and treat 128/234 as debt to re-export. This ADR does not decide between
   (a) and (b) — that choice is exactly the "requer decisão humana" below.
2. **Y-sort: define one explicit contract** for when `sortingOrder` may be set manually
   (e.g. static scenery with a fixed, known draw order) versus when a system should rely on
   Unity's default sprite-transparency-sort mode driven by Y position (e.g. player/NPCs/movable
   objects that need dynamic depth as they move through the scene). Today, 25 files set
   `sortingOrder` manually with no documented rule distinguishing the two cases, which risks
   inconsistent depth behavior as more content is added (directly relevant to ADR-0023's proposed
   TownScene forest migration, which depends on correct Y-sort/occlusion behavior).

### Alternatives considered

- **Do nothing (status quo):** ADR-0011 stays technically "accepted" while being contradicted by
  every actually-shipping PPU value in the codebase — this is itself a governance problem (an
  accepted ADR that does not describe reality misleads future readers who trust it, per
  `docs-governance`'s spirit even though ADRs are not explicitly covered by that rule's file-path
  restrictions).
  Doing nothing also leaves the sortingOrder inconsistency undocumented, so the risk continues to
  compound with every new scene-decoration system (e.g. ADR-0023's forest migration).
- **Amend PPU only, defer Y-sort:** addresses the more clearly urgent gap (ADR-0011 is
  demonstrably false today) without also solving Y-sort in the same change; keeps this ADR
  narrower. This is a legitimate scope-split the human may prefer.
- **Amend both together (this proposal):** both problems are about the same underlying gap — "the
  world-rendering contract that ADR-0011 was supposed to lock down was never fully implemented or
  enforced" — and the Y-sort question directly affects how any PPU reconciliation should treat
  layered/occluding content (trees, houses, characters), so addressing them together avoids solving
  PPU in a way that later turns out to conflict with whatever Y-sort contract gets chosen.

## Cost

- PPU reconciliation: medium-high — depends entirely on which option (a) vs (b) above is chosen;
  option (a) (creatures/world-grid diverge intentionally) is near-zero migration cost (just
  documentation); option (b) (converge everything to one PPU) would require re-exporting/reimporting
  every sprite currently at 128/234, a large art-pipeline pass.
- Y-sort contract: low — mostly a documentation/convention task (when to override vs. rely on
  default), with possibly a handful of files needing correction if audit finds actual
  inconsistency bugs, not just undocumented-but-correct manual overrides.

## Risk

- If option (b) (converge to one PPU) is chosen, re-exporting hundreds of already-shipped sprites
  (enemies, NPCs, houses) is exactly the scenario ADR-0011 itself flagged as "technical debt to be
  re-exported during the art phase" — i.e. the original ADR anticipated this could happen, but
  doing it now (mid-development, with a large existing asset library) is costlier than doing it
  before first art existed.
- An undocumented Y-sort contract risks silent depth-ordering bugs as new scene content
  (particularly ADR-0023's proposed Tilemap-based forest) is added without a clear rule for how it
  should interact with existing manually-ordered sprites.

## Requer decisão humana

**SIM** — motivo: (1) this formally amends an already-accepted ADR (ADR-0011) that the codebase
already silently contradicts, and superseding/amending an accepted architecture decision requires
the same sign-off the original did; (2) choosing option (a) vs (b) for PPU reconciliation has a
large cost delta (documentation-only vs. re-exporting the art library) that only the human can
weigh against the project's art-phase timeline; (3) any change to the Y-sort contract risks
regressing currently-correct manual sortingOrder values across 25 files if applied mechanically
without review.

## Source Documents

- [ADR-0011-pixel-art-scale-32px-per-tile.md](./ADR-0011-pixel-art-scale-32px-per-tile.md)
  (the ADR this proposes to amend)
- `Assets/_Game/Scripts/Editor/GeneratedSpriteImporter.cs` (confirmed 64px modular-houses PPU +
  the comment anticipating this exact amendment)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
