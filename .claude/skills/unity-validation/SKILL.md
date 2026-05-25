---
name: unity-validation
description: Validate Unity compile, logs, scenes, prefabs, and residual risks
version: 1.0
---

# Unity Validation Skill

Use when the task alters C# runtime, scenes, prefabs, assets, or ProjectSettings.

## When to Validate

- **Always:** If any `.cs` file changed
- **Always:** If any scene or prefab modified
- **Always:** If any asset setting changed (sprite import, scriptable object, etc.)
- **Always:** If any ProjectSettings altered
- **Optional:** For pure documentation tasks (skip unless integrated with runtime task)

## Validation Steps

### Step 1: Docs Validation (mandatory for all tasks)

```powershell
.\tools\docs\validate_docs.ps1
```

**Expected outcomes:**
- ✅ PASS: All docs consistent
- ⚠️ WARNING: Minor issues (preexisting, acceptable)
- ❌ FAIL: Breaking docs issues (must fix)

**Action:**
- If PASS or WARNING: Continue
- If FAIL: Fix docs, re-run, then continue

### Step 2: Unity Compile Validation (if runtime task)

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"
```

**Expected outcomes:**
- ✅ PASS: "Tundra build success" in log
- ⚠️ WARNING: Assembly warnings (preexisting, acceptable)
- ❌ FAIL: New C# errors (must fix)

**If Unity path unknown:**
- Check: `$env:UNITY_EDITOR_PATH`
- Or list: `ls "C:\Program Files\Unity\Hub\Editor\"`
- Or search: `Get-ChildItem -Path "C:\Program Files\Unity" -Recurse -Filter "Unity.exe" | Select-Object -First 3`
- Register in `.claude/settings.local.json` for future runs

**If cannot run:**
- Document: `Reason: [sandbox|permissions|timeout|missing]`
- Register in closeout as NOT RUN with residual risk

### Step 3: Log Scanner (if Step 2 succeeded)

```powershell
.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

**Expected outcomes:**
- ✅ PASS: No new C# errors introduced
- ⚠️ WARNING: Preexisting Assembly firstpass warnings (acceptable)
- ❌ FAIL: New errors found (must fix)

**Action:**
- If PASS or WARNING: Validation complete
- If FAIL: Identify errors, fix in code, re-run Step 2 and 3

## Validation Report Format

```text
Validation Summary
─────────────────

Docs validation: ✅ PASS
  - No issues found

Unity compile:   ✅ PASS
  - Tundra build success
  - Log: .\Logs\unity-compile-validation.log

Log scan:        ✅ PASS
  - No new C# errors
  - Preexisting Assembly warnings noted

Overall: ✅ READY FOR CLOSEOUT
```

## Error Handling

### Compilation Errors

If Step 2 shows compilation errors:

1. **Read error message:** `file.cs:line: error CS####: message`
2. **Categorize error:**
   - **Missing using directive** → Add to file
   - **Type mismatch** → Fix type or cast
   - **Missing method** → Implement or use correct method name
   - **Namespace conflict** → Resolve naming
3. **Fix in code**
4. **Re-run validation**

### Validation Cannot Run

If the validation script cannot execute due to environment:

**Document in closeout:**

```text
Unity validation: NOT RUN
Reason: [sandbox environment | missing Unity editor | timeout | permissions]
Command attempted: .\tools\unity\RunUnityCompileValidation.ps1
Residual risk: Unity compile not validated locally. Features may break on manual build.
Mitigation: User must validate in Unity Editor before production build.
```

## Rules

- [ ] Do NOT skip docs validation even if only code changed
- [ ] Do NOT claim "compile success" without running Step 2
- [ ] Do NOT ignore Step 3 output if errors are listed
- [ ] Do NOT fix errors silently without re-running validation
- [ ] Do NOT hide validation failures in final report
- [ ] Do NOT declare validated without evidence (log file)

## Success Criteria

- ✅ Docs validation: PASS or preexisting WARNING
- ✅ Unity compile: PASS or NOT RUN with documented reason
- ✅ Log scan: PASS or NOT RUN with documented reason
- ✅ All failures fixed before closeout
- ✅ Evidence (log files) preserved for audit

## Integration with Other Skills

- **Spec Execution** → Calls this skill if runtime task
- **Implementation Closeout** → Requires validation results
- **Non-Regression Review** → Audits diff separately (doesn't replace this)

**Next:** If validation passes, proceed to `/finish-spec` for task closeout.
