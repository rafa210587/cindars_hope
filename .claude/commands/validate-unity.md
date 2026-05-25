# /validate-unity

Use when the task alters runtime C#, scenes, prefabs, assets, or ProjectSettings.

## When to Run

- After any runtime code changes
- After modifying scenes or prefabs
- After changing asset settings (sprites, scriptable objects, etc.)
- As part of task closeout

## Execution Steps

### Step 1: Docs Validation (always)

```powershell
.\tools\docs\validate_docs.ps1
```

**Expected outcome:** PASS or list specific docs issues to fix.

### Step 2: Unity Compile Validation (if runtime changed)

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"
```

**Expected outcome:**
- PASS: "Tundra build success"
- FAIL: List compilation errors with file:line references

**If Unity path differs or is unknown:**
- Check environment: `$env:UNITY_EDITOR_PATH`
- Check installed versions: `ls "C:\Program Files\Unity\Hub\Editor\"`
- Register in `.claude/settings.local.json` for future runs

### Step 3: Log Scanner (if Step 2 ran)

```powershell
.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

**Expected outcome:**
- PASS: No new C# errors
- WARNING: Preexisting Assembly warnings (acceptable)
- FAIL: New errors introduced by this task

## Failure Handling

If Step 2 cannot run due to:
- Sandbox environment
- Missing Unity installation
- Permissions issue
- Timeout

**Required documentation in task closeout:**

```text
Unity validation: NOT RUN
Reason: <specific reason>
Command attempted: <command that failed>
Residual risk: Unity compile not validated locally
```

**Do NOT declare runtime as validated without evidence.**

## Success Criteria

- ✅ Docs validation: PASS
- ✅ Unity compile: PASS (if executed)
- ✅ Log scan: No new errors (if executed)
- ✅ If not executed: Clear reason and risk registered

## Do NOT

- Skip validation and claim it "probably works"
- Ignore docs validation even if runtime didn't change
- Hide validation failures in summary
- Proceed to closeout if validation blocking issue exists

---

**Next:** If validation passes, proceed to `/finish-spec` for task closeout.
