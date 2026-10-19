#requires -Version 7.0
[CmdletBinding()]
param(
    [ValidateRange(1, 1000)]
    [int]$MaxResults = 20,

    [string]$ProjectRoot = (Get-Location).Path,

    [string]$CompleteInventoryPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-GitText {
    param(
        [Parameter(Mandatory)] [string[]]$Arguments
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = 'git'
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $startInfo.StandardErrorEncoding = [System.Text.Encoding]::UTF8
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            throw 'Could not start git.'
        }
        # Both streams must be drained concurrently: a large stderr cannot block git while stdout is read.
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) {
            $detail = $stderr.Trim()
            if ([string]::IsNullOrWhiteSpace($detail)) { $detail = "git exited with code $($process.ExitCode)." }
            throw "Git command failed: $detail"
        }

        return $stdout
    }
    catch {
        throw "Unable to run git: $($_.Exception.Message)"
    }
    finally {
        $process.Dispose()
    }
}

function Get-ChangeState {
    param([Parameter(Mandatory)] [string]$Status)

    if ($Status -eq '??') { return 'untracked' }
    if ($Status -match 'U|^(DD|AA)$') { return 'conflicted' }

    $staged = $Status[0] -ne ' '
    $unstaged = $Status[1] -ne ' '
    if ($staged -and $unstaged) { return 'staged+unstaged' }
    if ($staged) { return 'staged' }
    if ($unstaged) { return 'unstaged' }
    return 'unknown'
}

function Get-PathCategory {
    param([Parameter(Mandatory)] [string]$Path)

    $normalized = $Path -replace '\\', '/'
    if ($normalized -match '^Assets/') { return 'assets' }
    if ($normalized -match '^docs/') { return 'docs' }
    if ($normalized -match '^tools/') { return 'tools' }
    if ($normalized -match '^\.specs/') { return 'specs' }
    if ($normalized -match '^ProjectSettings/') { return 'projectSettings' }
    if ($normalized -match '^Packages/') { return 'packages' }
    return 'other'
}

function Resolve-SafeInventoryPath {
    param(
        [Parameter(Mandatory)] [string]$Path,
        [Parameter(Mandatory)] [string]$RepositoryRoot
    )

    $candidatePath = if ([System.IO.Path]::IsPathRooted($Path)) {
        [System.IO.Path]::GetFullPath($Path)
    }
    else {
        [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot $Path))
    }
    if (Test-Path -LiteralPath $candidatePath) {
        throw 'CompleteInventoryPath already exists; refusing to overwrite it.'
    }

    $assetsRoot = [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot 'Assets'))
    $assetsPrefix = $assetsRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if ($candidatePath.Equals($assetsRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
        $candidatePath.StartsWith($assetsPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'CompleteInventoryPath must be outside Assets.'
    }

    # Refuse reparse points in ancestors so a lexical path cannot escape into Assets through a junction/symlink.
    $ancestor = Split-Path -Parent $candidatePath
    while (-not [string]::IsNullOrWhiteSpace($ancestor)) {
        if (Test-Path -LiteralPath $ancestor) {
            $item = Get-Item -LiteralPath $ancestor -Force
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw 'CompleteInventoryPath cannot use a reparse-point ancestor.'
            }
        }
        $nextAncestor = Split-Path -Parent $ancestor
        if ($nextAncestor -eq $ancestor) { break }
        $ancestor = $nextAncestor
    }

    return $candidatePath
}

try {
    $resolvedRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
    $repositoryRoot = (Invoke-GitText -Arguments @('-C', $resolvedRoot, 'rev-parse', '--show-toplevel')).Trim()
    if ([string]::IsNullOrWhiteSpace($repositoryRoot)) {
        throw 'Git did not return a repository root.'
    }
    $repositoryRoot = (Resolve-Path -LiteralPath $repositoryRoot).Path

    $statusOutput = Invoke-GitText -Arguments @('-C', $repositoryRoot, 'status', '--porcelain=v1', '-z', '--untracked-files=all')
}
catch {
    Write-Error "Change summary unavailable: $($_.Exception.Message)"
    exit 1
}

# Porcelain v1 with -z uses NUL delimiters and reports a rename as destination then source.
$records = [System.Collections.Generic.List[object]]::new()
$tokens = $statusOutput.Split([char[]]@([char]0), [System.StringSplitOptions]::None)
for ($index = 0; $index -lt ($tokens.Length - 1); $index++) {
    $entry = $tokens[$index]
    if ($entry.Length -lt 3) { continue }

    $status = $entry.Substring(0, 2)
    $path = $entry.Substring(3)
    $isRenameOrCopy = $status.Contains('R') -or $status.Contains('C')
    $originalPath = $null
    if ($isRenameOrCopy) {
        if ($index + 1 -ge ($tokens.Length - 1)) {
            Write-Error "Change summary unavailable: incomplete rename/copy entry from git status."
            exit 1
        }
        $index++
        $originalPath = $tokens[$index]
    }

    $state = Get-ChangeState -Status $status
    $records.Add([pscustomobject]@{
        Path = $path
        OriginalPath = $originalPath
        Status = $status
        State = $state
        IsRename = $status.Contains('R')
        IsCopy = $status.Contains('C')
    })
}

$tracked = @($records | Where-Object { $_.State -ne 'untracked' }).Count
$untracked = @($records | Where-Object { $_.State -eq 'untracked' }).Count
$staged = @($records | Where-Object { $_.State -in @('staged', 'staged+unstaged') }).Count
$unstaged = @($records | Where-Object { $_.State -in @('unstaged', 'staged+unstaged') }).Count
$conflicted = @($records | Where-Object { $_.State -eq 'conflicted' }).Count
$renamed = @($records | Where-Object { $_.IsRename }).Count
$copied = @($records | Where-Object { $_.IsCopy }).Count
$categories = [ordered]@{ assets = 0; docs = 0; tools = 0; specs = 0; projectSettings = 0; packages = 0; other = 0 }
foreach ($record in $records) {
    $category = Get-PathCategory -Path $record.Path
    $categories[$category]++
}

$inventoryPath = $null
if (-not [string]::IsNullOrWhiteSpace($CompleteInventoryPath)) {
    try { $candidatePath = Resolve-SafeInventoryPath -Path $CompleteInventoryPath -RepositoryRoot $repositoryRoot }
    catch { Write-Error $_.Exception.Message; exit 1 }

    $parentDirectory = Split-Path -Parent $candidatePath
    if (-not [string]::IsNullOrWhiteSpace($parentDirectory)) {
        [System.IO.Directory]::CreateDirectory($parentDirectory) | Out-Null
    }
    $inventoryJson = ConvertTo-Json -InputObject $records.ToArray() -Depth 4 -Compress
    try {
        $stream = [System.IO.File]::Open($candidatePath, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
        try {
            $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($inventoryJson)
            $stream.Write($bytes, 0, $bytes.Length)
        }
        finally {
            $stream.Dispose()
        }
    }
    catch [System.IO.IOException] {
        Write-Error 'CompleteInventoryPath already exists or could not be created exclusively; refusing to overwrite it.'
        exit 1
    }
    $inventoryPath = $candidatePath
}

$visibleRecords = @($records | Select-Object -First $MaxResults)
$result = [pscustomobject]@{
    SchemaVersion = 1
    RepositoryRoot = $repositoryRoot
    Summary = [ordered]@{
        Total = $records.Count
        Tracked = $tracked
        Untracked = $untracked
        Staged = $staged
        Unstaged = $unstaged
        Conflicted = $conflicted
        Renamed = $renamed
        Copied = $copied
        Categories = $categories
    }
    Paths = $visibleRecords
    PathsTruncated = ($records.Count -gt $visibleRecords.Count)
    OmittedPathCount = [Math]::Max(0, $records.Count - $visibleRecords.Count)
    CompleteInventoryPath = $inventoryPath
}

$result | ConvertTo-Json -Depth 6 -Compress
