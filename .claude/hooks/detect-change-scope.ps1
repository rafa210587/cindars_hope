# Detect Change Scope Hook
# Analyzes git diff and determines what type of changes were made
# Outputs: .claude/.runtime/change-scope.json

param(
    [switch]$Force
)

# Create runtime directory if it doesn't exist
$runtimeDir = ".\.claude\.runtime"
if (-not (Test-Path $runtimeDir)) {
    New-Item -ItemType Directory -Path $runtimeDir -Force | Out-Null
}

# Get changed files
$changedFiles = @()
try {
    $gitDiff = git diff --name-only 2>$null
    if ($gitDiff) {
        $changedFiles = $gitDiff -split "`n" | Where-Object { $_ -match '\S' }
    }
}
catch {
    Write-Warning "Could not read git diff"
}

# Also check for untracked files that were staged
try {
    $gitStatus = git status --porcelain 2>$null
    if ($gitStatus) {
        $untracked = $gitStatus | Where-Object { $_ -match "^\?\?" } | ForEach-Object { $_.Substring(3) }
        $changedFiles += $untracked
    }
}
catch {
    # Ignore
}

# Initialize flags
$scope = @{
    "timestamp" = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    "docsChanged" = $false
    "unityRuntimeChanged" = $false
    "saveSystemChanged" = $false
    "eventContractsChanged" = $false
    "projectSettingsChanged" = $false
    "forbiddenPathsChanged" = $false
    "rootSpecsRecreated" = $false
    "specDocsChanged" = $false
    "specMigrationDetected" = $false
    "refinementDocsChanged" = $false
    "refinementMigrationDetected" = $false
    "changedFileCount" = $changedFiles.Count
    "changedFiles" = @()
}

# Analyze each changed file
foreach ($file in $changedFiles) {
    # Normalize path separators
    $file = $file -replace '\\', '/'

    # Skip empty lines
    if (-not $file) { continue }

    $scope.changedFiles += $file

    # Check for docs changes
    if ($file -match '^docs/|^CLAUDE\.md$|^AGENTS\.md$|^PROJECT_LOG\.md$|^\.gitignore$') {
        $scope.docsChanged = $true
    }

    # Check for Unity runtime changes
    if ($file -match '^Assets/.*\.(cs|unity|prefab|asset)$' -or
        $file -match '^Packages/') {
        $scope.unityRuntimeChanged = $true
    }

    # Check for ProjectSettings changes
    if ($file -match '^ProjectSettings/') {
        $scope.projectSettingsChanged = $true
    }

    # Save system / DTO changes (testing-quality-gate: save tests required)
    if ($file -match '^Assets/_Game/Scripts/Save/' -or $file -match 'SaveData\.cs$' -or $file -match 'SectionProvider\.cs$') {
        $scope.saveSystemChanged = $true
    }

    # Event bus contract changes (testing-quality-gate: contract tests required)
    if ($file -match '^Assets/_Game/Scripts/Core/Events/') {
        $scope.eventContractsChanged = $true
    }

    # Check for docs_old (always forbidden)
    if ($file -match '^docs_old/') {
        $scope.forbiddenPathsChanged = $true
    }

    # Check for root specs recreation (always forbidden)
    if ($file -match '^specs/' -or $file -match '^spec/') {
        $scope.rootSpecsRecreated = $true
        $scope.forbiddenPathsChanged = $true
    }

    # Check for spec docs changes (not forbidden, but tracked)
    if ($file -match '^\.specs/') {
        $scope.specDocsChanged = $true

        # Detect spec migration (a_implementar → implementados)
        if ($file -match '^\.specs/a_implementar/' -or $file -match '^\.specs/implementados/') {
            $scope.specMigrationDetected = $true
        }
    }

    # Check for refinement docs changes (not forbidden, but tracked)
    if ($file -match '^docs/refinements/') {
        $scope.refinementDocsChanged = $true

        # Detect refinement migration
        if ($file -match '^docs/refinements/(a_implementar/pre_refinamentos|implementados)') {
            $scope.refinementMigrationDetected = $true
        }
    }
}

# Save to JSON
$jsonPath = "$runtimeDir/change-scope.json"
$scope | ConvertTo-Json | Out-File -FilePath $jsonPath -Encoding UTF8 -Force

# Display summary
Write-Host "[SCOPE] Change Scope Detected:"
Write-Host ""
Write-Host "  Files changed: $($scope.changedFileCount)"
Write-Host "  Docs changed: $($scope.docsChanged)"
Write-Host "  Unity runtime changed: $($scope.unityRuntimeChanged)"
Write-Host "  ProjectSettings changed: $($scope.projectSettingsChanged)"
Write-Host "  Spec docs changed: $($scope.specDocsChanged)"
Write-Host "  Spec migration detected: $($scope.specMigrationDetected)"
Write-Host "  Refinement docs changed: $($scope.refinementDocsChanged)"
Write-Host "  Refinement migration detected: $($scope.refinementMigrationDetected)"
Write-Host "  Forbidden paths (docs_old/specs/spec/): $($scope.forbiddenPathsChanged)"
Write-Host "  Root specs recreated: $($scope.rootSpecsRecreated)"
Write-Host ""

if ($scope.forbiddenPathsChanged -or $scope.rootSpecsRecreated) {
    Write-Warning "ALERT: Forbidden paths detected! Non-regression check will catch this."
    Write-Host ""
}

Write-Host "  Saved: $jsonPath"
Write-Host ""

exit 0
