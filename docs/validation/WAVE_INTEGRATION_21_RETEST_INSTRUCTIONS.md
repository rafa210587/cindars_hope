# WAVE_INTEGRATION_21 — Retest Instructions

**Date:** 2026-06-10
**Status:** NO_OP_NO_P0_P1_FOUND

---

## Contexto

WAVE21 não corrigiu código — resultado foi NO_OP_NO_P0_P1_FOUND.

Portanto, não há passos de "repro anterior" nem "passos de reteste específicos de fix".

O reteste é equivalente ao checklist original da WAVE20.

---

## Branch e Commit

```
Branch:    dev
Commit:    4ffcfd6 (WAVE20) — nenhum novo commit de código em WAVE21
Remote:    origin/dev
```

---

## Bugs Corrigidos por Esta Wave

Nenhum — NO_OP.

---

## Pré-condições para Reteste

1. Unity Editor aberto, sem erros vermelhos.
2. Branch `dev`, commit mais recente.
3. FarmScene como cena inicial.

---

## Passos de Repro dos P1 HUMAN_WIRING (para referência)

Estes P1 não foram fixados por código — precisam de wiring humano:

### B001 — Scene transitions (Farm→Town→Cave)

**Repro:** Abrir FarmScene em Play Mode. Caminhar até a saída para Town. Sem gate wired, não há transição.

**Resolução humana:** Seguir `docs/validation/WAVE_INTEGRATION_13_HUMAN_UNITY_SCENE_WIRING_INSTRUCTIONS.md`.

### B002 — Cave entrance

**Repro:** Caminhar até Zone_CaveEntrance no FarmScene. Sem CaveEntranceInteractable wired, não há prompt.

**Resolução humana:** Seguir `docs/validation/WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md`.

### B003 — Cave combat/loot

**Repro:** Entrar na cave (se B002 resolvido). Enemy não spawna sem CaveSmokeTestSpawnerBridge.

**Resolução humana:** Seguir `docs/validation/WAVE_INTEGRATION_17_HUMAN_PLAYMODE_CHECKLIST.md`.

### B004 — FarmScene layout

**Repro:** Tentar interagir com FarmPlot, Resource, SellPoint. GameObjects ausentes.

**Resolução humana:** Rodar `CindarsHope/Integration/Create MVP Farm Scene` no Unity Editor.

---

## Checklist de Reteste (Sub-Slice Imediato)

Executar os steps 1-5, 12-14, 33-36, 46-53 do WAVE20 checklist:

```
docs/validation/WAVE_INTEGRATION_20_FINAL_HUMAN_PLAYMODE_CHECKLIST.md
```

Estes steps não dependem de wiring manual e validam o core engine.

---

## Ordem Recomendada no Unity

```
1. git pull origin dev (confirmar commit 4ffcfd6)
2. Abrir Unity Editor → FarmScene
3. Press Play
4. Executar sub-slice: boot → inventory → skill tree → save/load → modal guards
5. (Opcional) Executar wiring manual per WAVE13/16/17 instructions
6. (Opcional) Executar checklist completo (56 steps)
7. Preencher WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md
```

---

## Resultado Esperado

```
Sub-slice (sem wiring):  ACCEPTED_WITH_DEBT
Slice completo (com wiring):  ACCEPTED_WITH_DEBT
P0 esperados:  0
P1 código esperados:  0
```

---

## O Que Registrar se Falhar

Se algum step falhar que não estava falhando antes de WAVE19/20:

```
1. Identificar commit que introduziu regressão (git log + git bisect)
2. Registrar em WAVE_INTEGRATION_20_BUG_DEBT_REGISTER.md (novo bug)
3. Classificar como P0/P1 se bloquear slice
4. Abrir WAVE22 se necessário para corrigir
```
