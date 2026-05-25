# Post-Edit Docs Validate Hook
# Reminds about docs validation after editing documentation files
# Non-blocking - informational only

param(
    [string[]]$EditedFiles
)

# Check if any docs were edited
$DocsPatterns = @(
    "docs/",
    "PROJECT_LOG.md",
    "IMPLEMENTATION_STATUS.md",
    "CLAUDE.md",
    "AGENTS.md"
)

$DocsEdited = $false

foreach ($file in $EditedFiles) {
    foreach ($pattern in $DocsPatterns) {
        if ($file -like "*$pattern*") {
            $DocsEdited = $true
            break
        }
    }
    if ($DocsEdited) { break }
}

if ($DocsEdited) {
    Write-Host ""
    Write-Host "📋 Reminder: Documentation files were edited"
    Write-Host ""
    Write-Host "   Before finishing this task, run:"
    Write-Host "   .\tools\docs\validate_docs.ps1"
    Write-Host ""
    Write-Host "   This validates:"
    Write-Host "   - Broken links"
    Write-Host "   - Registry consistency"
    Write-Host "   - YAML frontmatter"
    Write-Host "   - Schema compliance"
    Write-Host ""
    Write-Host "   ℹ️  This is a reminder, not a blocker."
    Write-Host ""
}

# Always return success (non-blocking)
return $true
