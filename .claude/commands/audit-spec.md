# /audit-spec

Use for Phase 0 audit: map what exists, what is missing, risks, and smallest safe delta. Does NOT implement.

**Arguments:** `$ARGUMENTS` — spec number or filename

---

## Objective

Produce an audit matrix for the spec's domain. Identify what already exists in the codebase vs. what the spec requires. Find the minimum change needed.

---

## Required Reads

1. `CLAUDE.md`
2. `docs/00_PROJECT/CURRENT_STATE.md`
3. Target spec
4. Relevant source files (search codebase for spec's domain)

## Optional Reads

- Prior validation reports listed as dependencies
- Architecture docs cited by the spec

## Do NOT Read By Default

```
PROJECT_LOG.md
ROADMAP.md
docs/IMPLEMENTATION_STATUS.md
SPEC_EXECUTION_ORDER.md
all refinements
```

---

## Procedure

1. Read spec and CURRENT_STATE.md
2. Search codebase for:
   - Classes/files the spec will create
   - Classes/files the spec will modify
   - Existing implementations of the same concern
3. Classify findings:
   - `ALREADY_EXISTS` — spec requirement is already met
   - `PARTIAL` — exists but incomplete per spec
   - `MISSING` — does not exist yet
   - `CONFLICT` — existing code contradicts spec requirement
4. Identify risks and the smallest safe delta
5. Output audit matrix (see format)

---

## Allowed Edits

Only one: create audit matrix at `docs/validation/<spec_id>_phase0_audit_matrix.md`

---

## Forbidden Edits

- No code changes
- No spec movement
- No status updates
- No other documentation changes

---

## Output Format

Create `docs/validation/<spec_id>_phase0_audit_matrix.md`:

```markdown
# Phase 0 Audit — <SPEC_ID>

## Requirements vs. Codebase

| Requirement | Status | Notes |
|-------------|--------|-------|
| <requirement> | ALREADY_EXISTS / PARTIAL / MISSING / CONFLICT | <detail> |

## Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| <risk> | LOW/MED/HIGH | <approach> |

## Smallest Safe Delta

<description of minimum changes needed>

## Stop Conditions Found

<any blockers discovered>
```
