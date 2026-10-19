# WAVE_INTEGRATION_16 — Cave Entrance + Cave Runtime Bridge

<!-- /speckit.specify -->
# /speckit.specify

<!-- /speckit.plan -->
# /speckit.plan

<!-- /speckit.tasks -->
# /speckit.tasks

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

## Ordem de execucao

WAVE_INTEGRATION_16 — executa após WAVE_INTEGRATION_15.

## Depende de

- WAVE_INTEGRATION_13: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (commit 315cfda)
- WAVE_INTEGRATION_14: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (commit 5bedba8)
- WAVE_INTEGRATION_15: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (commit f3f4d5f)

## Bloqueia

- WAVE_INTEGRATION_17 (combat/loot in cave)

---

Status: A_IMPLEMENTAR
Domínio: cave / scene transition / runtime bridge / player spawn / state preservation
Wave: WAVE_INTEGRATION_16 — Cave Entrance + Cave Runtime Bridge
Resultado esperado: loop jogável farm/town → cave → retorno, preservando inventory/gold/quest state.

Objetivo:
FarmScene/TownScene → CaveEntranceInteractable → SceneTransitionRouter → CaveScene → spawn seguro → CaveExitPortal → retorno surface

Proibido: combat, loot, enemy AI, parallel systems, Packages/ProjectSettings
Status final permitido: BUILD_VALIDATED_SCENE_WIRED / BUILD_VALIDATED_WITH_CAVE_RUN_SAVE_DEBT / CODE_READY_HUMAN_UNITY_ACTION_REQUIRED / BLOCKED
