# WAVE_INTEGRATION_16 — Cave Runtime Bridge Report

## Runtime Strategy Decision

`USE_EXISTING_CAVE_RUNTIME`

The existing cave runtime is mature and fully covers run lifecycle management. WAVE16 creates a thin bridge layer only.

---

## Bridge Architecture

| Component | Role | Pattern |
|-----------|------|---------|
| CaveRuntimeBridge | DontDestroyOnLoad singleton; subscribes to events; publishes CaveExitedEvent; validates cave entry | RuntimeInitializeOnLoadMethod (same as WAVE14/15) |
| CaveEntranceInteractable | IInteractable on FarmScene Zone_CaveEntrance; publishes CaveRunStartedEvent; calls SceneTransitionRouter | IInteractable pattern (same as CaveExitPortal) |
| CaveRunStartedEvent | New event — signals cave run beginning before scene load | readonly struct (same pattern as all cave events) |
| CaveExitedEvent | New event — signals surface return; carries RunSeed + level + return route | readonly struct |

---

## Cave Runtime System Reuse

| Existing System | Role in WAVE16 |
|-----------------|----------------|
| CaveRunManager | StartRun → Awake/InitializeIfNeeded/RestoreCachedStateIfNeeded (bootstrap cache restore); ExitRun → BackExit level 1 → FarmScene |
| CaveRuntimeState | Full run state; no duplication needed |
| CaveLevelRuntimeController | Level generation/restore; start generation called in Start() |
| CaveEntryController | Checkpoint selection when entering cave level |
| CaveExitPortal | BackExit (level 1 → Farm) and ForwardExit (advance to next level) |
| CaveSpawnAnchor (enum) | Entrance/ForwardExit/BackExit — used by CaveLevelRuntimeController |
| SceneTransitionRouter | Execute(request) → loads CaveScene via EditorSceneManager/SceneManager |
| SceneId.SpawnCaveFromFarm | "spawn_cave_from_farm" — target spawn anchor in CaveScene |

---

## Scene Target

| Route | From | To | Spawn |
|-------|------|----|-------|
| Enter cave from Farm | FarmScene | CaveScene | spawn_cave_from_farm |
| Return to Farm from cave | CaveScene (level 1 BackExit) | FarmScene | spawn_farm_from_cave |

---

## Run Seed / State

CaveRunSeed is NOT set by CaveEntranceInteractable (ADR-0005 compliance).
CaveRunManager.Awake() in CaveScene handles:
1. RestoreCachedStateIfNeeded — restores from GameBootstrap cache if returning to cave
2. InitializeIfNeeded — creates new seed if no cached state (first entry)

This preserves the stable-run contract: seed only changes on new game / death / debug command.

---

## Player Spawn

| Item | Status |
|------|--------|
| Spawn anchor in CaveScene | spawn_cave_from_farm (in SpawnPoints GO) |
| PlayerSpawnResolver (WAVE13) | Reads SceneTransitionState.PendingSpawn set by SceneTransitionRouter |
| Fallback | CaveScene Player GO starts at world origin if no spawn resolver is wired |

---

## Camera

| Item | Status |
|------|--------|
| Camera in CaveScene | Main Camera present in scene |
| Camera follow | CaveLevelRuntimeController.RepositionCamera() moves camera to player after generation |
| Camera bounds | CavePlayerPathConfinement handles confinement |

---

## Debts

| Item | Status | Wave |
|------|--------|------|
| CAVE_RUN_SAVE_LOAD_DEBT | CaveRunManager.CaptureSaveData/RestoreFromSaveData exists; not integrated with SaveManager | WAVE18 |
| Combat/loot in cave | Not implemented | WAVE17 |
| TownScene cave entrance | No CaveEntranceInteractable in TownScene | WAVE17+ |
| Checkpoint selection entry flow | CaveEntryController in CaveScene needs human wiring | Human action |
| CaveExitPortal in CaveScene | BackExit/ForwardExit needs human wiring | Human action |
| spawn_farm_from_cave | FarmScene needs SceneSpawnPoint added | Human action |
| Fade transition | No fade system | Future |

---

*Created: 2026-06-10 (WAVE_INTEGRATION_16)*
