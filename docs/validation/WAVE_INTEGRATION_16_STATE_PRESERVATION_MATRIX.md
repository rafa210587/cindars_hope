# WAVE_INTEGRATION_16 — State Preservation Matrix

## Status

Analysis based on code audit (2026-06-10). Play Mode validation not yet run.

---

## State Preservation Table

| State | Must Preserve? | Source System | How Preserved | Result |
|-------|---------------|---------------|---------------|--------|
| Player Inventory | YES | InventoryManager (DontDestroyOnLoad via GameBootstrap) | GameBootstrap persists across scenes | LIKELY_YES |
| Player Gold | YES | EconomyManager / InventoryManager (DontDestroyOnLoad) | GameBootstrap persists | LIKELY_YES |
| Quest state (active/progress) | YES | QuestRuntimeBootstrap (DontDestroyOnLoad, WAVE15) | QuestService holds in-memory state | LIKELY_YES (QUEST_SAVE_LOAD_IN_MEMORY per WAVE15) |
| Player health / HP | YES | PlayerManager / StaminaManager (DontDestroyOnLoad) | GameBootstrap persists | LIKELY_YES |
| Player stamina | YES | StaminaManager (DontDestroyOnLoad) | GameBootstrap persists | LIKELY_YES |
| Active skill slots | YES | SkillTreeManager (DontDestroyOnLoad) | GameBootstrap persists | LIKELY_YES |
| Player position | RESET | CaveScene / FarmScene | PlayerSpawnResolver sets position on load | CORRECT_BEHAVIOR |
| Cave run seed | YES (on re-entry) | CaveRunManager | GameBootstrap.SetCachedCaveRunState() called on exit; restored on re-enter | LIKELY_YES |
| Cave run snapshots | YES (within run) | CaveRuntimeState.VisitedLevelSnapshots | Cached in GameBootstrap; restored in CaveRunManager.Awake() | LIKELY_YES |
| Cave depleted nodes | YES | CaveRuntimeState.DepletedNodeIds | Same GameBootstrap cache path | LIKELY_YES |
| Cave boss defeat states | YES | CaveRuntimeState.BossDefeatStates | Same GameBootstrap cache path | LIKELY_YES |
| Cave current level | YES | CaveRuntimeState.CurrentCaveLevel | GameBootstrap cache | LIKELY_YES |
| Day/time | YES | TimeManager / GameTimeManager (DontDestroyOnLoad) | GameBootstrap persists | LIKELY_YES |
| Weather | YES | Managed by TimeManager subsystems | GameBootstrap persists | LIKELY_YES |
| Equipment | YES | EquipmentManager (DontDestroyOnLoad) | GameBootstrap persists | LIKELY_YES |
| Player XP / progression | YES | PlayerProgressionManager (DontDestroyOnLoad) | GameBootstrap persists | LIKELY_YES |
| NPC shop stock | YES | ShopManager (DontDestroyOnLoad) | GameBootstrap persists | LIKELY_YES |
| CaveRunSeed (save to disk) | DEBT | SaveManager integration | CAVE_RUN_SAVE_LOAD_DEBT — not integrated with SaveManager yet | DEBT_WAVE18 |

---

## Key Finding

The project uses a DontDestroyOnLoad `GameBootstrap` that holds all managers. Scene transitions destroy scene-specific objects (Player, Camera, spawned enemies) but **managers are preserved**. Therefore inventory/gold/quest/health/stamina/skills/time are all LIKELY preserved correctly.

The `CaveRunManager` uses a special mechanism: it caches its runtime state into `GameBootstrap._cachedCaveRunState` via `SetCachedCaveRunState()` before leaving CaveScene (on `SceneTransitionStartedEvent`). On re-entry, `CaveRunManager.Awake()` calls `RestoreCachedStateIfNeeded()` to restore from the cache. This ensures stable run continuity even after scene reload.

---

## Debt Items

| Debt | Impact | Mitigation |
|------|--------|-----------|
| CAVE_RUN_SAVE_LOAD_DEBT | If the game is closed while in cave, cave state is lost | CaveRunManager.CaptureSaveData/RestoreFromSaveData exists; needs SaveManager section integration (WAVE18) |
| QUEST_SAVE_LOAD_IN_MEMORY | Quest state is in-memory only (per WAVE15) | Same debt; WAVE18 planned |
| spawn_farm_from_cave anchor | Player may spawn at wrong position in FarmScene after cave exit | Human must add SceneSpawnPoint "spawn_farm_from_cave" to FarmScene |

---

*Created: 2026-06-10 (WAVE_INTEGRATION_16)*
