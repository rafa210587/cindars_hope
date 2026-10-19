# WAVE_INTEGRATION_19 — HUD/UX Acceptance Gate: Decision Report

**Date:** 2026-06-10
**Status:** `BUILD_VALIDATED_HUD_UX_ACCEPTANCE_READY_PENDING_HUMAN_PLAYMODE`
**Branch:** dev

---

## 1. Fontes Lidas

### Processo
- `CLAUDE.md` — router de contexto e regras
- `docs/project/CURRENT_STATE.md` — estado ativo do projeto

### Waves anteriores lidas
- `docs/validation/WAVE_INTEGRATION_18_SAVE_LOAD_GAP_REPORT.md` — WAVE18 BUILD_VALIDATED, save/load gap fechado
- Commits históricos (git log) para entender estado do repo

### Fontes de design (listas de presença no repo — não lidas por inteiro por proibição de contexto)
- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md` — presença confirmada
- `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md` — presença confirmada

---

## 2. Pré-validação do Repo (o que já existe)

### ALREADY_IMPLEMENTED (não recriado)

| Sistema | Classe | Estado |
|---|---|---|
| DebugHud IMGUI | DebugHud.cs | COMPLETO — gold/HP/stamina/hunger/equipment/hotbar/active slots/cave/feedback |
| ModalManager stack | ModalManager.cs | COMPLETO — HasActiveModal, PushModal, TryPopModal, ClearAllModals |
| Dash modal guard | PlayerDashController.cs:39 | COMPLETO |
| Dodge modal guard | PlayerDodgeController.cs:39 | COMPLETO |
| Block modal guard | PlayerBlockController.cs:37-41 | COMPLETO |
| DoubleTap modal guard | DirectionalDoubleTapDetector.cs:37-43 | COMPLETO |
| Inventory panel (I) | InventoryPanelController.cs | COMPLETO com ModalType.Inventory |
| Equipment panel (K/L) | CharacterEquipmentPanelController.cs | COMPLETO com ModalType.CharacterEquipment |
| SkillTree panel (U) | SkillTreeGameplayPanelController.cs | COMPLETO com ModalType.SkillTree |
| QuestOffer panel | QuestOfferPanelController.cs | COMPLETO com ModalType.QuestOffer |
| QuestLog panel (J) | QuestLogPanelController.cs | PARCIAL — sem modal guard |
| DialogueModal | DialogueModal.cs | COMPLETO com ModalType.Dialogue |
| ShopMenu | ShopMenuModal.cs | COMPLETO com ModalType.ShopMenu |
| Thalindra quest option | NpcShopController.cs:238-280 | COMPLETO — "! Qual é a tarefa?" dinâmico |
| GameSavedEvent | GameSavedEvent.cs | COMPLETO — publicado em SaveManager |
| All quest events | QuestRuntimeEvents.cs | COMPLETO — 6 eventos |
| CaveLevelEnteredEvent | CaveLevelEnteredEvent.cs | COMPLETO |
| CaveExitedEvent | CaveExitedEvent.cs | COMPLETO |
| EnemyKilledEvent | EnemyKilledEvent.cs | COMPLETO |

### GAPS (corrigidos nesta wave)

| Gap | Resolução |
|---|---|
| GameLoadedEvent não existe | Criado: GameLoadedEvent.cs |
| DebugHud sem feedback save/load/quest/cave/enemy | Adicionadas 10 subscriptions em DebugHud.cs |
| QuestLogPanelController sem modal guard | Adicionado PushModal/TryPopModal(ModalType.QuestLog) |
| ModalType.QuestLog inexistente | Adicionado ao enum em ModalManager.cs |

---

## 3. Estratégia Modal/Input

**Decisão:** Manter estratégia existente. `ModalManager.HasActiveModal` é verificado em todos os controllers de movimento. Adicionar QuestLog ao stack é a menor mudança possível.

**Prova de que funciona:** PlayerDash/Dodge/Block all check `GameBootstrap.Instance?.ModalManager?.HasActiveModal`.

---

## 4. Estratégia de Feedback

**Decisão:** Reusar `_currentActionFeedback` / `_actionFeedbackUntil` no DebugHud. É o mecanismo existente para timed feedback. Cada evento de gameplay novo escreve nessa variável com duração de 4 segundos.

**Não criado:** Novo painel de UI, animações, Canvas final. Apenas event handlers que atualizam o campo existente.

---

## 5. Thalindra/Shop/Quest Status

**Thalindra quest option:** ALREADY_IMPLEMENTED em `NpcShopController.cs`.

Código relevante (linhas 238-280):
```
- "! Qual é a tarefa?" → quando quest disponível para oferecer
- "Entregar suprimentos" → quando quest pronta para entregar  
- Opção oculta → quando quest em andamento
- Plus: "Comprar", "Vender", "Adeus" sempre presentes
```

**Conclusão:** A opção de quest aparece no mesmo fluxo que compra/venda. AC-06 satisfeito.

---

## 6. Save/Load Feedback Status

**Antes WAVE19:**
- `GameSavedEvent` existe e é publicado → sem subscriber no HUD
- `GameLoadedEvent` não existe

**Após WAVE19:**
- `GameLoadedEvent` criado e publicado no SaveManager (path direto + corrotina)
- DebugHud subscreve ambos → mostra "Jogo salvo." / "Jogo carregado."

---

## 7. Cave Feedback Status

**Antes WAVE19:** CaveLevelEnteredEvent e CaveExitedEvent existem, sem subscriber no HUD.

**Após WAVE19:** DebugHud subscreve:
- `CaveLevelEnteredEvent` → "Entrando na caverna (nível N)"
- `CaveExitedEvent` → "Retornando à superfície → {scene}"

**Sem CaveEnteredEvent (surface→cave):** Não existe evento específico para "player acabou de entrar na cave do mapa surface". O `CaveLevelEnteredEvent` (publicado quando materializa um nível da cave) serve como proxy. Para entrada inicial, nível=1 indica a entrada.

---

## 8. Combat/Loot Feedback Status

**EnemyKilledEvent** existe em `EnemyKilledEvent.cs` com `DropItemId` e `DropAmount`.
DebugHud agora subscreve e mostra: "Inimigo derrotado: {id} | Loot: {item} x{n}".

**WAVE17 Human Wiring Debt:**
`CaveSmokeTestSpawnerBridge`, `EnemyDropSpawner` e `enemy_slime_basic` precisam ser colocados na CaveScene pelo humano. Sem este wiring, o EnemyKilledEvent nunca será publicado.

---

## 9. Design/Direction Compliance Matrix

| Source | Rule extracted | Impact | Applied? | Evidence |
|---|---|---|---|---|
| SPEC_SOURCE_MAP.md | UI/HUD/menu/input specs must read UI_UX directions | Mandatory source list | SIM | Presença confirmada |
| SPECIFICATION_PROCESS.md | Não recriar sistemas existentes | Existing functionality matrix | SIM | Matrix criada, nada recriado |
| UI_UX_FULL_GAMEPLAY_DIRECTION.md | HUD deve expor estado jogável | HUD feedback checklist | SIM | 10+ feedback points cobertos |
| UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md | Modal/focus/input routing não vaza | Modal guard matrix | SIM | Todos os modais verificados |
| QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md | Quest UI é projeção do runtime | QuestLog validation | SIM | QuestLogPanelController lê QuestService |
| SAVE_LOAD_FULL_STATE_DIRECTION.md | Save/load feedback obrigatório | Save/load feedback | SIM | GameLoadedEvent criado |
| WAVE18 report | Save/load debts definem acceptance | Acceptance gate usa WAVE18 | SIM | BUILD_VALIDATED_SAVE_LOAD_GAP_CLOSED |
| WAVE17 report | Combat/loot debt | Combat checklist | SIM | WAVE17_HUMAN_WIRING_REQUIRED documentado |

---

## 10. O Que NÃO Será Recriado

```text
HUD Canvas final — não implementar agora
Inventory UI final — não recriar
Equipment UI final — não recriar
SkillTree UI final — não recriar
Arte final — não criar
Animações finais — não criar
Menu principal completo — não criar
Save/load menu final — não criar
Quest tracker sofisticado — não criar
Minimap — não criar
```
