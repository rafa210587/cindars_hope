---
name: unity-validation-triage
description: Classify Unity, dotnet, and log validation failures without hiding real compile errors
version: 1.0
---

# Unity Validation Triage

Use this skill whenever Unity batchmode, `dotnet build`, or Unity log scanning fails.

## Goal

Separate code failures from environment/tooling failures before reporting status.

## Required Inputs

- Command attempted.
- Exit code.
- Log file path, when present.
- Key output lines.
- Whether `dotnet build .\Assembly-CSharp.csproj --no-restore` was attempted.
- Whether `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` was attempted.

## Classification Rules

### Real Compile Failure

Treat as code failure if any current log contains:

```text
error CS
error Unity
error NETSDK
```

Exception: `NETSDK1004 project.assets.json not found` is a restore/setup issue if fixed by `dotnet restore`.

Action:

1. Fix the code.
2. Re-run the same validation.
3. Do not close the spec while real compile errors remain.

### Local `.csproj` Drift

Likely if `dotnet build` reports missing types for files just added, and Unity has not regenerated the project files.

Action:

1. Check whether the new `.cs` file is listed in `Assembly-CSharp.csproj` or `Assembly-CSharp-Editor.csproj`.
2. Add local compile includes only when needed for validation in this repo.
3. Re-run `dotnet build`.
4. Mention `.csproj` drift in validation notes if touched.

### Unity Already Open

Likely if log/output contains:

```text
another Unity instance is running with this project open
Multiple Unity instances cannot open the same project
```

Action:

1. Do not call this a compile failure.
2. Record Unity validation as blocked by editor lock.
3. If authorized by the human/project protocol, close stale Unity and rerun.
4. Otherwise leave Play Mode/Editor validation pending.

### License / Hub / Package Noise

Do not call failure a code compile failure if the log has no `error CS` and only contains:

```text
license
Package Manager
Assembly-CSharp-firstpass.dll not valid
EditorTests.dll not valid
Package tests assembly not valid
```

Action:

1. Record as environment/tooling blocker.
2. Prefer `dotnet build` as compile fallback.
3. Keep Unity Play Mode validation pending.

### Sandbox / Permission

Likely if output contains:

```text
Access to the path is denied
couldn't create signal pipe
cannot write Temp\obj
```

Action:

1. Re-run the same necessary command with approved escalation if policy allows.
2. If still blocked, record exact path and command.

## Reporting Template

```text
Validation triage:
- Command: <command>
- Exit code: <code>
- Classification: <real compile failure | csproj drift | Unity lock | tooling noise | permission>
- Evidence: <short key line>
- Follow-up: <fixed/rerun/pending>
```

## Rules

- Do not flatten all validation failures into "build failed".
- Do not claim Unity compile passed from `dotnet build` alone.
- Do not ignore `error CS`.
- Do not hide blocked Unity validation; document it as pending.
