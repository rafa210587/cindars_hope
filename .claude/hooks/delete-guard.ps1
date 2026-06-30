# Delete Guard Hook
# Avisa quando um plano de tarefa ou git diff inclui delecoes de arquivos de doc
# sem que o arquivo apareca em DOCUMENT_DELETE_CANDIDATES.md.
# Desabilitado por padrao. Rode manualmente antes de qualquer operacao de delete em lote.

$CandidatesFile = "docs/00_PROJECT/DOCUMENT_DELETE_CANDIDATES.md"

# Pega os arquivos marcados (staged) para delecao
try {
    $deletedFiles = @(git diff --cached --name-status 2>$null) | Where-Object {
        $_ -match "^D\s"
    } | ForEach-Object {
        $_ -replace "^D\s+", ""
    }
}
catch {
    $deletedFiles = @()
}

# Tambem verifica Remove-Item em comandos recentes (heuristica)
$docDeletions = $deletedFiles | Where-Object {
    $_ -match "^docs/" -or $_ -match "^AGENTS\.md" -or $_ -match "^CLAUDE\.md" -or $_ -match "^PROJECT_LOG\.md"
}

if ($docDeletions.Count -eq 0) {
    Write-Host "Delete Guard: No doc file deletions detected."
    exit 0
}

# Verifica o arquivo de candidates
if (-not (Test-Path $CandidatesFile)) {
    Write-Host ""
    Write-Host "Delete Guard: CANDIDATES FILE MISSING - $CandidatesFile not found!"
    Write-Host "  No doc file deletions are authorized without this file."
    foreach ($f in $docDeletions) {
        Write-Host "  Blocked deletion: $f"
    }
    exit 1
}

$candidates = Get-Content $CandidatesFile -Raw -ErrorAction SilentlyContinue

$violations = @()
foreach ($file in $docDeletions) {
    $basename = Split-Path $file -Leaf
    if ($candidates -notmatch [regex]::Escape($basename) -and $candidates -notmatch [regex]::Escape($file)) {
        $violations += "  '$file' not found in DOCUMENT_DELETE_CANDIDATES.md"
    }
}

if ($violations.Count -gt 0) {
    Write-Host ""
    Write-Host "==================================================="
    Write-Host "Delete Guard - VIOLATIONS"
    Write-Host "==================================================="
    foreach ($v in $violations) {
        Write-Host $v
    }
    Write-Host ""
    Write-Host "  Rule: docs-governance.md (no-doc-delete-without-candidate)"
    Write-Host "  Add file to DOCUMENT_DELETE_CANDIDATES.md first."
    Write-Host "==================================================="
    Write-Host ""
    exit 1
}

Write-Host "Delete Guard: PASS - all deleted files are in candidates list."
exit 0
