# SPEC 15 Finalization & SPEC 16 Phase 0 Stabilization

**Start Date:** 2026-05-25  
**Objective:** Fix compilation errors, stabilize modal system, prepare SPEC 16

---

## Errors Found & Fixes Applied

### 1. IInteractable Missing Usings ❌ → 🔄

**Files:**
- `Assets/_Game/Scripts/Locations/AnyaFountainInteractable.cs`
- `Assets/_Game/Scripts/World/CorpseInteractable.cs`

**Issue:** Missing `using CindarsHope.Interaction;`

**Fix Status:** PENDING

---

### 2. IInteractable CanInteract() Not Implemented ❌ → 🔄

**Files:**
- `AnyaFountainInteractable` - needs implementation
- `CorpseInteractable` - needs implementation

**Fix Status:** PENDING

---

### 3. FindObjectOfType Global Search Violations ❌ → 🔄

**Files:**
- `AnyaFountainInteractable.OnEnable()` - Line 39
- `CorpseInteractable.Initialize()` - Line 31

**Fix:** Replace with GameBootstrap injection or serialized field

**Fix Status:** PENDING

---

### 4. ModalBase Missing ❌ → 🔄

**File:** `Assets/_Game/Scripts/UI/Modal/ModalBase.cs` (DOESN'T EXIST)

**Required By:**
- `CorpseRecoveryModal`
- `AnyaFountainMenu`

**Fix Status:** PENDING (NEW FILE NEEDED)

---

### 5. ModalManager Missing OpenModal<T>() ❌ → 🔄

**File:** `Assets/_Game/Scripts/UI/Modal/ModalManager.cs`

**Missing Enum Values:**
- CorpseRecovery
- AnyaFountain
- SkillTree

**Missing Method:** `OpenModal<T>(T prefab) where T : ModalBase`

**Fix Status:** PENDING

---

### 6. CaveRunManager Namespace & API ❌ → 🔄

**File:** `Assets/_Game/Scripts/Cave/Death/CaveDeathResolver.cs`

**Issues:**
- Missing `using CindarsHope.Cave.Runtime;`
- Line 75: `_caveRunManager.State.CurrentRunSeed` (check if correct API)
- Line 77: `_caveRunManager.CurrentLevel` (should be CurrentCaveLevel?)

**Fix Status:** PENDING (NEED TO VERIFY API)

---

### 7. InventoryManager.GetAllItems() Missing ❌ → 🔄

**File:** `CaveDeathResolver.cs` Line 94

**Fix:** Create method in InventoryManager or refactor approach

**Fix Status:** PENDING

---

### 8. EquipmentManager GetAllEquippedItems() & UnequipAll() Missing ❌ → 🔄

**File:** `CaveDeathResolver.cs` (lines TBD)

**Fix:** Create methods in EquipmentManager

**Fix Status:** PENDING

---

### 9. PlayerProgressionManager API Mismatch ❌ → 🔄

**File:** `CaveDeathResolver.cs`

**Issues:**
- Uses `_progressionManager.CurrentLevel` (might be `Level`)
- Uses `_progressionManager.ResetCurrentLevelXp()` (might not exist)

**Fix:** Add aliases or adapt usage

**Fix Status:** PENDING (NEED TO VERIFY)

---

### 10. PlayerXpChangedEvent & PlayerLevelChangedEvent Missing ❌ → 🔄

**File:** `PlayerProgressionManager.cs` publishes but events don't exist

**Fix:** Create `PlayerProgressionEvents.cs`

**Fix Status:** PENDING

---

### 11. Input Blocking When Modal Active ❌ → 🔄

**Issue:** WASD/E/R/T/Y/G should be blocked when modal open

**Files to Check:**
- `PlayerController.ReadMoveInput()` - blocks WASD ✅
- `InteractionSystem.Update()` - blocks E ✅
- `ActiveSkillSlots.Update()` - NEEDS CHECK
- Any R/T/Y/G binding

**Fix Status:** PENDING (NEED TO VERIFY ACTIVE SLOTS)

---

### 12. Tracking Documentation Inconsistencies ❌ → 🔄

**Files:**
- `docs/specs/SPEC_EXECUTION_ORDER.md`
- `docs/IMPLEMENTATION_STATUS.md`
- `PROJECT_LOG.md`

**Issue:** SPEC 15 marked as pending but code is complete

**Fix:** Mark as implemented in code, validation pending

**Fix Status:** PENDING

---

## Build Order for Fixes

1. ✅ **Understand IInteractable contract** (already done)
2. → **Fix IInteractable usings + CanInteract()**
3. → **Verify CaveRunManager API**
4. → **Create ModalBase**
5. → **Expand ModalManager**
6. → **Fix CaveDeathResolver API calls**
7. → **Add missing manager methods (Inventory, Equipment, Progression)**
8. → **Create missing events**
9. → **Fix FindObjectOfType → Injection**
10. → **Verify input blocking**
11. → **Update documentation**
12. → **Run validation scripts**

---

## Status

- Total Issues: 12
- Fixed: 0
- In Progress: 0
- Pending: 12
- Blocked: 0

**Next:** Fix IInteractable implementations

