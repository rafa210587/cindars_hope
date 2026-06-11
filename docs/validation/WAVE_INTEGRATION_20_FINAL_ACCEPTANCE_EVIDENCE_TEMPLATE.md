# WAVE_INTEGRATION_20 — Final Acceptance Evidence Template

**Date:** 2026-06-10
**Status:** PENDING_HUMAN_EXECUTION

---

## Instruções

Este template deve ser preenchido pelo humano após executar o Play Mode checklist.
Não preencher campos de evidência sem ter realmente executado as ações.

---

## Identificação da Sessão de Teste

```
Unity version:         [ex: Unity 2022.3.x LTS]
Branch:                dev
Commit:                [hash do commit no momento do teste]
Date/time:             [ex: 2026-06-10 18:30]
Tester:                [nome/alias do testador]
Plataforma:            Windows 11
```

---

## Cenas Testadas

```
[ ] FarmScene
[ ] TownScene (se Farm→Town transition wired)
[ ] CaveScene (se WAVE16+17 wired)
```

---

## Resultado do Checklist

```
Total de steps:        56
Steps executados:      __/56
Steps passaram:        __/56
Steps falharam:        __/56
Steps N/A (wiring):    __/56
```

---

## Contagem de Bugs

```
P0 bugs encontrados:   [número]
P1 bugs de código:     [número]
P1 wiring pendente:    [número] (HUMAN_WIRING_REQUIRED — não bloqueiam com workaround)
P2 bugs novos:         [número]
P3 bugs novos:         [número]
```

---

## Evidência por Área

### Boot e Player
```
[ ] Player spawna em Play Mode
[ ] Player anda normalmente
[ ] DebugHud visível (gold/HP/stamina/hotbar)
[ ] Console sem NullReferenceException recorrente
```

### Farm Loop
```
[ ] FarmPlot interagível (ou N/A — wiring pendente)
[ ] Plantar/regar/colher funciona (ou N/A)
[ ] Resource interactable funciona (ou N/A)
[ ] SellPoint vende e muda gold (ou N/A)
```

### Inventory e Modal Guard
```
[ ] I key abre inventory
[ ] Dash bloqueado durante inventory
[ ] Escape fecha inventory
[ ] Input retorna após fechar
```

### Economy
```
[ ] Comprar item no NPC (se Town transition wired)
[ ] Gold reduz corretamente (ou N/A)
[ ] Vender funciona (ou N/A)
```

### Quest
```
[ ] Thalindra mostra "! Qual é a tarefa?" (se Town wired)
[ ] Quest aceita (ou N/A)
[ ] QuestLog abre com J (ou N/A)
[ ] Quest completa com reward (ou N/A)
[ ] Reward não duplica (idempotência)
```

### Skill Tree
```
[ ] U key abre skill tree
[ ] Comprar node funciona
[ ] Active slot R/T/Y/G atualiza
[ ] Dash/Dodge/Block executam
```

### Cave (se wired)
```
[ ] CaveEntrance prompt aparece (ou N/A)
[ ] Cave carrega (ou N/A)
[ ] Enemy spawna (ou N/A)
[ ] Loot cai no inventory (ou N/A)
[ ] Cave exit funciona (ou N/A)
```

### Save/Load
```
[ ] F5 salva → "Jogo salvo." no HUD
[ ] F9 carrega → "Jogo carregado." no HUD
[ ] Gold preservado após load
[ ] Quest state preservada após load
[ ] Skill equipada preservada após load
```

---

## Screenshots / Videos

```
Screenshot 1: [caminho ou URL]
Screenshot 2: [caminho ou URL]
Video: [caminho ou URL]
```

---

## Decisão Final

```
P0 bugs: [número] → [se > 0: REJECTED]
P1 bugs de código: [número] → [se > 0: REJECTED]
P1 wiring: [número] → [se presente: bloqueia ACCEPTED completo mas não ACCEPTED_WITH_DEBT do sub-slice]
Fluxo mínimo sub-slice passou: [SIM/NÃO]

DECISÃO:
  [ ] ACCEPTED           — P0=0, P1=0, fluxo completo passou
  [ ] ACCEPTED_WITH_DEBT — P0=0, P1=0 código, fluxo mínimo passou, debts documentados
  [ ] REJECTED_BLOCKED   — P0 ou P1 código encontrado; listar bugs abaixo
```

---

## Bugs Novos Encontrados em Play Mode

| ID | Severidade | Sintoma | Repro | Notas |
|---|---|---|---|---|
|  |  |  |  |  |

---

## Assinatura

```
Testador: ___________________
Data: ___________________
Decisão: ___________________
```
