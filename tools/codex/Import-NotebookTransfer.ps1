#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PackageRoot,
    [string]$ProjectDestination,
    [string]$CodexDestination = $(if ($env:CODEX_HOME) { $env:CODEX_HOME } else { Join-Path $env:USERPROFILE '.codex' }),
    [switch]$RestoreCodex,
    [switch]$VerifyOnly
)
. "$PSScriptRoot/Transfer.Common.ps1"
$PackageRoot = Get-TransferPath $PackageRoot
Test-TransferHashes $PackageRoot
$meta = Get-Content -LiteralPath (Join-Path $PackageRoot 'transfer.json') -Raw | ConvertFrom-Json
if ($meta.SchemaVersion -ne 1) { throw 'Versao de pacote desconhecida.' }
if ($VerifyOnly) { return }
if (-not $ProjectDestination) { $ProjectDestination = $meta.ProjectRoot }
$ProjectDestination = Get-TransferPath $ProjectDestination
$CodexDestination = Get-TransferPath $CodexDestination
Assert-TransferOutside $ProjectDestination $PackageRoot
Assert-TransferOutside $PackageRoot $ProjectDestination
Assert-TransferOutside $CodexDestination $PackageRoot
Assert-TransferOutside $PackageRoot $CodexDestination
Assert-TransferOutside $ProjectDestination $CodexDestination
Assert-TransferOutside $CodexDestination $ProjectDestination
if (Test-Path -LiteralPath $ProjectDestination) { throw 'Destino do projeto ja existe. Use pasta nova para preservar o trabalho local.' }
if ($RestoreCodex -and $meta.ProjectOnly) { throw 'Pacote ProjectOnly nao contem estado Codex.' }
Assert-TransferAppsClosed -ProjectOnly:(!$RestoreCodex)
$agentsTarget = Join-Path $env:USERPROFILE '.agents'
if ($RestoreCodex -and (Test-Path -LiteralPath (Join-Path $PackageRoot 'user-agents')) -and (Test-Path -LiteralPath $agentsTarget)) {
    throw '.agents global ja existe. Restaure skills manualmente apos comparar; nenhuma pasta foi alterada.'
}
$codexBackup = $null
if ($RestoreCodex -and (Test-Path -LiteralPath $CodexDestination)) {
    $existing = Get-Item -LiteralPath $CodexDestination -Force
    if (-not $existing.PSIsContainer -or ($existing.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        -not (Test-Path -LiteralPath (Join-Path $CodexDestination 'config.toml') -PathType Leaf)) {
        throw 'CODEX_HOME existente precisa ser pasta comum com config.toml; escolha um destino novo ou confira o caminho.'
    }
    # Retain the destination account/session data in a sibling folder, never merge databases.
    $codexBackup = $CodexDestination + '.antes-transferencia-' + [DateTime]::Now.ToString('yyyyMMdd-HHmmss-fff')
    Assert-TransferOutside $codexBackup $CodexDestination
    Move-Item -LiteralPath $CodexDestination -Destination $codexBackup
    Write-Host "Codex anterior preservado: $codexBackup"
}
Copy-TransferTree (Join-Path $PackageRoot 'project') $ProjectDestination
if ($RestoreCodex) {
    Copy-TransferTree (Join-Path $PackageRoot 'codex') $CodexDestination
    # Leave recurring jobs paused locally until paths and dependencies have been checked.
    $jobs = Join-Path $CodexDestination 'automations'
    if (Test-Path -LiteralPath $jobs) {
        foreach ($job in Get-ChildItem -LiteralPath $jobs -Filter automation.toml -Recurse -File) {
            $content = Get-Content -LiteralPath $job.FullName -Raw
            $content = $content -replace '(?m)^(status\s*=\s*)"ACTIVE"', '${1}"PAUSED"'
            Set-Content -LiteralPath $job.FullName -Value $content -NoNewline -Encoding utf8
        }
    }
    if (Test-Path -LiteralPath (Join-Path $PackageRoot 'user-agents')) { Copy-TransferTree (Join-Path $PackageRoot 'user-agents') $agentsTarget }
}
# Re-hash copied project content, detecting copy errors before opening Unity.
foreach ($row in @(Get-Content -LiteralPath (Join-Path $PackageRoot 'SHA256.json') -Raw | ConvertFrom-Json)) {
    if ($row.Path -notmatch '^project[\\/]') { continue }
    $relative = $row.Path.Substring(8)
    if ((Get-FileHash -LiteralPath (Join-Path $ProjectDestination $relative) -Algorithm SHA256).Hash -ne $row.SHA256) { throw "Copia diverge: $relative" }
}
$actualHead = & git -C $ProjectDestination rev-parse HEAD
if ($LASTEXITCODE -or "$actualHead" -ne $meta.Head) { throw 'HEAD restaurado diverge.' }
$actualStatus = @(& git -C $ProjectDestination status --porcelain=v1 --untracked-files=all)
if ($LASTEXITCODE -or ((@($meta.GitStatus) -join "`n") -cne ($actualStatus -join "`n"))) {
    throw 'Estado Git diverge do exportado; examine antes de trabalhar.'
}
Write-Host "Projeto restaurado e verificado: $ProjectDestination"
if ($RestoreCodex) {
    Write-Warning 'Estado local copiado; exibicao de chats depende da compatibilidade do app. Faca login novamente, revise caminhos absolutos e reconecte MCP/plugins.'
    if ($ProjectDestination -ne $meta.ProjectRoot -or $env:USERPROFILE -ne $meta.UserProfile) {
        Write-Warning 'Caminhos diferentes. Chats, configuracoes e worktrees podem referenciar o computador anterior; consulte o guia. Nao foram reescritos bancos SQLite.'
    }
}
Write-Host 'Extras, saves e outros worktrees: restauracao manual conforme transfer.json. Unity validation: NOT RUN (transferencia de arquivos).'
