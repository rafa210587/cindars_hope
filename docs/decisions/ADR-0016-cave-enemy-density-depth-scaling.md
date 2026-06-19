---
doc_type: adr
status: accepted
adr_id: ADR-0016
title: Cave Enemy Density and Depth Scaling
date: 2026-06-19
source_documents:
  - Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs
  - docs/decisions/ADR-0005-cave-stable-run-and-replay.md
  - .specs/a_implementar/fable/fable_67_spec_canonical_governance_input_map_adr.md
  - docs/validation/wave_gameplay_expansion_2026_06_12_execution_report.md
supersedes:
  - ADR-0005 (only the §Ranges "Enemies per level per run" value)
superseded_by: []
applies_to:
  - cave-procedural-generation
  - gameplay-balance
relates_to:
  - docs/game_rules/cave_rules.md
---

# ADR-0016 — Cave Enemy Density and Depth Scaling

## Status

**accepted** (2026-06-19)

## Context

ADR-0005 (§Ranges) and `docs/game_rules/cave_rules.md` (Rule: Enemy Count Range) both
state **"12-20 enemies per level"**. That was the original SPEC_24 planning value.

The shipping code diverged. `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs`
implements a denser, depth-scaling model (introduced/accepted in the
`GAMEPLAY_EXPANSION_SLICE`, BUILD_VALIDATED 2026-06-12 — cave density 16-32 with depth
scaling, hard cap 44). The constants are explicit (lines 12-17):

```csharp
private const int DefaultMaxEnemies = 32;
private const int MinEnemiesPerLevel = 16;
private const int MaxEnemiesPerLevel = 32;
private const int DepthScalingHardCap = 44;
```

and the count is resolved by `ResolveTargetEnemyCount` (lines 294-303):

```csharp
var depth = Math.Max(0, caveLevel);
var minBound = Math.Min(DepthScalingHardCap, MinEnemiesPerLevel + depth / 12);   // +1 to min every 12 levels
var maxBase  = Math.Max(MaxEnemiesPerLevel, configuredMaxEnemies);
var maxBound = Math.Min(DepthScalingHardCap, maxBase + depth / 8);               // +1 to max every 8 levels
var upperBound = Math.Max(minBound, maxBound);
var range = Math.Max(1, upperBound - minBound + 1);
return minBound + Math.Abs(levelSeed % range);                                   // deterministic per level seed
```

Per docs governance (#3: ADRs/game_rules are canonical; code vs. ADR/game_rule conflict
means one of them is stale; an already-accepted behavior change requires a *superseding*
decision, not a silent edit), the documentation — not the code — is stale. The 16/32/44
density was implemented and accepted during wave validation; the docs were never updated.

## Decision

**The canonical cave enemy density is the live `CaveEnemySpawnPlanner` model. This ADR
partially supersedes ADR-0005, replacing ONLY its "Enemies per level per run" range.
No code changes — this records reality.**

- **Base range:** **16 (min) to 32 (max)** enemies per cave level on first visit.
- **Depth scaling:** the min grows +1 every 12 cave levels; the max grows +1 every 8
  cave levels (`MinEnemiesPerLevel + depth/12`, `max(32, configured) + depth/8`).
- **Hard cap:** **44** enemies per level (`DepthScalingHardCap`), applied to both bounds,
  so deep floors stay dense but playable.
- **Determinism:** the count is chosen deterministically from the per-level stable seed
  (`levelSeed`), preserving the ADR-0005 stable-run invariant — a revisited level within
  the same `CaveRunSeed` yields the same count.
- **Scene-serialized `_maxEnemiesPerLevel`** from older scenes is treated as a **base**
  and still receives the depth bonus, so density rises without scene regeneration.
- **Everything else in ADR-0005 remains canonical** (stable-run invariant, snapshot
  persistence, exit actions never reroll the seed, resource node range 4-10, enemy-level
  distribution). This ADR touches the enemy *count* only.

## Consequences

### Positive

- The canonical docs now match shipping code; an agent reading ADR-0005/cave_rules will
  no longer "fix" the planner toward 12-20 and regress accepted density.
- The depth-scaling rationale (dense-but-capped deep floors) is recorded.

### Negative

- This legalizes a value the human may still wish to retune. The decision is **reversible
  by a future ADR**; the execution report flags the 12-20 → 16-32/44 delta for human review.

### Operational

- `CaveEnemySpawnPlanner` (unchanged) is the implementation of record.
- `docs/game_rules/cave_rules.md` (Rule: Enemy Count Range) is updated to 16-32 / cap 44
  / depth scaling, citing this ADR.
- `ADR-0005` gains a surgical superseding note in §Ranges (enemies line only).
- Validation: docs validation must remain exit 0; zero diff in `Assets/**`.

## Applies To

- Cave procedural generation enemy count (`CaveEnemySpawnPlanner.ResolveTargetEnemyCount`)
- Cave gameplay balance / encounter density

## NOT Applicable To

- ADR-0005 resource node range (4-10) — unchanged.
- ADR-0005 stable-run invariant, snapshot, seed rules — unchanged and canonical.
- Any runtime code (this ADR is documentation-only).

## Source Documents

- [`CaveEnemySpawnPlanner.cs`](../../Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs) — constants 16/32/44, `ResolveTargetEnemyCount`
- [ADR-0005: Cave Stable Run and Replay](./ADR-0005-cave-stable-run-and-replay.md) — partially superseded (enemy range only)
- [cave_rules.md](../game_rules/cave_rules.md) — updated by this ADR
- `docs/validation/wave_gameplay_expansion_2026_06_12_execution_report.md` — where 16-32/44 was implemented and BUILD_VALIDATED

## Migration Notes

- ADR-0005 §Ranges enemies line annotated "superseded by ADR-0016".
- cave_rules.md "Enemy Count Range" rule rewritten to 16-32 / cap 44 / depth scaling.
- No amendment retired; no document deleted.

---

*Created: 2026-06-19 (fable_67)*
*Status: accepted*
*Partially supersedes: ADR-0005 (enemy count range only)*
