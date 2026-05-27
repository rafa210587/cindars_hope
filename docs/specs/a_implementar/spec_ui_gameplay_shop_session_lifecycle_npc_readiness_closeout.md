# SPEC 17E - Fix definitivo ShopSession lifecycle e NpcShopController readiness

**Status:** Implementado em codigo - validacao Unity e Play Mode pendentes
**Ordem de execucao:** 17E
**Depende de:** SPEC 17D implementada em codigo
**Bloqueia:** fechamento humano final de shops/UI gameplay

## /speckit.specify

Garantir que `shop_weapons_armor` e `shop_seeds_tools` sejam registrados no `ShopManager` persistente usado pelos NPCs e pelos paines de compra/venda, inclusive apos transicao para `TownScene`.

## /speckit.plan

- Promover `ShopManager` ao contexto persistente de `GameBootstrap`, com rebind explicito no save.
- Tornar `NpcShopController` idempotente em `OnEnable`, `Start`, interacao e buy/sell.
- Separar diagnosticos de readiness, refs, asset vazio e sessao ausente.
- Expor sessoes registradas no `ShopManager`.
- Validar scene wiring, assets, sessoes e missing scripts em Editor utility.

## /speckit.tasks

- [x] Registrar causa raiz e evidencia de baseline.
- [x] Implementar lifecycle persistente e readiness idempotente.
- [x] Implementar validator `Validate Town Shop Wiring`.
- [x] Executar gates automaticos disponiveis.
- [x] Registrar pendencias de Play Mode final.
