# SPEC_19 Phase 0 - Audit Matrix

**Date:** 2026-06-01  
**Status:** PRE-EXECUTION AUDIT  
**Scope:** Save, Inventory, Farm, World systems state assessment before implementation

---

## 1. Save System Audit

### Current State: **IMPLEMENTED AND FUNCTIONAL**

**Schema:**
- Current schema version: V5 (multiple migrations exist: V1→V2, V2→V3, V3→V4, V4→V5)
- Classes implemented: GameSaveData, PlayerSaveData, InventorySaveData, FarmSaveData, WorldSaveData, CaveSaveData, DeathSaveData, etc.
- Migration infrastructure: Complete (ISaveMigration, SaveMigrationRegistry, SaveBackupService)

**What Works:**
- ✓ Save file persistence via SaveManager
- ✓ Save/load round-trip infrastructure
- ✓ Migration registry and sequencing
- ✓ Backup service for save safety
- ✓ Inventory slots persistence (Capacity, Slots array, Items legacy)
- ✓ Farm plot persistence (FarmPlotSaveData with state/seed/water/growth/regrow)
- ✓ Tree persistence (TreeSaveData with HP/stump/regrow)
- ✓ Pickup persistence (ItemPickupSaveData with ID)
- ✓ Hotbar persistence (HotbarSaveData)

**What Is Missing/Partial:**
- Last migration (V4→V5) requires validation of payload changes
- Play Mode validation not yet completed for all data round-trip scenarios

**Files:**
- `Assets/_Game/Scripts/Save/SaveManager.cs` — NOT to be modified unless critical bug found
- `Assets/_Game/Scripts/Save/SaveData.cs` — NOT to be modified without migration explicit
- `Assets/_Game/Scripts/Save/Migrations/*` — All exist, NO new migrations needed for SPEC_19

**Risk:** Modifying SaveManager or schema without migration creates data loss risk.

---

## 2. Inventory System Audit

### Current State: **IMPLEMENTED WITH GAPS**

**Implemented:**
- ✓ InventoryManager with slot-based storage (List<InventorySlot>)
- ✓ Capacity management (18 default, max 30)
- ✓ AddItem, RemoveItem, SplitSlot, DestroySlot operations
- ✓ Items aggregate (Dictionary<string,int>) for backward compat
- ✓ Starter inventory via PlayerDataSO.StartingItems
- ✓ ItemDatabase integration
- ✓ Item persistence in save (InventorySaveData with Slots and Items)
- ✓ InventoryPanelController (IMGUI modal panel, I to open/close)
- ✓ Hotbar binding (hotbar slots point to inventory slots)
- ✓ InventoryChangedEvent publication

**Gaps (Runtime Minimal):**
- ✓ TryUseItem method exists in ItemUseManager (handler-based)
- ✓ DropItem method exists in InventoryManager (uses ItemDropSpawner)
- ? Use handler registration — may need validation
- ? Drop spawner availability — depends on scene setup

**What Is NOT Implemented:**
- UI/UX final (drag/drop, sort, auto-organize) — documented as SPEC_28
- Equipment binding visual UI — documented as future
- Crafting consumption integration — documented as future

**Files:**
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs` — READ ONLY unless critical bug
- `Assets/_Game/Scripts/Inventory/ItemUseManager.cs` — READ ONLY
- `Assets/_Game/Scripts/Inventory/InventoryPanelController.cs` — READ ONLY
- `Assets/_Game/Scripts/Inventory/ItemUseHandler.cs` — Abstract, handlers extend this
- ItemUseHandler implementations — May need validation

**Risk:** Use/Drop without proper handlers or spawner cause silent failures. Need validation.

---

## 3. Farm System Audit

### Current State: **IMPLEMENTED WITH GAPS**

**Implemented:**
- ✓ FarmPlotState enum (Raw, TilledDry, TilledWet, PlantedDry, PlantedWet, ReadyToHarvest, Blocked, Dead)
- ✓ FarmPlot contextual menu (E to open, W/S navigate, E/Enter to confirm)
- ✓ Actions: Arar (till), Molhar (water), Plantar (plant from inventory), Colher (harvest)
- ✓ FarmPlotSaveData with state, seed, progress, water, regrow, day
- ✓ SeedDataSO with RequiresWater, RegrowDays, SeasonTags
- ✓ Growth state machine (advances only in PlantedWet)
- ✓ Water reset on day advance
- ✓ Harvest logic with optional regrow
- ✓ Farm persistence via FarmSaveData
- ✓ Planting UI via contextual menu (IMGUI vertical)

**Gaps (Non-Critical):**
- UI is IMGUI/minimal, not Canvas final — documented as SPEC_28
- Stamina cost deferred to SPEC_09
- No drag/drop or advanced farm management UI

**What Is NOT Implemented:**
- Climate/rain simulation — documented as future
- Fertilizer mechanic — documented as future
- Seasonal growth adjustments — documented as future
- Sprinkler automation — documented as future

**Files:**
- `Assets/_Game/Scripts/Farm/FarmPlot.cs` — READ ONLY
- `Assets/_Game/Scripts/Farm/FarmPlotState.cs` — READ ONLY
- `Assets/_Game/Scripts/Farm/FarmPlotSaveData.cs` — READ ONLY
- `Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs` — READ ONLY (data only)

**Risk:** Modifying FarmPlot logic risks breaking existing growth/harvest behavior. Menu system is working.

---

## 4. World Activities Audit

### Current State: **IMPLEMENTED WITH GAPS**

**Implemented:**
- ✓ TreeNode with HP, tool tier, madeira per hit, bonus 2x+, stump state, regrowth
- ✓ TreeDataSO with HP, tool/tier, wood drops, multiplier, regrowth, loot table hook
- ✓ Tree persistence via TreeSaveData (HP, stump, regrow remaining)
- ✓ FishingSpot requiring rod, casting, timing window, loot table resolution
- ✓ LootTableSO with entries (ItemId, min/max amount, weight, tags)
- ✓ ItemPickupSaveData with persistent ID
- ✓ World tree/pickup persistence via WorldSaveData
- ✓ Interaction/collider setup for fishing spots and tree nodes

**Gaps (Non-Critical):**
- Tree drops go to inventory when spawner unavailable — fallback working
- FarmScene may not have guaranteed fishing spots — needs validation
- Cave fishing integration deferred to avoid snapshot regression — documented
- Dynamic spawner for tree drops — documented as future

**What Is NOT Implemented:**
- Stamina cost per tree hit — documented as SPEC_09
- Durability consumption per hit — documented as future (SPEC_20)
- Bait/isca mechanic — documented as future
- Climate/weather effects — documented as future
- Economy integration (pricing) — documented as future

**Files:**
- `Assets/_Game/Scripts/World/TreeNode.cs` — READ ONLY
- `Assets/_Game/Scripts/World/FishingSpot.cs` — READ ONLY
- `Assets/_Game/Scripts/World/Data/TreeDataSO.cs` — READ ONLY (data only)
- `Assets/_Game/Scripts/Loot/LootTableSO.cs` — READ ONLY (data only)
- `Assets/_Game/Scripts/World/ItemDropSpawner.cs` — Validation may be needed

**Risk:** Tree/fishing behavior is stable. Drop spawner availability is the only functional risk.

---

## 5. Hotbar System Audit

### Current State: **IMPLEMENTED AND FUNCTIONAL**

**Implemented:**
- ✓ HotbarManager with 8 slots
- ✓ Slot-based bindings to inventory items via item ID
- ✓ Hotbar persistence via HotbarSaveData
- ✓ Q/E/Space input binding
- ✓ Hotbar UI display (HUD minimal)
- ✓ Hotbar item equip/use integration

**What Is Missing:**
- Final UI/UX — documented as SPEC_28

**Files:**
- `Assets/_Game/Scripts/UI/Hotbar/**` — READ ONLY
- HotbarSectionProvider for save integration — existing

**Risk:** Hotbar is stable. No changes needed for SPEC_19.

---

## 6. Validators Audit

### Current State: **PARTIAL — VALIDATORS EXIST BUT MAY NEED EXTENSION**

**Existing Validators:**
- `CombatDatabaseValidator` — Validates weapon/spell/status effect databases
- `ProjectilePrefabValidator` — Validates projectile prefab references
- `FarmTownMVPValidator` — Validates Town/Farm scene setup
- `CaveMVPValidator` — Validates Cave scene setup

**New Validators Needed:**
- ✓ Hotbar slot pointing to non-existent item
- ✓ Pickup without persistent ID
- ✓ Farm plot without save ID
- ✓ Tree without save ID
- ✓ Fishing spot without interaction/collider
- ✓ Item database with null/duplicate entry
- ✓ Starter inventory pointing to non-existent item

**Files:**
- `Assets/_Game/Scripts/Editor/Validation/**` — May need extension for new checks

**Risk:** Missing validators could hide data corruption at save load time.

---

## 7. Files Status Summary

### Files NOT to Modify (Risk: Regression)

| File | Reason |
|------|--------|
| `Assets/_Game/Scripts/Save/SaveManager.cs` | Core save/load logic — any change risks data loss |
| `Assets/_Game/Scripts/Save/SaveData.cs` | Save schema — change requires migration |
| `Assets/_Game/Scripts/Inventory/InventoryManager.cs` | Core inventory — change risks inventory loss |
| `Assets/_Game/Scripts/Inventory/ItemUseManager.cs` | Core use logic — change risks use failures |
| `Assets/_Game/Scripts/Inventory/InventoryPanelController.cs` | Working UI — change risks UI breakage |
| `Assets/_Game/Scripts/Farm/FarmPlot.cs` | Core farm logic — change risks growth/harvest |
| `Assets/_Game/Scripts/World/TreeNode.cs` | Core tree logic — change risks HP/drops |
| `Assets/_Game/Scripts/World/FishingSpot.cs` | Core fishing logic — change risks fishing breaks |
| `Assets/_Game/Scenes/FarmScene.unity` | Must edit via Unity Editor only, never YAML |
| `Assets/_Game/Scenes/TownScene.unity` | Must edit via Unity Editor only, never YAML |

### Files That MAY Need Reading (Audit)

| File | Reason |
|------|--------|
| Scene creators (`CreateMvpTownScene.cs`, etc.) | Verify bootstrap wiring post-SPEC_18 |
| `ItemUseHandler` implementations | Verify handlers are registered |
| `ItemDropSpawner.cs` | Verify spawner is available at runtime |
| Item database assets | Verify no null/duplicate entries |
| `FarmScene.unity` | Verify fishing spot count (need 2+) |

---

## 8. Menor Delta Seguro (Safe Minimal Changes)

**Phase 0 Analysis Result:**

All four systems (Save, Inventory, Farm, World) have **working runtime implementation**. The gaps identified are:

1. **Validation gaps** — Missing validators for consistency
2. **Scene setup gaps** — FarmScene may need fishing spot verification
3. **Handler registration gaps** — Use handlers must be wired to items
4. **Play Mode testing gaps** — Round-trip save/load not yet validated in Play Mode

**Safe Minimal Delta for SPEC_19:**

1. ✓ Run validators in Unity Editor to detect real gaps
2. ✓ Verify scene setup (fishing spots, plot IDs, tree IDs exist)
3. ✓ Verify item database consistency (no nulls/dupes)
4. ✓ Verify starter inventory items exist in database
5. ✓ Run Play Mode smoke test: plant, water, harvest, fish, drop/pickup, save/load
6. ✓ Create/extend validators for the 7 consistency checks above
7. ✓ Document any real gaps found and resolve within SPEC_19 scope
8. ✗ DO NOT modify core Save/Inventory/Farm/World runtime code unless critical bug found
9. ✗ DO NOT change save schema
10. ✗ DO NOT rewrite managers

---

## 9. Validações Necessárias

**Automated (PowerShell):**
```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
tools/docs/validate_docs.ps1
```

**Manual (Unity Editor, if available):**
- CindarsHope/Repair and Validate Project
- CindarsHope/Validate/Combat/Validate Projectile Prefabs (regression check)
- CindarsHope/Validate/Combat/Validate Combat Databases (regression check)
- CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP
- CindarsHope/Advanced/Legacy/Validate/Validate Cave MVP

**Play Mode Smoke Test (FarmScene):**
- Move player (WASD)
- Interact with farm plot (E)
- Plant seed
- Water plot
- Harvest
- Interact with tree (E)
- Chop tree
- Go to fishing spot (E)
- Fish (mini timing game)
- Drop item from inventory (I, select, Drop action)
- Pick up item
- Open hotbar (check bindings)
- Save (S)
- Load (test persists inventory/farm/world state)
- Verify Console: no NEW critical errors

**Stop Conditions:**
- Build fails with compile error → STOP
- Validator reports critical data corruption → STOP
- Play Mode crash → STOP
- Save/load loses inventory/farm/world data → STOP
- Hotbar breaks → STOP
- Starter inventory missing items → STOP

---

## 10. Conclusion: Phase 0 Audit Complete

**Status:** All four systems **FUNCTIONAL at runtime level**. Gaps are **validation/UI/future features**, not core logic.

**Next Step:** Phase 1 — Execute automated validations and create execution report with audit findings.

**Decision Path:**
- If validators pass and Play Mode stable → Extend validators for consistency, mark specs 02-05 as MVP complete
- If validators find gaps → Document and fix within SPEC_19 scope
- If Play Mode finds regression → Investigate and fix
- If no real gaps → Create closeout report, unblock SPEC_20
