---
name: economy-balance-tuning
description: Design and verify economy values — prices, rewards, sinks/sources — for farm sim + RPG loops. Use for specs touching shop prices, sell points, quest/loot rewards, upgrade costs, or any new money/resource source or sink.
---

# Skill: Economy Balance Tuning

## Existing guardrails (reuse, don't duplicate)

- `EconomyPricingService` + `EconomyPricingServiceTests` — pricing profiles, buy/sell channels.
- `EconomyAntiArbitrageValidationTests` — no profit loop from buying and reselling through any channel chain. **Any new price/reward must keep these green.**
- Sell flow: SellPoint / shipping (`FarmShippingServiceTests`); shop stock: `ShopInventoryStockTests`.

## Core rules

1. **Every SOURCE names its SINK.** New money/resource inflow (crop, drop, quest reward, service) must state which sink absorbs it (seeds, tools, upgrades, construction, repair, licenses, services). New sink without source = dead content; new source without sink = inflation.
2. **Anti-arbitrage invariant:** for any item, `min(buy price across all channels) > max(sell price across all channels)`, including transformation chains (buy ingredients → craft → sell must be justified as intended gameplay profit, with time/labor cost, not instant arbitrage).
3. **Profit-per-day is the farm sim balance unit.** For a crop: `(sellPrice * yield - seedCost) / growthDays`, adjusted by watering labor and season length. Higher-tier options must win on profit-per-day only when they cost more upfront or demand more skill/infrastructure (greenhouse, fertilizer, processing).
4. **Processing adds value for time:** processed goods (workshop/crafting outputs) should beat raw selling by a margin proportional to processing time + station investment — never below (or processing is dead).
5. **Reward consistency:** a quest/cave reward should pay within ±30% of the best farm activity for the same time investment at the same progression stage. Big outliers must be deliberate (boss, milestone) and documented.
6. **No invented numbers.** If the spec/GDD lacks a value, derive it from a comparable existing item and record the derivation, or STOP and ask (agent: game-design-reviewer).

## Deliverable for any economy spec

A sink/source delta table in the execution report:

```text
| Item/Activity | Source (per day/run) | Sink touched | Profit/day | Comparable | Verdict |
```

Plus: anti-arbitrage tests still PASS, and new entries added to `EconomyAntiArbitrageValidationTests` when new channels appear.
