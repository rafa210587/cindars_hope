---
doc_type: game_rule
status: accepted
domain: economy-gameplay
source_adrs:
  - ADR-0006
  - ADR-0007
  - ADR-0010
source_documents:
  - docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
  - docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md
  - docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
  - Assets/_Game/Scripts/Economy/EconomyManager.cs
  - Assets/_Game/Scripts/Economy/ShopManager.cs
  - Assets/_Game/Scripts/Economy/Pricing/EconomyPricingService.cs
last_reviewed: 2026-06-13
---

# Economy Rules

> **This document is current operational behavior plus the binding direction it converges
> toward, not desired future state.** Change a canonical value only via a new ADR or spec.
> Where live runtime code and the canonical direction disagree, the disagreement is called
> out explicitly below (see "Two Economy Paths Coexist") so executors do not silently pick
> the wrong number.

## Purpose

Defines how prices are computed, how the player earns and spends gold, how shop stock is
restocked, and which rules keep the economy from being broken by arbitrage. It is the
canonical "what" for pricing/value/sell/buy/restock/sinks/sources; it does **not** redefine
equipment stats, loot tables, the item list, or save schema (those have their own canonical
sources — see Cross-References).

Scope:

- Base value (`BaseValue`) per item category and how it is authored.
- Quality multipliers for crops/animal products (the live 3-level system) vs. the future
  Q0-Q4 system.
- Sell price vs. buy price and the margin invariant.
- Shop stock restock timing and the no-reload-restock rule.
- Gold sinks and sources, and the anti-arbitrage invariants that protect them.
- Equipment upgrade cost and durability-by-class×material (cross-referenced, not duplicated).
- The Mana Fruit (`item_fruto_mana`) special-channel-only sale.
- Cave gate / boss reward rules (first-time vs. repeat).

Out of scope (owned elsewhere): equipment stat budgets, weapon damage/ASPD/armor, the nominal
item list and per-item `BaseValue` table, the save DTO field list, and the skill tree
economy hooks (`IGoldDropModifier`, etc.).

---

## Definitions and Terms

| Term | Meaning |
|---|---|
| `BaseValue` (BV) | The raw economic value of an item at Normal quality, before any channel, quality, rarity, demand, reputation, season, or markup factor. Authored per item in the catalog; required `> 0` for any sellable item. |
| `SellPrice` | Gold the player receives when selling an item. |
| `BuyPrice` | Gold the player pays to buy an item from a shop/service. |
| `ShopSellToPlayerPrice` | Price the shop charges the player (a buy, from the player's view). |
| `ShopBuyFromPlayerPrice` | Price the shop pays the player (a sell, from the player's view). |
| `ShippingPrice` | Price received via the SellPoint / shipping box. |
| `ServicePrice` | Cost of a service: repair, upgrade, heal, identify, translate, build. |
| `OrderRewardValue` | Total reward of a commission/order, possibly mixing gold, reputation, item and unlock. |
| Channel | The route a transaction takes (SellPoint, generic shop, specialized shop, order, festival, service). Each channel has its own multiplier. |
| Quality | Normal / Silver / Gold tiers for crops and animal products (3 levels, live). Distinct from Tier and from Rarity. |
| Rarity | Common…Unique. Affects price, stock and availability. Distinct from Quality. |
| Restock | Replenishing shop stock at a defined moment (day start, week, season, event). Never on menu open. |
| Sink / Source | A gold sink removes gold from the economy; a source adds it. The economy is balanced by keeping sinks meaningful and sources bounded. |

Three-axis rule (canonical): **Tier ≠ Quality ≠ Rarity.** They are independent and must not
be collapsed into one multiplier.

---

## Two Economy Paths Coexist (executor note)

The runtime currently contains two economy implementations. Both are live code; a future
spec is expected to converge the first onto the second.

1. **MVP transaction path (simplest, currently wired to scene interactables):**
   - `EconomyManager` handles `ItemPurchaseRequestedEvent` and `SellAllRequestedEvent` over
     the `GameEventBus`.
   - SellAll values items at **full `BaseValue × amount`** with no channel multiplier
     (`EconomyManager.cs:131`), and `SellPoint` does the same (`SellPoint.cs:50`).
   - `ShopManager` (NPC shops) applies a sell multiplier (`SellPriceMultiplier`, default
     `0.6`) and a buy multiplier (`BuyPriceMultiplier`, default `1.0`) and restocks daily
     (`ShopManager.cs:271-299`, `357-374`).
2. **Direction-aligned pricing path (matches `ECONOMY_PRICING` formulas):**
   - `Economy/Pricing/EconomyPricingService` computes price from `BaseValue × channel ×
     quality × rarity × demand × reputation × season × stock × story`, floored, min 1
     (`EconomyPricingService.cs:43-46`).
   - `Economy/Pricing/PricingProfile` holds the canonical multiplier tables.
   - `Economy/Pricing/AntiArbitrageValidator` enforces the margin invariants.
   - `Shops/ShopRestockProcessor` + `Shops/ShopStockState` implement the StockLine restock
     model with persisted counters.

**Rule for executors:** when a spec touches pricing, prefer the `Economy/Pricing` service as
the canonical formula owner. The full-`BaseValue` SellAll behavior is an MVP convenience and
**must not** be treated as the canonical sell price; it is the most generous channel and is
itself a latent arbitrage risk until reconciled.

---

## Canonical Rules

### Rule: Base Value is mandatory per sellable item

- **Rule:** Every sellable item declares `BaseValue > 0`. `CanSell = false` ignores the
  common sell price. Quest items, key items, and unique/lore-locked items are non-sellable
  (or require a strong confirmation) and are never priced for ordinary trade.
- **Code:** `SellableItemPolicy.IsSellable` rejects `BaseValue <= 0`, `KeyItem`, `Quest`, and
  essential tools (`pickaxe`/`hoe`/`watering`) (`SellableItemPolicy.cs:9-48`). The pricing
  service blocks quest/key/lore-locked/non-sellable items before computing a price
  (`EconomyPricingService.cs:20-26`).
- **Applies to:** all sell/buy transactions and shop initialization.
- **Source:** ECONOMY_PRICING §3, §J; ITEM_CATALOG §1.

### Rule: Base Value baseline by category

- **Rule:** `BaseValue` is authored within direction baselines per category. These are
  starting ranges to be tuned by playtest, **not** final per-instance values; the nominal
  per-item BV table is owned by ITEM_CATALOG, not by this rule.
- **Constraint:** processed/crafted goods derive BV from their inputs (see formula below);
  equipment BV derives from material + recipe + tier + stat budget + rarity (stats owned by
  equipment docs).
- **Applies to:** item authoring and the data generators that derive item assets.
- **Source:** ECONOMY_PRICING §4, §6, §7; ITEM_CATALOG (nominal table).

Category baseline ranges (Normal quality, Common rarity, in gold; direction baseline):

| Category | BaseValue baseline | Note |
|---|---:|---|
| Common wood/stone resource | 2-6 | abundant |
| Fiber / forage | 4-14 | season/rarity dependent |
| Seed | 5-80 | per crop/season/expected profit |
| Crop | 8-120 | growth/yield/season dependent |
| Animal product | 20-180 | animal/care/quality dependent |
| Fish | 12-160 | rarity/location/weather dependent |
| Ore | 8-80 | depth/tier dependent |
| Ingot | sum(ores) × 1.25-1.60 | refining |
| Gem | 30-250 | sale/order/rare craft |
| Monster part (common / rare) | 8-120 / 80-500 | family/depth/risk |
| Reagent | 8-80 | alchemy/crafting |
| Food | sum(inputs) × 1.10-1.80 | buff-dependent |
| Potion | sum(inputs) × 1.50-3.00 | effect-dependent |
| Oil / processed alchemy | sum(inputs) × 1.20-3.00 | effect-dependent |
| CastScroll T1-T2 / LearnableScroll T1-T2 | 40-180 / 120-500 | consumable < teaching |
| Tome / Wand / Focus-Relic | 400-3000+ / 120-1500+ / 250-3000+ | persistent/rare/lore |

Processed-goods formula (direction): `ProcessedBaseValue = sum(InputBaseValue × Quantity) ×
ProcessingMultiplier (+ optional station-tier flat)`. Crop formula (direction):
`CropQ0BaseValue = round((SeedCost × TargetGrossMultiplier) / ExpectedYieldPerSeedCycle)`.
*Concrete per-item BV values: see ITEM_CATALOG (canonical list); not duplicated here.*

### Rule: Quality multipliers (crops and animal products — LIVE system)

- **Rule (Decision 2.1, binding):** crops and animal products use **3 quality levels** as
  separate items distinguished by suffix:
  - Normal (no suffix): BV × **1.0**
  - `_silver` (Silver): BV × **1.5**
  - `_gold` (Gold): BV × **2.0**
- **Correction:** Gold is × **2.0**, NOT × 2.2. Any generator emitting × 2.2 (the F32
  generator did) is wrong and must be corrected to × 2.0.
- **Constraint:** quality multiplies sale/order/gift value and consumable potency; it does
  **not** substitute equipment tier.
- **Applies to:** crop and animal-product valuation and the quality resolver.
- **Source:** FABLE v3.0 Decision 2.1; ITEM_CATALOG §2 + EMENDA 2026-06-13-V3 E2.1.

### Rule: Quality multipliers (Q0-Q4 — FUTURE system, not crops)

- **Rule:** the Q0-Q4 table — Q0 ×1.00, Q1 ×1.15, Q2 ×1.35, Q3 ×1.70, **Q4 ×2.20** — is a
  **future, more granular instance-quality system**. It is present in code today as the
  pricing-service default `QualityMultipliers` (`PricingProfile.cs:23`) but **does not apply
  to crops/animal products in v1** — those use exclusively the 3-level Silver/Gold system
  above.
- **Constraint:** do not apply Q4 ×2.20 to a crop or animal product; that conflates two
  systems and inflates crop economy.
- **Applies to:** future instance-quality items only.
- **Source:** FABLE v3.0 Decision 2.1; ECONOMY_PRICING §11; ITEM_CATALOG EMENDA E2.1.

### Rule: Sell price, buy price, and the margin invariant

- **Rule (invariant):** for the same item, `ShopSellToPlayerPrice > ShopBuyFromPlayerPrice`.
  The shop must charge more to sell than it pays to buy. The player cannot buy cheap and sell
  back at a profit.
- **Channel sell multipliers (direction-aligned, live in `PricingProfile.cs:12-20`):**
  - SellPoint: × **0.90**
  - Generic shop buys from player: × **0.70**
  - Specialized shop buys from player: × **0.85**
  - Shop sells to player (buy channel): × **1.30**
  - Order reward: × **1.10** (a representative total; orders pay more when they impose a
    condition, see below)
  - Festival reward: × **1.20**
- **MVP shop path (live):** `ShopManager.CalculateSellPrice = floor(BaseValue ×
  SellPriceMultiplier) × amount`, min 1, default `SellPriceMultiplier = 0.6`
  (`ShopManager.cs:271-280`, `ShopDataSO.cs:19`). `CalculateBuyPrice = BuyPriceOverride`, else
  `round(BaseValue × BuyPriceMultiplier) × amount`, min 1, default `BuyPriceMultiplier = 1.0`
  (`ShopManager.cs:282-288`, `ShopDataSO.cs:18`).
- **Pricing-service path (direction-aligned):** unit price = `BaseValue × channelMult ×
  qualityMult × rarityMod × demandMod × reputMod × seasonMod × stockMod × storyMod`, floored,
  min `MinPrice = 1` (`EconomyPricingService.cs:43-46`, `PricingProfile.cs`).
- **Constraint:** no spec may introduce an unbounded buy-cheap-sell-dear loop. Channels that
  pay better than the SellPoint must require a condition (specialized category, order
  quality/quantity/deadline, festival/event).
- **Applies to:** all sell/buy channels.
- **Source:** ECONOMY_PRICING §2, §9, §10, §13-16, §29.

### Rule: Shop stock and restock timing

- **Rule:** restock happens at defined moments — typically at day start, before the shop
  opens — and **never on menu open / scene reload**. Buying decrements current stock; a
  depleted line stays empty until the next valid restock (or permanently for unique/lifetime
  lines).
- **MVP path (live):** shops restock once per in-game day on `DayStartedEvent`, to
  `BaseDailyStock`, only when `DailyRestock` is true and the day advanced
  (`ShopManager.cs:290-299`, `357-374`). Finite-stock entries (`IsFiniteStock`) block buys
  that exceed remaining stock (`ShopManager.cs:159-162`). Stock change publishes
  `ShopStockChangedEvent`; restock publishes `ShopRestockedEvent`.
- **Direction-aligned path:** `ShopRestockProcessor.ProcessDayStart` restocks per StockLine
  policy: `Never` never; `DailyMorning` when `currentDay > LastRestockDay`; `Weekly` when
  `currentDay >= NextRestockDay`. UniqueStock never restocks; LimitedStock respects a lifetime
  cap; RotatingStock uses a per-day seeded roll; daily purchase counters reset on restock
  (`ShopRestockProcessor.cs:8-63`).
- **Constraint:** UniqueStock does not restock; LimitedStock persists counters; rotating
  stock uses a persisted seed so reloading does not reroll the offer.
- **Applies to:** all shop inventories with restockable stock.
- **Source:** ECONOMY_PRICING §17-24, §28.

### Rule: SellPoint / shipping box

- **Rule:** the SellPoint is a safe channel, not a shop. The player deposits items; at the
  day transition the system computes `ShippingPrice`, clears the bin, and credits gold (with
  a morning report). The SellPoint does not buy quest/key/unique items and has no restock.
- **Code:** `SellAllPoint` / `SellPoint` publish `SellAllRequestedEvent` /
  `EconomyTransactionCompletedEvent`; the live SellAll path credits full `BaseValue × amount`
  (`SellPoint.cs:50`, `EconomyManager.cs:131`) — see the MVP-vs-direction note above.
- **Constraint:** the SellPoint is safe/simple but may pay **less** than a specialized shop
  for an accepted category; it is never the best channel by default.
- **Applies to:** the shipping/SellPoint flow.
- **Source:** ECONOMY_PRICING §13, §J.

### Rule: Equipment upgrade cost (derived — cross-reference)

- **Rule (Decision 2.11):** upgrade cost for tier **+N** is
  `Cost(+N) = (2N × MaterialBandValue) + (BV × 0.5N)` gold, where `BV` is the item's base
  value and `MaterialBandValue` is the BV of the item's band material (e.g. iron_ore 15,
  silver_ore 30, mithril_ore 80, bromecian_alloy 90, star_iron 120). It is a `ServicePrice`
  (Brumdar's forge).
- **Worked examples (from ITEM_CATALOG EMENDA E2.11):** `sword_iron` (BV 120, iron_ore 15) +1
  = (2×15) + (120×0.5) = **90g**; +2 = (4×15) + (120×1.0) = **180g**; `sword_mithril`
  (BV 640, mithril_ore 80) +1 = (2×80) + (640×0.5) = **480g**.
- **Constraint:** cost grows with N and with the item's BV — upgrading expensive late gear is
  a deliberate gold sink.
- **Canonical owner:** the upgrade-cost rule is also stated in
  [inventory_equipment_rules.md](inventory_equipment_rules.md); this rule restates it for the
  economy/sink view and **must stay numerically identical** to it. The material-consumed-per-
  upgrade list is owned by the forge spec, not here.
- **Source:** FABLE v3.0 Decision 2.11; ITEM_CATALOG EMENDA E2.11.

### Rule: Durability by class × material (cross-reference)

- **Rule (Decision 2.11):** maximum durability is derived: base **80 for weapons, 150 for
  armor/shields**, multiplied by the per-material modifier from EQUIPMENT_MECHANICAL_BASELINES
  §16/§26 (e.g. Wood/Leather low, Iron +0%, Steel +15%, Mithril +35%, Bromecian +40%). Repair
  restores durability for a gold cost (a sink); current durability persists per item instance.
- **Economic relevance:** durability loss + repair is a recurring gold sink that scales with
  how much the player fights; it is the routine counterpart to the one-time upgrade sink.
- **Canonical owner:** the derived DurabilityMax table is owned by ITEM_CATALOG EMENDA E2.11
  and restated in [inventory_equipment_rules.md](inventory_equipment_rules.md). This rule does
  **not** re-derive the per-material numbers.
- **Source:** FABLE v3.0 Decision 2.11; ITEM_CATALOG EMENDA E2.11; EQUIPMENT_MECHANICAL_BASELINES §16/§26.

### Rule: Mana Fruit (`item_fruto_mana`) — special-channel-only sale

- **Rule (Decision 5.4 = option B; A.3):** the Mana Fruit is sellable at an extremely high
  price and is extremely hard to produce. Its `BaseValue` is **~5,000g**. It is produced at
  most **1 fruit per bloom**, at most **1 bloom per season**, only after the late-game Mana
  Root awakens.
- **Channel constraint:** the Mana Fruit may be sold **only through a special channel**
  (Finan's caravan / wandering merchant) — **never** through the ordinary shipping/SellPoint.
  Selling it is an **event**, with NPC reactions, not a routine transaction.
- **Constraint:** because it is politically/sacred-significant and unique-per-season, it must
  not be treated as a farmable gold source; the special channel and the per-season cap are
  the anti-inflation guard.
- **Applies to:** the Mana Root/Tree economy and the wandering-merchant channel.
- **Source:** FABLE v2.0 Decision 5.4 (B) and §A.3.

### Rule: Cave gate / boss rewards (first-time vs. repeat)

- **Rule:** boss gates exist at cave levels **15, 30, 45, 60, 75, 90, 100** (100 is the final
  gate, unlocking level 101). Each gate, when first cleared, grants a relevant **first-time
  reward** (a unique/rare component, a craft-progression shortcut, optionally a blueprint or
  lore resource, plus significant — but not primary — gold). A defeated boss must **not**
  grant its unique first-time reward again; repeat clears use a separate, smaller, controlled
  repeat-reward table.
- **Constraint:** boss/gate rewards must not enable infinite gold farming; gold from a boss is
  meaningful but is never the boss's main reward (mechanics + lore + progression are).
- **Concrete per-gate gold amounts:** **proposta a calibrar** — the directions fix the
  *structure* (first-time unique component + significant-not-primary gold; first-time ≠
  repeat) but do not fix per-gate gold numbers. A future loot/economy spec must author and
  playtest them against the farm-gold-per-day vs. cave-gold-per-day balance.
- **Applies to:** cave boss-gate reward tables and the stable-run reward snapshot.
- **Source:** CAVE_DESIGN_DIRECTION §7-10; LOOT_CRAFTING_ECONOMY_DIRECTION §23, §26;
  ECONOMY_PRICING §27 (cave loot tied to run/snapshot; boss first-time ≠ repeat).

### Rule: Gold sinks and sources must stay balanced (anti-arbitrage)

- **Rule:** the economy must avoid early infinite gold. The protected invariants are:
  1. shops buy for less than they sell (`ShopSellToPlayer > ShopBuyFromPlayer`);
  2. limited stock does not reset on menu open; restock is time/event-gated;
  3. unique stock has a persisted purchased flag and never restocks;
  4. buyback (if ever added) must cost more than the price the shop paid;
  5. the SellPoint processes at the day transition and is not a shop;
  6. orders/commissions only pay more when they impose a condition;
  7. rotating stock uses a persisted seed/counter;
  8. boss/gate first-time rewards are not repeatable.
- **Code:** `AntiArbitrageValidator.ValidateArbitrage` flags
  `ShopSellToPlayer <= ShopBuyFromPlayer` unless a bounded exception is declared
  (`AntiArbitrageValidator.cs:21-43`); `ValidateNoInfiniteLoop` flags a specialized buy-from-
  player price `>=` the shop's sell-to-player price (`AntiArbitrageValidator.cs:46-64`).
- **Primary sources (gold in):** SellPoint shipping, shop/specialized sales, crop/animal
  income, fishing, cave loot/ore sales, boss first-time gold, order rewards.
- **Primary sinks (gold out):** shop purchases, equipment upgrades (+N), repairs, identify
  service (Veska), crafting/processing services, farm expansion, decorations, consumables for
  cave preparation.
- **Applies to:** every new gold source or sink a spec introduces.
- **Source:** ECONOMY_PRICING §2, §28, §29; LOOT_CRAFTING_ECONOMY_DIRECTION §"sinks".

---

## Edge Cases

- **Insufficient gold on buy:** the transaction fails and publishes a failed
  `EconomyTransactionCompletedEvent`; no item is granted (`EconomyManager.cs:78-82`,
  `ShopManager.cs:165-168`).
- **Inventory full on buy:** the buy fails before spending; if gold was spent and the add
  fails, gold is **refunded** (`EconomyManager.cs:90-98`, `ShopManager.cs:170-188`).
- **Zero-value or non-sellable item in SellAll:** silently skipped, not sold
  (`EconomyManager.cs:120-136`, `SellableItemPolicy.cs`).
- **Buy amount or cost invalid (`<= 0` amount, `< 0` cost):** rejected with a failure event
  (`EconomyManager.cs:60-76`).
- **Finite stock smaller than requested amount:** buy rejected, no partial fill
  (`ShopManager.cs:159-162`).
- **`BuyPriceOverride > 0`:** overrides the BV×multiplier buy price for that shop entry
  (`ShopManager.cs:282-288`).
- **Restock idempotency:** restocking the same day twice is a no-op (`_lastRestockDay`
  guard, `ShopManager.cs:359`; `LastRestockDay`/`NextRestockDay` guard in
  `ShopRestockProcessor.cs:53-63`).
- **Mana Fruit into ordinary shipping:** must be rejected by the channel (special-channel-only);
  ordinary SellPoint never accepts it.
- **Quest/key/lore-locked/non-sellable item priced:** blocked before any number is computed
  (`EconomyPricingService.cs:20-26`).

---

## What Persists in Save

Persisted (IDs and simple values only — per ADR-0006; no Unity refs):

- Player gold (owned by player/save domain).
- Per-shop stock state: `ShopId`, `LastRestockDay`, and `CurrentStock` per item/StockLine
  (`ShopStockSaveData` / `ShopItemStockEntry`, `ShopManager.cs:397-406`; `ShopStockState`).
- Unique-stock purchased flags and limited-stock lifetime/day counters
  (`ShopStockState`, `ShopRestockProcessor.cs:28`).
- Rotating-stock seed/selection (so reload does not reroll the offer).
- Mana Root bloom state and per-season bloom counter (Mana Root domain).
- Boss/gate first-time-reward-claimed flags (cave snapshot domain — so a unique reward is not
  re-granted).

Not persisted as a primary source (recalculable): final computed prices, and stock that is
purely flag-derived and was not altered by a purchase (recompute on load).

---

## What Tests Must Cover

Per [.claude/rules/testing-quality-gate.md](../../.claude/rules/testing-quality-gate.md),
economy pricing and shop stock are deterministic logic and require **EditMode tests**:

- Sell price and buy price computation (channel × quality × rarity × min-price × rounding),
  including the margin invariant `ShopSellToPlayer > ShopBuyFromPlayer`.
- Quality multipliers: Silver ×1.5 / Gold ×2.0 on crops/animal products; assert Q4 ×2.20 is
  **not** applied to crops.
- Anti-arbitrage: `ValidateArbitrage` and `ValidateNoInfiniteLoop` fail when a loop is
  possible and pass for a healthy spread.
- Restock rules: DailyMorning / Weekly / Never gating; unique never restocks; limited respects
  lifetime cap; rotating uses a persisted seed (same seed → same offer); restock is idempotent
  per day and never triggered by menu open.
- Stock save/load round-trip: `CurrentStock`, `LastRestockDay`, unique flags, limited counters
  survive reload; invalid/unknown saved item ids are ignored, not crashed
  (`ShopManager.cs:386-394`).
- Upgrade cost formula `(2N × material band) + (BV × 0.5N)` for several N and items (must match
  the worked examples).
- Transaction safety: gold refunded when inventory add fails; insufficient gold / full
  inventory / invalid amount paths reject cleanly.
- Mana Fruit: ordinary shipping rejects it; only the special channel accepts it; per-season
  bloom cap holds.
- No-Unity-refs assertion on the shop stock DTOs (ADR-0006).

Save/load and event-bus contract behavior may additionally need a Play Mode / human scenario
where it depends on scene shop wiring (NPC shop open/close, panel flow) rather than pure logic.

---

## Open Questions

- When do the two economy paths converge — i.e., when does the MVP full-`BaseValue` SellAll
  get replaced by the channel-based pricing service? (Tracked as a future reconciliation spec.)
- Concrete per-gate boss gold amounts (proposta a calibrar against farm-vs-cave gold/hour).
- Final per-item `BaseValue` table beyond the catalog baselines (owned by ITEM_CATALOG; needs
  playtest tuning of band 71+).
- Exact special-channel mechanics and NPC reactions for the Mana Fruit sale event.
- Whether reputation/season/demand modifiers ship in v1 (present in the profile as ×1.0
  defaults today) or are deferred.

---

## Fontes

Directions:
- `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md` (§2-4,
  §6-17, §22-29, §J — pricing, BV, restock, anti-arbitrage).
- `docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md` + EMENDA
  2026-06-13-V3 (E2.1 quality 3-level Silver/Gold; E2.11 durability + upgrade cost; nominal BVs).
- `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md` (§16/§26 material
  durability modifiers — referenced, not re-derived).
- `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md` (§7-10 boss gates 15-100).
- `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md` (§23-26
  guaranteed vs. random; boss/elite/common drops; sinks).

Decisions (binding):
- `docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md` — Decision 5.4 (B, Mana Fruit high price) and
  §A.3 (BV ~5,000g, 1 fruit/bloom, ≤1 bloom/season, special-channel-only).
- `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` — Decision 2.1 (3-level quality, Gold ×2.0,
  Q0-Q4 future), Decision 2.11 (durability + upgrade cost), Decision 2.12 (slots, cross-ref).

Code (file:line):
- `Assets/_Game/Scripts/Economy/EconomyManager.cs` (SellAll = full `BaseValue × amount`, line 131;
  buy/refund flow, lines 60-101).
- `Assets/_Game/Scripts/Economy/ShopManager.cs` (`CalculateSellPrice` floor×0.6 default, lines
  271-280; `CalculateBuyPrice`, 282-288; daily restock, 290-299 / 357-374; finite stock, 159-162;
  stock save/load, 376-406).
- `Assets/_Game/Scripts/Economy/ShopDataSO.cs` (`BuyPriceMultiplier=1.0`, `SellPriceMultiplier=0.6`,
  `DailyRestock`, `BaseDailyStock`, lines 16-20; `ShopItemEntry`, 52-67).
- `Assets/_Game/Scripts/Economy/SellPoint.cs` / `SellAllPoint.cs` (SellPoint full BaseValue, line 50).
- `Assets/_Game/Scripts/Economy/SellableItemPolicy.cs` (sellability gates, lines 9-48).
- `Assets/_Game/Scripts/Economy/Pricing/EconomyPricingService.cs` (full multiplier formula, lines
  43-46; protection gates, 20-26).
- `Assets/_Game/Scripts/Economy/Pricing/PricingProfile.cs` (channel/quality/rarity tables, lines 12-37).
- `Assets/_Game/Scripts/Economy/Pricing/AntiArbitrageValidator.cs` (margin + no-loop checks).
- `Assets/_Game/Scripts/Shops/ShopRestockProcessor.cs` (restock policy + counters, lines 8-63).

---

## Cross-References

Sibling game_rules (do not duplicate; defer to the canonical owner):

- [inventory_equipment_rules.md](inventory_equipment_rules.md) — **canonical owner** of the
  derived durability table and the upgrade-cost formula (Decision 2.11). This rule restates
  them for the sink view only and must stay numerically identical.
- [farm_rules.md](farm_rules.md) — crop growth, yields and farm income (the steady gold
  source). Crop sell value uses the BV/quality rules here.
- [save_rules.md](save_rules.md) — save schema and migration rules for shop stock / gold.
- [cave_rules.md](cave_rules.md) — stable-run snapshot that fixes cave reward state across
  revisits (so first-time rewards are not re-rolled or re-granted).
- [event_rules.md](event_rules.md) — `EconomyTransactionCompletedEvent`, `ShopStockChangedEvent`,
  `ShopRestockedEvent` bus contracts.

Related ADRs:

- [ADR-0006: Save Data Contracts — simple DTOs](../decisions/ADR-0006-save-data-contracts-simple-dtos.md)
  (shop stock / gold persist as IDs and simple values only).
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md)
  (all economy transactions flow through the `GameEventBus`).
- [ADR-0010: FABLE Skill and Inventory Rules Reconciliation](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md)
  (durability/upgrade reconciliation that this rule cross-references).

---

*Last Reviewed: 2026-06-13 (created from ECONOMY_PRICING + ITEM_CATALOG EMENDA V3 + FABLE v2.0/v3.0 + live economy code)*
