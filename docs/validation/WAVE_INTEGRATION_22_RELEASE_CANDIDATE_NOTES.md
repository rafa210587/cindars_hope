# WAVE_INTEGRATION_22 — Release Candidate Notes

**Date:** 2026-06-10
**Status:** BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE
**Branch:** dev
**Commit (WAVE22):** TBD (será preenchido após commit desta spec)
**Commit (WAVE21 base):** be69e24
**Commit (WAVE20 base):** 4ffcfd6

---

## MVP Status

```
Playable Slice Status: BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE

O que significa:
- Todo o código está escrito e compila sem erros
- Todos os sistemas estão integrados via GameEventBus
- Build C# PASS em ambos os assemblies
- 18+ checklists de Play Mode humano criados
- 56-step checklist de aceitação criado (WAVE20)
- Human Play Mode NÃO FOI executado ainda
- Scene wiring manual NÃO foi feito ainda (4 P1 HUMAN_WIRING_REQUIRED)

O que pode ser testado AGORA (sem wiring manual):
- Boot → FarmScene Play Mode
- Player movement (WASD/Arrows)
- DebugHud visibility (gold/HP/stamina)
- Inventory (I key → IMGUI panel abre)
- Skill tree (U key → IMGUI panel abre)
- Dash (Space + direction), Dodge (double-tap direction), Block (Left Shift)
- Save (F5 → "Jogo salvo." no HUD)
- Load (F9 → "Jogo carregado." no HUD)
- Modal guards (inventory/skill tree open → dash/dodge/block bloqueados)
```

---

## Known Issues Aceitos

| DebtId | Severity | Issue | Aceitável? |
|---|---|---|---|
| DEBT-SCENE-001 | P1-WIRING | Farm→Town→Cave sem wiring manual | SIM para sub-slice; NÃO para release completo |
| DEBT-SCENE-002 | P1-WIRING | Cave entrance não interagível sem wiring | SIM para sub-slice |
| DEBT-SCENE-003 | P1-WIRING | Cave combat não funciona sem wiring | SIM para sub-slice |
| DEBT-SCENE-004 | P1-WIRING | FarmPlot/SellPoint sem wiring | SIM para sub-slice |
| DEBT-SCENE-005 | P2 | CraftingStation ausente | SIM — not in minimum roadmap |
| DEBT-UX-001 | P2 | HUD é IMGUI debug | SIM — funcional |
| DEBT-UX-002 | P2 | Quest tracker IMGUI | SIM — funcional |
| DEBT-SAVE-001 | P2 | Cave enemy HP não persiste | SIM — cave save deferred |
| DEBT-SAVE-002 | P2 | Companion save ausente | SIM — companion system future |
| DEBT-CAVE-002 | P2 | CaveEnteredEvent proxy | SIM — aceitável como proxy |
| DEBT-NPC-001 | P2 | NPC schedules placeholder | SIM — content debt |
| DEBT-FARM-001 | P2 | CraftingModal não wired | SIM — crafting deferred |
| DEBT-UX-003 | P3 | Tooltip minimal | SIM — polish |
| DEBT-UX-004 | P3 | Equipment panel sem ModalManager | SIM — trivial fix pendente |
| DEBT-UX-005 | P3 | Canvas art placeholders | SIM — visual debt |
| DEBT-AUDIO-001 | P3 | Sem audio feedback | SIM — audio not in scope |

---

## Como Rodar

### Pre-requisitos

```
1. Unity (versão do projeto)
2. git checkout dev
3. git pull origin dev
4. Abrir projeto no Unity Editor
5. Aguardar compilação
```

### Sub-slice (sem wiring)

```
1. Abrir FarmScene no Unity Editor
2. Press Play
3. Testar movement, inventory, skill tree, dash/dodge/block, save/load
4. Verificar modal guards
5. Preencher steps 1-5, 12-14, 33-36, 46-53 do WAVE20 checklist
```

### Slice completo (com wiring manual)

```
1. Executar CreateMvpFarmScene: CindarsHope/Integration/Create MVP Farm Scene
2. Seguir WAVE_INTEGRATION_13_HUMAN_UNITY_SCENE_WIRING_INSTRUCTIONS.md
3. Seguir WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md
4. Seguir WAVE_INTEGRATION_17_HUMAN_PLAYMODE_CHECKLIST.md (cave combat)
5. Executar todos os 56 steps do WAVE20 checklist
6. Preencher WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md
```

---

## Cenas a Abrir

```
FarmScene — cena inicial; player spawn; farm gameplay
TownScene — NPCs wired; shop/dialogue funcional (após WAVE13 wiring)
CaveScene — combat loop (após WAVE16+17 wiring)
```

---

## Limitações Esperadas

```
- HUD debug IMGUI (não é Canvas final)
- Inventory tooltip mostra apenas itemId+amount
- Quest tracker IMGUI simples
- Cave tem apenas 1 enemy (slime_basic) e 1 loot (stone)
- Apenas 1 questline de smoke test
- NPC schedules estáticos/placeholder
- Sem audio SFX
- Farm tem apenas 1 crop type (crop_carrot) e resources básicos
- Equipment panel não bloqueia dash (P3 trivial fix)
```

---

## Próxima Ação

```
1. IMEDIATO: Humano executa Play Mode sub-slice (sem wiring)
2. IMEDIATO: Preencher WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md
3. PRÓXIMOS DIAS: Executar wiring manual (MVP_PLUS_00)
4. PÓS-WIRING: Executar checklist completo (56 steps)
5. PÓS-ACEITE: Iniciar MVP_PLUS_01 (HUD Canvas) ou MVP_PLUS_04 (modal fix trivial)
```

---

## Validadores Disponíveis no Unity Editor

Execute no menu `CindarsHope/`:

```
CindarsHope/Validate Wave 19 HUD UX Acceptance Gate    — 22 checks
CindarsHope/Validate Wave 20 Playable Slice Acceptance  — 35 checks
CindarsHope/Validate Wave 21 Post-Acceptance Bugfix     — 19 checks
CindarsHope/Validate Wave 22 Debt Backlog               — TBD (validator WAVE22)
```

---

## Checklists de Play Mode Disponíveis

```
WAVE_INTEGRATION_06A_HUMAN_PLAYMODE_CHECKLIST.md    — debug loadout
WAVE_INTEGRATION_07_HUMAN_PLAYMODE_CHECKLIST.md     — economy/sell
WAVE_INTEGRATION_08_HUMAN_PLAYMODE_CHECKLIST.md     — HUD binding
WAVE_INTEGRATION_09_HUMAN_PLAYMODE_CHECKLIST.md     — inventory/equipment
WAVE_INTEGRATION_10_HUMAN_PLAYMODE_CHECKLIST.md     — skill tree
WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md     — movement actions
WAVE_INTEGRATION_12_HUMAN_PLAYMODE_CHECKLIST.md     — NPC/dialogue
WAVE_INTEGRATION_13_HUMAN_PLAYMODE_CHECKLIST.md     — scene transitions
WAVE_INTEGRATION_14_HUMAN_PLAYMODE_CHECKLIST.md     — crafting
WAVE_INTEGRATION_15_HUMAN_PLAYMODE_CHECKLIST.md     — quest giver
WAVE_INTEGRATION_16_HUMAN_PLAYMODE_CHECKLIST.md     — cave entrance
WAVE_INTEGRATION_17_HUMAN_PLAYMODE_CHECKLIST.md     — cave combat/loot
WAVE_INTEGRATION_18_HUMAN_PLAYMODE_CHECKLIST.md     — save/load
WAVE_INTEGRATION_19_HUMAN_PLAYMODE_CHECKLIST.md     — HUD/UX (40 steps)
WAVE_INTEGRATION_20_FINAL_HUMAN_PLAYMODE_CHECKLIST.md — slice completo (56 steps)
```
