# /validate-spec

Run validations after implementation. Records each level separately. Always documents NOT RUN with reason.

**Arguments:** `$ARGUMENTS` — spec ID or name (used to identify what changed)

---

## Objective

Execute the appropriate validation levels for what was changed. Record results honestly.

---

## Required Reads

1. `CLAUDE.md`
2. Target spec (to determine required validation levels)
3. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — validation levels required by change type
4. `docs/project/CURRENT_STATE.md` (for context on what changed)

## Do NOT Read By Default

```
PROJECT_LOG.md
ROADMAP.md
full IMPLEMENTATION_STATUS.md
```

---

## Validation Levels (execute only what applies)

### Level 1 — Docs Validation (if any .md files changed)

```powershell
.\tools\docs\validate_docs.ps1
```

Expected: PASS 14/14

### Level 2 — C# Runtime Build (if any .cs files in Assets/ changed)

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
```

Expected: 0 errors, 0 new warnings

### Level 3 — C# Editor Build (if any editor .cs files changed)

```powershell
dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Expected: 0 errors (pre-existing warnings acceptable)

### Level 4 — Unity Validators (Phase 2 — requires local Unity Editor)

```
CindarsHope/Repair and Validate Project
CindarsHope/Validate/Combat/Validate Combat Databases
```

Result: PASS / FAIL / NOT RUN

### Level 5 — Play Mode (Phase 3 — requires human in Unity Editor)

Per checklist in the spec's execution report.

Result: PASS / FAIL / NOT RUN

---

## NOT RUN Documentation

If a validation level cannot run, record:

```
<Level>: NOT RUN
Reason: <specific blocker — Unity lock, sandbox, timeout, no Unity license, docs-only spec>
Command attempted: <command>
Residual risk: <what is unvalidated>
```

---

## Output Format

```
Validation Results — <SPEC_ID>

| Level | Type | Result | Duration | Notes |
|-------|------|--------|----------|-------|
| 1 | Docs validation | PASS 14/14 / FAIL / NE | Xs | |
| 2 | C# runtime build | PASS 0E/0W / FAIL / NE | Xs | |
| 3 | C# editor build | PASS 0E/XW / FAIL / NE | Xs | |
| 4 | Unity validators | PASS / FAIL / NOT RUN | — | Phase 2 |
| 5 | Play Mode | PASS / FAIL / NOT RUN | — | Phase 3 — human |

NE = Not Executed (not applicable for this change type)
```

---

## Stop Conditions

- Any C# build error: stop, report, do not claim BUILD_VALIDATED
- Docs validation failure: stop, report
- If Level 4-5 are NOT RUN: explicitly record and do NOT claim UNITY_VALIDATED or ACCEPTED
