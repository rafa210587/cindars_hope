# Delete Guard Hook
# Warns when a task plan or git diff includes doc file deletions
# without the file appearing in DOCUMENT_DELETE_CANDIDATES.md.
# Disabled by default. Run manually before any bulk delete operations.

$CandidatesFile = "docs/00_PROJECT/DOCUMENT_DELETE_CANDIDATES.md"

# Get files staged for deletion
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

# Also check for Remove-Item in recent commands (heuristic)
$docDeletions = $deletedFiles | Where-Object {
    $_ -match "^docs/" -or $_ -match "^AGENTS\.md" -or $_ -match "^CLAUDE\.md" -or $_ -match "^PROJECT_LOG\.md"
}

if ($docDeletions.Count -eq 0) {
    Write-Host "Delete Guard: No doc file deletions detected."
    exit 0
}

# Check candidates file
if (-not (Test-Path $CandidatesFile)) {
    Write-Host ""
    Write-Host "Delete Guard: CANDIDATES FILE MISSING — $CandidatesFile not found!"
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
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Host "Delete Guard — VIOLATIONS"
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    foreach ($v in $violations) {
        Write-Host $v
    }
    Write-Host ""
    Write-Host "  Rule: no-doc-delete-without-candidate.md"
    Write-Host "  Add file to DOCUMENT_DELETE_CANDIDATES.md first."
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Host ""
    exit 1
}

Write-Host "Delete Guard: PASS — all deleted files are in candidates list."
exit 0
