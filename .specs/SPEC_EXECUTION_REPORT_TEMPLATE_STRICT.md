# <Spec ID> — Execution Report (Strict)

> **Spec:** `<spec_id>`  
> **Wave:** WAVE XX  
> **Priority:** P0 / P1 / P2  
> **Type:** Runtime / UI / Data / Integration  
> **Executor:** Claude Code (Haiku 4.5)  
> **Date:** YYYY-MM-DD  
> **Status:** BUILD_VALIDATED / BUILD_VALIDATED_WITH_WARNINGS / CONTRACT_ONLY / CONTRACT_ONLY_NEEDS_INTEGRATION / DEFERRED_UI_VISUAL / NEEDS_REWORK / BLOCKED

---

## Acceptance Criteria Extracted

| # | Criterion | Required | Evidence | Status |
|---|---|---:|---|---|
| 1 | (spec says this) | YES/NO | (code shows this) | OK / DEFERRED / FAIL |
| 2 | | | | |

If status is FAIL or several DEFERRED, explain why status is not lower.

---

## Existing Systems Audit

| System | Found | Type | Reused | Notes |
|---|---:|---|---:|---|
| (system name) | YES/NO | (runtime/data/ui) | YES/NO | (how) |

Decision: REUSE_EXISTING / HARDEN_EXISTING / ADAPTER / CONTRACT_ONLY / DEFERRED / BLOCKED

---

## Scope Executed

### In Scope
- (item from spec scope)
- (item from spec scope)

### Out of Scope Respected
- (item from spec "out of scope" section) — confirmed not altered
- (item from spec "out of scope" section) — confirmed not altered

### Stop Conditions
- (any stop condition from spec?) — evaluation: triggered / not triggered

---

## Files Changed

| File | Change | Reason | Status |
|---|---|---|---|
| (path) | (created/modified) | (implements X requirement) | (OK / DEFERRED / NOT_NEEDED) |

### Forbidden Files Check
- Packages/ — ✓ NOT altered
- ProjectSettings/ — ✓ NOT altered
- Assets/**/*.unity — ✓ NOT altered
- Assets/**/*.prefab — ✓ NOT altered
- Assets/**/*.asset — ✓ NOT altered
- `.claude/*.lock` — ✓ NOT committed

---

## Tests / Validators

| Test/Validator | Location | Count | Status | Notes |
|---|---|---:|---|---|
| EditMode tests | `Assets/_Game/Tests/EditMode/**` | N | OK / MISSING / FAIL | (EditMode pytest or NUnit) |
| Integration validator | `Assets/_Game/Scripts/Editor/Validation/**` | N | OK / N/A | (editor validator) |
| PlayMode automated | (location) | N | DEFERRED / NOT_REQUIRED | (scheduled for which phase?) |
| Human scenario | `docs/validation/<spec_id>_human_test_scenario.md` | - | DEFERRED / NOT_REQUIRED | (when will it run?) |

**Test File Location Verification:**
- ✓ All `*Tests.cs` files are under `Assets/_Game/Tests/EditMode/**`, NOT under `Assets/_Game/Scripts/**`

---

## Spec Compliance Matrix

Core requirement mapping:

| Spec Requirement (exact quote or paraphrase) | Implementation Evidence (code snippet / file) | Status | Notes |
|---|---|---|---|
| (from spec section X) | (code location + excerpt) | OK / OK_WITH_WARNINGS / CONTRACT_ONLY / DEFERRED / FAIL / NOT_APPLICABLE | (why this status?) |
| | | | |

Allowed status:
- `OK` — Implemented fully
- `OK_WITH_WARNINGS` — Implemented, known limitations documented
- `CONTRACT_ONLY` — Contract/DTO created, integration deferred
- `DEFERRED` — Explicit defer documented in spec or here
- `FAIL` — Not implemented, defer not documented (problematic)
- `NOT_APPLICABLE` — Requirement not relevant to this context

---

## Validation

### Docs Validation
```
Command: .\tools\docs\validate_docs.ps1
Result: PASS / FAIL
Errors: (if any)
Warnings: (if any, expected or new)
```

### Assembly-CSharp Build
```
Command: dotnet build .\Assembly-CSharp.csproj --no-restore
Result: PASS / FAIL
Errors: (count of new errors)
Warnings: (count of new warnings on new code)
```

### Assembly-CSharp-Editor Build
```
Command: dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
Result: PASS / FAIL
Errors: (count of new errors)
Warnings: (count of new warnings on test code)
Test assembly: (compiles? Y/N)
```

### Quality Check
```
Command: .\tools\docs\check_spec_quality.ps1
Result: PASS / FAIL / EXPECTED_FAIL
Issues: (if any)
```

### EditMode Test Execution (if applicable)
```
Framework: NUnit / (other)
Test command: (dotnet test or Unity runner)
Status: (automated test runner not required; unit tests compile is sufficient)
```

### PlayMode / Human Validation
```
Status: NOT_RUN / DEFERRED / SCHEDULED
Why: (spec doesn't require, or phase deferred, or scheduled for final gate)
Scheduled phase: (if deferred, when?)
```

---

## Honest Status Rationale

Explain why the final status is not inflated.

Example rationale for each status:

**BUILD_VALIDATED:**
- All acceptance criteria are met (compliance matrix: all OK)
- Execution report complete
- Build/docs/quality passed
- Tests cover deterministic logic
- No deferred work without clear timeline
- No system parallelism created
- No forbidden files altered

**BUILD_VALIDATED_WITH_WARNINGS:**
- Core criteria met (compliance matrix: mostly OK, some DEFERRED)
- Deferred work is UI/visual/PlayMode (not critical path)
- Deferred work has documented timeline
- Build/docs/quality passed
- Tests cover what's implemented

**CONTRACT_ONLY:**
- Only DTO/model/interface created
- No integration with systems
- No operational logic implemented
- Expected for foundation phases (data contracts first, wiring later)

**CONTRACT_ONLY_NEEDS_INTEGRATION:**
- Contract created (compiles)
- Depends on wiring with another system (GameplayInputRouter, ModalManager, etc)
- Integration is deferred and documented
- Cannot proceed to next spec if this is critical path

**DEFERRED_UI_VISUAL:**
- Business logic ready and tested
- Only visual/scene/prefab portion deferred
- UI/scene creation is out of scope (spec says so)
- Core logic is sound and independently testable

**NEEDS_REWORK:**
- P0/P1 spec has unmet core criterion
- Example: spec requires N feature, code has M < N
- Example: spec requires system reuse, code created parallel
- Rationale: blockers must be resolved before proceeding

**BLOCKED:**
- Cannot continue without external trigger
- Reason: (Packages / ProjectSettings / scene/pets/future / rewrite / etc)
- Documented in "Remaining work" section

---

## Remaining Work

### Deferred to Phase X
- (item) — target phase, reason, dependency
- (item) — target phase, reason, dependency

### Known Risks
- (risk) — mitigation
- (risk) — mitigation

### Next Spec Dependencies
- (next spec) can proceed: YES/NO — because (reason)
- (next wave) can start: YES/NO — because (reason)

### Blockers for Promotion to Implementados
If status is CONTRACT_ONLY / NEEDS_REWORK / BLOCKED:
- Do NOT move to implementados/ until [specific condition]
- Condition: (when can this be moved?)

---

## Sign-Off

**Executor:** Claude Code (Haiku 4.5)  
**Status:** (final status from top of report)  
**Evidence:** (summary of what exists: code files, tests, report, matrix)  
**Recommendation:** (Proceed to next spec / Hold for rework / Defer to next wave)

---

*Template: SPEC_EXECUTION_REPORT_TEMPLATE_STRICT.md (2026-06-08)*
