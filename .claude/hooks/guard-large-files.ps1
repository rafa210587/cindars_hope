# Guard Large Files: limite por arquivo de conteudo novo em Write/Edit/patch.
# O limite mede a adicao recebida; nao afirma medir o tamanho final de um update.
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'edit-tool-payload.ps1')
$operations = @(Read-EditToolOperations)
$maxBytes = 1MB
foreach ($operation in $operations) {
    $bytes = [System.Text.Encoding]::UTF8.GetByteCount($operation.AddedContent)
    if ($bytes -gt $maxBytes) {
        [Console]::Error.WriteLine("BLOCKED: added content exceeds the $maxBytes byte per-file limit. Rule: security-and-files.")
        exit 2
    }
}
exit 0