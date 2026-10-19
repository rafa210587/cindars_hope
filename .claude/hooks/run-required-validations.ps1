# Hook Run Required Validations
# Le change-scope.json e roda as validacoes apropriadas
# So roda as validacoes que sao de fato necessarias

param(
    [switch]$Force
)

$runtimeDir = ".\.claude\.runtime"
$scopeFile = "$runtimeDir/change-scope.json"

# Verifica se o scope file existe
if (-not (Test-Path $scopeFile)) {
    Write-Host "ALERT: No scope file found. Run detect-change-scope first or use /implement-spec."
    exit 0
}

# Le o scope
$scope = Get-Content $scopeFile -Raw | ConvertFrom-Json

Write-Host ""
Write-Host "=================================================="
Write-Host "Running Required Validations Based on Change Scope"
Write-Host "=================================================="
Write-Host ""

$validationsPassed = $true
$validationResults = @()

# === DOCS VALIDATION ===
if ($scope.docsChanged) {
    Write-Host "[DOCS] Docs changed - Running docs validation..."
    Write-Host ""

    if (Test-Path ".\tools\docs\validate_docs.ps1") {
        try {
            & ".\tools\docs\validate_docs.ps1" 2>&1
            $docsResult = if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL" }
            Write-Host ""
            Write-Host "[OK] Docs validation: $docsResult"
            $validationResults += @{ "type" = "Docs"; "result" = $docsResult }
        }
        catch {
            Write-Error "[ERROR] Docs validation failed: $_"
            $validationResults += @{ "type" = "Docs"; "result" = "ERROR" }
            $validationsPassed = $false
        }
    }
    else {
        Write-Warning "[WARN] tools/docs/validate_docs.ps1 not found"
        $validationResults += @{ "type" = "Docs"; "result" = "SKIPPED (script not found)" }
    }
}
else {
    Write-Host "[SKIP] Docs unchanged - Skipping docs validation"
    $validationResults += @{ "type" = "Docs"; "result" = "SKIPPED (no changes)" }
}

Write-Host ""

# === UNITY VALIDATION ===
if ($scope.unityRuntimeChanged -or $scope.projectSettingsChanged) {
    Write-Host "[UNITY] Runtime or ProjectSettings changed - Running Unity validation..."
    Write-Host ""

    $unityExePath = "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"

    # Verifica se ha override local
    if (Test-Path ".\.claude\settings.local.json") {
        try {
            $localSettings = Get-Content ".\.claude\settings.local.json" -Raw | ConvertFrom-Json
            if ($localSettings.validation.unityEditorPath) {
                $unityExePath = $localSettings.validation.unityEditorPath
            }
        }
        catch {
            # Ignora erros de parse
        }
    }

    if (Test-Path ".\tools\unity\RunUnityCompileValidation.ps1") {
        Write-Host "  Step 1: Running Unity compile validation..."
        try {
            & ".\tools\unity\RunUnityCompileValidation.ps1" `
                -UnityEditorPath $unityExePath `
                -ProjectPath "." `
                -LogFile ".\Logs\unity-compile-validation.log" 2>&1 | Out-Null

            $compileResult = if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL" }
            Write-Host "  [OK] Compile validation: $compileResult"

            # === LOG SCAN ===
            if (Test-Path ".\tools\unity\ScanUnityLogs.ps1") {
                Write-Host "  Step 2: Scanning logs..."
                try {
                    & ".\tools\unity\ScanUnityLogs.ps1" `
                        -LogFile ".\Logs\unity-compile-validation.log" 2>&1 | Out-Null

                    $scanResult = if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL" }
                    Write-Host "  [OK] Log scan: $scanResult"
                    $validationResults += @{ "type" = "Unity"; "result" = if ($compileResult -eq "PASS" -and $scanResult -eq "PASS") { "PASS" } else { "FAIL" } }
                }
                catch {
                    Write-Error "  [ERROR] Log scan failed: $_"
                    $validationResults += @{ "type" = "Unity"; "result" = "FAIL (log scan error)" }
                    $validationsPassed = $false
                }
            }
            else {
                Write-Warning "  [WARN] ScanUnityLogs script not found"
                $validationResults += @{ "type" = "Unity"; "result" = "PARTIAL (no log scan)" }
            }
        }
        catch {
            Write-Error "  [ERROR] Unity validation failed: $_"
            Write-Host ""
            Write-Host "Unity validation: NOT RUN"
            Write-Host "Reason: $($_.Message)"
            Write-Host "Command attempted: RunUnityCompileValidation.ps1"
            Write-Host "Residual risk: Unity compile not validated locally"
            Write-Host ""
            $validationResults += @{ "type" = "Unity"; "result" = "NOT RUN"; "reason" = $_.Message }
        }
    }
    else {
        Write-Warning "[WARN] tools/unity/RunUnityCompileValidation.ps1 not found"
        $validationResults += @{ "type" = "Unity"; "result" = "SKIPPED (script not found)" }
    }
}
else {
    Write-Host "[SKIP] No Unity runtime/ProjectSettings changes - Skipping Unity validation"
    $validationResults += @{ "type" = "Unity"; "result" = "SKIPPED (no changes)" }
}

Write-Host ""
Write-Host "=================================================="
Write-Host "Validations Summary"
Write-Host "=================================================="
Write-Host ""

foreach ($result in $validationResults) {
    $status = switch ($result.result) {
        "PASS" { "[OK]" }
        "FAIL" { "[FAIL]" }
        "SKIPPED*" { "[SKIP]" }
        "NOT RUN" { "[ALERT]" }
        default { "[INFO]" }
    }
    Write-Host "  $status $($result.type): $($result.result)"
}

Write-Host ""
Write-Host "=================================================="
Write-Host ""

if ($validationsPassed) {
    Write-Host "[OK] All required validations completed."
}
else {
    Write-Host "[FAIL] Some validations failed. Review above and fix."
}

Write-Host ""

# Monta o JSON de resultados de validacao
$validationJson = @{
    "timestamp" = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    "docs" = @{
        "required" = $scope.docsChanged
        "result" = if ($scope.docsChanged) {
            ($validationResults | Where-Object {$_.type -eq "Docs"}).result
        } else {
            "SKIPPED"
        }
        "command" = ".\tools\docs\validate_docs.ps1"
    }
    "unityCompile" = @{
        "required" = $scope.unityRuntimeChanged -or $scope.projectSettingsChanged
        "result" = if ($scope.unityRuntimeChanged -or $scope.projectSettingsChanged) {
            ($validationResults | Where-Object {$_.type -eq "Unity"}).result
        } else {
            "SKIPPED"
        }
        "command" = ".\tools\unity\RunUnityCompileValidation.ps1"
        "logFile" = ".\Logs\unity-compile-validation.log"
        "reason" = if (($validationResults | Where-Object {$_.type -eq "Unity"}).reason) {
            ($validationResults | Where-Object {$_.type -eq "Unity"}).reason
        } else {
            $null
        }
    }
    "unityLogScan" = @{
        "required" = $scope.unityRuntimeChanged -or $scope.projectSettingsChanged
        "result" = if ($scope.unityRuntimeChanged -or $scope.projectSettingsChanged) {
            ($validationResults | Where-Object {$_.type -eq "LogScan"}).result
        } else {
            "SKIPPED"
        }
        "command" = ".\tools\unity\ScanUnityLogs.ps1"
        "reason" = $null
    }
    "overall" = if ($validationsPassed) { "PASS" } else { "FAIL" }
}

# Salva os resultados de validacao
$resultsPath = "$runtimeDir\validation-results.json"
$validationJson | ConvertTo-Json -Depth 10 | Out-File -FilePath $resultsPath -Encoding UTF8 -Force
Write-Host "[SAVE] Validation results saved: $resultsPath"
Write-Host ""

if ($validationsPassed) {
    exit 0
} else {
    exit 1
}
