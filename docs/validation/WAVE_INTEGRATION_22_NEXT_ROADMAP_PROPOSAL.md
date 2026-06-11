# MVP+ Roadmap Proposal — Cindar's Hope

**Date:** 2026-06-10
**Status:** PROPOSED
**Fase:** FASE_MVP_PLUS

---

## Gate de Entrada

Para iniciar MVP+:

```
✓ WAVE20 executado: SIM
✓ WAVE21 executado: SIM (NO_OP)
✓ P0 abertos: 0
✓ P1 código abertos: 0
✓ WAVE22 debt register criado: SIM
✓ Status do slice: BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE
```

**IMPORTANTE:** O humano deve executar o Play Mode sub-slice ANTES de iniciar MVP+ para confirmar que o engine está funcionando. Sem este gate, MVP+ pode estar construindo sobre fundação não verificada.

Gate mínimo recomendado:

```
Executar steps 1-5, 12-14, 33-36, 46-53 do WAVE20 checklist.
Preencher WAVE_INTEGRATION_20_FINAL_ACCEPTANCE_EVIDENCE_TEMPLATE.md.
Status mínimo: ACCEPTED_WITH_DEBT (sub-slice).
```

---

## Objetivo da Fase MVP+

Transformar o playable slice validado em uma experiência completa e apresentável:

```
- Substituir UIs temporárias por Canvas final
- Expandir farm loop com profundidade
- Adicionar múltiplas questlines
- Expandir cave com variedade de enemigos
- Hardening de save/load
- Feedback sonoro básico
- Automated regression gate
```

---

## Specs Propostas (em ordem de prioridade)

### Fase 0 — Wiring Humano (ação Unity Editor, não código)

| Ordem | Spec | Tipo | Prioridade | Gate |
|---|---|---|---|---|
| 0 | MVP_PLUS_00_scene_wiring_validation | HUMAN_ACTION | NOW | Play Mode sub-slice + acesso ao Unity Editor |

**Instruções existentes:**
- FarmScene layout: `CindarsHope/Integration/Create MVP Farm Scene` (editor menu)
- Scene transitions: `WAVE_INTEGRATION_13_HUMAN_UNITY_SCENE_WIRING_INSTRUCTIONS.md`
- Cave entrance: `WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md`
- Cave combat: `WAVE_INTEGRATION_17_HUMAN_PLAYMODE_CHECKLIST.md`

### Fase 1 — Fixes Triviais e Quick Wins

| Ordem | Spec | Tipo | Debts | Prioridade |
|---|---|---|---|---|
| 1 | MVP_PLUS_04_equipment_modal_guard_fix | CODE_TRIVIAL | DEBT-UX-004 | NEXT |

### Fase 2 — Systems Polish (após wiring)

| Ordem | Spec | Tipo | Debts | Prioridade |
|---|---|---|---|---|
| 2 | MVP_PLUS_01_ui_hud_canvas_finalization | CODE+UNITY | DEBT-UX-001/002 | NEXT |
| 3 | MVP_PLUS_02_farm_crafting_integration | CODE+UNITY | DEBT-SCENE-005, DEBT-FARM-001/002 | NEXT |
| 4 | MVP_PLUS_03_cave_enemy_state_persistence | CODE | DEBT-SAVE-001 | NEXT |

### Fase 3 — Content Expansion

| Ordem | Spec | Tipo | Debts | Prioridade |
|---|---|---|---|---|
| 5 | MVP_PLUS_05_farm_loop_depth | CODE+DATA | DEBT-FARM-003 | LATER |
| 6 | MVP_PLUS_06_town_npc_schedule_expansion | CODE+DATA | DEBT-NPC-001 | LATER |
| 7 | MVP_PLUS_07_questline_expansion | CODE+DATA | DEBT-QUEST-001 | LATER |
| 8 | MVP_PLUS_08_cave_enemy_roster | CODE+DATA | DEBT-CAVE-001, DEBT-LOOT-001 | LATER |

### Fase 4 — Hardening e Polish Final

| Ordem | Spec | Tipo | Debts | Prioridade |
|---|---|---|---|---|
| 9 | MVP_PLUS_09_save_load_playmode_automation | CODE+TEST | DEBT-TEST-001/002 | LATER |
| 10 | MVP_PLUS_10_visual_audio_placeholder_cleanup | ASSET | DEBT-UX-005, DEBT-AUDIO-001 | LATER |

---

## Dependências entre Specs

```
MVP_PLUS_00 (wiring) → desbloqueia tudo
MVP_PLUS_04 (modal fix) → independente
MVP_PLUS_01 (HUD Canvas) → depende MVP_PLUS_00
MVP_PLUS_02 (crafting) → depende MVP_PLUS_00
MVP_PLUS_03 (cave save) → depende MVP_PLUS_00 (cave funcional)
MVP_PLUS_05 (farm depth) → depende MVP_PLUS_00 + MVP_PLUS_02
MVP_PLUS_06 (npc schedule) → depende MVP_PLUS_00
MVP_PLUS_07 (questline) → depende MVP_PLUS_06
MVP_PLUS_08 (cave enemy) → depende MVP_PLUS_00
MVP_PLUS_09 (automation) → depende MVP_PLUS_00 + slice funcional
MVP_PLUS_10 (visual/audio) → depende MVP_PLUS_01 (Canvas)
```

---

## Critérios de Aceite da Fase MVP+

```
- Todos os P1 HUMAN_WIRING_REQUIRED resolvidos e verificados em Play Mode
- HUD Canvas substituiu DebugHud IMGUI
- Farm loop com ≥3 crop types e ciclo diário
- ≥3 questlines completas
- ≥3 enemy types na cave
- Cave enemy HP persiste em save/load
- Play Mode regression checklist automatizado
- SFX básico implementado
- Evidence template WAVE20 preenchida com ACCEPTED (não apenas ACCEPTED_WITH_DEBT)
```

---

## O Que Não Entra em MVP+

```
- Pets (HOLD/BLOCKED_SCOPE permanente)
- Romance/companion final systems
- Multiplayer
- Procedural world (fora de cave)
- Balancing final de endgame
- Arte final completa (apenas placeholders principais)
- Voices / cutscenes
```

---

## Riscos

| Risco | Impacto | Mitigação |
|---|---|---|
| Scene wiring manual incorreto | Cenas quebradas em Play Mode | Seguir instruções existentes cuidadosamente; verificar cada step |
| HUD Canvas exige prefab extenso | Spec longa e complexa | Dividir em sub-specs se necessário |
| Cave enemy save → schema v6 | Migration pode quebrar saves existentes | EditMode tests para migration |
| Questline expansion sem design | Feature bloat | Ler QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION antes de implementar |
| NPC schedules sem arte | NPCs se movem mas parecem sem propósito | Coordenar com art direction |

---

## Human Validation Checkpoints

```
Checkpoint 1: Após MVP_PLUS_00
  - Executar WAVE20 checklist completo (56 steps)
  - Preencher evidence template
  - Status esperado: ACCEPTED_WITH_DEBT

Checkpoint 2: Após Fase 2 (specs 1-4)
  - Executar checklist expandido (HUD Canvas + crafting + cave save)
  - Verificar modal guards ainda funcionam
  - Status esperado: ACCEPTED_WITH_MINOR_DEBT

Checkpoint 3: Após Fase 3 (specs 5-8)
  - Executar content validation (farm loop, quest chain, cave roster)
  - Verificar economia balanceada
  - Status esperado: CONTENT_VALIDATED

Checkpoint 4: Após Fase 4 (specs 9-10)
  - Executar automated regression suite
  - Visual/audio inspection
  - Status esperado: RELEASE_CANDIDATE
```
