# Stop Summary Check Hook (Stop)
# Protocolo de hook do Claude Code: JSON via stdin; exit 0 = allow stop, exit 2 = block stop (stderr -> Claude).
# Checklist inteligente que se adapta ao tipo de mudancas feitas.
# Le change-scope.json (escrito por detect-change-scope.ps1, que roda primeiro no evento Stop).
# NOTA: ASCII-only de proposito - PowerShell 5.1 interpreta errado scripts UTF-8 sem BOM.

# Consome stdin e respeita stop_hook_active para evitar loops infinitos de stop
try {
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    if ($hookInput -and $hookInput.stop_hook_active) {
        exit 0
    }
}
catch {
    # Sem stdin ou stdin malformado: continua como hook informativo
}

# Le o scope se ele existir
$scopeFile = ".\.claude\.runtime\change-scope.json"
$scope = $null
if (Test-Path $scopeFile) {
    try {
        $scope = Get-Content $scopeFile -Raw | ConvertFrom-Json
    }
    catch {
        # Ignora erros de parse
    }
}

# O detector anterior possui o inventario; nao repetir scans git neste hook.
if ($scope -and $scope.changedFileCount -eq 0) { exit 0 }

# Paths proibidos: bloqueia o stop para Claude tratar a violacao antes de finalizar
if ($scope -and ($scope.forbiddenPathsChanged -or $scope.rootSpecsRecreated)) {
    if ($scope.forbiddenPathsChanged) {
        [Console]::Error.WriteLine("STOP-GUARD: changes detected in forbidden paths (docs_old/, specs/, spec/). Revert or move them to canonical paths before finishing (rule: docs-governance).")
    }
    if ($scope.rootSpecsRecreated) {
        [Console]::Error.WriteLine("STOP-GUARD: root specs/ or spec/ directory was created. The only spec source is .specs/ - remove it before finishing (rule: spec-lifecycle).")
    }
    exit 2
}

Write-Host ""
Write-Host "==================================================="
Write-Host "Changes detected. Closeout checklist:"
Write-Host "==================================================="
Write-Host ""

# Checklist adaptativo baseado no scope
if ($scope -and ($scope.docsChanged -or $scope.unityRuntimeChanged -or $scope.projectSettingsChanged)) {
    Write-Host "Changes detected in:"
    if ($scope.docsChanged) { Write-Host "   - Documentation" }
    if ($scope.unityRuntimeChanged) { Write-Host "   - Unity runtime (Assets/)" }
    if ($scope.projectSettingsChanged) { Write-Host "   - ProjectSettings/" }
    Write-Host ""

    Write-Host "Required before delivery:"
    Write-Host ""

    if ($scope.docsChanged) {
        Write-Host "   [ ] Docs: registrar resultado real e evidencia do gate aplicavel; reusar inputs equivalentes"
    }

    if ($scope.unityRuntimeChanged -or $scope.projectSettingsChanged) {
        Write-Host "   [ ] Unity compile/testes: resultado real (PASS / FAIL / NOT RUN + motivo) conforme matriz"
        Write-Host "   [ ] Log scan: conferir evidencia incorporada ao runner; nao repetir scan no mesmo log"
    }

    Write-Host "   [ ] Non-regression: resultado real e findings; revisao proporcional ao risco"
    if ($scope.unityRuntimeChanged -or $scope.projectSettingsChanged) { Write-Host "   [ ] Play Mode: registrar execucao/evidencia real ou NOT RUN com motivo e risco; nunca presumir resultado" }

    if ($scope.saveSystemChanged) {
        Write-Host "   [ ] SAVE SYSTEM CHANGED: save tests updated (defaults, null section, invalid ID, round-trip) or justified (rule: testing-quality-gate; skill: save-section-provider)"
    }
    if ($scope.eventContractsChanged) {
        Write-Host "   [ ] EVENT CONTRACTS CHANGED: GameEventBus contract tests cover new/changed events (rule: testing-quality-gate)"
    }
    Write-Host ""

    Write-Host "If spec was implemented:"
    Write-Host "   [ ] Status da spec conferido por /finish-spec; promover somente se elegivel com evidencia"
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
Write-Host "-> Reportar gates aplicaveis, falhas e pendencias sem declarar aprovacao automatica"
Write-Host ""
Write-Host "==================================================="
Write-Host ""

exit 0
