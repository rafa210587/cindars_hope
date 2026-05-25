# Stop Summary Check Hook
# Intelligent checklist that adapts to the type of changes made
# Reads change-scope.json to determine what validations should be done

# Check if there were any changes at all
try {
    $changedFiles = @(git diff --name-only 2>$null)
}
catch {
    # If git fails, just skip (e.g., in non-repo context)
    exit 0
}

if ($changedFiles.Count -eq 0 -and -not (git status --porcelain 2>$null)) {
    # No changes - skip this hook
    exit 0
}

# Read scope if it exists
$scopeFile = ".\.claude\.runtime\change-scope.json"
$scope = $null
if (Test-Path $scopeFile) {
    try {
        $scope = Get-Content $scopeFile -Raw | ConvertFrom-Json
    }
    catch {
        # Ignore parse errors
    }
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host "✓ Changes detected. Closeout checklist:"
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host ""

# Adaptive checklist based on scope
if ($scope -and ($scope.docsChanged -or $scope.unityRuntimeChanged -or $scope.projectSettingsChanged)) {
    Write-Host "📊 Changes detected in:"
    if ($scope.docsChanged) { Write-Host "   • Documentation" }
    if ($scope.unityRuntimeChanged) { Write-Host "   • Unity runtime (Assets/)" }
    if ($scope.projectSettingsChanged) { Write-Host "   • ProjectSettings/" }
    Write-Host ""

    Write-Host "📋 Required before delivery:"
    Write-Host ""

    if ($scope.docsChanged) {
        Write-Host "   □ Docs validation: PASS / WARNING (run: .\tools\docs\validate_docs.ps1)"
    }

    if ($scope.unityRuntimeChanged -or $scope.projectSettingsChanged) {
        Write-Host "   □ Unity compile: PASS / NOT RUN with reason"
        Write-Host "   □ Log scan: PASS / NOT RUN with reason"
    }

    Write-Host "   □ Non-regression: PASS / WARNING (run: /review-non-regression)"
    Write-Host "   □ Play Mode features: NOT RUN (sandboxed) — user tests later"
    Write-Host ""

    Write-Host "📝 If spec was implemented:"
    Write-Host "   □ Spec moved to docs/specs/implementados/"
    Write-Host "   □ Evidence header added (commit, files, validations)"
    Write-Host "   □ PROJECT_LOG.md updated"
    Write-Host "   □ IMPLEMENTATION_STATUS.md updated"
    Write-Host ""
}
else {
    Write-Host "📋 Deliverables:"
    Write-Host "   □ Changed files listed"
    Write-Host "   □ Commits summarized"
    Write-Host "   □ Scope boundaries clear"
    Write-Host ""
    Write-Host "✅ Validations:"
    Write-Host "   □ Appropriate validations executed"
    Write-Host "   □ Non-regression reviewed"
    Write-Host ""
    Write-Host "📝 Documentation:"
    Write-Host "   □ PROJECT_LOG.md updated (if significant change)"
    Write-Host "   □ IMPLEMENTATION_STATUS.md updated (if status changed)"
    Write-Host ""
}

Write-Host "⚠️  Critical — DO NOT:"
Write-Host "   ✗ Execute git push (awaits user approval)"
Write-Host "   ✗ Open PR/MR (awaits user approval)"
Write-Host "   ✗ Hide validation failures"
Write-Host "   ✗ Claim compliance without evidence"
Write-Host ""

if ($scope -and ($scope.forbiddenPathsChanged -or $scope.rootSpecsRecreated)) {
    Write-Host "🚨 ALERT:"
    if ($scope.forbiddenPathsChanged) {
        Write-Host "   ❌ Changes detected in forbidden paths (docs_old, specs/, spec/)"
    }
    if ($scope.rootSpecsRecreated) {
        Write-Host "   ❌ Root specs/ or spec/ directory was created (must be removed)"
    }
    Write-Host ""
}

Write-Host "→ If all checks ✓, ready to deliver to user"
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host ""

exit 0
