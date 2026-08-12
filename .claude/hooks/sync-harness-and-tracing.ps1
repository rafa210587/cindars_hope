# Sync Harness & Tracing Hook (evento Stop)
# Mantem a INTERCAMBIALIDADE Claude<->Codex e o TRACING de specs sempre frescos,
# sem depender de o agente lembrar de rodar os geradores.
#
# - Se as fontes do harness mudaram (CLAUDE.md, .claude/{skills,rules,agents,commands,settings.json}),
#   re-roda tools/codex/Generate-CodexHarness.ps1 (materializa AGENTS.md + .codex/ + .agents/ a partir de .claude/).
# - Se .specs/** mudou, re-roda tools/generate_spec_index.ps1 (regenera SPEC_INDEX.md/.json).
#
# Nao bloqueia o Stop: e conveniencia de sincronia, nao um guard. Sempre sai 0; falha vira aviso.
# Sem risco de loop: as saidas geradas (AGENTS.md, .codex/, .agents/, SPEC_INDEX.*) NAO sao fontes
# monitoradas aqui, entao regenera-las nao re-dispara o hook.

$ErrorActionPreference = 'Continue'

# Raiz do repo (o hook fica em .claude/hooks/)
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $repo

# Coleta os paths sujos (modificados, staged e untracked), normalizando barras.
$dirty = @()
try {
    $porcelain = git status --porcelain 2>$null
    if ($porcelain) {
        foreach ($line in ($porcelain -split "`n")) {
            if ($line -match '\S') {
                # Formato porcelain: 'XY <path>' (path a partir da coluna 4); tolera '-> ' de rename.
                $path = ($line.Substring(3)).Trim()
                if ($path -match '->') { $path = ($path -split '->')[-1].Trim() }
                $path = $path.Trim('"')
                $dirty += ($path -replace '\\', '/')
            }
        }
    }
}
catch {
    Write-Host "[sync-harness] git status indisponivel; nada a sincronizar."
    exit 0
}

if ($dirty.Count -eq 0) { exit 0 }

# Fontes que exigem re-gerar o harness do Codex.
$harnessChanged = $dirty | Where-Object {
    $_ -eq 'CLAUDE.md' -or
    $_ -eq '.claude/settings.json' -or
    $_ -match '^\.claude/(skills|rules|agents|commands)/'
}

# Mudancas na arvore de specs exigem re-gerar o indice.
$specsChanged = $dirty | Where-Object { $_ -match '^\.specs/' }

if ($harnessChanged) {
    Write-Host "[sync-harness] Fontes do harness mudaram -> regenerando o harness do Codex..."
    try {
        & powershell -NoProfile -ExecutionPolicy Bypass -File 'tools/codex/Generate-CodexHarness.ps1' | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[sync-harness] AVISO: Generate-CodexHarness.ps1 saiu com codigo $LASTEXITCODE. Rode manualmente e verifique .codex/ / AGENTS.md."
        } else {
            Write-Host "[sync-harness] Codex sincronizado (AGENTS.md + .codex/ + .agents/)."
        }
    }
    catch {
        Write-Host "[sync-harness] AVISO: falha ao rodar Generate-CodexHarness.ps1: $($_.Exception.Message)"
    }
}

if ($specsChanged) {
    Write-Host "[sync-harness] .specs/ mudou -> regenerando SPEC_INDEX..."
    try {
        & powershell -NoProfile -ExecutionPolicy Bypass -File 'tools/generate_spec_index.ps1' | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[sync-harness] AVISO: generate_spec_index.ps1 saiu com codigo $LASTEXITCODE."
        } else {
            Write-Host "[sync-harness] SPEC_INDEX.md/.json atualizados."
        }
    }
    catch {
        Write-Host "[sync-harness] AVISO: falha ao rodar generate_spec_index.ps1: $($_.Exception.Message)"
    }
}

exit 0
