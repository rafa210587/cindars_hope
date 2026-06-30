# Unity YAML Edit Guard Hook
# Avisa quando os arquivos alterados incluem arquivos .unity, .prefab ou .asset.
# Desabilitado por padrao. Rode manualmente ou via deteccao de scope pos-edit.

$UnityYamlExtensions = @(".unity", ".prefab", ".asset")

# Pega os arquivos alterados do git
try {
    $changedFiles = @(git diff --name-only HEAD 2>$null) + @(git diff --cached --name-only 2>$null)
    $changedFiles = $changedFiles | Sort-Object -Unique
}
catch {
    Write-Host "Unity YAML Edit Guard: Could not get git diff. Skipping."
    exit 0
}

$yamlEdits = $changedFiles | Where-Object {
    $ext = [System.IO.Path]::GetExtension($_).ToLower()
    $UnityYamlExtensions -contains $ext
}

if ($yamlEdits.Count -eq 0) {
    Write-Host "Unity YAML Edit Guard: No Unity YAML files modified."
    exit 0
}

Write-Host ""
Write-Host "==================================================="
Write-Host "Unity YAML Edit Guard - REVIEW REQUIRED"
Write-Host "==================================================="
Write-Host ""
Write-Host "  Unity YAML files modified:"
foreach ($f in $yamlEdits) {
    Write-Host "    $f"
}
Write-Host ""
Write-Host "  Rule: unity-assets.md (unity-yaml-editing-policy)"
Write-Host "  Manual YAML edits to .unity/.prefab/.asset can corrupt scene/prefab refs."
Write-Host ""
Write-Host "  Verify:"
Write-Host "    [ ] Was this edit authorized by the active spec?"
Write-Host "    [ ] Was it done via Unity Editor / repair menu (preferred)?"
Write-Host "    [ ] If manual: has it been verified in Unity Editor for GUID integrity?"
Write-Host ""
Write-Host "  If repair menu was used - this warning is expected. Proceed."
Write-Host "  If manually edited - verify in Unity Editor before claiming PASS."
Write-Host "==================================================="
Write-Host ""

# Nao bloqueante: exit 0 para nao parar o workflow, apenas informa
exit 0
