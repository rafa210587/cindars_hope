#!/usr/bin/env pwsh
<#
.SYNOPSIS
Run orchestrator with 4-tab monitoring setup

.DESCRIPTION
Automatically opens 4 PowerShell tabs:
1. Main orchestrator execution
2. Live monitor (status updates)
3. Claude output stream
4. Git watch stream

.PARAMETER Mode
Execution mode: 'spec' or 'prompt'

.PARAMETER InputDir
Directory containing items to execute

.PARAMETER NoAutoCommit
Don't auto-commit on success

.PARAMETER StopOnFailure
Stop on first failure

.EXAMPLE
.\run_with_monitoring.ps1 -Mode prompt -InputDir ".\docs\agent_prompts\a_executar"

#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('spec', 'prompt')]
    [string]$Mode,

    [Parameter(Mandatory=$true)]
    [string]$InputDir,

    [switch]$NoAutoCommit,
    [switch]$StopOnFailure = $true
)

# Build base command
$baseCmd = "python .\orquestrador\run_orquestrador.py `
  --mode $Mode `
  --input-dir `"$InputDir`""

if ($NoAutoCommit) {
    $baseCmd += " --no-auto-commit"
}

if ($StopOnFailure) {
    $baseCmd += " --stop-on-failure"
}

# Tab 1: Main execution
$tab1 = @"
Write-Host "=== TAB 1: MAIN ORCHESTRATOR EXECUTION ===" -ForegroundColor Cyan
Write-Host "Starting orchestrator..."
Write-Host ""
$baseCmd
"@

# Tab 2: Monitor (waits a bit for logs to be created)
$tab2 = @"
Write-Host "=== TAB 2: LIVE MONITOR ===" -ForegroundColor Green
Start-Sleep -Seconds 3
Write-Host "Connecting to live monitor..."
Write-Host ""
python .\orquestrador\run_orquestrador.py --monitor
"@

# Tab 3: Claude output stream
$tab3 = @"
Write-Host "=== TAB 3: CLAUDE OUTPUT STREAM ===" -ForegroundColor Yellow
Start-Sleep -Seconds 5
Write-Host "Waiting for log directory..."
Write-Host ""
`$run = Get-ChildItem .\orquestrador\logs -Directory -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (`$run) {
    `$item = Get-ChildItem `$run.FullName -Directory -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (`$item) {
        Write-Host "Streaming from: `$(`$item.FullName)\agent_primary_combined.log" -ForegroundColor Cyan
        Get-Content "`$(`$item.FullName)\agent_primary_combined.log" -Wait -Tail 80
    } else {
        Write-Host "Waiting for item logs..." -ForegroundColor Red
    }
} else {
    Write-Host "Waiting for run logs..." -ForegroundColor Red
}
"@

# Tab 4: Git watch
$tab4 = @"
Write-Host "=== TAB 4: GIT WATCH STREAM ===" -ForegroundColor Magenta
Start-Sleep -Seconds 5
Write-Host "Waiting for log directory..."
Write-Host ""
`$run = Get-ChildItem .\orquestrador\logs -Directory -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (`$run) {
    `$item = Get-ChildItem `$run.FullName -Directory -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (`$item) {
        Write-Host "Streaming from: `$(`$item.FullName)\git_watch.log" -ForegroundColor Magenta
        Get-Content "`$(`$item.FullName)\git_watch.log" -Wait -Tail 30
    } else {
        Write-Host "Waiting for item logs..." -ForegroundColor Red
    }
} else {
    Write-Host "Waiting for run logs..." -ForegroundColor Red
}
"@

Write-Host "Opening 4-tab monitoring setup..." -ForegroundColor Cyan
Write-Host ""
Write-Host "Tab 1: Main execution" -ForegroundColor Cyan
Write-Host "Tab 2: Monitor (live status)" -ForegroundColor Green
Write-Host "Tab 3: Claude output stream" -ForegroundColor Yellow
Write-Host "Tab 4: Git watch stream" -ForegroundColor Magenta
Write-Host ""

# Create temp files for each tab
$tempDir = [System.IO.Path]::GetTempPath()
$guid = [System.Guid]::NewGuid()

$tab1File = "$tempDir\tab1_$guid.ps1"
$tab2File = "$tempDir\tab2_$guid.ps1"
$tab3File = "$tempDir\tab3_$guid.ps1"
$tab4File = "$tempDir\tab4_$guid.ps1"

# Write scripts to temp files
$tab1 | Out-File -FilePath $tab1File -Encoding UTF8
$tab2 | Out-File -FilePath $tab2File -Encoding UTF8
$tab3 | Out-File -FilePath $tab3File -Encoding UTF8
$tab4 | Out-File -FilePath $tab4File -Encoding UTF8

# Get current directory
$currentDir = Get-Location

# Open tabs
try {
    # Tab 1: Main
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$currentDir'; & '$tab1File'" `
        -WindowStyle Normal

    # Tab 2: Monitor
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$currentDir'; & '$tab2File'" `
        -WindowStyle Normal

    # Tab 3: Claude stream
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$currentDir'; & '$tab3File'" `
        -WindowStyle Normal

    # Tab 4: Git watch
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$currentDir'; & '$tab4File'" `
        -WindowStyle Normal

    Write-Host "✓ All 4 tabs opened!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Tips:" -ForegroundColor Cyan
    Write-Host "- Tab 1 shows orchestrator progress"
    Write-Host "- Tab 2 updates every 2 seconds with status"
    Write-Host "- Tab 3 streams Claude's full output"
    Write-Host "- Tab 4 shows git status/diff changes"
    Write-Host ""
    Write-Host "To stop execution:" -ForegroundColor Yellow
    Write-Host "  New-Item .\orquestrador\state\STOP -ItemType File -Force"
    Write-Host ""
}
catch {
    Write-Error "Failed to open tabs: $_"
    exit 1
}

# Cleanup temp files after a delay
Start-Sleep -Seconds 2
Remove-Item -Path $tab1File, $tab2File, $tab3File, $tab4File -Force -ErrorAction SilentlyContinue
