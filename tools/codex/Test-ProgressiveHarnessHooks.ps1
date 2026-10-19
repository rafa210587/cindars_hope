param()
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\','/')
$fixture = Join-Path $tempParent ('cindars-hooks-test-' + [guid]::NewGuid().ToString('N'))
$utf8 = [Text.UTF8Encoding]::new($false)
$count = 0
function Put([string]$Path, [string]$Text) {
    $dest = Join-Path $fixture $Path
    [void][IO.Directory]::CreateDirectory((Split-Path -Parent $dest))
    [IO.File]::WriteAllText($dest, $Text, $utf8)
}
function Check([bool]$Value, [string]$Name) { if (-not $Value) { throw $Name }; $script:count++ }
function Run([string]$Script) {
    $p = [Diagnostics.Process]::new()
    $p.StartInfo.FileName = (Get-Command powershell.exe).Source
    $p.StartInfo.Arguments = '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + (Join-Path $fixture $Script) + '"'
    $p.StartInfo.WorkingDirectory = $fixture
    $p.StartInfo.UseShellExecute = $false
    $p.StartInfo.CreateNoWindow = $true
    $p.StartInfo.RedirectStandardOutput = $true
    $p.StartInfo.RedirectStandardError = $true
    $p.StartInfo.RedirectStandardInput = $true
    try {
        [void]$p.Start(); $p.StandardInput.WriteLine('{}'); $p.StandardInput.Close()
        $o = $p.StandardOutput.ReadToEndAsync(); $e = $p.StandardError.ReadToEndAsync()
        $p.WaitForExit()
        return @{Code=$p.ExitCode; Text=$o.Result+$e.Result}
    } finally { $p.Dispose() }
}
function Runs([string]$Name) { return @(Get-Content -LiteralPath (Join-Path $fixture "$Name.runs")).Count }
try {
    foreach ($hook in @('sync-harness-and-tracing','stop-summary-check')) { Put ".claude/hooks/$hook.ps1" ([IO.File]::ReadAllText((Join-Path $repo ".claude/hooks/$hook.ps1"))) }
    Put '.claude/skills/probe/SKILL.md' 'Version one'
    Put '.specs/probe.md' 'Spec one'
    Put 'tools/codex/Generate-CodexHarness.ps1' @'
Add-Content harness.runs 'run'
if (Test-Path fail.flag) { Write-Output 'WARNING fixture failure'; exit 7 }
if (Test-Path mutate.flag) { Add-Content .claude/skills/probe/SKILL.md 'changed'; Remove-Item mutate.flag }
Write-Output 'WARNING fixture retained'
exit 0
'@
    Put 'tools/generate_spec_index.ps1' @'
Add-Content specs.runs 'run'
Set-Content .specs/SPEC_INDEX.md ([guid]::NewGuid())
Set-Content .specs/SPEC_INDEX.json '{}'
exit 0
'@
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ($r.Code -eq 0 -and $r.Text.Contains('WARNING fixture retained')) ('runner warning retained: ' + $r.Text)
    Check ((Runs harness) -eq 1 -and (Runs specs) -eq 1) 'first run generates both'
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ((Runs harness) -eq 1 -and (Runs specs) -eq 1) 'identical inputs including generated specs skip'
    Put '.claude/skills/probe/references/deep/check.md' 'new support'
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ((Runs harness) -eq 2 -and (Runs specs) -eq 1) 'nested reference invalidates only harness'
    Put 'fail.flag' 'fail'
    Put '.claude/skills/probe/SKILL.md' 'Version two'
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ($r.Code -eq 0 -and $r.Text.Contains('codigo 7')) 'failure visible without blocking Stop'
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ((Runs harness) -eq 4) 'unchanged failed inputs retry'
    Remove-Item -LiteralPath (Join-Path $fixture 'fail.flag')
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ((Runs harness) -eq 5) 'successful retry cached'
    Put '.claude/.runtime/sync-inputs.json' '{'
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ($r.Text.Contains('estado invalido') -and (Runs harness) -eq 6) 'corrupt cache warns and rebuilds'
    Put 'mutate.flag' 'mutate'
    Put '.claude/skills/probe/SKILL.md' 'Version three'
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ($r.Text.Contains('mudaram durante execucao')) 'mid-run mutation not cached'
    $r = Run '.claude/hooks/sync-harness-and-tracing.ps1'
    Check ((Runs harness) -eq 8) 'mid-run mutation retried'
    Put '.claude/.runtime/change-scope.json' '{"changedFileCount":1,"docsChanged":true,"unityRuntimeChanged":false}'
    $r = Run '.claude/hooks/stop-summary-check.ps1'
    Check ($r.Code -eq 0 -and $r.Text -notmatch 'Play Mode:|Log scan:') 'docs scope has no Unity checklist'
    Put '.claude/.runtime/change-scope.json' '{"changedFileCount":1,"unityRuntimeChanged":true}'
    $r = Run '.claude/hooks/stop-summary-check.ps1'
    Check ($r.Text.Contains('execucao/evidencia real') -and $r.Text -notmatch 'NOT RUN \(sandboxed\)') 'runtime requires actual results'
    Put '.claude/.runtime/change-scope.json' '{"changedFileCount":1,"forbiddenPathsChanged":true}'
    $r = Run '.claude/hooks/stop-summary-check.ps1'
    Check ($r.Code -eq 2) 'forbidden scope still blocks'
    Put '.claude/.runtime/change-scope.json' '{"changedFileCount":0}'
    $r = Run '.claude/hooks/stop-summary-check.ps1'
    Check ($r.Code -eq 0 -and [string]::IsNullOrWhiteSpace($r.Text)) 'empty scope silent without git scan'
    Write-Output "PROGRESSIVE_HOOK_TESTS: PASS ($count assertions; isolated fixtures)"
} finally {
    $resolved = [IO.Path]::GetFullPath($fixture)
    if (-not $resolved.StartsWith($tempParent + [IO.Path]::DirectorySeparatorChar + 'cindars-hooks-test-', [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
