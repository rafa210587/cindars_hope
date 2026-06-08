# Rule: Spec Dependency Resolution

## Central Rule

When a spec depends on another **unresolved spec in the same wave**, resolve the dependency chain **automatically** without asking the user.

**Do not ask the user.**  
**Do not pivot randomly.**  
**Do not mark definitive BLOCKED unless dependency is forbidden/out-of-scope.**

Automatic resolution prevents context loss, reduces user friction, and ensures deterministic execution order based on dependencies, not random pivots.

---

## Dependency Status Taxonomy

Allowed dependency statuses when resolving chains:

| Status | Meaning | Can Continue? |
|--------|---------|---|
| `READY` | Not yet executed | Yes, resolve first |
| `BUILD_VALIDATED` | Completed successfully | Yes, use result |
| `BUILD_VALIDATED_WITH_WARNINGS` | Completed with integration deferred | Yes, use result |
| `CONTRACT_ONLY` | Contract/model created, no integration | Yes, but next spec may need integration |
| `CONTRACT_ONLY_NEEDS_INTEGRATION` | Contract created, integration deferred | Conditional (foundational specs must wait) |
| `BLOCKED_BY_DEPENDENCY_PENDING` | Blocked waiting for its own dependency | Resolve recursively |
| `BLOCKED_BY_FORBIDDEN_SCOPE` | Blocked (future/pets/HOLD) | No, stop |
| `BLOCKED_BY_FUTURE_SCOPE` | Depends on future wave spec | No, stop |
| `BLOCKED_BY_PETS_SCOPE` | Depends on pets spec | No, stop |
| `BLOCKED_BY_WAVE_ORDER` | Depends on earlier wave not done | No, stop |
| `BLOCKED` | Other blocker (doc, missing system, etc.) | Maybe (evaluate) |

---

## Same-Wave Dependency Resolution

When current spec finds a dependency on another spec in **the same wave** that is still `READY`:

1. Mark current spec as `BLOCKED_BY_DEPENDENCY_PENDING` (temporary).
2. Add dependency to the **dependency stack**.
3. Identify the **deepest/root dependency** (the one nothing else depends on).
4. Execute the root dependency first using `/execute-spec-strict`.
5. Return to the blocked spec after the dependency passes.
6. `BLOCKED_BY_DEPENDENCY_PENDING` **does not count as a final failure**.

### Dependency Stack Example

```
Current spec: companion_farm_job_board (READY, depends on farm_animals)
  ↓
farm_animals (READY, depends on farm_buildings)
  ↓
farm_buildings (READY, depends on farm_building_footprints)
  ↓
farm_building_footprints (READY, depends on farm_level1_layout)
  ↓
farm_level1_layout (READY, depends on farm_scale_tilemap)
  ↓
farm_scale_tilemap (READY, no dependencies)  ← ROOT
```

**Execution order:** `farm_scale_tilemap` → `farm_level1_layout` → `farm_building_footprints` → `farm_buildings` → `farm_animals` → `companion_farm_job_board`.

---

## Forbidden Dependencies (Stop Immediately)

Stop resolution and mark the current spec as `BLOCKED_BY_FORBIDDEN_SCOPE` if dependency is:

- `future` or `mapped` (future wave spec)
- `pets` (pets scope)
- `HOLD` (held spec)
- `BLOCKED_SCOPE` (explicitly out of scope)
- **WAVE 06+** (future wave)
- Requires **Packages/** or **ProjectSettings/** changes
- Requires **scene/prefab/asset** creation or editing
- Requires **Unity Test Runner** or **Play Mode** (out of scope for current phase)

Document the forbidden dependency reason in the execution report.

---

## Dependency Resolution Artifacts

Maintain these files to track resolution state:

### 1. Dependency Resolution Plan

File: `docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md`

```markdown
| Order | Spec | Depends On | Reason | Status | Commit |
|-------|------|-----------|--------|--------|--------|
| 1 | farm_scale_tilemap | (none) | root | BUILD_VALIDATED | abc1234 |
| 2 | farm_level1_layout | farm_scale_tilemap | layout needs scale | BUILD_VALIDATED | def5678 |
...
```

### 2. Batch State File

File: `docs/validation/WAVE_<wave>_BATCH_STATE.md`

```markdown
# WAVE 05 Batch State

## Current Dependency Stack
| Order | Spec | Status |
|-------|------|--------|

## Executed This Batch
| Spec | Status | Commit |

## Pending Specs
| Spec | Blocked By | Plan |

## Next Action
[resolve dependency | execute original | loop continue | STOPPED]
```

Both files persist state across agent invocations.

---

## Return-To-Origin Rule

After the entire dependency chain is **successfully resolved**, return to the **original target spec** before selecting any unrelated specs.

Example:
```
Original target: companion_farm_job_board
Resolved chain: farm_scale_tilemap → farm_level1_layout → farm_building_footprints → farm_buildings → farm_animals
Next action: Execute companion_farm_job_board (original target)
Don't pivot to: other_farm_spec_not_in_chain
```

This ensures the user's original intent is fulfilled, not overridden by automatic resolution.

---

## No Random Pivot Rule

**Do NOT pivot to unrelated specs while a dependency chain is unresolved.**

Bad:
```
Original: companion_farm_job_board (depends on farm_animals)
Current: farm_animals (READY)
Wrong action: Execute companion_eligibility instead
Correct action: Execute farm_animals, then return to companion_farm_job_board
```

Maintain the dependency stack in `BATCH_STATE.md` to enforce this.

---

## Dependency Extraction

When reading a spec, extract dependencies from:

1. `Depends on:` section
2. `Dependencies:` section
3. `Required systems:` section
4. `Required specs:` section
5. `Acceptance criteria:` mentioning systems/specs
6. `Blocks:` section (reverse dependency)
7. `Permissions:` section (required assets, scenes)

---

## Recursive Dependency Handling

If a dependency itself has dependencies, resolve **recursively**:

```
A depends on B
B depends on C
C depends on D

Resolve: D → C → B → A (depth-first)
```

---

## Validation

Before continuing after dependency resolution:

```text
✓ All dependencies in chain resolved with BUILD_VALIDATED or better
✓ Forbidden dependencies identified and stopped early
✓ Dependency plan file exists and updated
✓ Batch state file exists and updated
✓ No random pivots while chain was open
✓ Original target will execute next (or is already in queue)
```

---

## Required Clause in Execution Reports

Every execution report that involved dependency resolution must include:

```text
## Dependency Chain

Original target: <spec>
Dependency chain:
  1. <root spec> → [status]
  2. <next spec> → [status]
  ...
  N. <original spec> → [status]

Forbidden dependencies: [none | listed]
Resolved depth: <N>
Plan file: docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md
Batch state: docs/validation/WAVE_<wave>_BATCH_STATE.md
Can continue original target: YES/NO
```

---

*Created: 2026-06-08 (Spec Dependency Resolution Harness)*  
*Applies to all `/loop-spec-batch-strict` and `/execute-spec-strict` tasks in same-wave execution.*
