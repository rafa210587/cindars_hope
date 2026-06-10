# WAVE_INTEGRATION_19 — Existing UI Functionality Matrix

**Date:** 2026-06-10

Audit do que já existe no repo `dev` antes de qualquer mudança WAVE19.

---

## Funcionalidades Auditadas

| Funcionalidade | Já existe? | Classe/arquivo | Completo? | Ação WAVE19 |
|---|---|---|---|---|
| DebugHud | SIM | `Assets/_Game/Scripts/UI/DebugHud.cs` | PARCIAL — sem quest/save/cave/combat feedback | DELTA: add event subscriptions |
| Interaction prompt | SIM | `DebugHud.OnInteractionPromptChanged` (InteractionPromptChangedEvent) | COMPLETO | ALREADY_IMPLEMENTED |
| Gold HUD | SIM | `DebugHud.DrawPlayerState()` — _playerManager.CurrentGold | COMPLETO | ALREADY_IMPLEMENTED |
| HP HUD | SIM | `DebugHud.DrawPlayerState()` — _playerManager.CurrentHP/MaxHP | COMPLETO | ALREADY_IMPLEMENTED |
| Stamina HUD | SIM | `DebugHud.DrawStaminaAndStatusState()` | COMPLETO | ALREADY_IMPLEMENTED |
| Hunger HUD | SIM | `DebugHud.DrawHungerState()` | COMPLETO | ALREADY_IMPLEMENTED |
| Hotbar HUD | SIM | `DebugHud.DrawEquipmentState()` — HotbarState.SelectedItemId | COMPLETO | ALREADY_IMPLEMENTED |
| Active skill slots HUD | SIM | `DebugHud.DrawActiveSkillSlots()` — R/T/Y/G | COMPLETO | ALREADY_IMPLEMENTED |
| Economy transaction feedback | SIM | `DebugHud.OnEconomyTransactionCompleted` | COMPLETO | ALREADY_IMPLEMENTED |
| XP/Level feedback | SIM | `DebugHud.OnPlayerXpChanged / OnPlayerLevelChanged` | COMPLETO | ALREADY_IMPLEMENTED |
| PlayerActionFeedback | SIM | `DebugHud.OnPlayerActionFeedback` | COMPLETO | ALREADY_IMPLEMENTED |
| Save feedback | NÃO | `GameSavedEvent` existe mas DebugHud não subscreve | GAP | DELTA: add OnGameSaved subscription |
| Load feedback | NÃO | `GameLoadedEvent` não existia | GAP | DELTA: criar GameLoadedEvent + OnGameLoaded |
| Quest feedback | NÃO | QuestRuntimeEvents existem mas DebugHud não subscreve | GAP | DELTA: add 5 quest subscriptions |
| Cave entry feedback | PARCIAL | `CaveLevelEnteredEvent` existe, sem subscriber no HUD | GAP | DELTA: add OnCaveLevelEntered |
| Cave exit feedback | PARCIAL | `CaveExitedEvent` existe, sem subscriber no HUD | GAP | DELTA: add OnCaveExited |
| Enemy killed feedback | PARCIAL | `EnemyKilledEvent` existe, sem subscriber no HUD | GAP | DELTA: add OnEnemyKilled |
| Inventory panel | SIM | `InventoryPanelController.cs` — I key | COMPLETO com modal guard | ALREADY_IMPLEMENTED |
| Equipment panel | SIM | `CharacterEquipmentPanelController.cs` — K/L keys | COMPLETO com modal guard | ALREADY_IMPLEMENTED |
| Skill tree panel | SIM | `SkillTreeGameplayPanelController.cs` — U key | COMPLETO com modal guard | ALREADY_IMPLEMENTED |
| Quest offer panel | SIM | `QuestOfferPanelController.cs` — evento QuestGiverInteractedEvent | COMPLETO com modal guard | ALREADY_IMPLEMENTED |
| Quest log panel | SIM | `QuestLogPanelController.cs` — J key via QuestLogRuntimeBinder | PARCIAL — sem modal guard | DELTA: add ModalManager guard |
| Shop menu | SIM | `ShopMenuModal.cs` — ModalType.ShopMenu | COMPLETO | ALREADY_IMPLEMENTED |
| Buy panel | SIM | `BuyPanel` via ShopMenuModal | COMPLETO | ALREADY_IMPLEMENTED |
| Sell panel | SIM | `SellPanel` via ShopMenuModal | COMPLETO | ALREADY_IMPLEMENTED |
| Dialogue modal | SIM | `DialogueModal.cs` — ModalType.Dialogue | COMPLETO | ALREADY_IMPLEMENTED |
| ModalManager stack | SIM | `ModalManager.cs` — HasActiveModal, PushModal, TryPopModal | COMPLETO | ALREADY_IMPLEMENTED |
| Dash modal guard | SIM | `PlayerDashController.cs:39` — HasActiveModal check | COMPLETO | ALREADY_IMPLEMENTED |
| Dodge modal guard | SIM | `PlayerDodgeController.cs:39` — HasActiveModal check | COMPLETO | ALREADY_IMPLEMENTED |
| Block modal guard | SIM | `PlayerBlockController.cs:37-41` — HasActiveModal check + StopBlock | COMPLETO | ALREADY_IMPLEMENTED |
| Double tap modal guard | SIM | `DirectionalDoubleTapDetector.cs:37-43` — resets tap times | COMPLETO | ALREADY_IMPLEMENTED |
| Thalindra quest option | SIM | `NpcShopController.cs:238-280` — "! Qual é a tarefa?" dinâmico | COMPLETO | ALREADY_IMPLEMENTED |
| NPC interaction flow | SIM | NpcShopController → DialogueModal → ShopMenuModal | COMPLETO | ALREADY_IMPLEMENTED |
| Cave summary HUD | SIM | `DebugHud.DrawCaveSummary()` — level/seed/rooms/enemies | COMPLETO (só dentro da cave) | ALREADY_IMPLEMENTED |

---

## Resumo dos Gaps

| Gap | Tipo | Resolução WAVE19 |
|---|---|---|
| GameLoadedEvent não existe | MISSING_EVENT | Criar `GameLoadedEvent.cs` |
| DebugHud sem feedback save/load | MISSING_SUBSCRIPTION | Adicionar OnGameSaved + OnGameLoaded |
| DebugHud sem feedback quest | MISSING_SUBSCRIPTION | Adicionar 5 handlers de quest |
| DebugHud sem feedback cave | MISSING_SUBSCRIPTION | Adicionar OnCaveLevelEntered + OnCaveExited |
| DebugHud sem feedback enemy killed | MISSING_SUBSCRIPTION | Adicionar OnEnemyKilled |
| QuestLogPanelController sem modal guard | MISSING_MODAL_GUARD | Adicionar PushModal/TryPopModal |
| ModalType.QuestLog não existe | MISSING_ENUM_VALUE | Adicionar ao enum |

---

## O Que Não Foi Recriado

- HUD/DebugHud principal — apenas delta de subscriptions
- InventoryPanelController — ALREADY_IMPLEMENTED
- CharacterEquipmentPanelController — ALREADY_IMPLEMENTED
- SkillTreeGameplayPanelController — ALREADY_IMPLEMENTED
- ModalManager — apenas adição de QuestLog ao enum
- NpcShopController — ALREADY_IMPLEMENTED (Thalindra quest já presente)
- DialogueModal — ALREADY_IMPLEMENTED
- ShopMenuModal — ALREADY_IMPLEMENTED
- QuestOfferPanelController — ALREADY_IMPLEMENTED
- Dash/Dodge/Block modal guards — ALREADY_IMPLEMENTED
