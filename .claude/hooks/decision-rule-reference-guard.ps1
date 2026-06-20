# Hook: decision-rule-reference-guard
# Avisa se specs citam amendments como canonico ou omitem campos required de ADR/game_rules

param(
    [switch]$FailOnWarning,
    [string]$ChangedFilesPath
)

$warningCount = 0
$errorCount = 0

# Encontra specs alteradas em a_implementar
$specs = Get-ChildItem -Path ".specs/a_implementar" -Filter "*.md" -Recurse | Select-Object -ExpandProperty FullName

foreach ($spec in $specs) {
    $content = Get-Content -Path $spec -Raw

    # Check 1: Spec cita amendments como fonte canonica
    if ($content -match 'docs/amendments/' -and $content -notmatch 'archived|historical') {
        Write-Warning "[$spec] References amendment as active source (not archived): check if content should be migrated to ADRs/game_rules"
        $warningCount++
    }

    # Check 2: Spec sem os campos required_adrs e required_game_rules
    if ($content -notmatch 'required_adrs:' -or $content -notmatch 'required_game_rules:') {
        Write-Warning "[$spec] Missing required_adrs or required_game_rules fields in frontmatter"
        $warningCount++
    }
}

# Encontra qualquer documento fora de templates que crie regras canonicas
$activeDocs = Get-ChildItem -Path ".specs/a_implementar", ".specs/implementados", "docs/project", "docs/refinements" -Filter "*.md" -Recurse 2>$null | Select-Object -ExpandProperty FullName
foreach ($doc in $activeDocs) {
    $content = Get-Content -Path $doc -Raw

    # Check 3: Rule definida fora dos locais canonicos
    if ($content -match '##\s+(Rule|Current|Canonical)' -and
        $doc -notmatch 'docs/decisions/' -and
        $doc -notmatch 'docs/game_rules/' -and
        $doc -notmatch '_templates') {
        Write-Warning "[$doc] Defines canonical rule outside docs/decisions or docs/game_rules"
        $warningCount++
    }
}

# Check 4: Amendments arquivados citados fora do historico de validation
$refDocs = @("docs/project/CURRENT_STATE.md", "docs/project/DOCUMENT_INDEX.md", "docs/project/DOCUMENT_GOVERNANCE.md", "docs/project/DECISION_LOG.md")
foreach ($doc in $refDocs) {
    if (Test-Path $doc) {
        $content = Get-Content -Path $doc -Raw
        if ($content -match 'docs/amendments/[^/]+\.md' -and $content -notmatch 'archived|historical') {
            Write-Warning "[$doc] References amendment as canonical (should be archived/historical)"
            $warningCount++
        }
    }
}

# Reporta resultados
Write-Host ""
Write-Host "Decision/Game Rule Reference Guard Results:"
Write-Host "  Warnings: $warningCount"
Write-Host "  Errors: $errorCount"
Write-Host ""

if ($FailOnWarning -and $warningCount -gt 0) {
    exit 1
}

exit 0
