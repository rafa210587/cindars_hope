# SPEC 17E - Repro Before - 2026-05-26

## Erro observado no Play Mode

`NPC_WeaponsArmorShop` chega ao menu, mas ao selecionar Buy registra que `_shopManager` nao possui sessao para `shop_weapons_armor`.

## Diagnostico estatico do baseline (`fffeaa9`)

- `TownScene` possui exatamente um `ShopManager`; ambos os `NpcShopController` apontam para ele e para os mesmos paines.
- Esse `ShopManager` esta no GameObject `_Bootstrap` de `TownScene`.
- `GameBootstrap.Awake()` usa singleton persistente com `DontDestroyOnLoad` e destroi o bootstrap duplicado de uma cena carregada depois.
- `FarmScene` e `CaveScene` nao serializam `ShopManager`; quando uma delas inicia a sessao, o bootstrap persistente nao fornece manager de shops.
- Inferencia de causa raiz: ao entrar em Town a partir de outra cena, as referencias dos NPCs apontam ao bootstrap local destruido; as sessoes inicializadas apenas em `Start()` deixam de estar disponiveis no contexto persistente usado durante a interacao.

## Assets auditados

- `Shop_Weapons_Armor.asset` possui id `shop_weapons_armor` e quatro itens.
- `Shop_Seeds_Tools.asset` possui id `shop_seeds_tools` e cinco itens.
- Os itens inspecionados possuem `BaseValue > 0`; a existencia efetiva no `ItemDatabaseSO` sera reafirmada pelo validator Editor.
