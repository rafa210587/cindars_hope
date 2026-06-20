---
name: economy-balance-tuning
description: Projeta e verifica valores de economy — prices, rewards, sinks/sources — para os loops de farm sim + RPG. Use em specs que toquem shop prices, sell points, quest/loot rewards, upgrade costs, ou qualquer novo source ou sink de money/resource.
---

# Skill: Economy Balance Tuning

O projeto já tem guardrails de economy no `EconomyPricingService` e em testes anti-arbitrage; esta skill garante que todo novo valor mantém esses contratos verdes e nomeia o seu sink.

## Sistemas existentes (reusar, não duplicar)

- `EconomyPricingService` + `EconomyPricingServiceTests` — pricing profiles, canais de buy/sell.
- `EconomyAntiArbitrageValidationTests` — sem profit loop por comprar e revender através de qualquer cadeia de canais. **Todo novo price/reward deve manter esses testes verdes.**
- Sell flow: SellPoint / shipping (`FarmShippingServiceTests`); shop stock: `ShopInventoryStockTests`.

## Regras

1. **Todo SOURCE nomeia o seu SINK.** Novo inflow de money/resource (crop, drop, quest reward, service) deve declarar qual sink o absorve (seeds, tools, upgrades, construction, repair, licenses, services). Novo sink sem source = dead content; novo source sem sink = inflação.
2. **Invariante anti-arbitrage:** para qualquer item, `min(buy price across all channels) > max(sell price across all channels)`, incluindo cadeias de transformação (buy ingredients → craft → sell precisa ser justificado como profit de gameplay intencional, com custo de time/labor, não arbitrage instantânea).
3. **Profit-per-day é a unidade de balance do farm sim.** Para um crop: `(sellPrice * yield - seedCost) / growthDays`, ajustado pelo labor de watering e pela season length. Opções de tier mais alto só devem vencer em profit-per-day quando custam mais upfront ou exigem mais skill/infraestrutura (greenhouse, fertilizer, processing).
4. **Processing agrega valor por time:** processed goods (outputs de workshop/crafting) devem bater a venda do raw por uma margem proporcional ao tempo de processing + ao investimento na station — nunca abaixo (ou o processing é dead).
5. **Consistência de reward:** um quest/cave reward deve pagar dentro de ±30% da melhor farm activity para o mesmo investimento de time no mesmo estágio de progressão. Outliers grandes precisam ser deliberados (boss, milestone) e documentados.
6. **Sem números inventados.** Se a spec/GDD não tiver um valor, derive-o de um item existente comparável e registre a derivação, ou PARE e pergunte (agent: game-design-reviewer).

## Entregável

Para qualquer economy spec, uma sink/source delta table no execution report:

```text
| Item/Activity | Source (per day/run) | Sink touched | Profit/day | Comparable | Verdict |
```

Mais: os testes anti-arbitrage continuam PASS, e novas entradas adicionadas a `EconomyAntiArbitrageValidationTests` quando surgirem novos canais.
