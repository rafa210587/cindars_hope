# Hook Docs Status Honesty Check
# Varre arquivos de doc modificados recentemente em busca de claims proibidos de aceitacao prematura.
# Desativado por padrao. Rodar manualmente antes do finish-spec ou em mudancas de arquivos de doc.

$ProhibitedPhrases = @(
    "MVP accepted",
    "100% fulfilled",
    "Play Mode PASS",
    "Phase 3 PASS",
    "Phase 2 PASS",
    "Unity validated",
    "Human acceptance complete",
    "fully accepted"
)

# Marcadores de evidence que tornam a frase aceitavel
$EvidenceMarkers = @(
    "evidence:",
    "executor:",
    "date:",
    "checklist.*PASS",
    "validator.*PASS"
)

# Pega os arquivos de doc alterados recentemente
try {
    $changedFiles = @(git diff --name-only HEAD 2>$null) + @(git diff --cached --name-only 2>$null)
    $changedFiles = $changedFiles | Where-Object { $_ -match "\.(md|txt)$" } | Sort-Object -Unique
}
catch {
    Write-Host "Docs Status Honesty Check: Could not get git diff. Skipping."
    exit 0
}

if ($changedFiles.Count -eq 0) {
    Write-Host "Docs Status Honesty Check: No doc files changed. Skipping."
    exit 0
}

$violations = @()

foreach ($file in $changedFiles) {
    if (-not (Test-Path $file)) { continue }
    $content = Get-Content $file -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { continue }

    foreach ($phrase in $ProhibitedPhrases) {
        if ($content -match $phrase) {
            # Verifica se ha marcadores de evidence por perto
            $hasEvidence = $false
            foreach ($marker in $EvidenceMarkers) {
                if ($content -match $marker) {
                    $hasEvidence = $true
                    break
                }
            }
            if (-not $hasEvidence) {
                $violations += "  $file : '$phrase' - no evidence markers found"
            }
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host ""
    Write-Host "==================================================="
    Write-Host "Docs Status Honesty Check - VIOLATIONS"
    Write-Host "==================================================="
    foreach ($v in $violations) {
        Write-Host $v
    }
    Write-Host ""
    Write-Host "  Rule: no-premature-acceptance-claims.md"
    Write-Host "  Add evidence before making this claim, or use honest phase status."
    Write-Host "==================================================="
    Write-Host ""
    exit 1
}

Write-Host "Docs Status Honesty Check: PASS - no premature claims detected."
exit 0
