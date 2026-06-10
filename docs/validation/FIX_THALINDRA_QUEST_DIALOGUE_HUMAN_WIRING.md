# Fix — Thalindra Quest Dialogue Wiring

## Status: RUNTIME_AUTO_WIRED_SHOP_CONTROLLER_FIXED

## Root Cause (resolvido em código)

Thalindra usa `NpcShopController`, não `NpcController`. O fix anterior tentou
conectar a quest via `NpcController.HandleChoiceSelected`, mas esse caminho não
roda para Thalindra. O `NpcShopController` controlava o fluxo inteiro:
`ShowOpeningDialogue()` → `ShowShopMenuOrClose()` → `ShowShopMenu()` (fixo: Comprar/Vender/Adeus).

## Solução implementada

`NpcShopController` agora detecta Thalindra por `_npcData.NpcId == "npc_thalindra"`
(fallback: `DisplayName` contém "Thalindra"). Quando detectado:

- `HandleOpeningClosed()` chama `ShowThalindraQuestShopDialogue()` em vez de `ShowShopMenu()`
- `ShowThalindraQuestShopDialogue()` usa `DialogueModal.ShowWithChoices()` com:
  - `"! Qual é a tarefa?"` (se quest não aceita) ou `"Entregar suprimentos"` (se pronta)
  - `"Comprar"` → abre `BuyPanel`
  - `"Vender"` → abre `SellPanel`
  - `"Adeus"` → fecha interação
- Voltar de Comprar/Vender retorna ao menu de escolhas (não ao ShopMenuModal)
- Ao escolher quest: publica `QuestGiverInteractedEvent` sem `ClearAllModals` para preservar `QuestOfferPanel`

## Não precisa de wiring manual

- Não precisa de DialogueTree asset
- Não precisa de NpcController
- Não precisa remover QuestGiverInteractable

## Teste esperado no Unity Play Mode

1. TownScene → Press Play → interagir com Thalindra
2. Aparece diálogo com opções:
   - `! Qual é a tarefa?`
   - `Comprar`
   - `Vender`
   - `Adeus`
3. Clicar `! Qual é a tarefa?` → abre `QuestOfferPanel` (IMGUI) com título/objetivos/recompensas
4. Clicar `Aceitar` → quest `quest_first_supplies_for_cindar` aceita
5. Ao retornar a Thalindra com objetivos completos → aparece `Entregar suprimentos` em vez de `! Qual é a tarefa?`
6. Clicar `Comprar` → abre loja de compra → `Back` retorna ao menu de escolhas
7. Clicar `Vender` → abre loja de venda → `Back` retorna ao menu de escolhas

## Modal Guard (verificar em Play Mode)

Durante o diálogo de escolhas ou o QuestOfferPanel:
- Dash (Space + direcional) não deve funcionar
- Dodge (duplo tap direcional) não deve funcionar
