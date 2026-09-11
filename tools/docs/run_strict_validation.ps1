<#
.SYNOPSIS
Executa os gates do projeto com exit codes isolados dos logs.
.DESCRIPTION
Cada script roda em um processo PowerShell filho. Falha de docs, build, ratchet,
qualidade, arquivo ausente ou exception interrompe a sequencia e retorna exit 1.
#>
param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    [string[]]$Gates,
    [string]$ScopePath = ''
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$powerShellPath = (Get-Command powershell.exe -ErrorAction Stop).Source

function Invoke-ValidationStep {
    param([string]$Name, [string]$RelativePath, [string]$Id)

    Write-Host "=== $Name ==="
    $scriptPath = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
        Write-Host "VALIDATION_SCRIPT_MISSING: $RelativePath"
        return 1
    }

    # Native stderr tambem e log; o exit code do processo define sucesso/falha.
    # Nao retornar stdout pela success stream junto com o resultado numerico.
    $previousPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $stepArguments = @('-NoProfile','-NonInteractive','-ExecutionPolicy','Bypass','-File',$scriptPath)
        if ($ScopePath -and $Id -in @('quality','diff')) { $stepArguments += @('-ScopePath',$ScopePath) }
        & $powerShellPath @stepArguments 2>&1 |
            ForEach-Object { Write-Host $_ }
        $stepExitCode = $LASTEXITCODE
    } catch {
        Write-Host "VALIDATION_PROCESS_FAILURE: $Name"
        return 1
    } finally {
        $ErrorActionPreference = $previousPreference
    }
    if ($null -eq $stepExitCode -or $stepExitCode -ne 0) {
        Write-Host "VALIDATION_STEP_FAILED: $Name (exit $stepExitCode)"
        return 1
    }
    Write-Host "VALIDATION_STEP_PASS: $Name"
    return 0
}

$steps = @(
    @{ Id = 'corruption'; Name = 'corruption guard'; Path = 'tools/validate_no_corruption.ps1'; Failure = 'CORRUPTION_DETECTED' },
    @{ Id = 'docs'; Name = 'docs validation'; Path = 'tools/docs/validate_docs.ps1'; Failure = 'DOCS_VALIDATION_FAILURE' },
    @{ Id = 'build'; Name = 'Unity generated-project builds'; Path = 'tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1'; Failure = 'UNITY_PROJECT_BUILD_FAILURE' },
    @{ Id = 'architecture'; Name = 'architecture ratchet'; Path = 'tools/architecture/Test-ArchitectureRatchet.ps1'; Failure = 'ARCHITECTURE_RATCHET_FAILURE' },
    @{ Id = 'diff'; Name = 'diff completeness'; Path = 'tools/docs/check_spec_diff_completeness.ps1'; Failure = 'DIFF_COMPLETENESS_FAILURE' },
    @{ Id = 'quality'; Name = 'quality check'; Path = 'tools/docs/check_spec_quality.ps1'; Failure = 'QUALITY_CHECK_FAILURE' }
)

$scoped = $PSBoundParameters.ContainsKey('Gates')
if ($scoped) {
    # -File do PowerShell aceita uma string CSV; chamadas in-process podem passar array.
    $Gates = @($Gates | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() })
    if ($Gates.Count -eq 0 -or @($Gates | Where-Object { $_ -notin $steps.Id }).Count -gt 0) {
        Write-Host 'STRICT_VALIDATION_RESULT: INVALID_GATES'; exit 1
    }
    $steps = @($steps | Where-Object { $_.Id -in $Gates })
}
if ($ScopePath) {
    if (-not $scoped) { Write-Host 'STRICT_VALIDATION_RESULT: SCOPE_REQUIRES_EXPLICIT_GATES'; exit 1 }
    try {
        $ScopePath = (Resolve-Path -LiteralPath $ScopePath).Path
        . (Join-Path $PSScriptRoot 'ValidationScope.ps1')
        $null = Read-ValidationScope $ScopePath $ProjectRoot
    } catch { Write-Host "STRICT_VALIDATION_RESULT: INVALID_SCOPE; $($_.Exception.Message)"; exit 1 }
}

Write-Host 'STRICT_VALIDATION_HARNESS'
Write-Host "Validation mode: $(if ($scoped) { 'SCOPED' } else { 'GLOBAL' }); gates: $($steps.Id -join ',')"
Push-Location -LiteralPath $ProjectRoot
try {
    foreach ($step in $steps) {
        $stepResult = Invoke-ValidationStep -Name $step.Name -RelativePath $step.Path -Id $step.Id
        if ($stepResult -ne 0) {
            Write-Host "STRICT_VALIDATION_RESULT: $($step.Failure)"
            exit 1
        }
    }
    if ($scoped) { Write-Host 'STRICT_VALIDATION_RESULT: SCOPED_PASS (not a global result)' }
    else { Write-Host 'STRICT_VALIDATION_RESULT: GLOBAL_PASS (VALIDATION_PASS)' }
    exit 0
} finally {
    Pop-Location
}
