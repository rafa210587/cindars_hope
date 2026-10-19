#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Destination,
    [string]$ProjectRoot = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent),
    [string]$CodexRoot = $(if ($env:CODEX_HOME) { $env:CODEX_HOME } else { Join-Path $env:USERPROFILE '.codex' }),
    [string[]]$ExtraPath = @(),
    [switch]$ProjectOnly
)
. "$PSScriptRoot/Transfer.Common.ps1"
$ProjectRoot = Get-TransferPath $ProjectRoot
$Destination = Get-TransferPath $Destination
$CodexRoot = Get-TransferPath $CodexRoot
Assert-TransferOutside $Destination $ProjectRoot
Assert-TransferOutside $Destination $CodexRoot
foreach ($extra in $ExtraPath) { Assert-TransferOutside $Destination $extra }
if (Test-Path -LiteralPath $Destination) { throw 'Escolha uma pasta de pacote NOVA; nao reutilize backup anterior.' }
foreach ($required in @('.git', 'Assets', 'Packages', 'ProjectSettings')) {
    if (-not (Test-Path -LiteralPath (Join-Path $ProjectRoot $required) -PathType Container)) {
        throw "Origem deve ser checkout principal Unity com .git em pasta. Ausente: $required"
    }
}
Assert-TransferAppsClosed -ProjectOnly:$ProjectOnly
if (-not $ProjectOnly -and -not (Test-Path -LiteralPath $CodexRoot -PathType Container)) { throw 'CODEX_HOME ausente.' }
foreach ($extra in $ExtraPath) {
    if (-not (Test-Path -LiteralPath $extra -PathType Container)) { throw "ExtraPath precisa ser pasta existente: $extra" }
}
$gitStatus = @(& git -C $ProjectRoot status --porcelain=v1 --untracked-files=all)
if ($LASTEXITCODE) { throw 'Nao foi possivel ler Git.' }
$branch = & git -C $ProjectRoot branch --show-current
$head = & git -C $ProjectRoot rev-parse HEAD
$worktrees = @(& git -C $ProjectRoot worktree list --porcelain)
$exclusions = @('Library', 'Temp', 'Obj', 'Logs', '.vs') | ForEach-Object { Join-Path $ProjectRoot $_ }
New-Item -ItemType Directory -Path $Destination | Out-Null
Write-Host 'Copiando projeto, .git, arte local e evidencias (sem caches Unity)...'
Copy-TransferTree $ProjectRoot (Join-Path $Destination 'project') $exclusions
if (-not $ProjectOnly) {
    # Copy state databases AND WAL sidecars together, only with the app stopped.
    # Credentials/browser profiles/sandbox provisioning must be recreated on the destination.
    $skipDirs = @('.sandbox', '.sandbox-bin', '.sandbox-secrets', 'browser', 'computer-use',
        'cache', 'tmp', '.tmp', 'node_repl', 'mcp-oauth-locks', 'thread-writer-locks', 'project-metadata-locks')
    $codexTarget = Join-Path $Destination 'codex'
    New-Item -ItemType Directory -Path $codexTarget | Out-Null
    foreach ($entry in Get-ChildItem -LiteralPath $CodexRoot -Force) {
        if ($entry.Name -in $skipDirs -or $entry.Name -match '^(auth\.json|cap_sid|installation_id|chrome-native-hosts.*|.*\.lock|.*\.guard)$') { continue }
        if ($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Link no CODEX_HOME: $($entry.FullName)" }
        if ($entry.PSIsContainer) { Copy-TransferTree $entry.FullName (Join-Path $codexTarget $entry.Name) }
        else { Copy-Item -LiteralPath $entry.FullName -Destination $codexTarget }
    }
    $globalSkills = Join-Path $env:USERPROFILE '.agents'
    if (Test-Path -LiteralPath $globalSkills -PathType Container) { Copy-TransferTree $globalSkills (Join-Path $Destination 'user-agents') }
}
$extras = @()
$index = 0
$saveRoot = $null
$settingsPath = Join-Path $ProjectRoot 'ProjectSettings/ProjectSettings.asset'
if (Test-Path -LiteralPath $settingsPath) {
    $settings = Get-Content -LiteralPath $settingsPath -Raw
    $company = [regex]::Match($settings, '(?m)^\s*companyName:\s*(.+)$').Groups[1].Value.Trim()
    $product = [regex]::Match($settings, '(?m)^\s*productName:\s*(.+)$').Groups[1].Value.Trim()
    if ($company -and $product) {
        $candidate = Join-Path $env:USERPROFILE "AppData/LocalLow/$company/$product"
        if (Test-Path -LiteralPath $candidate -PathType Container) { $saveRoot = $candidate }
    }
}
$allExtras = @($ExtraPath | ForEach-Object { Get-TransferPath $_ })
if ($saveRoot) { $allExtras += $saveRoot }
# Registered sibling worktrees can contain unfinished work absent from the primary checkout.
foreach ($line in $worktrees) {
    if ($line -notmatch '^worktree (.+)$') { continue }
    $checkout = Get-TransferPath $Matches[1]
    if ($checkout -eq $ProjectRoot -or
        $checkout.StartsWith($ProjectRoot + [IO.Path]::DirectorySeparatorChar, 'OrdinalIgnoreCase') -or
        (!$ProjectOnly -and $checkout.StartsWith($CodexRoot + [IO.Path]::DirectorySeparatorChar, 'OrdinalIgnoreCase'))) { continue }
    if (Test-Path -LiteralPath $checkout -PathType Container) { $allExtras += $checkout }
    else { Write-Warning "Worktree registrado ausente, nao copiado: $checkout" }
}
foreach ($extra in $allExtras | Select-Object -Unique) {
    Assert-TransferOutside $Destination $extra
    $slot = 'extras/{0:D3}' -f $index++
    Copy-TransferTree (Get-TransferPath $extra) (Join-Path $Destination $slot)
    $extras += [ordered]@{ Source = (Get-TransferPath $extra); Slot = $slot }
}
$unityVersion = Get-Content -LiteralPath (Join-Path $ProjectRoot 'ProjectSettings/ProjectVersion.txt') -Raw
$metadata = [ordered]@{
    SchemaVersion = 1; CreatedUtc = [DateTime]::UtcNow.ToString('o'); Computer = $env:COMPUTERNAME
    ProjectRoot = $ProjectRoot; CodexRoot = $CodexRoot; UserProfile = $env:USERPROFILE
    Branch = "$branch"; Head = "$head"; GitStatus = $gitStatus; Worktrees = $worktrees
    UnityVersion = $unityVersion; ProjectOnly = [bool]$ProjectOnly; Extras = $extras; DetectedSaveRoot = $saveRoot
    ExcludedProjectDirectories = $exclusions
}
$metadata | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $Destination 'transfer.json') -Encoding utf8
# Bundle the tools and guide so restoring does not require a pre-existing checkout.
Copy-Item -LiteralPath "$PSScriptRoot/Transfer.Common.ps1", "$PSScriptRoot/Import-NotebookTransfer.ps1" -Destination $Destination
$guide = Join-Path $ProjectRoot 'docs/operations/MIGRACAO_NOTEBOOK_CODEX.md'
if (Test-Path -LiteralPath $guide) { Copy-Item -LiteralPath $guide -Destination $Destination }
Write-Host 'Calculando SHA-256 de todos os arquivos; aguarde em projetos grandes...'
Write-TransferHashes $Destination
Test-TransferHashes $Destination
Write-Host "Pacote pronto: $Destination"
Write-Host 'Extras ficam arquivados para restauracao manual. Consulte transfer.json e o guia.'
