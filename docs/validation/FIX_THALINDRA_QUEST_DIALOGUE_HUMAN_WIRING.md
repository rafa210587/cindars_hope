# Fix — Thalindra Quest Dialogue Wiring

## Status: RUNTIME_AUTO_WIRED_SHOP_CONTROLLER_FIXED

## Root Cause (resolvido em código)

Thalindra usa `NpcShopController`, não `NpcController`. Tentativas anteriores
conectaram a quest via `NpcController` e `QuestGiverInteractable`, mas esse caminho
não roda quando o GameObject usa `NpcShopController`. O `NpcShopController` controlava
o fluxo inteiro: `ShowOpeningDialogue()` → `ShowShopMenuOrClose()` → `ShowShopMenu()`
(fixo: Comprar/Vender/Adeus).

## Solução implementada (NpcShopController)

`NpcShopController` detecta Thalindra por `_npcData.NpcId == "npc_thalindra"`
(fallback: `DisplayName` contém "Thalindra"). Quando detectado:

- `HandleOpeningClosed()` chama `ShowThalindraQuestShopDialogue()` em vez de `ShowShopMenu()`
- `ShowThalindraQuestShopDialogue()` usa `DialogueModal.ShowWithChoices()` com:
  - "! Qual é a tarefa?" — se quest não aceita
  - "Entregar suprimentos" — se pronta para turn-in
  - "Comprar" → abre BuyPanel
  - "Vender" → abre SellPanel
  - "Adeus" → fecha interação
- Voltar de Comprar/Vender retorna ao menu de escolhas (não ao ShopMenuModal)
- Ao escolher quest: publica QuestGiverInteractedEvent sem ClearAllModals
  para preservar QuestOfferPanel

## Complementar (NpcController + QuestGiverInteractable)

Os commits f6d5bb2 e 5bccb76 adicionaram fallback runtime em NpcController
e QuestGiverInteractable para auto-wire de Thalindra. Esses caminhos rodam
quando o NPC usa NpcController diretamente (sem shop). Para Thalindra, o caminho
que importa é o NpcShopController (fix principal acima).

## Não precisa de wiring manual

- Não precisa de DialogueTree asset
- Não precisa arrastar asset no Inspector
- Não precisa remover QuestGiverInteractable

## Teste esperado no Unity Play Mode

1. TownScene → Press Play → interagir com Thalindra
2. Aparece diálogo com opções:
   - ! Qual é a tarefa?
   - Comprar
   - Vender
   - Adeus
3. Clicar "! Qual é a tarefa?" → abre QuestOfferPanel (IMGUI)
4. Clicar "Aceitar" → quest quest_first_supplies_for_cindar aceita
5. Ao retornar com objetivos completos → "Entregar suprimentos" no lugar de "! Qual é a tarefa?"
6. "Comprar" → abre loja → "Back" retorna ao menu de escolhas
7. "Vender" → abre loja → "Back" retorna ao menu de escolhas

## Modal Guard (verificar em Play Mode)

Durante o diálogo de escolhas ou o QuestOfferPanel:
- Dash (Space + direcional) não deve funcionar
- Dodge (duplo tap direcional) não deve funcionar
