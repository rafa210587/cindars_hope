# Rule: Spec Dependency Resolution

## Central Rule

When a spec depends on another **unresolved spec in the same wave**, resolve the chain **automatically**: do not ask the user, do not pivot randomly, do not mark final BLOCKED unless the dependency is forbidden/out-of-scope.

## Algorithm

1. Mark current spec `BLOCKED_BY_DEPENDENCY_PENDING` (temporary — never a final failure).
2. Push the dependency onto the dependency stack; recurse until the root (depth-first).
3. Execute the root first via `/execute-spec-strict`, then walk back up the chain.
4. **Return-to-origin:** after the chain resolves, execute the ORIGINAL target spec before anything else. Never pivot to unrelated specs while a chain is open.

## Stop immediately (`BLOCKED_BY_FORBIDDEN_SCOPE`) if the dependency is

future/mapped wave (WAVE 06+), pets, HOLD, BLOCKED_SCOPE, or requires: Packages/, ProjectSettings/, scene/prefab/asset creation or editing, Unity Test Runner, Play Mode.

## Dependency extraction

Read these spec sections: `Depends on`, `Dependencies`, `Required systems`, `Required specs`, `Blocks`, `Permissions`, and acceptance criteria that name systems/specs.

Dependency statuses that allow continuing: `READY` (resolve first), `BUILD_VALIDATED[_WITH_WARNINGS]` (use result), `CONTRACT_ONLY[_NEEDS_INTEGRATION]` (conditional — foundational specs must wait). Full taxonomy: `.claude/rules/spec_quality_gate.md`.

## Mandatory artifacts (persist state across invocations)

- `docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md` — table: Order | Spec | Depends On | Reason | Status | Commit
- `docs/validation/WAVE_<wave>_BATCH_STATE.md` — current stack, executed this batch, pending specs, next action

## Required clause in execution reports

```text
## Dependency Chain
Original target: <spec>
Dependency chain: 1..N with statuses
Forbidden dependencies: [none | listed]
Resolved depth: <N>
Plan file / Batch state: paths above
Can continue original target: YES/NO
```

---

*Updated: 2026-06-12 (condensed — full prior text in git history). Applies to /loop-spec-batch-strict and /execute-spec-strict.*
