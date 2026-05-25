# Run Required Validations Hook
# Reads change-scope.json and runs appropriate validations
# Only runs validations that are actually needed

param(
    [switch]$Force
)

$runtimeDir = ".\.claude\.runtime"
$scopeFile = "$runtimeDir/change-scope.json"

# Check if scope file exists
if (-not (Test-Path $scopeFile)) {
    Write-Host "⚠️  No scope file found. Run detect-change-scope first or use /implement-spec."
    exit 0
}

# Read scope
$scope = Get-Content $scopeFile -Raw | ConvertFrom-Json

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host "Running Required Validations Based on Change Scope"
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host ""

$validationsPassed = $true
$validationResults = @()

# === DOCS VALIDATION ===
if ($scope.docsChanged) {
    Write-Host "📋 Docs changed — Running docs validation..."
    Write-Host ""

    if (Test-Path ".\tools\docs\validate_docs.ps1") {
        try {
            & ".\tools\docs\validate_docs.ps1" 2>&1
            $docsResult = if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL" }
            Write-Host ""
            Write-Host "✅ Docs validation: $docsResult"
            $validationResults += @{ "type" = "Docs"; "result" = $docsResult }
        }
        catch {
            Write-Error "❌ Docs validation failed: $_"
            $validationResults += @{ "type" = "Docs"; "result" = "ERROR" }
            $validationsPassed = $false
        }
    }
    else {
        Write-Warning "⚠️  tools/docs/validate_docs.ps1 not found"
        $validationResults += @{ "type" = "Docs"; "result" = "SKIPPED (script not found)" }
    }
}
else {
    Write-Host "⊘ Docs unchanged — Skipping docs validation"
    $validationResults += @{ "type" = "Docs"; "result" = "SKIPPED (no changes)" }
}

Write-Host ""

# === UNITY VALIDATION ===
if ($scope.unityRuntimeChanged -or $scope.projectSettingsChanged) {
    Write-Host "🎮 Unity runtime or ProjectSettings changed — Running Unity validation..."
    Write-Host ""

    $unityExePath = "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"

    # Check for local override
    if (Test-Path ".\.claude\settings.local.json") {
        try {
            $localSettings = Get-Content ".\.claude\settings.local.json" -Raw | ConvertFrom-Json
            if ($localSettings.validation.unityEditorPath) {
                $unityExePath = $localSettings.validation.unityEditorPath
            }
        }
        catch {
            # Ignore parse errors
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
            Write-Host "  ✅ Compile validation: $compileResult"

            # === LOG SCAN ===
            if (Test-Path ".\tools\unity\ScanUnityLogs.ps1") {
                Write-Host "  Step 2: Scanning logs..."
                try {
                    & ".\tools\unity\ScanUnityLogs.ps1" `
                        -LogFile ".\Logs\unity-compile-validation.log" 2>&1 | Out-Null

                    $scanResult = if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL" }
                    Write-Host "  ✅ Log scan: $scanResult"
                    $validationResults += @{ "type" = "Unity"; "result" = if ($compileResult -eq "PASS" -and $scanResult -eq "PASS") { "PASS" } else { "FAIL" } }
                }
                catch {
                    Write-Error "  ❌ Log scan failed: $_"
                    $validationResults += @{ "type" = "Unity"; "result" = "FAIL (log scan error)" }
                    $validationsPassed = $false
                }
            }
            else {
                Write-Warning "  ⚠️  ScanUnityLogs script not found"
                $validationResults += @{ "type" = "Unity"; "result" = "PARTIAL (no log scan)" }
            }
        }
        catch {
            Write-Error "  ❌ Unity validation failed: $_"
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
        Write-Warning "⚠️  tools/unity/RunUnityCompileValidation.ps1 not found"
        $validationResults += @{ "type" = "Unity"; "result" = "SKIPPED (script not found)" }
    }
}
else {
    Write-Host "⊘ No Unity runtime/ProjectSettings changes — Skipping Unity validation"
    $validationResults += @{ "type" = "Unity"; "result" = "SKIPPED (no changes)" }
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host "Validations Summary"
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host ""

foreach ($result in $validationResults) {
    $status = switch ($result.result) {
        "PASS" { "✅" }
        "FAIL" { "❌" }
        "SKIPPED*" { "⊘" }
        "NOT RUN" { "⚠️" }
        default { "ℹ️" }
    }
    Write-Host "  $status $($result.type): $($result.result)"
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host ""

if ($validationsPassed) {
    Write-Host "✅ All required validations completed."
}
else {
    Write-Host "❌ Some validations failed. Review above and fix."
}

Write-Host ""

exit if ($validationsPassed) { 0 } else { 1 }
