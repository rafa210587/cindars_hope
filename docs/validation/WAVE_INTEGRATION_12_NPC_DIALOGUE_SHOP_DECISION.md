# WAVE INTEGRATION 12 - NPC Dialogue Shop Decision

Date: 2026-06-09
Status: BUILD_VALIDATED_SCENE_WIRED_PENDING_HUMAN_PLAYMODE

## Decision

Use `TownScene` as the target scene and reuse existing runtime:

| Area | Decision | Evidence |
|---|---|---|
| Target scene | TownScene | Scene already contains NPCs, DialogueModal, ShopCanvas, ShopManager, buy/sell panels |
| NPC model | DATA_DRIVEN_GENERIC_CONTROLLERS | `NpcDataSO`, `NpcController`, `NpcShopController`, `NpcManager` |
| Dialogue | Reuse `DialogueTreeSO` + `DialogueModal` | `NpcController` now sends choices to `DialogueModal` and resolves `NextNodeId` |
| Shop | Reuse `ShopDataSO` + `ShopManager` + `ShopMenuModal` | `NpcShopController` opens menu and panels use real buy/sell APIs |
| Scene edits | No scene YAML edits | Existing TownScene already has NPC/shop/dialogue wiring |
| FarmScene | Not modified | Farm remains prior integration target; social/commercial loop is TownScene |

## Design/Direction Compliance Matrix

| Direction source | Found? | Rule/constraint extracted | Impact on this spec | Applied? | Evidence |
|---|---:|---|---|---:|---|
| `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` | YES | Canonical city roster uses functional classes and service tags; no D&D classes as mechanics | Roster doc uses functional roles and marks most canonical NPCs future scope | YES | `WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md` |
| `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` | YES | Pip is tutorial/commercial/explorer direction | Existing Pip runtime kept; ID alias debt documented | PARTIAL | `npc_pip_miudinho` current asset |
| `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md` | YES | Buy/sell must use value/stock/pricing channels and avoid arbitrage | Shop uses `ShopManager`, `ShopDataSO`, item base value, multipliers, stock | YES | `Shop_Seeds_Tools.asset`, `Shop_Weapons_Armor.asset` |
| `docs/validation/WAVE_INTEGRATION_09_INVENTORY_TOOLTIP_EQUIPMENT_REPORT.md` | YES | ModalManager and input focus must block gameplay during modal UI | Reused `ModalManager` and existing shop/dialogue modal stack | YES | `DialogueModal`, `ShopMenuModal`, `BuyPanel`, `SellPanel` |
| `docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md` | YES | WAVE11 is build-valid but still needs human Play Mode | WAVE12 implemented as build/static scene wired, not accepted | YES | Status remains pending human Play Mode |

## Reference Gaps

Reference not found:
- `docs/GDD_v2.6.md`
- `docs/FASE7_SPEC_MVP_FARM_v2.2.md`
- `docs/FASE6_FARM_backlog_v1.2.md`
- `docs/FASE6_INDEX_global_v1.2.md`
- `docs/CINDARS_HOPE_PROJECT_REFINEMENT_SKILL.md`
- `docs/validation/WAVE_07_PLAYABLE_SCENE_INTEGRATION_ROADMAP_MACRO.md`
- `docs/validation/PLAYABLE_SLICE_INTEGRATION_ROADMAP_MACRO.md`

## Constraints

- No one-class-per-NPC implementation.
- No new inventory/economy wallet.
- No final relationship, romance, reputation, schedule, or quest implementation.
- No scene YAML changes in this execution.
- Human Play Mode validation is still required.
