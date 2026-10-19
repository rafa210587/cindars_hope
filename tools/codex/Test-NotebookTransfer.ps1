#Requires -Version 7.0
# Isolated file/Git fixture; never uses the actual user's Codex home or project.
[CmdletBinding()]
param()
. "$PSScriptRoot/Transfer.Common.ps1"
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('cindars-transfer-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$oldProfile = $env:USERPROFILE
$checks = 0
function Assert-Check([bool]$Condition, [string]$Label) {
    if (-not $Condition) { throw "FAIL: $Label" }
    $script:checks++
}
function Assert-Rejected([scriptblock]$Action, [string]$Label) {
    $rejected = $false
    try { & $Action | Out-Null } catch { $rejected = $true }
    Assert-Check $rejected $Label
}
# Mock process enumeration so the fixture can run inside Codex/alongside Unity.
# The real export/import scripts have no bypass for the app-closed precondition.
function Get-Process { [pscustomobject]@{ ProcessName = 'Codex' } }
Assert-Rejected { Assert-TransferAppsClosed } 'active Codex blocked'
function Get-Process { @() }
try {
    $env:USERPROFILE = Join-Path $fixture 'profile'
    $source = Join-Path $fixture 'source'
    $codex = Join-Path $fixture 'codex-source'
    foreach ($dir in @('Assets/Editor/Build', 'Packages', 'ProjectSettings', 'Library', 'art', '.specs', '.agents/skills/sample')) {
        New-Item -ItemType Directory -Path (Join-Path $source $dir) -Force | Out-Null
    }
    New-Item -ItemType Directory -Path $codex | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $codex 'sessions') | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $codex 'automations/sample') -Force | Out-Null
    'status = "ACTIVE"' | Set-Content (Join-Path $codex 'automations/sample/automation.toml')
    'fake-state' | Set-Content (Join-Path $codex 'state_5.sqlite')
    'fake-wal' | Set-Content (Join-Path $codex 'state_5.sqlite-wal')
    'fake-transcript' | Set-Content (Join-Path $codex 'sessions/conversa.jsonl')
    'FAKE credential must not travel' | Set-Content (Join-Path $codex 'auth.json')
    'm_EditorVersion: 6000.5.7f1' | Set-Content (Join-Path $source 'ProjectSettings/ProjectVersion.txt')
    "companyName: Fixture`nproductName: Transfer" | Set-Content (Join-Path $source 'ProjectSettings/ProjectSettings.asset')
    "/Library/`n/art/" | Set-Content (Join-Path $source '.gitignore')
    'original' | Set-Content (Join-Path $source 'Assets/tracked.txt')
    'source must remain' | Set-Content (Join-Path $source 'Assets/Editor/Build/tool.cs')
    'cache must not travel' | Set-Content (Join-Path $source 'Library/cache.bin')
    'ignored art source' | Set-Content (Join-Path $source 'art/personagem.aseprite')
    & git -C $source init --quiet
    if ($LASTEXITCODE) { throw 'git init falhou' }
    & git -C $source add .
    & git -C $source -c user.name=Fixture -c user.email=fixture@example.invalid commit --quiet -m 'Cria fixture de transferencia'
    if ($LASTEXITCODE) { throw 'git commit fixture falhou' }
    'staged change' | Set-Content (Join-Path $source 'Assets/tracked.txt')
    & git -C $source add Assets/tracked.txt
    'unstaged change after stage' | Set-Content (Join-Path $source 'Assets/tracked.txt')
    'new untracked file' | Set-Content (Join-Path $source 'Assets/novo ação.txt')
    $otherCheckout = Join-Path $fixture 'sibling-worktree'
    & git -C $source worktree add --quiet -b codex/fixture-transfer $otherCheckout HEAD
    if ($LASTEXITCODE) { throw 'git worktree fixture falhou' }
    'unfinished sibling work' | Set-Content (Join-Path $otherCheckout 'pending.txt')
    $saves = Join-Path $env:USERPROFILE 'AppData/LocalLow/Fixture/Transfer'
    New-Item -ItemType Directory -Path $saves -Force | Out-Null
    'save fixture' | Set-Content (Join-Path $saves 'save.json')
    $package = Join-Path $fixture 'package'
    & "$PSScriptRoot/Export-NotebookTransfer.ps1" -ProjectRoot $source -CodexRoot $codex -Destination $package
    Assert-Check (Test-Path (Join-Path $package 'project/art/personagem.aseprite')) 'ignored art preserved'
    Assert-Check (Test-Path (Join-Path $package 'project/Assets/Editor/Build/tool.cs')) 'nested source Build preserved'
    Assert-Check (!(Test-Path (Join-Path $package 'project/Library'))) 'Unity root cache omitted'
    Assert-Check (!(Test-Path (Join-Path $package 'codex/auth.json'))) 'known auth omitted'
    Assert-Check (Test-Path (Join-Path $package 'codex/state_5.sqlite-wal')) 'WAL preserved'
    Assert-Check (Test-Path (Join-Path $package 'extras/000/save.json')) 'Unity saves archived'
    Assert-Check (Test-Path (Join-Path $package 'extras/001/pending.txt')) 'registered sibling worktree archived'
    & "$PSScriptRoot/Import-NotebookTransfer.ps1" -PackageRoot $package -VerifyOnly
    $target = Join-Path $fixture 'restored-project'
    $codexTarget = Join-Path $fixture 'restored-codex'
    New-Item -ItemType Directory -Path $codexTarget | Out-Null
    '# fixture' | Set-Content (Join-Path $codexTarget 'config.toml')
    'not overwritten' | Set-Content (Join-Path $codexTarget 'previous.txt')
    & "$PSScriptRoot/Import-NotebookTransfer.ps1" -PackageRoot $package -ProjectDestination $target -CodexDestination $codexTarget -RestoreCodex
    Assert-Check ((Get-Content (Join-Path $target 'Assets/tracked.txt')) -eq 'unstaged change after stage') 'working tree restored'
    $indexContent = & git -C $target show :Assets/tracked.txt
    Assert-Check ($indexContent -eq 'staged change') 'Git index preserved separately'
    Assert-Check (Test-Path (Join-Path $target 'Assets/novo ação.txt')) 'untracked Unicode file preserved'
    Assert-Check ((Get-Content (Join-Path $codexTarget 'automations/sample/automation.toml')) -eq 'status = "PAUSED"') 'automations paused'
    $backup = @(Get-ChildItem -LiteralPath $fixture -Directory -Filter 'restored-codex.antes-transferencia-*')
    Assert-Check ($backup.Count -eq 1 -and (Test-Path (Join-Path $backup[0].FullName 'previous.txt'))) 'previous Codex retained'
    Assert-Rejected { & "$PSScriptRoot/Import-NotebookTransfer.ps1" -PackageRoot $package -ProjectDestination $target } 'existing project blocked'
    Assert-Rejected { & "$PSScriptRoot/Export-NotebookTransfer.ps1" -ProjectRoot $source -CodexRoot $codex -Destination $package } 'existing package blocked'
    Assert-Rejected { & "$PSScriptRoot/Export-NotebookTransfer.ps1" -ProjectRoot $source -CodexRoot $codex -Destination (Join-Path $source 'recursive') } 'recursive export blocked'
    Assert-Rejected { Assert-TransferOutside $source ([IO.Path]::GetPathRoot($source)) } 'drive root containment checked'
    'CORRUPTED' | Set-Content (Join-Path $package 'project/Assets/tracked.txt')
    Assert-Rejected { & "$PSScriptRoot/Import-NotebookTransfer.ps1" -PackageRoot $package -VerifyOnly } 'corruption detected'
    Write-Host "PASS: $checks checks. Fixture preservada: $fixture"
} finally {
    $env:USERPROFILE = $oldProfile
}
