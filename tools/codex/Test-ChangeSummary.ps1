[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptPath = Join-Path $PSScriptRoot 'Get-ChangeSummary.ps1'
$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("change-summary-fixture-" + [guid]::NewGuid().ToString('N'))
$conflictRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("change-summary-conflict-" + [guid]::NewGuid().ToString('N'))
$inventoryPath = $null
$protectedInventoryPath = $null
$assertions = 0

function Assert-That {
    param([bool]$Condition, [string]$Message)
    $script:assertions++
    if (-not $Condition) { throw "Assertion failed: $Message" }
}

function Invoke-FixtureGit {
    param([Parameter(Mandatory)] [string[]]$Arguments)
    & git -C $fixtureRoot @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Fixture git failed: git $($Arguments -join ' ')" }
}

function Remove-TestPath {
    param([Parameter(Mandatory)] [string]$Path)

    $resolvedPath = [System.IO.Path]::GetFullPath($Path)
    $tempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $tempPrefix = $tempRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedPath.StartsWith($tempPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing test cleanup outside temp: $resolvedPath"
    }
    if (Test-Path -LiteralPath $resolvedPath) {
        Remove-Item -LiteralPath $resolvedPath -Recurse -Force
    }
}

try {
    New-Item -ItemType Directory -Path $fixtureRoot -Force | Out-Null
    Invoke-FixtureGit @('init', '--quiet')
    Invoke-FixtureGit @('config', 'user.email', 'fixture@example.test')
    Invoke-FixtureGit @('config', 'user.name', 'Fixture')

    @('staged.txt', 'unstaged.txt', 'mixed.txt', "odd 'quote ü.txt", 'old name.txt') | ForEach-Object {
        Set-Content -LiteralPath (Join-Path $fixtureRoot $_) -Value 'base' -Encoding utf8NoBOM
    }
    Invoke-FixtureGit @('add', '.')
    Invoke-FixtureGit @('commit', '--quiet', '-m', 'fixture inicial')
    $cleanSummary = (& $scriptPath -ProjectRoot $fixtureRoot | ConvertFrom-Json)
    Assert-That ($cleanSummary.Summary.Total -eq 0 -and -not $cleanSummary.PathsTruncated) 'Clean repositories must produce an empty, untruncated summary.'

    Set-Content -LiteralPath (Join-Path $fixtureRoot 'staged.txt') -Value 'staged' -Encoding utf8NoBOM
    Invoke-FixtureGit @('add', 'staged.txt')
    Set-Content -LiteralPath (Join-Path $fixtureRoot 'unstaged.txt') -Value 'unstaged' -Encoding utf8NoBOM
    Set-Content -LiteralPath (Join-Path $fixtureRoot 'mixed.txt') -Value 'index' -Encoding utf8NoBOM
    Invoke-FixtureGit @('add', 'mixed.txt')
    Add-Content -LiteralPath (Join-Path $fixtureRoot 'mixed.txt') -Value 'worktree'
    Set-Content -LiteralPath (Join-Path $fixtureRoot "odd 'quote ü.txt") -Value 'weird' -Encoding utf8NoBOM
    Invoke-FixtureGit @('mv', 'old name.txt', 'renamed space ü.txt')
    Set-Content -LiteralPath (Join-Path $fixtureRoot 'untracked space ü.txt') -Value 'new' -Encoding utf8NoBOM

    $inventoryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("change-summary-inventory-" + [guid]::NewGuid().ToString('N') + '.json')
    $summary = (& $scriptPath -ProjectRoot $fixtureRoot -MaxResults 2 -CompleteInventoryPath $inventoryPath | ConvertFrom-Json)
    Assert-That ($summary.Summary.Total -eq 6) 'Total must count each porcelain record once.'
    Assert-That ($summary.Summary.Tracked -eq 5) 'Tracked count must exclude only untracked records.'
    Assert-That ($summary.Summary.Untracked -eq 1) 'Untracked count must be reported.'
    Assert-That ($summary.Summary.Staged -eq 3) 'Staged count must include a rename and mixed record.'
    Assert-That ($summary.Summary.Unstaged -eq 3) 'Unstaged count must include a mixed record.'
    Assert-That ($summary.Summary.Renamed -eq 1) 'Rename must be classified.'
    Assert-That ($summary.Paths.Count -eq 2 -and $summary.PathsTruncated -and $summary.OmittedPathCount -eq 4) 'MaxResults must cap paths and make truncation explicit.'

    $inventory = Get-Content -Raw -LiteralPath $inventoryPath | ConvertFrom-Json
    Assert-That (@($inventory).Count -eq 6) 'Complete inventory must contain all records.'
    Assert-That ((@($inventory) | Where-Object { $_.Path -eq "odd 'quote ü.txt" }).Count -eq 1) 'Quoted unicode paths must parse without corruption.'
    $rename = @($inventory | Where-Object { $_.IsRename })[0]
    Assert-That ($rename.Path -eq 'renamed space ü.txt' -and $rename.OriginalPath -eq 'old name.txt') 'Rename source and destination must be preserved.'

    $assetsPath = Join-Path $fixtureRoot 'Assets/inventory.json'
    $null = New-Item -ItemType Directory -Path (Split-Path -Parent $assetsPath) -Force
    $assetOutput = & pwsh -NoProfile -File $scriptPath -ProjectRoot $fixtureRoot -CompleteInventoryPath $assetsPath 2>&1
    $assetExit = $LASTEXITCODE
    Assert-That ($assetExit -eq 1 -and ($assetOutput -join "`n") -match 'outside Assets') 'Inventory writes inside Assets must fail.'

    $protectedInventoryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("change-summary-existing-" + [guid]::NewGuid().ToString('N') + '.json')
    Set-Content -LiteralPath $protectedInventoryPath -Value 'preserve me' -Encoding utf8NoBOM
    $existingOutput = & pwsh -NoProfile -File $scriptPath -ProjectRoot $fixtureRoot -CompleteInventoryPath $protectedInventoryPath 2>&1
    $existingExit = $LASTEXITCODE
    Assert-That ($existingExit -eq 1 -and ($existingOutput -join "`n") -match 'refusing to overwrite' -and (Get-Content -Raw -LiteralPath $protectedInventoryPath).Trim() -eq 'preserve me') 'Existing inventory files must not be overwritten.'

    $nonRepository = Join-Path ([System.IO.Path]::GetTempPath()) ("change-summary-nonrepo-" + [guid]::NewGuid().ToString('N'))
    $null = New-Item -ItemType Directory -Path $nonRepository -Force
    try {
        $failureOutput = & pwsh -NoProfile -File $scriptPath -ProjectRoot $nonRepository 2>&1
        $failureExit = $LASTEXITCODE
    }
    finally {
        Remove-TestPath -Path $nonRepository
    }
    Assert-That ($failureExit -eq 1 -and ($failureOutput -join "`n") -match 'Change summary unavailable') 'Non-repository errors must be explicit.'

    New-Item -ItemType Directory -Path $conflictRoot -Force | Out-Null
    & git -C $conflictRoot init --quiet
    & git -C $conflictRoot config user.email fixture@example.test
    & git -C $conflictRoot config user.name Fixture
    Set-Content -LiteralPath (Join-Path $conflictRoot 'conflict.txt') -Value 'base' -Encoding utf8NoBOM
    & git -C $conflictRoot add conflict.txt
    & git -C $conflictRoot commit --quiet -m 'base'
    $conflictMain = (& git -C $conflictRoot branch --show-current).Trim()
    & git -C $conflictRoot checkout --quiet -b left
    Set-Content -LiteralPath (Join-Path $conflictRoot 'conflict.txt') -Value 'left' -Encoding utf8NoBOM
    & git -C $conflictRoot commit --quiet -am 'left'
    & git -C $conflictRoot checkout --quiet $conflictMain
    Set-Content -LiteralPath (Join-Path $conflictRoot 'conflict.txt') -Value 'right' -Encoding utf8NoBOM
    & git -C $conflictRoot commit --quiet -am 'right'
    $null = & git -C $conflictRoot merge left --no-edit 2>$null
    Assert-That ($LASTEXITCODE -eq 1) 'Fixture must create an unmerged conflict.'
    $conflictSummary = (& $scriptPath -ProjectRoot $conflictRoot | ConvertFrom-Json)
    Assert-That ($conflictSummary.Summary.Conflicted -eq 1 -and $conflictSummary.Summary.Staged -eq 0 -and $conflictSummary.Summary.Unstaged -eq 0) 'Conflicts must be categorized separately from staged or unstaged changes.'

    Write-Output "PASS $assertions assertions"
}
finally {
    Remove-TestPath -Path $fixtureRoot
    Remove-TestPath -Path $conflictRoot
    if ($inventoryPath) { Remove-TestPath -Path $inventoryPath }
    if ($protectedInventoryPath) { Remove-TestPath -Path $protectedInventoryPath }
}
