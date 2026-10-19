# WAVE_INTEGRATION_17 — Extraction State Preservation Matrix

**Date:** 2026-06-10

---

## State Preservation After Cave Run

| State | DontDestroyOnLoad | On Cave Entry | In Cave | On Cave Exit | After FarmScene Load |
|-------|------------------|--------------|---------|-------------|---------------------|
| Inventory contents | YES (InventoryManager) | PRESERVED | PRESERVED | PRESERVED | PRESERVED |
| Gold | YES (EconomyManager) | PRESERVED | PRESERVED | PRESERVED | PRESERVED |
| Player HP | YES (PlayerManager) | PRESERVED | MODIFIED by enemies | PRESERVED | PRESERVED |
| Player Stamina | YES (StaminaManager) | PRESERVED | MODIFIED by combat | PRESERVED | PRESERVED |
| Quest state | YES (QuestService) | PRESERVED | PRESERVED | PRESERVED | PRESERVED |
| Active skill slots | YES (SkillTreeManager) | PRESERVED | PRESERVED | PRESERVED | PRESERVED |
| Equipment | YES (EquipmentManager) | PRESERVED | PRESERVED | PRESERVED | PRESERVED |
| Cave run state | YES (GameBootstrap.SetCachedCaveRunState) | NEW RUN | TRACKED | CLEARED on exit | N/A |
| Loot collected in cave | In InventoryManager | N/A | ADDED during run | PRESERVED | PRESERVED |

---

## Drop Preservation Chain

1. Enemy defeated in cave → `EnemyKilledEvent`
2. `EnemyDropSpawner.OnEnemyKilled` → `InventoryManager.AddItem`
3. Item now in `InventoryManager._items` dict (in-memory)
4. `GameBootstrap` survives scene transitions via `DontDestroyOnLoad`
5. On FarmScene load: `InventoryManager` is same instance → item still present
6. On Save: `SaveManager.SaveGame()` → inventory written to save file

---

## Risk: EnemyDropSpawner Scene Dependency

`EnemyDropSpawner` is a scene-placed MonoBehaviour. It subscribes to `EnemyKilledEvent` in `OnEnable` and unsubscribes in `OnDisable`.

**Risk:** If `EnemyDropSpawner` is in FarmScene (or another scene that unloads during cave run), its subscription will be destroyed and drops will not trigger.

**Mitigation (WAVE17 scope):** Human must place an `EnemyDropSpawner` in the CaveScene (or whatever test scene is used). Alternatively, `EnemyDropSpawner` can be placed on a `DontDestroyOnLoad` object — but this requires scene wiring that is outside WAVE17 code scope.

**Residual risk:** `EnemyDropSpawner` scene placement is required by human before smoke test.

---

## Not Covered by WAVE17

- Save/load of loot collected during cave run: SAVE_LOAD_DEBT (same as WAVE16)
- Cave run state snapshot: SNAPSHOT_DEBT (same as WAVE16)
- Item loss if inventory full: PRE_EXISTING_BEHAVIOR (not changed by WAVE17)
