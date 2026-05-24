# Refinement implementado - Economy shop stock, pricing e UI

> Spec relacionada: `docs/specs/implementados/spec_economy_shop_stock_pricing_ui.md`
> Origem absorvida: `docs/refinements/a_implementar/pre_refinamentos/refinamento_init_economy_shop_stock_pricing_ui.md`
> Status: Implementado completo
> Data: 2026-05-24

## Decisoes executadas

- Pip permanece recepcionista, sem `ShopDataSO`.
- Dois lojistas usam IDs estaveis: `shop_weapons_armor` e `shop_seeds_tools`.
- Loja da cidade substitui os pontos legados de venda/compra da fazenda.
- Conteudo de shop e NPC fica em `ScriptableObject`; stock mutavel fica em save por IDs/tipos simples.
- O controle modal permite somente uma superficie interativa por vez.

## Hardening realizado

- Mutacao de ouro, inventory e stock saiu dos paines UI e foi centralizada no `ShopManager`.
- Compra falha sem mutacao se faltar ouro, espaco ou stock.
- Venda filtra itens nao vendaveis e usa multiplicador `0.6` com arredondamento para baixo.
- Restock diario publica `ShopRestockedEvent`; compra publica `ShopStockChangedEvent`.
- Os geradores Editor atualizam conteudo e cenas de forma repetivel.

## Evidencias de aceite

- `Assets/_Game/Scenes/TownScene.unity` contem Pip, dois lojistas e modal UI.
- `Assets/_Game/Scenes/FarmScene.unity` nao contem `SellPoint`/`SeedShopPoint`.
- `Logs/spec06-shop-validation-final.log`: `24 passed, 0 failed`.
- `Logs/spec06-scene-validation-final.log`: validacao de ambas as cenas aprovada.
- `Logs/unity-compile-spec06-corrected.log`: `Tundra build success`.

## Validacao manual pendente

Executar em Play Mode os fluxos de compra, venda, stock esgotado, save/load, restock por novo dia e entrada na cidade com Pip.
