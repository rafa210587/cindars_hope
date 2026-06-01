# /finish-spec

Phase-aware closeout. Promotes spec to `implementados/` only when required evidence exists.

**Arguments:** `$ARGUMENTS` — spec ID (used to find execution report and determine promotion eligibility)

---

## Objective

Check evidence, determine phase status, promote if eligible, update documentation.

---

## Required Reads

1. `CLAUDE.md`
2. Execution report for this spec: `docs/validation/<spec_id>_execution_report.md`
3. The spec file itself (to check Phase 2-3 requirements)

## Do NOT Read By Default

```
PROJECT_LOG.md (full)
ROADMAP.md
SPEC_EXECUTION_ORDER.md (full)
```

---

## Promotion Eligibility

### Docs-only spec (no C#, no Unity)

Required: `BUILD_VALIDATED` (docs PASS)
→ May promote after docs validation passes.

### Code spec (C# changes, no Unity game objects)

Required: `BUILD_VALIDATED` (dotnet build 0E/0W + docs PASS)
→ May promote after Phase 1-2 validation passes.
→ Phase 3 (Play Mode) is optional for purely mechanical code changes.

### Runtime spec (touches gameplay, scene, prefab, ScriptableObject behavior)

Required: `ACCEPTED` (Phases 1-3 all have evidence)
→ Must NOT promote until Phase 2 (Unity validators) and Phase 3 (Play Mode) pass.
→ If Phase 2-3 not yet run: update status to `BUILD_VALIDATED`, do NOT promote.

---

## Promotion Checklist

- [ ] Execution report exists in `docs/validation/`
- [ ] Required validation phases have PASS evidence in report
- [ ] Phase 2-3 eligibility check above performed
- [ ] Spec is eligible for promotion (see above)

**If NOT eligible:** Update execution report status to current phase level. Stop. Do not promote.

---

## Closeout Steps (when eligible)

1. Move spec: `docs/specs/a_implementar/<spec>.md` → `docs/specs/implementados/<spec>.md`
2. Add evidence header to spec file:
   ```
   ---
   status: implemented
   implemented_date: YYYY-MM-DD
   phase_status: <BUILD_VALIDATED / ACCEPTED>
   evidence: docs/validation/<spec_id>_execution_report.md
   ---
   ```
3. If refinement exists: move from `docs/refinements/a_implementar/` to `docs/refinements/implementados/`
4. Update `PROJECT_LOG.md` — add short entry
5. Update `docs/IMPLEMENTATION_STATUS.md` — update spec status
6. Run `tools/docs/validate_docs.ps1` — must PASS

---

## Output Format

```markdown
## Closeout — <SPEC_ID>

**Date:** YYYY-MM-DD
**Phase Status:** <BUILD_VALIDATED / UNITY_VALIDATED / ACCEPTED / PARTIAL>
**Promoted:** YES / NO

### Promotion Decision
[Why promoted or not promoted]

### Evidence
[Execution report path]
[Validation results]

### Phase Summary
| Phase | Status |
|-------|--------|
| Phase 0 (Audit) | COMPLETE / NOT RUN |
| Phase 1 (Build) | PASS / FAIL / NOT RUN |
| Phase 2 (Unity) | PASS / FAIL / NOT RUN — PENDING |
| Phase 3 (Play Mode) | PASS / FAIL / NOT RUN — PENDING |

### Next Step
[If Phase 2-3 pending: list required actions]
[If accepted: suggest next spec]
```

---

## Rules

- DO NOT promote without execution report
- DO NOT claim ACCEPTED without Phase 2-3 evidence if spec requires it
- DO NOT skip docs validation after moving files
- DO NOT push or open PR
