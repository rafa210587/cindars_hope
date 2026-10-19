# Runtime Code Guard (PostToolUse). Inspeciona apenas conteudo adicionado.
# Exit 2 pede correcao: PostToolUse nao desfaz a escrita.
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'edit-tool-payload.ps1')
$operations = @(Read-EditToolOperations)
$problems = [System.Collections.Generic.List[string]]::new()

foreach ($operation in $operations) {
    $path = $operation.Path
    if ($path -notmatch '\.cs$' -or $operation.Operation -eq 'Delete') { continue }
    $newContent = $operation.AddedContent
    if ([string]::IsNullOrEmpty($newContent)) { continue }
    $isRuntime = $path -match '^Assets/_Game/Scripts/' -and $path -notmatch '/Editor/'
    if ($isRuntime) {
        $searchPattern = '\b(?:GameObject\.Find|FindObjectOfType|FindObjectsOfType|FindObjectsByType|FindFirstObjectByType|FindAnyObjectByType)\s*[(<]'
        if ($newContent -match $searchPattern) {
            $problems.Add("Runtime scene search added to '$path'. Rule: unity-architecture. Use explicit wiring.")
        }
    }
    if ($newContent -match 'namespace\s+CindarsHope\.(Debug|Temp)\b') {
        $problems.Add("Forbidden namespace CindarsHope.Debug/CindarsHope.Temp added to '$path'.")
    }
    if ($operation.IsFullWrite -and $path -match '^Assets/_Game/(Scripts|Tests)/') {
        $classMatches = [regex]::Matches($newContent, '(?m)^\s*(?:public|internal)?\s*(?:sealed|abstract|static)?\s*class\s+([A-Za-z_]\w+)')
        foreach ($match in $classMatches) {
            $className = $match.Groups[1].Value
            $existing = & git grep -l --untracked -E "class\s+$className\b" -- 'Assets/_Game/Scripts/*.cs' 'Assets/_Game/Tests/*.cs' 2>$null
            if ($LASTEXITCODE -gt 1) {
                $problems.Add("Duplicate-class inspection failed for '$path'; verify repository access.")
                continue
            }
            $others = @($existing | Where-Object { ($_ -replace '\\', '/') -ne $path })
            if ($others.Count -gt 0) {
                $problems.Add("Class '$className' in '$path' also occurs in: $($others -join ', '). Review namespaces/reuse with system-reuse-audit.")
            }
        }
    }
}
if ($problems.Count -gt 0) {
    foreach ($problem in $problems) { [Console]::Error.WriteLine("RUNTIME-CODE-GUARD: $problem") }
    exit 2
}
exit 0