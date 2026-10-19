---
doc_type: adr
status: accepted
adr_id: ADR-0006
title: Save Data Contracts Simple DTOs
date: 2026-06-01
source_documents:
  - .claude/rules/unity-architecture.md
  - .specs/implementados/SPEC_10_FARM_PERSISTENCE.md
  - docs/game_rules/save_rules.md
supersedes: []
superseded_by: []
applies_to:
  - save-load-system
  - data-persistence
  - serialization
---

# ADR-0006 — Save Data Contracts Simple DTOs

## Status

**accepted** (architectural decision for save/load system)

## Context

Save data must be portable across Unity versions, platform-agnostic, deterministic, and reversible. Unity object references (`ScriptableObject`, `GameObject`, `Sprite`, etc.) are fragile: they break on package updates, change after serialization, and cannot be reliably loaded in different contexts. Question: How should the save system persist game state without Unity refs?

## Decision

**Save DTOs contain only simple types. Assets, runtime objects, and complex references are resolved after load via existing registries.**

### Allowed In Save DTOs

```csharp
// Simple types only:
- string
- int, float, bool
- enums (persisted as string or int)
- List<T> / T[] where T is simple
- Nested DTOs containing only simple types
- stable IDs (strings or ints) for assets, entities, scenes
```

### Prohibited In Save DTOs

```csharp
// NO Unity object refs:
- ScriptableObject (reference or serialized field)
- GameObject
- Transform, Collider, Rigidbody, etc.
- Sprite, Texture, Material
- MonoBehaviour references
- UnityEngine.Object of any kind

// NO complex managed objects:
- system.object[]
- Dictionary (use list of key-value DTOs instead)
```

### Resolution Pattern

1. **Persist:** Save stable IDs (string or int), not object references
   ```csharp
   public class EquippedItemDTO
   {
       public string itemId;      // ✓ stable ID
       public int slotIndex;      // ✓ simple int
       // NOT: public WeaponSO weapon;  // ✗ prohibited
   }
   ```

2. **Load:** Resolve IDs via registries at load time
   ```csharp
   var item = ItemDatabase.GetById(dto.itemId);
   var spell = SpellDatabase.GetById(dto.spellId);
   ```

3. **Register:** All persistent assets have IDs in their databases
   - `ItemDatabaseSO` for items
   - `SpellDatabaseSO` for spells
   - `StatusEffectDatabaseSO` for status effects
   - Scene registry for scenes
   - Entity ID allocator for dynamic entities

## Versioning & Migration

Save schema changes must be backward compatible:

```csharp
public class PlayerSaveData
{
    public int saveVersion = 2;  // Current version
    
    // Old field (v1): kept for migration
    public string legacyWeaponName = "";
    
    // New field (v2): replaces old
    public string equippedWeaponId = "";
    
    // Migration logic in deserialization:
    if (saveVersion == 1)
        equippedWeaponId = LegacyWeaponNameToId(legacyWeaponName);
}
```

## Consequences

- Save files are portable and versionable
- No Unity upgrade surprises
- Clear serialization boundary (DTO layer)
- Registry lookups required at load time
- Migration path documented for schema changes
- Testable without Unity Editor (serialize/deserialize DTO independently)

## Applies To

- `GameSaveData` and all nested DTOs
- `PlayerSaveData`, `FarmSaveData`, `CaveRunSaveData`, etc.
- Any new persistent data structure

## Source Documents

- [unity-architecture.md](./../.claude/rules/unity-architecture.md) — enforcement rule
- [SPEC_10: Farm Persistence](./../specs/implementados/SPEC_10_FARM_PERSISTENCE.md) — implementation example
- [ADR-0001: Canonical Documentation](./ADR-0001-canonical-documentation-structure.md) — folder for data registries

---

*Created: 2026-06-01*  
*Status: accepted*  
*Related: ADR-0007 (event bus for state changes), ADR-0005 (cave snapshot persistence)*
