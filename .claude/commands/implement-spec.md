# /implement-spec

Execute a spec. Generates execution report. Does NOT auto-promote spec to `implementados/` — closeout is phase-gated.

**Arguments:** `$ARGUMENTS` — spec number or filename

---

## Objective

Implement the spec within its declared scope. Validate. Generate execution report with honest phase status. Stop before promoting spec.

---

## Required Reads

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md`
3. `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — execution governance and phase taxonomy
4. `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md` — validation requirements by change type
5. Target spec
6. Files explicitly in spec scope

## Optional Reads (only if spec cites them)

- A specific refinement
- An implemented dependency spec (only if directly referenced)
- Prior validation report listed as dependency

## Conditional Required Reads (if runtime/code changes)

For specs that change runtime behavior or code:

- `.claude/rules/testing-quality-gate.md` — mandatory testing requirements
- `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` — human validation checklist and timing

## Do NOT Read By Default

```
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
SPEC_EXECUTION_ORDER.md (full)
ROADMAP.md
memory/ (unless spec cites prior pattern)
```

---

## Execution Phases

### Phase 0 — Scope Lock

- [ ] Read spec and identify scope
- [ ] List permitted files
- [ ] List forbidden files
- [ ] Identify applicable skills
- [ ] Check CURRENT_STATE.md for blockers
- [ ] If blocked: stop and report

### Phase 1 — Implementation

- [ ] Implement per spec exactly
- [ ] Do NOT amplify scope
- [ ] Do NOT touch files outside scope
- [ ] Commit frequently in Portuguese
- [ ] Do NOT move spec to implementados/ yet

### Phase 2 — Validation

Run `/validate-spec` after implementation:

- [ ] Docs validation (if docs changed)
- [ ] C# runtime build (if .cs changed)
- [ ] C# editor build (if editor .cs changed)
- [ ] Record NOT RUN with reason for Phase 4-5 (Unity/Play Mode)

### Phase 3 — Non-Regression Review

Run `/review-non-regression`:

- [ ] No forbidden patterns introduced (GameObject.Find, direct gameplay calls)
- [ ] No files outside scope modified
- [ ] No root specs/ created
- [ ] No docs_old/ edited
- [ ] No Unity refs in save DTOs

### Phase 4 — Execution Report

Create `docs/validation/<spec_id>_execution_report.md` with:

- Phase status table
- Files changed
- Commits
- Validations executed
- Validations NOT RUN (with reason and residual risk)
- Status classification (see taxonomy)

---

## Phase Status Taxonomy

Do NOT use just "COMPLETE". Use:

| Status | Meaning |
|--------|---------|
| `CODE_COMPLETE` | Code written; not yet validated |
| `BUILD_VALIDATED` | dotnet build + docs PASS |
| `UNITY_VALIDATED` | Unity validators PASS (Phase 2) |
| `PLAYMODE_VALIDATED` | Play Mode checklist PASS (Phase 3) |
| `ACCEPTED` | All required phases complete |
| `DEFERRED_TO_FINAL_HUMAN_VALIDATION` | Code complete; Phase 3 human validation deferred to wave-end batch (per docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md) |
| `PARTIAL` | Some phases done, some blocked |
| `BLOCKED` | Blocker found; cannot continue |

---

## Spec Promotion Rule

DO NOT move spec to `implementados/` in this command.

Promotion happens in `/finish-spec` ONLY when:
- Spec does NOT require Phase 2-3: `BUILD_VALIDATED` is sufficient
- Spec DOES require Phase 2-3: must reach `PLAYMODE_VALIDATED` or `ACCEPTED`
- Both: execution report exists in repo

---

## Output Format

```markdown
## Resumo Técnico — <SPEC_ID>

**Data:** YYYY-MM-DD
**Status:** <BUILD_VALIDATED / PARTIAL / BLOCKED / etc.>

### Deliverables
[what was implemented]

### Files Changed
[git diff --name-only]

### Commits
[git log --oneline]

### Validations

| Level | Type | Result | Notes |
|-------|------|--------|-------|
| 1 | Docs | PASS/FAIL/NE | |
| 2 | C# runtime | PASS 0E/0W / FAIL / NE | |
| 3 | C# editor | PASS 0E/XW / FAIL / NE | |
| 4 | Unity validators | NOT RUN | Reason: requires Unity Editor |
| 5 | Play Mode | NOT RUN | Reason: requires human |

### Risks
[if any]

### Next Step
[/finish-spec when Phase 2-3 evidence collected, OR: spec is docs-only → can promote now]
```

---

## Rules

- DO implement spec exactly as scoped
- DO run validations and record results
- DO create execution report
- DO NOT push to remote
- DO NOT open PR/MR
- DO NOT move spec to implementados/ here
- DO NOT claim ACCEPTED without Phase 2-3 evidence if required
