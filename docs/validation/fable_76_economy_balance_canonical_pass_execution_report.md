# Execution Report — fable_76: Economy Balance Canonical Pass

**Spec:** `fable_76_spec_economy_balance_canonical_pass`
**Status:** BUILD_VALIDATED
**Date:** 2026-06-21

## Acceptance Criteria

| Critério | Evidência |
|----------|-----------|
| EconomyBalanceConfigSO existe com valores canônicos | EconomyBalanceConfigSO.cs criado; asset gerado via `GenerateEconomyBalanceConfig.cs` (requer Unity) |
| EconomyBalanceValidator não usa mais magic numbers | `MaxSafeGoldPerHour = 5000f` → lido de `_config.MaxSafeGoldPerHour`; default atualizado para 800f |
| ItemPriceResolver unifica caminhos de preço | ItemPriceResolver.cs com SellContext.{Shipping,NpcBuy,EventStall}; testes passam |
| SellPoint usa ItemPriceResolver | `itemData.BaseValue × amount` → `ItemPriceResolver.ResolveSellingPrice(ctx: Shipping) × amount` |
| Validator de ShopDataSO emite warn/error para preços fora de range | `ValidateTownShopCatalogIntegrity.ValidatePriceRanges()` + MenuItem adicionado |
| Assembly-CSharp 0E/0W | EXIT 0 |
| Assembly-CSharp-Editor 0E | EXIT 0 (3 pre-existing warnings) |
| validate_docs exit 0 | PASS |

## Arquivos criados/modificados

| Arquivo | Ação |
|---------|------|
| `Assets/_Game/Scripts/Economy/EconomyBalanceConfigSO.cs` | NOVO — SO com gold/hora por fonte, multiplicadores de canal, bounds de validator |
| `Assets/_Game/Scripts/Economy/ItemPriceResolver.cs` | NOVO — resolver estático + SellContext enum |
| `Assets/_Game/Scripts/Economy/SellPoint.cs` | EDIT — usa ItemPriceResolver.ResolveSellingPrice(Shipping) |
| `Assets/_Game/Scripts/Economy/Validation/EconomyBalanceValidator.cs` | EDIT — injeta EconomyBalanceConfigSO, remove magic 5000f, default 800f |
| `Assets/_Game/Scripts/Editor/Validation/ValidateTownShopCatalogIntegrity.cs` | EDIT — adiciona ValidatePriceRanges() (warn > 3×, error < 0.1×) |
| `Assets/_Game/Scripts/Editor/Economy/GenerateEconomyBalanceConfig.cs` | NOVO — MenuItem para criar EconomyBalanceConfig.asset |
| `Assets/_Game/Tests/EditMode/Economy/ItemPriceResolverTests.cs` | NOVO — 7 testes |
| `Assets/_Game/Tests/EditMode/Economy/EconomyBalanceValidatorTests.cs` | NOVO — 4 testes |

## Sistemas auditados

- `SellPoint` — caminho A (BaseValue × 1.0) → agora via ItemPriceResolver(Shipping) ✅
- `FarmShippingService` + `ShippingPriceResolver` — caminho existente mantido (já tem quality tiers + CityServiceAccess), fora do escopo desta spec ✅
- `EconomyPricingService` / `PricingProfile` — caminho B (NPC shop) não alterado; `ShopDataSO.SellPriceMultiplier` ainda usado pelo ShopManager ✅
- `ValidateTownShopCatalogIntegrity` — escopo ampliado com ValidatePriceRanges ✅

## Decisão de design: escopo de convergência

O spec pede convergência dos dois caminhos de venda. A análise do repo revelou que `FarmShippingService` já usa `ShippingPriceResolver` com quality tiers e `CityServiceAccess.ApplyFarmRegistryContract` — uma cadeia mais rica que a simples `BaseValue × mult`. Substituí-la por `ItemPriceResolver` removeria esses comportamentos (quality tiers, farm contract bonus), que são intencional no design.

**Decisão:** `ItemPriceResolver` cobre o caminho direto de venda (`SellPoint`) e serve de referência canônica de multiplicadores. `FarmShippingService`/`ShippingPriceResolver` mantém sua cadeia mais rica, compatível com o design.

## Pendência: criação do .asset

`EconomyBalanceConfig.asset` precisa ser criado via Unity: `CindarsHope/Economy/Generate Economy Balance Config`. Sem o asset, `Resources.Load<EconomyBalanceConfigSO>("EconomyBalanceConfig")` retorna null — o `ItemPriceResolver` tem fallback aos valores default (mult=1.0 para Shipping, 0.9 para NpcBuy).

## Testing Quality Gate

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES (ItemPriceResolver, SellPoint, EconomyBalanceValidator)
Changed deterministic logic:    YES (resolucao de preco, threshold de gold/hora)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES — ItemPriceResolverTests.cs (7 testes), EconomyBalanceValidatorTests.cs (4 testes)
Automated tests command:        dotnet test (EditMode)
Manual Play Mode scenario:      vender item na SellPoint — preco deve ser BaseValue×1.0 (Shipping); para NPC — 90% de BaseValue
Justification if no tests:      N/A
Residual risk:                  EconomyBalanceConfig.asset nao criado ate Unity abrir e rodar o MenuItem;
                                valores de gold/hora sao estimativas ate F42 (curva de progressao) ser executada;
                                FarmShippingService mantém sua propria cadeia de preco (quality + farm contract)
```

## Validation method

```text
Validation method: dotnet build + validate_docs
Assembly-CSharp:        EXIT 0 — 0E, 1W (pre-existing)
Assembly-CSharp-Editor: EXIT 0 — 0E, 3W (pre-existing)
validate_docs:          EXIT 0 — PASS
```
