# WAVE_INTEGRATION_19 — Feedback Event Coverage Matrix

**Date:** 2026-06-10

Auditoria de eventos de feedback e cobertura no HUD.

---

## Eventos de Gameplay → Feedback no DebugHud

| Evento/Ação | Classe que publica | Subscrição no DebugHud | Feedback visível | Gap | Resolução WAVE19 |
|---|---|---|---|---|---|
| PlayerActionFeedbackEvent | Múltiplos produtores | SIM — OnPlayerActionFeedback | "Feedback: {message}" | NENHUM | ALREADY_IMPLEMENTED |
| EconomyTransactionCompletedEvent (venda/compra) | ShopManager / BuyPanel / SellPanel | SIM — OnEconomyTransactionCompleted | "Economia: {message}" | NENHUM | ALREADY_IMPLEMENTED |
| PlayerXpChangedEvent | PlayerProgressionManager | SIM — OnPlayerXpChanged | "Progressao: XP ..." | NENHUM | ALREADY_IMPLEMENTED |
| PlayerLevelChangedEvent | PlayerProgressionManager | SIM — OnPlayerLevelChanged | "Progressao: Level X→Y" | NENHUM | ALREADY_IMPLEMENTED |
| InteractionPromptChangedEvent | InteractionSystem | SIM — OnInteractionPromptChanged | "Interacao: {prompt}" | NENHUM | ALREADY_IMPLEMENTED |
| GameSavedEvent | SaveManager.PublishSaveResult | ANTES: NÃO → APÓS: SIM | "Jogo salvo." / "Falha ao salvar:" | WAS_GAP | DELTA: OnGameSaved adicionado |
| GameLoadedEvent | SaveManager.LoadGame | ANTES: NÃO existia → APÓS: SIM | "Jogo carregado." / "Falha ao carregar:" | WAS_GAP | DELTA: criado GameLoadedEvent + OnGameLoaded |
| QuestAcceptedEvent | QuestService.AcceptQuest | ANTES: NÃO → APÓS: SIM | "Quest aceita: {questId}" | WAS_GAP | DELTA: OnQuestAccepted |
| QuestObjectiveProgressedEvent | QuestService | ANTES: NÃO → APÓS: SIM | "Objetivo: {id} {current}/{required}" | WAS_GAP | DELTA: OnQuestObjectiveProgressed |
| QuestReadyToCompleteEvent | QuestService | ANTES: NÃO → APÓS: SIM | "Pronto para entregar: {questId}" | WAS_GAP | DELTA: OnQuestReadyToComplete |
| QuestCompletedEvent | QuestService.TurnIn | ANTES: NÃO → APÓS: SIM | "Quest concluída: {questId}" | WAS_GAP | DELTA: OnQuestCompleted |
| QuestRewardClaimedEvent | QuestService.TurnIn | ANTES: NÃO → APÓS: SIM | "Recompensa: {gold}g" | WAS_GAP | DELTA: OnQuestRewardClaimed |
| CaveLevelEnteredEvent | CaveRuntimeBridge (entre níveis) | ANTES: NÃO → APÓS: SIM | "Entrando na caverna (nível {N})" | WAS_GAP | DELTA: OnCaveLevelEntered |
| CaveExitedEvent | CaveRuntimeBridge (saída cave) | ANTES: NÃO → APÓS: SIM | "Retornando à superfície → {scene}" | WAS_GAP | DELTA: OnCaveExited |
| EnemyKilledEvent | EnemyController / EnemyDropSpawner | ANTES: NÃO → APÓS: SIM | "Inimigo derrotado: {id} | Loot: {item} x{n}" | WAS_GAP | DELTA: OnEnemyKilled |

---

## Eventos que NÃO têm subscrição no HUD (aceitável)

| Evento | Motivo de não ter feedback no HUD |
|---|---|
| InventoryChangedEvent | Inventory já visível no painel DebugHud.DrawInventory() |
| GoldChangedEvent | Gold já visível em DrawPlayerState() |
| EquipmentSlotChangedEvent | Equipment já visível em DrawEquipmentState() |
| CaveCheckpointUnlockedEvent | Checkpoint visível no DrawCaveSummary() |
| HPChangedEvent | HP já visível em DrawPlayerState() |
| StaminaChangedEvent | Stamina já visível em DrawStaminaAndStatusState() |
| HungerChangedEvent | Hunger já visível em DrawHungerState() |

---

## Eventos Ausentes (sem impacto crítico nesta wave)

| Evento | Situação | Impacto |
|---|---|---|
| CaveEnteredEvent (surface→cave) | Não existe; CaveLevelEnteredEvent cobre | LOW — CaveLevelEnteredEvent já mostra nível |
| LootCollectedEvent | Não existe separado; EnemyKilledEvent tem drop info | LOW — DROP_DIRECT_TO_INVENTORY, info no EnemyKilledEvent |
| EnemyDamagedEvent | Existe em EnemyEvents.cs mas sem subscriber | LOW — feedback hit visual é in-scene |
| InventoryFullEvent | Não existe como evento | LOW — check síncrono no AddItem |

---

## Resultado

Todos os feedbacks mínimos obrigatórios da spec estão cobertos após delta WAVE19.

```text
Save:          "Jogo salvo." / "Falha ao salvar:"      ✓ COVERED
Load:          "Jogo carregado." / "Falha ao carregar:" ✓ COVERED
Quest aceita:  "Quest aceita: {id}"                    ✓ COVERED
Quest progresso: "Objetivo: {id} {x}/{y}"              ✓ COVERED
Quest pronta:  "Pronto para entregar: {id}"            ✓ COVERED
Quest completa: "Quest concluída: {id}"                ✓ COVERED
Recompensa:    "Recompensa: {gold}g"                   ✓ COVERED
Cave entrada:  "Entrando na caverna (nível {N})"       ✓ COVERED
Cave saída:    "Retornando à superfície → {scene}"     ✓ COVERED
Enemy derrotado: "Inimigo derrotado: {id} | Loot: ..." ✓ COVERED
Interaction:   "Interacao: {prompt}"                   ✓ ALREADY
Compra/venda:  "Economia: {message}"                   ✓ ALREADY
```
