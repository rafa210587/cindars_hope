# Pre-Bash Guard Hook (PreToolUse: Bash|PowerShell)
# Claude Code hook protocol: JSON via stdin; exit 0 = allow, exit 2 = block (stderr -> Claude).
#
# Blocks only patterns that are NEVER acceptable:
#   1. Unity batchmode while another Unity instance is running (rule: no-parallel-unity-batchmode)
#   2. dotnet build piped through filters that hide errors/exit code (rule: validation-truth)
#
# Destructive git (push/reset/clean/stash/rebase) is handled by permissions.ask in
# .claude/settings.json, so the human authorizes per instance — do not duplicate here.

$ErrorActionPreference = "Stop"

try {
    $raw = [Console]::In.ReadToEnd()
    $data = $raw | ConvertFrom-Json
}
catch {
    # Malformed input: do not block the tool because of hook failure
    exit 0
}

$command = $null
if ($data -and $data.tool_input) { $command = $data.tool_input.command }
if (-not $command) { exit 0 }

# --- 1. No parallel Unity batchmode ---------------------------------------
if ($command -match "Unity(\.exe)?['""]?\s+.*-batchmode" -or $command -match "Unity\.exe.*-batchmode") {
    $unityProcesses = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
    if ($unityProcesses) {
        [Console]::Error.WriteLine("BLOCKED: Unity batchmode while another Unity process is running ($($unityProcesses.Count) found).")
        [Console]::Error.WriteLine("Rule: no-parallel-unity-batchmode. Close the Unity Editor or record the validation as BLOCKED with reason 'Unity lock'.")
        exit 2
    }
}

# --- 2. Build Validation Truth Gate ----------------------------------------
# 'dotnet build | Select-String' (and similar filters) lose $LASTEXITCODE and hide errors.
if ($command -match "dotnet\s+build[^|;]*\|\s*(Select-String|Out-String|Tee-Object|Out-Null|findstr)") {
    [Console]::Error.WriteLine("BLOCKED: 'dotnet build' piped through an output filter. This loses the exit code and hides errors (rule: validation-truth / Build Validation Truth Gate).")
    [Console]::Error.WriteLine("Required pattern: run 'dotnet build <proj> --no-restore' bare, then check `$LASTEXITCODE -ne 0`. Preferred: .\tools\docs\run_strict_validation.ps1")
    exit 2
}

exit 0
