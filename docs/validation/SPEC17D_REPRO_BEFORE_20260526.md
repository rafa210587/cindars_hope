# SPEC 17D - Repro Before - 2026-05-26

## Relato recebido

- Play Mode em `TownScene`: `BuyPanel` reportou `ShopManager was not initialized` para `shop_seeds_tools`.
- O mesmo contrato precisava ser verificado para `SellPanel`.
- O painel `L` ainda nao oferecia selecao de item compativel por slot.

## Evidencia no baseline commitado (`90b5693`)

- Inspecao de `Assets/_Game/Scenes/TownScene.unity` encontra um `ShopManager` serializado e os dois `NpcShopController` apontando para o mesmo `BuyPanel`, `SellPanel` e `ShopManager`.
- `NpcShopController.Start()` chama `BuyPanel.Initialize(...)` e `SellPanel.Initialize(...)`, mas `Show(...)` nao possui contrato verificavel de que o painel permanece ligado ao mesmo contexto do NPC que abriu a transacao.
- `CharacterEquipmentPanelController` oferece apenas `Desequipar`; nao ha botoes `Equipar/Trocar` nem transicao para inventario em modo de selecao.
- `InventoryPanelController` marca/limpa equipamento por `itemId`; com copias iguais, a operacao pode limpar mais de um binding.

## Limite da reproducao automatica

O fluxo de clique em Play Mode nao foi executado automaticamente nesta etapa. A correcao sera validada por compile, validator de wiring, scanner de missing scripts e testes automatizados disponiveis; o fluxo interativo final permanece humano.
