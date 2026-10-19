---
doc_type: game_rule
status: accepted
domain: farming-gameplay
source_adrs: []
source_documents:
  - .specs/implementados/SPEC_10_FARM_PERSISTENCE.md
  - .specs/implementados/SPEC_12_FARMING_SYSTEM_CLOSEOUT.md
last_reviewed: 2026-06-01
---

# Farm Rules

## Purpose

Defines farming mechanics, crop growth, resource management, and daily cycles.

---

## Canonical Rules

### Rule: Farm Layout and Plots

- **Rule:** Farm has fixed layout:
  - 10 planting plots (2x5 grid)
  - 1 barn for storage and crafting
  - 1 water source for watering
  - Walking paths between areas

- **Plot state:** Each plot can be:
  - Empty (no crop)
  - Planted with seed (growing)
  - Ready to harvest (crop mature)
  - Harvested (empty again)

- **Applies to:** Farm exploration and farming gameplay

### Rule: Crop Growth Cycle

- **Rule:** Crops grow through stages:
  1. **Planting:** Player selects seed from inventory and plants in plot
  2. **Growth:** Crop grows over time (4 days standard; configurable per crop)
  3. **Ready:** Crop indicates readiness (visual indicator, glow, etc.)
  4. **Harvest:** Player interacts with plot to harvest
  5. **Yield:** Player receives harvest (vegetables, resources, etc.)

- **Growth speed:** 1 day per growth stage (4 days to mature)
- **Watering:** Crops require daily watering (not implemented in MVP; always water available)
- **Constraint:** Crops do not die from neglect in MVP

- **Applies to:** Daily progression and resource farming

### Rule: Crop Types and Yields

| Crop | Seed | Days to Mature | Yield | Gold Value |
|---|---|---|---|---|
| Turnip | Turnip Seed | 3 | Turnip x2 | 25g |
| Carrot | Carrot Seed | 4 | Carrot x2 | 30g |
| Pepper | Pepper Seed | 4 | Pepper x1 | 40g |
| Cabbage | Cabbage Seed | 5 | Cabbage x1 | 35g |
| Wheat | Wheat Seed | 3 | Wheat x3 | 50g |

### Rule: Daily Farm Reset

- **Rule:** At daily midnight (in-game day advance):
  - All crops advance 1 growth stage
  - Ready crops remain ready (don't expire)
  - New day counter increments
  - Resources (water) replenish

- **Time:** In-game day advances at real-time midnight or when player sleeps
- **Applies to:** Daily progression and farming loop

### Rule: Inventory and Storage

- **Rule:** Farm storage operates separately from main inventory:
  - Farm produces crops; stored in barn storage (50 slot limit)
  - Main inventory carries equipment + consumables (20 slots)
  - Player can transfer items between farm storage and inventory
  - Barn storage persists in save

- **Constraint:** Farm storage has size limit; cannot overflow
- **Applies to:** Resource management

### Rule: Farming Economy

- **Rule:** Farm produces income:
  - Harvest crops → sell to vendor
  - Vendor buys crops at fixed prices (see Crop Types table)
  - Gold gained → used for equipment, respec, recovery, etc.

- **Gold generation:** Average 50-100 gold per day (depends on crops planted)
- **Optional:** Player can choose not to farm (not required for progression)

- **Applies to:** Economy and player funding

### Rule: Farm Upgrades (Future)

- **Rule:** Farm can be upgraded (Batch 2):
  - More plots (currently 10; max 20 possible)
  - Better tools (faster growth or higher yields)
  - Barn capacity increase

- **Current MVP:** No upgrades; fixed 10 plots, fixed yields
- **Cost:** Planned to require gold + resources
- **Applies to:** Long-term progression (not MVP)

---

## Farm State Persistence

- **Rule:** Farm state saved in GameSaveData:
  - Plot states (empty, planted, stage, crop type)
  - Growth timers (days until mature per plot)
  - Barn inventory (crops stored)
  - Farm upgrades (if any purchased)

- **Save format:** FarmSaveData with plotStates list + barnInventory list
- **No Unity refs:** Only crop IDs and counts; resolved at load time

- **Applies to:** All save/load mechanics

---

## Daily Loop Integration

- **Rule:** Farm integrates into daily cycle:
  - Morning: Player wakes at farm (spawn location)
  - Day: Player can plant, water, manage crops OR go adventuring
  - Evening: Player returns to farm (manual travel or fast travel)
  - Night: Player sleeps (advances day; resets resources)

- **Optional:** Player need not farm every day (not forced progression)
- **Applies to:** Daily routine and time management

---

## Open Questions

- Can crops be harvested before ready? (Current: no; must wait for ready state)
- Do crops die if not watered? (Current: no; always watered in MVP)
- Can players plant multiple crops of same type? (Current: yes; up to 10 plots)
- Are there seasonal crops? (Current: no; all crops year-round in MVP)
- Can farm be expanded? (Current: no in MVP; Batch 2 feature)

---

## Related ADRs

- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (farm state persistence)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (harvest/growth events)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: SPEC_10, SPEC_12*
