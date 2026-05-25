# /finish-spec

Use when completing implementation of a spec or relevant task.

## Mandatory Closeout Checklist

### 1. Verify Scope

```powershell
git diff --name-only
```

- [ ] All changes are within spec/task scope
- [ ] No accidental edits to docs_old/**
- [ ] No files outside permitted scope (e.g., gameplay when tooling task)
- [ ] Branch is current and clean

### 2. Run Applicable Validations

- [ ] Docs altered? Run:
  ```powershell
  .\tools\docs\validate_docs.ps1
  ```
  
- [ ] Runtime/Unity changed? Run:
  ```powershell
  .\tools\unity\RunUnityCompileValidation.ps1
  .\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
  ```

- [ ] Validation not executable? Document:
  ```
  Validation: NOT RUN
  Reason: <reason>
  Command attempted: <command>
  Residual risk: <impact>
  ```

### 3. Update Documentation

- [ ] If spec implemented: Move spec from `docs/specs/a_implementar/` to `docs/specs/implementados/`
- [ ] If refinement completed: Move from `docs/refinements/a_implementar/` to `docs/refinements/implementados/`
- [ ] Update registries: `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` (if file exists)
- [ ] Update status file: `docs/IMPLEMENTATION_STATUS.md` (add capability or update evidence)
- [ ] Update project log: `PROJECT_LOG.md` (add entry with date, objective, deliverables, validations)

### 4. Verify Non-Regression

Use `/review-non-regression` to audit:
- [ ] No root-level specs/ or spec/ created
- [ ] No docs_old/** edits
- [ ] No prohibited Git commands used
- [ ] No forbidden namespaces created
- [ ] No runtime searches (GameObject.Find) introduced
- [ ] Save still using IDs and simple types
- [ ] Events published via GameEventBus (if gameplay)
- [ ] MonoBehaviours remain thin (if runtime)

### 5. Prepare Delivery

Assemble final report with:

**Changed files:**
```
git diff --name-only
```

**Commit summary:**
```
git log --oneline -5
```

**Technical summary:** (1-3 sentences)
- What was implemented/fixed
- Scope boundaries
- Key architectural decisions (if any)

**Validations executed:**
- [x] Docs validation: PASS/FAIL/NOT RUN
- [x] Unity compile: PASS/FAIL/NOT RUN
- [x] Log scan: PASS/FAIL/NOT RUN
- [x] Play Mode: [reason not run]

**Validations not executed:**
- Reason
- Residual risk

**Non-regression:**
- Status: PASS/WARNING/FAIL

**Pending items:**
- (if any)

**Residual risk:**
- (if any)

**Next recommended step:**
- (suggest next spec or action)

## Rules

- [ ] Do NOT run `git push`
- [ ] Do NOT open PR/MR
- [ ] Do NOT mark spec as implemented without evidence in repo
- [ ] Do NOT skip validation if it's applicable
- [ ] Do NOT hide validation failures
- [ ] Do NOT merge documentation changes without running doc validation

## Output Format

```markdown
## Closeout Summary — [Spec/Task Name]

**Date:** YYYY-MM-DD
**Branch:** [branch name]
**Status:** COMPLETE / PARTIAL / BLOCKED

### Deliverables

[List what was delivered]

### Files Changed

[git diff --name-only output]

### Validations

- Docs validation: ✅ PASS / ⚠️ WARNING / ❌ FAIL / ⊗ NOT RUN
- Unity compile: ✅ PASS / ⚠️ WARNING / ❌ FAIL / ⊗ NOT RUN
- Log scan: ✅ PASS / ⚠️ WARNING / ❌ FAIL / ⊗ NOT RUN
- Non-regression: ✅ PASS / ⚠️ WARNING / ❌ FAIL

### Pending

[if any]

### Residual Risk

[if any]

### Next Step

[recommendation]
```

---

**You are now ready for user review and potential merge/push.**
