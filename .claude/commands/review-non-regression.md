# /review-non-regression

Audit current diff against project non-regression rules and detected change scope.

**Argument expected:** (optional) Force full audit even if no changes detected

## Pre-Flight: Check Change Scope

First, read `.claude/.runtime/change-scope.json` (if it exists from `/implement-spec` or detect-change-scope hook):

```json
{
  "docsChanged": true,
  "unityRuntimeChanged": false,
  "projectSettingsChanged": false,
  "forbiddenPathsChanged": false,
  "rootSpecsRecreated": false,
  "specDocsChanged": true,
  "specMigrationDetected": true,
  "refinementDocsChanged": false,
  "refinementMigrationDetected": false
}
```

**Critical failures (immediate FAIL):**
- `forbiddenPathsChanged == true` (docs_old/, root specs/)
- `rootSpecsRecreated == true` (specs/ or spec/ created in root)

**Legitimate actions (NOT failures):**
- `specMigrationDetected == true` during `/implement-spec` closeout
- `refinementMigrationDetected == true` during spec closeout

## Mandatory Checks

### 1. File Structure

- [ ] No creation of root-level `specs/` or `spec/` directories
- [ ] No edits to `docs_old/**`
- [ ] No edits outside permitted scope

### 2. Git Safety

- [ ] No `git push` executed
- [ ] No `git reset --hard` executed
- [ ] No `git clean` executed
- [ ] No `git stash` executed
- [ ] Branch is clean or only has intended commits

### 3. Gameplay/Runtime Safety (if runtime task)

- [ ] No `GameObject.Find()` calls introduced
- [ ] No `FindObjectOfType()` calls introduced
- [ ] No `FindObjectsByType()` calls introduced
- [ ] No direct MonoBehaviour-to-MonoBehaviour communication (use GameEventBus)
- [ ] Save doesn't serialize UnityEngine refs (no GameObject, Transform, MonoBehaviour, Sprite, Collider, Rigidbody)
- [ ] Save uses IDs and simple types only
- [ ] No hardcoded game data in MonoBehaviour (use ScriptableObject)
- [ ] No `StreamingAssets` for editable save data (use Application.persistentDataPath)
- [ ] ScriptableObjects prefixed correctly (ItemDataSO, WeaponDataSO, etc.)
- [ ] Events prefixed correctly (DayStartedEvent, ItemCraftedEvent, etc.)
- [ ] No forbidden namespaces created (`CindarsHope.Debug`, etc.)

### 4. Runtime Evidence (if available)

Inspect `.claude/.runtime/` directory for evidence files:

- [ ] `.claude/.runtime/change-scope.json` — Persisted change detection
- [ ] `.claude/.runtime/validation-results.json` — Persisted validation results

Use these as evidence for required validations.

### 5. Validations Executed (if using /implement-spec)

If `.claude/.runtime/validation-results.json` exists:

- [ ] Docs validation: PASS or acceptable WARNING
- [ ] Unity compile: PASS or documented NOT RUN
- [ ] Log scan: PASS or documented NOT RUN
- [ ] Overall status: PASS/FAIL/WARNING

If files are missing, check console output for validation evidence.

### 6. Spec/Roadmap Safety (if spec task)

- [ ] Task doesn't exceed spec scope
- [ ] Didn't implement blocked/future spec (check SPEC_EXECUTION_ORDER.md)
- [ ] Didn't skip specs in wrong order
- [ ] Status updates have evidence in repo (code, assets, or validated logs)

### 7. Documentation Safety

- [ ] No spec marked as implemented without evidence
- [ ] IMPLEMENTATION_STATUS.md changes reflect actual state
- [ ] PROJECT_LOG.md updated when task was significant
- [ ] No duplicate entries in registries

## Output Format

```text
Status: PASS | WARNING | FAIL

Evidence:
  Changed files: <list>
  Scope: <task scope>
  Validations: <which were run>

Critical files analyzed:
  git diff --name-only: <output>
  Key changes: <summary>

Issues found:
  [If any]

Corrective actions required:
  [If any - mandatory to fix]

Residual risk:
  [If any]
```

## Examples

### PASS

```text
Status: PASS

Evidence:
  Changed files:
    - docs/specs/implementados/spec_12_player_combat.md
    - Assets/Scripts/Runtime/Combat/PlayerCombatManager.cs
    - Assets/_Game/Data/Combat/WeaponDataSO.cs

  Scope: SPEC 12 - Player Combat (approved, no overshoot)
  Validations: Docs PASS, Unity compile PASS

Critical files analyzed:
  - No docs_old/** edits ✓
  - No root specs/ ✓
  - No GameObject.Find() ✓
  - Save contract untouched ✓
  - Event bus used for combat events ✓

Issues found:
  None

Residual risk:
  Play Mode features await manual user testing
```

### WARNING

```text
Status: WARNING

Issues found:
  - DamageAppliedEvent now carries Transform reference (was TargetPosition ID before)

Corrective actions required:
  Change DamageAppliedEvent.TargetTransform → DamageAppliedEvent.TargetPositionId
  Verify no save serialization of this event

Residual risk:
  Runtime may break if event is serialized; needs validation in Play Mode
```

### FAIL

```text
Status: FAIL

Issues found:
  - Attempted: git push (BLOCKED)
  - Found: GameObject.Find() in PlayerCombatManager.cs:123
  - Found: Save serializing WeaponDataSO ref (forbidden)

Corrective actions required:
  1. Remove GameObject.Find() → use GameEventBus publish/subscribe
  2. Change save to store WeaponID (int) instead of WeaponDataSO ref
  3. Do NOT execute git push

Residual risk:
  CRITICAL - Task cannot proceed until these are fixed
```

---

## Do NOT

- Ignore warnings and call it "close enough"
- Skip this check and claim everything is fine
- Mark FAIL as acceptable for production task
- Fix issues silently without documenting in PROJECT_LOG.md

**If audit shows FAIL:** Fix issues, run this command again.
