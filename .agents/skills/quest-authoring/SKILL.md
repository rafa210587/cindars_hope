---
name: quest-authoring
description: Estrutura uma quest — objectives, conditions, triggers, reward application com idempotency, flags, anti-softlock para quests críticas, spoiler/visibility projection e save/load de estado — reusando o QuestService/QuestRegistry existentes. Use em specs de quest 03_* (condition/trigger, flags registry, objective event contract) e 09_* (reward idempotency, log visibility, save/load), ou ao criar/estender qualquer questline, board contract ou cave contract.
---

# Skill: Autoria de Quest

O projeto já tem um orchestrator de quest completo (`QuestService` + `QuestRegistry`, com idempotency de reward via `GrantedRewardIds`, dynamic instances de board/cave e save section dedicada); esta skill garante que toda nova quest se pluga nesse flow vivo em vez de criar um segundo sistema.

## Sistemas existentes (reusar, não duplicar)

- `QuestService` (`Assets/_Game/Scripts/Quests/Runtime/QuestService.cs`) — orchestrator de runtime: `AcceptQuest`, `CheckObjectiveProgress`, `MarkObjectiveComplete`, `ProgressObjectiveCount`, `EvaluateReadyToComplete`, `TurnIn`, `RestoreFromSaveData`. Quests dinâmicas (board/cave) entram pelo MESMO registry via `RegisterDynamicInstance`/`AcceptDynamicInstance` — não há segundo registry.
- `QuestRegistry` (`Quests/Runtime/QuestRegistry.cs`) — `TryGetQuest`, `GetObjectives`, `GetRewards`, `Register`. Fonte das `QuestObjective` ricas (`ObjectiveType` + `TargetId`).
- `QuestDefinition` / `QuestObjective` / `QuestObjectiveType` (CollectItem, CraftItem, HarvestCrop, SellItem, TalkToNpc, ReachCaveDepth, DefeatEnemy, DeliverItem, CompleteCaveRun…) — modele o objective como dado, não como branch hardcoded.
- Conditions: `QuestConditionResolver` (C# puro, side-effect free, `EvaluateAll`/`Evaluate`), `QuestConditionDefinition`, `QuestConditionContext`, `ConditionEvaluationResult` (carrega `KnownFailureReasons`/`HiddenFailureReasons`/`CanShowInQuestLog`/`SuggestedFallbackId`).
- Triggers: `QuestTriggerRouter` (`Route`, dedupe por `GetEffectiveDedupeKey` + `QuestTriggerDeduplicationPolicy`, `ResetForNewRun`), `QuestTriggerDefinition`, `QuestEventEnvelope`.
- Rewards: `QuestRewardApplicator` (`Apply`, switch por `QuestRewardType`, idempotency via `AlreadyGrantedRewardIds`/`AlreadyGrantedFlagIds`, `IsFutureReward()` defer), `QuestRewardDefinition` (`IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId`), `QuestRewardApplicationResult`.
- Flags: `QuestFlagService` (`GrantFlag`/`ClearFlag`), `QuestFlagRegistry`, `QuestFlagDefinition`, `QuestFlagValidator`.
- Save: `QuestStateSection` + `QuestStateRecord` (`GrantedRewardIds`, `GrantedFlagIds`, `RewardedMainActIds`, `DiscoveredSecretQuestIds`), `QuestStateNormalizer`, `QuestStateRecord` ↔ `QuestStateSaveData` DTO. `RestoreFromSaveData` re-registra dynamic instances no registry após load.
- Eventos: `QuestProgressEventBridge` (Subscribe único, mantido vivo por `QuestRuntimeBootstrap`) traduz gameplay events (`InventoryChangedEvent`, `ItemCraftedEvent`, `CropHarvestedEvent`, `EnemyKilledEvent`, `CaveLevelEnteredEvent`…) em progresso de objective.
- Projeção: `QuestLogProjectionService` + `QuestVisibilityPolicy`/`QuestVisibilityState` (Hidden/Discovered/Known/FullyKnown/DebugOnly) + `WaitingReason` — controla spoiler/visibility no Quest Log.
- Interactables: `QuestGiverInteractable`, `QuestBoardInteractable`, `MuralInteractable`, `QuestBoardService` (offer gating via `ArePrerequisitesComplete`).

## Procedimento

1. **Definição como dado.** Crie a `QuestDefinition` + lista de `QuestObjective` e registre via `QuestRegistry.Register` (ou `RegisterDynamicInstance` para board/cave). Nunca duplique o lifecycle do `QuestService`; quests novas são dados, não código novo.
2. **Objectives por `ObjectiveType` existente.** Mapeie cada objective para um `QuestObjectiveType` já roteado pelo `QuestProgressEventBridge`. Se precisar de um tipo novo, adicione UMA subscription no bridge + UM handler no `QuestService` (precedente: `OnEnemyKilled`/`OnCropHarvested`) — não crie um listener paralelo.
3. **Conditions side-effect free.** Gates de aceitar/avançar vão em `QuestConditionDefinition` avaliadas por `QuestConditionResolver`. Conditions de sistema ainda não pronto devem ser `IsFutureCondition()` (defer = Pass), nunca bloquear quest principal num sistema não implementado.
4. **Triggers com dedupe.** Toda fonte de trigger passa por `QuestTriggerRouter.Route` com a `QuestTriggerDeduplicationPolicy` correta para evitar dupla contagem; chame `ResetForNewRun` quando a run reinicia.
5. **Reward application idempotente.** Defina rewards como `QuestRewardDefinition` com `IdempotencyPolicy = TrackByRewardId` e aplique SEMPRE via `QuestRewardApplicator.Apply` dentro de `TurnIn`. O guard é `GrantedRewardIds`/`GrantedFlagIds` no `QuestStateRecord` — reload + re-turn-in não pode re-conceder (precedente: o `reward_instance_xp` sintético).
6. **Flags via `QuestFlagService`.** Grant/clear de flag passa por `QuestFlagService` (registrado em `QuestFlagRegistry`, validado por `QuestFlagValidator`); o resultado entra em `GrantedFlagIds`. Sem set direto de bool espalhado.
7. **Anti-softlock para quests críticas.** Quest crítica precisa de fallback explícito: `SuggestedFallbackId` na condition, defer de future-condition, ou um caminho de turn-in alternativo. Uma quest principal nunca pode travar por sistema ausente ou por reward que falhou ao aplicar.
8. **Spoiler/visibility.** Conteúdo escondido (secret quest, future steps, hidden rewards) é projetado por `QuestVisibilityPolicy` no `QuestLogProjectionService`, não omitido na definição. Secret quest só aparece no log após `OfferSecretQuest`/`MarkSecretDiscovered`.
9. **Comunicação só via `GameEventBus`.** `AcceptQuest`/`MarkObjectiveComplete`/`TurnIn` publicam `QuestAcceptedEvent`, `QuestObjectiveProgressedEvent`, `QuestReadyToCompleteEvent`, `QuestCompletedEvent`, `QuestRewardClaimedEvent`. UI/NPC consomem esses eventos — sem chamada direta MonoBehaviour→MonoBehaviour (rule: event-bus-only-gameplay-communication).
10. **Save por id.** Estado novo vai no `QuestStateRecord` como simple types + ids estáveis (sem Unity refs); cubra defaults e restore em `RestoreFromSaveData`/`QuestStateNormalizer` (rule: save-dto-simple-types-only, skill: save-load-pattern).

## Testes

A seção **"Quest test expectations"** do (rule: testing-quality-gate) é **obrigatória**. O orchestrator é C# puro testável — adicione EditMode tests (skill: editmode-test-authoring) cobrindo, com justificativa explícita para qualquer item não testado:

- condition evaluation (`QuestConditionResolver.EvaluateAll` — pass/fail, future = Pass);
- trigger handling (dedupe de `QuestTriggerRouter`, condition-blocked);
- reward idempotency (`TurnIn` duas vezes + reload concede o reward UMA vez via `GrantedRewardIds`);
- quest flag set/clear (`QuestFlagService` + `GrantedFlagIds`);
- objective progress (`CheckObjectiveProgress`/`ProgressObjectiveCount` clamp em `RequiredAmount`);
- spoiler visibility (`QuestVisibilityPolicy` → projeção esconde o que deve);
- anti-softlock fallback para quests críticas;
- save/load de quest state (round-trip de `QuestStateRecord`, dynamic instance re-registrada após load).

Fluxo vivo (quest giver, board, log UI, Esc-close) vai para um human Play Mode scenario (skill: gameplay-test-scenario).

## Onde se aplica

- Specs de wave **03_*** — condition/trigger contracts, flags registry, objective event contract.
- Specs de wave **09_*** — reward idempotency, quest log visibility, save/load de quest state.

## Relacionados

- (skill: npc-dialogue-authoring) — quest giver / dialogue que oferta a quest.
- (skill: data-catalog-authoring) — catálogo de quest definitions + flags como dados com id estável.
- (skill: editmode-test-authoring) — cobertura de condition/trigger/reward/save.
- (skill: save-load-pattern) — persistência de `QuestStateRecord` por id.
- (rule: event-bus-only-gameplay-communication), (rule: save-dto-simple-types-only), (rule: testing-quality-gate).
