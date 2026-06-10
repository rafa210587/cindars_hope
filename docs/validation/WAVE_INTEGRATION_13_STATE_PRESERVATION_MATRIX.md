# WAVE_INTEGRATION_13 — State Preservation Matrix

## Status

DOCUMENTATION — assessed based on architecture audit. Verification requires human Play Mode testing.

## Matrix

| State | Source | Preserved? | Mechanism | Debt / Blocker |
|-------|--------|-----------|-----------|----------------|
| Inventory | InventoryManager (ScriptableObject-backed) | LIKELY_YES | SO data is not destroyed on scene unload | Requires InventoryManager to exist in destination scene or be DontDestroyOnLoad |
| Gold | EconomyManager | LIKELY_YES if DONTDESTROY | RuntimeInitializeOnLoad on EconomyManager unknown; audit required | May reset if EconomyManager is re-created in destination scene |
| Shipping Crates | ShippingPointRuntime | LIKELY_NO | ShippingPoint is scene-local object in FarmScene | No persistence mechanism; crate state lost on scene transition |
| Skills (unlocked) | SkillTreeManager / PlayerProgressionManager | LIKELY_YES | SO-backed progress | Requires manager in destination or DONTDESTROY |
| Active Slots (equipped skills) | SkillTreeManager | LIKELY_YES | SO-backed | Same as above |
| Cooldowns | SkillEffect runtime | LIKELY_NO | Cooldown is in-memory, lost on scene unload | Not persisted |
| Time / Weather | TimeManager / GameTimeManager | LIKELY_YES | TimeManager may use SaveManager; audited in WAVE_02 | Requires TimeManager in destination |
| Player Condition (health, stamina, mana) | PlayerManager / HungerManager / StaminaManager | LIKELY_NO without persist | Scene-local component state | May reset to defaults on destination load unless saved before transition |
| Quest Flags | QuestManager | LIKELY_YES | Quest data SO-backed or SaveManager | Requires QuestManager in destination |
| Current Scene ID | SceneTransitionState.PendingSpawnId | CLEARED after spawn | Static; survives scene load | Intentional; cleared post-spawn |
| Last Spawn Anchor ID | SceneTransitionState.PendingSpawnId | AVAILABLE during load | Read by PlayerSpawnResolver in Start() | Cleared after spawn as designed |

## Critical Debt Items

### PersistentManagers Scene (P0 Debt)

Without a PersistentManagers scene using DontDestroyOnLoad, each scene that lacks its own GameBootstrap will lose all manager state after transition.

**Risk**: HIGH — player health/stamina/cooldowns reset on Farm→Town transition.

**Mitigation**: SaveManager could auto-save before transition and auto-load after spawn.
- Not implemented in WAVE_INTEGRATION_13.
- Defer to WAVE_INTEGRATION_14 or explicit human-authorized spec.

### Gold / Economy State

EconomyManager is serialized in FarmScene's GameBootstrap. If TownScene does not have its own GameBootstrap wired with EconomyManager, gold balance is unavailable in TownScene shop.

**Risk**: MEDIUM — shop may fail or show 0 gold.

### Shipping State

Shipping crates state is scene-local. Items placed in crate before going to Town are not persisted.

**Risk**: LOW for MVP — shipping is FarmScene-only activity.
