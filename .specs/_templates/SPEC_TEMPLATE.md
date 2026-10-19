---
doc_type: spec
status: draft
source_of_truth: true
do_not_execute: false
spec_id: SPEC_NN_<slug>
depends_on: []
blocks: []
created_from_refinement: ""
read_refinement_by_default: false
required_read:
  - AGENTS.md
  - docs/project/CURRENT_STATE.md
  - this_spec
optional_read:
  - docs/refinements/accepted/<refinement>.md
required_adrs: []
required_game_rules: []
do_not_read_by_default:
  - PROJECT_LOG.md
  - docs/project/ROADMAP.md
  - docs/IMPLEMENTATION_STATUS.md
  - unrelated docs/validation reports
  - all ADRs/game_rules unless explicitly listed
validation_required:
  - dotnet build runtime (0E/0W)
  - dotnet build editor (0E/0W)
  - tools/docs/validate_docs.ps1
---

# SPEC_NN — Title

> Spec ID: `spec_<slug>`  
> Order: NN  
> Type: Feature | Validation | Documentation | Governance  
> Branch: `dev`  
> Status: draft → proposed → active → in_progress → implemented  
> Depends on: SPEC_XX  
> Blocks: SPEC_YY  

---

## 1. Objective

One paragraph. What does this spec accomplish?

---

## 2. Non-Goals

What this spec explicitly does NOT do:

- No new runtime features beyond scope
- No refactors not required
- No schema changes unless specified
- No breaking changes to validated gameplay

---

## 3. Required ADRs and Game Rules

List only what is necessary for this spec.

```yaml
required_adrs:
  - ADR-0005-cave-stable-run-and-replay
required_game_rules:
  - cave_rules.md
  - event_rules.md
```

**If no ADR/game_rule is needed, keep both lists empty.**

This spec must remain self-contained. ADRs explain *why* decisions were made. Game rules define *what is true now*.

---

## 4. Current Known State

What is the state of the system before this spec runs?

```
System X: implemented (Phase 0-1)
System Y: pending (Phase 2-3)
Build: PASS 0E/0W (from last validation)
```

---

## 5. Scope

**Source files:**
- `Assets/_Game/Scripts/...`

**Optional (if spec is ambiguous):**
- `docs/refinements/accepted/<refinement>.md`

**Do NOT read:**
- PROJECT_LOG.md
- ROADMAP.md
- GDD
- IMPLEMENTATION_STATUS.md
- Unrelated validation reports

---

## 5. Implementation Rules

1. No `GameObject.Find()` or `FindObjectOfType()` in runtime
2. Use `GameEventBus.Publish()` for communication
3. No hardcoded balance data in MonoBehaviour
4. Unsubscribe in `OnDisable` or `OnDestroy`
5. Save data: IDs and simple types only
6. No namespace `Debug` under `CindarsHope.*`

---

## 6. Regression Risks

- Risk A: describes what existing behavior could break
- Risk B: which existing tests/validators cover it

---

## 7. Tasks

- [ ] Task 1
- [ ] Task 2
- [ ] Task 3

---

## 8. Validation

### Phase 0 — Audit
- Create `docs/validation/spec_<spec_id>_phase0_audit_matrix.md`
- No code changes before audit complete

### Phase 1 — Automated
```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
tools/docs/validate_docs.ps1
```

Expected: 0E/0W runtime, 0E/0W editor, 14/14 docs

### Phase 2 — Unity Validators (if applicable)
```
CindarsHope/Repair and Validate Project
CindarsHope/Validate/Combat/...
```

### Phase 3 — Play Mode (if applicable)
- [ ] Checklist item 1
- [ ] Checklist item 2

---

## 9. Acceptance Criteria

1. Criterion 1
2. Criterion 2
3. No new errors or warnings
4. `tools/docs/validate_docs.ps1` PASS

---

## 10. Stop Conditions

Stop and report if:
- Build fails
- Docs validation fails
- Spec requires runtime changes outside scope
- Unexpected regression found

---

## 11. Required Report

Create: `docs/validation/spec_<spec_id>_execution_report.md`

Update:
- `PROJECT_LOG.md`
- `docs/00_PROJECT/CURRENT_STATE.md` if status changes
- `docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md` if validation ran

---
