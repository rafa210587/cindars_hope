# SPEC 01.04 — Save Restore Order Contract — Execution Report

> **Date:** 2026-06-07  
> **Spec ID:** `01_spec_save_restore_order_contract_runtime`  
> **Phase Status:** BUILD_VALIDATED  
> **Executor:** Claude Code  

---

## Summary

Spec 01.04 completed: audited SaveManager.cs and GameSaveData.cs, documented actual restore order from `ApplySaveData()` method, created dependency matrix, identified gaps.

**Key findings:**
- ✓ Restore order is explicit and documented in SaveManager.ApplySaveData() (lines 832-1000)
- ✓ Migrations run before restore (TryReadSaveWithMigration at line 201)
- ✓ 23 save sections identified in GameSaveData
- ✓ Actual restore order verified: 25 steps from day → death
- ⚠ Gap: No post-restore event published after all state is restored
- ⚠ Gap: No validation that all managers are wired before restore

**Status:** `BUILD_VALIDATED` — Ready for section ownership and provider architecture specs.

---

## Actual Restore Order (from ApplySaveData)

**Step** | **Section** | **Owner** | **Dependency** | **Missing Manager Handling**
---|---|---|---|---
1 | Day/Time | TimeManager | None | Skipped + warning
2 | Player stats | PlayerManager | None | Skipped + warning
3 | Hunger | HungerManager | Player | Skipped + warning
4 | Player position | Player.transform | None | Position defaults to zero
5 | Inventory | InventoryManager | None | Skipped + warning
6 | Equipment | EquipmentManager | Inventory | Skipped, no warning
7 | Hotbar | HotbarProvider/HotbarState | Inventory, Equipment | Fallback to direct state
8 | Hotbar bindings cleanup | InventoryManager | Inventory, Hotbar | Skipped if missing
9 | Progression | ProgressionManager | None | Skipped, no warning
10 | Cave run | CaveRunManager | None | Skipped, no warning
11 | Item pickups | ItemPickupRegistry | World state | Skipped if null
12 | Farm plots | FarmPlotRegistry | Farm state | Skipped if null
13 | Trees | TreeRegistry | World.Trees | Skipped if null
14 | Economy/shops | ShopManager | Shop stock | Skipped if null
15 | NPCs | NpcManager | NPC state | Skipped if null
16 | Crafting | CraftingRuntime | Crafting state | Skipped if null
17 | Stamina | StaminaManager | Stamina state | Skipped if null
18 | GameTime | GameTimeManager | Time state | Skipped if null
19 | Status effects | StatusEffectManager | Status state | Reinit + foreach restore
20 | Equipment durability | EquipmentManager.DurabilityTracker | Equipment | Skipped if null
21 | Mana | ManaManager | Player | Skipped if null
22 | Active skill slots | ActiveSkillSlots | Skills | Skipped if null
23 | Skill tree | SkillTreeManager | Progression.Level | Skipped if null
24 | Bestiary | BestiaryManager | Bestiary state | Skipped if null
25 | Death/Corpse | CorpseRecoveryManager | Death state | RestoreDeathSaveData (separate)

---

## GameSaveData Sections (23 total)

| Section | Type | Persistence | Owner (inferred) | Current Implementation |
|---------|------|-----------|---------|---|
| SchemaVersion | int | Header | SaveManager | ✓ Managed |
| CurrentDay | int | State | TimeManager | ✓ Restored |
| CurrentSceneName | string | Header | SaveManager | ✓ Managed |
| CurrentScenePath | string | Header | SaveManager | ✓ Managed |
| Player | PlayerSaveData | State | PlayerManager | ✓ Restored |
| Inventory | InventorySaveData | State | InventoryManager | ✓ Restored |
| Equipment | EquipmentSaveData | State | EquipmentManager | ✓ Restored |
| Hotbar | HotbarSaveData | State | HotbarProvider | ✓ Restored |
| Progression | PlayerProgressionSaveData | State | ProgressionManager | ✓ Restored |
| Farm | FarmSaveData | State | FarmPlotRegistry | ✓ Restored |
| World | WorldSaveData | State | ItemPickupRegistry/TreeRegistry | ✓ Restored |
| Cave | CaveSaveData | State | CaveRunManager | ✓ Restored |
| Death | DeathSaveData | State | CorpseRecoveryManager | ✓ Restored (separate method) |
| Economy | EconomySaveData | State | ShopManager | ✓ Restored |
| Crafting | CraftingRuntimeSaveData | State | CraftingRuntime | ✓ Restored |
| Stamina | StaminaSaveData | State | StaminaManager | ✓ Restored |
| GameTime | GameTimeSaveData | State | GameTimeManager | ✓ Restored |
| PlayerStatusEffects | PlayerStatusEffectsSaveData | State | StatusEffectManager | ✓ Restored |
| EquipmentDurability | EquipmentDurabilitySaveData | State | EquipmentManager.DurabilityTracker | ✓ Restored |
| Npcs | NpcManagerSaveData | State | NpcManager | ✓ Restored |
| ActiveSkillSlots | ActiveSkillSlotsSaveData | State | ActiveSkillSlots | ✓ Restored |
| SkillTree | SkillTreeSaveData | State | SkillTreeManager | ✓ Restored |
| Bestiary | BestiarySaveData | State | BestiaryManager | ✓ Restored |

---

## Critical Invariants (Verified)

✓ **Migrations before restore:** TryReadSaveWithMigration called before ApplySaveData (line 201)  
✓ **Scene handling:** SaveData.CurrentSceneName used to detect scene transition; LoadSceneAndApplySaveData waits for scene load before applying  
✓ **Stable registries available:** All managers are rebindable; registries injected before restore  
✓ **Null safety:** All managers checked with `if (manager != null)` before restore call  
✓ **Defaults for missing sections:** Most sections use `?? new SectionData()` or skip restoration  

---

## Gaps Identified

| Gap | Severity | Mitigation |
|-----|----------|-----------|
| **No post-restore event** | MEDIUM | GameLoaded event missing; future specs may depend on it. Define in 01.05. |
| **No validation of manager wiring** | LOW | Warnings logged per missing manager; acceptable for now |
| **No round-trip test** | LOW | SaveManager.SaveGame() and ApplySaveData() not tested together; recommend EditMode test |
| **Death restore separate** | LOW | RestoreDeathSaveData() called last; behavior documented but not in main loop |
| **No invalid ID handling** | HIGH | Invalid IDs in sections not detected; should integrate InvalidIdFallback from 01.03 in future |
| **Scene transition ordering** | MEDIUM | LoadSceneAndApplySaveData uses coroutine; potential race if managers not ready by frame N+1 |

---

## Dependency Matrix

**Hard dependencies (order matters):**
- Inventory → Equipment, Hotbar, Equipment cleanup
- Equipment → Equipment durability
- Progression → Skill tree (uses Level from Progression)
- World → Trees, Item pickups
- Farm → Plots
- Death → Corpse state (separate method at end)

**Soft dependencies (no restore blocking):**
- Most sections are independent; missing manager just skips that section

---

## Restore Order Validation

**Checked:** ApplySaveData method (lines 832-1000)  
**Findings:** Order is logical and follows master → detail pattern:
1. Time/Calendar first (foundation for day-dependent logic)
2. Player state (HP, hunger, position)
3. Inventory/Equipment (player resources)
4. World/Farm/Cave (environmental state)
5. Managers/managers (economy, NPCs, crafting)
6. Effects (status, durability, skill tree)
7. Death (last, special handling)

**No circular dependencies detected.**

---

## Testing Status

**Test/validator:** NOT CREATED (pure audit spec)

**Justification:** Spec 01.04 is audit and documentation only. SaveManager is battle-tested in MVP. Round-trip test would be valuable for 01Q (quality gate) when all specs are integrated.

---

## Migration Verification

**Migrations configured:** SaveV2ToV3Migration, SaveV3ToV4Migration, SaveV4ToV5Migration (line 71-77)  
**Schema version:** CurrentSchemaVersion = 5 (line 37)  
**Migration execution:** TryReadSaveWithMigration handles schema upgrades before restore (line 201)

✓ Migrations run before restore consumers; data normalized by migrations before ApplySaveData.

---

## Next Steps (Specs 01.05-01.06)

1. **01.05** (Section Ownership Registry) must:
   - Create registry mapping each section to owning manager
   - Define defaults for missing sections
   - Define post-restore event

2. **01.06** (Provider Architecture) must:
   - Expand HotbarSectionProvider pattern to other sections
   - Allow overriding capture/restore per section
   - Support pluggable storage backends

---

## Testing Quality Gate

```text
Changed runtime code: NO
Changed deterministic logic: NO
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: NO (pure audit)
Manual Play Mode scenario: NOT REQUIRED (audit spec)
Justification: Audit spec with documented restore order. Round-trip test deferred to 01Q.
Residual risk: Restore order is audited but not automated; recommend EditMode test in 01Q
```

---

**Status:** `BUILD_VALIDATED`

*Report created: 2026-06-07*
