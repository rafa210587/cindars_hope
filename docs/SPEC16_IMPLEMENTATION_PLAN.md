# SPEC 16 - Implementation Plan

**Date:** 2026-05-25  
**Spec:** spec_skill_trees_active_slots_respec_anya_runtime.md  
**Audit Reference:** docs/SPEC16_CODEBASE_AUDIT.md  
**Status:** Detailed Plan (Ready for Execution)

---

## Quick Summary

**Goal:** Implement skill trees (55 nodes across 5 trees), skill points (grants at even levels), active slots (R/T/Y/G for equippable skills), passive skills (automatic bonuses), respec at Anya's Fountain, K-key modal, and full save/load.

**Deliverables:**
- 5 skill trees with 55 nodes + data assets
- Enhanced SkillNodeDataSO, SkillTreeManager
- 15+ events for skill system
- SkillTreePanel modal with K-key binding
- SkillRespecService for Anya integration
- 6 skill action shapes: block, dash, leap, charge, pierce, multishot
- Complete save/load integration
- Derived stats integration

**Scope:** ~8,000-12,000 lines of code (includes data assets + systems)

**Estimated Effort:** 8-12 hours for full implementation + testing

---

## Phase 0: Compilation Fix (1-2 hours)

**Must do first** - code doesn't compile without these.

### Phase 0.1: Create Missing Events (required for compilation)

**File to Create:** `Assets/_Game/Scripts/Core/Events/PlayerProgressionEvents.cs`

```csharp
namespace CindarsHope.Core.Events
{
    public class PlayerXpChangedEvent
    {
        public int XpGained { get; }
        public int TotalCurrentXp { get; }
        public int XpToNextLevel { get; }
        public int CurrentLevel { get; }
        
        public PlayerXpChangedEvent(int xpGained, int totalXp, int xpToNext, int level)
        {
            XpGained = xpGained;
            TotalCurrentXp = totalXp;
            XpToNextLevel = xpToNext;
            CurrentLevel = level;
        }
    }

    public class PlayerLevelChangedEvent
    {
        public int OldLevel { get; }
        public int NewLevel { get; }
        public int AttributePointsGranted { get; }
        public int SkillPointsGranted { get; }
        
        public PlayerLevelChangedEvent(int oldLevel, int newLevel, int attrPts, int skillPts)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
            AttributePointsGranted = attrPts;
            SkillPointsGranted = skillPts;
        }
    }
}
```

**Checklist:**
- [ ] Create PlayerProgressionEvents.cs
- [ ] Add using to PlayerProgressionManager.cs (if not auto)
- [ ] Verify compile

### Phase 0.2: Consolidate SkillActionSO

**Problem:** Two SkillActionSO classes in different namespaces

**Decision:** Use `Assets/_Game/Scripts/Skills/SkillActionSO.cs` as canonical
- SPEC 12 uses Skills namespace
- Combat.Skills version is newer but less aligned

**Actions:**
- [ ] Expand `Assets/_Game/Scripts/Skills/SkillActionSO.cs` with SPEC 16 fields
- [ ] Delete `Assets/_Game/Scripts/Combat/Skills/SkillActionSO.cs`
- [ ] Update any imports in Combat namespace to use Skills.SkillActionSO
- [ ] Verify no compile errors

**Fields to Add to SkillActionSO:**
```csharp
// New fields for SPEC 16
public float ChargeTimeSeconds = 0f;  // For charged shot
public int ProjectileCount = 1;  // For multishot
public float ProjectileSpreadDegrees = 0f;  // For spread
public int LinePierceCount = 0;  // For line piercer
public float BlockDurationSeconds = 0f;  // For block skill
public float DashDistance = 0f;  // For dash
public float LeapDistance = 0f;  // For leap
public enum AreaShape { None, Circle, Line, Cone, Rectangle }
public AreaShape AreaShape = AreaShape.None;
public float AreaRadius = 0f;
public string[] StatusApplicationRuleIds = new string[0];  // For status effects
```

---

## Phase 1: Data Structures (2-3 hours)

**Goal:** Expand and create data structures for skill system

### Phase 1.1: Enhance SkillNodeDataSO

**File:** `Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs`

**Add Enums:**
```csharp
public enum SkillNodeType
{
    PassiveStat,
    PassiveModifier,
    UnlockSkillAction,
    UpgradeSkillAction,
    UnlockSpell,
    Capstone
}

public enum SkillCategory
{
    PassiveSkill,
    EquippableSkill,
    CapstonePassive
}
```

**Add Fields:**
```csharp
public string TreeId;  // Parent tree ID
public SkillNodeType NodeType = SkillNodeType.PassiveStat;
public SkillCategory SkillCategory = SkillCategory.PassiveSkill;
public bool IsCapstone = false;

// For prerequisite validation
public string[] PrerequisiteNodeIds = new string[0];  // Multiple prerequisites
public int RequiredPurchasedNodesInTree = 0;  // For capstone (min 8)

// For equippable skills
public string UnlockedSkillActionId;  // SkillActionSO ID to unlock

// For spell unlocks
public string LinkedSpellId;  // SpellDataSO ID

// For passives
public string[] PassiveModifierIds = new string[0];  // Array of SkillPassiveModifierSO IDs
```

**Deprecate/Remove:**
- Single `RequiredSkillNodeId` (replace with `PrerequisiteNodeIds[]`)

**Actions:**
- [ ] Add enums to SkillNodeDataSO
- [ ] Add new fields
- [ ] Update OnValidate() for new fields
- [ ] Verify existing nodes don't break (they won't - old field still works)

### Phase 1.2: Enhance SkillTreeDataSO

**File:** `Assets/_Game/Scripts/Skills/SkillTreeDataSO.cs`

**Add Fields:**
```csharp
public string CapstoneNodeId;  // Explicit reference
public bool AllowPartialRespec = false;  // MVP: only full respec
public int RespecCostGoldAfterFirst = 250;  // Gold cost for respec 2+
```

**Actions:**
- [ ] Add CapstoneNodeId field
- [ ] Add respec configuration fields

### Phase 1.3: Create SkillPassiveModifierSO

**New File:** `Assets/_Game/Scripts/Skills/SkillPassiveModifierSO.cs`

```csharp
[CreateAssetMenu(fileName = "SkillPassiveModifier_", menuName = "CindarsHope/Skills/Passive Modifier")]
public class SkillPassiveModifierSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string ModifierName;
    [TextArea] public string Description;
    
    public enum ModifierType
    {
        StatFlat,
        StatPercent,
        ResistanceFlat,
        DamageFlat,
        DamagePercent,
        CostReduction,
        CooldownReduction,
        DurationBonus,
        SpeedBonus
    }
    
    public ModifierType Type = ModifierType.StatFlat;
    public string TargetStat;  // "MaxHP", "MaxStamina", "Attack", etc.
    public float Value = 1f;
    
    string IIdentifiedData.Id => Id;
}
```

**Actions:**
- [ ] Create SkillPassiveModifierSO.cs
- [ ] Implement all modifier types

### Phase 1.4: Create SkillTreeRegistrySO

**New File:** `Assets/_Game/Scripts/Skills/SkillTreeRegistrySO.cs`

```csharp
[CreateAssetMenu(fileName = "SkillTreeRegistry", menuName = "CindarsHope/Skills/Skill Tree Registry")]
public class SkillTreeRegistrySO : ScriptableObject, IIdentifiedData
{
    public SkillTreeDataSO[] SkillTrees = new SkillTreeDataSO[0];
    
    public SkillTreeDataSO GetTree(string treeId)
    {
        foreach (var tree in SkillTrees)
        {
            if (tree.Id == treeId) return tree;
        }
        return null;
    }
    
    public SkillNodeDataSO FindNode(string nodeId)
    {
        foreach (var tree in SkillTrees)
        {
            foreach (var node in tree.Nodes)
            {
                if (node.Id == nodeId) return node;
            }
        }
        return null;
    }
    
    string IIdentifiedData.Id => "SkillTreeRegistry";
}
```

**Actions:**
- [ ] Create SkillTreeRegistrySO.cs
- [ ] Add to GameBootstrap as serialized field

### Phase 1.5: Create SkillTreeSaveData (Proper)

**File:** `Assets/_Game/Scripts/Save/SkillTreeSaveData.cs`

```csharp
[System.Serializable]
public class SkillTreeSaveData
{
    public string[] PurchasedNodeIds = new string[0];
    public ActiveSkillSlotSaveData[] ActiveSlots = new ActiveSkillSlotSaveData[0];
    public int RespecCount = 0;
    public int LastRespecLevelWhenFree = 0;  // Track if free respec was used
}

[System.Serializable]
public class ActiveSkillSlotSaveData
{
    public int SlotIndex;  // 0=R, 1=T, 2=Y, 3=G
    public string KeyName;  // "R", "T", "Y", "G"
    public string SkillActionId;  // null if empty
}
```

**Actions:**
- [ ] Create SkillTreeSaveData.cs in Save folder
- [ ] Remove duplicate from SkillTreeDataSO.cs
- [ ] Add SkillTreeSaveData field to SaveData.cs

---

## Phase 2: Core Services (3-4 hours)

**Goal:** Implement skill purchase, passive application, respec logic

### Phase 2.1: Create Skill Purchase Service

**New File:** `Assets/_Game/Scripts/Skills/SkillPurchaseService.cs`

**Responsibility:**
- Validate purchase (cost, prerequisites, level, tree nodes)
- Apply purchase (mark node as purchased)
- Apply passives immediately for PassiveSkill nodes
- Unlock SkillAction for EquippableSkill nodes
- Publish events
- Calculate derived stats changes

**Key Methods:**
```csharp
public bool TryPurchaseNode(
    SkillNodeDataSO node,
    int playerLevel,
    int availableSkillPoints,
    HashSet<string> purchasedNodeIds,
    out string failureReason)

public void ApplyPassiveModifiers(SkillNodeDataSO node, PlayerProgressionManager progression)

private bool ValidatePrerequisites(SkillNodeDataSO node, HashSet<string> purchased)
private bool ValidateCapstoneRequirements(SkillNodeDataSO node, HashSet<string> purchased)
```

**Actions:**
- [ ] Create SkillPurchaseService.cs
- [ ] Implement validation logic
- [ ] Implement passive application
- [ ] Implement event publishing

### Phase 2.2: Create Derived Stats Recalculation Service

**New File:** `Assets/_Game/Scripts/Skills/SkillDerivedStatsService.cs`

**Responsibility:**
- Collect all passive modifiers from purchased nodes
- Recalculate derived stats (Attack, Defense, MaxHP, etc.)
- Integrate with existing stat systems (SPEC 10)
- Called on: purchase, respec, load

**Key Methods:**
```csharp
public void RecalculateDerivedStats(
    HashSet<string> purchasedNodeIds,
    PlayerProgressionManager progression)

private List<SkillPassiveModifierSO> CollectModifiers(HashSet<string> nodeIds)
private void ApplyModifiers(List<SkillPassiveModifierSO> modifiers)
```

**Actions:**
- [ ] Create SkillDerivedStatsService.cs
- [ ] Research existing stat systems (PlayerManager, etc.)
- [ ] Implement modifier collection
- [ ] Implement application (hooks, not direct modification)

### Phase 2.3: Create Skill Respec Service

**New File:** `Assets/_Game/Scripts/Skills/SkillRespecService.cs`

**Responsibility:**
- Validate respec cost (free first, gold after)
- Clear purchased nodes
- Clear passives
- Recalculate skill points
- Clear invalid active slots
- Publish events
- Update EconomyManager (deduct gold)

**Key Methods:**
```csharp
public bool TryRespec(
    int respecCount,
    int playerGold,
    int playerLevel,
    out string failureReason,
    out int goldCost)

public void ExecuteRespec(
    out HashSet<string> clearedNodeIds,
    out List<string> clearedActiveSlots)
```

**State to Clear Only:**
- PurchasedNodeIds
- ActiveSkillSlots (invalid ones)
- PassiveModifiers applied

**State to NOT Clear:**
- Level, XP, inventory, equipment, corpse, cave state, checkpoints

**Actions:**
- [ ] Create SkillRespecService.cs
- [ ] Implement cost validation (first free, 250 gold after)
- [ ] Implement full reset of skill state
- [ ] Integrate with EconomyManager

### Phase 2.4: Create Active Slot Manager

**New File:** `Assets/_Game/Scripts/Skills/ActiveSkillSlotManager.cs`

**Responsibility:**
- Manage R/T/Y/G slots
- Assign EquippableSkill to slot
- Validate skill is unlocked and equippable
- Clear slot
- Persist/restore slot state
- Never allow PassiveSkill or CapstonePassive in slot

**Key Methods:**
```csharp
public bool TryAssignSkillToSlot(
    int slotIndex,  // 0=R, 1=T, 2=Y, 3=G
    string skillActionId,
    HashSet<string> unlockedSkillActionIds,
    out string failureReason)

public void ClearSlot(int slotIndex)

public string GetSlotSkillActionId(int slotIndex)

public bool IsSlotValid()  // Check for unequipped changes
```

**Actions:**
- [ ] Create ActiveSkillSlotManager.cs
- [ ] Implement slot assignment with validation
- [ ] Implement unlocked skill checking

---

## Phase 3: Enhanced Skill Tree Manager (2-3 hours)

**Goal:** Upgrade SkillTreeManager to handle full SPEC 16 requirements

**File:** `Assets/_Game/Scripts/Skills/SkillTreeManager.cs` (REWRITE)

**Key Changes:**
1. Track purchased nodes per tree (multi-tree support)
2. Separate PassiveSkill from EquippableSkill tracking
3. Validate capstones (8 nodes + prerequisites)
4. Publish events on purchase/clear
5. Integrate DerivedStatsService
6. Support respec with full reset

**New Structure:**
```csharp
private Dictionary<string, HashSet<string>> _purchasedNodesByTree;  // TreeId -> NodeIds
private ActiveSkillSlotManager _activeSlots;
private SkillPurchaseService _purchaseService;
private SkillDerivedStatsService _derivedStatsService;
private SkillRespecService _respecService;

public bool PurchaseNode(SkillNodeDataSO node, int playerLevel, int availablePoints)
public bool CanPurchaseNode(SkillNodeDataSO node, int playerLevel, int availablePoints)
public bool UnlockNode(SkillNodeDataSO node) // Passive alias for purchase
public void ExecuteRespec()
public bool IsNodePurchased(string nodeId)
public List<string> GetPurchasedNodesInTree(string treeId)
public List<string> GetUnlockedSkillActionIds()
```

**Actions:**
- [ ] Rewrite SkillTreeManager with new structure
- [ ] Add service integrations
- [ ] Add event publishing
- [ ] Maintain backward compatibility with simple use cases

---

## Phase 4: Events (1 hour)

**New File:** `Assets/_Game/Scripts/Core/Events/SkillTreeEvents.cs`

**Create All 15+ Events:**
```csharp
public class SkillPointGrantedEvent { int PointsGranted, int TotalAvailable }
public class SkillNodePurchaseRequestedEvent { string NodeId, int Cost, bool Valid }
public class SkillNodePurchasedEvent { string NodeId, string TreeId, int SkillPointsSpent }
public class SkillPurchaseFailedEvent { string NodeId, string FailureReason }
public class SkillPassiveAppliedEvent { string PassiveId, string[] AffectedStats }
public class SkillPassiveRemovedEvent { string PassiveId }
public class ActiveSkillSlotAssignRequestedEvent { int SlotIndex, string SkillActionId }
public class ActiveSkillSlotAssignedEvent { int SlotIndex, string SkillActionId }
public class ActiveSkillSlotClearedEvent { int SlotIndex }
public class SkillTreeOpenedEvent { }
public class SkillTreeClosedEvent { }
public class SkillTreeRespecRequestedEvent { int RespecCount, int CostGold }
public class SkillTreeRespecCompletedEvent { int RespecCount }
public class SkillTreeRespecFailedEvent { string FailureReason }
public class SkillDerivedStatsChangedEvent { Dictionary<string, float> StatDeltas }
```

**Actions:**
- [ ] Create SkillTreeEvents.cs
- [ ] Define all event classes with properties
- [ ] Add brief documentation for each

---

## Phase 5: Skill Asset Creation (2-3 hours)

**Goal:** Create 5 trees with 55 nodes as data assets

**Trees to Create:**
1. Melee (10 common + 1 capstone)
2. Ranged (10 common + 1 capstone)
3. Magic (10 common + 1 capstone)
4. Survival (10 common + 1 capstone)
5. Crafting (10 common + 1 capstone)

**Per Node Required:**
- NodeId (unique)
- DisplayName
- Description
- Icon (can be placeholder)
- SkillPointCost (1 for most, 1 for capstone)
- NodeType (from spec table)
- SkillCategory (PassiveSkill or EquippableSkill)
- Prerequisites (none for tier 1)
- For capstone: RequiredPurchasedNodesInTree = 8

**Process:**
1. Read SPEC 16 Tables (lines 301-385)
2. Create SkillTreeDataSO for each tree
3. Create SkillNodeDataSO for each node (55 total)
4. Create SkillPassiveModifierSO for each passive effect
5. Create SkillActionSO for each equippable skill (or link to spell)
6. Create SkillTreeRegistrySO and assign all trees

**Files to Create:**
```
Assets/_Game/Data/Skills/Trees/SkillTree_Melee.asset
Assets/_Game/Data/Skills/Trees/SkillTree_Ranged.asset
Assets/_Game/Data/Skills/Trees/SkillTree_Magic.asset
Assets/_Game/Data/Skills/Trees/SkillTree_Survival.asset
Assets/_Game/Data/Skills/Trees/SkillTree_Crafting.asset

Assets/_Game/Data/Skills/Nodes/melee_*.asset (11 nodes)
Assets/_Game/Data/Skills/Nodes/ranged_*.asset (11 nodes)
Assets/_Game/Data/Skills/Nodes/magic_*.asset (11 nodes)
Assets/_Game/Data/Skills/Nodes/survival_*.asset (11 nodes)
Assets/_Game/Data/Skills/Nodes/crafting_*.asset (11 nodes)

Assets/_Game/Data/Skills/Modifiers/skillpassive_*.asset (for each passive)
Assets/_Game/Data/Skills/SkillTreeRegistry.asset
```

**Actions:**
- [ ] Create SkillTree_ assets (5)
- [ ] Create SkillNode_ assets (55)
- [ ] Create SkillPassiveModifier_ assets (25-30)
- [ ] Create SkillAction_ assets for equippable skills (not in spells)
- [ ] Create SkillTreeRegistry and populate

---

## Phase 6: Skill Action Shapes (1-2 hours)

**Goal:** Implement 6 skill action types needed by SPEC 16

**New File:** `Assets/_Game/Scripts/Combat/Skills/SkillActionExecutor.cs`

**Implement Shapes:**
1. **Block:** Temporary damage reduction window
2. **Dash:** Short movement with stamina cost
3. **Leap:** Jump attack with damage on impact
4. **Charge:** Hold to charge, release to fire with power scaling
5. **Line Piercer:** Projectile that hits all in straight line
6. **Multishot:** Fire 3 projectiles in spread pattern

**MVP Approach:** Each shape has basic implementation + hooks for future complexity

**Actions:**
- [ ] Create SkillActionExecutor.cs
- [ ] Implement 6 shape executors
- [ ] Hook into combat system (use GameEventBus, don't modify PlayerController)
- [ ] Add fallback behaviors for incomplete implementations

---

## Phase 7: UI & Input (2-3 hours)

### Phase 7.1: K-Key Binding

**File:** `Assets/_Game/Scripts/Player/PlayerController.cs` (MODIFY)

**Add:**
```csharp
private void Update()
{
    // Existing code...
    
    // K key for skill tree panel
    if (Input.GetKeyDown(KeyCode.K))
    {
        GameEventBus.Publish(new SkillTreeOpenedEvent());
    }
}
```

**Actions:**
- [ ] Add K-key detection to PlayerController
- [ ] Publish SkillTreeOpenedEvent

### Phase 7.2: SkillTreePanel Modal

**New File:** `Assets/_Game/Scripts/UI/Skills/SkillTreePanelController.cs`

**Responsibility:**
- Listen to SkillTreeOpenedEvent
- Open modal via ModalManager
- Manage navigation (W/A/S/D, Q/E for tabs, Enter for select)
- Prevent Q/E from affecting hands (inside modal only)
- Display: skill points, nodes, prerequisites, active slots
- Handle purchases and slot assignments
- Respect modal stack

**Key Methods:**
```csharp
private void OnSkillTreeOpenedEvent(SkillTreeOpenedEvent evt)
private void HandleTabNavigation(int direction)  // Q/E
private void HandleNodeSelection(Vector2 direction)  // W/A/S/D
private void HandleNodePurchase()  // Enter
private void HandleSlotAssignment(int slotIndex)  // R/T/Y/G
```

**Actions:**
- [ ] Create SkillTreePanelController.cs
- [ ] Implement navigation logic
- [ ] Implement purchase/slot assignment
- [ ] Add to GameBootstrap initialization

### Phase 7.3: SkillTreePanel UI Layout

**New Prefab:** `Assets/_Game/Prefabs/UI/Modals/SkillTreePanel.prefab`

**Components:**
- Canvas (modal)
- TreeTabButtons (5 tabs: Melee, Ranged, Magic, Survival, Crafting)
- NodeGridDisplay (shows nodes in tree)
- SkillPointsDisplay
- ActiveSlotDisplay (shows R/T/Y/G)
- DetailsPanel (shows selected node info)

**Actions:**
- [ ] Create prefab structure
- [ ] Layout tabs
- [ ] Layout node grid
- [ ] Layout active slot display
- [ ] Create displays for info panels

### Phase 7.4: Anya Fountain Integration

**File:** `Assets/_Game/Scripts/UI/Locations/AnyaFountainUIController.cs` (MODIFY from SPEC 15)

**Add Respec Option:**
- Menu already shows: Return to Cave, Respec (disabled), Exit
- On Respec selection:
  - Check if SkillRespecService can execute (cost, first-free check)
  - Call SkillRespecService.TryRespec()
  - Show confirmation dialog with cost
  - On confirm: Execute respec, show success message
  - Publish SkillTreeRespecCompletedEvent

**Actions:**
- [ ] Modify AnyaFountainMenu modal to enable Respec button
- [ ] Add respec logic to AnyaFountainUIController
- [ ] Integrate with SkillRespecService

---

## Phase 8: Save/Load Integration (1-2 hours)

### Phase 8.1: SaveData Integration

**File:** `Assets/_Game/Scripts/Save/SaveData.cs` (MODIFY)

**Add Field:**
```csharp
public SkillTreeSaveData SkillTreeData = new SkillTreeSaveData();
```

**Actions:**
- [ ] Add SkillTreeSaveData field to GameSaveData
- [ ] Update serialization if needed

### Phase 8.2: SaveManager Integration

**File:** `Assets/_Game/Scripts/Save/SaveManager.cs` (MODIFY)

**Add Methods:**
```csharp
private void CaptureSkillTreeSaveData()
{
    if (_currentSaveData != null)
    {
        _currentSaveData.SkillTreeData = SkillTreeManager.CaptureSaveData();
    }
}

private void RestoreSkillTreeSaveData(SkillTreeSaveData data)
{
    if (data != null && SkillTreeManager != null)
    {
        SkillTreeManager.RestoreFromSaveData(data);
    }
}
```

**Call Sites:**
- SaveGame(): call CaptureSkillTreeSaveData()
- LoadGame(): call RestoreSkillTreeSaveData()

**Actions:**
- [ ] Add capture/restore methods to SaveManager
- [ ] Wire into SaveGame() and LoadGame()
- [ ] Test save/load cycle

### Phase 8.3: GameBootstrap Integration

**File:** `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` (MODIFY)

**Add Field:**
```csharp
[SerializeField] private SkillTreeRegistrySO _skillTreeRegistry;
[SerializeField] private SkillTreeManager _skillTreeManager;
```

**Add Initialization:**
```csharp
private void InitializeSkillSystem()
{
    if (_skillTreeRegistry != null && _skillTreeManager != null)
    {
        _skillTreeManager.Initialize(_skillTreeRegistry);
    }
}
```

**Call from InitializeManagers():**
```csharp
InitializeSkillSystem();
```

**Actions:**
- [ ] Add skill system fields to GameBootstrap
- [ ] Implement InitializeSkillSystem()
- [ ] Call from InitializeManagers()

---

## Phase 9: Testing & Anti-Regression (2-3 hours)

### Phase 9.1: Play Mode Testing Checklist

**Level/SkillPoint Tests:**
- [ ] Start game at level 1: 0 skill points
- [ ] Reach level 2: gain 1 skill point
- [ ] Reach level 3: no skill point (odd level)
- [ ] Reach level 4: gain 1 skill point
- [ ] Check UnspentSkillPoints tracks correctly

**Purchase Tests:**
- [ ] Open skill tree with K
- [ ] Cannot purchase without points (fails gracefully)
- [ ] Purchase node costs 1 point
- [ ] Cannot purchase without prerequisites
- [ ] Purchase prerequisite first, then main node (succeeds)
- [ ] Cannot purchase node twice
- [ ] Purchased nodes stay purchased on close/reopen

**Passive Skill Tests:**
- [ ] Purchase PassiveSkill with no prerequisites
- [ ] Effect applies immediately (check derived stats)
- [ ] Passive not shown in active slots list (only equippables)
- [ ] Cannot equip PassiveSkill to R/T/Y/G

**Equippable Skill Tests:**
- [ ] Purchase EquippableSkill
- [ ] Skill unlocked (shown in slot list)
- [ ] Can assign to R/T/Y/G
- [ ] Key press executes skill
- [ ] Can reassign to different slot
- [ ] Unequip from slot clears it
- [ ] Skill points available updates correctly

**Tree & Capstone Tests:**
- [ ] 5 trees exist in modal tabs (Melee, Ranged, Magic, Survival, Crafting)
- [ ] Each tree has 10+ nodes
- [ ] Capstone cannot be purchased without 8 purchased nodes
- [ ] Purchase 8 nodes → capstone becomes available
- [ ] Purchase capstone → applies effect

**Respec Tests:**
- [ ] First respec is FREE
- [ ] Second respec costs 250 gold
- [ ] Cannot respec without enough gold
- [ ] Respec clears all purchased nodes
- [ ] Respec clears passives (stat goes back to base)
- [ ] Respec clears active slots
- [ ] Respec recalculates skill points (all unspent again)
- [ ] Respec does NOT affect level, XP, inventory, equipment

**Save/Load Tests:**
- [ ] Purchase node → save → load → node still purchased
- [ ] Equip skill to slot → save → load → slot still equipped
- [ ] Respec count increments → save → load → count preserved
- [ ] Passives stay applied on load

**Anti-Regression Tests:**
- [ ] Q/E work normally outside modal (hands work)
- [ ] Q/E in modal do NOT affect hands
- [ ] Movement blocked while modal open (respects modal stack)
- [ ] Cannot open skill tree during death/respawn
- [ ] Active slots R/T/Y/G still work (SPEC 12 compat)
- [ ] Equipment/durability unaffected (SPEC 10)
- [ ] Hunger/stamina still work (SPEC 09)
- [ ] Damage/status still work (SPEC 11)

### Phase 9.2: Compilation & Validation

**Run:**
```powershell
# Validation scripts
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

**Must Pass:**
- Zero compile errors
- Zero runtime errors in logs
- No warnings about undefined events
- No asset reference errors

**Actions:**
- [ ] Run all validation scripts
- [ ] Fix any compilation errors
- [ ] Fix any runtime errors in logs

---

## Phase 10: Documentation & Tracking (1 hour)

### Phase 10.1: Update Spec Status

**File:** `docs/specs/implementados/spec_skill_trees_active_slots_respec_anya_runtime.md`

**Actions:**
- [ ] Move spec from a_implementar to implementados
- [ ] Move prompt file (docs/prompts) if exists
- [ ] Move refinement files if exists

### Phase 10.2: Update PROJECT_LOG.md

**Add SPEC 16 Session Entry:**

```markdown
## Sessão 2026-05-25 (18ª) - SPEC 16: Skill Trees, Active Slots, Respec

**Data:** 2026-05-25
**Foco:** 5 skill trees, 55 nodes, SkillPoints at even levels, active slots, respec, K-modal
**Status:** IMPLEMENTADO (Core systems + 5 trees + save/load + 6 skill shapes)

### Deliverables
...
```

**Actions:**
- [ ] Add entry to PROJECT_LOG.md

### Phase 10.3: Update BACKLOG.md

**Change SPEC 16 status:**
```markdown
## SPEC 16 - Skill Trees, Active Slots, Respec, Anya Runtime

**Status: ✅ IMPLEMENTATION COMPLETE**
- Phase 1: Events + Data Structures ✅
- Phase 2: Core Services ✅
- Phase 3: Skill Tree Manager ✅
- Phase 4: Events ✅
- Phase 5: Skill Assets (55 nodes) ✅
- Phase 6: Skill Action Shapes (6 types) ✅
- Phase 7: UI + Input ✅
- Phase 8: Save/Load ✅
- Validation: ✅ Unity compile + anti-regression
- Next: SPEC 17 (Polish & Art)
```

**Actions:**
- [ ] Update BACKLOG.md status

### Phase 10.4: Update IMPLEMENTATION_STATUS.md

**Actions:**
- [ ] Add SPEC 16 completion to status tracking
- [ ] Update next spec blocker status
- [ ] Update overall progress

---

## Safe Build Order Summary

**Why This Order:**

1. **Phase 0 (Compilation)** - MUST do first or nothing compiles
2. **Phase 1 (Data)** - Define contracts before using them
3. **Phase 2 (Services)** - Logic before UI
4. **Phase 3 (Manager)** - Orchestrator last (uses all services)
5. **Phase 4 (Events)** - Used by multiple phases (needed for Phase 7)
6. **Phase 5 (Assets)** - Data assets once structures exist
7. **Phase 6 (Shapes)** - Skill execution logic
8. **Phase 7 (UI)** - User interaction layer
9. **Phase 8 (Save/Load)** - Persistence integration
10. **Phase 9 (Testing)** - Validate everything works
11. **Phase 10 (Docs)** - Update tracking

**Each Phase:**
- Has clear definition of done
- Doesn't block other phases
- Can be tested independently
- Contributes to single deliverable

---

## Risk Mitigation

| Risk | Mitigation | Owner |
|------|-----------|-------|
| Duplicate SkillActionSO breaks compile | Consolidate in Phase 0 | Phase 0.2 |
| Respec affects level/XP | Isolated service, only touches skill state | Phase 2.3 |
| K-key breaks modal stack | Use existing modal patterns | Phase 7 |
| Passive stats don't apply | DerivedStatsService integration testing | Phase 9 |
| Save file corrupted | DTOs only simple types, validation in load | Phase 8 |
| Anti-regression fails | Run full test checklist | Phase 9 |

---

## Estimated Timeline

```
Phase 0: 1-2 hours
Phase 1: 2-3 hours
Phase 2: 3-4 hours
Phase 3: 2-3 hours
Phase 4: 1 hour
Phase 5: 2-3 hours
Phase 6: 1-2 hours
Phase 7: 2-3 hours
Phase 8: 1-2 hours
Phase 9: 2-3 hours
Phase 10: 1 hour
----------
Total: 20-31 hours (estimated)

Realistic: 25-35 hours with:
- Asset creation iteration
- Bug fixes during testing
- Documentation updates
- Pause points for code review
```

---

## Definition of Done (DoD)

✅ All 10 phases complete
✅ Skill tree modal opens/closes with K
✅ 55 nodes across 5 trees created and purchasable
✅ Passive skills apply without equipping
✅ Equippable skills assignable to R/T/Y/G
✅ Capstone requires 8 nodes + prerequisites
✅ Respec works (first free, 250 gold after)
✅ Skill points grant at even levels (2, 4, 6, etc.)
✅ Save/load preserves: purchased nodes, active slots, respec count
✅ Anti-regression checklist passed (20/20 tests)
✅ Compilation validation passed
✅ Spec/Prompt/Refinement files moved to implementados
✅ PROJECT_LOG.md, BACKLOG.md, IMPLEMENTATION_STATUS.md updated

---

## Next Steps

1. ✅ Review this plan (CURRENT)
2. → Approve plan or request changes
3. → Begin Phase 0 (Compilation Fix)
4. → Proceed sequentially through phases
5. → Execute Phase 9 testing
6. → Execute Phase 10 tracking updates
7. → Commit and close SPEC 16

---

**Plan Created:** 2026-05-25  
**Status:** Ready for Execution  
**Confidence:** HIGH (based on comprehensive audit)

