# Detect Change Scope Hook
# Analisa o git diff e determina que tipo de mudancas foram feitas
# Saida: .claude/.runtime/change-scope.json

param(
    [switch]$Force
)

# Cria o diretorio de runtime se ele nao existir
$runtimeDir = ".\.claude\.runtime"
if (-not (Test-Path $runtimeDir)) {
    New-Item -ItemType Directory -Path $runtimeDir -Force | Out-Null
}

# Pega os arquivos alterados
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

# Tambem verifica arquivos untracked que foram staged
try {
    $gitStatus = git status --porcelain 2>$null
    if ($gitStatus) {
        $untracked = $gitStatus | Where-Object { $_ -match "^\?\?" } | ForEach-Object { $_.Substring(3) }
        $changedFiles += $untracked
    }
}
catch {
    # Ignora
}

# Inicializa as flags
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

# Analisa cada arquivo alterado
foreach ($file in $changedFiles) {
    # Normaliza os separadores de path
    $file = $file -replace '\\', '/'

    # Pula linhas vazias
    if (-not $file) { continue }

    $scope.changedFiles += $file

    # Verifica mudancas em docs
    if ($file -match '^docs/|^CLAUDE\.md$|^AGENTS\.md$|^PROJECT_LOG\.md$|^\.gitignore$') {
        $scope.docsChanged = $true
    }

    # Verifica mudancas no Unity runtime
    if ($file -match '^Assets/.*\.(cs|unity|prefab|asset)$' -or
        $file -match '^Packages/') {
        $scope.unityRuntimeChanged = $true
    }

    # Verifica mudancas em ProjectSettings
    if ($file -match '^ProjectSettings/') {
        $scope.projectSettingsChanged = $true
    }

    # Mudancas de save system / DTO (testing-quality-gate: save tests required)
    if ($file -match '^Assets/_Game/Scripts/Save/' -or $file -match 'SaveData\.cs$' -or $file -match 'SectionProvider\.cs$') {
        $scope.saveSystemChanged = $true
    }

    # Mudancas de contrato do event bus (testing-quality-gate: contract tests required)
    if ($file -match '^Assets/_Game/Scripts/Core/Events/') {
        $scope.eventContractsChanged = $true
    }

    # Verifica docs_old (sempre proibido)
    if ($file -match '^docs_old/') {
        $scope.forbiddenPathsChanged = $true
    }

    # Verifica recriacao de root specs (sempre proibido)
    if ($file -match '^specs/' -or $file -match '^spec/') {
        $scope.rootSpecsRecreated = $true
        $scope.forbiddenPathsChanged = $true
    }

    # Verifica mudancas em spec docs (nao proibido, mas rastreado)
    if ($file -match '^\.specs/') {
        $scope.specDocsChanged = $true

        # Detecta migracao de spec (a_implementar -> implementados)
        if ($file -match '^\.specs/a_implementar/' -or $file -match '^\.specs/implementados/') {
            $scope.specMigrationDetected = $true
        }
    }

    # Verifica mudancas em refinement docs (nao proibido, mas rastreado)
    if ($file -match '^docs/refinements/') {
        $scope.refinementDocsChanged = $true

        # Detecta migracao de refinement
        if ($file -match '^docs/refinements/(a_implementar/pre_refinamentos|implementados)') {
            $scope.refinementMigrationDetected = $true
        }
    }
}

# Salva em JSON
$jsonPath = "$runtimeDir/change-scope.json"
$scope | ConvertTo-Json | Out-File -FilePath $jsonPath -Encoding UTF8 -Force

# Exibe o resumo
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
