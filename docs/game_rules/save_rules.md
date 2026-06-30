---
doc_type: game_rule
status: accepted
domain: data-persistence
source_adrs:
  - ADR-0006
source_documents:
  - docs/decisions/ADR-0006-save-data-contracts-simple-dtos.md
  - .claude/rules/unity-architecture.md
last_reviewed: 2026-06-01
---

# Save Data Rules

## Purpose

Defines save data contracts, serialization requirements, and asset resolution patterns.

---

## Canonical Rules

### Rule: Save DTOs Contain Only Simple Types

- **Rule:** All save data structures (GameSaveData, PlayerSaveData, FarmSaveData, etc.) must contain only simple types:
  - `string`, `int`, `float`, `bool`
  - Enums (persisted as string or int)
  - Lists/arrays of simple values: `List<string>`, `List<int>`, `int[]`
  - Nested DTOs that themselves contain only simple types

- **Prohibited in save DTOs:**
  - ScriptableObject refs
  - GameObject refs
  - Transform, Collider, Rigidbody, Sprite, Texture, Material
  - MonoBehaviour refs
  - Any UnityEngine.Object reference

- **Why:** Save files must be portable across Unity versions and platforms; Unity object refs are fragile and break on serialization
- **Applies to:** GameSaveData hierarchy, all nested save DTOs

### Rule: Persistent IDs for Assets and Runtime Objects

- **Rule:** Instead of saving object refs, save stable IDs:
  - Item IDs: `"item_sword_iron"` (from ItemDatabaseSO)
  - Spell IDs: `"spell_fireball"` (from SpellDatabaseSO)
  - Status effect IDs: `"status_burn"` (from StatusEffectDatabaseSO)
  - Scene IDs: `"cave_level_1"` (from scene registry)
  - Entity IDs: Allocated by entity registry; never reused within save

- **Example:**
  ```csharp
  public class EquippedItemDTO
  {
      public string itemId;        // ✓ ID, not ref
      public int slotIndex;        // ✓ simple int
      // NOT: public ItemSO item;  // ✗ prohibited
  }
  ```

### Rule: Load-Time Resolution via Registries

- **Rule:** When loading save data, resolve IDs through registries:
  ```csharp
  // Load from save:
  var itemId = saveData.equippedItemId;
  
  // Resolve at load time:
  var itemSO = ItemDatabase.GetById(itemId);
  var equipment = new EquippedItem(itemSO, slotIndex);
  ```

- **Registries available:**
  - `ItemDatabaseSO` → ItemSO by ID
  - `SpellDatabaseSO` → SpellSO by ID
  - `StatusEffectDatabaseSO` → StatusEffectSO by ID
  - Scene registry → SceneID to path
  - Entity ID allocator → EntityID to runtime entity

- **Constraint:** ID must exist in registry or load fails gracefully (missing item → default/null, not crash)
- **Applies to:** All save/load deserialization

### Rule: Backward-Compatible Schema Versioning

- **Rule:** Save schema changes must be backward compatible:
  - Add version field to GameSaveData: `public int saveVersion = 2;`
  - Keep old fields in schema for one major version
  - Provide migration logic in deserialization

- **Example:**
  ```csharp
  public class PlayerSaveData
  {
      public int saveVersion = 2;
      
      // v1 field (deprecated but kept for migration)
      public string legacyWeaponName = "";
      
      // v2 field (new)
      public string equippedWeaponId = "";
      
      // Migration:
      [JsonIgnore]
      public void Migrate()
      {
          if (saveVersion == 1)
              equippedWeaponId = LegacyWeaponNameToId(legacyWeaponName);
      }
  }
  ```

- **Applies to:** Any schema change; deprecation must be explicit

### Rule: Snapshot Persistence for Cave Runs

- **Rule:** Cave runs use snapshots for stable run behavior:
  - `CaveVisitedLevelSnapshot` contains LayoutHash, ContentHash, enemy list, resource list
  - Snapshot created on first level visit
  - Snapshot persisted in game save (save file includes cavity data)
  - Snapshot loaded on revisit; no regeneration

- **DTO structure:**
  ```csharp
  public class CaveVisitedLevelSnapshot
  {
      public int levelNumber;
      public string layoutHash;
      public string contentHash;
      public List<EnemyDTO> enemies;
      public List<ResourceNodeDTO> resources;
  }
  ```

- **Constraint:** Snapshot must preserve exact state; any mismatch on load indicates corruption
- **Applies to:** Cave runtime persistence

---

## Open Questions

- What happens if an ItemID in save doesn't exist in current ItemDatabase? (Current: log warning, use default item or null)
- How far back are schema versions supported? (Current: 1 major version back; older saves require patch)
- Can save DTOs contain Lists of DTOs? (Current: yes, if nested DTOs are simple-type only)

---

## Related ADRs

- [ADR-0006: Save Data Contracts Simple DTOs](../decisions/ADR-0006-save-data-contracts-simple-dtos.md)
- [ADR-0005: Cave Stable Run and Replay](../decisions/ADR-0005-cave-stable-run-and-replay.md) (snapshot persistence)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (save system listens to events)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: docs/decisions/ADR-0006-save-data-contracts-simple-dtos.md*
