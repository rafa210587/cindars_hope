# SPEC - Town NPC dialogue, schedule e quests

> Spec ID: `spec_town_npc_dialogue_schedule_quests`
> Status: Implementado completo
> Ordem de execucao: 08
> Data: 2026-05-24
> Origem: `.specs/a_implementar/spec_town_npc_dialogue_schedule_quests.md`

## Estado entregue

- `NpcDataSO` inclui `DefaultSceneId`, `DefaultPositionId`, `MovementMode` e dados de wander.
- Pip possui `DialogueTreeSO` com quatro escolhas, sem abertura de loja ou quest.
- Os dois lojistas mantem `NpcDataSO` e binding para `shop_weapons_armor` e `shop_seeds_tools`.
- `npc_vaalara_wanderer_01` possui tres falas aleatorias e movimento lento limitado a area segura.
- `DialogueModal` suporta escolhas e o gerador da `TownScene` configura seu container/template de botoes.
- `NpcController` e `NpcShopController` publicam eventos de inicio/fim de interacao via `GameEventBus`.
- `NpcManager` e `SaveManager` capturam/restauram somente `NpcId`, `SceneId`, posicao e `HasMet`.
- `CreateMvpTownScene` define Pip como NPC de dialogo, dois lojistas formais, wanderer e `NpcManager`.
- `MvpSceneValidator.ValidateSpec08Scene()` verifica topologia da cena e contratos de dados/dialogo.

## Evidencias

```text
Assets/_Game/Scripts/NPC/NpcDataSO.cs
Assets/_Game/Scripts/NPC/NpcController.cs
Assets/_Game/Scripts/NPC/NpcShopController.cs
Assets/_Game/Scripts/NPC/NpcWanderer.cs
Assets/_Game/Scripts/NPC/NpcManager.cs
Assets/_Game/Scripts/Core/Events/NpcInteractionEvents.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueModal.cs
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
Assets/_Game/Scripts/Editor/Validation/MvpSceneValidator.cs
Assets/_Game/Data/NPCs/*.asset
Assets/_Game/Data/Dialogues/*.asset
```

## Fora de escopo preservado

- Schedule completo por horario, dia ou clima.
- Quests ativas, relationship, friendship e rewards.
- Pathfinding completo e animacoes finais.

## Validacao Executada

```text
Unity compile: PASS - `*** Tundra build success` em `Logs/spec08-scene-validation-final.log`.
TownScene generation: PASS - `MVP TownScene created at Assets/_Game/Scenes/TownScene.unity` em `Logs/spec08-scene-generation.log`.
SPEC 08 scene/data validator: PASS - `SPEC 08 scene and NPC dialogue validation passed for TownScene.` em `Logs/spec08-scene-validation-final.log`.
Regression validators: PASS - SPEC 06 (`Logs/spec08-regression-spec06.log`) e SPEC 07 (`Logs/spec08-regression-spec07.log`).
Docs validation: PASS - `tools/docs/validate_docs.ps1`.
Unity log scanner: FAIL - `ScanUnityLogs.ps1` identifica as mensagens conhecidas de `Assembly-CSharp-Editor-firstpass.dll` e `Assembly-CSharp-firstpass.dll` como criticas, apesar de `Tundra build success`.
Play Mode: NOT RUN.
Residual risk: interacao humana de UI, variacao aleatoria visual, movimento em tempo real e save/load interativo aguardam Play Mode. O log tambem expoe duplicidade de `item_crop_wheat` disparada pelo inicializador da SPEC 07, fora do escopo desta spec.
```

Checklist manual remanescente registrado em `docs/validation/SPEC_08_TOWN_NPC_VALIDATION_20260524.md`.
