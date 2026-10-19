# WAVE_INTEGRATION_22 — Canonical Debt Register

**Date:** 2026-06-10
**Status:** CONSOLIDATED
**Source:** WAVE13-21 reports + WAVE20 bug register

---

## Instruções de Uso

- IDs são estáveis — não renomear ao atualizar
- Disposition: FIX_NOW_TRIVIAL / NEXT_SPEC_REQUIRED / NEXT_ROADMAP_ITEM / ACCEPTED_DEBT / NEEDS_HUMAN_DECISION
- Prioridade: NOW / NEXT / LATER / ICEBOX
- WAVE20 B-IDs mapeados para IDs canônicos neste register

---

## Debts de Scene Wiring (SCENE_WIRING_DEBT)

| DebtId | WAVE20_Ref | Domain | Severity | Type | Descrição | Impact | Prioridade | Disposition | Spec Candidata |
|---|---|---|---|---|---|---|---|---|---|
| DEBT-SCENE-001 | B001 | SceneTransition | P1-WIRING | SCENE_WIRING_DEBT | Scene gates/anchors não colocados em FarmScene, TownScene, CaveScene | Farm→Town→Cave não funciona sem wiring | NOW | NEEDS_HUMAN_DECISION | MVP_PLUS_01_scene_wiring_validation |
| DEBT-SCENE-002 | B002 | CaveEntrance | P1-WIRING | SCENE_WIRING_DEBT | CaveEntranceInteractable não está no FarmScene | Cave não pode ser acessada | NOW | NEEDS_HUMAN_DECISION | MVP_PLUS_01_scene_wiring_validation |
| DEBT-SCENE-003 | B003 | CaveCombat | P1-WIRING | SCENE_WIRING_DEBT | CaveSmokeTestSpawnerBridge não está na CaveScene | Enemy não spawna na cave | NOW | NEEDS_HUMAN_DECISION | MVP_PLUS_01_scene_wiring_validation |
| DEBT-SCENE-004 | B004 | FarmScene | P1-WIRING | SCENE_WIRING_DEBT | FarmPlot/SellPoint/Resources precisam de CreateMvpFarmScene ou wiring manual | Farm gameplay core não funciona | NOW | NEEDS_HUMAN_DECISION | MVP_PLUS_01_scene_wiring_validation |
| DEBT-SCENE-005 | B005 | Crafting | P2 | SCENE_WIRING_DEBT | CraftingStation não está no FarmScene | Crafting não disponível em Play Mode | NEXT | NEXT_SPEC_REQUIRED | MVP_PLUS_02_farm_crafting_integration |

**Nota:** DEBT-SCENE-001 a 004 são HUMAN_WIRING_REQUIRED — código correto, ação Unity Editor pendente. Não são bugs de código.

---

## Debts de HUD/UX (UX_DEBT)

| DebtId | WAVE20_Ref | Domain | Severity | Type | Descrição | Impact | Prioridade | Disposition | Spec Candidata |
|---|---|---|---|---|---|---|---|---|---|
| DEBT-UX-001 | B008 | HUD | P2 | UX_DEBT | HUD é IMGUI debug panel, não Canvas final | Apresentação não-final; funcional mas não polido | NEXT | NEXT_ROADMAP_ITEM | MVP_PLUS_UI_01_hud_canvas_finalization |
| DEBT-UX-002 | B009 | QuestUI | P2 | UX_DEBT | Quest tracker é IMGUI simples (QuestLogPanelController) | Experiência de quest não polida | NEXT | NEXT_ROADMAP_ITEM | MVP_PLUS_UI_01_hud_canvas_finalization |
| DEBT-UX-003 | B013 | Inventory | P3 | UX_DEBT | Tooltip de inventário mostra apenas itemId+amount | Sem nome/descrição/stats | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_UI_02_inventory_tooltip_polish |
| DEBT-UX-004 | B014 | Equipment | P3 | UX_DEBT | CharacterEquipmentPanel não usa ModalManager stack (K/L keys) | Dash pode funcionar com equipment aberto | LATER | FIX_NOW_TRIVIAL | Trivial fix em CharacterEquipmentPanelController |
| DEBT-UX-005 | B015 | Visual | P3 | VISUAL_ASSET_DEBT | Todas as UIs são IMGUI ou Canvas placeholder | Apresentação não-final | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_UI_03_visual_canvas_art |

---

## Debts de Save/Load (SAVE_LOAD_DEBT)

| DebtId | WAVE20_Ref | Domain | Severity | Type | Descrição | Impact | Prioridade | Disposition | Spec Candidata |
|---|---|---|---|---|---|---|---|---|---|
| DEBT-SAVE-001 | B006 | CaveEnemy | P2 | SAVE_LOAD_DEBT | Enemy HP não persiste entre saves (mid-combat) | Enemy respawna com HP cheio após load | NEXT | NEXT_SPEC_REQUIRED | MVP_PLUS_SAVE_01_cave_enemy_state_persistence |
| DEBT-SAVE-002 | B007 | Companion | P2 | SAVE_LOAD_DEBT | CompanionManager save não implementado | Companion state perdida ao salvar/carregar | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_SAVE_02_companion_state_persistence |
| DEBT-SAVE-003 | — | PlayMode | P2 | TESTING_DEBT | Quest save/load round-trip não verificado em Play Mode | WAVE18 implementado mas Play Mode pending | NOW | NEEDS_HUMAN_DECISION | Executar WAVE18 checklist (humano) |

---

## Debts de Cave/Combat/Loot (CAVE_DEBT / COMBAT_DEBT / LOOT_DEBT)

| DebtId | WAVE20_Ref | Domain | Severity | Type | Descrição | Impact | Prioridade | Disposition | Spec Candidata |
|---|---|---|---|---|---|---|---|---|---|
| DEBT-CAVE-001 | — | CaveEnemy | P2 | CAVE_DEBT | Apenas 1 enemy type (slime_basic); 1 loot item (stone) | Cave combat superficial | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_CAVE_01_enemy_roster_expansion |
| DEBT-CAVE-002 | B010 | CaveEvent | P2 | CAVE_DEBT | CaveEnteredEvent proxy via CaveLevelEnteredEvent | Semântica imprecisa; aceitável para MVP | LATER | ACCEPTED_DEBT | Aceitar; criar evento real se necessário futuramente |
| DEBT-COMBAT-001 | — | Combat | P2 | COMBAT_DEBT | Combat balance não verificado in Play Mode | Damage/HP valores não testados por humano | NEXT | NEEDS_HUMAN_DECISION | Executar WAVE17 checklist (humano) |
| DEBT-LOOT-001 | — | Loot | P3 | LOOT_DEBT | Loot table mínima (1 item/enemy) | Economia de loot rasa | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_CAVE_01_enemy_roster_expansion |

---

## Debts de Crafting (FARM_DEBT / CRAFT_DEBT)

| DebtId | WAVE20_Ref | Domain | Severity | Type | Descrição | Impact | Prioridade | Disposition | Spec Candidata |
|---|---|---|---|---|---|---|---|---|---|
| DEBT-FARM-001 | B012 | CraftingUI | P2 | FARM_DEBT | CraftingModal Canvas não wired; crafting é headless | Crafting sem UI visual | NEXT | NEXT_SPEC_REQUIRED | MVP_PLUS_02_farm_crafting_integration |
| DEBT-FARM-002 | — | CraftingRecipes | P2 | FARM_DEBT | Smoke test recipes marcadas TEMPORARY_CRAFTING_TEST_RECIPE | Recipes de teste não são finais | NEXT | NEXT_SPEC_REQUIRED | MVP_PLUS_02_farm_crafting_integration |
| DEBT-FARM-003 | — | FarmLoop | P2 | FARM_DEBT | Farm loop depth superficial: 1 crop type, 1 resource cycle | Farm gameplay rasa | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_FARM_01_farm_loop_depth |

---

## Debts de NPC/Quest (NPC_DEBT / QUEST_DEBT)

| DebtId | WAVE20_Ref | Domain | Severity | Type | Descrição | Impact | Prioridade | Disposition | Spec Candidata |
|---|---|---|---|---|---|---|---|---|---|
| DEBT-NPC-001 | B011 | NPCSchedule | P2 | NPC_DEBT | NPCs de TownScene têm schedules simplificados/placeholder | NPCs não se movem realisticamente | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_NPC_01_npc_schedule_expansion |
| DEBT-QUEST-001 | — | QuestContent | P2 | QUEST_DEBT | Apenas 1 smoke test quest (quest_first_supplies_for_cindar) | Conteúdo de quest mínimo | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_QUEST_01_questline_expansion |
| DEBT-QUEST-002 | — | QuestSave | — | QUEST_DEBT | Quest save/load implementado (WAVE18) mas não verificado em Play Mode | Debt técnico resolvido; Play Mode pending | NOW | NEEDS_HUMAN_DECISION | Executar WAVE18 checklist |

---

## Debts de Testing (TESTING_DEBT)

| DebtId | WAVE20_Ref | Domain | Severity | Type | Descrição | Impact | Prioridade | Disposition | Spec Candidata |
|---|---|---|---|---|---|---|---|---|---|
| DEBT-TEST-001 | — | PlayMode | P2 | TESTING_DEBT | 18+ Play Mode checklists PENDING_HUMAN_EXECUTION | Acceptance parcial sem Play Mode | NOW | NEEDS_HUMAN_DECISION | Executar checklists humanos |
| DEBT-TEST-002 | — | Automation | P2 | TESTING_DEBT | Sem automated Play Mode tests | Regressões detectáveis apenas manualmente | LATER | NEXT_ROADMAP_ITEM | MVP_PLUS_TEST_01_playmode_automation |
| DEBT-TEST-003 | — | FIX001B | P2 | TESTING_DEBT | FIX-001B Play Mode checklist pendente (global search eliminado) | Validação humana de FIX-001B ausente | NOW | NEEDS_HUMAN_DECISION | Executar FIX_001B checklist |

---

## Debts de Audio (AUDIO_DEBT)

| DebtId | WAVE20_Ref | Domain | Severity | Type | Descrição | Impact | Prioridade | Disposition | Spec Candidata |
|---|---|---|---|---|---|---|---|---|---|
| DEBT-AUDIO-001 | — | Audio | P3 | VISUAL_ASSET_DEBT | Sem sistema de audio feedback (SFX para attack, collect, save, etc.) | Experiência imersiva reduzida | ICEBOX | NEXT_ROADMAP_ITEM | MVP_PLUS_AUDIO_01_sfx_feedback_system |

---

## Sumário por Severidade

| Severidade | Total | Disposition breakdown |
|---|---|---|
| P1 WIRING (humano) | 4 | NEEDS_HUMAN_DECISION (B001-B004) |
| P2 | 13 | 3 NEEDS_HUMAN_DECISION + 4 NEXT_SPEC_REQUIRED + 5 NEXT_ROADMAP_ITEM + 1 ACCEPTED_DEBT |
| P3 | 4 | 1 FIX_NOW_TRIVIAL + 3 NEXT_ROADMAP_ITEM |
| Total | 21 | — |

---

## Debts com Disposition ACCEPTED_DEBT

| DebtId | Justificativa |
|---|---|
| DEBT-CAVE-002 | CaveEnteredEvent proxy é aceitável para MVP; semântica é boa o suficiente |

---

## Debts com Disposition NEEDS_HUMAN_DECISION

| DebtId | Decisão necessária |
|---|---|
| DEBT-SCENE-001 a 004 | Quando executar Unity Editor wiring? Ordem? Prioridade? |
| DEBT-SAVE-003 | Quando executar WAVE18 checklist de Play Mode? |
| DEBT-QUEST-002 | Quando executar WAVE18/15 Play Mode checklists? |
| DEBT-TEST-001 | Quais checklists executar primeiro? Sequência sugerida: WAVE06A→07→08→09→10→11→12→13→15→16→17→18→19 |
| DEBT-TEST-003 | Quando executar FIX-001B Play Mode checklist? |
| DEBT-COMBAT-001 | Quando executar WAVE17 combat Play Mode? |

---

## Debts com Disposition FIX_NOW_TRIVIAL

| DebtId | Fix proposto |
|---|---|
| DEBT-UX-004 | CharacterEquipmentPanelController: adicionar ModalManager.PushModal no Open() e TryPopModal no Close() — similar ao InventoryPanelController |

**Nota DEBT-UX-004:** Este fix é trivial e local. Não muda contrato público, não mexe em scene, não altera schema. Pode ser executado em uma spec pequena ou PR separado. Não foi executado em WAVE22 para manter WAVE22 como DOC_ONLY.
