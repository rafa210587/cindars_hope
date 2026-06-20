# Hook Post-Edit Docs Validate
# Lembra de rodar a validacao de docs depois de editar arquivos de documentacao
# Nao-bloqueante - apenas informativo

param(
    [string[]]$EditedFiles
)

# Verifica se algum doc foi editado
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
    Write-Host "Lembrete: arquivos de documentacao foram editados"
    Write-Host ""
    Write-Host "   Antes de finalizar esta tarefa, rode:"
    Write-Host "   .\tools\docs\validate_docs.ps1"
    Write-Host ""
    Write-Host "   Isto valida:"
    Write-Host "   - Links quebrados"
    Write-Host "   - Consistencia de registry"
    Write-Host "   - YAML frontmatter"
    Write-Host "   - Conformidade de schema"
    Write-Host ""
    Write-Host "   Isto e um lembrete, nao um bloqueio."
    Write-Host ""
}

# Sempre retorna sucesso (nao-bloqueante)
return $true
