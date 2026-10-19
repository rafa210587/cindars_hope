# Stop convenience hook. Persist successful input identity, not git dirty status.
# Always exits 0; failures stay visible and retry on the next invocation.
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $repo
$statePath = Join-Path $repo '.claude/.runtime/sync-inputs.json'
$utf8 = [Text.UTF8Encoding]::new($false)

function Get-InputFingerprint {
    param([string[]]$Paths, [string[]]$Excluded = @())
    $lines = @()
    foreach ($relative in $Paths) {
        $path = Join-Path $repo $relative
        if (-not (Test-Path -LiteralPath $path)) { $lines += "$relative`:MISSING"; continue }
        $files = if ((Get-Item -LiteralPath $path).PSIsContainer) { @(Get-ChildItem -LiteralPath $path -Recurse -File -Force) } else { @(Get-Item -LiteralPath $path) }
        foreach ($file in $files) {
            $name = $file.FullName.Substring($repo.Length + 1).Replace('\','/')
            if ($Excluded -contains $name) { continue }
            $fileSha = [Security.Cryptography.SHA256]::Create()
            try { $lines += $name + ':' + [BitConverter]::ToString($fileSha.ComputeHash([IO.File]::ReadAllBytes($file.FullName))) } finally { $fileSha.Dispose() }
        }
    }
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($sha.ComputeHash($utf8.GetBytes((($lines | Sort-Object) -join "`n")))) } finally { $sha.Dispose() }
}

try {
    $state = @{}
    if (Test-Path -LiteralPath $statePath) {
        try {
            $saved = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
            foreach ($property in $saved.PSObject.Properties) { $state[$property.Name] = $property.Value }
        } catch { Write-Host '[sync-harness] AVISO: estado invalido; recalculando inputs.' }
    }
    $jobs = @(
        @{ Key='harness'; Script='tools/codex/Generate-CodexHarness.ps1'; Paths=@('CLAUDE.md','.claude/HARNESS_INDEX.md','.claude/HARNESS_AUTHORING_STANDARD.md','.claude/skills','.claude/rules','.claude/agents','.claude/commands','.claude/settings.json','.claude/hooks','tools/codex/Generate-CodexHarness.ps1'); Excluded=@() },
        @{ Key='specs'; Script='tools/generate_spec_index.ps1'; Paths=@('.specs','tools/generate_spec_index.ps1'); Excluded=@('.specs/SPEC_INDEX.md','.specs/SPEC_INDEX.json') }
    )
    foreach ($job in $jobs) {
        $before = Get-InputFingerprint -Paths $job.Paths -Excluded $job.Excluded
        if ($state[$job.Key] -eq $before) { continue }
        # Invalidate old success before running so reverted inputs after failure retry too.
        $state.Remove($job.Key)
        Write-Host "[sync-harness] Inputs $($job.Key) mudaram; executando $($job.Script)."
        try {
            & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $job.Script
            if ($LASTEXITCODE -ne 0) { throw "Runner saiu com codigo $LASTEXITCODE" }
            $after = Get-InputFingerprint -Paths $job.Paths -Excluded $job.Excluded
            if ($before -eq $after) { $state[$job.Key] = $before }
            else { Write-Host "[sync-harness] AVISO: inputs $($job.Key) mudaram durante execucao; nova tentativa no proximo Stop." }
        } catch { Write-Host "[sync-harness] AVISO: $($job.Key): $($_.Exception.Message). Nova tentativa no proximo Stop." }
    }
    [void][IO.Directory]::CreateDirectory((Split-Path -Parent $statePath))
    [IO.File]::WriteAllText($statePath, ($state | ConvertTo-Json), $utf8)
} catch { Write-Host "[sync-harness] AVISO: $($_.Exception.Message)" }
exit 0
