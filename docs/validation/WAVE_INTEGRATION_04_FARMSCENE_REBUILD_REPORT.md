# WAVE INTEGRATION 04 - FarmScene Rebuild Foundation - Execution Report

## Status

BUILD_VALIDATED_SCENE_WIRED

## Summary

`Assets/_Game/Scenes/FarmScene.unity` was confirmed as the target FarmScene and was updated with a `FarmSceneFoundationZones` visual marker layer. The layer adds 11 non-blocking zone markers with stable IDs for player spawn, crop field, tree resources, rock resources, forage, lake/fishing, shipping/sellpoint, construction, house entrance, town exit, and cave entrance.

No crop, chopping, mining, fishing, shipping, inventory, HUD, NPC, shop, crafting, quest, cave combat, or skill gameplay was implemented. Existing player, camera, spawns, farm plots, trees, fishing spot, portals, bounds, and runtime references were preserved. `CreateMvpFarmScene` was updated so the same foundation zones are recreated if the scene is regenerated.

## Source documents read

| Document | Found | Notes |
|---|---|---|
| `AGENTS.md` | YES | Project rules and scene-change constraints. |
| Attached `WAVE_INTEGRATION_04_farmscene_rebuild_foundation.md` | YES | Active spec supplied by human. |
| `docs/project/CURRENT_STATE.md` | YES | Confirmed WAVE_INTEGRATION_03 baseline and WAVE 07 state. |
| `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md` | YES | Updated with WAVE_INTEGRATION_04 result. |
| `docs/validation/WAVE_INTEGRATION_01_UNITY_CLEAN_BASELINE_SCENE_INVENTORY_REPORT.md` | NO | File not found. |
| `docs/validation/WAVE_07_01_UNITY_CLEAN_BASELINE_SCENE_INVENTORY_REPORT.md` | YES | Prior clean baseline/inventory context. |
| `docs/validation/WAVE_07_SCENE_INVENTORY.md` | YES | Confirms FarmScene exists as active target. |
| `docs/validation/WAVE_INTEGRATION_02_SCENE_ARCHITECTURE.md` | YES | Confirms FarmScene as WAVE integration target. |
| `docs/validation/WAVE_INTEGRATION_02_SCENE_ARCHITECTURE_DECISION.md` | YES | Confirms manager/scene architecture decisions. |
| `docs/validation/WAVE_INTEGRATION_02_MANAGER_AUDIT.md` | YES | Reuse existing runtime managers; do not create parallel managers. |
| `docs/validation/WAVE_INTEGRATION_03_PLAYER_CAMERA_MOVEMENT_DECISION.md` | YES | Confirms target scene/player/camera/movement strategy. |
| `docs/validation/WAVE_INTEGRATION_03_PLAYER_CAMERA_MOVEMENT_REPORT.md` | YES | Confirms WAVE_INTEGRATION_04 can start with residual Play Mode risk. |
| `docs/validation/WAVE_INTEGRATION_03_HUMAN_PLAYMODE_CHECKLIST.md` | YES | Still pending human Play Mode validation. |
| `docs/validation/WAVE_00_12_RECONCILIATION_AUDIT.md` | YES | Legacy validation context. |
| `docs/validation/WAVE_00_12_SPEC_CODE_AUDIT_REPORT.md` | YES | Legacy validation context. |
| `docs/validation/05_spec_farm_scale_tilemap_player_footbox_runtime_execution_report.md` | YES | Confirms `FarmScaleContract`. |
| `docs/validation/05_spec_farm_level1_layout_fixed_anchors_runtime_execution_report.md` | YES | Historical blocked report; code contract now exists in repo. |
| `docs/design/` | YES | Folder exists; farm design docs found. |
| `docs/directions/` | NO | Design/directions folder not found: `docs/directions`. |
| `docs/design_directions/` | NO | Design/directions folder not found: `docs/design_directions`. |

## Preflight

| Check | Result |
|---|---|
| Branch | PASS - `dev` |
| Working tree | PASS - clean before execution |
| Latest local HEAD before execution | `3169116 docs: validar baseline de player camera e movimento` |
| `git fetch origin dev` | PASS |

## Build validation

| Target | Before | After | Result |
|---|---|---|---|
| Assembly-CSharp | PASS - exit code 0, 0 warnings, 0 errors | PASS - exit code 0, 0 warnings, 0 errors | PASS |
| Assembly-CSharp-Editor | PASS - exit code 0, 3 pre-existing warnings, 0 errors | PASS - exit code 0, 3 pre-existing warnings, 0 errors | PASS |
| Docs validation | Not run before this spec | EXPECTED_FAIL_LEGACY_ONLY | PASS_WITH_LEGACY_WARNINGS |

## Target FarmScene

| Field | Value |
|---|---|
| TargetScene | FarmScene |
| TargetScenePath | `Assets/_Game/Scenes/FarmScene.unity` |
| Decision | USE_EXISTING_FARMSCENE |
| Scene changes | YES - FarmScene only |
| Other scene changes | NO |
| Prefab/asset changes | NO prefabs, ScriptableObjects, images, animations, controllers |
| Packages/ProjectSettings changes | NO |

## Scene file modification record

| Field | Value |
|---|---|
| Scene file to modify | `Assets/_Game/Scenes/FarmScene.unity` |
| Reason | The spec explicitly authorizes controlled FarmScene visual foundation changes. |
| Expected changes | Add `FarmSceneFoundationZones` and 11 visual non-blocking zone markers. |
| Backup/restore strategy | Git diff + isolated commit. Static YAML checks were run for duplicate fileIDs, expected stable ID count, marker components, and root registration. |

## Farm contracts audit

| Contract/System | Found | Evidence | Decision |
|---|---:|---|---|
| `FarmScaleContract` | YES | `Assets/_Game/Scripts/Farm/FarmScaleContract.cs` | Use for scale/camera/footbox reference. |
| `FarmLevel1LayoutContract` | YES | `Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs` | Use as macro layout reference; current scene remains centered world coordinates. |
| `FarmBuildingFootprint` | NO | No matching runtime class found in source audit. | Do not create; construction marker prepares future hookup. |
| `FarmExpansionZone` | YES | `Assets/_Game/Scripts/Farm/Expansion/FarmExpansionZone.cs` | Runtime contract exists; no gameplay connected in this spec. |
| Farm resource runtime | YES | `Assets/_Game/Scripts/Farm/Resources/*`, `Assets/_Game/Scripts/World/ResourceNode.cs` | Resource zones marked; no resource gameplay added. |
| Crop/soil runtime | YES | `Assets/_Game/Scripts/Farm/FarmPlot.cs`, crop/soil service files | Existing `FarmPlots` preserved and covered by crop zone marker. |
| Shipping/sellpoint runtime | YES | `Assets/_Game/Scripts/Farm/Shipping/*`, `Assets/_Game/Scripts/Economy/SellPoint.cs` | Shipping zone marked; no sellpoint gameplay added. |

## Existing scene contents

| Expected element | Found | Evidence | Decision |
|---|---:|---|---|
| Player | YES | `FarmScene.unity` contains `Player` and `PlayerController`. | Preserve. |
| Camera | YES | `Main Camera` has `CameraFollow2D`. | Preserve. |
| Spawn points | YES | `farm_default`, `farm_from_town`, `farm_from_cave`. | Preserve. |
| Bounds | YES | `Bounds/Top`, `Bottom`, `Left`, `Right`. | Preserve. |
| Crop field | YES | `FarmPlots` with 9 `FarmPlot_*` children. | Preserve and mark with `Zone_CropField`. |
| Tree resources | YES | `Trees` with 19 `TreeNode` references. | Preserve and mark with `Zone_ResourceTrees`. |
| Rocks | MARKER_ONLY | `Zone_ResourceRocks`. | Prepare future mining hookup; no gameplay. |
| Forage | MARKER_ONLY | `Zone_Forage`. | Prepare future forage hookup; no gameplay. |
| Lake/fishing | YES | `FishingSpot` and lake edge triggers. | Preserve and mark with `Zone_LakeFishing`. |
| Shipping/sellpoint | MARKER_ONLY | `Zone_ShippingSellpoint`. | Prepare future shipping hookup; no gameplay. |
| Construction | MARKER_ONLY | `Zone_Construction`. | Prepare future building hookup; no gameplay. |
| House entrance | MARKER_ONLY | `Zone_HouseEntrance`. | Prepare future home transition; no gameplay. |
| Town exit | YES | `Portal_Farm_To_Town`. | Preserve and mark with `Zone_TownExit`. |
| Cave entrance | YES | `Portal_Farm_To_Cave`. | Preserve and mark with `Zone_CaveEntrance`. |

## Zone map

Link:
- `docs/validation/WAVE_INTEGRATION_04_FARMSCENE_ZONE_MAP.md`

Static scene validation:
- `NO_DUP_FILEIDS`
- `marker stableId count: 11`
- `root in SceneRoots: True`
- All 11 `Zone_*` objects found.
- All 11 `FarmSceneZoneMarker` components found.
- All zone marker colliders are triggers.

## Scene changes

| File/Object | Change | Reason |
|---|---|---|
| `Assets/_Game/Scenes/FarmScene.unity` | Added root `FarmSceneFoundationZones`. | Single visible foundation layer for future scene hookups. |
| `Zone_PlayerSpawn` | Added visual marker + trigger + stable ID. | Marks spawn baseline. |
| `Zone_CropField` | Added visual marker + trigger + stable ID. | Marks existing crop plot area. |
| `Zone_ResourceTrees` | Added visual marker + trigger + stable ID. | Marks existing tree/resource area. |
| `Zone_ResourceRocks` | Added visual marker + trigger + stable ID. | Reserves future rocks/mining area. |
| `Zone_Forage` | Added visual marker + trigger + stable ID. | Reserves future forage area. |
| `Zone_LakeFishing` | Added visual marker + trigger + stable ID. | Marks existing lake/fishing area. |
| `Zone_ShippingSellpoint` | Added visual marker + trigger + stable ID. | Reserves future shipping/sellpoint area. |
| `Zone_Construction` | Added visual marker + trigger + stable ID. | Reserves future construction area. |
| `Zone_HouseEntrance` | Added visual marker + trigger + stable ID. | Reserves future home transition. |
| `Zone_TownExit` | Added visual marker + trigger + stable ID. | Marks existing town transition. |
| `Zone_CaveEntrance` | Added visual marker + trigger + stable ID. | Marks existing cave transition. |
| `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` | Added generator support for `FarmSceneFoundationZones`. | Keeps regenerated scene aligned with manual scene changes. |

## Code created

| File | Reason |
|---|---|
| `Assets/_Game/Scripts/Farm/Scene/FarmSceneZoneMarker.cs` | Minimal marker metadata for scene zones. No gameplay. |

## Human Unity actions required

| Action | Required | Reason |
|---|---:|---|
| Open `FarmScene` in Unity | YES | Confirm scene deserializes visually with no missing script warnings. |
| Run Play Mode checklist | YES | Static/build validation cannot prove camera framing, visual clarity, or Console state. |
| Confirm marker opacity/readability | YES | Visual marker colors are placeholders and may need art-side tuning. |

## Unity validation

Unity validation: NOT RUN
Reason: Unity Editor processes are already running for this editor/project, and starting a concurrent batchmode validation risks project lock/editor contention.
Command attempted: Not attempted to avoid concurrent Unity project lock while existing Unity processes are active.
Residual risk: Unity scene deserialization, Play Mode visual behavior, camera framing, and Console state not validated locally by this run.

## Docs validation residual

`tools/docs/validate_docs.ps1` returned exit code 1 with known legacy-only failures outside this spec scope:

- `.specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md` missing dependency headers: `Ordem de execucao`, `Depende de`, `Bloqueia`.
- `.specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md` missing `required_adrs` and `required_game_rules`.
- Older validation reports missing `validated_adrs` and `validated_game_rules`.
- Two implemented specs cite amendments as canonical sources.

No new WAVE_INTEGRATION_04 report/doc was listed as a docs validation failure.

## Acceptance criteria matrix

| AC | Result | Evidence |
|---|---|---|
| AC-01 | PASS | WAVE_INTEGRATION_03 decision/report/checklist exist and define FarmScene/player/camera/movement. |
| AC-02 | PASS | Runtime build passed before/after. |
| AC-03 | PASS | Editor build passed before/after. |
| AC-04 | PASS | FarmScene target is `Assets/_Game/Scenes/FarmScene.unity`. |
| AC-05 | PASS | Farm contracts audit completed. |
| AC-06 | PASS | Zone map created. |
| AC-07 | PASS | FarmScene has existing visuals plus `FarmSceneFoundationZones`. |
| AC-08 | PASS | Existing player/camera references preserved; no parallel controller/camera created. |
| AC-09 | PASS | Scene modification documented in decision/report. |
| AC-10 | PASS | Human Play Mode checklist created. |
| AC-11 | PASS | Next spec can use zone stable IDs for crops/interactables. |

## Decision

- Can start WAVE_INTEGRATION_05: YES after human Play Mode checklist passes or the human explicitly accepts residual Play Mode risk.
- Blocking issues: None in C# build/static scene wiring.
- Human Play Mode validation required: YES.

## Post-WAVE05 Spatial Reconciliation (2026-06-08)

Problem visual found: Trees scattered across west+east, crop field overlapping cave portal zone, house entrance near bounds edge, zone markers misaligned with objects.

Status before: SCENE_REVERTED_TO_WAVE03 — FarmScene reverted to WAVE03 baseline after YAML corruption in WAVE04 zone wiring attempt.

Layout new: See docs/validation/WAVE_INTEGRATION_04_FARMSCENE_ZONE_MAP.md (v2).

Objects from WAVE05 preserved: FarmPlot.cs crop interactable code intact; CreateMvpFarmScene.cs WAVE04+05 code intact.

Crop plots repositioned: FarmPlots parent moved from (-4.75, -1) to (1, -1.5) to separate from portals; placed inside updated Zone_CropField at (1, -2.25).

References corrected: CreateMvpFarmScene correctly wires all FarmPlot refs (WAVE05 staminaManager, seedDatabase, inventoryManager, smoke hook on FarmPlot_00).

Duplicated/removed: No duplicate crop system; FarmPlot reused as per WAVE05 decision.

What depends on human action: Must run CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene in Unity Editor to apply corrected layout.

New WAVE04 status: CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED
