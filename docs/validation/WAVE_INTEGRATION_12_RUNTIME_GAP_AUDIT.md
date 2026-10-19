# WAVE INTEGRATION 12 - Runtime Gap Audit

Status: WAVE12_READY_FOR_FIX_APPLIED_WITH_HUMAN_PLAYMODE_PENDING

## Artifact Audit

| Artifact | Exists after fix | Notes |
|---|---:|---|
| WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md | 1 | Updated to MVP playable-slice subset |
| WAVE_INTEGRATION_12_NPC_PLACEMENT_MAP.md | 1 | Created |
| WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md | 1 | Updated to 7 MVP NPCs plus existing extra |
| WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md | 1 | Updated |
| WAVE_INTEGRATION_12_NPC_SHOP_SERVICES.md | 1 | Updated |
| WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md | 1 | Updated |
| WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_AUTHORING_MODEL.md | 1 | Updated |
| WAVE_INTEGRATION_12_HUMAN_PLAYMODE_CHECKLIST.md | 1 | Updated |

## TownScene Audit

| Check | Result | Evidence |
|---|---|---|
| TownScene exists | PASS | `Assets/_Game/Scenes/TownScene.unity` |
| MVP NPC objects | PASS | 7 named MVP NPC objects |
| Placement markers | PASS | 7 `NpcScenePlacementMarker` entries for MVP NpcIds |
| Dialogue interactables | PASS_STATIC | `NpcController` for Pip, Thalindra, Nimble; `NpcShopController` for merchants |
| Merchant interactables | PASS_STATIC | Sylveth, Brumdar, Renko, Zrix use `NpcShopController` |
| Player spawn/camera/bounds | PASS_EXISTING | TownScene generator and previous scene reports provide player/camera/bounds |
| Reachability | PASS_STATIC_WITH_HUMAN_RISK | Positions are inside known Town bounds; Play Mode still required |

## Runtime Audit

| System | Found | Decision |
|---|---:|---|
| NpcDataSO | 1 | Reused |
| NpcDefinition | 1 | Reused as pure contract |
| NpcController | 1 | Reused |
| NpcShopController | 1 | Reused |
| DialogueTreeSO | 1 | Reused |
| DialogueModal | 1 | Reused |
| ShopDataSO | 1 | Reused |
| ShopManager | 1 | Reused |
| InventoryManager/PlayerManager | 1 | Reused for buy/sell |
| ModalManager/Input focus | 1 | Reused |
| NpcScenePlacementMarker | 1 | Created as missing authoring/validation adapter |

## Classification

- Prior state: WAVE12_VERTICAL_SLICE_ONLY / WAVE12_PARTIAL_NPC_PLACEMENT.
- Fixed state: BUILD_VALIDATED_WITH_NPC_DIALOGUE_SHOP_DEBT pending human Play Mode.

## Gaps Left

| Gap | Status | Reason |
|---|---|---|
| Final narrative dialogue | DEFERRED | Not in WAVE12 scope |
| Relationship/reputation/romance | DEFERRED | Explicitly out of scope |
| Quest final content | DEFERRED | Explicitly out of scope |
| Daily schedule/calendar routing | DEFERRED | Movement profile documented; final schedules later |
| Human Play Mode | PENDING | Unity editor was already open and batchmode could not run |
