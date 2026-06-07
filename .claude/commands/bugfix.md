# /bugfix

Fix a specific bug within the affected files' scope. Minimal context. No roadmap.

**Arguments:** `$ARGUMENTS` — bug description or reference (e.g., `"save not persisting health"`)

---

## Objective

Identify root cause, fix minimally, validate, document. Do not refactor beyond the fix.

---

## Required Reads

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md` — check if bug is a known blocker
3. Files directly related to the bug

## Optional Reads

- Spec that originally implemented the buggy feature (if known)
- Prior validation report if the bug was introduced in a recent change

## Do NOT Read By Default

```
PROJECT_LOG.md
ROADMAP.md
docs/IMPLEMENTATION_STATUS.md
SPEC_EXECUTION_ORDER.md
unrelated validation reports
```

---

## Procedure

1. Read bug report / description
2. Identify affected files
3. Trace root cause
4. Implement minimal fix
5. Run validation:
   - Docs: if any .md changed
   - C# build: if any .cs changed
   - Non-regression review: check fix does not introduce new violations
6. Classify: regression (introduced by recent spec) vs. gap (known limitation)
7. Create commit with Portuguese message describing the fix

---

## Allowed Edits

- Source files directly causing the bug
- Docs if bug report or validation doc needs update

## Forbidden Edits

- No refactoring beyond the fix
- No scope creep
- No spec movement
- No roadmap changes

---

## Output Format

```markdown
## Bug Fix — <description>

**Root Cause:** <what caused the bug>
**Fix:** <what was changed>
**Classification:** Regression / Gap

### Files Changed
[list]

### Validation
| Level | Result |
|-------|--------|
| Docs | PASS/NE |
| C# build | PASS/NE |
| Non-regression | PASS/WARNING |

### Residual Risk
[if any]
```
