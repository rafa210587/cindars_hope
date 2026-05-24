#!/usr/bin/env pwsh
<#
.SYNOPSIS
Send questions/hints to Claude/Codex during execution WITHOUT stopping

.DESCRIPTION
Interactive mode to send messages to agent mid-execution.
Messages are appended to a command queue that the agent can check.

.PARAMETER Message
The question or hint to send

.PARAMETER RunId
Specific run ID (auto-detects most recent if not provided)

.PARAMETER ItemId
Specific item ID (auto-detects most recent if not provided)

.EXAMPLE
.\ask_agent.ps1 -Message "Usa GameEventBus para comunicação entre sistemas"
.\ask_agent.ps1 -Message "Verifica se estás seguindo o padrão de naming _SO para ScriptableObjects"
.\ask_agent.ps1 -Message "O que está faltando? Diz [TRACE] dos próximos passos"

#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Message,

    [string]$RunId,
    [string]$ItemId
)

# Detect most recent run and item
if (-not $RunId) {
    $run = Get-ChildItem ".\orquestrador\logs" -Directory -ErrorAction SilentlyContinue |
           Sort-Object LastWriteTime -Descending |
           Select-Object -First 1
    if (-not $run) {
        Write-Error "No orchestrator logs found. Is execution running?"
        exit 1
    }
    $RunId = $run.Name
}

$runPath = ".\orquestrador\logs\$RunId"
if (-not (Test-Path $runPath)) {
    Write-Error "Run not found: $runPath"
    exit 1
}

if (-not $ItemId) {
    $item = Get-ChildItem $runPath -Directory -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
    if (-not $item) {
        Write-Error "No items found in run: $RunId"
        exit 1
    }
    $ItemId = $item.Name
}

$itemPath = "$runPath\$ItemId"
if (-not (Test-Path $itemPath)) {
    Write-Error "Item not found: $itemPath"
    exit 1
}

# Create commands directory
$cmdDir = "$itemPath\commands"
New-Item -ItemType Directory -Path $cmdDir -Force | Out-Null

# Write command with timestamp
$timestamp = (Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff")
$cmdFile = "$cmdDir\command_$(Get-Date -Format 'yyyyMMdd_HHmmss_fff').txt"

$cmdContent = @"
TIMESTAMP: $timestamp
SENDER: user
MESSAGE: $Message
"@

$cmdContent | Out-File -FilePath $cmdFile -Encoding UTF8 -Force

Write-Host ""
Write-Host "✓ Message sent to agent" -ForegroundColor Green
Write-Host ""
Write-Host "Run:  $RunId" -ForegroundColor Cyan
Write-Host "Item: $ItemId" -ForegroundColor Cyan
Write-Host "Msg:  $Message" -ForegroundColor Yellow
Write-Host ""
Write-Host "Command file: $(Split-Path $cmdFile -Leaf)" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Monitoring for response..." -ForegroundColor Green
Write-Host ""

# Watch agent output for response (next 30 seconds)
$timeout = (Get-Date).AddSeconds(30)
$found = $false
$lastPos = 0

$agentLog = "$itemPath\agent_primary_combined.log"
if (Test-Path $agentLog) {
    $initialContent = Get-Content $agentLog -ErrorAction SilentlyContinue
    $lastPos = $initialContent.Length
}

while ((Get-Date) -lt $timeout) {
    if (Test-Path $agentLog) {
        $content = Get-Content $agentLog -ErrorAction SilentlyContinue
        if ($content.Length -gt $lastPos) {
            $newContent = $content.Substring($lastPos)
            if ($newContent -match "\[TRACE\]|Processing|Executando|Checking|Verificando") {
                Write-Host "Agent responding..." -ForegroundColor Green
                Write-Host $newContent -ForegroundColor Yellow
                $found = $true
                break
            }
            $lastPos = $content.Length
        }
    }
    Start-Sleep -Milliseconds 500
}

if (-not $found) {
    Write-Host "No immediate response detected (agent may still be processing)." -ForegroundColor DarkYellow
    Write-Host "Check the agent log for updates:" -ForegroundColor Gray
    Write-Host "  Get-Content '$agentLog' -Wait" -ForegroundColor Gray
}

Write-Host ""
Write-Host "You can send more messages or Ctrl+C to exit." -ForegroundColor Cyan
Write-Host ""
