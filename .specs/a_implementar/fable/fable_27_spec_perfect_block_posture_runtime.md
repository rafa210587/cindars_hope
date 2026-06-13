# SPEC — Combate: Perfect Block + Janela CoreExposed por Quebra de Postura

> **Spec ID:** `fable_27_spec_perfect_block_posture_runtime`
> **Status:** BUILD_VALIDATED (executada — E10; evidência: docs/validation/fable_27_spec_perfect_block_posture_runtime_execution_report.md) | **Wave:** FABLE Batch 2 | **Priority:** P1
> **Type:** Runtime | **Domain:** Combat / Player
> **Parallelizable:** NO (combat core lock)
> **Must not run with:** F02, F03 (mesma cadeia de combate — ordem F02 → F27)
> **Repo lock scope:** BlockController/PlayerCombat, postura (F02), DamageCalculator
> **Depends on:** F02 (postura/stagger/CriticalWindow implementados)
> **Blocks:** F23 (relíquia de Kanthor consome hook), F29 (skills de bloqueio)
> **Scope:** perfect block (janela 0.15s) + reflexo de postura + CoreExposed canônico.
> **Out de scope:** parry com arma (charged da Sword cobre — F02), block de projéteis mágicos especiais.

required_adrs: []
required_game_rules: [combat_rules.md]

## Contexto / Problema / Objetivo

COMBAT_CORE define: block 18 STA/s drenagem, perfect block na janela inicial (0.12-0.18s)
nega TODO o dano, devolve dano de POSTURA ao atacante e não consome stamina do hit;
quebra de postura inimiga abre CoreExposed (crit garantido + bônus). O block atual é
mitigação simples sem janela. Objetivo: timing real de defesa — o coração do "combate
expressivo" da direction.

## Fontes obrigatórias

```text
docs/design/gameplay/combat/COMBAT_CORE_MECHANICS_DIRECTION.md (block/postura/janelas)
docs/design/gameplay/combat/COMBAT_BALANCE_BASELINE (números)
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe: BlockController/block input (hold), stamina drain, postura inimiga (F02),
CriticalWindow/stagger (F02), DamageCalculator provider. Não existe: janela de perfect
block, reflexo de postura, CoreExposed como estado distinto de stagger.
Auditar Fase 0: onde o block intercepta o dano hoje.
```

## Escopo

```text
- PerfectBlockWindow: 0.15s a partir do início do block (configurável no balance asset);
  hit nessa janela → dano 0, stamina 0, atacante recebe dano de postura = 40% do dano
  bruto do golpe, SFX/flash distinto, evento PerfectBlockEvent (hook p/ relíquia F23);
- block normal (fora da janela): mitigação existente + drenagem 18 STA/s mantidas;
- CoreExposed: quando postura inimiga quebra → 1.2s de stagger normal + 1.5s adicionais
  de CoreExposed (crit garantido + multiplicador ×1.3 sobre o crit) — estado visível
  (tint/ícone placeholder);
- anti-spam: re-block dentro de 0.4s não reabre a janela perfeita (cooldown da janela);
- inimigos com escudo (Veilkin Warden): perfect block do PLAYER quebra guard deles
  imediatamente (interação canônica);
- EditMode tests: janela por timestamps puros, cooldown anti-spam, reflexo de postura,
  CoreExposed timing/multiplicador, drenagem inalterada.
```

## Fora de escopo / Não duplicação

```text
Não duplicar mitigação — janela decide ANTES do caminho atual. Parry de arma = charged
Sword (F02). Sem i-frame novo (dodge é o único i-frame — canon).
```

## Critérios de aceite

```text
CA-1: hit aos 0.10s de block = perfeito (0 dano, postura refletida); aos 0.30s = normal.
CA-2: re-block em 0.3s não rearma janela (anti-spam).
CA-3: quebra de postura → stagger + CoreExposed com crit garantido ×1.3 (teste calculadora).
CA-4: drenagem 18 STA/s e mitigação normal sem regressão (testes existentes passam).
```

# /speckit.plan

```text
Arquitetura: BlockController (janela+cooldown), DamageCalculator (branch perfect/CoreExposed),
EnemyBrain hook (postura refletida/guard break), Core/Events/CombatEvents (aditivo),
Tests/EditMode/Combat/PerfectBlockTests.cs.
Save: NO. Eventos: +1. UI: flash placeholder. Play Mode final: YES (DEFERRED — feel).
Risco: janela injusta com lag de input → medir do INPUT, não do frame de animação.
Rollback: janela configurável 0 = comportamento antigo.
```

# /speckit.tasks

```md
- [ ] T001 — Auditar caminho block→dano atual.
- [ ] T002 — Janela perfeita + cooldown + reflexo (helpers puros + testes).
- [ ] T003 — CoreExposed pós-quebra + multiplicador.
- [ ] T004 — Guard break de inimigo escudado; evento; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES | EditMode: YES | PlayMode/human: YES (timing feel é humano) | Regression: YES
- Human timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum evidence: testes + cenário humano com perfect block e quebra de postura

## DoD / Anti-regressão

```text
Perfect block canônico + CoreExposed funcionais; dodge segue único i-frame; mitigação
normal intacta. Builds 0E; report.
```
