---
name: save-load-pattern
description: Implement save/load with IDs, simple types, and no Unity refs
version: 1.0
---

# Save/Load Data Pattern

Use when the task involves persistence, save data, load data, DTOs, snapshots, or registries.

## Core Rules

1. **Save uses IDs and simple types only.** Never Unity refs.
2. **Forbidden to serialize:**
   - `ScriptableObject`
   - `GameObject`
   - `Transform`
   - `MonoBehaviour`
   - `Sprite`
   - `Collider`
   - `Rigidbody`
3. **Editable save uses `Application.persistentDataPath`.** Never `StreamingAssets`.
4. **Load resolves IDs via registries.** Registry is source of truth.
5. **Schema versioning is required.** Track version and provide migrations.
6. **IDs are stable.** Never rename or reuse IDs without documented migration.

## Save Data Structure Pattern

### ✅ Correct Example

```csharp
namespace CindarsHope.Runtime.Save
{
    [System.Serializable]
    public class ItemSaveData
    {
        public int itemId;        // ID only
        public int quantity;      // Simple type
        public float durability;  // Simple type
    }
    
    [System.Serializable]
    public class EquipmentSaveData
    {
        public int equipmentId;   // ID only
        public float currentDurability; // Simple type
        public int weaponSlot;    // Slot index (int)
    }
    
    [System.Serializable]
    public class PlayerSaveData
    {
        public int version = 3;   // Schema version
        public float positionX, positionY; // Floats only
        public int[] equippedIds; // IDs only
        public List<ItemSaveData> inventory; // DTOs with IDs
    }
}
```

### ❌ Incorrect Example

```csharp
// DO NOT DO THIS:
[System.Serializable]
public class ItemSaveData
{
    public ItemDataSO itemData;     // ❌ ScriptableObject!
    public Sprite icon;              // ❌ Sprite!
    public Transform position;       // ❌ Transform!
    public MonoBehaviour handler;    // ❌ MonoBehaviour!
}
```

## Load Pattern with Registry Resolution

### Step 1: Define Registry Interface

```csharp
public interface IItemRegistry
{
    ItemDataSO GetItemById(int itemId);
    bool TryGetItem(int itemId, out ItemDataSO item);
}

public class ItemRegistry : MonoBehaviour, IItemRegistry
{
    [SerializeField] private ItemDataSO[] items;
    
    public ItemDataSO GetItemById(int itemId)
    {
        return items.FirstOrDefault(i => i.Id == itemId);
    }
    
    public bool TryGetItem(int itemId, out ItemDataSO item)
    {
        item = GetItemById(itemId);
        return item != null;
    }
}
```

### Step 2: Load From Save Data

```csharp
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private IItemRegistry itemRegistry;
    
    public void LoadFromSaveData(InventorySaveData saveData)
    {
        inventory.Clear();
        
        foreach (var itemData in saveData.items)
        {
            if (itemRegistry.TryGetItem(itemData.itemId, out var item))
            {
                inventory.Add(new InventorySlot 
                { 
                    item = item,
                    quantity = itemData.quantity 
                });
            }
            else
            {
                Debug.LogWarning($"Item {itemData.itemId} not found in registry");
            }
        }
    }
}
```

## Cave Snapshots (FASE9F Rule)

If task involves cave stable run/snapshots:

**Before implementing, read:**
- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`

**Core rule:**
- `CaveLevel` already visited in same `CaveRunSeed` → Load from snapshot
- `ForwardExit` and `BackExit` → Cannot regenerate layout/enemies/resource nodes
- Procedural only changes on new game, KO/death, or explicit debug command
- Save must persist snapshots with simple types, no Unity refs

**Snapshot pattern:**

```csharp
[System.Serializable]
public class CaveLevelSnapshot
{
    public int caveRunSeedId;    // ID only
    public int levelIndex;       // Position in cave
    public List<EnemySpawnData> enemies;      // DTOs with IDs
    public List<LootSpawnData> loot;          // DTOs with IDs
    public CaveLayoutData layoutData;         // Simple types
    // NO: Transform, GameObject, Collider refs
}

[System.Serializable]
public class EnemySpawnData
{
    public int enemyTypeId;      // ID only
    public float positionX, positionY;
    public float health;
    // NO: MonoBehaviour refs
}
```

## Schema Versioning

Always include version number and migration support:

```csharp
[System.Serializable]
public class SaveFileHeader
{
    public const int CURRENT_VERSION = 3;
    public int schemaVersion = CURRENT_VERSION;
    public string gameVersion = "1.0.0";
    public System.DateTime lastSaveTime;
}

public class SaveMigration
{
    public static PlayerSaveData MigrateV2ToV3(string v2Json)
    {
        var v2Data = JsonUtility.FromJson<PlayerSaveDataV2>(v2Json);
        
        // Migrate EquipmentDurability: Dictionary → List
        var newDurability = new List<DurabilityEntryData>();
        foreach (var kvp in v2Data.equipmentDurability)
        {
            newDurability.Add(new DurabilityEntryData 
            { 
                equipmentId = kvp.Key, 
                durability = kvp.Value 
            });
        }
        
        return new PlayerSaveData
        {
            version = 3,
            durabilityData = newDurability,
            // ... other fields
        };
    }
}
```

## File Location Pattern

```
Editable save (user-controlled):
  Application.persistentDataPath + "/saves/save1.json"

Read-only seed data:
  Application.streamingAssetsPath + "/data/seeds.json" ✓ OK

Initial game data (NOT for user save):
  Application.streamingAssetsPath + "/templates/*.json" ✓ OK

DO NOT use StreamingAssets for user save.
```

## ID Lifecycle

### Creating IDs

```csharp
public class ItemRegistry : MonoBehaviour
{
    [SerializeField] private ItemDataSO[] items;
    
    private void OnValidate()
    {
        // Ensure IDs are unique and stable
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Id != i + 1) // IDs start at 1
            {
                items[i].Id = i + 1;
                EditorUtility.SetDirty(items[i]);
            }
        }
    }
}
```

### Renaming/Removing IDs (With Migration)

```csharp
public class SaveMigration
{
    private static readonly Dictionary<int, int> IdRemap = new()
    {
        { 5, 10 }, // Old ID 5 → New ID 10
    };
    
    public static void MigrateIdChanges(PlayerSaveData data)
    {
        // Update inventory item IDs
        foreach (var item in data.inventory)
        {
            if (IdRemap.TryGetValue(item.itemId, out var newId))
            {
                item.itemId = newId;
            }
        }
    }
}
```

## Audit Checklist

Before finalizing save/load code:

- [ ] No `ScriptableObject` refs in DTOs
- [ ] No `GameObject` refs in save data
- [ ] No `Transform`, `Sprite`, `Collider`, `Rigidbody` in serialized classes
- [ ] IDs used for all references (int, not object)
- [ ] Registry exists and is injectable
- [ ] Load resolves IDs via registry (with fallback warning)
- [ ] Schema version tracked
- [ ] Migration support documented
- [ ] IDs are stable and documented
- [ ] Editable save uses `Application.persistentDataPath`
- [ ] Read-only data uses `StreamingAssets` (if applicable)

## Common Mistakes (Do NOT)

❌ Serialize ScriptableObject directly:
```csharp
public ItemDataSO item; // WRONG!
```

❌ Serialize Transform:
```csharp
public Transform position; // WRONG!
```

❌ Use StreamingAssets for user save:
```csharp
var path = Application.streamingAssetsPath + "/mysave.json"; // WRONG for user data!
```

❌ Forget migration on ID changes:
```csharp
// If you rename ID 5 → 10, you MUST migrate existing saves
```

❌ Hard-link ScriptableObjects in save class:
```csharp
[SerializeField] private ItemDataSO defaultItem; // In save? WRONG!
```

## Integration

- **Spec Execution** → Uses this pattern if persistence in scope
- **Non-Regression Review** → Audits for serialization violations
- **Implementation Closeout** → Validates save pattern in final audit

---

**Pattern compliance is mandatory for any save/load/persistence task.**
