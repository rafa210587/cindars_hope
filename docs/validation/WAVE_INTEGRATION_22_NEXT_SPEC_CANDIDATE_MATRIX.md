# WAVE_INTEGRATION_22 — Next Spec Candidate Matrix

**Date:** 2026-06-10
**Status:** PROPOSED (não implementadas)
**Fase:** MVP_PLUS

---

## Instrução

Estas specs são **candidatas** — não foram implementadas.
Cada spec candidata deve ser criada como spec completa SpecKit-style antes de ser executada.
O executor da próxima fase deve criar a spec formal antes de implementar.

---

## Candidatas Imediatas (Prioridade NOW)

### MVP_PLUS_00_scene_wiring_validation

| Campo | Valor |
|---|---|
| DebtIds | DEBT-SCENE-001, DEBT-SCENE-002, DEBT-SCENE-003, DEBT-SCENE-004 |
| Goal | Executar wiring manual no Unity Editor para todas as scenes do playable slice |
| Tipo | HUMAN_ACTION (não código) — requer Unity Editor aberto |
| Dependencies | Unity Editor disponível; branch dev |
| Risk | Alto se wiring incorreto — requer revisão cuidadosa das wiring instructions existentes |
| Visible Result | Farm→Town→Cave funciona em Play Mode; NPCs responsivos; FarmPlot/SellPoint/Cave interagíveis |
| Validation | Executar WAVE20 checklist completo (56 steps) após wiring |
| Human instructions | WAVE_INTEGRATION_13_HUMAN_UNITY_SCENE_WIRING_INSTRUCTIONS.md + WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md + WAVE_INTEGRATION_17_HUMAN_PLAYMODE_CHECKLIST.md + CreateMvpFarmScene editor menu |
| Note | Esta é ação humana, não spec de código. O humano deve abrir o Unity Editor e seguir as instruções existentes. |

---

## Candidatas Próximas (Prioridade NEXT)

### MVP_PLUS_01_ui_hud_canvas_finalization

| Campo | Valor |
|---|---|
| DebtIds | DEBT-UX-001, DEBT-UX-002 |
| Goal | Substituir DebugHud IMGUI por HUD Canvas proper; Quest tracker Canvas |
| Dependencies | MVP_PLUS_00_scene_wiring_validation (Play Mode deve funcionar para validar Canvas) |
| Risk | Alto — HUD Canvas exige prefab wiring extenso no Unity Editor |
| Visible Result | HUD polido visualmente; gold/HP/stamina em Canvas; quest tracker em Canvas |
| Validation | Play Mode com HUD Canvas visível; todos os modal guards mantidos |
| Note | Spec grande — provavelmente 3-5 sessions. Requer design direction read de UI_UX_FULL_GAMEPLAY_DIRECTION.md |

### MVP_PLUS_02_farm_crafting_integration

| Campo | Valor |
|---|---|
| DebtIds | DEBT-SCENE-005, DEBT-FARM-001, DEBT-FARM-002 |
| Goal | Wiring da CraftingStation no FarmScene; CraftingModal Canvas wired; recipes finais |
| Dependencies | MVP_PLUS_00_scene_wiring_validation |
| Risk | Médio — CraftingRuntime já existe; apenas wiring e recipe data |
| Visible Result | Crafting funciona em Play Mode com UI Canvas |
| Validation | Executar WAVE14 checklist após wiring |
| Note | Requer Unity Editor para colocar CraftingPoint + CraftingRuntime + CraftingModal na cena |

### MVP_PLUS_03_cave_enemy_state_persistence

| Campo | Valor |
|---|---|
| DebtIds | DEBT-SAVE-001 |
| Goal | Enemy HP persiste entre saves; cave state hardening |
| Dependencies | DEBT-SCENE-003 wired (cave funcional) |
| Risk | Médio — modifica save schema (SchemaVersion bump) |
| Visible Result | Enemy HP mantido após save/load mid-combat |
| Validation | EditMode tests + Play Mode cave save round-trip |
| Note | Requer migration SCHEMA_V6 + save DTO simple types only |

### MVP_PLUS_04_equipment_modal_guard_fix

| Campo | Valor |
|---|---|
| DebtIds | DEBT-UX-004 |
| Goal | CharacterEquipmentPanelController usa ModalManager stack |
| Dependencies | Nenhuma |
| Risk | Baixo — fix local, similar ao que InventoryPanelController já faz |
| Visible Result | Dash/Dodge/Block bloqueados quando equipment panel aberto |
| Validation | EditMode test + manual check |
| Note | Fix trivial — poderia ser executado como micro-PR independente sem spec completa |

---

## Candidatas Posteriores (Prioridade LATER)

### MVP_PLUS_05_farm_loop_depth

| Campo | Valor |
|---|---|
| DebtIds | DEBT-FARM-003 |
| Goal | Expandir farm loop: múltiplos crops, seasonal variation, daily goals |
| Dependencies | MVP_PLUS_00, MVP_PLUS_02 |
| Risk | Alto — requer design direction read de FARM_DESIGN_DIRECTION_v1.3.md |
| Visible Result | Farm mais viva com múltiplos ciclos de crop |
| Validation | Play Mode farm loop completo |

### MVP_PLUS_06_town_npc_schedule_expansion

| Campo | Valor |
|---|---|
| DebtIds | DEBT-NPC-001 |
| Goal | NPCs de TownScene com schedules realistas (posições por hora do dia) |
| Dependencies | MVP_PLUS_00 (Town wired) |
| Risk | Médio — NPC system existe; schedules são data + MonoBehaviour update |
| Visible Result | NPCs se movem e mudam posição ao longo do dia |
| Validation | Play Mode time-lapse observation |

### MVP_PLUS_07_questline_expansion

| Campo | Valor |
|---|---|
| DebtIds | DEBT-QUEST-001 |
| Goal | Múltiplas questlines; diversidade de objetivos (DefeatEnemy, Deliver, Talk) |
| Dependencies | MVP_PLUS_00, MVP_PLUS_06 (NPCs wired) |
| Risk | Médio-Alto — requer conteúdo e design de quest |
| Visible Result | 3-5 questlines completas com recompensas variadas |
| Validation | Play Mode quest chain completo |

### MVP_PLUS_08_cave_enemy_roster

| Campo | Valor |
|---|---|
| DebtIds | DEBT-CAVE-001, DEBT-LOOT-001 |
| Goal | 3+ enemy types; loot tables diversificadas |
| Dependencies | MVP_PLUS_00 (cave wired) |
| Risk | Médio — enemy system existe; apenas data + enemy prefabs |
| Visible Result | Cave com variedade de enemigos |
| Validation | Play Mode cave combat com múltiplos enemy types |

### MVP_PLUS_09_save_load_playmode_automation

| Campo | Valor |
|---|---|
| DebtIds | DEBT-TEST-001, DEBT-TEST-002 |
| Goal | Automated Play Mode tests; harness de regression automático |
| Dependencies | MVP_PLUS_00 (slice funcional) |
| Risk | Alto — Unity Test Runner + Play Mode automation são complexos |
| Visible Result | CI-like Play Mode regression gate |
| Validation | Tests passam em Unity Test Runner |

### MVP_PLUS_10_visual_audio_placeholder_cleanup

| Campo | Valor |
|---|---|
| DebtIds | DEBT-UX-005, DEBT-AUDIO-001 |
| Goal | Art final para UIs principais; SFX básico para ações core |
| Dependencies | MVP_PLUS_01 (Canvas HUD) |
| Risk | Baixo tecnicamente; alto em conteúdo (requer assets) |
| Visible Result | Jogo visualmente apresentável; feedback sonoro básico |
| Validation | Visual inspection + audio verification |

---

## Candidatas Sem Debt Correspondente (Removidas)

Os seguintes candidatos da spec foram removidos porque não há debt real associado no registro canônico:

| Candidato Original | Motivo de Remoção |
|---|---|
| MVP_PLUS_companion_save | Companion system não implementado; save debt existe mas system base ausente → ICEBOX |

---

## Ordem de Execução Sugerida

```
Fase 0 (humano): MVP_PLUS_00_scene_wiring_validation (Unity Editor)
Fase 1:          MVP_PLUS_04_equipment_modal_guard_fix (trivial, qualquer ordem)
Fase 2:          MVP_PLUS_01_ui_hud_canvas_finalization
                 MVP_PLUS_02_farm_crafting_integration
                 MVP_PLUS_03_cave_enemy_state_persistence
Fase 3:          MVP_PLUS_05_farm_loop_depth
                 MVP_PLUS_06_town_npc_schedule_expansion
                 MVP_PLUS_07_questline_expansion
                 MVP_PLUS_08_cave_enemy_roster
Fase 4:          MVP_PLUS_09_save_load_playmode_automation
                 MVP_PLUS_10_visual_audio_placeholder_cleanup
```
