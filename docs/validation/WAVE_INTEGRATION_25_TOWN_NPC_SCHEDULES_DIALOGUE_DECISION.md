# WAVE INTEGRATION 25 - Town NPC Schedules + Dialogue Expansion - Decision

## Status

BUILD_VALIDATED_WITH_SCENE_WIRING_DEBT

---

## Sources Read

| Document | Found | Notes |
|---|---:|---|
| CLAUDE.md | YES | Router rules |
| CURRENT_STATE.md | YES | All WAVE20-24 gates satisfied |
| WAVE25 spec (session) | YES | Full spec reviewed |
| docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md | YES | Canonical roster source |
| docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md | YES | Layout/schedule source |
| docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SHOP_REPORT.md | YES | Prior NPC report |
| docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md | YES | MVP roster (7 NPCs) |
| docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_CANONICAL_ROSTER.md | YES | Full roster (23 NPCs) |
| docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_DIALOGUE_SETS.md | YES | All 23 dialogue sets |
| docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_MOVEMENT_SCHEDULES.md | YES | All 23 schedule profiles |
| docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_SHOP_SERVICES.md | YES | All 23 shop/service definitions |
| docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_TOWN_PLACEMENT_MAP.md | YES | All 23 placements |
| Assets/_Game/Scripts/NPC/*.cs | YES | Full NPC runtime audit |
| Assets/_Game/Scripts/City/Schedule/*.cs | YES | FULL schedule system found |
| Assets/_Game/Scripts/Save/SaveData.cs | YES | NpcManagerSaveData/NpcSaveData |

---

## Gate Validation

| Gate | Status | Evidence |
|---|---|---|
| WAVE20 | SATISFIED | docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md |
| WAVE21 | SATISFIED | docs/validation/WAVE_INTEGRATION_21_POST_ACCEPTANCE_BUGFIX_REPORT.md |
| WAVE22 | SATISFIED | docs/validation/WAVE_INTEGRATION_22_DEBT_BACKLOG_REPORT.md |
| WAVE23 | SATISFIED | GameplayFeedbackService available (commit 1f82f1c) |
| WAVE24 | SATISFIED | FarmDailyGoalService available (commit bb3fd61) |
| P0/P1 open | 0 | CURRENT_STATE.md confirmed |

---

## Existing NPC Audit Summary

### Core Systems - ALL EXIST, DO NOT RECREATE

| System | File | Status |
|---|---|---|
| NpcManager | Assets/_Game/Scripts/NPC/NpcManager.cs | FULLY_IMPLEMENTED |
| NpcController | Assets/_Game/Scripts/NPC/NpcController.cs | FULLY_IMPLEMENTED |
| NpcShopController | Assets/_Game/Scripts/NPC/NpcShopController.cs | FULLY_IMPLEMENTED |
| NpcWanderer | Assets/_Game/Scripts/NPC/NpcWanderer.cs | FULLY_IMPLEMENTED |
| NpcScenePlacementMarker | Assets/_Game/Scripts/NPC/Runtime/NpcScenePlacementMarker.cs | FULLY_IMPLEMENTED |
| NpcDataSO | Assets/_Game/Scripts/NPC/NpcDataSO.cs | FULLY_IMPLEMENTED |
| DialogueTreeSO | Assets/_Game/Scripts/NPC/DialogueTreeSO.cs | FULLY_IMPLEMENTED |
| NpcInteractionStartedEvent | Assets/_Game/Scripts/Core/Events/NpcInteractionEvents.cs | FULLY_IMPLEMENTED |
| NpcInteractionEndedEvent | Assets/_Game/Scripts/Core/Events/NpcInteractionEvents.cs | FULLY_IMPLEMENTED |
| DayStartedEvent | Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs | FULLY_IMPLEMENTED |
| NpcManagerSaveData | Assets/_Game/Scripts/Save/SaveData.cs | FULLY_IMPLEMENTED |
| NpcSaveData | Assets/_Game/Scripts/Save/SaveData.cs | FULLY_IMPLEMENTED |
| NpcScheduleDefinition | Assets/_Game/Scripts/City/Schedule/NpcScheduleDefinition.cs | FULLY_IMPLEMENTED |
| NpcScheduleResolver (pure) | Assets/_Game/Scripts/City/Schedule/NpcScheduleResolver.cs | FULLY_IMPLEMENTED |
| SchedulePeriod | Assets/_Game/Scripts/City/Schedule/SchedulePeriod.cs | FULLY_IMPLEMENTED |
| SchedulePeriodHelper | Assets/_Game/Scripts/City/Schedule/SchedulePeriod.cs | FULLY_IMPLEMENTED |

### NOT Found (TimeBlock)

- GameTimeManager does NOT publish a `TimeBlockChangedEvent` — only `DayStartedEvent` and `GameTimeTickEvent`
- TIME_BLOCK_DEBT: schedule advances only on DayStartedEvent (per-day resolution)

---

## Roster Decisions

23 canonical NPCs from WAVE12C:
- 7 original MVP (WAVE12): pip, sylveth, brumdar, renko, thalindra, zrix, nimble
- 16 extended (WAVE12C): corvus, mara, gurd, hund, ozzra, gruta, yael, dagna, alaric, mirela, eiran, liora, orlan, savra, tovin, maelor

All 23 have: NpcDataSO assets, DialogueTreeSO assets (10+ nodes), NpcScenePlacementMarker in TownScene.

---

## Dialogue Strategy

All 23 NPCs already have DialogueTreeSO assets with >=10 nodes (WAVE12C).
WAVE25 creates NpcDialogueSetRegistry (C# class) to document the dialogue coverage for validator access.
No new dialogue lines needed — all content exists.

---

## Schedule Strategy

Existing system in `Assets/_Game/Scripts/City/Schedule/`:
- `NpcScheduleDefinition` (pure data model) - EXISTS
- `NpcScheduleResolver` (pure resolver) - EXISTS
- `SchedulePeriod` enum (Morning/WorkStart/Midday/WorkAfternoon/Evening/Night/SleepLateNight) - EXISTS

TIME_BLOCK_DEBT: GameTimeManager does not publish TimeBlockChangedEvent. Schedule advances only on DayStartedEvent.
Strategy: NpcScheduleService subscribes to DayStartedEvent, resolves default blocks, teleports NPCs to home anchors.
No pathfinding — teleport/set position only.

NpcScheduleAnchor: new lightweight MonoBehaviour to mark positions (reuses NpcScenePlacementMarker data).

---

## Scene Edit Strategy

DO NOT rebuild TownScene. DO NOT manually edit .unity YAML.
All 23 NPCs already have NpcScenePlacementMarker wired in TownScene (WAVE12C).
NpcScheduleService will resolve positions from existing NpcDataSO.DefaultPosition.
Human must wire NpcScheduleService MonoBehaviour to TownScene if needed.

---

## Save/Load Strategy

ALREADY IMPLEMENTED in SaveManager + NpcManager:
- NpcManagerSaveData persists all NPCs' HasMet (bool) + Position (Vector2) + NpcId
- SaveManager calls NpcManager.CaptureSaveData/RestoreFromSaveData
- NpcScheduleRuntimeState is transient (not persisted - schedule advances fresh on DayStarted)
- No save debt for basic position/HasMet

---

## Design/Direction Compliance Matrix

| Direction | Constraint | Applied |
|---|---|---|
| City NPC Roster v1.1 | 23 canonical NPCs with roles/services | WAVE12C complete |
| No recreation of NpcManager/ShopManager | All reused from WAVE08/12 | YES |
| Thalindra quest+shop+adeus | NpcShopController fully implements this | ALREADY_IMPLEMENTED |
| 5+ NPCs with distinct dialogue | All 23 have 10+ nodes each | YES |
| NPC during interaction: no wander | NpcWanderer.SetInteractionPaused(true) called | ALREADY_IMPLEMENTED |
| Save/load position+HasMet | NpcManagerSaveData covers both | ALREADY_IMPLEMENTED |
| Modal/input guard: dash/dodge blocked during dialogue | ModalManager handles this | ALREADY_IMPLEMENTED |
