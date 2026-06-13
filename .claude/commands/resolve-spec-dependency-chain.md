# /resolve-spec-dependency-chain

> **NOTA DE RECONCILIAÇÃO (2026-06-12):** a fila wave-based foi executada e movida para executadas_build_validated/. A fila ativa é .specs/a_implementar/fable/. Exemplos com paths NN_spec_* abaixo são históricos.


Reference command for resolving same-wave spec dependencies.

## Purpose

Extract and resolve a spec's dependency chain before proceeding with its execution.

This is called **automatically** by `/execute-spec-strict` and `/loop-spec-batch-strict` when a same-wave dependency is found. It is **not** a user-facing command, but a reference for understanding the resolution flow.

---

## Manual Usage

```text
/resolve-spec-dependency-chain .specs/a_implementar/05_spec_companion_farm_job_board_automation_runtime_execution.md
```

---

## Automatic Flow (Inside `/execute-spec-strict` or `/loop`)

When `/execute-spec-strict <spec>` finds a same-wave dependency:

1. **Mark current spec** as `BLOCKED_BY_DEPENDENCY_PENDING`.
2. **Extract dependencies** from spec:
   - Read spec fully
   - Search for: `Depends on`, `Dependencies`, `Required systems`, `Required specs`, acceptance criteria
3. **Search same-wave specs**:
   - Query `.specs/a_implementar/<wave>_spec_*.md` files
   - Match extracted names to file paths
4. **Build dependency chain**:
   - Create DAG (directed acyclic graph) of dependencies
   - Identify root (spec with no dependencies)
5. **Execute root first**:
   - Call `/execute-spec-strict <root_spec_path>`
   - Wait for result
6. **Propagate upward**:
   - After root completes, execute next spec in chain
   - Continue until original spec is reached
7. **Return to original**:
   - After chain is resolved, execute original target spec
   - Don't pivot to unrelated specs

---

## Dependency Chain Output

Every dependency resolution must produce:

```text
DEPENDENCY_RESOLUTION_RESULT
═════════════════════════════════════════

Original spec:           <spec_id> (path)
Depends on (direct):     <list>
Recursive dependencies:  <full chain>
Root dependency:         <spec_id>
Forbidden dependencies:  YES/NO (<list if YES>)
Next spec to execute:    <path>
Depth:                   <N specs>
Plan file:               docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md
Batch state:             docs/validation/WAVE_<wave>_BATCH_STATE.md
Can continue:            YES/NO
Stop reason:             [none | forbidden scope | future wave | pets | blocked]
```

---

## Required Files Updated

1. **`docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md`**
   - Add row for each resolved spec
   - Update `Status` and `Commit` after execution

2. **`docs/validation/WAVE_<wave>_BATCH_STATE.md`**
   - Update `Current Dependency Stack`
   - Move specs to `Executed This Batch` after completion
   - Update `Next Action`

---

## Forbidden Scope Examples

Stop resolution if dependency is:

```text
future_spec_companion_deep_romance_romance_system_design (WAVE 06+)
spec_cave_stable_run_codex_regen_replay (future/mapped)
spec_pets_companion_taming_system (pets scope)
```

---

## Same-Wave Resolution Example

**Scenario:** User executes `companion_farm_job_board`.

```
1. Read spec → found: "Depends on: farm_animals"
2. Search .specs/a_implementar/05_spec_*.md → find farm_animals spec
3. Check farm_animals → found: "Depends on: farm_buildings"
4. Check farm_buildings → found: "Depends on: farm_building_footprints"
5. Check farm_building_footprints → found: "Depends on: farm_level1_layout"
6. Check farm_level1_layout → found: "Depends on: farm_scale_tilemap"
7. Check farm_scale_tilemap → no dependencies (ROOT)

Execution order:
  1. farm_scale_tilemap (root)
     → returns BUILD_VALIDATED + commit abc1234
  2. farm_level1_layout (depends on root result)
     → returns BUILD_VALIDATED + commit def5678
  ... continue upward ...
  N. companion_farm_job_board (original target)
     → returns BUILD_VALIDATED + commit xyz9999

Return to original: YES ✓
Pivot to unrelated: NO ✓
```

---

## Policy: Never Implicit Multi-Wave Jumps

If resolving dependencies would require executing future waves:

```text
STOP immediately.
Status: BLOCKED_BY_FORBIDDEN_SCOPE (future wave).
Reason: companion_farm_job_board depends on structure_upgrades (WAVE 06).
Action: Return to user; cannot auto-resolve across waves.
```

---

## Policy: No Zombie Partial Chains

If resolution fails midway:

```text
Partially resolved: farm_scale_tilemap ✓, farm_level1_layout ✓
Blocked at: farm_buildings (BLOCKED_BY_FORBIDDEN_SCOPE: requires scene creation)
Status: Original spec becomes BLOCKED_BY_FORBIDDEN_SCOPE (not PENDING)
Dependency chain state: saved in BATCH_STATE.md for human review
Next action: User must unblock farm_buildings manually
```

---

*Created: 2026-06-08 (Reference Command)*  
*Not a user-facing command, but reference for understanding automatic resolution in `/execute-spec-strict`.*
