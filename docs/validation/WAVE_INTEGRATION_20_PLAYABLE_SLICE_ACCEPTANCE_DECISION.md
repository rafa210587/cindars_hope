# WAVE_INTEGRATION_20 — Playable Slice Acceptance Checklist + Closeout: Decision Report

**Date:** 2026-06-10
**Status:** `BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE`
**Branch:** dev

---

## 1. Fontes Lidas

### Processo
- `CLAUDE.md` — router de contexto e regras
- `docs/project/CURRENT_STATE.md` — estado ativo do projeto (fonte primária)
- `AGENTS.md` — regras de agente (aplicadas sem leitura completa)

### Roadmap
- Spec WAVE_INTEGRATION_20 fornecida via IDE selection — `SOURCE_PROVIDED_BY_USER_NOT_IN_REPO`
- Fluxo mínimo do roadmap: 15 steps (entrar no jogo → confirmar estado persistido)

### Reports de Waves Anteriores
- `WAVE_INTEGRATION_13_SCENE_TRANSITION_REPORT.md` — LIDO ✓
- `WAVE_INTEGRATION_14_CRAFTING_PROCESSING_REPORT.md` — LIDO ✓
- `WAVE_INTEGRATION_15_QUEST_GIVER_REPORT.md` — LIDO ✓
- `WAVE_INTEGRATION_16_CAVE_ENTRANCE_REPORT.md` — LIDO ✓
- `WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_REPORT.md` — LIDO ✓
- `WAVE_INTEGRATION_18_SAVE_LOAD_GAP_REPORT.md` — LIDO ✓
- `WAVE_INTEGRATION_19_HUD_UX_ACCEPTANCE_REPORT.md` — LIDO ✓

### Fontes de design (presença confirmada, não lidas por proibição de contexto durante implementação)
- `docs/design/SPEC_SOURCE_MAP.md` — presença confirmada
- `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md` — presença confirmada
- `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` — presença confirmada
- `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md` — presença confirmada
- `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md` — presença confirmada
- `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md` — presença confirmada
- `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md` — presença confirmada
- `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md` — presença confirmada
- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md` — presença confirmada
- `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md` — presença confirmada
- `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md` — presença confirmada

---

## 2. Roadmap Alignment

### WAVE20 = SPEC 20 do Roadmap
Esta spec é a execução do item "Playable Slice Acceptance Checklist" do roadmap macro.

Fluxo mínimo do roadmap a cobrir:
```
1.  Entrar no jogo.
2.  Andar na farm.
3.  Plantar/regar/colher.
4.  Coletar recurso.
5.  Vender item.
6.  Abrir inventory.
7.  Comprar/vender no NPC.
8.  Pegar quest.
9.  Completar quest.
10. Comprar/equipar skill.
11. Usar skill.
12. Entrar na cave.
13. Derrotar inimigo/coletar loot.
14. Salvar/carregar.
15. Confirmar estado persistido.
```

Resultado esperado: **Primeira versão realmente jogável do MVP.**

---

## 3. Preflight Results

| Check | Result |
|---|---|
| Branch | `dev` ✓ |
| Working tree | CLEAN ✓ |
| Assembly-CSharp (before) | PASS (0E/0W) ✓ |
| Assembly-CSharp-Editor (before) | PASS (0E/3W pre-existing) ✓ |
| WAVE18 gate | SATISFIED — BUILD_VALIDATED_SAVE_LOAD_GAP_CLOSED ✓ |
| WAVE19 gate | SATISFIED — BUILD_VALIDATED_HUD_UX_ACCEPTANCE_READY ✓ |
| WAVE20 already exists | NO — primeira execução ✓ |

---

## 4. Wave Baseline Table

| Sistema | WAVE | Status Report | Play Mode Required? | Blocking for Slice? | Action |
|---|---|---|---|---|---|
| Scene transitions | 13 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED | YES | P1 (gate_on_wiring) | Human: place gates/anchors in scenes |
| Crafting/processing | 14 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED | YES | P2 (não no roteiro mínimo) | Human: place CraftingStation in FarmScene |
| Quest giver/log | 15 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED | YES | P1 (gate_on_wiring) | Runtime auto-wire via QuestRuntimeBootstrap; Thalindra patches from FIX-001/001B |
| Cave entrance | 16 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED | YES | P1 (gate_on_wiring) | Human: place CaveEntranceInteractable in FarmScene |
| Cave combat/loot | 17 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED | YES | P1 (gate_on_wiring) | Human: place CaveSmokeTestSpawnerBridge + EnemyDropSpawner in CaveScene |
| Save/load gap closure | 18 | BUILD_VALIDATED_SAVE_LOAD_GAP_CLOSED_PENDING_HUMAN_PLAYMODE | YES | SAFE_WITH_DEBT | Human: execute WAVE18 checklist |
| HUD/UX/debug validation | 19 | BUILD_VALIDATED_HUD_UX_ACCEPTANCE_READY_PENDING_HUMAN_PLAYMODE | YES | SAFE_WITH_DEBT | Human: execute WAVE19 checklist (40 steps) |
| Farm loop baseline | WAVE05 | COMPLETED_WITH_KNOWN_LEGACY_GATES (~123 EditMode tests) | YES | SAFE | Code complete; PlayMode wiring needed |
| Inventory/equipment | WAVE04/07/09 | BUILD_VALIDATED_WITH_UI_DEBT | YES | SAFE | InventoryPanelController + CharEquipmentPanel implemented |
| Skill tree/effects | WAVE10/11 | BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE | YES | SAFE | SkillTreeGameplayPanelController + runtime input wired |

---

## 5. Definition of Acceptance

### Para ACCEPTED (sem debt)
```
P0 = 0
P1 = 0
P2 = 0 (ou documentados sem impacto no slice)
Todos os 26 steps do checklist humano passam
Build passa (0E/0W runtime)
Play Mode executado por humano
```

### Para ACCEPTED_WITH_DEBT (aceite com debt)
```
P0 = 0
P1 = 0 (ou todos P1 são HUMAN_WIRING_REQUIRED com workaround documentado)
P2/P3 documentados no bug register
Fluxo mínimo dos 15 steps do roadmap passa
Build passa
Play Mode executado por humano
```

### Para BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE (status atual)
```
Builds passam
Checklists/matrizes criadas
P0/P1 classificados
Play Mode ainda não executado
```

---

## 6. Arquivos Permitidos / Proibidos

### Permitidos (código)
```
Assets/_Game/Scripts/UI/**/*
Assets/_Game/Scripts/NPC/NpcShopController.cs
Assets/_Game/Scripts/Quests/Runtime/**/*
Assets/_Game/Scripts/Save/**/*
Assets/_Game/Scripts/Cave/Runtime/**/*
Assets/_Game/Scripts/Loot/Runtime/**/*
Assets/_Game/Scripts/Player/Movement/**/*
Assets/_Game/Scripts/Interaction/**/*
Assets/_Game/Scripts/Editor/Validation/ValidateWave20PlayableSliceAcceptance.cs
```

### Proibidos
```
Packages/**
ProjectSettings/**
Library/**
Temp/**
Logs/**
.claude/*.lock
Assets/TextMesh Pro/**
docs/design/** durante implementação
Assets/_Game/Scenes/** (exceto se correção P0/P1 documentada)
```

---

## 7. Severidade — Definições

### P0 — bloqueia totalmente
```
Unity com erro vermelho persistente; projeto não compila; Play Mode não inicia;
player não aparece/move; save corrompe dados; crash recorrente.
```

### P1 — bloqueia aceite do slice
```
farm loop básico não funciona; inventory não abre; economy não funciona;
Thalindra não oferece quest; quest não completa; skill não equipa/usa;
cave não entra/sai; enemy/loot mínimo não funciona;
save/load não preserva estado principal; HUD não mostra feedback mínimo;
dash/dodge/block furam modal.
```

### P2 — aceitável com debt
```
UI feia; tooltip incompleto; NPC schedule simplificado;
cave enemy placeholder; loot balance temporário; debug panel simples;
efeitos visuais ausentes.
```

### P3 — polish
```
texto melhorável; posicionamento visual imperfeito; logs verbosos.
```

---

## 8. Design/Direction Compliance Matrix

| Source | Rule Extracted | Impact on WAVE20 | Applied? | Evidence |
|---|---|---|---|---|
| Roadmap playable slice | SPEC20 deve consolidar checklist humano final | Checklist final é o artefato primário | SIM | FINAL_HUMAN_PLAYMODE_CHECKLIST.md criado |
| SPECIFICATION_PROCESS.md | Specs devem declarar fontes/estado/validação | Estrutura completa usada | SIM | Todas seções preenchidas |
| UI_UX directions | UX deve expor estado acionável e bloquear input vazado | Modal/input checks | SIM | Modal guard matrix WAVE19 reutilizada |
| SAVE_LOAD direction | save/load deve preservar estado sem duplicação/perda | Cases de save/load no checklist | SIM | Steps 23-24 no checklist |
| QUEST direction | estado/reward idempotency deve se manter | Cases de quest no checklist | SIM | Steps 13-15 no checklist |
| CAVE direction | cave loop deve ser jogável e stateful | Cases de cave no checklist | SIM | Steps 19-22 no checklist (conditional) |
| CURRENT_STATE | status do projeto é a fonte de verdade | Gates baseados em reports reais | SIM | Wave baseline table preenchida |
| INVARIANT_RULES | no GameObject.Find, no GameEventBus bypass | Correções P0/P1 somente via sistema existente | SIM | Sem sistemas paralelos criados |
