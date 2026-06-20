# Avisa quando arquivos de runtime/procedural da cave foram alterados.
# Isto e intencionalmente nao-bloqueante; lembra o agent de aplicar os guardrails da FASE9F.

param(
    [switch]$FailOnMissingValidationDoc
)

$ErrorActionPreference = "Stop"

$changed = @()
try {
    $status = git status --porcelain
    if ($status) {
        $changed = $status |
            ForEach-Object { $_.Substring(3) -replace '\\', '/' } |
            Where-Object {
                $_ -match '^Assets/_Game/Scripts/Cave/' -or
                $_ -match '^Assets/_Game/Scenes/CaveScene\.unity$'
            }
    }
}
catch {
    Write-Warning "Could not inspect git status: $($_.Exception.Message)"
    exit 0
}

if (-not $changed -or $changed.Count -eq 0) {
    Write-Host "[CAVE_GUARD] No cave runtime/procedural changes detected."
    exit 0
}

Write-Warning "[CAVE_GUARD] Cave runtime/procedural changes detected."
Write-Warning "Read before editing/closing:"
Write-Warning "  docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md"
Write-Warning "  docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md"
Write-Warning "  docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md"
Write-Warning "Changed cave files:"
foreach ($file in $changed) {
    Write-Warning "  $file"
}

if ($FailOnMissingValidationDoc) {
    $validationChanged = $false
    try {
        $status = git status --porcelain
        $validationChanged = [bool]($status | Where-Object { $_ -match 'docs/validation/' })
    }
    catch {
        $validationChanged = $false
    }

    if (-not $validationChanged) {
        Write-Error "[CAVE_GUARD] Cave change requires docs/validation evidence."
        exit 1
    }
}

exit 0
