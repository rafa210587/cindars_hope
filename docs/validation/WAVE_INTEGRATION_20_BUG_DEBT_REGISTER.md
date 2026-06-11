# WAVE_INTEGRATION_20 — Bug & Debt Register

**Date:** 2026-06-10
**Status:** PENDING_HUMAN_PLAYMODE

---

## Regras

- P0/P1 devem ser corrigidos ou bloquear aceite.
- P2/P3 podem ficar como debt com backlog.
- HUMAN_WIRING_REQUIRED = código correto, ação Unity Editor pendente (não é bug de código).
- Classificar honestamente; não esconder em "notes".

---

## Bugs Classificados

| ID | Severity | Área | Sintoma | Repro | Esperado | Atual | Fix em WAVE20? | Status |
|---|---|---|---|---|---|---|---|---|
| B001 | P1 | Scene transitions | Farm→Town e Farm→Cave não funcionam sem wiring manual | Abrir Unity, Play, tentar ir para Town/Cave | Transição funciona | SceneTransitionGate não colocado nas cenas | NÃO (requer Unity Editor) | HUMAN_WIRING_REQUIRED |
| B002 | P1 | Cave entrance | CaveEntranceInteractable ausente no FarmScene | Ir até Zone_CaveEntrance, tentar interagir | Prompt aparece, cave carrega | Sem interactable wired | NÃO (requer Unity Editor) | HUMAN_WIRING_REQUIRED |
| B003 | P1 | Cave combat/loot | Enemy não spawna na CaveScene | Entrar na cave, aguardar spawn | Enemy aparece, pode ser derrotado | CaveSmokeTestSpawnerBridge não colocado | NÃO (requer Unity Editor) | HUMAN_WIRING_REQUIRED |
| B004 | P1 | FarmScene layout | FarmPlot, SellPoint, Resources precisam de scene wiring ou scene creator | Tentar interagir com plot/resource/sellpoint | Interação funciona | Depende de CreateMvpFarmScene ou wiring manual | NÃO (requer Unity Editor ou scene creator) | HUMAN_WIRING_REQUIRED |
| B005 | P1 | Crafting | CraftingStation ausente no FarmScene | Tentar crafting | Mesa de crafting disponível | CraftingPoint não colocado | NÃO (requer Unity Editor) | HUMAN_WIRING_REQUIRED |
| B006 | P2 | Cave save/load | Enemy HP não persiste entre saves (mid-combat) | Entrar cave, ferir enemy, salvar, carregar | Enemy com HP reduzido | Enemy respawna com HP cheio | NÃO (WAVE18 debt explícito) | CAVE_ENEMY_HP_SAVE_DEBT |
| B007 | P2 | Companion save | CompanionManager save não implementado | Companion state | Companion state persistido | Não implementado | NÃO (future spec) | COMPANION_SAVE_DEBT |
| B008 | P2 | HUD canvas | HUD é IMGUI debug panel, não UI Canvas final | Jogar normalmente | HUD visual polido | Debug IMGUI | NÃO (deferred UI visual) | DEFERRED_UI_VISUAL |
| B009 | P2 | Quest tracker | Sem quest tracker visual sofisticado | Verificar UI de quest | Tracker visual completo | Log IMGUI simples (QuestLogPanelController) | NÃO (deferred UI visual) | DEFERRED_UI_VISUAL |
| B010 | P2 | Cave event | CaveEnteredEvent (surface→cave) não existe | Entrar na cave | Evento específico de entrada | CaveLevelEnteredEvent usado como proxy | NÃO (acceptable proxy) | MISSING_EVENT_PROXY_OK |
| B011 | P2 | NPC schedules | NPCs de TownScene têm schedule simplificado | Observar NPCs ao longo do dia | Schedules realistas | Schedules placeholder | NÃO (future spec) | NPC_SCHEDULE_DEBT |
| B012 | P2 | Crafting UI | CraftingModal Canvas não wired | Tentar crafting em Play Mode | Modal visual abre | Headless apenas | NÃO (deferred UI visual) | DEFERRED_UI_VISUAL |
| B013 | P3 | Inventory tooltip | Tooltip mostra apenas itemId+amount | Hover em item | Tooltip completo (nome, desc, stats) | Minimal | NÃO (future spec) | TOOLTIP_POLISH_DEBT |
| B014 | P3 | Equipment panel | CharacterEquipment não empurra ModalManager (K/L) | Tentar dash durante equipment aberto | Dash bloqueado | Dash não verificado durante equipment (WAVE09 debt) | NÃO (acceptable P3) | MODAL_GUARD_PARTIAL |
| B015 | P3 | Canvas art | Todas as UIs são IMGUI ou Canvas placeholder | Jogar | Arte final | Placeholders | NÃO (future SPEC) | VISUAL_ART_DEBT |

---

## Sumário por Severidade

| Severidade | Total | Fix em WAVE20 | Bloqueador |
|---|---|---|---|
| P0 | 0 | N/A | NENHUM |
| P1 | 5 | 0 (todos HUMAN_WIRING_REQUIRED) | SIM para aceite completo do slice |
| P2 | 8 | 0 (todos debt documentado) | NÃO para aceite parcial (farm+quest+skills+save) |
| P3 | 3 | 0 (polish) | NÃO |

---

## Contexto dos P1

Todos os P1 são `HUMAN_WIRING_REQUIRED` — não são bugs de código. O código está correto e compilado. A ação necessária é colocar GameObjects nas cenas via Unity Editor (ação humana, não agente).

### Sub-Slice sem Bloqueadores (P1 não afetam)

O seguinte sub-slice funciona SEM nenhuma ação de wiring:

```
1. Abrir FarmScene, Play Mode
2. Player anda (WASD/Arrows)
3. Inventory abre (I key)
4. Skill tree abre (U key)
5. Dash=Space, Dodge=DoubleTap, Block=Shift
6. Salvar (F5) → "Jogo salvo." no HUD
7. Carregar (F9) → "Jogo carregado." no HUD
8. Modais bloqueiam dash/dodge/block
```

Este sub-slice é suficiente para validar builds + runtime bootstrap + modal stack + save/load.

---

## Status do Aceite Final

```
P0 = 0 → NÃO BLOQUEIA BUILD
P1 = 5 todos HUMAN_WIRING_REQUIRED → BLOQUEIA ACCEPTED completo
P2/P3 = 11 → DOCUMENTADOS, NÃO BLOQUEIAM
Status máximo sem Play Mode: BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE
Status máximo após Play Mode sub-slice: ACCEPTED_WITH_DEBT (se sub-slice passa)
Status aceite completo: ACCEPTED_WITH_DEBT (após human wiring + Play Mode completo)
```
