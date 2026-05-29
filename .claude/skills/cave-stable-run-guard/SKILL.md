---
name: cave-stable-run-guard
description: Guardrails for cave procedural/runtime edits under the FASE9F stable run rule
version: 1.0
---

# Cave Stable Run Guard

Use this skill before editing any cave procedural, materialization, runtime state, snapshot, spawn, resource node, exit, checkpoint, or save code.

## Mandatory Reading

Read these first:

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

## Core Rule

Within the same `CaveRunSeed`, a previously visited `CaveLevel` must keep:

- layout;
- entrance and exit;
- enemy composition;
- enemy count;
- enemy positions;
- enemy IDs/types;
- resource node composition;
- resource node count;
- resource node positions;
- depleted node state;
- boss/miniboss state when applicable.

Procedural content may change only on:

- new game;
- KO/death/defeat;
- explicit debug run regeneration.

`ForwardExit` and `BackExit` must not change `CaveRunSeed`.

## Implementation Checklist

- [ ] Do not reroll visited level content on re-entry.
- [ ] Keep seeds deterministic: `CaveWorldSeed + CaveRunSeed + CaveLevel + stable salt`.
- [ ] Use stable IDs for runtime content; do not generate random GUIDs for stable run content.
- [ ] Save only IDs/simple DTO data.
- [ ] Do not serialize `GameObject`, `Transform`, `MonoBehaviour`, `ScriptableObject`, `Sprite`, `Collider`, or `Rigidbody`.
- [ ] Materializer should materialize from generated/snapshot data, not silently invent gameplay content.
- [ ] If full snapshot support is out of scope, explicitly document that limitation.

## Red Flags

Stop and re-check scope if the change:

- changes `CaveRunSeed` on portal use;
- adds `Guid.NewGuid()` or timestamp to enemy/resource instance IDs;
- calls random without a deterministic seed;
- modifies cave save schema without migration;
- adds respawn/redistribution/boss completion when the spec excludes it;
- uses runtime scene search to wire cave systems.

## Closeout Notes

When closing a cave task, report:

- how run stability is preserved;
- whether first-visit vs revisit behavior changed;
- whether snapshot/save was changed;
- what remains for the next SPEC 14 slice.
