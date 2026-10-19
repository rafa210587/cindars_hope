# WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real

<!-- /speckit.specify -->
# /speckit.specify

<!-- /speckit.plan -->
# /speckit.plan

<!-- /speckit.tasks -->
# /speckit.tasks

required_adrs: []
required_game_rules: []

## Ordem de execucao

WAVE_INTEGRATION_15 — executa após WAVE_INTEGRATION_14.

## Depende de

- WAVE_INTEGRATION_13: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (commit 315cfda)
- WAVE_INTEGRATION_14: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (commit 5bedba8)

## Bloqueia

- WAVE_INTEGRATION_16 (fase futura)

---

Status: A_IMPLEMENTAR
Domínio: quests / objectives / events / quest log / NPC dialogue / reward / save-load
Wave: WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
Resultado esperado: primeira quest jogável ponta a ponta: quest giver/board → aceitar → Quest Log → progresso real → turn-in → reward idempotente.

Quest principal: quest_repair_kit_for_town (se WAVE14 crafting OK) ou quest_first_supplies_for_cindar (fallback)
Quest giver: npc_thalindra preferência / fallback QuestBoard_FirstQuest_01

Status final permitido:
- BUILD_VALIDATED_SCENE_WIRED
- BUILD_VALIDATED_WITH_QUEST_SAVE_DEBT
- CODE_READY_HUMAN_UNITY_ACTION_REQUIRED
- BLOCKED
