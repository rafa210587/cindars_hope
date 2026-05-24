#!/usr/bin/env pwsh
<#
.SYNOPSIS
Interactive ask mode - Send messages to agent during execution

.DESCRIPTION
Interactive terminal that lets you send messages to Claude/Codex
while it's executing, without stopping the execution.

Automatically detects the latest run and item.

#>

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       ASK MODE - Guiar Claude em Tempo Real                       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Write-Host "INSTRUÇÕES:" -ForegroundColor Green
Write-Host ""
Write-Host "Digite uma pergunta ou dica para Claude/Codex." -ForegroundColor White
Write-Host "A mensagem será entregue SEM PARAR a execução!" -ForegroundColor Yellow
Write-Host ""

Write-Host "EXEMPLOS:" -ForegroundColor Magenta
Write-Host ""
Write-Host "  > Usa GameEventBus para comunicação entre sistemas" -ForegroundColor DarkGray
Write-Host "  > O que falta para passar nas validações?" -ForegroundColor DarkGray
Write-Host "  > Verifica se estás seguindo padrão _SO para ScriptableObjects" -ForegroundColor DarkGray
Write-Host "  > Diz [TRACE] dos próximos passos que vais fazer" -ForegroundColor DarkGray
Write-Host "  > Nunca use GameObject.Find() - viola regra inviolável" -ForegroundColor DarkGray
Write-Host ""

Write-Host "CONTROLES:" -ForegroundColor Cyan
Write-Host "  [Enter]     → Enviar mensagem" -ForegroundColor DarkGray
Write-Host "  Ctrl+C      → Sair do ask mode (a execução continua)" -ForegroundColor DarkGray
Write-Host ""

Write-Host "────────────────────────────────────────────────────────────────────" -ForegroundColor DarkCyan
Write-Host ""

# Loop de input
while ($true) {
    try {
        Write-Host "ASK > " -ForegroundColor Cyan -NoNewline
        $message = Read-Host

        if ([string]::IsNullOrWhiteSpace($message)) {
            continue
        }

        # Find latest run and item
        $run = Get-ChildItem ".\orquestrador\logs" -Directory -ErrorAction SilentlyContinue |
               Sort-Object LastWriteTime -Descending |
               Select-Object -First 1

        if (-not $run) {
            Write-Host "✗ Nenhuma execução encontrada. Inicia o orquestrador primeiro!" -ForegroundColor Red
            continue
        }

        $item = Get-ChildItem $run.FullName -Directory -ErrorAction SilentlyContinue |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1

        if (-not $item) {
            Write-Host "✗ Nenhum item em execução ainda. Aguarda..." -ForegroundColor Red
            continue
        }

        # Send via ask_agent.ps1
        Write-Host ""
        & ".\orquestrador\ask_agent.ps1" $message
        Write-Host ""
    }
    catch {
        if ($_.Exception.Message -match "Ctrl") {
            Write-Host ""
            Write-Host "Ask mode fechado (execução continua)." -ForegroundColor Green
            break
        }
        Write-Host "Erro: $_" -ForegroundColor Red
    }
}
