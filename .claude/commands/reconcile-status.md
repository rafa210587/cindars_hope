# /reconcile-status

Audit inconsistencies between documentation files. May read PROJECT_LOG.md, validation reports, and IMPLEMENTATION_STATUS.md — audit mode only.

**Arguments:** `$ARGUMENTS` — optional scope (e.g., `SPEC_18-28` or `MVP`)

---

## Objective

Find and document discrepancies between: execution reports, CURRENT_STATE.md, PROJECT_LOG.md, IMPLEMENTATION_STATUS.md, release reports, spec locations. Do NOT fix code.

---

## Required Reads

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md`
3. Scope-relevant validation reports
4. `docs/release/MVP_ACCEPTANCE_REPORT.md` (if MVP scope)

## Conditional Reads (audit mode — allowed here)

- `PROJECT_LOG.md` — for timeline of events
- `docs/IMPLEMENTATION_STATUS.md` — for spec status claims
- `docs/specs/SPEC_EXECUTION_ORDER.md` — for dependency graph
- Specific validation reports in scope

---

## Procedure

1. Identify the reconciliation scope
2. Read all relevant reports for that scope
3. For each claim, check if evidence exists:
   - Claim: "Phase 1 PASS" → check: dotnet build result in report
   - Claim: "Phase 2 PASS" → check: Unity validator output in report
   - Claim: "Phase 3 PASS" → check: Play Mode checklist with ✓ marks
   - Claim: "spec implemented" → check: spec in `implementados/` with evidence header
4. Create audit matrix
5. List inconsistencies with severity: CRITICAL / HIGH / MEDIUM / INFO
6. Propose corrections (do NOT auto-apply)
7. Output reconciliation report

---

## Allowed Edits

Only: create `docs/validation/<scope>_reconciliation_audit_matrix.md`

## Forbidden Edits

- No spec movement
- No code changes
- No status updates without human confirmation
- No document deletion

---

## Output Format

```markdown
# Reconciliation Audit — <Scope>

## Claims vs. Evidence

| Claim | Location | Evidence Found? | Inconsistency | Severity |
|-------|----------|----------------|---------------|---------|
| <claim> | <file> | YES/NO | <description> | CRITICAL/HIGH/MED/INFO |

## Proposed Corrections

1. <correction> — requires: <human action / agent action>

## No Action Needed

[Claims that are consistent]
```
