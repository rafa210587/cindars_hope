# WAVE_INTEGRATION_16 — Cave Entrance + Cave Runtime Bridge: Decision Document

## Status

`CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED`

---

## Sources Read

| Source | Status | Notes |
|--------|--------|-------|
| docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md | FOUND | Full cave direction; preserve existing systems |
| docs/game_rules/cave_rules.md | FOUND | ADR-0005; stable run contract |
| docs/decisions/ADR-0005-cave-stable-run-and-replay.md | FOUND | Cave stable run decision |
| docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md | FOUND | Save direction |
| docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md | FOUND | UI direction |
| docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md | FOUND | Quest events |
| docs/validation/WAVE_INTEGRATION_13_SCENE_TRANSITION_REPORT.md | FOUND | WAVE13 baseline |
| docs/validation/WAVE_INTEGRATION_13_STATE_PRESERVATION_MATRIX.md | FOUND | State preservation basis |
| docs/validation/WAVE_INTEGRATION_15_QUEST_GIVER_REPORT.md | FOUND | WAVE15 baseline |
| CAVE_DESIGN_DIRECTION.md section on existing systems | READ | Prohibits rebuilding from scratch |
| docs/design/SPEC_SOURCE_MAP.md | FOUND | Domain map |
| docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md | FOUND | Lore canon |
| docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md | FOUND | Layout direction |

---

## Existing Systems Audit

| System | Exists | Path | Decision |
|--------|--------|------|----------|
| CaveRunManager | YES | Scripts/Cave/Runtime/CaveRunManager.cs | REUSE — manages full run lifecycle |
| CaveRuntimeState | YES | Scripts/Cave/Runtime/CaveRuntimeState.cs | REUSE — full run state (seed, level, snapshots) |
| CaveLevelRuntimeController | YES | Scripts/Cave/CaveLevelRuntimeController.cs | REUSE — level generation/restore in scene |
| CaveEntryController | YES | Scripts/Cave/Runtime/CaveEntryController.cs | REUSE — checkpoint selection for cave entry |
| CaveExitPortal | YES | Scripts/Cave/CaveExitPortal.cs | REUSE — handles BackExit (level 1→Farm) and ForwardExit |
| CaveSpawnAnchor | YES | Scripts/Cave/Runtime/CaveSpawnAnchor.cs | REUSE (enum: Entrance/ForwardExit/BackExit) |
| CaveEntryDataSO | YES | Scripts/Cave/CaveEntryDataSO.cs | REUSE — entry data ScriptableObject |
| SceneTransitionRouter | YES | Scripts/World/Scenes/SceneTransitionRouter.cs | REUSE — WAVE13 transition service |
| SceneId | YES | Scripts/World/Scenes/SceneId.cs | REUSE — stable IDs; SpawnCaveFromFarm = "spawn_cave_from_farm" |
| CaveLevelEnteredEvent | YES | Core/Events/CaveLevelEnteredEvent.cs | REUSE — already covers cave level entry |
| CaveRunRegeneratedEvent | YES | Core/Events/CaveRunRegeneratedEvent.cs | REUSE — already covers run seed change |
| IInteractable | YES | Interaction/IInteractable.cs | REUSE — interface for cave entrance |
| CaveRunState.cs | NOT needed | - | CaveRuntimeState already covers this |
| CaveRunService.cs | NOT needed | - | CaveRunManager already covers this |
| CaveRuntimeBootstrap.cs | NOT needed | - | CaveRuntimeBridge.cs (thinner) created instead |

---

## Decision Matrix

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Cave runtime strategy | USE_EXISTING_CAVE_RUNTIME | CaveRunManager/CaveLevelRuntimeController/CaveExitPortal fully implemented |
| Run state management | USE_EXISTING_CaveRunManager | Handles StartRun (Awake/InitializeIfNeeded), state caching, bootstrap restore |
| Scene transition | REUSE_WAVE13_SceneTransitionRouter | SceneTransitionRouter.Execute() is the canonical transition path |
| Cave scene target | CaveScene (existing) | Only cave scene in project; has CaveRuntime + SpawnPoints |
| FarmScene entrance | Zone_CaveEntrance (existing GO) | Needs CaveEntranceInteractable component wired by human |
| Cave exit (surface) | CaveExitPortal (existing) | BackExit level 1 → FarmScene already implemented |
| Cave exit event | NEW CaveExitedEvent | Not covered by existing events |
| Cave run started event | NEW CaveRunStartedEvent | Published by CaveEntranceInteractable before transition |
| Cave runtime bridge | NEW CaveRuntimeBridge | Thin bridge: DontDestroyOnLoad, publishes CaveExitedEvent, validates entry |
| Save/load cave run | CAVE_RUN_SAVE_LOAD_DEBT | CaveRunManager.CaptureSaveData/RestoreFromSaveData exists; integration with SaveManager deferred to WAVE18 |
| Combat/loot | NOT IN SCOPE | WAVE_INTEGRATION_17 scope |
| Scene YAML edits | HUMAN_ACTION_REQUIRED | Per unity-yaml-editing-policy.md |

---

## State of Repository — Key Systems

| System | Status | Notes |
|--------|--------|-------|
| CaveRunManager | SCENE_WIRED | In CaveScene YAML (confirmed) |
| CaveLevelRuntimeController | SCENE_WIRED | In CaveScene YAML (confirmed) |
| CaveEntryController | CODE_EXISTS_NOT_SCENE_WIRED | Need to add to CaveScene |
| CaveExitPortal | CODE_EXISTS_NOT_SCENE_WIRED | Need to add to CaveScene |
| Zone_CaveEntrance (FarmScene) | SCENE_WIRED_NEEDS_COMPONENT | Exists as empty GO; needs CaveEntranceInteractable |
| SceneTransitionRouter | CODE_READY | WAVE13 BUILD_VALIDATED |
| PlayerSpawnResolver | CODE_READY | WAVE13 BUILD_VALIDATED |
| cave_from_farm spawn | SCENE_WIRED | SpawnPoints > cave_from_farm in CaveScene |
| CaveEntranceInteractable | CODE_READY_NEW | Created by WAVE16 |
| CaveRuntimeBridge | CODE_READY_NEW | Created by WAVE16 |
| CaveRunStartedEvent | CODE_READY_NEW | Created by WAVE16 |
| CaveExitedEvent | CODE_READY_NEW | Created by WAVE16 |

---

## WAVE13/14/15 Baseline

| Wave | Status | Relevant output |
|------|--------|----------------|
| WAVE_INTEGRATION_13 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED | SceneTransitionRouter/SceneId/SceneTransitionGate/PlayerSpawnResolver |
| WAVE_INTEGRATION_14 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED | CraftingStationRuntimeBootstrap pattern (used as WAVE16 pattern) |
| WAVE_INTEGRATION_15 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED | QuestRuntimeBootstrap pattern (used as WAVE16 pattern) |

---

## Design/Direction Compliance Matrix

| Rule | Source | Status |
|------|--------|--------|
| Do not rebuild cave from scratch; preserve CaveRunManager/CaveLevelRuntimeController | CAVE_DESIGN_DIRECTION.md Part A | COMPLIANT — both reused |
| CaveRunSeed must NOT change on ForwardExit/BackExit | ADR-0005 / cave_rules.md | COMPLIANT — CaveEntranceInteractable does not touch CaveRunSeed |
| Stable run: revisit levels restore from snapshot | cave_rules.md | COMPLIANT — not affected by WAVE16 |
| GameEventBus for gameplay communication | RULES.md event-bus rule | COMPLIANT — CaveRunStartedEvent/CaveExitedEvent via GameEventBus |
| No DontDestroyOnLoad in wrong context | Pattern from WAVE14/15 | COMPLIANT — CaveRuntimeBridge is the only DontDestroyOnLoad singleton |
| No GameObject.Find at runtime | RULES.md no-runtime-global-search | COMPLIANT — CaveRuntimeBridge uses FindFirstObjectByType once on scene transition teardown only |
| No direct scene YAML edits | unity-yaml-editing-policy.md | COMPLIANT — all wiring deferred to human |
| Save DTOs simple types only | ADR-0006 / save-dto rule | COMPLIANT — CaveExitedEvent/CaveRunStartedEvent contain only strings and int |
| IInteractable for all player-interactable objects | scene-interactable-wiring skill | COMPLIANT — CaveEntranceInteractable implements IInteractable |
| SceneTransitionRouter for scene transitions | WAVE13 pattern | COMPLIANT — CaveEntranceInteractable uses SceneTransitionRouter.Execute() |

---

*Created: 2026-06-10 (WAVE_INTEGRATION_16)*
