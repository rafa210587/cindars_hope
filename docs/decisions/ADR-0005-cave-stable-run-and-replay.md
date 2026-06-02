---
doc_type: adr
status: accepted
adr_id: ADR-0005
title: Cave Stable Run and Replay
date: 2026-06-01
source_documents:
  - docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md
  - SPEC_24
supersedes: []
superseded_by: []
applies_to:
  - cave-procedural-generation
  - gameplay-save-load
---

# ADR-0005 — Cave Stable Run and Replay

## Status

**accepted** (approved for SPEC_24 implementation)

## Context

Cave generation is complex: layout, enemies, resources, positions, states. Players revisit cave levels within the same run. Question: should revisits rerroll level content, or preserve it?

## Decision

**Caves are procedural by run, not by entry.**

Within the same `CaveRunSeed`, a `CaveLevel` visited before must preserve:

- Layout/structure
- Entrance and exit
- Enemy composition, count, positions, IDs, types
- Resource node composition, count, positions, IDs, types
- Depletion state of nodes
- Boss/miniboss state

Procedural changes only on:
1. New game
2. KO/death/defeat
3. Explicit debug regeneration command

`ForwardExit` and `BackExit` never change `CaveRunSeed`.

### Ranges

- Enemies per level per run: 12-20 (random on first visit, fixed on revisits)
- Resource nodes per level per run: 4-10 (random on first visit, fixed on revisits)

## Implementation

- First visit: generate layout, enemies, resources; create snapshot; save snapshot
- Revisit: load existing snapshot; no regeneration; materialize from snapshot

## Consequences

- Player revisits are deterministic and recoverable
- Save/load works correctly
- Snapshot mechanism is required
- LayoutHash and ContentHash tracking needed

## Applies To

- Cave procedural generation (CaveLevel, CaveRunSeed)
- Save/load system (snapshot persistence)
- Gameplay testing (revisit behavior)

## Source Documents

- [FASE9F Amendment](./../amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md) (contains mojibake; corrected content in ADR)
- [SPEC_24: Cave Runtime Closeout](./../specs/a_implementar/closeout_mvp/SPEC_24_CAVE_RUNTIME_CHECKPOINTS_BOSS_GATES_CLOSEOUT.md)

---

*Created: 2026-06-01*  
*Status: accepted*  
*Implemented by: SPEC_24*
