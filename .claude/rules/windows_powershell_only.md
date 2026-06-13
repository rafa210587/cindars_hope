# Rule: Windows / PowerShell Only

This project is executed on Windows with PowerShell. No Unix/Bash commands.

## Required Shell

All agent execution must use PowerShell syntax only (`pwsh` or `powershell`).

## Forbidden Unix/Bash Commands

Do NOT use in any agent task:

```text
head
tail
ls
find
grep
cat
pwd
cd d:/...
bash pipes assuming Unix tools
```

These fail on Windows or produce incorrect behavior.

## PowerShell Equivalents

Use these instead:

| Need | Unix/Bash | PowerShell |
|---|---|---|
| go to repo | `cd d:/path` | `Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'` |
| first N lines | `head -50 file` | `Get-Content file \| Select-Object -First 50` |
| last N lines | `tail -50 file` | `Get-Content file \| Select-Object -Last 50` |
| list files | `ls -la /path` | `Get-ChildItem /path -Recurse` |
| find files | `find . -name '*.cs'` | `Get-ChildItem -Recurse -Filter *.cs` |
| grep text | `grep -r "pattern" .` | `Select-String -Path . -Pattern "pattern" -Recurse` |
| first lines of git | `git status \| head -20` | `git status --short \| Select-Object -First 20` |
| all specs in wave | `ls .specs/a_implementar/fable/fable_*.md` | `Get-ChildItem .\.specs\a_implementar\fable\fable_*.md \| Sort-Object Name` |
| check if file exists | `test -f path` | `Test-Path path` |
| file content | `cat file.md` | `Get-Content file.md` |

## Environment Failure Policy

If a command fails because of shell mismatch (Unix syntax on Windows):

1. **Do not mark the spec BLOCKED immediately.**
2. Mark the step as `ENV_COMMAND_RETRY_REQUIRED`.
3. Retry once using the PowerShell equivalent.
4. Only if PowerShell retry **also fails**, classify as `ENV_COMMAND_FAILURE`.
5. `ENV_COMMAND_FAILURE` is **not** a spec failure unless fundamental, e.g., missing file or logic error.

## Required Preflight (Every Spec)

Before executing any spec, run:

```powershell
Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
git status --short | Select-Object -First 50
git branch --show-current
```

Do not proceed without confirming:
- ✓ Correct directory
- ✓ Correct branch (`dev`)
- ✓ Expected uncommitted state

## PowerShell Exit Code Rule

**When using PowerShell, always check `$LASTEXITCODE` after external commands.**

Never rely on filtered output to infer success:

```powershell
# ❌ FORBIDDEN
dotnet build ... | Select-String "error"
# Exit code is lost; success/failure unknown

# ✓ REQUIRED
dotnet build ...
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed"
    exit 1
}
```

This applies to:
- `dotnet build`
- `dotnet test`
- `git` commands
- PowerShell scripts
- Any external executable

---

## Fallback Rule

If an agent uses Unix syntax on Windows, the human executing the task may see failures. The execution report must document:

```text
Command retry:
  Original: head -50 file.md
  Failed: [error message]
  Retry: Get-Content file.md | Select-Object -First 50
  Result: [success/failure]
```

If the agent used bash and did not retry in PowerShell, this is **not** an agent error — but the task may not have run.

---

*Created: 2026-06-08 (Windows Harness)*  
*Applies to all agent-run spec execution and validation tasks.*
