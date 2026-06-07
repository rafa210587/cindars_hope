# SPEC 01.05 — Save Section Ownership Registry — Execution Report

> **Date:** 2026-06-07  
> **Spec ID:** `01_spec_save_section_ownership_registry`  
> **Phase Status:** BUILD_VALIDATED  
> **Executor:** Claude Code  

---

## Summary

Spec 01.05 completed: created SaveSectionOwnershipRegistry.cs documenting all 23 GameSaveData sections with owners, dependencies, defaults, and restore order from 01.04 audit.

**Deliverables:**
- ✓ SaveSectionOwnershipRegistry.cs with SaveSectionOwnership data class
- ✓ 23 sections documented (GameSaveData fields mapped to owners)
- ✓ Ownership matrix: who captures, who restores, dependencies, defaults
- ✓ Template for declaring new sections in future specs
- ✓ GetOwnership() query method for runtime lookups

**Status:** `BUILD_VALIDATED` — Ready for provider architecture (01.06).

---

## Ownership Registry Structure

**SaveSectionOwnership fields:**
- SectionName: Unique identifier
- DtoType: Serializable DTO type
- OwnerDomain: System responsible for this data
- CaptureOwner: Who captures during save
- RestoreOwner: Who restores during load
- DefaultBehavior: Explicit default if section missing (never null)
- CanRestoreIndependently: YES if restore order doesn't matter
- DependsOn: Required sections before this one can restore
- EnablesRestore: Sections that depend on this one
- PostRestoreEvent: Event published after restore (if any)
- RiskLevel: INFO/LOW/MEDIUM/HIGH
- MigrationOwner: Who handles schema migrations

---

## 23 Sections Documented

| # | Section | Owner | Default | Risk |
|---|---------|-------|---------|------|
| 1 | CurrentDay | TimeManager | Day 1 | LOW |
| 2 | Player | PlayerManager | Warn if missing | MEDIUM |
| 3 | Inventory | InventoryManager | Starter items injected | MEDIUM |
| 4 | Equipment | EquipmentManager | Empty | MEDIUM |
| 5 | Hotbar | HotbarSectionProvider | Default hotbar | LOW |
| 6 | Progression | ProgressionManager | Level 1, 0 XP | LOW |
| 7 | Farm | FarmPlotRegistry | Empty farm | LOW |
| 8 | World | ItemPickupRegistry | Empty world | MEDIUM |
| 9 | Cave | CaveRunManager | No active run | MEDIUM |
| 10 | Death | CorpseRecoveryManager | No death state | MEDIUM |
| 11 | Economy | ShopManager | All shops restocked | LOW |
| 12-23 | (Other sections) | (See SaveSectionOwnershipRegistry.cs) | (Per section) | (Per section) |

---

## ISaveSectionProvider Pattern

**Architecture found:** ISaveSectionProvider.cs and HotbarSectionProvider.cs already exist.

**Interface contract:**
- ProviderId: Unique identifier
- Capture(existingSaveData): Returns domain DTO or null to skip
- Restore(sectionData): Handles null section data gracefully

**Strategy for future specs:** Expand this pattern incrementally; no migration required.

---

## New Section Declaration Process

Future specs must declare new sections using the template:

```csharp
new SaveSectionOwnership
{
    SectionName = "...",
    OwnerDomain = "...",
    DependsOn = new[] { /* required sections */ },
    DefaultBehavior = "...", // Never null or undefined
    MigrationOwner = "...",
    // ... other fields
}
```

---

## Testing Status

**Test/validator:** NOT CREATED (registry is data-driven; tests in 01Q quality gate)

**Justification:** Registry is documentation + query method. Validation happens when new sections are added; deferred to 01Q.

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: NO
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: NO (defer to 01Q)
Manual Play Mode scenario: NOT REQUIRED
Justification: SaveSectionOwnershipRegistry is a data registry and utility class. No logic changes. Queries tested in 01Q integration.
Residual risk: Ownership matrix must be kept in sync with GameSaveData; recommend validator in 01Q
```

---

**Status:** `BUILD_VALIDATED`

*Report created: 2026-06-07*
