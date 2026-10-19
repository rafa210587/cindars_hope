---
doc_type: adr
status: accepted
adr_id: ADR-0019
title: Cave Biome Mineable Budget Supersedes Fixed 4-10 Resource Node Range
date: 2026-06-23
source_documents:
  - .specs/a_implementar/fable/fable_78_spec_cave_ecosystem_population_runtime.md
  - docs/decisions/ADR-0005-cave-stable-run-and-replay.md
  - docs/game_rules/cave_rules.md
supersedes:
  - ADR-0005 (only the §"Resource Node Range per Level per Run" value: 4-10)
superseded_by: []
applies_to:
  - cave-procedural-generation
  - gameplay-balance
relates_to:
  - docs/game_rules/cave_rules.md
---

# ADR-0019 — Cave Biome Mineable Budget Supersedes Fixed 4-10 Resource Node Range

## Status

**accepted** (2026-06-23)

## Context

`docs/game_rules/cave_rules.md` (Rule: Resource Node Range per Level per Run) states a fixed
**"4-10 resource nodes per level"**, sourced from ADR-0005 / FASE9F / SPEC_24.

`fable_78` ("Caverna Viva") makes ore the **thematic, per-biome** source of mining material:
each band has its own profile of environmental elements (stone + ore in every band, plus
biome-specific mixes), and the mineable density scales with depth. A fixed 4-10 count
contradicts a per-biome budget where deeper/richer bands deliberately carry more (and rarer)
veins to feed gear crafting (`fable_49`).

Critically, this is **not a parallel mineable pool**. The new mineable elements are the
**expansion of the existing resource-node budget** — they reuse `ResourceNode` +
`ResourceNodeDatabaseSO` + `CaveLootSnapshotService` (depletion idempotency via
`MiningNode`/`RareVein`), as required by the spec's non-duplication rules (§13). What changes
is only the **count band** and that the count is **themed per biome**, not a flat 4-10.

Per `docs-governance` (#3), a behavior change to a canonical game rule requires a
*superseding* ADR before implementation. `fable_78` §14.10 (G2) calls for exactly this.

## Decision

**The canonical cave mineable source is a per-biome, per-band resource-node budget that
supersedes the fixed 4-10 range. The new range is defined per band (numbers are tunable; the
final signed values are `fable_59` tuning). Mineables remain the existing resource-node
system — not a parallel pool.**

- **Per-band budget replaces flat 4-10:** the number of resource/mineable nodes per level is
  a deterministic range **per biome band** (Stone, Fungal, Ice, Fire, Ruins, Deep, Void),
  not a single 4-10 range for all levels.
- **Intent of the new range:** every band still guarantees a minimum mineable presence
  (stone + at least the band's base ore tier), and richer/deeper bands carry a higher ceiling
  and rarer veins (e.g. mithril/arcane in Deep/Void). The **per-band min/max numbers are
  authored in data** (`CaveEnvironmentElementProfileSO` element density + the balance SO),
  and the **final signed values are deferred to `fable_59`** balance tuning. This ADR fixes
  the *contract* (per-biome budget supersedes 4-10), not the final numbers.
- **Not a parallel system:** mineable elements reuse `ResourceNode`,
  `ResourceNodeDatabaseSO`, and `CaveLootSnapshotService` (`MiningNode`/`RareVein`). They are
  the thematic expansion of the resource-node budget, sharing the same depletion-idempotency
  and stable-run snapshot contract.
- **Determinism unchanged (ADR-0005):** counts and positions are derived from
  `(CaveWorldSeed, CaveRunSeed, CaveLevel)` via `CaveLayoutStableHash` (FNV-1a). A revisited
  level yields the same nodes; depletion state persists.

## Consequences

### Positive

- Ore has a real, depth-themed source, unblocking gear crafting economy (`fable_49`).
- Docs no longer pin a flat 4-10 that the biome-themed design must violate.
- Stable-run determinism and depletion idempotency are preserved (same systems).

### Negative

- Legalizes a wider, per-band range the human may still retune; reversible by a future ADR.
  Final numbers are intentionally deferred to `fable_59`.

### Operational

- `docs/game_rules/cave_rules.md` (Rule: Resource Node Range) updated to a per-biome budget
  superseding 4-10, citing this ADR.
- The per-band density lives in `CaveEnvironmentElementProfileSO` / `CaveEcosystemBalanceSO`
  (no magic values in code).
- Validation: docs validation exit 0; resource depletion replay unchanged.

## Applies To

- Cave resource/mineable node count and per-biome distribution (`fable_78`)
- Cave gameplay balance / mining economy

## NOT Applicable To

- ADR-0005 stable-run invariant, snapshot, seed rules — unchanged and canonical.
- Depletion idempotency contract (`CaveLootSnapshotService`) — unchanged (reused).
- Any second mineable/loot system — explicitly forbidden by the spec (§13).

## Source Documents

- [`fable_78` spec](../../.specs/a_implementar/fable/fable_78_spec_cave_ecosystem_population_runtime.md) — §14.2, §14.10 (G2), §16.1
- [ADR-0005: Cave Stable Run and Replay](./ADR-0005-cave-stable-run-and-replay.md) — partially superseded (resource node range only)
- [cave_rules.md](../game_rules/cave_rules.md) — updated by this ADR

## Migration Notes

- `cave_rules.md` "Resource Node Range" rule rewritten to a per-biome budget superseding the
  flat 4-10, citing this ADR; final numbers deferred to `fable_59`.
- No amendment retired; no document deleted.

---

*Created: 2026-06-23 (fable_78 SLICE 1)*
*Status: accepted*
*Partially supersedes: ADR-0005 (resource node range 4-10 only)*
