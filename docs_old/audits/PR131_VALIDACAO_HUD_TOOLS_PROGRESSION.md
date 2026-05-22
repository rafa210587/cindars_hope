# PR-131 - Validacao HUD, tools, hotbar e progressao

Data: 2026-05-20
Branch: `feature/pr-131-sync-validacao-hud-tools-progression`

## Objetivo

Registrar o estado real validado antes de entrar em Cave Procedural, focando nas lacunas de debug/gameplay de HUD, ferramentas, hotbar, plantio e progressao.

## O que foi validado

- HUD debug existe, persiste cross-scene e mostra informacoes em uma unica coluna OnGUI.
- Hotbar existe como estado salvo/debug e troca slot por input numerico.
- Tool debug existe em `EquipmentManager` e alterna ferramenta com `T`.
- XP sobe por `EnemyKilledEvent` via `PlayerProgressionManager`.
- Level up concede pontos de atributo/skill no estado de progressao.
- Cave atual ainda e MVP fixo, sem `CaveWorldSeed`, `CaveRunSeed`, `CaveRunManager` ou geracao procedural.

## O que funciona

- `DebugHud` mostra cena, dia, ouro, HP, fome, inventario, ferramenta, hotbar e mensagens de XP/level.
- `HotbarState` possui 6 slots e `HotbarDebugInput` permite selecionar slot.
- `SaveManager` inicializa slots debug com `seed_wheat` e `seed_carrot` quando nao ha save restaurado.
- `EquipmentManager` alterna ferramentas debug com `T` e persiste `EquippedToolId`/`EquippedWeaponId`.
- `PlayerProgressionManager` recebe XP, sobe level e salva/restaura dados simples.

## O que esta parcial

- HUD ainda nao esta dividida em painel de acoes e painel de informacoes.
- Tool existe, mas `TreeNode` e `FishingSpot` ainda nao consultam `EquipmentManager`; pesca ainda valida posse do item no inventario.
- Hotbar existe, mas `FarmPlot` ainda escolhe a primeira seed disponivel no inventario.
- Pontos de atributo existem, mas ainda nao ha controle debug para distribuir pontos.
- Mensagens de falha ainda dependem de logs; nao ha evento unico de feedback para a HUD.

## O que ainda nao existe

- `PlayerActionFeedbackEvent`.
- Contratos `EquipmentManager.HasTool(...)` para gating.
- Tool gating real para arvore/pesca.
- Seed gating real por hotbar no plantio.
- `PlayerAttributeType`, `PlayerAttributeDebugInput` e evento de atributos alterados.
- Cave procedural, seeds de cave e status procedural na HUD.

## Proximo pacote recomendado

Seguir FASE9E-D antes da Cave Procedural:

1. PR-132 - DebugHud layout v2.
2. PR-133 - Action feedback event.
3. PR-134 - Tool gating contracts.
4. PR-135 - Tool gating para arvore e pesca.
5. PR-136 - Hotbar seed gating para FarmPlot.
6. PR-137 - Attribute allocation debug MVP.
7. PR-138 - DebugHud progression/tool/hotbar polish.
8. PR-139 - Handoff para Cave Procedural.
