# Rule: Cave Stable Run

Any cave procedural/runtime change must preserve the FASE9F stable-run contract.

## Mandatory Reading Before Cave Changes

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

## Invariant

Within the same `CaveRunSeed`, a revisited `CaveLevel` must not reroll:

- layout;
- entrance/exit;
- enemy composition/count/positions/IDs;
- resource node composition/count/positions/IDs;
- depleted resource state;
- boss/miniboss state.

## Seed Changes Allowed Only On

- new game;
- KO/death/defeat;
- explicit debug run regeneration command.

`ForwardExit` and `BackExit` must never change `CaveRunSeed`.

## Implementation Requirements

- Use deterministic seeds based on `CaveWorldSeed + CaveRunSeed + CaveLevel + stable salt`.
- Do not use random GUIDs or timestamps for stable runtime content IDs.
- If full snapshot persistence is out of scope, document that limitation and keep the next SPEC 14 slice clear.
