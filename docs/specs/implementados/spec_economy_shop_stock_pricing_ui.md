# SPEC - Economy shop stock, pricing e UI

> Spec ID: spec_economy_shop_stock_pricing_ui
> Status: Implementado
> Ordem de execucao: 06
> Data: 2026-05-24
> Evidencia: Assets/_Game/Scripts/Economy/ShopManager.cs, ShopDataSO.cs, Assets/_Game/Scripts/UI/Shop/BuyPanel.cs, SellPanel.cs, ShopMenuModal.cs, NpcShopController.cs, Assets/_Game/Data/Economy/*.asset

## Resumo de Implementacao

### Implementado:
- `ShopDataSO` com campos: Id, ShopKeeperId, Items[], BaseDailyStock, PriceMultiplier
- `ShopItemEntry` com ItemId, MaxStock
- `ShopManager` como singleton gerenciando multiplas lojas
- `ShopSession` para gerenciar estado de cada loja (estoque, restock, venda/compra)
- Calculo de precos: Buy = BaseValue * PriceMultiplier, Sell = BaseValue * 0.6
- Estoque finito com reposicao diaria idempotente
- Save/load de estoque via `ShopStockSaveData`
- DialogueModal para falas de abertura/despedida
- ShopMenuModal com opcoes Comprar/Vender/Sair
- BuyPanel com lista de itens, precos, estoque, validacao de gold/inventory
- SellPanel com inventory do jogador e precos de venda
- NpcShopController integrando todo o fluxo conversacion/compra/venda
- 2 lojas data criadas: shop_weapons_armor, shop_seeds_tools
- Events: GoldChangedEvent, InventoryChangedEvent, IntegracaoEconomyManager
- Modal stack respeitando exclusividade de modais

### Nao implementado (futuro):
- Afinidade/reputacao como multiplicador futuro
- Schedule completo de NPCs
- Construcao de lojas pelo jogador
- UI final consolidada e visual

### Validacao pendente:
- Unity compile validation
- Play mode test em TownScene
