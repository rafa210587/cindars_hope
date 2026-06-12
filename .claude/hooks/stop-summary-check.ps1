# Stop Summary Check Hook (Stop)
# Claude Code hook protocol: JSON via stdin; exit 0 = allow stop, exit 2 = block stop (stderr -> Claude).
# Intelligent checklist that adapts to the type of changes made.
# Reads change-scope.json (written by detect-change-scope.ps1, which runs first in the Stop event).
# NOTE: ASCII-only on purpose - PowerShell 5.1 misparses UTF-8 scripts without BOM.

# Consume stdin and honor stop_hook_active to avoid infinite stop loops
try {
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    if ($hookInput -and $hookInput.stop_hook_active) {
        exit 0
    }
}
catch {
    # No/malformed stdin: continue as informational hook
}

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

# Forbidden paths: block the stop so Claude addresses the violation before finishing
if ($scope -and ($scope.forbiddenPathsChanged -or $scope.rootSpecsRecreated)) {
    if ($scope.forbiddenPathsChanged) {
        [Console]::Error.WriteLine("STOP-GUARD: changes detected in forbidden paths (docs_old/, specs/, spec/). Revert or move them to canonical paths before finishing (rule: docs-governance).")
    }
    if ($scope.rootSpecsRecreated) {
        [Console]::Error.WriteLine("STOP-GUARD: root specs/ or spec/ directory was created. The only spec source is docs/specs/ - remove it before finishing (rule: spec-lifecycle).")
    }
    exit 2
}

Write-Host ""
Write-Host "==================================================="
Write-Host "Changes detected. Closeout checklist:"
Write-Host "==================================================="
Write-Host ""

# Adaptive checklist based on scope
if ($scope -and ($scope.docsChanged -or $scope.unityRuntimeChanged -or $scope.projectSettingsChanged)) {
    Write-Host "Changes detected in:"
    if ($scope.docsChanged) { Write-Host "   - Documentation" }
    if ($scope.unityRuntimeChanged) { Write-Host "   - Unity runtime (Assets/)" }
    if ($scope.projectSettingsChanged) { Write-Host "   - ProjectSettings/" }
    Write-Host ""

    Write-Host "Required before delivery:"
    Write-Host ""

    if ($scope.docsChanged) {
        Write-Host "   [ ] Docs validation: PASS / WARNING (run: .\tools\docs\validate_docs.ps1)"
    }

    if ($scope.unityRuntimeChanged -or $scope.projectSettingsChanged) {
        Write-Host "   [ ] Unity compile: PASS / NOT RUN with reason"
        Write-Host "   [ ] Log scan: PASS / NOT RUN with reason"
    }

    Write-Host "   [ ] Non-regression: PASS / WARNING (run: /review-non-regression)"
    Write-Host "   [ ] Play Mode features: NOT RUN (sandboxed) - user tests later"

    if ($scope.saveSystemChanged) {
        Write-Host "   [ ] SAVE SYSTEM CHANGED: save tests updated (defaults, null section, invalid ID, round-trip) or justified (rule: testing-quality-gate; skill: save-section-provider)"
    }
    if ($scope.eventContractsChanged) {
        Write-Host "   [ ] EVENT CONTRACTS CHANGED: GameEventBus contract tests cover new/changed events (rule: testing-quality-gate)"
    }
    Write-Host ""

    Write-Host "If spec was implemented:"
    Write-Host "   [ ] Spec moved to docs/specs/implementados/ (only via /finish-spec eligibility)"
    Write-Host "   [ ] Evidence header added (commit, files, validations)"
    Write-Host "   [ ] PROJECT_LOG.md updated"
    Write-Host "   [ ] IMPLEMENTATION_STATUS.md updated"
    Write-Host ""
}
else {
    Write-Host "Deliverables:"
    Write-Host "   [ ] Changed files listed"
    Write-Host "   [ ] Commits summarized"
    Write-Host "   [ ] Scope boundaries clear"
    Write-Host ""
    Write-Host "Validations:"
    Write-Host "   [ ] Appropriate validations executed"
    Write-Host "   [ ] Non-regression reviewed"
    Write-Host ""
    Write-Host "Documentation:"
    Write-Host "   [ ] PROJECT_LOG.md updated (if significant change)"
    Write-Host "   [ ] IMPLEMENTATION_STATUS.md updated (if status changed)"
    Write-Host ""
}

Write-Host "Critical - DO NOT:"
Write-Host "   x Execute git push (awaits user approval)"
Write-Host "   x Open PR/MR (awaits user approval)"
Write-Host "   x Hide validation failures"
Write-Host "   x Claim compliance without evidence"
Write-Host ""
Write-Host "-> If all checks pass, ready to deliver to user"
Write-Host ""
Write-Host "==================================================="
Write-Host ""

exit 0
