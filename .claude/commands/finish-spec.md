# /finish-spec

Phase-aware closeout with human test scenario requirement. Promotes spec to `implementados/` only when required evidence exists.

**Arguments:** `$ARGUMENTS` — spec ID (used to find execution report and determine promotion eligibility)

---

## Objective

Check evidence, determine phase status, promote if eligible, update documentation.

---

## Required Reads

1. `CLAUDE.md`
2. `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — phase taxonomy and promotion rules
3. Execution report for this spec: `docs/validation/<spec_id>_execution_report.md`
4. The spec file itself (to check Phase 2-3 requirements)

## Conditional Required Reads

- `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` — if runtime spec requires Phase 3 human validation
- Human test scenario file: `docs/validation/playmode/<spec_id>_human_test_scenario.md` (if runtime)

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
→ No test scenario required.
→ May promote after docs validation passes.

### Code spec (C# changes, no Unity game objects)

Required: `BUILD_VALIDATED` (dotnet build 0E/0W + docs PASS)
→ If spec changes purely mechanical logic (no UI, event, combat, save): Phase 3 optional.
→ If spec changes UI state or event publishing: human test scenario required.
→ May promote after Phase 1-2 validation passes + test scenario (if needed).

### Runtime spec (touches gameplay, scene, prefab, ScriptableObject behavior)

Required: `ACCEPTED` (Phases 1-3 all have evidence + test scenario evidence)
→ Human test scenario file REQUIRED: `docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md`
→ Test scenario must be referenced in execution report's Phase 3 section.
→ Must NOT promote until Phase 2 (Unity validators) and Phase 3 (Play Mode) pass with test scenario evidence.
→ If Phase 2-3 not yet run: update status to `BUILD_VALIDATED`, do NOT promote.

#### Scope Detection

Runtime scope includes:

- Any change to gameplay behavior (movement, combat, interaction, progression)
- Any UI state change (hotbar, inventory, menus, modals)
- Any save/load persistence logic
- Cave procedural generation or snapshot runtime
- Event publishing or event handler changes
- ScriptableObject-based data that affects gameplay at runtime
- Farm, shop, or economy mechanics
- Equipment, skill tree, or character progression

Use `detect-change-scope.ps1` output (change-scope.json) to confirm scope.

---

## Promotion Checklist

- [ ] Execution report exists in `docs/validation/`
- [ ] Required validation phases have PASS evidence in report
- [ ] Phase 2-3 eligibility check above performed
- [ ] Spec is eligible for promotion (see above)

**If NOT eligible:** Update execution report status to current phase level. Stop. Do not promote.

---

## Closeout Steps (when eligible)

**Before moving files, verify test scenario (if runtime):**

- If spec is runtime/gameplay: confirm `docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md` exists.
- If spec is code-only and nonplayable: skip test scenario check.
- If test scenario is missing for a runtime spec: DO NOT PROMOTE. Stop and update report to `BUILD_VALIDATED`.

**If eligible:**

1. Move spec: `docs/specs/a_implementar/<spec>.md` → `docs/specs/implementados/<spec>.md`
2. Add evidence header to spec file:
   ```
   ---
   status: implemented
   implemented_date: YYYY-MM-DD
   phase_status: <BUILD_VALIDATED / ACCEPTED>
   evidence: docs/validation/<spec_id>_execution_report.md
   human_test_scenario: docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md (if runtime)
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

### How to Test (if runtime/gameplay)
[If spec is runtime/gameplay:]
- Human test scenario: `docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md`
- Expected test time: [X minutes]
- Tester: [human] (not automated)
- Status: [NOT RUN until Phase 3]

[If spec is docs-only or code-only:]
- Not applicable (test scenario not required)

### Expected Gameplay Behavior (if applicable)
[What the human should observe when testing:]
- Feature works without crash
- Expected visual/audio feedback
- Expected state changes
- No forbidden console errors
- Save/load persists feature state (if applicable)

### Next Step
[If Phase 2-3 pending: list required actions]
[If accepted: suggest next spec]
```

---

## Rules

- DO NOT promote without execution report
- DO NOT claim ACCEPTED without Phase 2-3 evidence if spec requires it
- DO NOT promote runtime/gameplay spec without human test scenario file
- DO NOT skip docs validation after moving files
- DO NOT push or open PR

## Usage Notes

**Determine spec type before running this command:**
- Use `detect-change-scope.ps1` to generate `change-scope.json`
- Read execution report to understand what changed
- Check Phase 2-3 validator output for Unity compile/Play Mode results
- If runtime changed and Phase 3 missing: update report to `BUILD_VALIDATED` and stop

**Test scenario creation:**
- For runtime specs: invoke `/gameplay-test-scenario` skill before `/finish-spec`
- Human tester must follow scenario steps and record results
- Test scenario evidence goes into Phase 3 section of execution report
- Without test scenario evidence, promote status is `BUILD_VALIDATED` max, not `ACCEPTED`
