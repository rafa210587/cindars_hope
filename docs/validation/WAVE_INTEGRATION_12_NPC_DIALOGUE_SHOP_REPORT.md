# WAVE INTEGRATION 12 - NPC Placement, Dialogue and Shop Bridge - Execution Report

## Status

BUILD_VALIDATED_WITH_NPC_DIALOGUE_SHOP_DEBT_PENDING_HUMAN_PLAYMODE

## Summary

WAVE12 was corrected from a vertical slice to a fuller TownScene MVP roster. The TownScene now has 7 MVP NPCs selected from canonical playable-slice needs: town guide, seed/farm merchant, blacksmith/repair merchant, general merchant, library/quest hook NPC, cave rumor/supplies NPC and workshop/crafting NPC. Runtime uses existing generic `NpcController`, `NpcShopController`, `DialogueTreeSO`, `DialogueModal`, `ShopManager`, `BuyPanel`, `SellPanel`, `InventoryManager`, and `PlayerManager`; no one-class-per-NPC scripts or parallel dialogue/shop/inventory systems were created.

## Source Documents Read

| Document | Found | Notes |
|---|---:|---|
| `AGENTS.md` | 1 | Rules and constraints |
| Attached corrective WAVE12 request | 1 | Scope: fix WAVE12 only |
| `docs/specs/a_implementar/spec_wave_integration_12_npc_placement_dialogue_shop_bridge.md` | 1 | Active materialized spec |
| `docs/project/CURRENT_STATE.md` | 1 | Previous WAVE12 vertical-slice status |
| `docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md` | 1 | Prior report audited |
| `docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md` | 1 | Updated |
| `docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md` | 1 | Updated |
| `docs/validation/WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md` | 1 | Updated |
| `docs/validation/WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md` | 1 | Updated |
| `docs/GDD_v2.6.md` | 0 | Reference not found |
| `docs/FASE7_SPEC_MVP_FARM_v2.2.md` | 0 | Reference not found |
| `docs/FASE6_FARM_backlog_v1.2.md` | 0 | Reference not found |
| `docs/FASE6_INDEX_global_v1.2.md` | 0 | Reference not found |
| `docs/DORNECIA_Guia_Completo.md` | 0 | Reference not found |
| `docs/validation/WAVE_07_SCENE_INVENTORY.md` | 1 | Scene inventory reference |
| `docs/validation/WAVE_INTEGRATION_02_SCENE_ARCHITECTURE.md` | 1 | Scene architecture reference |
| `docs/validation/WAVE_INTEGRATION_03_PLAYER_CAMERA_MOVEMENT_REPORT.md` | 1 | Player/camera reference |
| `docs/validation/WAVE_INTEGRATION_09_INVENTORY_TOOLTIP_EQUIPMENT_REPORT.md` | 1 | Modal/inventory reference |

## Preflight

| Check | Result |
|---|---|
| `git fetch origin dev` | PASS |
| Branch | dev |
| Working tree | Dirty before changes: `Assets/_Game/Scenes/TownScene.unity` only |
| Dirty tree classification | Existing user/local TownScene LFS scene change, preserved and completed |
| Last commits | WAVE12 commit `378dcd1` on top |

## Root Cause

Prior WAVE12 reused existing TownScene wiring and expanded only a small vertical slice. It did not materialize the MVP/canonical playable-slice roster in TownScene. Corrective action added the missing MVP NPC data, dialogue trees, scene placement markers, shop ownership mappings and TownScene objects.

## Design/Direction Compliance Matrix

| Direction source | Found? | Rule/constraint extracted | Impact on this spec | Applied? | Evidence |
|---|---:|---|---|---:|---|
| City NPC roster direction | 1 | Canonical city roster has functional classes and services | Select MVP subset from canonical/service needs | 1 | Roster doc |
| City NPC roster direction | 1 | NPCs have roles, services and future social hooks | Purpose/responsibilities documented for each MVP NPC | 1 | Roster and authoring model |
| Economy direction | 1 | Shop buy/sell uses stock/pricing/gold/inventory runtime | Reused `ShopManager` and `ShopDataSO` | 1 | Shop services doc |
| WAVE09 report | 1 | Modal/focus blocks gameplay input | Reused `ModalManager` and existing UI panels | 1 | Runtime audit |
| Corrective request | 1 | Do not execute WAVE13 or create final quest/social systems | Only WAVE12 NPC/dialogue/shop placement was changed | 1 | Scope/debt tables |

## Canonical NPC Roster Summary

| Metric | Value |
|---|---:|
| MVP NPCs selected | 7 |
| MVP NPCs placed in TownScene | 7 |
| MVP NPCs with dialogue tree >=10 nodes | 7 |
| MVP merchants with shop/service | 4 |
| Existing temporary extras retained | 1 |
| Future-scope NPCs documented | 16 |

## Placement Map

Link: `docs/validation/WAVE_INTEGRATION_12_NPC_PLACEMENT_MAP.md`

## Dialogue Coverage

Link: `docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md`

| NpcId | Entries/options | >=10? | Temporary? |
|---|---:|---:|---:|
| npc_pip_miudinho | 10 | 1 | 1 |
| npc_sylveth | 10 | 1 | 1 |
| npc_brumdar | 10 | 1 | 1 |
| npc_renko | 10 | 1 | 1 |
| npc_thalindra | 10 | 1 | 1 |
| npc_zrix | 10 | 1 | 1 |
| npc_nimble | 10 | 1 | 1 |

## Movement Coverage

Link: `docs/validation/WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md`

| MovementProfile | Count | Notes |
|---|---:|---|
| Stationary | 3 | Pip, Thalindra, Nimble |
| ShopKeeperFixed | 4 | Sylveth, Brumdar, Renko, Zrix |
| WanderWithinZone | 1 extra | Existing Wanderer retained |

## Shop/Service Coverage

Link: `docs/validation/WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md`

| NpcId | ShopId | ServiceType | Buy | Sell |
|---|---|---|---:|---:|
| npc_sylveth | shop_seeds_tools | SeedVendor | 1 | 1 |
| npc_brumdar | shop_blacksmith | BlacksmithRepairUpgrade | 1 | 1 |
| npc_renko | shop_general_store | GeneralMerchant | 1 | 1 |
| npc_zrix | shop_cave_supplies | CaveRumorInfo | 1 | 1 |

## Runtime/Code Created

| File | Reason |
|---|---|
| `Assets/_Game/Scripts/NPC/Runtime/NpcScenePlacementMarker.cs` | Stable scene placement authoring/validation adapter |

## Runtime/Code Changed

| File | Reason |
|---|---|
| `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` | Regenerator now creates the 7-MVP NPC TownScene roster |
| `Assets/_Game/Scripts/Editor/Validation/ValidateNpcDialogueShopBridge.cs` | Validator now checks MVP roster assets/docs/shops/scene markers |

## Data Created/Changed

| Area | Files |
|---|---|
| New NPC assets | `Npc_Sylveth`, `Npc_Brumdar`, `Npc_Renko`, `Npc_Thalindra`, `Npc_Zrix`, `Npc_Nimble` |
| New dialogue trees | `DialogueTree_Sylveth`, `DialogueTree_Brumdar`, `DialogueTree_Renko`, `DialogueTree_Thalindra`, `DialogueTree_Zrix`, `DialogueTree_Nimble` |
| Shop ownership | `Shop_Seeds_Tools`, `Shop_Blacksmith`, `Shop_General_Store`, `Shop_Cave_Supplies` |

## Scene Changes

| File/Object | Change | Reason |
|---|---|---|
| `Assets/_Game/Scenes/TownScene.unity` | Added/updated 7 MVP NPC scene objects and placement markers | Correct WAVE12 placement gap |
| `NPCs/NpcManager` | Tracks dialogue NPCs and shop NPCs | Runtime capture/restore and interaction registry |

## Debts

| Debt | Status |
|---|---|
| TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER | Present for MVP dialogue text |
| TODO_NARRATIVE_FINALIZATION | Present |
| Daily schedule/calendar routine | Deferred |
| Relationship/reputation/romance/companions/pets | Not implemented, out of scope |
| Quest final content | Not implemented, out of scope |
| Unity Play Mode | Pending human validation |

## Build Validation

| Target | Result |
|---|---|
| Assembly-CSharp | PASS, 0 warnings, 0 errors |
| Assembly-CSharp-Editor | PASS, 0 warnings, 0 errors |
| Unity batchmode scene generation | NOT RUN - blocked by another Unity instance already open |
| TownScene static YAML check | PASS - 7 MVP NPC names, 7 MVP NpcIds, 0 duplicate fileIDs |
| Dialogue node static check | PASS - 10 node definitions in each new MVP dialogue tree |
| `git diff --check` | PASS |
| `tools/docs/validate_docs.ps1` | FAIL_KNOWN_LEGACY - existing `spec_test_harness_editmode_playmode_quality_gate.md`, old validation ADR/game-rule metadata, and old amendment citations |
| `tools/docs/check_spec_quality.ps1` | FAIL_HARNESS - Pester `Should` used outside `Describe` |

## Final Design/Direction Revalidation

| Check | Result | Evidence |
|---|---|---|
| All listed design/direction references checked | PASS_WITH_MISSING_REFERENCES_DOCUMENTED | Source table |
| Applicable NPC/dialogue/shop rules extracted | PASS | Compliance matrix |
| NPC placement rules applied | PASS_STATIC | Placement map and TownScene markers |
| Dialogue rules applied | PASS_WITH_PLACEHOLDER_DEBT | Dialogue sets |
| Shop/economy rules applied or deferred | PASS | Shop services |
| Inventory rules applied or deferred | PASS | Existing ShopManager/InventoryManager |
| Input/focus/modal rules applied | PASS_STATIC | Existing ModalManager/UI |
| Stable ID rules applied | PASS | NpcDataSO + placement markers |
| No design conflict remains | PASS_WITH_DEBT | Future systems deferred |

## Completeness Revalidation Pass 2

| Check | Result |
|---|---|
| WAVE13/WAVE14/WAVE15 not executed | PASS |
| No final quest/social/reputation/romance/pets added | PASS |
| No class per NPC created | PASS |
| No parallel dialogue/shop/inventory runtime created | PASS |
| TownScene has MVP NPCs by name | PASS_STATIC |
| Each MVP NPC has >=10 dialogue nodes | PASS_STATIC |
| Merchants use real shop runtime | PASS_STATIC |

## Decision

- Can continue WAVE13: NO until human Play Mode validates WAVE12, unless the human explicitly accepts the risk.
- Human Play Mode validation required: YES.
