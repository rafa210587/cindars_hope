# SPEC 16 - Codebase Audit & Dependencies

**Date:** 2026-05-25  
**Spec:** spec_skill_trees_active_slots_respec_anya_runtime.md  
**Status:** Pre-Implementation Audit

---

## Executive Summary

SPEC 16 has **significant existing infrastructure** but with **critical gaps**:

✅ **EXISTS:**
- PlayerProgressionManager + XP/Level system (correctly grants skill points at even levels)
- PlayerProgressionRules with SkillPointIntervalLevels = 2 (correct for SPEC 16)
- SkillActionSO, SkillNodeDataSO, SkillTreeDataSO (partial definitions)
- SkillTreeManager (basic, needs enhancement)
- GameEventBus infrastructure
- Modal stack system
- Save/Load framework (SaveManager, SaveData DTOs)
- AnyaFountain & interaction system (from SPEC 15)

⚠️ **CRITICAL GAPS:**
- PlayerLevelChangedEvent, PlayerXpChangedEvent NOT DEFINED (but referenced in code!)
- Events for skill tree system (SkillPointGranted, SkillNodePurchased, etc.) missing
- SkillNodeDataSO lacks: NodeType, SkillCategory, UnlockedSkillActionId, IsCapstone, RequiredPurchasedNodesInTree
- SkillTreeManager too basic: no passive tracking, no capstone validation, no event publishing
- SkillPassiveModifierSO not found
- SkillTreeRegistrySO not found
- No K-key input binding for skill tree modal
- No SkillTreePanel UI controller/modal
- No SkillRespecService
- No skill action shapes: block, dash, leap, charge, pierce, multishot
- Duplicate SkillActionSO in two namespaces (Skills vs Combat.Skills)

---

## Existing Codebase Inventory

### Progression System ✅ READY

**File:** `Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs`

**Status:** Core progression exists and is CORRECT for SPEC 16

```csharp
// Skill point granting (CORRECT)
public static int CalculateSkillPointsGrantedOnLevelUp(int newLevel)
{
    if (newLevel <= 1) return 0;
    return newLevel % SkillPointIntervalLevels == 0 ? 1 : 0;  // SkillPointIntervalLevels = 2
}
```

**Integration Points:**
- Publishes `PlayerLevelChangedEvent(oldLevel, newLevel, grantedAttributePoints, grantedSkillPoints)` ✅
- Tracks `UnspentSkillPoints` ✅
- Has `CaptureSaveData()` and `RestoreFromSaveData()` ✅

**Issues:**
- Events `PlayerLevelChangedEvent` and `PlayerXpChangedEvent` are published but NOT DEFINED
  - They're referenced in code but the classes don't exist
  - This will cause compile errors until fixed

**Data Structure:**
```csharp
public class PlayerProgressionSaveData
{
    public int Level;
    public int CurrentXp;
    public int XpToNextLevel;
    public int UnspentAttributePoints;
    public int UnspentSkillPoints;
    // 6 attributes...
}
```

### Skill Data Assets ⚠️ PARTIAL

**File:** `Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs`

**Current Definition:**
```csharp
public string Id;
public string NodeName;
public string Description;
public Sprite Icon;
public int SkillPointCost = 1;
public int RequiredLevel = 1;
public string RequiredSkillNodeId;  // Single prerequisite only
public int StrengthBonus;
public int DexterityBonus;
public int IntelligenceBonus;
public int WillpowerBonus;
public int ConstitutionBonus;
public int BreathBonus;
public int DamageBonus;
public int DefenseBonus;
```

**Missing for SPEC 16:**
- `NodeType` (PassiveStat, PassiveModifier, UnlockSkillAction, UpgradeSkillAction, UnlockSpell, Capstone)
- `SkillCategory` (PassiveSkill, EquippableSkill, CapstonePassive, etc.)
- `UnlockedSkillActionId` (for EquippableSkill nodes)
- `LinkedSpellId` (for spell unlock nodes)
- `PassiveModifiers[]` (array of modifier definitions)
- `IsCapstone` (bool)
- `RequiredPurchasedNodesInTree` (int, for capstone validation)
- `TreeId` (reference back to parent tree)

**File:** `Assets/_Game/Scripts/Skills/SkillTreeDataSO.cs`

**Current Definition:**
```csharp
public string Id;
public string TreeName;
public string Description;
public SkillNodeDataSO[] Nodes;
public int MaxActiveSlots = 4;
public bool AllowRespeccing = true;
```

**Missing for SPEC 16:**
- `CapstoneNodeId` (explicit reference to capstone node)
- Per-tree configuration (cost multipliers, name localization, etc.)

**Status:** Needs EXPANSION, not rewrite

### Skill Tree Manager ⚠️ BASIC

**File:** `Assets/_Game/Scripts/Skills/SkillTreeManager.cs`

**Current Capabilities:**
- Track unlocked nodes via HashSet
- Basic prerequisite validation
- Active slot management (limited to List of node IDs)
- Respec clears everything

**Missing for SPEC 16:**
- No distinction between PassiveSkill and EquippableSkill
- No capstone validation (8 nodes minimum + prerequisites)
- No event publishing (SkillNodePurchased, SkillPassiveApplied, etc.)
- No passive modifier application/recalculation
- No derived stats integration
- No passive-specific tracking (passives don't go in active slots)
- Single-tree focus; needs multi-tree support

**Status:** Needs significant ENHANCEMENT

### Skill Action Definition ❌ CONFLICT

**TWO VERSIONS EXIST:**

1. `Assets/_Game/Scripts/Skills/SkillActionSO.cs` (namespace `CindarsHope.Skills`)
```csharp
public string SkillActionId;
public SkillActionType SkillActionType;  // DamageSkill, SelfBuffSkill, LinkedSpellSkill
public float CooldownSeconds = 1f;
public float StaminaCost = 0f;
public float ManaCost = 0f;
public int BaseDamage = 0;
public DamageType DamageType = DamageType.Physical;
public float Range = 5f;
public string LinkedSpellId;
```

2. `Assets/_Game/Scripts/Combat/Skills/SkillActionSO.cs` (namespace `CindarsHope.Combat.Skills`)
```csharp
public string Id;
public string ActionName;
public SkillActionType Type;  // None, Melee, Ranged, Magic, Utility, Heal, Dash, Block
public int BaseDamage;
public int StaminaCost;
public int ManaCost;
public int CooldownMs;
public int RequiredLevel;
public int CastRangeMeters = 5;
public float AreaOfEffectRadius = 0f;
```

**ISSUE:** Naming conflict + different field names + different SkillActionType enums

**Decision Needed:** Which one is canonical for SPEC 16?
- SPEC 12 requirement: SkillActionSO for skills + active slots
- Recommend: Consolidate to single namespace + single definition

**Missing Fields for SPEC 16:**
- `LinkedSpellId` (to wrap spells in skill actions)
- `ChargeTimeSeconds` (for charged shot)
- `AreaShape` (for area skills)
- `ProjectileCount` (for multishot)
- `ProjectileSpreadDegrees` (for multishot spread)
- `LinePierceCount` (for line piercer)
- `BlockDurationSeconds` (for block skill)
- `DashDistance` (for dash skill)
- `LeapDistance` (for leap skill)
- `StatusApplicationRules[]` (for status effects like Bleed)

### Events ❌ CRITICAL MISSING

**Currently Published but NOT DEFINED:**
```
PlayerLevelChangedEvent  ← USED in PlayerProgressionManager.AddXp()
PlayerXpChangedEvent     ← USED in PlayerProgressionManager.AddXp()
```

**Must Create for SPEC 16:**
```
SkillPointGrantedEvent
SkillNodePurchaseRequestedEvent
SkillNodePurchasedEvent
SkillPurchaseFailedEvent
SkillPassiveAppliedEvent
SkillPassiveRemovedEvent
ActiveSkillSlotAssignRequestedEvent
ActiveSkillSlotAssignedEvent
ActiveSkillSlotClearedEvent
SkillTreeOpenedEvent
SkillTreeClosedEvent
SkillTreeRespecRequestedEvent
SkillTreeRespecCompletedEvent
SkillTreeRespecFailedEvent
SkillDerivedStatsChangedEvent
```

### UI / Input ⚠️ INCOMPLETE

**Modal Stack:** Exists (GameBootstrap.ModalManager) ✅

**K-Key Binding:** NOT IMPLEMENTED
- PlayerController reads: W/A/S/D, E (interaction), Q/E (hands)
- No K-key listener
- No SkillTreePanel input handler

**SkillTreePanel Modal:** NOT CREATED
- Must respect modal stack
- Must pause gameplay (hook into TimeManager/GameTimeManager)
- Must support navigation: W/A/S/D, Q/E (tree tabs), R/T/Y/G (slot selection)
- Must show: skill points, nodes, prerequisites, active slots

**UI Controllers:** None exist yet
- SkillTreePanelController
- SkillTreeTabController (for tabs)
- SkillNodeDisplayController
- ActiveSkillSlotUIController

### Anya's Fountain Integration ✅ READY

**From SPEC 15:**
- `AnyaFountain` component exists
- `AnyaFountainInteractable` exists
- `AnyaFountainUIController` exists
- `AnyaFountainMenu` modal stub exists

**Ready to integrate:**
- AnyaSkillRespecOption added to menu
- SkillRespecService called on selection

### Save/Load ⚠️ PARTIALLY READY

**SaveData Structure Exists:** `Assets/_Game/Scripts/Save/SaveData.cs`

**What's integrated:**
- SaveManager has Save/Load framework ✅
- SaveData contains all game state
- DTOs use simple types (no Unity refs) ✅

**What's missing:**
- `SkillTreeSaveData` class (exists in SkillTreeDataSO.cs but not hooked to SaveData)
- SaveData.SkillTreeSaveData field (not added)
- SaveManager.CaptureSkillTreeSaveData() method (not created)
- SaveManager.RestoreSkillTreeSaveData() method (not created)
- Migration logic for future updates

**Current SkillTreeSaveData Definition:**
```csharp
public class SkillTreeSaveData
{
    public string TreeId;
    public string[] UnlockedNodeIds;
    public string[] ActiveSlotNodeIds;
    public int TotalSpentPoints;
}
```

**Missing for SPEC 16:**
- `PurchasedNodeIds[]` (equivalent to UnlockedNodeIds, but clearer)
- `RespecCount` (track respec usage)
- `ActiveSkillSlotSaveData[]` with SlotIndex + SkillActionId

---

## Dependencies Matrix

### Blocking This Spec
- None. SPEC 15 is complete.

### This Spec Blocks
- SPEC 17 (UI/UX Polish)
- Any future specs depending on skill system

### Soft Dependencies (Must Not Break)
- SPEC 12: Active slots R/T/Y/G, SkillActionSO, skill execution
- SPEC 15: AnyaFountain, death/corpse, respawn
- SPEC 14: Cave runtime (no direct dependency)
- SPEC 13: Enemy AI (for bestiary integration)
- SPEC 11: Damage/status system (for skill effects)
- SPEC 10: Equipment/derived stats (for passive modifiers)
- SPEC 09: Hunger/stamina/time (for resource costs)
- SPEC 03: Inventory (for skill items/recipes future)

---

## Risk Assessment

### High Risk
1. **Duplicate SkillActionSO**: Two versions in different namespaces
   - Will cause compile errors if both referenced
   - Must consolidate before implementation starts
   - Impact: BLOCKS implementation

2. **Missing Events**: PlayerLevelChangedEvent, PlayerXpChangedEvent not defined
   - Referenced in PlayerProgressionManager.cs
   - Will fail at runtime if not defined
   - Impact: Breaks progression system

3. **SkillTreeManager Too Basic**: Insufficient for SPEC 16 complexity
   - No passive vs equippable distinction
   - No multi-tree support
   - No event publishing
   - Need: Rewrite or significant enhancement
   - Impact: Scope

### Medium Risk
1. **Modal Stack Integration**: K-key and SkillTreePanel need careful integration
   - Must respect existing modal precedence
   - Must not break Q/E input outside modal
   - Mitigation: Follow existing modal patterns (look at InventoryPanel)

2. **Derived Stats Recalculation**: Passive modifiers must integrate with existing systems
   - Need to understand current derived stats pipeline
   - Mitigation: Check SPEC 10 implementation

3. **Respec Isolation**: Must not affect level, XP, inventory, equipment
   - Only affect: purchased nodes, passives, active slots
   - Mitigation: Clear service contract + unit-testable logic

### Low Risk
1. **Skill Shapes**: Action shapes (dash, leap, block, etc.) can fallback
   - MVP allows basic implementations
   - Mitigation: Documentation of fallback behavior

---

## Compilation Status

**Current:** Will NOT compile
- Reason: PlayerLevelChangedEvent, PlayerXpChangedEvent undefined

**Must Fix Before Implementation:**
1. Create PlayerLevelChangedEvent, PlayerXpChangedEvent
2. Consolidate SkillActionSO (pick one, reference it universally)
3. Extend SkillNodeDataSO with SPEC 16 fields

---

## Files Needing Review Before Plan

1. SaveData.cs (check current fields)
2. SaveManager.cs (check capture/restore pattern)
3. ModalManager.cs (understand modal stack)
4. TimeManager.cs (for pause on K-press)
5. PlayerProgressionSaveData.cs (skill point fields)
6. GameEventBus.cs (event publishing pattern)

---

## Summary Table

| System | Exists | Complete | Gaps | Risk |
|--------|--------|----------|------|------|
| Progression/XP/Level | ✅ | ~90% | Missing event classes | HIGH |
| SkillActionSO | ✅ | 50% | Duplicate, missing fields | HIGH |
| SkillNodeDataSO | ✅ | 40% | Missing 8+ fields | MEDIUM |
| SkillTreeDataSO | ✅ | 60% | Missing capstone ref | LOW |
| SkillTreeManager | ✅ | 20% | Too basic | MEDIUM |
| Events | ❌ | 0% | 15+ events needed | HIGH |
| Save/Load | ✅ | 70% | Skill data not hooked | LOW |
| UI/Modal | ⚠️ | 30% | Panel + input missing | MEDIUM |
| Anya Integration | ✅ | 80% | Respec option missing | LOW |

---

## Next Steps

1. ✅ Complete this audit (DONE)
2. → Create implementation plan with safe build order
3. → Fix critical issues (events, SkillActionSO consolidation)
4. → Implement in safe dependency order
5. → Test anti-regression
6. → Update tracking files

**Status:** Ready for detailed planning

---

**Audit Date:** 2026-05-25  
**Audit by:** Claude Code Agent  
**Confidence Level:** HIGH (based on codebase exploration)
