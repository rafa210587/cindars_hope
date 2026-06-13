---
name: implementation-closeout
description: Final checklist for closing any relevant task with validations and documentation
version: 1.0
---

# Implementation Closeout Skill

Use at the end of any significant task to ensure all validations, documentation, and logging are complete.

## When to Use

- After implementing a spec
- After fixing a bug with gameplay impact
- After migrating documentation
- After significant refactoring or stabilization work

## Mandatory Closeout Steps

### Step 1: Verify Scope

```powershell
git diff --name-only
git status
```

**Checklist:**
- [ ] All changes are within task scope
- [ ] No accidental edits outside permitted files
- [ ] No `docs_old/**` edits
- [ ] No root `specs/` or `spec/` created
- [ ] Branch is current

### Step 2: Run All Applicable Validations

**Docs validation (mandatory for all tasks):**
```powershell
.\tools\docs\validate_docs.ps1
```
- [ ] Result: PASS ✅ or preexisting WARNING ⚠️

**Unity compile validation (if runtime changed):**
```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "..." -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```
- [ ] Result: PASS ✅ or NOT RUN with documented reason

**Non-regression review:**
```
Run /review-non-regression
```
- [ ] Result: PASS ✅ or acceptable WARNING ⚠️

### Step 3: Update Documentation

**If spec implemented:**
- [ ] Move spec from `.specs/a_implementar/` to `.specs/implementados/`
- [ ] Add evidence header with commit, files, validations
- [ ] Update registries (if they exist)

**If refinement completed:**
- [ ] Move from `docs/refinements/a_implementar/` to `docs/refinements/implementados/`

**Always:**
- [ ] Update `docs/IMPLEMENTATION_STATUS.md` with evidence (spec file or validated logs)
- [ ] Update `PROJECT_LOG.md` with entry (date, objective, deliverables, validations)

### Step 4: Prepare Delivery Report

**Changed files:**
```
Assembled from: git diff --name-only
```

**Commits:**
```
From: git log --oneline -10
Include: All commits related to this task
```

**Technical summary** (1-3 sentences):
- What was implemented or fixed
- Scope boundaries
- Key decisions (if any)

**Validations executed:**
- [ ] Docs validation: PASS / WARNING / FAIL / NOT RUN
- [ ] Unity compile: PASS / FAIL / NOT RUN
- [ ] Log scan: PASS / FAIL / NOT RUN
- [ ] Non-regression: PASS / WARNING / FAIL
- [ ] Play Mode: NOT RUN (reason: sandboxed environment)

**Validations not executed** (if any):
- Reason (sandbox, permissions, timeout, etc.)
- Residual risk (what could break)

**Pending items** (if any):
- List what wasn't completed
- Reason
- Blocking next spec?

**Residual risks** (if any):
- Missing validations
- Features that can't be tested here
- Known gotchas

**Next recommended step:**
- Which spec next?
- Any dependency blocker?
- Any follow-up task?

### Step 5: Do NOT

- [ ] Execute `git push` (user approval required)
- [ ] Open PR/MR (user approval required)
- [ ] Merge branches (user approval required)
- [ ] Mark task as "complete" in external systems without user confirmation
- [ ] Hide validation failures
- [ ] Claim compliance without evidence
- [ ] Commit changes to PROJECT_LOG without updating it

## Output Format Template

```markdown
## Closeout Report — [SPEC Name]

**Date:** YYYY-MM-DD  
**Branch:** [branch-name]  
**Status:** COMPLETE / PARTIAL / BLOCKED  

### Summary

[1-3 sentences: what was implemented, scope, key decisions]

### Changed Files

```
.specs/a_implementar/spec_12_player_combat.md
.specs/implementados/spec_12_player_combat.md
Assets/Scripts/Runtime/Combat/PlayerCombatManager.cs
Assets/Scripts/Runtime/Combat/WeaponDataSO.cs
Assets/_Game/Data/Combat/weapons-basic.asset
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

### Commits

```
abc1234 feat: spec 12 - player combat melee/ranged attacks
def5678 fix: event bus pattern in attack delivery
```

### Validations Executed

| Validation | Result | Evidence |
|---|---|---|
| Docs validation | ✅ PASS | tools/docs/validate_docs.ps1 |
| Unity compile | ✅ PASS | Logs/unity-compile-validation.log: Tundra build success |
| Log scan | ✅ PASS | No new C# errors |
| Non-regression | ✅ PASS | No GameEvent.Find(), IDs in save, events published |
| Play Mode | ⊗ NOT RUN | Reason: sandboxed, cannot launch game client |

### Validations Not Executed

- **Play Mode features:** Cannot test game loop in sandbox
  - Risk: Weapon swap, damage calculation, status effects await user testing
  - Mitigation: User can manually test in Unity Editor

### Deliverables

- ✅ PlayerCombatManager with melee/ranged attack logic
- ✅ WeaponDataSO with stats (damage, range, cooldown)
- ✅ DamageRequest published via GameEventBus
- ✅ Weapon hotbar selection (if UI scope allowed)

### Pending

- (none) — task complete

### Residual Risks

- (none) — all validations passed

### Next Step

- SPEC 13: NPC Combat & Tactics
- Dependency: SPEC 12 (✅ complete)
- Ready to implement

---

Delivered for user review. Ready for merge pending user approval.
```

## Abbreviated Format (For Smaller Tasks)

```markdown
## Closeout — Bug Fix: [Issue]

**Status:** COMPLETE

**Changed:** 
- Assets/Scripts/Runtime/Path/File.cs

**Validation:** 
- Docs: PASS
- Unity: PASS
- Non-regression: PASS

**Risk:** None

**Next:** [if applicable]
```

## Red Flags (Do NOT Deliver)

- ❌ Validation shows FAIL and hasn't been fixed
- ❌ Changes outside task scope and not acknowledged
- ❌ Non-regression shows FAIL
- ❌ Spec marked as implemented without evidence
- ❌ Documentation not updated
- ❌ PROJECT_LOG not updated for significant task

## Checklisting

Before delivering report:

- [ ] All changed files listed
- [ ] All applicable validations run and documented
- [ ] Non-regression audit complete
- [ ] Docs updated and validated
- [ ] PROJECT_LOG updated
- [ ] IMPLEMENTATION_STATUS updated (if spec/capability)
- [ ] Summary is accurate
- [ ] No hidden failures
- [ ] No claims without evidence
- [ ] Next step recommended

## Integration

- **Spec Execution** → Calls this at Phase 4
- **Finish-Spec** → Depends on this for final report
- **Non-Regression Review** → Provides audit input
- **Docs Migration** → Updates docs for this skill to use
- **Unity Validation** → Provides validation evidence

---

**Closeout is NOT complete until the report is generated and delivered to user.**
