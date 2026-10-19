# WAVE_INTEGRATION_19 — Playable Slice Acceptance Matrix

**Date:** 2026-06-10

Classificação objetiva de cada área do playable slice como bloqueante ou debt.

---

## Critérios

- **BLOCKING**: impede o playable slice de ser válido para aceitação final
- **DEBT_NON_BLOCKING**: funcional com limitação conhecida; não impede WAVE20
- **HUMAN_WIRING_REQUIRED**: código pronto mas wiring no Unity Editor ainda não feito

---

## Matriz

| Área | Must Pass | Current Status | Blocking? | Evidence / Debt |
|---|---|---|---|---|
| Farm loop básico | Mover player, interagir com plantio, colher | HUMAN_PLAY_MODE_PENDING | Não (base implementada WAVE02-08) | FarmScene wired, sem confirmação Play Mode |
| Town/NPC dialogue | Thalindra abre diálogo com opções | ALREADY_IMPLEMENTED (code) | Não (código wired) | NpcShopController mostra quest+compra+venda+adeus |
| Shop buy/sell | Comprar/vender itens com feedback | ALREADY_IMPLEMENTED (code) | Não | BuyPanel/SellPanel com EconomyTransactionCompletedEvent |
| Quest accept/log/complete | Aceitar quest da Thalindra, ver no log, completar | BUILD_VALIDATED (code) | Não (WAVE15+16+17 wiring) | QuestOfferPanelController + QuestLogPanelController + QuestService |
| Quest save/load round-trip | Quest persiste após save+reload | BUILD_VALIDATED (WAVE18) | Não | GameSavedEvent + GameLoadedEvent + QuestStateSectionSaveData |
| Crafting/processing | Mesa de crafting funciona com UI | HUMAN_WIRING_REQUIRED | Não blocking (código WAVE14) | CraftingStation wiring precisa de human placement |
| Scene transitions | Farm → Town → Cave | HUMAN_WIRING_REQUIRED | Sim para cave flow | WAVE16: CaveEntranceInteractable + CaveExitPortal aguardam human wiring |
| Cave enter/exit feedback | Feedback visível ao entrar/sair | BUILD_VALIDATED (WAVE19 delta) | Não | CaveLevelEnteredEvent + CaveExitedEvent → DebugHud |
| Cave combat/loot | Enemy spawn, derrota, loot | HUMAN_WIRING_REQUIRED | Não blocking (debt explícito) | WAVE17: CaveSmokeTestSpawnerBridge + EnemyDropSpawner aguardam human wiring |
| Save/load feedback | "Jogo salvo." / "Jogo carregado." no HUD | BUILD_VALIDATED (WAVE19 delta) | Não | GameSavedEvent + GameLoadedEvent → DebugHud.OnGameSaved/OnGameLoaded |
| HUD/feedback geral | Gold/HP/stamina/quest/cave/economy visível | BUILD_VALIDATED (WAVE19 delta) | Não | DebugHud com 15 event subscriptions |
| Modal/input guard | Dash/dodge/block bloqueados durante modal | BUILD_VALIDATED (WAVE19 delta) | Não | HasActiveModal verificado em Dash+Dodge+Block+DoubleTap |
| QuestLog acessível | Tecla J abre log, Escape fecha | BUILD_VALIDATED (WAVE19 delta) | Não | QuestLogPanelController com ModalType.QuestLog |
| Inventory modal guard | Tecla I, Escape fecha, sem vazamento | ALREADY_IMPLEMENTED | Não | InventoryPanelController com ModalType.Inventory |
| Equipment modal guard | Tecla K/L, sem vazamento | ALREADY_IMPLEMENTED | Não | CharacterEquipmentPanelController com ModalType.CharacterEquipment |
| SkillTree modal guard | Tecla U, active slots R/T/Y/G | ALREADY_IMPLEMENTED | Não | SkillTreeGameplayPanelController com ModalType.SkillTree |

---

## Blockers para Aceite Final (ACCEPTED)

| Blocker | Tipo | Resolução |
|---|---|---|
| Scene transitions Cave (WAVE16) | HUMAN_WIRING_REQUIRED | Humano coloca CaveEntranceInteractable + CaveExitPortal na CaveScene |
| Combat/loot Cave (WAVE17) | HUMAN_WIRING_REQUIRED | Humano coloca CaveSmokeTestSpawnerBridge + EnemyDropSpawner na CaveScene |
| Play Mode não executado | HUMAN_PLAY_MODE_PENDING | Humano executa WAVE19_HUMAN_PLAYMODE_CHECKLIST |

---

## Debts Non-Blocking

| Debt | Tipo | Impacto |
|---|---|---|
| Enemy HP não persiste mid-combat (WAVE18) | CAVE_ENEMY_HP_SAVE_DEBT | LOW — smoke test aceitável |
| CompanionManager save não implementado (WAVE18) | COMPANION_SAVE_DEBT | LOW — companion não é parte do smoke test |
| CaveEnteredEvent (surface→cave) não existe | MISSING_EVENT | LOW — CaveLevelEnteredEvent cobre para HUD |
| LootCollectedEvent não existe | MISSING_EVENT | LOW — EnemyKilledEvent já tem drop info |
| Crafting wiring humano (WAVE14) | HUMAN_WIRING_REQUIRED | MEDIUM — WAVE14 wired no editor mas confirmar |
| HUD Canvas final | DEFERRED_UI_VISUAL | FUTURE_WAVE — não impede smoke test |
| Quest tracker sofisticado | DEFERRED_UI_VISUAL | FUTURE_WAVE |

---

## Conclusão

O playable slice é validável com as áreas marcadas como `HUMAN_WIRING_REQUIRED` como condição pré-jogo. O código está BUILD_VALIDATED para todos os sistemas. A acceptance final depende do humano executar o checklist de Play Mode.

**Pode continuar WAVE20:** SIM — todos os code-level blockers resolvidos.
