# Test Scenario Required Guard
#
# Verifica se um arquivo de human test scenario existe quando mudancas de runtime/gameplay sao detectadas.
# Le change-scope.json (gerado por detect-change-scope.ps1) e verifica o test scenario correspondente
# em docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md.
#
# Status: DISABLED (manual trigger)
# Rationale: Ajuda a garantir que os deliverables da Phase 3 estejam documentados antes do closeout do /finish-spec

param(
    [string]$SpecId,
    [string]$ChangeScopeJsonPath = "$PSScriptRoot/../change-scope.json"
)

function Test-TestScenarioRequired {
    param(
        [string]$SpecId,
        [string]$ChangeScopeJsonPath
    )

    # Se nenhum change scope foi detectado, pula a verificacao
    if (-not (Test-Path $ChangeScopeJsonPath)) {
        Write-Host "[INFO] test-scenario-required-guard: change-scope.json not found, skipping check" -ForegroundColor Cyan
        return $true
    }

    try {
        $changeScope = Get-Content $ChangeScopeJsonPath | ConvertFrom-Json
    } catch {
        Write-Host "[WARN] test-scenario-required-guard: Could not parse change-scope.json: $_" -ForegroundColor Yellow
        return $true  # Nao bloquear em erro de parse
    }

    # Determina se mudancas de runtime/gameplay foram detectadas
    $isRuntime = $changeScope.runtime_changes -eq $true -or `
                 $changeScope.gameplay_changes -eq $true -or `
                 $changeScope.ui_changes -eq $true -or `
                 $changeScope.cave_changes -eq $true -or `
                 $changeScope.combat_changes -eq $true -or `
                 $changeScope.save_changes -eq $true -or `
                 $changeScope.event_changes -eq $true

    if (-not $isRuntime) {
        Write-Host "[INFO] test-scenario-required-guard: No runtime/gameplay changes detected, test scenario not required" -ForegroundColor Cyan
        return $true
    }

    # Verifica se o arquivo de test scenario existe
    $testScenarioPath = Join-Path (Split-Path $ChangeScopeJsonPath) "docs/05_VALIDATION/playmode/$($SpecId)_human_test_scenario.md"

    if (Test-Path $testScenarioPath) {
        Write-Host "[PASS] test-scenario-required-guard: Test scenario found at $testScenarioPath" -ForegroundColor Green
        return $true
    } else {
        Write-Host "[WARN] test-scenario-required-guard: Runtime/gameplay changes detected but NO test scenario found" -ForegroundColor Yellow
        Write-Host "[WARN]   Expected: $testScenarioPath" -ForegroundColor Yellow
        Write-Host "[WARN]   Action: Invoke /gameplay-test-scenario skill to create test scenario before /finish-spec" -ForegroundColor Yellow
        return $false
    }
}

# Roda o guard
$result = Test-TestScenarioRequired -SpecId $SpecId -ChangeScopeJsonPath $ChangeScopeJsonPath
exit $(if ($result) { 0 } else { 1 })
