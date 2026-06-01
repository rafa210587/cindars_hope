---
name: spec-execution
description: Execute a spec respecting scope, permitted files, mandatory validations, and phase-gated closeout
version: 2.0
when_to_use: Implementing any spec from docs/specs/a_implementar/
---

# Spec Execution Skill

## Use When

Task involves implementing or advancing a spec from `docs/specs/a_implementar/`.

## Required Reads

1. `CLAUDE.md`
2. `docs/00_PROJECT/CURRENT_STATE.md`
3. Target spec
4. Files explicitly in spec scope

## Do Not Read By Default

```
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/operations/AGENT_EXECUTION_PROTOCOL.md
SPEC_EXECUTION_ORDER.md (full — use CURRENT_STATE.md queue)
ROADMAP.md
memory/ (unless spec cites prior pattern)
```

## Procedure

### Phase 0: Preparation

1. Read CURRENT_STATE.md — check for blockers
2. Read target spec — identify scope, dependencies, validation requirements
3. Check if dependencies are resolved
4. Identify applicable sub-skills (see below)
5. If blocked: stop and report

### Phase 1: Scope Lock

- [ ] Permitted files listed
- [ ] Forbidden files listed (docs_old, root specs, files outside spec scope)
- [ ] Mandatory validations identified
- [ ] Phase 2-3 requirement determined (does spec need Unity validators / Play Mode?)

### Phase 2: Implementation

- [ ] Implement per spec exactly
- [ ] No scope amplification
- [ ] No forbidden pattern introduction (GameObject.Find, direct gameplay calls)
- [ ] Commit frequently in Portuguese
- [ ] Do NOT move spec to implementados/ yet

### Phase 3: Validation

Run `/validate-spec`:
- [ ] Docs validation (if docs changed)
- [ ] C# runtime build (if .cs changed)
- [ ] C# editor build (if editor .cs changed)
- [ ] Record NOT RUN with reason for Phase 4-5

Run `/review-non-regression`:
- [ ] No forbidden patterns
- [ ] No out-of-scope file edits
- [ ] No Unity refs in save

### Phase 4: Execution Report

Create `docs/validation/<spec_id>_execution_report.md`.

Record phase status using taxonomy:

| Status | Meaning |
|--------|---------|
| `BUILD_VALIDATED` | dotnet build + docs PASS |
| `UNITY_VALIDATED` | Unity validators PASS |
| `PLAYMODE_VALIDATED` | Play Mode checklist PASS |
| `ACCEPTED` | All required phases complete |
| `PARTIAL` | Some phases complete, some not |
| `BLOCKED` | Cannot proceed |

### Phase 5: Closeout (via /finish-spec)

DO NOT auto-move spec to implementados/.

Call `/finish-spec` which checks promotion eligibility:
- Docs-only spec: promote after `BUILD_VALIDATED`
- Code spec (no gameplay): promote after `BUILD_VALIDATED`
- Runtime/gameplay spec: promote only after `ACCEPTED` (Phase 2-3 required)

## Validation

```powershell
# Docs
.\tools\docs\validate_docs.ps1

# C# runtime
dotnet build .\Assembly-CSharp.csproj --no-restore

# C# editor
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

## Common Regressions

- Moving spec to implementados/ before Phase 2-3 evidence collected
- Claiming ACCEPTED based on Phase 1 build only
- Reading PROJECT_LOG.md as default context
- Amplifying scope beyond spec

## Stop Conditions

- Spec and CURRENT_STATE.md conflict
- Spec scope is ambiguous after careful reading
- Dependency blocked (per CURRENT_STATE.md)
- Mandatory validation fails with no documented path forward
- Any forbidden file would be modified

## Output

Execution report at `docs/validation/<spec_id>_execution_report.md` + commit history.

## Applicable Sub-Skills

- `unity-validation` — if runtime changes
- `save-load-pattern` — if persistence in scope
- `event-bus-pattern` — if gameplay communication in scope
- `bootstrap-wiring` — if manager wiring in scope
- `non-regression-review` — before closeout
- `docs-migration` — when promoting eligible spec
- `implementation-closeout` — final checklist
