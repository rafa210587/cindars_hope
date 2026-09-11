# Protected Path Guard (PreToolUse). Origem e destino de moves sao inspecionados.
# YAML/Packages/ProjectSettings continuam sob as autorizacoes existentes.
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'edit-tool-payload.ps1')
$operations = @(Read-EditToolOperations)
foreach ($operation in $operations) {
    foreach ($path in @($operation.PreviousPath, $operation.Path)) {
        if (-not $path) { continue }
        $reason = $null
        if ($path -match '(^|/)docs_old/') {
            $reason = 'docs_old is read-only historical archive'
        } elseif ($path -match '(^|/)docs/0[0-7]_(PROJECT|PRODUCT|ARCHITECTURE|SPECS|REFINEMENTS|VALIDATION|BACKLOG|RELEASES)/') {
            $reason = 'legacy numbered docs folder; use canonical paths'
        } elseif ($path -match '^(specs|spec)/') {
            $reason = 'root spec/specs folder; use .specs'
        } elseif ($path -match '^Assets/_Game/Scripts/.*Tests\.cs$') {
            $reason = 'tests belong under Assets/_Game/Tests/EditMode'
        }
        if ($reason) {
            [Console]::Error.WriteLine("BLOCKED: '$path': $reason. Rule: docs-governance / unity-architecture.")
            exit 2
        }
    }
}
exit 0